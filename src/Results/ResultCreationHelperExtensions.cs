// <copyright file="ResultCreationHelpersExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Zentient.Results
{
    /// <summary>
    /// Provides a comprehensive set of extension methods for <see cref="IResult"/> and <see cref="IResult{T}"/>
    /// to facilitate fluent, expressive, and consistent functional-style error handling and result manipulation.
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
        /// <param name="failureError">Optional failure <see cref="ErrorInfo"/>; defaults to general error if omitted.</param>
        /// <returns>An <see cref="IResult"/> representing success or failure.</returns>
        public static IResult AsResult(this bool isSuccess, string? successMessage = null, ErrorInfo? failureError = null) => isSuccess
                ? Result.Success(successMessage)
                : Result.Failure(failureError ?? new ErrorInfo("GeneralError", "Operation failed."));

        /// <summary>
        /// Converts a <see cref="bool"/> and a value into a generic <see cref="IResult{T}"/>,
        /// producing success with the value if <paramref name="isSuccess"/> is true; otherwise failure.
        /// Optional success message and failure error details can be provided.
        /// </summary>
        /// <typeparam name="T">Type of the value to carry.</typeparam>
        /// <param name="isSuccess">Boolean flag indicating success status.</param>
        /// <param name="value">Value to include if success.</param>
        /// <param name="successMessage">Optional success message to include.</param>
        /// <param name="failureError">Optional failure <see cref="ErrorInfo"/>; defaults to general error if omitted.</param>
        /// <returns>An <see cref="IResult{T}"/> representing success or failure.</returns>
        public static IResult<T> AsResult<T>(this bool isSuccess, T value, string? successMessage = null, ErrorInfo? failureError = null) => isSuccess
            ? Result<T>.Success(value, successMessage)
            : Result<T>.Failure(failureError ?? new ErrorInfo("GeneralError", "Operation failed."));

        /// <summary>
        /// Converts a string message into a successful non-generic <see cref="IResult"/>.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <returns>A successful <see cref="IResult"/> with the message.</returns>
        public static IResult AsSuccess(this string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Success message cannot be null or empty.", nameof(message));
            }

            return Result.Success(message);
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
        /// Converts an exception into a failed non-generic <see cref="IResult"/>,
        /// optionally including a custom status.
        /// </summary>
        /// <typeparam name="TException">Type of the exception.</typeparam>
        /// <param name="ex">The exception instance.</param>
        /// <param name="status">Optional custom result status.</param>
        /// <returns>A failed <see cref="IResult"/> encapsulating the exception.</returns>
        public static IResult AsError<TException>(this TException ex, IResultStatus? status = null)
            where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex)); // Add null check for extension method
            return Result.FromException(ex, status); // This now correctly calls the updated FromException
        }

        /// <summary>
        /// Converts an exception into a failed generic <see cref="IResult{T}"/> with default value,
        /// optionally including a custom status.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="ex">The exception instance.</param>
        /// <param name="status">Optional custom result status.</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the exception.</returns>
        public static IResult<T> AsError<T>(this Exception ex, IResultStatus? status = null) =>
            Result<T>.FromException(default, ex, status);

        /// <summary>
        /// Converts an exception and an explicit value into a failed generic <see cref="IResult{T}"/>,
        /// optionally including a custom status.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="ex">The exception instance.</param>
        /// <param name="value">The value to include despite failure.</param>
        /// <param name="status">Optional custom result status.</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the exception and value.</returns>
        public static IResult<T> AsError<T>(this Exception ex, T? value, IResultStatus? status = null) =>
            Result<T>.FromException(value, ex, status);

        /// <summary>
        /// Converts a string message into a failed non-generic <see cref="IResult"/> with an optional error code.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="code">The error code. Defaults to "GeneralError".</param>
        /// <returns>A failed <see cref="IResult"/> encapsulating the error info.</returns>
        public static IResult AsError(this string message, string code = "GeneralError") =>
            Result.Failure(new ErrorInfo(ErrorCategory.General, code, message));

        /// <summary>
        /// Converts a string message and a value into a failed generic <see cref="IResult{T}"/> with an optional error code.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="message">The error message.</param>
        /// <param name="value">The value to include despite failure.</param>
        /// <param name="code">The error code. Defaults to "GeneralError".</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the error info and value.</returns>
        public static IResult<T> AsError<T>(this string message, T? value, string code = "GeneralError") =>
            Result<T>.Failure(value, new ErrorInfo(ErrorCategory.General, code, message));

        /// <summary>
        /// Converts a single <see cref="ErrorInfo"/> into a failed non-generic <see cref="IResult"/>.
        /// </summary>
        /// <param name="error">The error info instance.</param>
        /// <returns>A failed <see cref="IResult"/> encapsulating the error.</returns>
        public static IResult AsError(this ErrorInfo error) =>
            Result.Failure(error);

        /// <summary>
        /// Converts a single <see cref="ErrorInfo"/> and a value into a failed generic <see cref="IResult{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="error">The error info instance.</param>
        /// <param name="value">The value to include despite failure.</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the error and value.</returns>
        public static IResult<T> AsError<T>(this ErrorInfo error, T? value) =>
            Result<T>.Failure(value, error);

        /// <summary>
        /// Converts multiple <see cref="ErrorInfo"/> instances into a failed non-generic <see cref="IResult"/>.
        /// </summary>
        /// <param name="errors">The error info collection.</param>
        /// <returns>A failed <see cref="IResult"/> encapsulating all errors.</returns>
        public static IResult AsError(this IEnumerable<ErrorInfo> errors) =>
            Result.Failure(errors);

        /// <summary>
        /// Converts multiple <see cref="ErrorInfo"/> instances and a value into a failed generic <see cref="IResult{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="errors">The error info collection.</param>
        /// <param name="value">The value to include despite failure.</param>
        /// <returns>A failed <see cref="IResult{T}"/> encapsulating the errors and value.</returns>
        public static IResult<T> AsError<T>(this IEnumerable<ErrorInfo> errors, T? value) =>
            Result<T>.Failure(value, errors);

        /// <summary>
        /// Converts a value into a NoContent generic <see cref="IResult{T}"/>.
        /// Useful to indicate absence of content while preserving type.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>A <see cref="IResult{T}"/> representing no content.</returns>
        public static IResult<T> AsNoContent<T>(this T _) =>
            Result<T>.NoContent();

        #endregion
    }
}
