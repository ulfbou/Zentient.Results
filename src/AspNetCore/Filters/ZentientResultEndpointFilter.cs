using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

using System.Net;

using static Microsoft.AspNetCore.Http.Results;
using System.Reflection;
using Microsoft.Extensions.Options;
using Zentient.Results.AspNetCore.Configuration;

namespace Zentient.Results.AspNetCore.Filters
{
    /// <summary>
    /// An endpoint filter that converts Zentient.Results.IResult and Zentient.Results.IResult{T}
    /// from Minimal API endpoints into appropriate HTTP results, including ProblemDetails for failures.
    /// </summary>
    public class ZentientResultEndpointFilter : IEndpointFilter
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ZentientProblemDetailsOptions _options;
        private readonly string _problemTypeBaseUri;

        /// <summary>Initializes a new instance of the <see cref="ZentientResultEndpointFilter"/> class.</summary>
        /// <param name="problemDetailsFactory">The factory used to create <see cref="ProblemDetails"/> instances.</param>
        /// <param name="httpContextAccessor">The accessor for the current HTTP context.</param>
        /// <param name="options">Configuration options for Zentient problem details.</param>
        public ZentientResultEndpointFilter(
                ProblemDetailsFactory problemDetailsFactory,
                IHttpContextAccessor httpContextAccessor,
                IOptions<ZentientProblemDetailsOptions> options)
        {
            _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _problemTypeBaseUri = _options.ProblemTypeBaseUri
                ?? ZentientProblemDetailsOptions.FallbackProblemDetailsBaseUri;
        }

        /// <summary>
        /// Invokes the endpoint filter logic to convert Zentient.Results.IResult and Zentient.Results.IResult{T}
        /// into corresponding ASP.NET Core HTTP results. If the result represents a failure, a ProblemDetails
        /// response is returned. Otherwise, the result is mapped to the appropriate HTTP status code and response type.
        /// </summary>
        /// <param name="context">The <see cref="EndpointFilterInvocationContext"/> for the current request.</param>
        /// <param name="next">The next <see cref="EndpointFilterDelegate"/> in the filter pipeline.</param>
        /// <returns>
        /// A <see cref="ValueTask{Object}"/> representing the asynchronous operation, containing the HTTP result
        /// or the original result if it is not a Zentient result type.
        /// </returns>
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var result = await next(context);

            if (result is Zentient.Results.IResult zentientResult)
            {
                if (zentientResult.IsFailure)
                {
                    // Pass the resolved options to the extension method
                    ProblemDetails problemDetails = zentientResult.ToProblemDetails(
                        _problemDetailsFactory,
                        _httpContextAccessor.HttpContext!, // Ensure HttpContext is available
                        _options, // Pass the options object
                        instance: context.HttpContext.Request.Path.Value // Pass instance explicitly
                    );

                    return Result.Problem(problemDetails);
                }
            }

            return result; // Pass through successful results or non-IResult types
        }

        private Microsoft.AspNetCore.Http.IResult InvokeGenericResultsMethod(string methodName, Type valueType, params object?[] args)
        {
            var resultsType = typeof(Microsoft.AspNetCore.Http.Results);
            MethodInfo? genericMethodDefinition = null;
            object?[] methodArgs;

            if (methodName == "Ok")
            {
                genericMethodDefinition = resultsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name == "Ok" &&
                        m.IsGenericMethodDefinition &&
                        m.GetGenericArguments().Length == 1 &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType.IsGenericParameter
                    );

                if (genericMethodDefinition == null)
                {
                    throw new InvalidOperationException($"Generic method 'Ok<T>(T value)' not found on Microsoft.AspNetCore.Http.Results.");
                }

                methodArgs = new object?[] { args?.FirstOrDefault() };
            }
            else if (methodName == "Created")
            {
                genericMethodDefinition = resultsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name == "Created" &&
                        m.IsGenericMethodDefinition &&
                        m.GetGenericArguments().Length == 1 &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[0].ParameterType == typeof(string) &&
                        m.GetParameters()[1].ParameterType.IsGenericParameter
                    );

                if (genericMethodDefinition == null)
                {
                    throw new InvalidOperationException($"Generic method 'Created<T>(string uri, T value)' not found on Microsoft.AspNetCore.Http.Results.");
                }

                methodArgs = args;
            }
            else
            {
                throw new NotSupportedException($"Method '{methodName}' is not supported for generic invocation.");
            }

            if (genericMethodDefinition == null)
            {
                throw new InvalidOperationException($"Could not find generic method definition for '{methodName}'.");
            }

            MethodInfo specificMethod = genericMethodDefinition.MakeGenericMethod(valueType);
            return (Microsoft.AspNetCore.Http.IResult)specificMethod.Invoke(null, methodArgs)!;
        }
    }
}
