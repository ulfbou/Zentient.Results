namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A concrete internal implementation of IResult&lt;T&gt; for successful outcomes in tests.
    /// </summary>
    internal class ConcreteResult<T> : IResult<T>
    {
        public T? Value { get; }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public IResultStatus Status { get; }
        public IReadOnlyList<ErrorInfo> Errors { get; }
        public IReadOnlyList<string> Messages { get; }

        private readonly Lazy<string?> _firstError;
        public string? ErrorMessage => _firstError.Value;

        // Constructor for a successful result with a value
        public ConcreteResult(T? value, IResultStatus status, IReadOnlyList<string>? messages = null)
        {
            Value = value;
            IsSuccess = true;
            Status = status;
            Messages = messages?.ToArray() ?? Array.Empty<string>();
            Errors = Array.Empty<ErrorInfo>();
            _firstError = new Lazy<string?>(() => null);
        }

        // Minimal constructor for success, defaulting to 200 OK
        public ConcreteResult(T? value) : this(value, new DummyStatus { Code = 200, Description = "OK" }) { }

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
