// File: Zentient.Results.AspNetCore/ZentientResultsExtensions.cs
// This file contains the higher-level, user-facing extension methods.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using System;
using System.Net;

using Zentient.Results.AspNetCore.Configuration;
using Zentient.Results.AspNetCore.Filters;

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// Extension methods for configuring Zentient.Results in ASP.NET Core applications.
    /// Provides methods to add Zentient.Results services for both MVC controllers and Minimal APIs,
    /// including automatic conversion of Zentient.Results to ProblemDetails responses.
    /// </summary>
    public static class ZentientResultsExtensions
    {
        /// <summary>
        /// Adds Zentient.Results.AspNetCore services to the specified <see cref="IServiceCollection"/>.
        /// This is the primary entry point to enable Zentient.Results integration for both MVC
        /// and Minimal APIs. Call this method in your application's `ConfigureServices` method.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configureProblemDetails">An optional action to configure global <see cref="ProblemDetailsOptions"/> (for MVC and ProblemDetailsFactory).</param>
        /// <param name="configureZentientProblemDetails">An optional action to configure <see cref="ZentientProblemDetailsOptions"/> (Zentient-specific problem details options).</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddZentientResults(
            this IServiceCollection services,
            Action<ProblemDetailsOptions>? configureProblemDetails = null,
            Action<ZentientProblemDetailsOptions>? configureZentientProblemDetails = null)
        {
            // Delegate all core service registration and global configuration to AddZentientResultsAspNetCore.
            // This ensures all necessary services are registered once.
            return services.AddZentientResultsAspNetCore(configureProblemDetails, configureZentientProblemDetails);
        }

        /// <summary>
        /// Configures MVC options to seamlessly handle Zentient.Results, including automatic
        /// conversion of validation Results to ProblemDetails.
        /// This method is typically used when you explicitly call `builder.Services.AddControllers().AddZentientResultsForMvc()`.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure.</param>
        /// <param name="configureProblemDetails">An optional action to configure <see cref="ProblemDetailsOptions"/> specific to MVC.</param>
        /// <param name="configureZentientProblemDetails">An optional action to configure <see cref="ZentientProblemDetailsOptions"/> specific to MVC.</param>
        /// <returns>The <see cref="IMvcBuilder"/> so that additional calls can be chained.</returns>
        public static IMvcBuilder AddZentientResultsForMvc(
            this IMvcBuilder builder,
            Action<ProblemDetailsOptions>? configureProblemDetails = null,
            Action<ZentientProblemDetailsOptions>? configureZentientProblemDetails = null)
        {
            // Ensure core Zentient.Results services are added.
            builder.Services.AddZentientResultsAspNetCore(configureProblemDetails, configureZentientProblemDetails);

            // MVC-specific override for InvalidModelStateResponseFactory to return 422 UnprocessableEntity
            // if this is the desired behavior for MVC-specific validation errors, overriding the 400 default.
            // If you want 400 universally, you can remove this block.
            builder.ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Keys
                        .Where(key => context.ModelState[key] != null)
                        .SelectMany(key => context.ModelState[key]!.Errors.Select(x =>
                            new ErrorInfo(ErrorCategory.Validation, key, x.ErrorMessage, Data: key)))
                        .ToList();

                    var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    var zentientOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>().Value;

                    var problemDetails = Result.Validation(errors).ToProblemDetails(
                        problemDetailsFactory,
                        context.HttpContext,
                        options: zentientOptions,
                        instance: context.HttpContext.Request.Path.Value // Use request path as instance
                    );

                    // Explicitly return 422 Unprocessable Entity for MVC validation by default here.
                    return new UnprocessableEntityObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            return builder;
        }

        /// <summary>
        /// Applies Zentient.Results handling to a group of Minimal API endpoints.
        /// This method adds the <see cref="ZentientResultEndpointFilter"/> to the specified group,
        /// ensuring that <see cref="Zentient.Results.Result"/> objects are automatically converted
        /// to <see cref="ProblemDetails"/> for failure cases, and successful results are passed through.
        /// It also configures OpenAPI documentation for common problem details responses.
        /// </summary>
        /// <param name="group">The <see cref="RouteGroupBuilder"/> to apply Zentient.Results handling to.</param>
        /// <returns>The <see cref="RouteGroupBuilder"/> for chaining.</returns>
        public static RouteGroupBuilder WithZentientResults(this RouteGroupBuilder group)
        {
            group.AddEndpointFilter<ZentientResultEndpointFilter>();

            // This call to WithOpenApi will now correctly resolve because the .csproj
            // ensures the compatible Microsoft.AspNetCore.OpenApi package is referenced
            // for both net8.0 and net9.0 builds, and the 'using' directive is present.
            group.WithOpenApi(operation =>
            {
                var problemDetailsSchema = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProblemDetails" } };
                var problemDetailsMediaType = new OpenApiMediaType { Schema = problemDetailsSchema };
                var problemDetailsContent = new Dictionary<string, OpenApiMediaType> { ["application/problem+json"] = problemDetailsMediaType };

                operation.Responses.TryAdd(((int)HttpStatusCode.BadRequest).ToString(),
                    new OpenApiResponse { Description = "Bad Request (e.g., validation, general client error)", Content = problemDetailsContent });
                operation.Responses.TryAdd(((int)HttpStatusCode.Unauthorized).ToString(),
                    new OpenApiResponse { Description = "Unauthorized (e.g., missing or invalid authentication)", Content = problemDetailsContent });
                operation.Responses.TryAdd(((int)HttpStatusCode.Forbidden).ToString(),
                    new OpenApiResponse { Description = "Forbidden (e.g., insufficient permissions)", Content = problemDetailsContent });
                operation.Responses.TryAdd(((int)HttpStatusCode.NotFound).ToString(),
                    new OpenApiResponse { Description = "Not Found (e.g., resource not found)", Content = problemDetailsContent });
                operation.Responses.TryAdd(((int)HttpStatusCode.Conflict).ToString(),
                    new OpenApiResponse { Description = "Conflict (e.g., resource already exists, state conflict)", Content = problemDetailsContent });
                operation.Responses.TryAdd(((int)HttpStatusCode.UnprocessableEntity).ToString(),
                    new OpenApiResponse { Description = "Unprocessable Entity (e.g., semantic validation errors, if applicable)", Content = problemDetailsContent });
                operation.Responses.TryAdd(((int)HttpStatusCode.InternalServerError).ToString(),
                    new OpenApiResponse { Description = "Internal Server Error (e.g., unhandled exceptions)", Content = problemDetailsContent });

                return operation;
            });

            return group;
        }

        /// <summary>
        /// Applies Zentient.Results handling to a single Minimal API endpoint.
        /// This method adds the <see cref="ZentientResultEndpointFilter"/> to the specified endpoint,
        /// ensuring that <see cref="Zentient.Results.Result"/> objects are automatically converted
        /// to <see cref="ProblemDetails"/> for failure cases.
        /// It also registers common ProblemDetails responses for OpenAPI documentation.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointConventionBuilder"/> for the endpoint.</param>
        /// <returns>The <see cref="IEndpointConventionBuilder"/> for chaining.</returns>
        public static IEndpointConventionBuilder WithZentientResults(this IEndpointConventionBuilder builder)
        {
            if (builder is RouteHandlerBuilder routeHandlerBuilder)
            {
                routeHandlerBuilder.AddEndpointFilter<ZentientResultEndpointFilter>();
                routeHandlerBuilder
                    .ProducesProblem((int)HttpStatusCode.BadRequest)
                    .ProducesProblem((int)HttpStatusCode.NotFound)
                    .ProducesProblem((int)HttpStatusCode.Conflict)
                    .ProducesProblem((int)HttpStatusCode.InternalServerError);
            }

            return builder;
        }
    }
}
