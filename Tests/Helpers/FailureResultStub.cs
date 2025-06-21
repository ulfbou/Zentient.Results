using Zentient.Results;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>A concrete internal implementation of IResult for failed outcomes in tests.</summary>
    internal class FailureResultStub : IResult
    {
        public bool IsSuccess => false;
        public bool IsFailure => true;
        public IResultStatus Status { get; }
        public IReadOnlyList<ErrorInfo> Errors { get; }
        public IReadOnlyList<string> Messages { get; }

        private readonly Lazy<string?> _firstError;
        public string? ErrorMessage => _firstError.Value;

        // Constructor for a failed result
        public FailureResultStub(IReadOnlyList<ErrorInfo> errors, IResultStatus status)
        {
            if (errors == null || !errors.Any())
            {
                throw new ArgumentException("Errors cannot be null or empty for a failure stub.", nameof(errors));
            }
            Errors = errors.ToList();
            Status = status;
            Messages = Array.Empty<string>(); // No messages for failure stub
            _firstError = new Lazy<string?>(() => Errors.FirstOrDefault()?.Message);
        }

        // Minimal constructor for failure, defaulting to 400 Bad Request
        public FailureResultStub(ErrorInfo error) : this(new[] { error }, new DummyStatus { Code = 400, Description = "Bad Request" }) { }
    }
}
