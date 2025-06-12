namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A concrete implementation of <see cref="IResult"/> for use in tests.
    /// This class provides a simple way to create results with a value, success status, and optional errors or messages, in a test environment.
    /// </summary>
    internal class ConcreteResult : IResult
    {
        /// <inheritdoc />
        public bool IsSuccess { get; init; }

        public bool IsFailure { get; }

        public IReadOnlyList<ErrorInfo> Errors { get; }

        public IReadOnlyList<string> Messages { get; }

        public string? Error { get; }

        public IResultStatus Status { get; }

        public ConcreteResult(
            bool isSuccess,
            IReadOnlyList<ErrorInfo> errors = null,
            IReadOnlyList<string> messages = null,
            string? error = null,
            IResultStatus status = null)
        {
            IsSuccess = isSuccess;
            IsFailure = !isSuccess;
            Errors = errors ?? Array.Empty<ErrorInfo>();
            Messages = messages ?? Array.Empty<string>();
            Error = error;
            Status = status ?? new MockResultStatus(200);
        }
    }
}
