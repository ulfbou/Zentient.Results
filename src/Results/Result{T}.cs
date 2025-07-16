// <copyright file="Result{T}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Zentient.Results
{
    /// <summary>Represents the outcome of an operation with a return value. It can be a success with a value and optional messages, or a failure with errors.</summary>
    /// <typeparam name="T">The type of the value encapsulated by the result.</typeparam>
    /// <remarks>This class is immutable after creation, with properties initialized via `init` setters.</remarks>
    [DataContract]
    [JsonConverter(typeof(Serialization.ResultJsonConverter))]
    public sealed class Result<T> : Result, IResult<T>, IEquatable<Result<T>>
    {
        /// <inheritdoc />
        [DataMember(Order = 4)]
        [JsonPropertyName("value")]
        public T? Value { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class.
        /// This is the primary constructor, determining success/failure based on the presence of errors.
        /// </summary>
        /// <param name="value">The value to encapsulate (for success results). Can be <see langword="null"/> for failed results.</param>
        /// <param name="messages">Optional informational messages. If <see langword="null"/>, an empty list is used.</param>
        /// <param name="errors">Optional error information. If <see langword="null"/> or empty, the result is considered a success.</param>
        [JsonConstructor]
        internal Result(
            T? value,
            IEnumerable<string>? messages = null,
            IEnumerable<ErrorInfo>? errors = null)
            : base(messages, errors)
        {
            // Only set Value if the result is intended to be a success.
            // If errors are present, Value should typically be default(T) or null,
            // but we allow it to be passed for scenarios where partial data might accompany an error.
            Value = value;
        }

        #region Static Factory Methods

        /// <summary>Creates a successful generic result.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult{T}"/>.</returns>
        public static IResult<T> Success(T value, IEnumerable<string>? messages = null)
            => new Result<T>(value, messages);

        /// <summary>Creates a successful generic result with a single message.</summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{T}"/>.</returns>
        public static IResult<T> Success(T value, string? message)
            => Success(value, messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a generic failure result from a single error.</summary>
        /// <param name="value">Optional value to encapsulate (can be default or partial data on failure).</param>
        /// <param name="error">The error information.</param>
        /// <returns>A failed <see cref="IResult{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is null.</exception>
        public static IResult<T> Failure(T? value, ErrorInfo error)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return new Result<T>(value, new[] { error.Message! }, new[] { error });
        }

        /// <summary>Creates a generic failure result from a single error, with default value.</summary>
        /// <param name="error">The error information.</param>
        /// <returns>A failed <see cref="IResult{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is null.</exception>
        public static new IResult<T> Failure(ErrorInfo error) =>
            Failure(default, error);

        /// <summary>
        /// Creates a generic failure result from a collection of errors.
        /// </summary>
        /// <param name="value">Optional value to encapsulate (can be default or partial data on failure).</param>
        /// <param name="errors">A collection of error information.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        /// <returns>A failed <see cref="IResult{T}"/>.</returns>
        public static IResult<T> Failure(T? value, IEnumerable<ErrorInfo> errors)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            var errorArray = errors as ErrorInfo[] ?? errors.ToArray();

            if (errorArray.Length == 0)
            {
                throw new ArgumentException("Error collection cannot be empty when creating a Failure result.", nameof(errors));
            }

            List<string> errorMessages = errorArray
                .Where(e => !string.IsNullOrWhiteSpace(e.Message))
                .Select(e => e.Message!)
                .Distinct()
                .ToList();

            return new Result<T>(value, errorMessages.Any() ? errorMessages : null, errorArray);
        }

        /// <summary>
        /// Creates a generic failure result from a collection of errors, with default value.
        /// </summary>
        /// <param name="errors">A collection of error information.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        /// <returns>A failed <see cref="IResult{T}"/>.</returns>
        public static new IResult<T> Failure(IEnumerable<ErrorInfo> errors)
            => Failure(default, errors);

        /// <summary>Creates a generic failure result representing validation errors.</summary>
        /// <param name="errors">A collection of validation errors.</param>
        /// <returns>A failed <see cref="IResult{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static new IResult<T> Validation(IEnumerable<ErrorInfo> errors)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            var errorArray = errors as ErrorInfo[] ?? errors.ToArray();

            if (errorArray.Length == 0)
            {
                throw new ArgumentException("Validation errors cannot be empty.", nameof(errors));
            }

            return Failure(default, errorArray);
        }

        /// <summary>
        /// Creates a generic failure result from an exception.
        /// </summary>
        /// <param name="value">Optional value to encapsulate (can be default or partial data on failure).</param>
        /// <param name="ex">The exception to convert into an error.</param>
        /// <param name="innerErrors">Optional inner error information to include with the exception's error.</param>
        /// <returns>A failed <see cref="IResult{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
        public static IResult<T> FromException(T? value, Exception ex, IEnumerable<ErrorInfo>? innerErrors = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            innerErrors ??= Enumerable.Empty<ErrorInfo>();
            return Failure(value, ErrorInfo.FromException(ex, ex.Message, innerErrors: innerErrors));
        }

        /// <summary>Creates a generic failure result from an exception, with default value.</summary>
        /// <param name="ex">The exception to convert into an error.</param>
        /// <param name="innerErrors">Optional inner error information to include with the exception's error.</param>
        /// <returns>A failed <see cref="IResult{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
        public static IResult<T> FromException(Exception ex, IEnumerable<ErrorInfo>? innerErrors = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            return FromException(default, ex, innerErrors);
        }

        #endregion

        #region Operators and Overrides

        /// <inheritdoc />
        public static bool operator ==(Result<T>? left, Result<T>? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator !=(Result<T>? left, Result<T>? right) => !(left == right);

        /// <summary>Allows implicit conversion from a value of type <typeparamref name="T"/> to a successful <see cref="Result{T}"/>.</summary>
        /// <param name="value">The value to encapsulate.</param>
        public static implicit operator Result<T>(T value) =>
            (Result<T>)Success(value);

        /// <inheritdoc />
        protected override IResult GetFirstErrorResult()
        {
            return Errors.Any()
                ? Result.Failure(Errors[0])
                : Result.Failure(ErrorInfo.General("NoErrors", "No errors found for a failure result."));
        }

        /// <inheritdoc />
        protected override IResult MapErrorInternal(Func<IReadOnlyList<ErrorInfo>, IReadOnlyList<ErrorInfo>> errorMapper)
            => Failure(Value, errorMapper(Errors));

        /// <inheritdoc />
        public override string ToString()
        {
            return IsSuccess
                ? $"Result<{typeof(T).Name}>: Success | Value: {Value?.ToString() ?? (Value is null ? "null" : $"<{typeof(T).Name} instance>")}{(Messages.Any() ? $" | Messages: {string.Join("; ", Messages)}" : "")}"
                : $"Result<{typeof(T).Name}>: {base.ToString()}";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Result<T>);

        /// <inheritdoc />
        public bool Equals(Result<T>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return base.Equals(other)
                && EqualityComparer<T?>.Default.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Value);
            return hash.ToHashCode();
        }

        #endregion

        #region Fluent API (Value-Specific)

        /// <summary>
        /// Transforms the encapsulated value of a successful result using a specified selector function.
        /// If the current result is a failure, it propagates the failure without executing the selector.
        /// </summary>
        /// <typeparam name="U">The type of the value to which the result will be transformed.</typeparam>
        /// <param name="selector">A function that transforms the current value into a new value of type <typeparamref name="U"/>.</param>
        /// <returns>A new <see cref="IResult{U}"/> instance representing the transformed result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="selector"/> is null.</exception>
        public IResult<U> Map<U>(Func<T, U> selector)
        {
            ArgumentNullException.ThrowIfNull(selector, nameof(selector));
            try
            {
                return IsSuccess ? Result<U>.Success(value: selector(Value!), Messages) : Result<U>.Failure(Errors);
            }
            catch (Exception ex)
            {
                return Result<U>.FromException(ex, Errors);
            }
        }

        /// <summary>
        /// Transforms the encapsulated value of a successful result using an asynchronous selector function.
        /// If the current result is a failure, it propagates the failure without executing the selector.
        /// </summary>
        /// <typeparam name="U">The type of the value to which the result will be transformed.</typeparam>
        /// <param name="selector">An asynchronous function that transforms the current value into a new value of type <typeparamref name="U"/>.</param>
        /// <returns>A <see cref="Task{IResult{U}}"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="selector"/> is null.</exception>
        public async Task<IResult<U>> MapAsync<U>(Func<T, Task<U>> selector)
        {
            ArgumentNullException.ThrowIfNull(selector, nameof(selector));
            if (IsFailure) return Result<U>.Failure(Errors);

            try
            {
                return Result<U>.Success(value: await selector(Value!).ConfigureAwait(false), Messages);
            }
            catch (Exception ex)
            {
                return Result<U>.FromException(ex, Errors);
            }
        }

        /// <summary>
        /// Chains a subsequent operation that produces a new <see cref="IResult{U}"/> based on the current successful result's value.
        /// This is useful for flattening nested results (e.g., `Result<Result<U>>` becomes `Result<U>`).
        /// If the current result is a failure, it propagates the failure without executing the binder.
        /// </summary>
        /// <typeparam name="U">The type of the value encapsulated by the new result.</typeparam>
        /// <param name="binder">A function that takes the current value and returns a new <see cref="IResult{U}"/>.</param>
        /// <returns>A new <see cref="IResult{U}"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="binder"/> is null.</exception>
        public IResult<U> Bind<U>(Func<T, IResult<U>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder, nameof(binder));
            return IsSuccess ? binder(Value!) : Result<U>.Failure(Errors);
        }

        /// <summary>
        /// Chains a subsequent asynchronous operation that produces a new <see cref="IResult{U}"/> based on the current successful result's value.
        /// This is useful for flattening nested results (e.g., `Result<Result<U>>` becomes `Result<U>`).
        /// If the current result is a failure, it propagates the failure without executing the binder.
        /// </summary>
        /// <typeparam name="U">The type of the value encapsulated by the new result.</typeparam>
        /// <param name="binder">An asynchronous function that takes the current value and returns a new <see cref="IResult{U}"/>.</param>
        /// <returns>A <see cref="Task{IResult{U}}"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="binder"/> is null.</exception>
        public async Task<IResult<U>> BindAsync<U>(Func<T, Task<IResult<U>>> binder)
        {
            ArgumentNullException.ThrowIfNull(binder, nameof(binder));
            return IsSuccess ? await binder(Value!).ConfigureAwait(false) : Result<U>.Failure(Errors);
        }

        /// <summary>
        /// Executes an action on the encapsulated value if the result is successful, and then returns the original result.
        /// Useful for side-effects like logging or auditing.
        /// </summary>
        /// <param name="onSuccess">The action to execute, receiving the encapsulated value.</param>
        /// <returns>The current <see cref="IResult{T}"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onSuccess"/> is null.</exception>
        public IResult<T> Tap(Action<T> onSuccess)
        {
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
            if (IsSuccess) onSuccess(Value!);
            return this;
        }

        /// <summary>
        /// Executes an asynchronous action on the encapsulated value if the result is successful, and then returns the original result.
        /// Useful for side-effects like logging or auditing.
        /// </summary>
        /// <param name="onSuccess">The asynchronous action to execute, receiving the encapsulated value.</param>
        /// <returns>A <see cref="Task{IResult{T}}"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onSuccess"/> is null.</exception>
        public async Task<IResult<T>> TapAsync(Func<T, Task> onSuccess)
        {
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
            if (IsSuccess) await onSuccess(Value!).ConfigureAwait(false);
            return this;
        }

        /// <summary>
        /// Executes a specified action if the result is successful, providing the encapsulated value.
        /// (This is an alias for Tap, kept for common terminology).
        /// </summary>
        /// <param name="action">The action to execute, receiving the encapsulated value.</param>
        /// <returns>The current <see cref="IResult{T}"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
        public IResult<T> OnSuccess(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            if (IsSuccess) action(Value!);
            return this;
        }

        /// <summary>
        /// Executes a specified asynchronous action if the result is successful, providing the encapsulated value.
        /// (This is an alias for TapAsync, kept for common terminology).
        /// </summary>
        /// <param name="action">The asynchronous action to execute, receiving the encapsulated value.</param>
        /// <returns>A <see cref="Task{IResult{T}}"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
        public async Task<IResult<T>> OnSuccessAsync(Func<T, Task> action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            if (IsSuccess) await action(Value!).ConfigureAwait(false);
            return this;
        }

        /// <summary>Executes a specified action if the result is a failure, providing a list of <see cref="ErrorInfo"/>.</summary>
        /// <param name="action">The action to execute, receiving a list of <see cref="ErrorInfo"/>.</param>
        /// <returns>The current <see cref="IResult{T}"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
        public new IResult<T> OnFailure(Action<IReadOnlyList<ErrorInfo>> action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            if (IsFailure) action(Errors);
            return this;
        }

        /// <summary>Executes a specified asynchronous action if the result is a failure, providing a list of <see cref="ErrorInfo"/>.</summary>
        /// <param name="action">The asynchronous action to execute, receiving a list of <see cref="ErrorInfo"/>.</param>
        /// <returns>A <see cref="Task{IResult{T}}"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
        public async Task<IResult<T>> OnFailureAsync(Func<IReadOnlyList<ErrorInfo>, Task> action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));
            if (IsFailure) await action(Errors).ConfigureAwait(false);
            return this;
        }

        /// <summary>
        /// Executes one of two functions based on whether the result is a success or a failure, and returns a new value.
        /// </summary>
        /// <typeparam name="U">The type of the value returned by the functions.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is successful, receiving the encapsulated value.</param>
        /// <param name="onFailure">The function to execute if the result is a failure, receiving a list of <see cref="ErrorInfo"/>.</param>
        /// <returns>The result of the executed function.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
        public U Match<U>(Func<T, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
            ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));
            return IsSuccess ? onSuccess(Value!) : onFailure(Errors);
        }

        /// <summary>
        /// Executes one of two asynchronous functions based on whether the result is a success or a failure, and returns a new value.
        /// </summary>
        /// <typeparam name="U">The type of the value returned by the functions.</typeparam>
        /// <param name="onSuccess">The asynchronous function to execute if the result is successful, receiving the encapsulated value.</param>
        /// <param name="onFailure">The asynchronous function to execute if the result is a failure, receiving a list of <see cref="ErrorInfo"/>.</param>
        /// <returns>A <see cref="Task{U}"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onSuccess"/> or <paramref name="onFailure"/> is null.</exception>
        public async Task<U> MatchAsync<U>(Func<T, Task<U>> onSuccess, Func<IReadOnlyList<ErrorInfo>, Task<U>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
            ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));
            return IsSuccess ? await onSuccess(Value!).ConfigureAwait(false) : await onFailure(Errors).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes an action regardless of whether the result is a success or a failure.
        /// </summary>
        /// <param name="onCompletion">The action to execute.</param>
        /// <returns>The current <see cref="IResult{T}"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onCompletion"/> is null.</exception>
        public new IResult<T> Finally(Action onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion, nameof(onCompletion));
            onCompletion();
            return this;
        }

        /// <summary>
        /// Executes an asynchronous action regardless of whether the result is a success or a failure.
        /// </summary>
        /// <param name="onCompletion">The asynchronous action to execute.</param>
        /// <returns>A <see cref="Task{IResult{T}}"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onCompletion"/> is null.</exception>
        public new async Task<IResult<T>> FinallyAsync(Func<Task> onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion, nameof(onCompletion));
            await onCompletion().ConfigureAwait(false);
            return this;
        }

        /// <summary>
        /// Executes a function regardless of whether the result is a success or a failure, and returns a new value.
        /// </summary>
        /// <typeparam name="U">The type of the value returned by the function.</typeparam>
        /// <param name="onCompletion">The function to execute, receiving the current result.</param>
        /// <returns>The result of the executed function.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onCompletion"/> is null.</exception>
        public U Finally<U>(Func<IResult<T>, U> onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion, nameof(onCompletion));
            return onCompletion(this);
        }

        /// <summary>
        /// Executes an asynchronous function regardless of whether the result is a success or a failure, and returns a new value.
        /// </summary>
        /// <typeparam name="U">The type of the value returned by the function.</typeparam>
        /// <param name="onCompletion">The asynchronous function to execute, receiving the current result.</param>
        /// <returns>A <see cref="Task{U}"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="onCompletion"/> is null.</exception>
        public async Task<U> FinallyAsync<U>(Func<IResult<T>, Task<U>> onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion, nameof(onCompletion));
            return await onCompletion(this).ConfigureAwait(false);
        }

        #endregion

        #region Value Access

        /// <inheritdoc />
        [return: NotNull]
        public T GetValueOrThrow() =>
            IsSuccess
            ? Value!
            : throw new InvalidOperationException(ErrorMessage ?? "Result was not successful and no specific error message was available.");

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

        #endregion
    }
}
