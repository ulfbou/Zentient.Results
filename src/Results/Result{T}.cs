// <copyright file="Result{T}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

#pragma warning disable CA1000 // Do not declare static members on generic types
#pragma warning disable CA1715 // Identifiers should have correct suffix
namespace Zentient.Results
{
    /// <summary>Represents the outcome of an operation with a return value. It can be a success with a value and optional messages, or a failure with errors.</summary>
    /// <typeparam name="T">The type of the value encapsulated by the result.</typeparam>
    /// <remarks>This class is immutable after creation, with properties initialized via `init` setters.</remarks>
    [DataContract] // Retain for potential WCF/legacy interop, but System.Text.Json is primary
    [JsonConverter(typeof(Serialization.ResultJsonConverter))]
    public sealed class Result<T> : Result, IResult<T>, IEquatable<Result<T>>
    {
        /// <inheritdoc />
        [DataMember(Order = 1)]
        [JsonPropertyName("value")]
        public T? Value { get; init; }

        // Inherits _errors, _messages, IsSuccess, IsFailure, ErrorMessage, Status from Result base class.
        // No need to redeclare private fields or properties that are already on the base class.

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class.
        /// </summary>
        /// <param name="value">The value to encapsulate (for success results).</param>
        /// <param name="status">The status of the result.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <param name="errors">Optional error information.</param>
        [JsonConstructor] // Explicitly inform System.Text.Json to use this constructor
        internal Result(
            T? value,
            IResultStatus status,
            IEnumerable<string>? messages = null,
            IEnumerable<ErrorInfo>? errors = null)
            : base(status, messages, errors)
        {
            Value = value;
        }

        /// <summary>Creates a successful generic result.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Success"/>.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/>.</returns>
        public static IResult<T> Success(T value, IResultStatus? status = null, IEnumerable<string>? messages = null) =>
            new Result<T>(value, status ?? ResultStatuses.Success, messages);

