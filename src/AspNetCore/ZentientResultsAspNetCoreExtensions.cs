// File: Zentient.Results.AspNetCore/ZentientResultsAspNetCoreExtensions.cs

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

using Zentient.Results;
using Zentient.Results.AspNetCore.Configuration;
using Zentient.Results.AspNetCore.Filters; // Assuming ErrorInfo and Result are from here

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// Provides extension methods for configuring Zentient.Results integration with ASP.NET Core.
    /// This includes setting up problem details, result filters, and endpoint filters for handling results
    /// from Zentient.Results in a way that is compatible with ASP.NET Core's API conventions.
    /// </summary>
    public static class ZentientResultsAspNetCoreExtensions
    {
        /// <summary>
        /// Adds Zentient.Results ASP.NET Core integration services to the specified <see cref="IServiceCollection"/>.
        /// This is the definitive and canonical method for registering all Zentient.Results-related
        /// ASP.NET Core services, behaviors, and options. It ensures:
        /// <list type="bullet">
        ///     <item>Global configuration of <see cref="ZentientProblemDetailsOptions"/>.</item>
        ///     <item>Deterministic registration of <see cref="ProblemDetailsFactory"/>.</item>
        ///     <item>Consistent handling of model state validation errors (HTTP 400).</item>
        ///     <item>Safe post-configuration of global <see cref="ProblemDetailsOptions"/> (e.g., traceId injection).</item>
        ///     <item>Registration of required filters (<see cref="ProblemDetailsResultFilter"/>, <see cref="ZentientResultEndpointFilter"/>).</item>
        /// </list>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> with Zentient.Results services added, for chaining.</returns>
        public static IServiceCollection AddZentientResultsAspNetCore(this IServiceCollection services)
        {
            // Register IHttpContextAccessor (essential for ProblemDetailsFactory and endpoint filters)
            services.AddHttpContextAccessor();

            // 1. Configure ZentientProblemDetailsOptions
            // This sets up the default ProblemTypeBaseUri if not provided by config/user.
            services.AddOptions<ZentientProblemDetailsOptions>()
                    .Configure(options =>
                    {
                        if (string.IsNullOrEmpty(options.ProblemTypeBaseUri))
                        {
                            options.ProblemTypeBaseUri = ZentientProblemDetailsOptions.FallbackProblemDetailsBaseUri;
                        }
                    });

            // 2. Deterministic Service Registration: ProblemDetailsFactory
            // Only register DefaultProblemDetailsFactory if one isn't already present.
            // This respects AddControllers() or other frameworks that might register it.
            if (!services.Any(sd => sd.ServiceType == typeof(ProblemDetailsFactory)))
            {
#if NET9_0_OR_GREATER
                // Use DefaultProblemDetailsFactory explicitly if targeting .NET 9+ where it's public.
                services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
#else
                // For older .NET versions, ProblemDetailsFactory is typically implemented by MVC internally.
                // We add it as a singleton. The framework will usually provide the concrete type.
                // If this causes issues in older versions, it might need to be conditionally linked
                // to a specific internal implementation or assumed to be provided by AddControllers().
                services.AddSingleton<ProblemDetailsFactory>();
#endif
            }

            // 3. Set up ApiBehaviorOptions.InvalidModelStateResponseFactory
            // This ensures consistent, REST-correct validation responses (HTTP 400).
            services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    // Use a concrete ProblemDetailsFactory instance to ensure it's functional
                    var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    var zentientOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>().Value;

                    // Create ValidationProblemDetails explicitly.
                    var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                        context.HttpContext,
                        context.ModelState,
                        (int)HttpStatusCode.BadRequest, // Validation errors always return 400 Bad Request
                        "One or more validation errors occurred.",
                        $"{zentientOptions.ProblemTypeBaseUri}validation",
                        instance: context.HttpContext.Request.Path.Value // Use request path as instance by default
                    );

                    // Ensure Status is deterministically 400.
                    // This is a defensive assignment, ensuring consistency regardless of
                    // the exact ProblemDetailsFactory implementation details for CreateValidationProblemDetails.
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;

                    // Attach zentientErrors extension based on ModelState, if configured
                    if (zentientOptions.IncludeZentientErrorsForModelState)
                    {
                        var errors = context.ModelState.Keys
                            .Where(key => context.ModelState[key] != null)
                            .SelectMany(key => context.ModelState[key]!.Errors.Select(x =>
                                new ErrorInfo(ErrorCategory.Validation, key, x.ErrorMessage, Data: key))) // Data: key stores the field name
                            .ToList();

                        if (errors.Any())
                        {
                            problemDetails.Extensions.Add("zentientErrors", errors);
                        }
                    }

                    // Return ObjectResult with the ProblemDetails and correct status/content type.
                    return new ObjectResult(problemDetails)
                    {
                        StatusCode = problemDetails.Status, // Will be 400
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            // 4. Safe Post-Configuration of global ProblemDetailsOptions (for trace enrichment)
            services.PostConfigure<Microsoft.AspNetCore.Http.ProblemDetailsOptions>(options =>
            {
                // Store the original CustomizeProblemDetails delegate to chain it.
                // This ensures any customizations from AddControllers() or other PostConfigure calls are respected.
                var originalCustomize = options.CustomizeProblemDetails;

                options.CustomizeProblemDetails = context =>
                {
                    // Execute any previously registered customization logic first
                    originalCustomize?.Invoke(context);

                    // Apply Zentient-specific enrichment: traceId injection
                    if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
                    {
                        // Use Activity.Current.Id for distributed tracing, fallback to HttpContext.TraceIdentifier
                        context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                    }
                };
            });

            // 5. Register Filters
            // These filters will be resolved from DI and applied to MVC controllers and Minimal API endpoints.
            services.AddScoped<ProblemDetailsResultFilter>();
            services.AddScoped<ZentientResultEndpointFilter>();

            // Ensure ProblemDetailsResultFilter is added globally to MVC via MvcOptions.Filters
            services.Configure<MvcOptions>(options =>
            {
                // Add as a ServiceFilter so it's resolved from DI, allowing constructor injection.
                options.Filters.AddService<ProblemDetailsResultFilter>();
            });

            return services;
        }

        // Overload for AddZentientResultsAspNetCore to support configuration actions
        // This makes it more flexible for users who want to customize options directly here.
        public static IServiceCollection AddZentientResultsAspNetCore(
            this IServiceCollection services,
            Action<Microsoft.AspNetCore.Http.ProblemDetailsOptions>? configureMvcProblemDetails = null,
            Action<ZentientProblemDetailsOptions>? configureZentientProblemDetails = null)
        {
            // Call the canonical method first to register all core services and default options
            services.AddZentientResultsAspNetCore();

            // Apply specific configurations if provided
            if (configureMvcProblemDetails != null)
            {
                services.Configure(configureMvcProblemDetails); // Direct configuration for Http.ProblemDetailsOptions
            }
            if (configureZentientProblemDetails != null)
            {
                services.Configure(configureZentientProblemDetails); // Direct configuration for ZentientProblemDetailsOptions
            }

            return services;
        }
    }
}
