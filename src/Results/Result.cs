// <copyright file="Result.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Linq;
using System.Collections.Generic;

using Zentient.Utilities;
using Zentient.Results.Serialization;
using Zentient.Results.Constants;

#if NET8_0_OR_GREATER
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
#endif

using System.Collections.Generic;
using System.Linq;

namespace Zentient.Results
{
    /// <summary>Represents the outcome of a non-generic operation. It can be a success with optional messages, or a failure with errors.</summary>
    /// <remarks>This class is immutable after creation, with properties initialized via `init` setters.</remarks>
    [DataContract] // Retain for potential WCF/legacy interop, but System.Text.Json is primary
    [JsonConverter(typeof(Serialization.ResultJsonConverter))]
    public class Result : IResult, IEquatable<Result>
    {
        [DataMember(Order = 1)]
        [JsonInclude]
        private readonly ErrorInfo[] _errors;

        [DataMember(Order = 2)]
        [JsonInclude]
        private readonly string[] _messages;

        /// <inheritdoc />
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("isFailure")]
        public bool IsFailure { get; init; }

        /// <inheritdoc />
        [JsonPropertyName("errors")]
        public IReadOnlyList<ErrorInfo> Errors => _errors;

        /// <inheritdoc />
        [JsonPropertyName("messages")]
        public IReadOnlyList<string> Messages => _messages;

        [JsonIgnore] // Value is lazily computed
        private readonly Lazy<string?> _firstError;

        /// <inheritdoc />
        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage => _firstError.Value;