        /// <summary>Creates a successful generic result with a single message.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/>.</returns>
        public static IResult<T> Success(T value, string? message) =>
            Success(value, messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful generic result with a "Created" status.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Created"/>.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with a "Created" status.</returns>
        public static IResult<T> Created(T value, IResultStatus? status = null, IEnumerable<string>? messages = null) =>
            new Result<T>(value, status ?? ResultStatuses.Created, messages);

        /// <summary>Creates a successful generic result with a "Created" status and a single message.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with a "Created" status.</returns>
        public static IResult<T> Created(T value, string? message) =>
            Created(value, messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful generic result with an "Accepted" status.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Accepted"/>.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with an "Accepted" status.</returns>
        public static IResult<T> Accepted(T value, IResultStatus? status = null, IEnumerable<string>? messages = null) =>
            new Result<T>(value, status ?? ResultStatuses.Accepted, messages);

        /// <summary>Creates a successful generic result with an "Accepted" status and a single message.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with an "Accepted" status.</returns>
        public static IResult<T> Accepted(T value, string? message) =>
            Accepted(value, messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful generic result with a "No Content" status.</summary>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.NoContent"/>.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with a "No Content" status.</returns>
        public new static IResult<T> NoContent(IResultStatus? status = null, IEnumerable<string>? messages = null) =>
            new Result<T>(default, status ?? ResultStatuses.NoContent, messages);

        /// <summary>Creates a successful generic result with a "No Content" status and a single message.</summary>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with a "No Content" status.</returns>
        public new static IResult<T> NoContent(string? message) =>
            NoContent(messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful generic result with an "OK" status.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with an "OK" status.</returns>
        public static IResult<T> Ok(T value, string? message = null) =>
            Success(value, ResultStatuses.Success, !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a generic failure result from a single error.</summary>
        /// <param name="value">Optional value to encapsulate (can be default or partial data on failure).</param>
        /// <param name="error">The error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/>.</returns>
        public static IResult<T> Failure(T? value, ErrorInfo error, IResultStatus? status = null) =>
            new Result<T>(value, status ?? ResultStatuses.BadRequest, null, new[] { error });

        /// <summary>Creates a generic failure result from a single error, with default value.</summary>
        /// <param name="error">The error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/>.</returns>
        public new static IResult<T> Failure(ErrorInfo error, IResultStatus? status = null) =>
            Failure(default, error, status);

        /// <summary>
        /// Creates a generic failure result from a collection of errors.
        /// </summary>
        /// <param name="value">Optional value to encapsulate (can be default or partial data on failure).</param>
        /// <param name="errors">A collection of error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        /// <returns>A failed <see cref="IResult{TValue}"/>.</returns>
        public static IResult<T> Failure(T? value, IEnumerable<ErrorInfo> errors, IResultStatus? status = null)
        {
            var arr = errors as ErrorInfo[] ?? errors?.ToArray() ?? throw new ArgumentNullException(nameof(errors));
            if (arr.Length == 0)
            {
                throw new ArgumentException("Error messages cannot be null or empty.", nameof(errors));
            }

            return new Result<T>(value, status ?? ResultStatuses.BadRequest, null, arr);
        }

        /// <summary>
        /// Creates a generic failure result from a collection of errors, with default value.
        /// </summary>
        /// <param name="errors">A collection of error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        /// <returns>A failed <see cref="IResult{TValue}"/>.</returns>
        public new static IResult<T> Failure(IEnumerable<ErrorInfo> errors, IResultStatus? status = null) =>
            Failure(default, errors, status);

        /// <summary>Creates a generic failure result representing validation errors.</summary>
        /// <param name="errors">A collection of validation errors.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.UnprocessableEntity"/>.</returns>
        public new static IResult<T> Validation(IEnumerable<ErrorInfo> errors) =>
            Failure(default, errors, ResultStatuses.UnprocessableEntity);

        /// <summary>Creates a generic failure result for "Not Found" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.NotFound"/>.</returns>
        public new static IResult<T> NotFound(string message = "Resource not found", string? code = null) =>
            Failure(default, ErrorInfo.NotFound(message, code), ResultStatuses.NotFound);

        /// <summary>Creates a generic failure result for "Unauthorized" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.Unauthorized"/>.</returns>
        public new static IResult<T> Unauthorized(string message = "Unauthorized access", string? code = null) =>
            Failure(default, ErrorInfo.Unauthorized(message, code), ResultStatuses.Unauthorized);

        /// <summary>Creates a generic failure result for "Forbidden" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.Forbidden"/>.</returns>
        public new static IResult<T> Forbidden(string message = "Access to resource is forbidden", string? code = null) =>
            Failure(default, ErrorInfo.Forbidden(message, code), ResultStatuses.Forbidden);

        /// <summary>Creates a generic failure result for "Conflict" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.Conflict"/>.</returns>
        public new static IResult<T> Conflict(string message = "Conflict occurred", string? code = null) =>
            Failure(default, ErrorInfo.Conflict(message, code), ResultStatuses.Conflict);

        /// <summary>Creates a generic failure result for "Request Timeout" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.RequestTimeout"/>.</returns>
        public new static IResult<T> RequestTimeout(string message = "Request timed out", string? code = null) =>
            Failure(default, new ErrorInfo(ErrorCategory.Timeout, code ?? "RequestTimeout", message), ResultStatuses.RequestTimeout);

        /// <summary>Creates a generic failure result for "Gone" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.Gone"/>.</returns>
        public new static IResult<T> Gone(string message = "Resource is no longer available", string? code = null) =>
            Failure(default, new ErrorInfo(ErrorCategory.ResourceGone, code ?? "Gone", message), ResultStatuses.Gone);

        /// <summary>Creates a generic failure result for "Precondition Failed" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.PreconditionFailed"/>.</returns>
        public new static IResult<T> PreconditionFailed(string message = "Precondition failed", string? code = null) =>
            Failure(default, new ErrorInfo(ErrorCategory.Validation, code ?? "PreconditionFailed", message), ResultStatuses.PreconditionFailed);

        /// <summary>Creates a generic failure result for "Too Many Requests" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.TooManyRequests"/>.</returns>
        public new static IResult<T> TooManyRequests(string message = "Too many requests", string? code = null) =>
            Failure(default, new ErrorInfo(ErrorCategory.RateLimit, code ?? "TooManyRequests", message), ResultStatuses.TooManyRequests);

        /// <summary>Creates a generic failure result for "Not Implemented" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.NotImplemented"/>.</returns>
        public new static IResult<T> NotImplemented(string message = "Operation not implemented", string? code = null) =>
            Failure(default, new ErrorInfo(ErrorCategory.NotImplemented, code ?? "NotImplemented", message), ResultStatuses.NotImplemented);

        /// <summary>Creates a generic failure result for "Service Unavailable" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/> with status <see cref="ResultStatuses.ServiceUnavailable"/>.</returns>
        public new static IResult<T> ServiceUnavailable(string message = "Service is temporarily unavailable", string? code = null) =>
            Failure(default, new ErrorInfo(ErrorCategory.ServiceUnavailable, code ?? "ServiceUnavailable", message), ResultStatuses.ServiceUnavailable);

        /// <summary>
        /// Creates a generic failure result from an exception.
        /// </summary>
        /// <param name="value">Optional value to encapsulate (can be default or partial data on failure).</param>
        /// <param name="ex">The exception to convert into an error.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Error"/>.</param>
        /// <returns>A failed <see cref="IResult{TValue}"/>.</returns>
        public static IResult<T> FromException(T? value, Exception ex, IResultStatus? status = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            return Failure(value, ErrorInfo.FromException(ex), status ?? ResultStatuses.Error);
        }

        /// <summary>Allows implicit conversion from a value of type <typeparamref name="T"/> to a successful <see cref="Result{T}"/>.</summary>
        /// <param name="value">The value to encapsulate.</param>
        public static implicit operator Result<T>(T value) =>
            value is null ? (Result<T>)NoContent() : (Result<T>)Success(value);

        /// <inheritdoc />
        public IResult<U> Map<U>(Func<T, U> selector)
        {
            ArgumentNullException.ThrowIfNull(selector, nameof(selector));
            return IsSuccess ? Result<U>.Success(selector(Value!)) : Result<U>.Failure(Errors, Status);
        }

        /// <summary>Transforms the errors of a failed result using a specified error mapper function.</summary>
        /// <param name="errorMapper">A function that transforms the current list of <see cref="ErrorInfo"/> into a new list of <see cref="ErrorInfo"/>.</param>
        /// <returns>A new <see cref="IResult{T}"/> with transformed errors if the current result is a failure; otherwise, the current successful result.</returns>
        public new IResult<T> MapError(Func<IReadOnlyList<ErrorInfo>, IReadOnlyList<ErrorInfo>> errorMapper)
        {
            ArgumentNullException.ThrowIfNull(errorMapper, nameof(errorMapper));
            return IsSuccess ? this : Failure(Value, errorMapper(Errors), Status);
        }

        /// <inheritdoc />
        public IResult<U> Bind<U>(Func<T, IResult<U>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder, nameof(binder));
            return IsSuccess ? binder(Value!) : Result<U>.Failure(Errors, Status);
        }

        /// <inheritdoc />
        public IResult<T> Tap(Action<T> onSuccess)
        {
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
            if (IsSuccess) onSuccess(Value!);
            return this;
        }

        /// <inheritdoc />
        public IResult<T> OnSuccess(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            if (IsSuccess) action(Value!);
            return this;
        }

        /// <inheritdoc />
        public new IResult<T> OnFailure(Action<IReadOnlyList<ErrorInfo>> action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            if (IsFailure) action(Errors);
            return this;
        }

        /// <inheritdoc />
        public U Match<U>(Func<T, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
            ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));
            return IsSuccess ? onSuccess(Value!) : onFailure(Errors);
        }

        /// <summary>Executes an action regardless of whether the result is a success or a failure.</summary>
        /// <param name="onCompletion">The action to execute.</param>
        /// <returns>The current <see cref="IResult{T}"/> instance.</returns>
        public new IResult<T> Finally(Action onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion, nameof(onCompletion));
            onCompletion();
            return this;
        }

        /// <summary>Executes a function regardless of whether the result is a success or a failure, and returns a new value.</summary>
        /// <typeparam name="U">The type of the value returned by the function.</typeparam>
        /// <param name="onCompletion">The function to execute.</param>
        /// <returns>The result of the executed function.</returns>
        public new U Finally<U>(Func<U> onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion, nameof(onCompletion));
            return onCompletion();
        }

