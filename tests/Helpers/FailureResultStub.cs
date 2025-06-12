using Zentient.Results;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>Mock implementation of <see cref="IResult"/> for testing purposes.</summary>
    internal class FailureResultStub : IResult
    {
        /// <inheritdoc />
        public bool IsSuccess => false;

        /// <inheritdoc />
        public bool IsFailure => true;

        /// <inheritdoc />
        public IReadOnlyList<ErrorInfo> Errors { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> Messages { get; }

        /// <inheritdoc />
        public string? Error { get; }

        /// <inheritdoc />
        public IResultStatus Status { get; }

        /// <summary>Initializes a new instance of the <see cref="FailureResultStub"/> class with errors and a status.</summary>
        /// <param name="errors">A collection of errors associated with the failure.</param>
        /// <param name="errorDetail">A detailed error message.</param>
        /// <param name="status">The status of the result.</param>
        public FailureResultStub(IEnumerable<ErrorInfo> errors, string errorDetail, IResultStatus status)
        {
            Errors = errors?.ToList() ?? new List<ErrorInfo>();
            Messages = errors?.Select(e => e.Message).ToList() ?? new List<string>();
            Error = errorDetail;
            Status = status;
        }
    }
}
