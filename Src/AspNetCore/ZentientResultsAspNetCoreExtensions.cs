using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Net;

using Zentient.Results.AspNetCore;

using Zentient.Results;
using Zentient.Results.AspNetCore.Filters;
using Zentient.Results.AspNetCore.Configuration;

namespace Zentient.Results.AspNetCore
{
    public static class ZentientResultsAspNetCoreExtensions
    {
        /// <summary>
        /// Adds Zentient.Results ASP.NET Core integration services to the specified <see cref="IServiceCollection"/>.
        /// This includes:
        /// <list type="bullet">
        ///     <item>Configuring <see cref="ProblemDetailsFactory"/>.</item>
        ///     <item>Overriding default API behavior for model state validation to use <see cref="ProblemDetails"/>.</item>
        ///     <item>Adding a global <see cref="ProblemDetailsResultFilter"/> for MVC controllers.</item>
        ///     <item>Registering <see cref="ZentientResultEndpointFilter"/> for Minimal APIs (must be applied to endpoints).</item>
        ///     <item>Configuring global <see cref="Microsoft.AspNetCore.Mvc.ProblemDetailsOptions"/> (e.g., adding traceId).</item>
        /// </list>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configureProblemDetails">An optional action to configure <see cref="Microsoft.AspNetCore.Mvc.ProblemDetailsOptions"/>.</param>
        /// <param name="configureZentientProblemDetails">An optional action to configure <see cref="ZentientProblemDetailsOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> with Zentient.Results services added.</returns>
        public static IServiceCollection AddZentientResultsAspNetCore(
                    this IServiceCollection services,
                    Action<Microsoft.AspNetCore.Http.ProblemDetailsOptions>? configureMvcProblemDetails = null,
                    Action<ZentientProblemDetailsOptions>? configureZentientProblemDetails = null)
        {
            services.AddOptions<ZentientProblemDetailsOptions>()
                    .Configure(options =>
                    { });

            if (configureZentientProblemDetails != null)
            {
                services.Configure(configureZentientProblemDetails);
            }

            services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
            services.AddHttpContextAccessor();

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
                    var zentientOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>();

                    if (configureZentientProblemDetails != null)
                    {
                        configureZentientProblemDetails(zentientOptions.Value);
                    }

                    var problemTypeBaseUri = zentientOptions.Value?.ProblemTypeBaseUri
                        ?? "https://default.com/errors/fallback/";
                    var problemDetails = result.ToProblemDetails(problemDetailsFactory, context.HttpContext, problemTypeBaseUri);

                    return new ObjectResult(problemDetails)
                    {
                        StatusCode = problemDetails.Status ?? (int)HttpStatusCode.BadRequest,
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            services.PostConfigure<Microsoft.AspNetCore.Http.ProblemDetailsOptions>(options =>
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
