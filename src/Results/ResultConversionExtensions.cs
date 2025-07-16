// <copyright file="ResultConversionExtensions.cs" company="Zentient Framework Team">
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
    public static class ResultConversionExtensions
    {
        #region Conversions & Representations

        /// <summary>
        /// Converts a non-generic <see cref="IResult"/> into a generic <see cref="IResult{bool}"/>,
        /// where success maps to <c>true</c> and failure to <c>false</c>.
        /// Errors and messages are preserved.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        /// <returns>A <see cref="IResult{bool}"/> reflecting the success or failure state.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static IResult<bool> ToBoolResult(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess
                ? Result<bool>.Success(true, messages: result.Messages)
                : Result<bool>.Failure(false, errors: result.Errors);
        }

        /// <summary>
        /// Concatenates all error messages from a failed <see cref="IResult"/> into a single string.
        /// Returns an empty string if the result is successful or if there are no error messages.
        /// </summary>
        /// <param name="result">The result to extract error messages from.</param>
        /// <param name="separator">The separator string to place between error messages. Defaults to "; ".</param>
        /// <returns>Concatenated error messages or an empty string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static string ToErrorString(this IResult result, string separator = "; ")
        {
            ArgumentNullException.ThrowIfNull(result);
            // Only join messages from errors if it's a failure and there are errors with messages.
            return result.IsFailure && result.Errors.Any(e => !string.IsNullOrWhiteSpace(e.Message))
                ? string.Join(separator, result.Errors.Where(e => !string.IsNullOrWhiteSpace(e.Message)).Select(e => e.Message))
                : string.Empty;
        }

        /// <summary>
        /// Returns the message of the first error if the result is a failure and has errors; otherwise, <c>null</c>.
        /// This method simply wraps the <see cref="IResult.ErrorMessage"/> property for consistency as an extension.
        /// </summary>
        /// <param name="result">The result to inspect.</param>
        /// <returns>The first error message or <c>null</c> if successful or no errors are present.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static string? FirstErrorMessage(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            // Directly use the ErrorMessage property from ResultBase, which already implements this logic.
            return result.ErrorMessage;
        }

        #endregion

        #region Value Extraction (for IResult<T>)

        /// <summary>
        /// Attempts to get the value from a successful <see cref="IResult{T}"/>,
        /// or returns the default value of <typeparamref name="T"/> if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="result">The result to extract the value from.</param>
        /// <returns>The encapsulated value if successful, otherwise <c>default(T)</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        [return: MaybeNull] // Use MaybeNull for default(T) scenarios
        public static T GetValueOrDefault<T>(this IResult<T> result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess ? result.Value : default;
        }

        /// <summary>
        /// Attempts to get the value from a successful <see cref="IResult{T}"/>,
        /// or returns a specified fallback value if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="result">The result to extract the value from.</param>
        /// <param name="fallbackValue">The value to return if the result is a failure.</param>
        /// <returns>The encapsulated value if successful, otherwise <paramref name="fallbackValue"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        [return: NotNullIfNotNull(nameof(fallbackValue))]
        public static T GetValueOrDefault<T>(this IResult<T> result, T fallbackValue)
        {
            ArgumentNullException.ThrowIfNull(result);
            // This directly delegates to the existing GetValueOrDefault(T fallback) on Result<T>
            return result.GetValueOrDefault(fallbackValue);
        }

        /// <summary>
        /// Attempts to get the value from a successful <see cref="IResult{T}"/>,
        /// or returns the result of invoking a fallback function if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="result">The result to extract the value from.</param>
        /// <param name="fallbackFunc">A function to invoke to get the fallback value if the result is a failure.</param>
        /// <returns>The encapsulated value if successful, otherwise the result of <paramref name="fallbackFunc"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="fallbackFunc"/> is <c>null</c>.</exception>
        public static T GetValueOrDefault<T>(this IResult<T> result, Func<T> fallbackFunc)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(fallbackFunc);
            return result.IsSuccess ? result.Value! : fallbackFunc();
        }

        /// <summary>
        /// Attempts to get the value from a successful <see cref="IResult{T}"/>,
        /// or returns the result of invoking an asynchronous fallback function if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="result">The result to extract the value from.</param>
        /// <param name="fallbackFunc">An asynchronous function to invoke to get the fallback value if the result is a failure.</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation. The task result is the encapsulated value if successful, otherwise the result of <paramref name="fallbackFunc"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="fallbackFunc"/> is <c>null</c>.</exception>
        public static async Task<T> GetValueOrDefaultAsync<T>(this IResult<T> result, Func<Task<T>> fallbackFunc)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(fallbackFunc);
            return result.IsSuccess ? result.Value! : await fallbackFunc().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the value from a successful <see cref="IResult{T}"/> or throws an <see cref="InvalidOperationException"/>
        /// if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="result">The result to extract the value from.</param>
        /// <returns>The encapsulated value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        [return: NotNull]
        public static T GetValueOrThrow<T>(this IResult<T> result)
        {
            ArgumentNullException.ThrowIfNull(result);
            // This directly delegates to the existing GetValueOrThrow() on Result<T>
            return result.GetValueOrThrow()!;
        }

        /// <summary>
        /// Gets the value from a successful <see cref="IResult{T}"/> or throws an <see cref="InvalidOperationException"/>
        /// with a specific message if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="result">The result to extract the value from.</param>
        /// <param name="message">The message for the <see cref="InvalidOperationException"/> if the result is a failure.</param>
        /// <returns>The encapsulated value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        [return: NotNull]
        public static T GetValueOrThrow<T>(this IResult<T> result, string message)
        {
            ArgumentNullException.ThrowIfNull(result);
            // This directly delegates to the existing GetValueOrThrow(string message) on Result<T>
            return result.GetValueOrThrow(message)!;
        }

        /// <summary>
        /// Gets the value from a successful <see cref="IResult{T}"/> or throws an exception
        /// created by the provided factory function if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="result">The result to extract the value from.</param>
        /// <param name="exceptionFactory">A function to create the exception to throw if the result is a failure.</param>
        /// <returns>The encapsulated value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="exceptionFactory"/> is <c>null</c>.</exception>
        /// <exception cref="Exception">The exception created by <paramref name="exceptionFactory"/> if the result is a failure.</exception>
        [return: NotNull]
        public static T GetValueOrThrow<T>(this IResult<T> result, Func<Exception> exceptionFactory)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(exceptionFactory);
            // This directly delegates to the existing GetValueOrThrow(Func<Exception> exceptionFactory) on Result<T>
            return result.GetValueOrThrow(exceptionFactory)!;
        }

        #endregion
    }
}
