// <copyright file="ResultStatusBase.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Net; // This using directive is no longer needed after removing FromHttpStatusCode
using System.Collections.Generic; // For IEquatable if not already imported

namespace Zentient.Results
{
    /// <summary>A default implementation of <see cref="IResultStatus"/> with a code and description.</summary>
    public readonly struct ResultStatusBase : IResultStatus, IEquatable<ResultStatus>
    {
        /// <summary>Gets the numerical code for the result status.</summary>
        public int Code { get; }

        /// <summary>Gets a human-readable description for the result status.</summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultStatus"/> struct.
        /// </summary>
        /// <param name="code">The numerical code.</param>
        /// <param name="description">The human-readable description.</param>
        public ResultStatusBase(int code, string description)
        {
            Code = code;
            Description = description;
        }

        /// <summary>
        /// Creates a custom <see cref="ResultStatus"/> instance.
        /// </summary>
        /// <param name="code">The numerical code for the custom status.</param>
        /// <param name="description">The description for the custom status.</param>
        /// <returns>A new <see cref="ResultStatus"/> instance.</returns>
        public static ResultStatus Custom(int code, string description) => new(code, description);

        // REMOVED: public static IResultStatus FromHttpStatusCode(int statusCode)
        // This method introduced an unwanted dependency on HTTP concerns in the core Results library.
        // Its functionality should reside in Zentient.Endpoints.Http.

        /// <summary>
        /// Returns a string representation of the result status.
        /// </summary>
        /// <returns>A string in the format "(Code) Description".</returns>
        public override string ToString() => $"({Code}) {Description}";

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is ResultStatus other && Equals(other);

        /// <inheritdoc />
        public bool Equals(ResultStatus other)
            => Code == other.Code && Description == other.Description;

        /// <inheritdoc />
        public override int GetHashCode() =>
            HashCode.Combine(Code, Description);

        /// <inheritdoc />
        public static bool operator ==(ResultStatusBase left, ResultStatusBase right) => // Changed from nullable to non-nullable for structs
            left.Equals(right); // Use struct's Equals for operators

        /// <inheritdoc />
        public static bool operator !=(ResultStatusBase left, ResultStatusBase right) => // Changed from nullable to non-nullable for structs
            !left.Equals(right); // Use struct's Equals for operators
    }
}
