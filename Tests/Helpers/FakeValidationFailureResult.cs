using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A fake implementation of <see cref="IResult"/> that simulates a validation failure result.
    /// This class is used in unit tests to simulate a successful result without any errors or messages.
    /// </summary>
    internal class FakeValidationFailureResult : IResult
    {

        /// <inheritdoc />
        public bool IsSuccess => false;

        /// <inheritdoc />
        public bool IsFailure => true;

        /// <inheritdoc />
        public IReadOnlyList<ErrorInfo> Errors => new[] { new ErrorInfo(ErrorCategory.Validation, "VAL", "Validation failed") };

        /// <inheritdoc />
        public IReadOnlyList<string> Messages => new[] { "Validation failed" };

        /// <inheritdoc />
        public string? Error => "Validation failed";

        /// <inheritdoc />
        public IResultStatus Status { get; } = new FakeResultStatus(422, "Unprocessable Entity");

        /// <summary>Converts this result to a <see cref="ProblemDetails"/> instance.</summary>
        /// <param name="factory">The <see cref="ProblemDetailsFactory"/> to use for creating the problem details.</param>
        /// <param name="context">The current <see cref="HttpContext"/>.</param>
        public ProblemDetails ToProblemDetails(ProblemDetailsFactory factory, HttpContext context)
        {
            return new ValidationProblemDetails(new Dictionary<string, string[]> { { "Field", new[] { "Error" } } })
            {
                Status = 422,
                Title = "Validation failed"
            };
        }
    }
}
