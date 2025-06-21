// <copyright file="ResultSideEffectExtensions.cs" company="Zentient Framework Team">
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
    public static class ResultSideEffectExtensions
    {
        #region Side-Effect Methods (Tap / OnSuccess / OnFailure)

        /// <summary>
        /// Executes an action if the non-generic <see cref="IResult"/> is successful.
        /// Allows fluent side-effect chaining without modifying the original result.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">The action to execute if success.</param>
        /// <returns>The original <see cref="IResult"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onSuccess"/> is <c>null</c>.</exception>
        public static IResult OnSuccess(this IResult result, Action onSuccess)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);

            if (result.IsSuccess)
            {
                onSuccess();
            }

            return result;
        }

        /// <summary>
        /// Executes an action if the generic <see cref="IResult{TValue}"/> is successful,
        /// passing the success value. Allows fluent side-effect chaining.
        /// </summary>
        /// <typeparam name="TValue">Type of the result value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onSuccess">Action accepting the success value.</param>
        /// <returns>The original <see cref="IResult{TValue}"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onSuccess"/> is <c>null</c>.</exception>
        public static IResult<TValue> OnSuccess<TValue>(this IResult<TValue> result, Action<TValue> onSuccess)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onSuccess);

            if (result.IsSuccess)
            {
                onSuccess(result.Value!);
            }

            return result;
        }

        /// <summary>
        /// Executes an action if the non-generic <see cref="IResult"/> is a failure,
        /// passing the list of errors. Allows fluent side-effect chaining.
        /// </summary>
        /// <param name="result">The result instance.</param>
        /// <param name="onFailure">Action accepting the error list.</param>
        /// <returns>The original <see cref="IResult"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static IResult OnFailure(this IResult result, Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onFailure);

            if (result.IsFailure)
            {
                onFailure(result.Errors);
            }

            return result;
        }

        /// <summary>
        /// Executes an action if the generic <see cref="IResult{TValue}"/> is a failure,
        /// passing the list of errors. Allows fluent side-effect chaining.
        /// </summary>
        /// <typeparam name="TValue">Type of the result value.</typeparam>
        /// <param name="result">The result instance.</param>
        /// <param name="onFailure">Action accepting the error list.</param>
        /// <returns>The original <see cref="IResult{TValue}"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="onFailure"/> is <c>null</c>.</exception>
        public static IResult<TValue> OnFailure<TValue>(this IResult<TValue> result, Action<IReadOnlyList<ErrorInfo>> onFailure)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNull(onFailure);


            if (result.IsFailure)
            {
                onFailure(result.Errors);
            }

            return result;
        }

        #endregion
    }
}
