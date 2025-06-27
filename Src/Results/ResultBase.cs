// <copyright file="ResultBase.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Zentient.Results
{
    /// <summary>
    /// Serves as the abstract base class for result types, encapsulating common logic for success/failure state,
    /// error and message storage, and status representation.
    /// </summary>
    /// <remarks>
    /// This class is immutable after creation, with properties initialized via constructors.
    /// Derived types should implement specific result behaviors (e.g., generic/non-generic).
    /// </remarks>
    [DataContract]
    public abstract class ResultBase : IResult, IEquatable<ResultBase>
    {
        [DataMember(Order = 1)]
        [JsonInclude]
        private readonly ErrorInfo[] _errors;

        [DataMember(Order = 2)]
        [JsonInclude]
        private readonly string[] _messages;

        [JsonIgnore]
        private readonly Lazy<string?> _firstError;

        /// <inheritdoc />
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; protected init; }

        /// <inheritdoc />
        [JsonPropertyName("isFailure")]
        public bool IsFailure { get; protected init; }

        /// <inheritdoc />
        [JsonPropertyName("errors")]
        public IReadOnlyList<ErrorInfo> Errors => _errors;

        /// <inheritdoc />
        [JsonPropertyName("messages")]
        public IReadOnlyList<string> Messages => _messages;

        /// <inheritdoc />
        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage => _firstError.Value;

        /// <inheritdoc />
        [DataMember(Order = 3)]
        [JsonPropertyName("status")]
        public IResultStatus Status { get; protected init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultBase"/> class.
        /// </summary>
        /// <param name="status">The status of the result.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <param name="errors">Optional error information.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="status"/> is null.</exception>
        protected ResultBase(IResultStatus status, IEnumerable<string>? messages = null, IEnumerable<ErrorInfo>? errors = null)
        {
            Status = status ?? throw new ArgumentNullException(nameof(status));

            _errors = errors is null ? Array.Empty<ErrorInfo>() : (errors as ErrorInfo[] ?? errors.ToArray());
            _messages = messages is null ? Array.Empty<string>() : (messages as string[] ?? messages.ToArray());

            ValidateErrors(_errors);
            ValidateMessages(_messages);

            IsSuccess = Status.Code >= 200 && Status.Code < 300 && _errors.Length == 0;
            IsFailure = !IsSuccess;

            ErrorInfo[] errorsCopy = _errors;
            _firstError = new Lazy<string?>(() =>
            {
                ErrorInfo? errorInfo = errorsCopy.FirstOrDefault();
                return errorInfo?.Message ?? errorInfo?.Code ?? errorInfo?.Metadata?.ToString();
            });
        }

        /// <summary>
        /// Executes a specified action if the result is successful.
        /// </summary>
        /// <param name="onSuccess">The action to execute.</param>
        /// <returns>The current <see cref="IResult"/> instance.</returns>
        public IResult OnSuccess(Action onSuccess)
        {
            ArgumentNullException.ThrowIfNull(onSuccess);
            if (IsSuccess) onSuccess();
            return this;
        }

        /// <summary>
        /// Executes a specified action if the result is a failure.
        /// </summary>
        /// <param name="onFailure">The action to execute, receiving a list of <see cref="ErrorInfo"/>.</param>
        /// <returns>The current <see cref="IResult"/> instance.</returns>
        public IResult OnFailure(Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onFailure);
            if (IsFailure) onFailure(Errors);
            return this;
        }

        /// <summary>
        /// Executes one of two functions based on whether the result is a success or a failure, and returns a new value.
        /// </summary>
        /// <typeparam name="U">The type of the value returned by the functions.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is successful.</param>
        /// <param name="onFailure">The function to execute if the result is a failure, receiving a list of <see cref="ErrorInfo"/>.</param>
        /// <returns>The result of the executed function.</returns>
        public U Match<U>(Func<U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess);
            ArgumentNullException.ThrowIfNull(onFailure);
            return IsSuccess ? onSuccess() : onFailure(Errors);
        }

        /// <summary>
        /// Executes an action regardless of whether the result is a success or a failure.
        /// </summary>
        /// <param name="onCompletion">The action to execute.</param>
        /// <returns>The current <see cref="IResult"/> instance.</returns>
        public IResult Finally(Action onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion);
            onCompletion();
            return this;
        }

        /// <summary>
        /// Executes a function regardless of whether the result is a success or a failure, and returns a new value.
        /// </summary>
        /// <typeparam name="U">The type of the value returned by the function.</typeparam>
        /// <param name="onCompletion">The function to execute, receiving the current result.</param>
        /// <returns>The result of the executed function.</returns>
        public U Finally<U>(Func<IResult, U> onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion);
            return onCompletion(this);
        }

        /// <summary>
        /// Transforms the errors of a failed result using a specified error mapper function.
        /// </summary>
        /// <param name="errorMapper">A function that transforms the current list of <see cref="ErrorInfo"/> into a new list of <see cref="ErrorInfo"/>.</param>
        /// <returns>A new <see cref="IResult"/> with transformed errors if the current result is a failure; otherwise, the current successful result.</returns>
        public IResult MapError(Func<IReadOnlyList<ErrorInfo>, IReadOnlyList<ErrorInfo>> errorMapper)
        {
            ArgumentNullException.ThrowIfNull(errorMapper);
            return IsSuccess ? this : MapErrorInternal(errorMapper);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IsSuccess
                ? $"Result: Success ({Status}){(Messages.Any() ? $" | Messages: {string.Join("; ", Messages)}" : "")}"
                : $"Result: Failure ({Status}) | Error: {ErrorMessage ?? "No specific error message."} | All Errors: {string.Join("; ", Errors.Select(e => e.ToString()))}";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as ResultBase);

        /// <inheritdoc />
        public virtual bool Equals(ResultBase? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Status.Equals(other.Status) &&
                IsSuccess == other.IsSuccess &&
                _errors.SequenceEqual(other._errors) &&
                _messages.SequenceEqual(other._messages);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Status);
            hash.Add(IsSuccess);
            foreach (var error in _errors) hash.Add(error);
            foreach (var message in _messages) hash.Add(message);
            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(ResultBase? left, ResultBase? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator !=(ResultBase? left, ResultBase? right) => !(left == right);

        /// <summary>
        /// Validates the collection of errors. Derived classes can override this method to add specific validation rules.
        /// Throws an <see cref="ArgumentException"/> if validation fails.
        /// </summary>
        /// <param name="errors">The collection of <see cref="ErrorInfo"/> to validate.</param>
        /// <exception cref="ArgumentException">Thrown if the errors collection contains null elements or elements that are not sufficiently defined.</exception>
        protected virtual void ValidateErrors(IEnumerable<ErrorInfo> errors)
        {
            if (errors.Any(e => e == null))
            {
                throw new ArgumentException("The error collection cannot contain null ErrorInfo objects.", nameof(errors));
            }

            // An ErrorInfo should have at least a message or a code to be meaningful.
            if (errors.Any(e => string.IsNullOrWhiteSpace(e.Message) && string.IsNullOrWhiteSpace(e.Code)))
            {
                throw new ArgumentException("Each ErrorInfo must have at least a message or a code.", nameof(errors));
            }
        }

        /// <summary>
        /// Validates the collection of messages. Derived classes can override this method to add specific validation rules.
        /// Throws an <see cref="ArgumentException"/> if validation fails.
        /// </summary>
        /// <param name="messages">The collection of messages (strings) to validate.</param>
        /// <exception cref="ArgumentException">Thrown if the messages collection contains null or empty/whitespace strings.</exception>
        protected virtual void ValidateMessages(IEnumerable<string> messages)
        {
            if (messages.Any(m => string.IsNullOrWhiteSpace(m)))
            {
                throw new ArgumentException("The message collection cannot contain null or empty/whitespace strings.", nameof(messages));
            }
        }

        /// <summary>
        /// Attempts to create a new failure result representing the first error in the current result.
        /// </summary>
        /// <returns>A new <see cref="IResult"/> representing the first error, or a generic failure if no errors are present.</returns>
        protected abstract IResult GetFirstErrorResult();

        /// <summary>
        /// Maps the errors of a failure result using a provided mapper function.
        /// </summary>
        /// <param name="errorMapper">A function to transform the current error collection.</param>
        /// <returns>A new <see cref="IResult"/> with the transformed errors if the current result is a failure; otherwise, the current result.</returns>
        protected abstract IResult MapErrorInternal(Func<IReadOnlyList<ErrorInfo>, IReadOnlyList<ErrorInfo>> errorMapper);
    }
}
