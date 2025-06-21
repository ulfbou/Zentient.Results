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
    /// to facilitate fluent, expressive, and consistent functional-style error handling and result manipulation.
    /// </summary>
    public static class ResultValueExtractionExtensions
    {
        #region Unwrapping and Value Extraction

        /// <summary>
        /// Unwraps the value from a successful generic <see cref="IResult{TValue}"/>.
        /// Throws <see cref="InvalidOperationException"/> if the result is failure.
        /// </summary>
        /// <typeparam name="TValue">Type of the result value.</typeparam>
        /// <param name="result">Result instance.</param>
        /// <returns>The contained value if successful.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the result is failure.</exception>
        public static TValue Unwrap<TValue>(this IResult<TValue> result)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Attempted to unwrap a failed result. Errors: {string.Join("; ", result.Errors.Select(e => e.Message))}");
            }

            return result.Value!;
        }

        /// <summary>
        /// Returns the success value or a specified default if the result is failure.
        /// </summary>
        /// <typeparam name="TValue">Type of the result value.</typeparam>
        /// <param name="result">Result instance.</param>
        /// <param name="defaultValue">Value to return if failure or null.</param>
        /// <returns>The success value or <paramref name="defaultValue"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static TValue GetValueOrDefault<TValue>(this IResult<TValue> result, TValue defaultValue)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess ? result.Value! : defaultValue;
        }

        #endregion
    }
}
