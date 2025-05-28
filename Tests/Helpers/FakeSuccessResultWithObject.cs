using System.Net;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A mock implementation of <see cref="IResult{T}"/> that represents a successful operation with an object value.
    /// This class is used in unit tests to simulate a successful result without any errors or messages.
    /// </summary>
    internal class FakeSuccessResultWithObject : IResult<object>
    {
        /// <inheritdoc />
        public FakeSuccessResultWithObject(object? value) => Value = value;

        /// <inheritdoc />
        public object? Value { get; }

        /// <inheritdoc />
        public object GetValueOrThrow() => Value!;

        /// <inheritdoc />
        public object GetValueOrThrow(string message) => Value!;

        /// <inheritdoc />
        public object GetValueOrThrow(Func<Exception> exceptionFactory) => Value!;

        /// <inheritdoc />
        public object GetValueOrDefault(object fallback) => Value ?? fallback;

        /// <inheritdoc />
        public IResult<U> Map<U>(Func<object, U> selector) => throw new NotImplementedException();

        /// <inheritdoc />
        public IResult<U> Bind<U>(Func<object, IResult<U>> binder) => throw new NotImplementedException();

        /// <inheritdoc />
        public IResult<object> Tap(Action<object> onSuccess) => this;

        /// <inheritdoc />
        public IResult<object> OnSuccess(Action<object> action) => this;

        /// <inheritdoc />
        public IResult<object> OnFailure(Action<IReadOnlyList<ErrorInfo>> action) => this;

        /// <inheritdoc />

        /// <inheritdoc />
        public U Match<U>(Func<object, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure) => onSuccess(Value!);

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
        public IResultStatus Status { get; } = new FakeResultStatus((int)HttpStatusCode.OK, "OK");
    }
}
