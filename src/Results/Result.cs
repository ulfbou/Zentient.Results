// <copyright file="Result.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Zentient.Results
{
    /// <summary>
    /// Represents the immutable outcome of a non-generic operation. It can be a success with optional messages, or a failure with errors.
    /// </summary>
    /// <remarks>This class is immutable after creation, with properties initialized via `init` setters in `ResultBase`.</remarks>
    [DataContract]
    [JsonConverter(typeof(Serialization.ResultJsonConverter))]
    public class Result : ResultBase, IResult, IEquatable<Result>
    {
        // REMOVED: The old JsonConstructor with IResultStatus parameter.
        // The base constructor in ResultBase is now expected to handle the core logic without status.

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// This constructor is primarily for internal use and deserialization.
        /// </summary>
        /// <param name="messages">Optional informational messages.</param>
        /// <param name="errors">Optional error information.</param>
        [JsonConstructor]
        internal Result(IEnumerable<string>? messages = null, IEnumerable<ErrorInfo>? errors = null)
            : base(messages, errors)
        {
            // The base constructor now sets IsSuccess/IsFailure based on 'errors' presence,
            // eliminating the need for an explicit 'status' parameter at this level.
        }

        /// <summary>
        /// Creates a **successful** non-generic result.
        /// </summary>
        /// <returns>A successful <see cref="IResult"/> with no messages.</returns>
        public static IResult Success()
            => new Result(messages: null);

        /// <summary>
        /// Creates a **successful** non-generic result with a single message.
        /// </summary>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult"/>.</returns>
        public static IResult Success(string? message)
            => Success(messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>
        /// Creates a **successful** non-generic result with multiple messages.
        /// </summary>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="messages"/> is null but not empty.</exception>
        public static IResult Success(IEnumerable<string>? messages)
        {
            if (messages != null && !messages.Any())
            {
                throw new ArgumentException("Messages collection cannot be empty if provided.", nameof(messages));
            }

            return new Result(messages: messages);
        }

        /// <summary>
        /// Creates a **failed** non-generic result from a single error.
        /// </summary>
        /// <param name="error">The error information. Must not be null.</param>
        /// <returns>A failed <see cref="IResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is null.</exception>
        public static IResult Failure(ErrorInfo error)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));

            var messages = !string.IsNullOrWhiteSpace(error.Message) ? new[] { error.Message! } : null;
            return new Result(messages, new[] { error });
        }

        /// <summary>
        /// Creates a **failed** non-generic result from a collection of errors.
        /// </summary>
        /// <param name="errors">A collection of error information. Must not be null or empty.</param>
        /// <returns>A failed <see cref="IResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static IResult Failure(IEnumerable<ErrorInfo> errors)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            var errorArray = errors.ToArray();

            if (!errorArray.Any())
            {
                throw new ArgumentException("Errors collection cannot be empty.", nameof(errors));
            }

            List<string> errorMessages = errorArray
                .Where(e => !string.IsNullOrWhiteSpace(e.Message))
                .Select(e => e.Message!)
                .Distinct()
                .ToList();

            return new Result(
            errorMessages.Any() ? errorMessages : null,
            errorArray);
        }

        /// <summary>
        /// Creates a **failed** non-generic result representing validation errors.
        /// This is a convenience factory that uses <see cref="ErrorInfo.Validation(string?, string?, IReadOnlyDictionary{string, object?}?, IEnumerable{ErrorInfo}?)"/>
        /// internally for individual errors if they are not already Validation errors.
        /// </summary>
        /// <param name="errors">A collection of error information, typically with <see cref="ErrorCategory.Validation"/>.</param>
        /// <returns>A failed <see cref="IResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static IResult Validation(IEnumerable<ErrorInfo> errors)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            var errorArray = errors.ToArray();

            if (!errorArray.Any())
            {
                throw new ArgumentException("Validation errors collection cannot be empty.", nameof(errors));
            }

            var validationErrors = errorArray.Select(
                e => e.Category == ErrorCategory.Validation ? e : ErrorInfo.Validation(code: e.Code, message: e.Message, metadata: e.Metadata, innerErrors: e.InnerErrors)).ToArray();

            List<string> errorMessages = validationErrors
                .Where(e => !string.IsNullOrWhiteSpace(e.Message))
                .Select(e => e.Message!)
                .Distinct()
                .ToList();

            return new Result(
            errorMessages.Any() ? errorMessages : null,
            validationErrors);
        }

        /// <summary>
        /// Creates a **failed** non-generic result from an exception.
        /// This method delegates error information extraction to <see cref="ErrorInfo.FromException(Exception, string?, string?, IReadOnlyDictionary{string, object?}?, IEnumerable{ErrorInfo}?)"/>.
        /// </summary>
        /// <param name="ex">The exception to convert into an error. Must not be null.</param>
        /// <returns>A failed <see cref="IResult"/> with the exception details.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
        public static IResult FromException(Exception ex)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            var errorInfo = ErrorInfo.FromException(ex);
            // The message from the exception's error info should populate the messages list
            var messages = !string.IsNullOrWhiteSpace(errorInfo.Message) ? new[] { errorInfo.Message! } : null;
            return new Result(messages, new[] { errorInfo });
        }

        /// <inheritdoc />
        protected override IResult GetFirstErrorResult()
            => Errors.Any() ? Failure(Errors[0]) : Failure(ErrorInfo.General("NoErrorsFound", "No errors were found when a failure result was expected."));

        /// <inheritdoc />
        protected override IResult MapErrorInternal(Func<IReadOnlyList<ErrorInfo>, IReadOnlyList<ErrorInfo>> errorMapper)
            => Failure(errorMapper(Errors));

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => Equals(obj as Result);

        /// <inheritdoc />
        public bool Equals(Result? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return IsSuccess == other.IsSuccess
            && Errors.SequenceEqual(other.Errors)
            && Messages.SequenceEqual(other.Messages);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(IsSuccess);
            foreach (var error in Errors) hash.Add(error);
            foreach (var message in Messages) hash.Add(message);
            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(Result? left, Result? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator !=(Result? left, Result? right)
            => !(left == right);

        /// <summary>
        /// Allows implicit conversion of a <see cref="Result"/> to a <see cref="bool"/>, indicating success.
        /// </summary>
        /// <param name="result">The <see cref="Result"/> instance.</param>
        /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
        public static implicit operator bool(Result result)
            => result.IsSuccess;
    }
}
