// <copyright file="ResultCreationHelpersExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

// Assuming Zentient.Results.Constants and Zentient.Results.Serialization are correctly referenced
// and contain ErrorCodes and MetadataKeys if used.

namespace Zentient.Results
{
    /// <summary>
    /// Provides a comprehensive set of extension methods for <see cref="IResult"/> and <see cref="IResult{T}"/>
    /// to facilitate fluent, expressive, and consistent functional-style error handling and result manipulation.
    /// These extensions leverage the static factory methods on <see cref="Result"/>, <see cref="Result{T}"/>,
    /// and the new <see cref="ErrorInfo"/> static factory methods to create transport-unaware result instances.
    /// </summary>
    public static class ResultCreationHelpersExtensions
    {
        #region Creation Helpers: AsResult, AsSuccess, AsError

        /// <summary>
        /// Converts a <see cref="bool"/> into a non-generic <see cref="IResult"/>,
        /// producing success if <paramref name="isSuccess"/> is true; otherwise failure.
        /// Optional messages and failure error details can be provided.
        /// </summary>
        /// <param name="isSuccess">Boolean flag indicating success status.</param>
        /// <param name="successMessage">Optional success message to include.</param>
        /// <param name="failureError">Optional failure <see cref="ErrorInfo"/>; defaults to a general error if omitted.</param>
        /// <returns>An <see cref="IResult"/> representing success or failure.</returns>
        public static IResult AsResult(this bool isSuccess, string? successMessage = null, ErrorInfo? failureError = null) => isSuccess
            ? Result.Success(successMessage)
            : Result.Failure(failureError ?? ErrorInfo.General("Operation failed.")); // Use new ErrorInfo.General()

        /// <summary>
        /// Converts a <see cref="bool"/> and a value into a generic <see cref="IResult{T}"/>,
        /// producing success with the value if <paramref name="isSuccess"/> is true; otherwise failure.
        /// Optional success message and failure error details can be provided.
        /// </summary>
        /// <typeparam name="T">Type of the value to carry.</typeparam>
        /// <param name="isSuccess">Boolean flag indicating success status.</param>
        /// <param name="value">Value to include if success.</param>
        /// <param name="successMessage">Optional success message to include.</param>
        /// <param name="failureError">Optional failure <see cref="ErrorInfo"/>; defaults to a general error if omitted.</param>
        /// <returns>An <see cref="IResult{T}"/> representing success or failure.</returns>
        public static IResult<T> AsResult<T>(this bool isSuccess, T value, string? successMessage = null, ErrorInfo? failureError = null) => isSuccess
            ? Result<T>.Success(value, successMessage)
            : Result<T>.Failure(value, failureError ?? ErrorInfo.General("Operation failed.")); // Use new ErrorInfo.General()

        /// <summary>
        /// Converts a string message into a successful non-generic <see cref="IResult"/>.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <returns>A successful <see cref="IResult"/> with the message.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="message"/> is null or empty.</exception>
        public static IResult AsSuccess(this string? message) // Changed parameter to nullable string
        {
            return Result.Success(message); // Now directly delegates, allowing null/empty messages
        }

        /// <summary>
        /// Converts a value into a successful generic <see cref="IResult{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The success value.</param>
        /// <param name="message">Optional success message.</param>
        /// <returns>A successful <see cref="IResult{T}"/> containing the value and optional message.</returns>
        public static IResult<T> AsSuccess<T>(this T value, string? message = null) =>
            Result<T>.Success(value, message);

        /// <summary>
        /// Converts an exception into a failed non-generic <see cref="IResult"/>.
        /// </summary>
        /// <typeparam name="TException">Type of the exception.</typeparam>
        /// <param name="ex">The exception instance.</param>
        /// <param name="message">Optional custom message for the error.</param>
        /// <param name="code">Optional custom code for the error.</param>
        /// <returns>A failed <see cref="IResult"/> encapsulating the exception as an <see cref="ErrorInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
        public static IResult AsError<TException>(this TException ex, string? message = null, string? code = null)
            where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            return Result.Failure(ErrorInfo.FromException(ex, message, code)); // Use new ErrorInfo.FromException()
        }

