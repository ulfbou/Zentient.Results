// <copyright file="ResultStatus.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Net;

namespace Zentient.Results
{
    /// <summary>A default implementation of <see cref="IResultStatus"/> with a code and description.</summary>
    public readonly struct ResultStatus : IResultStatus, IEquatable<ResultStatus>
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
        public ResultStatus(int code, string description)
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

        /// <summary>Creates a <see cref="ResultStatus"/> from an HTTP status code.</summary>
        /// <param name="statusCode">The HTTP status code to convert.</param>
        /// <returns>A new <see cref="IResultStatus"/> instance representing the HTTP status code.</returns>
        public static IResultStatus FromHttpStatusCode(int statusCode)
        {
            var httpStatusCode = (HttpStatusCode)statusCode;
            string description = httpStatusCode.ToString();

            return ResultStatuses.GetStatus(statusCode, description);
        }

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
        public static bool operator ==(ResultStatus? left, ResultStatus? right) =>
            Equals(left, right);

        /// <inheritdoc />
        public static bool operator !=(ResultStatus? left, ResultStatus? right) =>
            !Equals(left, right);
    }
}
