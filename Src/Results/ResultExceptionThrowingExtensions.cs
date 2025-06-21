// <copyright file="ResultExceptionThrowingExtensions.cs" company="Zentient Framework Team">
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
    public static class ResultExceptionThrowingExtensions
    {
        #region Exception Throwing

        /// <summary>
        /// Throws a <see cref="ResultException"/> if the result is failure.
        /// Encapsulates all errors in the exception.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        /// <exception cref="ResultException">Thrown if the result is a failure.</exception>
        public static void ThrowIfFailure(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.IsFailure)
            {
                throw new ResultException(result.Errors);
            }
        }

        #endregion
    }
}
