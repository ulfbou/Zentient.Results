namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// Mock implementation of <see cref="IResult"/> for testing purposes.
    /// This class is used in unit tests to simulate a successful result without any errors or messages.
    /// </summary>
    internal class FakeFailureResult : IResult
    {
        /// <inheritdoc />
        public bool IsSuccess => false;

        /// <inheritdoc />
        public bool IsFailure => true;

        /// <inheritdoc />
        public IReadOnlyList<ErrorInfo> Errors => new[] { new ErrorInfo(ErrorCategory.General, "ERR", "Failure") };

        /// <inheritdoc />
        public IReadOnlyList<string> Messages => new[] { "Failure" };

        /// <inheritdoc />
        public string? Error => "Failure";

        /// <inheritdoc />
        public IResultStatus Status { get; } = new FakeResultStatus(500, "Internal Server Error");
    }
}
