namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A concrete internal implementation of IResult&lt;T&gt; for failed outcomes in tests.
    /// </summary>
    internal class FailureResultStub<T> : IResult<T>
    {
        public T? Value { get; } // Value can be default for failures
        public bool IsSuccess => false;
        public bool IsFailure => true;
        public IResultStatus Status { get; }
        public IReadOnlyList<ErrorInfo> Errors { get; }
        public IReadOnlyList<string> Messages { get; }

        private readonly Lazy<string?> _firstError;
        public string? ErrorMessage => _firstError.Value;

        // Constructor for a failed result with a default value
        public FailureResultStub(T? value, IReadOnlyList<ErrorInfo> errors, IResultStatus status)
        {
            if (errors == null || !errors.Any())
            {
                throw new ArgumentException("Errors cannot be null or empty for a failure stub.", nameof(errors));
            }
            Value = value;
            Errors = errors.ToList();
            Status = status;
            Messages = Array.Empty<string>(); // No messages for failure stub
            _firstError = new Lazy<string?>(() => Errors.FirstOrDefault()?.Message);
        }

        // Minimal constructor for failure, defaulting to 400 Bad Request
        public FailureResultStub(T? value, ErrorInfo error) : this(value, new[] { error }, new DummyStatus { Code = 400, Description = "Bad Request" }) { }

        public T GetValueOrThrow() => throw new NotImplementedException();
        public T GetValueOrThrow(string message) => throw new NotImplementedException();
        public T GetValueOrThrow(Func<Exception> exceptionFactory) => throw new NotImplementedException();
        public T GetValueOrDefault(T fallback) => throw new NotImplementedException();
        public IResult<U> Map<U>(Func<T, U> selector) => throw new NotImplementedException();
        public IResult<U> Bind<U>(Func<T, IResult<U>> binder) => throw new NotImplementedException();
        public IResult<T> Tap(Action<T> onSuccess) => throw new NotImplementedException();
        public IResult<T> OnSuccess(Action<T> action) => throw new NotImplementedException();
        public IResult<T> OnFailure(Action<IReadOnlyList<ErrorInfo>> action) => throw new NotImplementedException();
        public U Match<U>(Func<T, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure) => throw new NotImplementedException();
    }
}
