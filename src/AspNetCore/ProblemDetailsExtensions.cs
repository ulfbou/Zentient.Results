// File: Zentient.Results.AspNetCore/ProblemDetailsExtensions.cs

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options; // New using directive

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Zentient.Results;
using Zentient.Results.AspNetCore.Configuration;
using Zentient.Utilities; // Assuming ErrorInfo and IResult are from here

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// Provides extension methods for converting <see cref="Zentient.Results.IResult"/> instances
    /// into ASP.NET Core <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> responses,
    /// adhering to RFC 7807.
    /// </summary>
    public static class ProblemDetailsExtensions
    {
        /// <summary>
        /// Converts a failed <see cref="Zentient.Results.IResult"/> instance into an appropriate
        /// <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> response.
        /// </summary>
        /// <param name="result">The <see cref="Zentient.Results.IResult"/> instance to convert.
        /// This method should only be called for failed results (<see cref="IResult.IsFailure"/> is true).</param>
        /// <param name="factory">The <see cref="ProblemDetailsFactory"/> instance, typically provided by the ASP.NET Core
        /// framework (e.g., injected into a filter or middleware).</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>, necessary for rich ProblemDetails generation
        /// (e.g., instance URI, trace ID, and custom problem details options).</param>
        /// <param name="options">The configured <see cref="ZentientProblemDetailsOptions"/>.</param>
        /// <param name="instance">An optional URI that identifies the specific occurrence of the problem.
        /// Defaults to the current request's path if not provided.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance representing the error.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the <paramref name="result"/> is a success result.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> or <paramref name="httpContext"/> or <paramref name="options"/> is null.</exception>
        public static ProblemDetails ToProblemDetails(
            this Zentient.Results.IResult result,
            ProblemDetailsFactory factory,
            HttpContext httpContext,
            ZentientProblemDetailsOptions options,
            string? instance = null)
        {
            ArgumentNullException.ThrowIfNull(factory, nameof(factory));
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            if (result.IsSuccess)
            {
                throw new InvalidOperationException("Cannot convert a successful result to ProblemDetails. ProblemDetails are for failure results only.");
            }

            string problemTypeBaseUri = options.ProblemTypeBaseUri;
            if (string.IsNullOrWhiteSpace(problemTypeBaseUri))
            {
                problemTypeBaseUri = ZentientProblemDetailsOptions.FallbackProblemDetailsBaseUri;
            }
            else if (!problemTypeBaseUri.EndsWith("/"))
            {
                problemTypeBaseUri += "/";
            }

            var firstError = result.Errors.FirstOrDefault();
            var httpStatusCodeEnum = result.Status.ToHttpStatusCode();
            int statusCode = (int)httpStatusCodeEnum;
            string problemType;

            // Determine problem type based on error category or status code
            if (result.Errors.Any(e => e.Category == ErrorCategory.Validation))
            {
                problemType = $"{problemTypeBaseUri}validation";
            }
            else if (result.Errors.Any())
            {
                if (!string.IsNullOrWhiteSpace(firstError.Code))
                {
                    problemType = $"{problemTypeBaseUri}{firstError.Code.ToLowerInvariant()}";
                }
                else if (firstError.Category.IsDefined() == true && firstError.Category != ErrorCategory.None)
                {
                    problemType = $"{problemTypeBaseUri}{firstError.Category.ToString().ToLowerInvariant()}";
                }
                else
                {
                    // Fallback to HTTP status code if no specific code or category is available
                    problemType = $"{problemTypeBaseUri}{statusCode.ToString().ToLowerInvariant()}";
                }
            }
            else
            {
                // Fallback to HTTP status code if no errors are present but result is a failure
                problemType = $"{problemTypeBaseUri}{statusCode.ToString().ToLowerInvariant()}";
            }

            string problemTitle = result.Status.Description;
            if (string.IsNullOrWhiteSpace(problemTitle))
            {
                problemTitle = $"HTTP {statusCode} Error";
            }

            string problemDetail = result.Error!;
            if (string.IsNullOrWhiteSpace(problemDetail))
            {
                problemDetail = $"An error occurred with status code {statusCode}.";
            }

            ProblemDetails problemDetails;
            if (result.Errors.Any(e => e.Category == ErrorCategory.Validation) || statusCode == (int)HttpStatusCode.UnprocessableEntity)
            {
                var modelState = new ModelStateDictionary();
                foreach (var error in result.Errors.Where(e => e.Category == ErrorCategory.Validation))
                {
                    string key;
                    if (error.Data is string dataString && !string.IsNullOrWhiteSpace(dataString))
                    {
                        key = dataString;
                    }
                    else if (!string.IsNullOrWhiteSpace(error.Code))
                    {
                        key = error.Code;
                    }
                    else
                    {
                        key = "General"; // Default key if no specific field/code
                    }
                    modelState.AddModelError(key, error.Message);
                }

                problemDetails = factory.CreateValidationProblemDetails(
                    httpContext: httpContext,
                    modelStateDictionary: modelState,
                    statusCode: statusCode, // Use the determined status code
                    title: problemTitle,
                    type: problemType,
                    detail: problemDetail,
                    instance: instance ?? httpContext.Request.Path.Value // Use provided instance or request path
                );
            }
            else // For all other non-validation failure scenarios
            {
                problemDetails = factory.CreateProblemDetails(
                    httpContext: httpContext,
                    statusCode: statusCode, // Use the determined status code
                    title: problemTitle,
                    type: problemType,
                    detail: problemDetail,
                    instance: instance ?? httpContext.Request.Path.Value // Use provided instance or request path
                );
            }

            if (problemDetails == null)
            {
                // This should ideally not happen if ProblemDetailsFactory is correctly implemented,
                // but adding a defensive check is good practice.
                throw new InvalidOperationException("ProblemDetailsFactory returned null ProblemDetails.");
            }

            // Ensure properties are set correctly by the factory or explicitly if not
            // The factory methods usually set these, but explicit assignment here ensures consistency.
            problemDetails.Status = statusCode;
            problemDetails.Title = problemTitle;
            problemDetails.Detail = problemDetail;
            problemDetails.Type = problemType;
            problemDetails.Instance ??= httpContext.Request.Path.Value; // Redundant but harmless, ensures instance is always there

            AddErrorInfoExtensions(problemDetails, result.Errors);

            return problemDetails;
        }

        /// <summary>
        /// Converts a failed <see cref="Zentient.Results.IResult{T}"/> instance into an appropriate
        /// <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> response.
        /// This method simply delegates to the non-generic <see cref="ToProblemDetails(IResult, ProblemDetailsFactory, HttpContext, ZentientProblemDetailsOptions, string?)"/>.
        /// </summary>
        /// <typeparam name="T">The type of the success value (ignored for failure conversion).</typeparam>
        /// <param name="result">The <see cref="Zentient.Results.IResult{T}"/> instance to convert.</param>
        /// <param name="factory">The <see cref="ProblemDetailsFactory"/> instance.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        /// <param name="zentientProblemDetailsOptions">The configured <see cref="ZentientProblemDetailsOptions"/>.</param>
        /// <param name="instance">An optional URI that identifies the specific occurrence of the problem.
        /// Defaults to the current request's path if not provided.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance representing the error.</returns>
        public static ProblemDetails ToProblemDetails<T>(
            this Zentient.Results.IResult<T> result,
            ProblemDetailsFactory factory,
            HttpContext httpContext,
            ZentientProblemDetailsOptions zentientProblemDetailsOptions, // Pass options directly
            string? instance = null) // Add optional instance parameter
        {
            return (result as Zentient.Results.IResult).ToProblemDetails(factory, httpContext, zentientProblemDetailsOptions, instance);
        }

        /// <summary>
        /// Converts the <see cref="IResultStatus"/> to an HTTP status code.
        /// This is a helper method to extract the status code from the result status.
        /// </summary>
        /// <param name="status">The <see cref="IResultStatus"/> instance containing the status code.</param>
        /// <returns>The HTTP status code as an integer.</returns>
        public static HttpStatusCode ToHttpStatusCode(this IResultStatus status) => (HttpStatusCode)status.Code;

        /// <summary>
        /// Gets the most appropriate HTTP status code based on the result's error categories.
        /// Defaults to 500 Internal Server Error if no specific category matches.
        /// </summary>
        /// <param name="result">The IResult instance.</param>
        /// <returns>An HttpStatusCode value.</returns>
        public static HttpStatusCode ToHttpStatusCode(this IResult result)
        {
            if (result.IsSuccess) return HttpStatusCode.OK; // Should ideally not be called for success results

            var firstErrorCategory = result.Errors?.FirstOrDefault().Category;

            return firstErrorCategory switch
            {
                ErrorCategory.NotFound => HttpStatusCode.NotFound,
                ErrorCategory.Validation => HttpStatusCode.BadRequest, // Validation errors typically map to 400
                ErrorCategory.Conflict => HttpStatusCode.Conflict,
                ErrorCategory.Authentication => HttpStatusCode.Unauthorized, // For authentication failures
                ErrorCategory.Network => HttpStatusCode.ServiceUnavailable,
                ErrorCategory.Timeout => HttpStatusCode.RequestTimeout,
                ErrorCategory.Security => HttpStatusCode.Forbidden, // For authorization failures
                ErrorCategory.Request => HttpStatusCode.BadRequest, // General client-side error
                // Explicitly map these to ensure they're covered even if not the first error
                ErrorCategory.Unauthorized => HttpStatusCode.Unauthorized,
                ErrorCategory.Forbidden => HttpStatusCode.Forbidden,
                ErrorCategory.ServiceUnavailable => HttpStatusCode.ServiceUnavailable,
                ErrorCategory.InternalServerError => HttpStatusCode.InternalServerError,
                _ => HttpStatusCode.InternalServerError // Default for unhandled or generic errors
            };
        }

        /// <summary>
        /// Adds a custom extension property "zentientErrors" to the <see cref="ProblemDetails.Extensions"/> dictionary.
        /// This extension contains a structured, hierarchical list of detailed error information from the Zentient.Results,
        /// including recursive handling of inner errors.
        /// </summary>
        /// <param name="problemDetails">The <see cref="ProblemDetails"/> instance to extend.</param>
        /// <param name="errors">The list of <see cref="ErrorInfo"/> objects to be added as an extension.
        /// If this list is null or empty, no "zentientErrors" extension will be added.</param>
        private static void AddErrorInfoExtensions(ProblemDetails problemDetails, IReadOnlyList<ErrorInfo> errors)
        {
            if (errors == null || !errors.Any())
            {
                return;
            }

            problemDetails.Extensions["zentientErrors"] = errors.Select(e => ToErrorObject(e)).ToList();

            static Dictionary<string, object?> ToErrorObject(ErrorInfo error)
            {
                var errorObject = new Dictionary<string, object?>
                {
                    { "category", error.Category.ToString().ToLowerInvariant() },
                    { "code", error.Code },
                    { "message", error.Message }
                };

                if (error.Data != null)
                {
                    errorObject["data"] = error.Data;
                }

                if (error.InnerErrors != null && error.InnerErrors.Any())
                {
                    errorObject["innerErrors"] = error.InnerErrors.Select(ie => ToErrorObject(ie)).ToList();
                }
                return errorObject;
            }
        }
    }
}
