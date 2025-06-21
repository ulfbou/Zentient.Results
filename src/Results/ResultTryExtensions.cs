// <copyright file="ResultTryExtensions.cs" company="Zentient Framework Team">
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
    public static class ResultTryExtensions
    {
        #region Try Methods

        /// <summary>Wraps an action into a non-generic <see cref="IResult"/>, catching exceptions and returning a failure result if an exception occurs.</summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>A successful <see cref="IResult"/> if the action completes without exception; otherwise, a failed <see cref="IResult"/> containing error information from the exception.</returns>
        public static IResult Try(this Action action) // Non-generic Try for Action
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            try
            {
                action();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        /// <summary>Wraps a function into a <see cref="Result{T}"/>, catching exceptions and returning a failure result if an exception occurs.</summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <returns>A successful <see cref="IResult{T}"/> with the function's return value if it completes without exception; otherwise, a failed <see cref="IResult{T}"/> containing error information from the exception.</returns>
        public static IResult<T> Try<T>(this Func<T> func)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));

            try
            {
                return Result<T>.Success(func());
            }
            catch (Exception ex)
            {
                return Result<T>.FromException(default(T), ex);
            }
        }

        #endregion
    }
}
