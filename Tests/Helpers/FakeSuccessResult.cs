using System.Net;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A mock implementation of <see cref="IResult"/> that represents a successful operation.
    /// This class is used in unit tests to simulate a successful result without any errors or messages.
    /// </summary>
    internal class FakeSuccessResult : IResult
    {
        /// <inheritdoc />
        public bool IsSuccess => true;

        /// <inheritdoc />
        public bool IsFailure => false;

        /// <inheritdoc />
        public IReadOnlyList<ErrorInfo> Errors => Array.Empty<ErrorInfo>();

        /// <inheritdoc />
        public IReadOnlyList<string> Messages => Array.Empty<string>();

        /// <inheritdoc />
        public string? Error => null;

        /// <inheritdoc />
        public IResultStatus Status { get; } = new FakeResultStatus((int)HttpStatusCode.NoContent, "No Content");
    }
}