        /// <inheritdoc />
        [return: NotNull]
        public T GetValueOrThrow() =>
            IsSuccess ? Value! : throw new InvalidOperationException(ErrorMessage ?? "Result was not successful.");

        /// <inheritdoc />
        [return: NotNull]
        public T GetValueOrThrow(string message) =>
            IsSuccess ? Value! : throw new InvalidOperationException(message);

        /// <inheritdoc />
        [return: NotNull]
        public T GetValueOrThrow(Func<Exception> exceptionFactory)
        {
            ArgumentNullException.ThrowIfNull(exceptionFactory, nameof(exceptionFactory));
            return IsSuccess ? Value! : throw exceptionFactory();
        }

        /// <inheritdoc />
        [return: NotNullIfNotNull(nameof(fallback))]
        public T GetValueOrDefault(T fallback) => IsSuccess && Value is not null ? Value : fallback;

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsSuccess)
            {
                return $"Result<{typeof(T).Name}>: Success ({Status}) | Value: {Value?.ToString() ?? "null"}{(Messages.Any() ? $" | Message: {string.Join("; ", Messages)}" : "")}";
            }

            return $"Result<{typeof(T).Name}>: Failure ({Status}) | Error: {ErrorMessage ?? "No specific error message."} | All Errors: {string.Join("; ", Errors.Select(e => e.ToString()))}";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Result<T>);

        /// <inheritdoc />
        public bool Equals(Result<T>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) &&
                   EqualityComparer<T?>.Default.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Value);
            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(Result<T>? left, Result<T>? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator !=(Result<T>? left, Result<T>? right) => !(left == right);
    }
}
#pragma warning restore CA1715 // Identifiers should have correct suffix
#pragma warning restore CA1000 // Do not declare static members on generic types
