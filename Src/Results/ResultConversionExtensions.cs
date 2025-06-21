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
                ? Result<bool>.Success(true, result.Status, result.Messages)
                : Result<bool>.Failure(false, result.Errors, result.Status);
        }

        /// <summary>
        /// Concatenates all error messages from a failed <see cref="IResult"/> into a single string.
        /// Returns empty string if successful or no errors.
        /// </summary>
        /// <param name="result">Result to extract error messages from.</param>
        /// <param name="separator">Separator string between error messages. Defaults to "; ".</param>
        /// <returns>Concatenated error messages or empty string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static string ToErrorString(this IResult result, string separator = "; ")
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsFailure && result.Errors != null
                ? string.Join(separator, result.Errors.Select(e => e.Message))
                : string.Empty;
        }

        /// <summary>
        /// Returns the message of the first error if the result is a failure; otherwise, null.
        /// </summary>
        /// <param name="result">Result to inspect.</param>
        /// <returns>The first error message or null if successful or no errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static string? FirstErrorMessage(this IResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsFailure && result.Errors?.Count > 0
                ? result.Errors[0].Message
                : null;
        }

        #endregion
    }
}
