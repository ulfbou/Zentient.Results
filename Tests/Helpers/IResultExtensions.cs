using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// Provides extension methods for converting <see cref="IResult"/> instances to <see cref="ProblemDetails"/>.
    /// This class is used in unit tests to simulate the conversion of Zentient results into ASP.NET Core problem details.
    /// </summary>
    internal static class IResultExtensions
    {
        /// <summary>Converts an <see cref="IResult"/> to a <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> instance.</summary>
        /// <param name="result">The result to convert.</param>
        /// <param name="factory">The <see cref="ProblemDetailsFactory"/> used for creating problem details (not used in this implementation).</param>
        /// <param name="context">The current <see cref="HttpContext"/> (not used in this implementation).</param>
        /// <returns>
        /// A <see cref="ValidationProblemDetails"/> if the result contains validation errors; otherwise, a <see cref="ProblemDetails"/> representing the error.
        /// </returns>
        public static ProblemDetails ToProblemDetails(this IResult result, ProblemDetailsFactory factory, HttpContext context)
        {
            if (result.IsFailure && result.Errors.Any(e => e.Category == ErrorCategory.Validation))
            {
                return new ValidationProblemDetails(new Dictionary<string, string[]> { { "Field", new[] { "Error" } } })
                {
                    Status = 422,
                    Title = result.ErrorMessage ?? "Validation failed"
                };
            }

            return new ProblemDetails
            {
                Status = result.Status.Code,
                Title = result.ErrorMessage ?? "Error",
                Detail = string.Join("; ", result.Messages)
            };
        }
    }
}