        /// <inheritdoc />
        [DataMember(Order = 3)]
        [JsonPropertyName("status")]
        public IResultStatus Status { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// </summary>
        /// <param name="status">The status of the result.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <param name="errors">Optional error information.</param>
        [JsonConstructor]
        internal Result(IResultStatus status, IEnumerable<string>? messages = null, IEnumerable<ErrorInfo>? errors = null)
        {
            Status = status ?? throw new ArgumentNullException(nameof(status));
            _errors = ((errors == null) ? Array.Empty<ErrorInfo>() : ((errors as ErrorInfo[]) ?? errors.ToArray()));
            _messages = ((messages == null) ? Array.Empty<string>() : ((messages as string[]) ?? messages.ToArray()));
            IsSuccess = Status.Code >= 200 && Status.Code < 300 && _errors.Length == 0;
            IsFailure = !IsSuccess;

            ErrorInfo[] errorsCopy = _errors;
            _firstError = new Lazy<string?>(() =>
            {
                ErrorInfo? errorInfo = errorsCopy.FirstOrDefault();
                return errorInfo?.Message ?? errorInfo?.Code ?? errorInfo?.Metadata?.ToString() ?? null;
            });
        }

        /// <summary>Creates a successful non-generic result.</summary>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Success"/>.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult"/>.</returns>
        public static IResult Success(IResultStatus? status = null, IEnumerable<string>? messages = null) =>
            new Result(status ?? ResultStatuses.Success, messages);

        /// <summary>Creates a successful non-generic result with a single message.</summary>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult"/>.</returns>
        public static IResult Success(string? message) =>
            Success(messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful non-generic result with a "Created" status.</summary>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Created"/>.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult"/> with a "Created" status.</returns>
        public static IResult Created(IResultStatus? status = null, IEnumerable<string>? messages = null) =>
            new Result(status ?? ResultStatuses.Created, messages);

        /// <summary>Creates a successful non-generic result with a "Created" status and a single message.</summary>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult"/> with a "Created" status.</returns>
        public static IResult Created(string? message) =>
            Created(messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful non-generic result with an "Accepted" status.</summary>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Accepted"/>.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult"/> with an "Accepted" status.</returns>
        public static IResult Accepted(IResultStatus? status = null, IEnumerable<string>? messages = null) =>
            new Result(status ?? ResultStatuses.Accepted, messages);

        /// <summary>Creates a successful non-generic result with an "Accepted" status and a single message.</summary>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult"/> with an "Accepted" status.</returns>
        public static IResult Accepted(string? message) =>
            Accepted(messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful non-generic result with a "No Content" status.</summary>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.NoContent"/>.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <returns>A successful <see cref="IResult"/> with a "No Content" status.</returns>
        public static IResult NoContent(IResultStatus? status = null, IEnumerable<string>? messages = null) =>
            new Result(status ?? ResultStatuses.NoContent, messages);

        /// <summary>Creates a successful non-generic result with a "No Content" status and a single message.</summary>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult"/> with a "No Content" status.</returns>
        public static IResult NoContent(string? message) =>
            NoContent(messages: !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a non-generic failure result from a single error.</summary>
        /// <param name="error">The error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <returns>A failed <see cref="IResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is null.</exception>
        public static IResult Failure(ErrorInfo error, IResultStatus? status = null)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));

            var messages = string.IsNullOrWhiteSpace(error.Message) ? null : new string[1] { error.Message };

            return new Result(status ?? ResultStatuses.BadRequest, messages, new ErrorInfo[1] { error });
        }

        /// <summary>Creates a non-generic failure result from a collection of errors.</summary>
        /// <param name="errors">A collection of error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <returns>A failed <see cref="IResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static IResult Failure(IEnumerable<ErrorInfo> errors, IResultStatus? status = null)
        {
            ErrorInfo[] array = (errors as ErrorInfo[]) ?? errors?.ToArray() ?? throw new ArgumentNullException(nameof(errors));

            if (array.Length == 0)
            {
                throw new ArgumentException("Error messages cannot be null or empty.", nameof(errors));
            }

            var errorMessages = array
                .Where(e => !string.IsNullOrWhiteSpace(e.Message))
                .Select(e => e.Message)
                .ToList();

            return new Result(
                status ?? ResultStatuses.BadRequest,
                errorMessages.Any() ? errorMessages : null,
                array);
        }

        /// <summary>Creates a non-generic failure result representing validation errors.</summary>
        /// <param name="errors">A collection of validation errors.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.UnprocessableEntity"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static IResult Validation(IEnumerable<ErrorInfo> errors)
        {
            var errorMessages = errors.Where(e => !string.IsNullOrWhiteSpace(e.Message)).Select(e => e.Message).ToList();
            return new Result(ResultStatuses.UnprocessableEntity, errorMessages.Any() ? errorMessages : null, (errors as ErrorInfo[]) ?? errors.ToArray());
        }

        /// <summary>Creates a non-generic failure result for "Not Found" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.NotFound"/>.</returns>
        public static IResult NotFound(string message = ResultStatusConstants.Description.NotFound, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.NotFound;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.NotFound.ToString();
            }

            return new Result(ResultStatuses.NotFound, new[] { message }, new ErrorInfo[1] { ErrorInfo.NotFound(message, code) });
        }

        /// <summary>Creates a non-generic failure result for "Unauthorized" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.Unauthorized"/>.</returns>
        public static IResult Unauthorized(string message = ResultStatusConstants.Description.Unauthorized, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.Unauthorized;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.Unauthorized.ToString();
            }

            return new Result(ResultStatuses.Unauthorized, new[] { message }, new ErrorInfo[1] { ErrorInfo.Unauthorized(message, code) });
        }

        /// <summary>Creates a non-generic failure result for "Forbidden" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.Forbidden"/>.</returns>
        public static IResult Forbidden(string message = ResultStatusConstants.Description.Forbidden, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.Forbidden;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.Forbidden.ToString();
            }

