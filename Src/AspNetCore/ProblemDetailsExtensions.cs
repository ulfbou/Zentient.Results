using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure; // For ProblemDetailsFactory, DefaultProblemDetailsFactory
using Microsoft.AspNetCore.Mvc.ModelBinding; // For ModelStateDictionary
using Microsoft.Extensions.DependencyInjection; // For GetService
using Microsoft.Extensions.Options; // For IOptions
using Zentient.Results;
using System.Net;
using Zentient.Utilities; // For HttpStatusCode

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// Provides extension methods for converting <see cref="Zentient.Results.IResult"/> instances
    /// into ASP.NET Core <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> responses,
    /// adhering to RFC 7807.
    /// </summary>
    public static class ProblemDetailsExtensions
    {
        // Base URI for problem types. Customize this to reflect your API's documentation.
        public const string DefaultProblemTypeBaseUri = "https://yourdomain.com/errors/";

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
        /// <returns>A <see cref="ProblemDetails"/> instance representing the error.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the <paramref name="result"/> is a success result.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> or <paramref name="httpContext"/> is null.</exception>
        public static ProblemDetails ToProblemDetails(
                    this Zentient.Results.IResult result,
                    ProblemDetailsFactory factory,
                    HttpContext httpContext,
                    string problemTypeBaseUri)
        {
            ArgumentNullException.ThrowIfNull(factory, nameof(factory));
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (result.IsSuccess)
            {
                throw new InvalidOperationException("Cannot convert a successful result to ProblemDetails. ProblemDetails are for failure results only.");
            }

            if (string.IsNullOrWhiteSpace(problemTypeBaseUri))
            {
                problemTypeBaseUri = DefaultProblemTypeBaseUri;
            }
            else if (!problemTypeBaseUri.EndsWith("/"))
            {
                problemTypeBaseUri += "/";
            }

            var firstError = result.Errors.FirstOrDefault();
            var httpStatusCodeEnum = result.Status.ToHttpStatusCode();
            int statusCode = (int)httpStatusCodeEnum;
            string problemType;

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
                else if (firstError.Category.IsDefined() && firstError.Category != ErrorCategory.None)
                {
                    problemType = $"{problemTypeBaseUri}{firstError.Category.ToString().ToLowerInvariant()}";
                }
                else
                {
                    problemType = $"{problemTypeBaseUri}{statusCode.ToString().ToLowerInvariant()}";
                }
            }
            else
            {
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
                // Only add validation errors to modelState.
                // If statusCode is 422 but Errors list is empty, modelState will remain empty, which is correct.
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
                        key = "General";
                    }
                    modelState.AddModelError(key, error.Message);
                }

                problemDetails = factory.CreateValidationProblemDetails(
                    httpContext: httpContext,
                    modelStateDictionary: modelState,
                    statusCode: (int)statusCode,
                    title: problemTitle,
                    type: problemType,
                    detail: problemDetail
                );
            }
            else // For all other non-validation failure scenarios
            {
                problemDetails = factory.CreateProblemDetails(
                    httpContext: httpContext,
                    statusCode: (int)statusCode,
                    title: problemTitle,
                    type: problemType,
                    detail: problemDetail
                );
            }

            if (problemDetails == null)
            {
                throw new InvalidOperationException("ProblemDetailsFactory returned null ProblemDetails.");
            }

            problemDetails.Status = (int)statusCode;
            problemDetails.Title = problemTitle;
            problemDetails.Detail = problemDetail;
            problemDetails.Type = problemType;
            problemDetails.Instance ??= httpContext.Request.Path.Value;


            AddErrorInfoExtensions(problemDetails, result.Errors);

            return problemDetails;
        }

        /// <summary>
        /// Converts a failed <see cref="Zentient.Results.IResult{T}"/> instance into an appropriate
        /// <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> response.
        /// This method simply delegates to the non-generic <see cref="ToProblemDetails(IResult, ProblemDetailsFactory, HttpContext)"/>.
        /// </summary>
        /// <typeparam name="T">The type of the success value (ignored for failure conversion).</typeparam>
        /// <param name="result">The <see cref="Zentient.Results.IResult{T}"/> instance to convert.</param>
        /// <param name="factory">The <see cref="ProblemDetailsFactory"/> instance.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance representing the error.</returns>
        public static ProblemDetails ToProblemDetails<T>(
            this Zentient.Results.IResult<T> result,
            ProblemDetailsFactory factory,
            HttpContext httpContext,
            string problemTypeBaseUri)
        {
            return (result as Zentient.Results.IResult).ToProblemDetails(factory, httpContext, problemTypeBaseUri);
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
            if (result.IsSuccess) return HttpStatusCode.OK;

            var firstErrorCategory = result.Errors?.FirstOrDefault().Category;

            return firstErrorCategory switch
            {
                ErrorCategory.NotFound => HttpStatusCode.NotFound,
                ErrorCategory.Validation => HttpStatusCode.BadRequest,
                ErrorCategory.Conflict => HttpStatusCode.Conflict,
                ErrorCategory.Authentication => HttpStatusCode.Unauthorized,
                ErrorCategory.Network => HttpStatusCode.ServiceUnavailable,
                ErrorCategory.Timeout => HttpStatusCode.RequestTimeout,
                ErrorCategory.Security => HttpStatusCode.Forbidden,
                ErrorCategory.Request => HttpStatusCode.BadRequest,
                ErrorCategory.Unauthorized => HttpStatusCode.Unauthorized,
                ErrorCategory.Forbidden => HttpStatusCode.Forbidden,
                ErrorCategory.ServiceUnavailable => HttpStatusCode.ServiceUnavailable,
                ErrorCategory.InternalServerError => HttpStatusCode.InternalServerError,
                //ErrorCategory.Unauthorized => HttpStatusCode.Unauthorized,
                //ErrorCategory.Forbidden => HttpStatusCode.Forbidden,
                //ErrorCategory.Concurrency => HttpStatusCode.Conflict,
                //ErrorCategory.TooManyRequests => (HttpStatusCode)429,
                //ErrorCategory.ExternalService => HttpStatusCode.ServiceUnavailable,
                _ => HttpStatusCode.InternalServerError
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
