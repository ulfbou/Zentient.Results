namespace Zentient.Results.Tests.Helpers
{
    /// <summary>Mock implementation of <see cref="IResult{T}"/> for testing purposes.</summary>
    /// typeparam name="T">The type of the value returned on success.</typeparam>
    internal class SuccessResultStub<T> : IResult<T>
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
        public IResultStatus Status { get; init; }

        /// <inheritdoc />
        public T Value { get; init; }

        public string? ErrorMessage { get; init; }

        /// <summary>Initializes a new instance of the <see cref="SuccessResultStub{T}"/> class with a value and status.</summary>
        /// <param name="value">The value to return on success.</param>
        /// <param name="status">The status of the result.</param>
        public SuccessResultStub(T value, IResultStatus status)
        {
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
