﻿using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Linq;
using System.Collections.Generic;

using Zentient.Utilities;

#if NET8_0_OR_GREATER
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
#endif

namespace Zentient.Results
{
    /// <summary>
    /// Represents the outcome of an operation without a return value.
    /// It can be a success with optional messages or a failure with errors.
    /// Provides static factory methods for creating common <see cref="ErrorResult"/> instances.
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(ResultJsonConverter))]
    public readonly struct Result : IResult
    {
        [DataMember(Order = 2)]
        [JsonIgnore]
        private readonly ErrorInfo[] _errors;

        [DataMember(Order = 3)]
        [JsonIgnore]
        private readonly string[] _messages;

        /// <inheritdoc />
        [JsonIgnore]
        public bool IsSuccess =>
            (Status.Code >= 200 && Status.Code < 300) && _errors.Length == 0;

        /// <inheritdoc />
        [JsonIgnore]
        public bool IsFailure => !IsSuccess;

        /// <inheritdoc />
        [JsonIgnore]
        public IReadOnlyList<ErrorInfo> Errors => _errors;

        /// <inheritdoc />
        [JsonPropertyName("messages")]
        public IReadOnlyList<string> Messages => _messages;

        [JsonIgnore]
        private readonly Lazy<string?> _firstError;

        /// <inheritdoc />
        [JsonPropertyName("error")]
        public string? Error => _firstError.Value;

        /// <inheritdoc />
        [DataMember(Order = 4)]
        [JsonPropertyName("status")]
        public IResultStatus Status { get; }

        /// <summary>Initializes a new instance of the <see cref="Result"/> struct.</summary>
        /// <param name="status">The status of the result.</param>
        /// <param name="messages">Optional informational messages.</param>
        /// <param name="errors">Optional error information.</param>
        [JsonConstructor]
        internal Result(
            IResultStatus status,
            IEnumerable<string>? messages = null,
            IEnumerable<ErrorInfo>? errors = null)
        {
            Status = status;
            _errors = errors is null ? Array.Empty<ErrorInfo>() : errors as ErrorInfo[] ?? errors.ToArray();
            _messages = messages is null ? Array.Empty<string>() : messages as string[] ?? messages.ToArray();
            // Workaround for CS1673: capture _errors in a local variable for the lambda
            // to avoid capturing the field directly in the lambda.
            // This is a known issue with C# 7.3 and earlier.
            ErrorInfo[] errorsArray = _errors;
            _firstError = new Lazy<string?>(() => (errorsArray.Length > 0) ? errorsArray[0].Message : null);
        }

        /// <summary>Creates a successful result.</summary>
        /// <param name="message">An optional success message.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Success"/>.</param>
        /// <returns>A successful <see cref="IResult"/>.</returns>
        public static IResult Success(IResultStatus? status = null, string? message = null) =>
            new Result(status ?? ResultStatuses.Success, !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful result with a "Created" status.</summary>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult"/> with a "Created" status.</returns>
        public static IResult Created(string? message = null) =>
            new Result(ResultStatuses.Created, !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a successful result with a "No Content" status.</summary>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult"/> with a "No Content" status.</returns>
        public static IResult NoContent(string? message = null) =>
            new Result(ResultStatuses.NoContent, !string.IsNullOrWhiteSpace(message) ? new[] { message! } : null);

        /// <summary>Creates a failure result from a single error.</summary>
        /// <param name="error">The error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <returns>A failure <see cref="IResult"/> representing the error.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is null.</exception>
        public static IResult Failure(ErrorInfo error, IResultStatus? status = null)
        {
            Guard.AgainstDefault(error, nameof(error));
            return new Result(status ?? ResultStatuses.BadRequest, null, new[] { error });
        }

        /// <summary>Creates a failure result from a collection of errors.</summary>
        /// <param name="errors">A collection of error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        /// <returns>A failure <see cref="IResult"/> representing the errors.</returns>
        public static IResult Failure(IEnumerable<ErrorInfo> errors, IResultStatus? status = null)
        {
            var arr = errors as ErrorInfo[] ?? errors?.ToArray();
            Guard.AgainstNullOrEmpty(arr, nameof(errors));
            return new Result(status ?? ResultStatuses.BadRequest, null, arr);
        }

        /// <summary>Creates a failure result representing validation errors.</summary>
        /// <param name="errors">A collection of validation errors.</param>
        /// <returns>A failure <see cref="IResult"/> representing validation errors.</returns>
        public static IResult Validation(IEnumerable<ErrorInfo> errors) =>
            Failure(errors, ResultStatuses.UnprocessableEntity);

        /// <summary>Creates an unauthorized failure result.</summary>
        /// <param name="error">Optional specific error info. Defaults to a generic "Unauthorized" error.</param>
        /// <returns>A failure <see cref="IResult"/> representing an unauthorized error.</returns>
        public static IResult Unauthorized(ErrorInfo? error = null) =>
            Failure(error ?? new ErrorInfo(ErrorCategory.Authentication, "Unauthorized", "Authentication required."), ResultStatuses.Unauthorized);

        /// <summary>Creates a forbidden failure result.</summary>
        /// <param name="error">Optional specific error info. Defaults to a generic "Forbidden" error.</param>
        /// <returns>A failure <see cref="IResult"/> representing a forbidden error.</returns>
        public static IResult Forbidden(ErrorInfo? error = null) =>
            Failure(error ?? new ErrorInfo(ErrorCategory.Authorization, "Forbidden", "You do not have permission to perform this action."), ResultStatuses.Forbidden);

        /// <summary>Creates a not found failure result.</summary>
        /// <param name="error">Optional specific error info. Defaults to a generic "NotFound" error.</param>
        /// <returns>A failure <see cref="IResult"/> representing a not found error.</returns>
        public static IResult NotFound(ErrorInfo? error = null) =>
            Failure(error ?? new ErrorInfo(ErrorCategory.NotFound, "NotFound", "The requested resource was not found."), ResultStatuses.NotFound);

        /// <summary>Creates a conflict failure result.</summary>
        /// <param name="error">Optional specific error info. Defaults to a generic "Conflict" error.</param>
        /// <returns>A failure <see cref="IResult"/> representing a conflict.</returns>
        public static IResult Conflict(ErrorInfo? error = null) =>
            Failure(error ?? new ErrorInfo(ErrorCategory.Conflict, "Conflict", "A conflict occurred with the current state of the resource."), ResultStatuses.Conflict);

        /// <summary>Creates an internal server error result.</summary>
        /// <param name="error">Optional specific error info. Defaults to a generic "An error occurred" error.</param>
        /// <returns>A failure <see cref="IResult"/> representing an internal error.</returns>
        public static IResult InternalError(ErrorInfo? error = null) =>
            Failure(error ?? new ErrorInfo(ErrorCategory.General, "InternalError", "An unexpected error occurred."), ResultStatuses.Error);

        /// <summary>
        /// Creates a failure result from an exception.
        /// </summary>
        /// <param name="ex">The exception to convert into an error.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Error"/>.</param>
        /// <returns>A failure <see cref="IResult"/> representing the exception.</returns>
        public static IResult FromException(Exception ex, IResultStatus? status = null)
            => Failure(new ErrorInfo(ErrorCategory.Exception, ex.GetType().Name, ex.Message, data: ex), status ?? ResultStatuses.Error);

        /// <summary>
        /// Allows implicit conversion from an <see cref="ErrorInfo"/> to a failure <see cref="Result"/>.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <returns>A <see cref="Result"/> representing a failure with the specified error.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is null.</exception>
        public static implicit operator Result(ErrorInfo error)
        {
            Guard.AgainstDefault(error, nameof(error));
            return new Result(ResultStatuses.BadRequest, null, new[] { error });
        }

        /// <summary>Creates a successful generic result.</summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/>.</returns>
        public static IResult<TValue> Success<TValue>(TValue value, string? message = null)
            => Result<TValue>.Success(value, message);

        /// <summary>Creates a successful generic result with a "No Content" status.</summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with a "No Content" status.</returns>
        public static IResult<TValue> Ok<TValue>(TValue value, string? message = null)
            => Result<TValue>.Success(value, message ?? string.Empty);

        /// <summary>Creates a successful generic result with a "Created" status.</summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with a "Created" status.</returns>
        public static IResult<TValue> Created<TValue>(TValue value, string? message = null)
            => Result<TValue>.Created(value, message);

        /// <summary>Creates a successful generic result with a "No Content" status.</summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="message">An optional success message.</param>
        /// <returns>A successful <see cref="IResult{TValue}"/> with a "No Content" status.</returns>
        public static IResult<TValue> NoContent<TValue>(string? message = null)
            => Result<TValue>.NoContent(message);

        /// <summary>Creates a generic failure result from a single error.</summary>
        /// <typeparam name="TValue">The type of the value (will be default).</typeparam>
        /// <param name="error">The error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <returns>A failure <see cref="IResult{TValue}"/> representing the error.</returns>
        public static IResult<TValue> Failure<TValue>(ErrorInfo error, IResultStatus? status = null)
            => Result<TValue>.Failure(default, error, status ?? ResultStatuses.BadRequest);

        /// <summary>Creates a generic failure result from a collection of errors.</summary>
        /// <typeparam name="TValue">The type of the value (will be default).</typeparam>
        /// <param name="errors">A collection of error information.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.BadRequest"/>.</param>
        /// <returns>A failure <see cref="IResult{TValue}"/> representing the errors.</returns>
        public static IResult<TValue> Failure<TValue>(IEnumerable<ErrorInfo> errors, IResultStatus? status = null)
            => Result<TValue>.Failure(default, errors, status ?? ResultStatuses.BadRequest);

        /// <summary>Creates a generic failure result representing validation errors.</summary>
        /// <typeparam name="TValue">The type of the value (will be default).</typeparam>
        /// <param name="errors">A collection of validation errors.</param>
        /// <returns>A failure <see cref="IResult{TValue}"/> representing validation errors.</returns>
        public static IResult<TValue> Validation<TValue>(IEnumerable<ErrorInfo> errors)
            => Result<TValue>.Validation(errors);

        /// <summary>Creates an unauthorized generic failure result.</summary>
        /// <typeparam name="TValue">The type of the value (will be default).</typeparam>
        /// <param name="error">Optional specific error info.</param>
        /// <returns>A failure <see cref="IResult{TValue}"/> representing an unauthorized error.</returns>
        public static IResult<TValue> Unauthorized<TValue>(ErrorInfo? error = null)
            => Result<TValue>.Failure(default, error ?? new ErrorInfo(ErrorCategory.Authentication, "Unauthorized", "Authentication required."), ResultStatuses.Unauthorized);

        /// <summary>Creates a forbidden generic failure result.</summary>
        /// <typeparam name="TValue">The type of the value (will be default).</typeparam>
        /// <param name="error">Optional specific error info.</param>
        /// <returns>A failure <see cref="IResult{TValue}"/> representing a forbidden error.</returns>
        public static IResult<TValue> Forbidden<TValue>(ErrorInfo? error = null)
            => Result<TValue>.Failure(default, error ?? new ErrorInfo(ErrorCategory.Authorization, "Forbidden", "You do not have permission to perform this action."), ResultStatuses.Forbidden);

        /// <summary>Creates a not found generic failure result.</summary>
        /// <typeparam name="TValue">The type of the value (will be default).</typeparam>
        /// <param name="error">Optional specific error info.</param>
        /// <returns>A failure <see cref="IResult{TValue}"/> representing a not found error.</returns>
        public static IResult<TValue> NotFound<TValue>(ErrorInfo? error = null)
            => Result<TValue>.Failure(default, error ?? new ErrorInfo(ErrorCategory.NotFound, "NotFound", "The requested resource was not found."), ResultStatuses.NotFound);

        /// <summary>Creates a conflict generic failure result.</summary>
        /// <typeparam name="TValue">The type of the value (will be default).</typeparam>
        /// <param name="error">Optional specific error info.</param>
        /// <returns>A failure <see cref="IResult{TValue}"/> representing a conflict.</returns>
        public static IResult<TValue> Conflict<TValue>(ErrorInfo? error = null)
            => Result<TValue>.Failure(default, error ?? new ErrorInfo(ErrorCategory.Conflict, "Conflict", "A conflict occurred with the current state of the resource."), ResultStatuses.Conflict);

        /// <summary>Creates an internal server error generic result.</summary>
        /// <typeparam name="TValue">The type of the value (will be default).</typeparam>
        /// <param name="error">Optional specific error info.</param>
        /// <returns>A failure <see cref="IResult{TValue}"/> representing an internal error.</returns>
        public static IResult<TValue> InternalError<TValue>(ErrorInfo? error = null)
            => Result<TValue>.Failure(default, error ?? new ErrorInfo(ErrorCategory.General, "InternalError", "An unexpected error occurred."), ResultStatuses.Error);

        /// <summary>
        /// Creates a generic failure result from an exception.
        /// </summary>
        /// <typeparam name="TValue">The type of the value (will be default).</typeparam>
        /// <param name="ex">The exception to convert into an error.</param>
        /// <param name="status">Optional custom status. Defaults to <see cref="ResultStatuses.Error"/>.</param>
        /// <returns>A failure <see cref="IResult{TValue}"/> representing the exception.</returns>
        public static IResult<T> FromException<T>(Exception ex, IResultStatus? status = null)
            => Result<T>.Failure(default, new ErrorInfo(ErrorCategory.Exception, ex.GetType().Name, ex.Message, data: ex), status ?? ResultStatuses.Error);

#if PROBLEM_DETAILS
        /// <summary>
        /// Creates a failure <see cref="IResult"/> from a <see cref="ProblemDetails"/> instance.
        /// The resulting <see cref="IResult"/>'s status will dynamically reflect the
        /// <see cref="ProblemDetails.Status"/> property.
        /// </summary>
        /// <param name="problemDetails">The <see cref="ProblemDetails"/> object containing details about the problem.</param>
        /// <returns>
        /// An <see cref="IResult"/> representing a problem details failure, with an <see cref="ErrorInfo"/>
        /// constructed from the <paramref name="problemDetails"/>.
        /// </returns>
        /// <remarks>
        /// This method aims to preserve as much information from the <see cref="ProblemDetails"/>
        /// as possible within the <see cref="IResult"/> abstraction.
        /// </remarks>
        internal static IResult Problem(ProblemDetails problemDetails)
        {
            ArgumentNullException.ThrowIfNull(problemDetails);

            int statusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
            IResultStatus resultStatus = ResultStatus.FromHttpStatusCode(statusCode);

            string errorCode = string.IsNullOrEmpty(problemDetails.Type)
                ? statusCode.ToString()
                : problemDetails.Type.Split('/').LastOrDefault()?.Replace("-", "") ?? problemDetails.Type;

            string errorMessage = problemDetails.Detail ?? problemDetails.Title ?? "An unknown problem occurred.";
            string? errorDetail = problemDetails.Title;

            IDictionary<string, object?>? errorExtensions = null;
            if (problemDetails.Extensions?.Any() == true)
            {
                errorExtensions = new Dictionary<string, object?>(problemDetails.Extensions);
            }

            List<ErrorInfo>? innerErrors = null;

            if (problemDetails is ValidationProblemDetails validationProblemDetails && validationProblemDetails.Errors?.Any() == true)
            {
                innerErrors = new List<ErrorInfo>();
                foreach (var kvp in validationProblemDetails.Errors)
                {
                    foreach (var msg in kvp.Value)
                    {
                        innerErrors.Add(new ErrorInfo(
                            ErrorCategory.Validation,
                            code: kvp.Key,
                            message: msg,
                            detail: $"Validation error for field '{kvp.Key}'.",
                            data: kvp.Key
                        ));
                    }
                }
            }

            var mainErrorInfo = new ErrorInfo(
                ErrorCategory.ProblemDetails,
                code: errorCode,
                message: errorMessage,
                detail: errorDetail,
                data: null,
                extensions: errorExtensions,
                innerErrors: innerErrors
            );
            var status = resultStatus ?? ResultStatuses.Error;

            return new Result(status, null, new[] { mainErrorInfo });
        }
#endif // PROBLEM_DETAILS

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsSuccess)
            {
                return $"Result: Success ({Status}){(Messages.Count > 0 ? $" | Message: {string.Join("; ", Messages)}" : "")}";
            }

#if NET8_0_OR_GREATER
            return $"Result: Failure ({Status}) | Error: {Error} | All Errors: {string.Join("; ", Errors.Select(e => e.ToString()))}";
#else
            // For earlier frameworks, convert to array first
            return $"Result: Failure ({Status}) | Error: {Error} | All Errors: {string.Join("; ", Errors.Select(e => e.ToString()).ToArray())}";
#endif
        }
    }
}
