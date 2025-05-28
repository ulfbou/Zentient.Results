using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Zentient.Results.AspNetCore.Configuration;
using Zentient.Results.AspNetCore.Filters;
using System.Net;
using Microsoft.Extensions.Options;

namespace Zentient.Results.AspNetCore
{
    public static class ZentientResultsServiceCollectionExtensions
    {
        public static IServiceCollection AddZentientResultsAspNetCore(this IServiceCollection services,
            Action<ProblemDetailsOptions>? configureMvcProblemDetails = null,
            Action<ZentientProblemDetailsOptions>? configureZentientProblemDetails = null)
        {
            services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>()
                .AddHttpContextAccessor();

            services.AddOptions<ZentientProblemDetailsOptions>()
                .Configure(options =>
                    { });

            if (configureZentientProblemDetails != null)
            {
                services.Configure(configureZentientProblemDetails);
            }

            services.PostConfigure<ApiBehaviorOptions>(options =>
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
                    var zentientProblemDetailsOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>().Value;

                    var problemDetails = result.ToProblemDetails(problemDetailsFactory, context.HttpContext, zentientProblemDetailsOptions.ProblemTypeBaseUri); // Pass the base URI

                    return new ObjectResult(problemDetails)
                    {
                        StatusCode = problemDetails.Status ?? (int)HttpStatusCode.BadRequest,
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            services.PostConfigure<ProblemDetailsOptions>(options =>
            {
                configureMvcProblemDetails?.Invoke(options);

                var originalCustomize = options.CustomizeProblemDetails;
                options.CustomizeProblemDetails = context =>
                {
                    originalCustomize?.Invoke(context);
                    if (!context.ProblemDetails.Extensions.ContainsKey("traceId"))
                    {
                        context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                    }
                };
            });

            services.AddScoped<ProblemDetailsResultFilter>();
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.AddService<ProblemDetailsResultFilter>();
            });

            services.AddScoped<ZentientResultEndpointFilter>();

            return services;
        }
    }
}
