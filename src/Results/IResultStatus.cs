// <copyright file="IResultStatus.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

namespace Zentient.Results
{
    /// <summary>
    /// Defines the contract for a result status.
    /// </summary>
    public interface IResultStatus
    {
        /// <summary>Gets the integer code representing the status (e.g., 200, 400, 404).</summary>
        int Code { get; }

        /// <summary>Gets a human-readable description for the result status (e.g., "OK", "Bad Request", "Not Found").</summary>
        string Description { get; }
    }
}