            return new Result(ResultStatuses.Forbidden, new[] { message }, new ErrorInfo[1] { ErrorInfo.Forbidden(message, code) });
        }

        /// <summary>Creates a non-generic failure result for "Conflict" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.Conflict"/>.</returns>
        public static IResult Conflict(string message = ResultStatusConstants.Description.Conflict, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.Conflict;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.Conflict.ToString();
            }

            return new Result(ResultStatuses.Conflict, new[] { message }, new ErrorInfo[1] { ErrorInfo.Conflict(message, code) });
        }

        /// <summary>Creates a non-generic failure result for "Request Timeout" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.RequestTimeout"/>.</returns>
        public static IResult RequestTimeout(string message = ResultStatusConstants.Description.RequestTimeout, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.RequestTimeout;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.RequestTimeout.ToString();
            }

            return new Result(ResultStatuses.RequestTimeout, new[] { message }, new ErrorInfo[1] { new ErrorInfo(ErrorCategory.Timeout, code ?? "RequestTimeout", message) });
        }

        /// <summary>Creates a non-generic failure result for "Gone" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.Gone"/>.</returns>
        public static IResult Gone(string message = ResultStatusConstants.Description.Gone, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.Gone;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.Gone.ToString();
            }

            return new Result(ResultStatuses.Gone, new[] { message }, new ErrorInfo[1] { new ErrorInfo(ErrorCategory.ResourceGone, code, message) });
        }

        /// <summary>Creates a non-generic failure result for "Precondition Failed" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.PreconditionFailed"/>.</returns>
        public static IResult PreconditionFailed(string message = ResultStatusConstants.Description.PreconditionFailed, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.PreconditionFailed;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.PreconditionFailed.ToString();
            }

            return new Result(ResultStatuses.PreconditionFailed, new[] { message }, new ErrorInfo[1] { new ErrorInfo(ErrorCategory.Validation, code, message) });
        }

        /// <summary>Creates a non-generic failure result for "Too Many Requests" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.TooManyRequests"/>.</returns>
        public static IResult TooManyRequests(string message = ResultStatusConstants.Description.TooManyRequests, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.TooManyRequests;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.TooManyRequests.ToString();
            }

            return new Result(ResultStatuses.TooManyRequests, new[] { message }, new ErrorInfo[1] { new ErrorInfo(ErrorCategory.RateLimit, code, message) });
        }

        /// <summary>Creates a non-generic failure result for "Not Implemented" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.NotImplemented"/>.</returns>
        public static IResult NotImplemented(string message = ResultStatusConstants.Description.NotImplemented, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.NotImplemented;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.NotImplemented.ToString();
            }

            return new Result(ResultStatuses.NotImplemented, new[] { message }, new ErrorInfo[1] { new ErrorInfo(ErrorCategory.NotImplemented, code, message) });
        }

        /// <summary>Creates a non-generic failure result for "Service Unavailable" scenarios.</summary>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="code">Optional error code.</param>
        /// <returns>A failed <see cref="IResult"/> with status <see cref="ResultStatuses.ServiceUnavailable"/>.</returns>
        public static IResult ServiceUnavailable(string message = ResultStatusConstants.Description.ServiceUnavailable, string? code = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResultStatusConstants.Description.ServiceUnavailable;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                code = ResultStatusConstants.Code.ServiceUnavailable.ToString();
            }

            return new Result(ResultStatuses.ServiceUnavailable, new[] { message }, new ErrorInfo[1] { new ErrorInfo(ErrorCategory.ServiceUnavailable, code, message) });
        }

        /// <summary>Creates a non-generic failure result from an exception.</summary>
        /// <param name="ex">The exception to convert into an error.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Error"/>.</param>
        /// <returns>A failed <see cref="IResult"/>.</returns>
        /// <remarks>
        /// This method leverages <see cref="ErrorInfo.FromException"/> to create the error details.
        /// The exception's message will be used as the primary message of the ErrorInfo.
        /// </remarks>
        public static IResult FromException(Exception ex, IResultStatus? status = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            return new Result(status ?? ResultStatuses.Error, new[] { ex.Message }, new ErrorInfo[1] { ErrorInfo.FromException(ex, ex.Message) });
        }

        /// <inheritdoc />
        public IResult GetFirstError() =>
            Errors.Any() ? Failure(Errors.First(), Status) : Failure(new ErrorInfo("NoErrors", "No errors found for a failure result."), Status);

        /// <summary>Executes a specified action if the result is successful.</summary>
        /// <param name="onSuccess">The action to execute.</param>
        /// <returns>The current <see cref="IResult"/> instance.</returns>
        public IResult OnSuccess(Action onSuccess)
        {
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
            if (IsSuccess) onSuccess();
            return this;
        }

        /// <summary>Executes a specified action if the result is a failure.</summary>
        /// <param name="onFailure">The action to execute, receiving a list of <see cref="ErrorInfo"/>.</param>
        /// <returns>The current <see cref="IResult"/> instance.</returns>
        public IResult OnFailure(Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));
            if (IsFailure) onFailure(Errors);
            return this;
        }

        /// <summary>Executes one of two functions based on whether the result is a success or a failure, and returns a new value.</summary>
        /// <typeparam name="U">The type of the value returned by the functions.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is successful.</param>
        /// <param name="onFailure">The function to execute if the result is a failure, receiving a list of <see cref="ErrorInfo"/>.</param>
        /// <returns>The result of the executed function.</returns>
        public U Match<U>(Func<U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure)
        {
            ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
            ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));
            return IsSuccess ? onSuccess() : onFailure(Errors);
        }

        /// <summary>Executes an action regardless of whether the result is a success or a failure.</summary>
        /// <param name="onCompletion">The action to execute.</param>
        /// <returns>The current <see cref="IResult"/> instance.</returns>
        public IResult Finally(Action onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion, nameof(onCompletion));
            onCompletion();
            return this;
        }

        /// <summary>Executes a function regardless of whether the result is a success or a failure, and returns a new value.</summary>
        /// <typeparam name="U">The type of the value returned by the function.</typeparam>
        /// <param name="onCompletion">The function to execute.</param>
        /// <returns>The result of the executed function.</returns>
        public U Finally<U>(Func<U> onCompletion)
        {
            ArgumentNullException.ThrowIfNull(onCompletion, nameof(onCompletion));
            return onCompletion();
        }

        /// <summary>Transforms the errors of a failed result using a specified error mapper function.</summary>
        /// <param name="errorMapper">A function that transforms the current list of <see cref="ErrorInfo"/> into a new list of <see cref="ErrorInfo"/>.</param>
        /// <returns>A new <see cref="IResult"/> with transformed errors if the current result is a failure; otherwise, the current successful result.</returns>
        public IResult MapError(Func<IReadOnlyList<ErrorInfo>, IReadOnlyList<ErrorInfo>> errorMapper)
        {
            ArgumentNullException.ThrowIfNull(errorMapper, nameof(errorMapper));
            return IsSuccess ? this : Failure(errorMapper(Errors), Status);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IsSuccess
                ? $"Result: Success ({Status}){(Messages.Any() ? $" | Messages: {string.Join("; ", Messages)}" : "")}"
                : $"Result: Failure ({Status}) | Error: {ErrorMessage ?? "No specific error message."} | All Errors: {string.Join("; ", Errors.Select(e => e.ToString()))}";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Result);

        /// <inheritdoc />
        public bool Equals(Result? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

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

            foreach (var error in _errors)
            {
                hash.Add(error);
            }

            foreach (var message in _messages)
            {
                hash.Add(message);
            }

            return hash.ToHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(Result? left, Result? right)
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
        public static bool operator !=(Result? left, Result? right) => !(left == right);

        /// <summary>Allows implicit conversion of a <see cref="Result"/> to a <see cref="bool"/>, indicating success.</summary>
        /// <param name="result">The <see cref="Result"/> instance.</param>
        /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
        public static implicit operator bool(Result result) => result.IsSuccess;
    }
}