        /// <summary>
        /// Converts an exception into a failed generic <see cref="IResult{T}"/> with its default value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="ex">The exception instance.</param>
        /// <param name="message">Optional custom message for the error.</param>
        /// <param name="code">Optional custom code for the error.</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the exception as an <see cref="ErrorInfo"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
        public static IResult<T> AsError<T>(this Exception ex, string? message = null, string? code = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            return Result<T>.Failure(default, ErrorInfo.FromException(ex, message, code)); // Use new ErrorInfo.FromException()
        }

        /// <summary>
        /// Converts an exception and an explicit value into a failed generic <see cref="IResult{T}"/>.
        /// The provided value will be carried by the result even in a failed state.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="ex">The exception instance.</param>
        /// <param name="value">The value to include despite failure.</param>
        /// <param name="message">Optional custom message for the error.</param>
        /// <param name="code">Optional custom code for the error.</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the exception and value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
        public static IResult<T> AsError<T>(this Exception ex, T? value, string? message = null, string? code = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            return Result<T>.Failure(value, ErrorInfo.FromException(ex, message, code)); // Use new ErrorInfo.FromException()
        }

        /// <summary>
        /// Converts a string message into a failed non-generic <see cref="IResult"/> using the <see cref="ErrorCategory.General"/> category.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="code">Optional error code. Defaults to <see cref="ErrorCodes.General"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        /// <returns>A failed <see cref="IResult"/> encapsulating the error info.</returns>
        public static IResult AsError(this string message, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            Result.Failure(ErrorInfo.General(message, code, detail, metadata, innerErrors)); // Use ErrorInfo.General()

        /// <summary>
        /// Converts a string message and a value into a failed generic <see cref="IResult{T}"/> using the <see cref="ErrorCategory.General"/> category.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="message">The error message.</param>
        /// <param name="value">The value to include despite failure.</param>
        /// <param name="code">Optional error code. Defaults to <see cref="ErrorCodes.General"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the error info and value.</returns>
        public static IResult<T> AsError<T>(this string message, T? value, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            Result<T>.Failure(value, ErrorInfo.General(message, code, detail, metadata, innerErrors)); // Use ErrorInfo.General()

        /// <summary>
        /// Converts a single <see cref="ErrorInfo"/> into a failed non-generic <see cref="IResult"/>.
        /// </summary>
        /// <param name="error">The error info instance.</param>
        /// <returns>A failed <see cref="IResult"/> encapsulating the error.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is null.</exception>
        public static IResult AsError(this ErrorInfo error)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return Result.Failure(error);
        }

        /// <summary>
        /// Converts a single <see cref="ErrorInfo"/> and a value into a failed generic <see cref="IResult{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="error">The error info instance.</param>
        /// <param name="value">The value to include despite failure.</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the error and value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is null.</exception>
        public static IResult<T> AsError<T>(this ErrorInfo error, T? value)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return Result<T>.Failure(value, error);
        }

        /// <summary>
        /// Converts multiple <see cref="ErrorInfo"/> instances into a failed non-generic <see cref="IResult"/>.
        /// </summary>
        /// <param name="errors">The error info collection. Must not be null or empty.</param>
        /// <returns>A failed <see cref="IResult"/> encapsulating all errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static IResult AsError(this IEnumerable<ErrorInfo> errors)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            if (!errors.Any())
            {
                throw new ArgumentException("Error collection cannot be empty.", nameof(errors));
            }
            return Result.Failure(errors);
        }

        /// <summary>
        /// Converts multiple <see cref="ErrorInfo"/> instances and a value into a failed generic <see cref="IResult{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="errors">The error info collection. Must not be null or empty.</param>
        /// <param name="value">The value to include despite failure.</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the errors and value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static IResult<T> AsError<T>(this IEnumerable<ErrorInfo> errors, T? value)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            if (!errors.Any())
            {
                throw new ArgumentException("Error collection cannot be empty.", nameof(errors));
            }
            return Result<T>.Failure(value, errors);
        }

        // --- Removed AsNoContent() ---
        // AsNoContent is removed because it implies a specific HTTP 204 status code,
        // which violates the principle of 100% transport-unawareness at the domain level.
        // If a "success without value" is needed, Result.Success() without a value (for non-generic)
        // or Result<T>.Success(default(T)) should be used, with the transport layer
        // mapping that to an appropriate status if needed (e.g., HTTP 200 with empty body, or HTTP 204).

        #endregion

        // ... (Other sections like 'Try' extensions, 'Bind', 'Map', 'Match' etc. would follow) ...
    }
}
