namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A concrete implementation of <see cref="IResult{T}"/> for use in tests.
    /// This class provides a simple way to create results with a value, success status, and optional errors or messages, in a test environment.
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success.</typeparam>
    internal class ConcreteResult<T> : IResult<T>
    {
        /// <inheritdoc />
        public bool IsSuccess { get; init; }

        /// <inheritdoc />
        public bool IsFailure => !IsSuccess;

        /// <inheritdoc />
        public IReadOnlyList<ErrorInfo> Errors { get; init; } = Array.Empty<ErrorInfo>();

        /// <inheritdoc />
        public IReadOnlyList<string> Messages { get; init; } = Array.Empty<string>();

        /// <inheritdoc />
        public string? Error { get; init; }

        /// <inheritdoc />
        public IResultStatus Status { get; init; } = new MockResultStatus(200);

        /// <inheritdoc />
        public T? Value { get; init; }

        /// <inheritdoc />
        public T GetValueOrThrow() => Value!;

        /// <inheritdoc />
        public T GetValueOrThrow(string message) => Value!;

        /// <inheritdoc />
        public T GetValueOrThrow(Func<Exception> exceptionFactory) => Value!;

        /// <inheritdoc />
        public T GetValueOrDefault(T fallback) => Value ?? fallback;

        // Not needed for testing!
        public IResult<U> Map<U>(Func<T, U> selector) => throw new NotImplementedException();
        public IResult<U> Bind<U>(Func<T, IResult<U>> binder) => throw new NotImplementedException();
        public IResult<T> Tap(Action<T> onSuccess) => throw new NotImplementedException();
        public IResult<T> OnSuccess(Action<T> action) => throw new NotImplementedException();
        public IResult<T> OnFailure(Action<IReadOnlyList<ErrorInfo>> action) => throw new NotImplementedException();
        public U Match<U>(Func<T, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure) => throw new NotImplementedException();
    }
}
