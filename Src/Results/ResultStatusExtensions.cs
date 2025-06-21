// <copyright file="ResultStatusExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

namespace Zentient.Results
{
    /// <summary>
    /// Provides extension methods for <see cref="IResultStatus"/>.
    /// </summary>
    public static class ResultStatusExtensions
    {
        /// <summary>
        /// Gets the HTTP status code equivalent for the given <see cref="IResultStatus"/>.
        /// For custom statuses, this simply returns the status's code.
        /// </summary>
        /// <param name="status">The result status.</param>
        /// <returns>The integer HTTP status code.</returns>
        public static int ToHttpStatusCode(this IResultStatus status)
        {
            ArgumentNullException.ThrowIfNull(status, nameof(status));
            return status.Code;
        }
    }
}
