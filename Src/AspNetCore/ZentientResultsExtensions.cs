using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using Zentient.Results;
using Zentient.Results.AspNetCore.Configuration;
using Zentient.Results.AspNetCore.Filters;

namespace Zentient.Results.AspNetCore
{
    public static class ZentientResultsExtensions
    {
        /// <summary>
        /// Adds Zentient.Results.AspNetCore services for both MVC controllers and Minimal APIs
        /// with optional configuration for problem details.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configureProblemDetails">An optional action to configure <see cref="Microsoft.AspNetCore.Http.ProblemDetailsOptions"/>.</param>
        /// <param name="configureZentientProblemDetails">An optional action to configure <see cref="ZentientProblemDetailsOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddZentientResults(
            this IServiceCollection services,
            Action<ProblemDetailsOptions>? configureProblemDetails = null,
            Action<ZentientProblemDetailsOptions>? configureZentientProblemDetails = null)
        {
            services
                .AddHttpContextAccessor()
                .AddOptions<ZentientProblemDetailsOptions>()
                .Configure(options =>
                {
                    options.ProblemTypeBaseUri = ProblemDetailsExtensions.FallbackProblemDetailsBaseUri;
                });

            if (configureZentientProblemDetails != null)
            {
                services.Configure(configureZentientProblemDetails);
            }

            services.PostConfigure<ProblemDetailsOptions>(options =>
            {
                var existingCustomize = options.CustomizeProblemDetails;
                options.CustomizeProblemDetails = context =>
                {
                    existingCustomize?.Invoke(context);
                    if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
                    {
                        context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                    }
                };

                configureProblemDetails?.Invoke(options);
            });

            services.AddScoped<ProblemDetailsResultFilter>();
            services.AddScoped<ZentientResultEndpointFilter>();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.AddService<ProblemDetailsResultFilter>();
            });

            return services;
        }

        /// <summary>
        /// Configures MVC options to seamlessly handle Zentient.Results, including automatic
        /// conversion of validation Results to ProblemDetails.
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
            builder.Services
                .AddHttpContextAccessor()
                .AddOptions<ZentientProblemDetailsOptions>()
                .Configure(options =>
                {
                    options.ProblemTypeBaseUri = ProblemDetailsExtensions.FallbackProblemDetailsBaseUri;
                });

            if (configureZentientProblemDetails != null)
            {
                builder.Services.Configure(configureZentientProblemDetails);
            }

            builder.ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Keys
                        .Where(key => context.ModelState[key] != null)
                        .SelectMany(key => context.ModelState[key]!.Errors.Select(x =>
                            new ErrorInfo(ErrorCategory.Validation, key, x.ErrorMessage, Data: key)))
                        .ToList();
                    var result = Result.Validation(errors);
                    var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    var zentientOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>();
                    var problemDetails = result.ToProblemDetails(problemDetailsFactory, context.HttpContext, zentientOptions.Value.ProblemTypeBaseUri);

                    return new UnprocessableEntityObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            builder.Services.PostConfigure<ProblemDetailsOptions>(options =>
            {
                var existingCustomize = options.CustomizeProblemDetails;

                options.CustomizeProblemDetails = context =>
                {
                    existingCustomize?.Invoke(context);
                    if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
                    {
                        context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                    }
                };

                configureProblemDetails?.Invoke(options);
            });

            builder.Services.AddScoped<ProblemDetailsResultFilter>();
            builder.Services.Configure<MvcOptions>(options =>
            {
                options.Filters.AddService<ProblemDetailsResultFilter>();
            });

            return builder;
        }

        /// <summary>
        /// Configures endpoint routing for Minimal APIs to seamlessly handle Zentient.Results,
        /// including automatic conversion of Results to ProblemDetails.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to configure.</param>
        /// <param name="configureProblemDetails">An optional action to configure <see cref="ProblemDetailsOptions"/> specific to Minimal APIs.</param>
        /// <param name="configureZentientProblemDetails">An optional action to configure <see cref="ZentientProblemDetailsOptions"/> specific to Minimal APIs.</param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/> so that additional calls can be chained.</returns>
        public static IEndpointRouteBuilder UseZentientResultsForMinimalApi(
            this IEndpointRouteBuilder endpoints,
            Action<ProblemDetailsOptions>? configureProblemDetails = null,
            Action<ZentientProblemDetailsOptions>? configureZentientProblemDetails = null)
        {
            var services = endpoints.ServiceProvider.GetRequiredService<IServiceCollection>();

            services
                .AddHttpContextAccessor()
                .AddOptions<ZentientProblemDetailsOptions>()
                .Configure(options =>
                {
                    options.ProblemTypeBaseUri = ProblemDetailsExtensions.FallbackProblemDetailsBaseUri;
                });

            if (configureZentientProblemDetails != null)
            {
                services.Configure(configureZentientProblemDetails);
            }

            services.PostConfigure<ProblemDetailsOptions>(options =>
            {
                var existingCustomize = options.CustomizeProblemDetails;
                options.CustomizeProblemDetails = context =>
                {
                    existingCustomize?.Invoke(context);
                    if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
                    {
                        context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                    }
                };

                configureProblemDetails?.Invoke(options);
            });

            services.AddScoped<ZentientResultEndpointFilter>();

            endpoints.Map("", () => Microsoft.AspNetCore.Http.Results.Ok())
                .AddEndpointFilter(async (context, next) =>
                {
                    var filter = context.HttpContext.RequestServices.GetService<ZentientResultEndpointFilter>();

                    if (filter != null)
                    {
                        var result = await next(context);

                        return await filter.InvokeAsync(
                            context,
                            c => ValueTask.FromResult<object?>(result)
                        );
                    }

                    return await next(context);
                });

            return endpoints;
        }
    }
}
