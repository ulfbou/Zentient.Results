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
        private readonly string _problemTypeBaseUri;

        /// <summary>Initializes a new instance of the <see cref="ZentientResultEndpointFilter"/> class.</summary>
        /// <param name="problemDetailsFactory">The factory used to create <see cref="ProblemDetails"/> instances.</param>
        /// <param name="problemDetailsOptions">Configuration options for problem details, including the base URI for problem types.</param>
        public ZentientResultEndpointFilter(ProblemDetailsFactory problemDetailsFactory, IOptions<ZentientProblemDetailsOptions> zentientProblemDetailsOptions)
        {
            _problemDetailsFactory = problemDetailsFactory;
            _problemTypeBaseUri = zentientProblemDetailsOptions.Value.ProblemTypeBaseUri
                ?? "https://default.com/errors/";
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

            if (result is not Zentient.Results.IResult zentientResult)
                return result;

            if (zentientResult.IsFailure)
            {
                var problemDetails = zentientResult.ToProblemDetails(_problemDetailsFactory, context.HttpContext, _problemTypeBaseUri);
                return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
            }

            object? value = null;
            bool isGenericResult = false;
            Type? genericResultValueType = null;

            var zentientResultType = zentientResult.GetType();
            var iResultGenericInterface = zentientResultType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Zentient.Results.IResult<>));

            if (iResultGenericInterface != null)
            {
                isGenericResult = true;
                genericResultValueType = iResultGenericInterface.GetGenericArguments()[0];
                var valueProp = zentientResultType.GetProperty("Value");
                if (valueProp != null && valueProp.PropertyType == genericResultValueType)
                {
                    value = valueProp.GetValue(zentientResult);
                }
                else if (valueProp != null && genericResultValueType != null && valueProp.PropertyType.IsAssignableFrom(genericResultValueType))
                {
                    value = valueProp.GetValue(zentientResult);
                }
            }

            return zentientResult.Status.Code switch
            {
                (int)HttpStatusCode.OK => isGenericResult && genericResultValueType != null
                    ? InvokeGenericResultsMethod("Ok", genericResultValueType, value)
                    : Microsoft.AspNetCore.Http.Results.NoContent(),
                (int)HttpStatusCode.Created => isGenericResult && genericResultValueType != null
                    // Pass both the URI string and the 'value' extracted earlier
                    ? InvokeGenericResultsMethod("Created", genericResultValueType, "https://default.com/created/", value)
                    : Microsoft.AspNetCore.Http.Results.StatusCode((int)HttpStatusCode.Created),
                (int)HttpStatusCode.NoContent => Microsoft.AspNetCore.Http.Results.NoContent(),
                _ => Microsoft.AspNetCore.Http.Results.StatusCode(zentientResult.Status.Code)
            };
        }

        private Microsoft.AspNetCore.Http.IResult InvokeGenericResultsMethod(string methodName, Type valueType, params object?[] args)
        {
            var resultsType = typeof(Microsoft.AspNetCore.Http.Results);
            MethodInfo? genericMethodDefinition = null;
            object?[] methodArgs; // Declare methodArgs here

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
                // For "Ok", the arguments array will contain just the value.
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
                // For "Created", the arguments array will contain uri and value.
                // Assuming args[0] is uri and args[1] is value.
                methodArgs = args; // Use the original args for Created as it's already a two-element array
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
            return (Microsoft.AspNetCore.Http.IResult)specificMethod.Invoke(null, methodArgs)!; // Use methodArgs here
        }
    }
}
