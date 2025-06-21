// <copyright file="ResultStatusCheckExtensions.cs" company="Zentient Framework Team">
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
    public static class ResultStatusCheckExtensions
    {
        #region Status Checks

        /// <summary>
        /// Determines whether the non-generic <see cref="IResult"/> represents a successful operation.
        /// </summary>
        /// <param name="result">The result instance to check.</param>
        /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static bool IsSuccess(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess;
        }

        /// <summary>
        /// Determines whether the generic <see cref="IResult{TValue}"/> represents a successful operation.
        /// Provided primarily for explicit type inference in fluent chains.
        /// </summary>
        /// <typeparam name="TValue">The value type of the result.</typeparam>
        /// <param name="result">The result instance to check.</param>
        /// <returns><c>true</c> if the result is successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for type inference.")]
        public static bool IsSuccess<TValue>(this IResult<TValue> result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsSuccess;
        }

        /// <summary>
        /// Determines whether the non-generic <see cref="IResult"/> represents a failure.
        /// </summary>
        /// <param name="result">The result instance to check.</param>
        /// <returns><c>true</c> if the result is a failure; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static bool IsFailure(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsFailure;
        }

        /// <summary>
        /// Determines whether the generic <see cref="IResult{TValue}"/> represents a failure.
        /// Provided primarily for explicit type inference in fluent chains.
        /// </summary>
        /// <typeparam name="TValue">The value type of the result.</typeparam>
        /// <param name="result">The result instance to check.</param>
        /// <returns><c>true</c> if the result is a failure; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for type inference.")]
        public static bool IsFailure<TValue>(this IResult<TValue> result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsFailure;
        }

        /// <summary>
        /// Checks if a failed <see cref="IResult"/> contains at least one error matching the specified <see cref="ErrorCategory"/>.
        /// </summary>
        /// <param name="result">The result to inspect.</param>
        /// <param name="category">The error category to match.</param>
        /// <returns><c>true</c> if the result is failure and contains any error with the given category; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static bool HasErrorCategory(this IResult result, ErrorCategory category)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsFailure && result.Errors.Any(e => e.Category == category);
        }

        /// <summary>
        /// Checks if a failed <see cref="IResult"/> contains at least one error with the specified error code.
        /// The comparison is case-sensitive.
        /// </summary>
        /// <param name="result">The result to inspect.</param>
        /// <param name="errorCode">The error code to match.</param>
        /// <returns><c>true</c> if the result is failure and contains any error with the given code; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="errorCode"/> is <c>null</c>.</exception>
        public static bool HasErrorCode(this IResult result, string errorCode)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(errorCode);
            return result.IsFailure && result.Errors.Any(e => e.Code == errorCode);
        }

        #endregion
    }
}
