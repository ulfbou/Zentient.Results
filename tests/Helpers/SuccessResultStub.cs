namespace Zentient.Results.Tests.Helpers
{
    /// <summary>Mock implementation of <see cref="IResult"/> for testing purposes.</summary>
    internal class SuccessResultStub : IResult
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
        public IResultStatus Status { get; }

        public string? ErrorMessage => null;

        /// <summary>Initializes a new instance of the <see cref="SuccessResultStub"/> class with a status.</summary>
        /// <param name="status">The status of the result.</param>
        public SuccessResultStub(IResultStatus status)
        {
            Status = status;
        }
    }
}
