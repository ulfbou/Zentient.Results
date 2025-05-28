using System;
using System.Collections.Generic;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// A concrete implementation of <see cref="IResult"/> for use in tests.
    /// This class provides a simple way to create results with a value, success status, and optional errors or messages, in a test environment.
    /// </summary>
    internal class ConcreteResult : IResult
    {
        /// <inheritdoc />
        public bool IsSuccess { get; init; }

        /// <inheritdoc />
        public bool IsFailure => !IsSuccess;

        /// <inheritdoc />
        public IReadOnlyList<ErrorInfo> Errors { get; init; } = Array.Empty<ErrorInfo>();

        /// <inheritdoc />
        public IReadOnlyList<string> Messages { get; init; } = Array.Empty<string>();

        /// <inheritdoc />
        public string? Error { get; init; }

        /// <inheritdoc />
        public IResultStatus Status { get; init; } = new MockResultStatus(200);
    }
}
