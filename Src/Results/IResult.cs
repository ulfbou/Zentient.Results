// <copyright file="IResult.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

namespace Zentient.Results
{
    /// <summary>
    /// Defines the contract for an non-generic operation's outcome, which can be either a success or a failure.
    /// </summary>
    public interface IResult
    {
        /// <summary>Gets a value indicating whether the operation was successful.</summary>
        bool IsSuccess { get; }

        /// <summary>Gets a value indicating whether the operation failed.</summary>
        bool IsFailure { get; }

        /// <summary>Gets a read-only list of detailed error information if the operation failed. Empty if successful.</summary>
        IReadOnlyList<ErrorInfo> Errors { get; }

        /// <summary>Gets a read-only list of messages associated with the result (success or failure).</summary>
        IReadOnlyList<string> Messages { get; }

        /// <summary>Gets the message of the first error if the operation failed; otherwise, null.</summary>
        string? ErrorMessage { get; }

        /// <summary>Gets the semantic status of the result, providing contextual information (e.g., HTTP-like status codes).</summary>
        IResultStatus Status { get; }
    }
}
