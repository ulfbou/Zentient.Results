
namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A concrete internal implementation of IResult for successful outcomes in tests.
    /// </summary>
    internal class ConcreteResult : IResult
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public IResultStatus Status { get; }
        public IReadOnlyList<ErrorInfo> Errors { get; }
        public IReadOnlyList<string> Messages { get; }

        private readonly Lazy<string?> _firstError;
        public string? ErrorMessage => _firstError.Value;

        // Constructor for a successful result
        public ConcreteResult(IResultStatus status, IReadOnlyList<string>? messages = null)
        {
            IsSuccess = true;
            Status = status;
            Messages = messages?.ToArray() ?? Array.Empty<string>();
            Errors = Array.Empty<ErrorInfo>();
            _firstError = new Lazy<string?>(() => null);
        }

        // Minimal constructor for success, defaulting to 200 OK
        public ConcreteResult() : this(new DummyStatus { Code = 200, Description = "OK" }) { }
    }
}
