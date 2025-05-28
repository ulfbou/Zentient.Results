namespace Zentient.Results.Tests.Helpers
{
    /// <summary>Mock implementation of <see cref="IResult{T}"/> for testing purposes.</summary>
    /// typeparam name="T">The type of the value returned on failure.</typeparam>
    internal class FailureResultStub<T> : IResult<T>
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

        /// <inheritdoc />
        public T Value { get; }

        /// <summary>Initializes a new instance of the <see cref="FailureResultStub{T}"/> class with errors, error detail, value, and status.</summary>
        /// <param name="errors">A collection of errors associated with the failure.</param>
        /// <param name="errorDetail">A detailed error message.</param>
        /// <param name="value">The value associated with the failure (can be null).</param>
        public FailureResultStub(IEnumerable<ErrorInfo> errors, string errorDetail, T value, IResultStatus status)
        {
            Errors = errors?.ToList() ?? new List<ErrorInfo>();
            Messages = errors?.Select(e => e.Message).ToList() ?? new List<string>();
            Error = errorDetail;
            Value = value;
            Status = status;
        }

        // Not implemented!
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
