// <copyright file="ResultValueExtractionExtensions.cs" company="Zentient Framework Team">
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
    /// to facilitate safe and explicit value extraction, including default value handling and exception throwing
    /// for failure scenarios.
    /// </summary>
    public static class ResultValueExtractionExtensions
    {
        #region Generic Result Value Extraction (IResult<T>)

        /// <summary>
        /// Attempts to retrieve the encapsulated value from a successful <see cref="IResult{TValue}"/>.
        /// This method is intended for scenarios where success is guaranteed or a failure should result in an exception.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <returns>The encapsulated value if the result is successful.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure. The exception message will include error details.</exception>
        [return: NotNull]
        public static TValue Unwrap<TValue>(this IResult<TValue> result)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.IsFailure)
            {
                // Leveraging the ErrorMessage property which concatenates messages.
                throw new InvalidOperationException($"Attempted to unwrap a failed result. Errors: {result.ErrorMessage}");
            }

            return result.Value!;
        }

        /// <summary>
        /// Retrieves the encapsulated value from a successful <see cref="IResult{TValue}"/>, or returns the default value of <typeparamref name="TValue"/>
        /// if the result is a failure.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <returns>The encapsulated value if successful, otherwise <c>default(TValue)</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        [return: MaybeNull]
        public static TValue GetValueOrDefault<TValue>(this IResult<TValue> result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess ? result.Value : default;
        }

        /// <summary>
        /// Retrieves the encapsulated value from a successful <see cref="IResult{TValue}"/>, or returns a specified fallback value
        /// if the result is a failure.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="fallbackValue">The value to return if the result is a failure.</param>
        /// <returns>The encapsulated value if successful, otherwise <paramref name="fallbackValue"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        [return: NotNullIfNotNull(nameof(fallbackValue))]
        public static TValue GetValueOrDefault<TValue>(this IResult<TValue> result, TValue fallbackValue)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess ? result.Value! : fallbackValue;
        }

        /// <summary>
        /// Retrieves the encapsulated value from a successful <see cref="IResult{TValue}"/>, or returns the result of
        /// invoking a fallback function if the result is a failure. The fallback function is only executed on failure.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="fallbackFunc">A function to invoke to get the fallback value if the result is a failure.</param>
        /// <returns>The encapsulated value if successful, otherwise the result of <paramref name="fallbackFunc"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="fallbackFunc"/> is <c>null</c>.</exception>
        public static TValue GetValueOrDefault<TValue>(this IResult<TValue> result, Func<TValue> fallbackFunc)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(fallbackFunc);
            return result.IsSuccess ? result.Value! : fallbackFunc();
        }

        /// <summary>
        /// Asynchronously retrieves the encapsulated value from a successful <see cref="IResult{TValue}"/>, or returns the result of
        /// asynchronously invoking a fallback function if the result is a failure. The fallback function is only executed on failure.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="fallbackFunc">An asynchronous function to invoke to get the fallback value if the result is a failure.</param>
        /// <returns>A <see cref="Task{TValue}"/> representing the asynchronous operation. The task result is the encapsulated value if successful, otherwise the result of <paramref name="fallbackFunc"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="fallbackFunc"/> is <c>null</c>.</exception>
        public static async Task<TValue> GetValueOrDefaultAsync<TValue>(this IResult<TValue> result, Func<Task<TValue>> fallbackFunc)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(fallbackFunc);
            return result.IsSuccess ? result.Value! : await fallbackFunc().ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to get the value from a successful <see cref="IResult{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="value">When this method returns, contains the value of <typeparamref name="TValue"/> if the operation was successful, otherwise the default value of <typeparamref name="TValue"/>.</param>
        /// <returns><c>true</c> if the result was successful and the value was retrieved; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static bool TryGetValue<TValue>(this IResult<TValue> result, [MaybeNullWhen(false)] out TValue value)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.IsSuccess)
            {
                value = result.Value!;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Retrieves the encapsulated value from a successful <see cref="IResult{TValue}"/> or throws an <see cref="InvalidOperationException"/>
        /// if the result is a failure. This method is an alias for <see cref="Unwrap{TValue}(IResult{TValue})"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <returns>The encapsulated value if successful.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        [return: NotNull]
        public static TValue GetValueOrThrow<TValue>(this IResult<TValue> result) => result.Unwrap();

        /// <summary>
        /// Retrieves the encapsulated value from a successful <see cref="IResult{TValue}"/> or throws an <see cref="InvalidOperationException"/>
        /// with a specific message if the result is a failure.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="message">The message for the <see cref="InvalidOperationException"/> if the result is a failure.</param>
        /// <returns>The encapsulated value if successful.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        [return: NotNull]
        public static TValue GetValueOrThrow<TValue>(this IResult<TValue> result, string message)
        {
            ArgumentNullException.ThrowIfNull(result);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(message);
            }
            return result.Value!;
        }

        /// <summary>
        /// Retrieves the encapsulated value from a successful <see cref="IResult{TValue}"/> or throws an exception
        /// created by the provided factory function if the result is a failure.
        /// </summary>
        /// <typeparam name="TValue">The type of the result's encapsulated value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="exceptionFactory">A function to create the exception to throw if the result is a failure.</param>
        /// <returns>The encapsulated value if successful.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="exceptionFactory"/> is <c>null</c>.</exception>
        /// <exception cref="Exception">The exception created by <paramref name="exceptionFactory"/> if the result is a failure.</exception>
        [return: NotNull]
        public static TValue GetValueOrThrow<TValue>(this IResult<TValue> result, Func<Exception> exceptionFactory)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(exceptionFactory);
            if (result.IsFailure)
            {
                throw exceptionFactory();
            }
            return result.Value!;
        }

        #endregion

        #region Non-Generic Result Value Extraction (IResult)

        /// <summary>
        /// Checks if a non-generic <see cref="IResult"/> is successful and throws an <see cref="InvalidOperationException"/>
        /// if it's a failure. This can be used when you expect a successful outcome and want to assert it.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <returns>The original <see cref="IResult"/> instance, allowing for fluent chaining after assertion.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        public static IResult Unwrap(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Attempted to unwrap a failed non-generic result. Errors: {result.ErrorMessage}");
            }

            return result;
        }

        /// <summary>
        /// Attempts to convert a non-generic <see cref="IResult"/> to a boolean, indicating success or failure.
        /// This is equivalent to checking the <see cref="IResult.IsSuccess"/> property.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static bool ToBoolean(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess;
        }

        /// <summary>
        /// Returns <c>true</c> if the non-generic <see cref="IResult"/> is successful; otherwise, <c>false</c>.
        /// This method is an alias for <see cref="IResult.IsSuccess"/>, providing a consistent "Try" pattern.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static bool TryGetSuccess(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess;
        }

        #endregion
    }
}
