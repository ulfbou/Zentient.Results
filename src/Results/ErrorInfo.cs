// <copyright file="ErrorInfo.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using Zentient.Results.Constants;
using Zentient.Results.Serialization;

namespace Zentient.Results
{
    /// <summary>Represents detailed information about an error that occurred during an operation.</summary>
    /// <remarks>This class is immutable after creation, with properties initialized via `init` setters.</remarks>
    [DataContract] // Retain for potential WCF/legacy interop, but System.Text.Json is primary
    public sealed class ErrorInfo : IEquatable<ErrorInfo>
    {
        /// <summary>Gets the category of the error.</summary>
        [DataMember(Order = 1)]
        [JsonPropertyName(JsonConstants.ErrorInfo.Category)]
        public ErrorCategory Category { get; init; }

        /// <summary>Gets a machine-readable code for the error, typically representing a specific type of error.</summary>
        /// <remarks>This code should be concise and stable for programmatic error identification.</remarks>
        [DataMember(Order = 2)]
        [JsonPropertyName(JsonConstants.ErrorInfo.Code)]
        public string Code { get; init; }

        /// <summary>Gets a human-readable message describing the error, suitable for end-users or logs.</summary>
        [DataMember(Order = 3)]
        [JsonPropertyName(JsonConstants.ErrorInfo.Message)]
        public string Message { get; init; }

        /// <summary>Gets additional, more granular details about the error, often for debugging or extended context.</summary>
        [DataMember(Order = 4)]
        [JsonPropertyName(JsonConstants.ErrorInfo.Detail)]
        public string? Detail { get; init; }

        /// <summary>
        /// Gets a dictionary of structured metadata associated with the error.
        /// This allows for strongly typed and extensible error information, e.g., validation rule failures.
        /// </summary>
        /// <remarks>
        /// The dictionary is an <see cref="IImmutableDictionary{TKey, TValue}"/> to maintain immutability.
        /// </remarks>
        [DataMember(Order = 5)]
        [JsonPropertyName(JsonConstants.ErrorInfo.Metadata)]
        public IImmutableDictionary<string, object?> Metadata { get; init; }

        /// <summary>Gets a list of inner errors, for nested or compound error scenarios.</summary>
        /// <remarks>
        /// The list is an <see cref="IImmutableList{T}"/> to maintain immutability.
        /// </remarks>
        [DataMember(Order = 6)]
        [JsonPropertyName(JsonConstants.ErrorInfo.InnerErrors)]
        public IImmutableList<ErrorInfo> InnerErrors { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorInfo"/> class with all properties.
        /// This is the primary constructor used for full error detail specification.
        /// </summary>
        /// <param name="category">The category of the error.</param>
        /// <param name="code">A machine-readable code for the error. Cannot be null.</param>
        /// <param name="message">A human-readable message describing the error. Cannot be null.</param>
        /// <param name="detail">Optional, more granular details about the error.</param>
        /// <param name="metadata">Optional dictionary of structured metadata. If null, an empty immutable dictionary is used.</param>
        /// <param name="innerErrors">Optional list of inner errors. If null, an empty immutable list is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="code"/> or <paramref name="message"/> is null.</exception>
        [JsonConstructor]
        public ErrorInfo(
            ErrorCategory category,
            string code,
            string message,
            string? detail = null,
            IImmutableDictionary<string, object?>? metadata = null,
            IImmutableList<ErrorInfo>? innerErrors = null)
        {
            Category = category;
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Detail = detail;
            Metadata = metadata ?? ImmutableDictionary<string, object?>.Empty;
            InnerErrors = innerErrors ?? ImmutableList<ErrorInfo>.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorInfo"/> class with default values
        /// for a general error without specific details.
        /// </summary>
        /// <remarks>
        /// This constructor initializes the error with <see cref="ErrorCategory.General"/>,
        /// <see cref="ErrorCodes.General"/>, and a default message.
        /// </remarks>
        public ErrorInfo()
            : this(ErrorCategory.General, ErrorCodes.General, "An unspecified error occurred.") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorInfo"/> class with a code and message,
        /// defaulting the category to <see cref="ErrorCategory.General"/>.
        /// </summary>
        /// <param name="code">A machine-readable code for the error. Cannot be null.</param>
        /// <param name="message">A human-readable message describing the error. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="code"/> or <paramref name="message"/> is null.</exception>
        public ErrorInfo(string code, string message)
            : this(ErrorCategory.General, code, message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorInfo"/> class with a specific category, code, and message.
        /// </summary>
        /// <param name="category">The category of the error.</param>
        /// <param name="code">A machine-readable code for the error. Cannot be null.</param>
        /// <param name="message">A human-readable message describing the error. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="code"/> or <paramref name="message"/> is null.</exception>
        // Note: This constructor overlaps with the [JsonConstructor] if not careful, but useful for explicit category setting.
        // I've kept it as it provides a direct way to set all core properties without optional parameters.
        // The JsonConstructor is the one with the most parameters (including optionals), which System.Text.Json prefers.
        public ErrorInfo(ErrorCategory category, string code, string message)
            : this(category, code, message, null, null, null) { }

        // --- Static Factory Methods (aligned with ErrorCategory and ErrorCodes) ---

        /// <summary>Creates a general error with a default code and message.</summary>
        /// <param name="message">Optional human-readable message. Defaults to a generic message.</param>
        /// <param name="code">Optional machine-readable code. Defaults to <see cref="ErrorCodes.General"/>.</param>
        public static ErrorInfo General(string? message = null, string? code = null) =>
            new ErrorInfo(ErrorCategory.General, code ?? ErrorCodes.General, message ?? "An unspecified error occurred.");

        /// <summary>Creates a validation error.</summary>
        /// <param name="message">The validation error message. Defaults to a generic validation message.</param>
        /// <param name="code">Optional code for the validation error. Defaults to <see cref="ErrorCodes.Validation"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata (e.g., field-specific errors).</param>
        public static ErrorInfo Validation(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null) =>
            new ErrorInfo(ErrorCategory.Validation, code ?? ErrorCodes.Validation, message ?? "One or more validation errors occurred.", detail, metadata?.ToImmutableDictionary());

        /// <summary>Creates a not found error.</summary>
        /// <param name="message">The not found error message. Defaults to a generic not found message.</param>
        /// <param name="code">Optional code for the not found error. Defaults to <see cref="ErrorCodes.NotFound"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        public static ErrorInfo NotFound(string? message = null, string? code = null, string? detail = null) =>
            new ErrorInfo(ErrorCategory.NotFound, code ?? ErrorCodes.NotFound, message ?? "The requested resource was not found.", detail);

        /// <summary>Creates an authentication error (e.g., unauthorized access due to missing/invalid credentials).</summary>
        /// <param name="message">The authentication error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the authentication error. Defaults to <see cref="ErrorCodes.Unauthorized"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        public static ErrorInfo Authentication(string? message = null, string? code = null, string? detail = null) =>
            new ErrorInfo(ErrorCategory.Authentication, code ?? ErrorCodes.Unauthorized, message ?? "Authentication required or failed.", detail);

        /// <summary>Creates an authorization error (e.g., forbidden access due to insufficient permissions).</summary>
        /// <param name="message">The authorization error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the authorization error. Defaults to <see cref="ErrorCodes.Forbidden"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        public static ErrorInfo Authorization(string? message = null, string? code = null, string? detail = null) =>
            new ErrorInfo(ErrorCategory.Authorization, code ?? ErrorCodes.Forbidden, message ?? "You do not have permission to perform this action.", detail);

        /// <summary>Creates a conflict error (e.g., resource already exists or state conflict).</summary>
        /// <param name="message">The conflict error message. Defaults to a generic conflict message.</param>
        /// <param name="code">Optional code for the conflict error. Defaults to <see cref="ErrorCodes.Conflict"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        public static ErrorInfo Conflict(string? message = null, string? code = null, string? detail = null) =>
            new ErrorInfo(ErrorCategory.Conflict, code ?? ErrorCodes.Conflict, message ?? "A conflict occurred with the current state of the resource.", detail);

        /// <summary>Creates an error for an unhandled exception.</summary>
        /// <param name="ex">The exception to convert to an error.</param>
        /// <param name="message">Optional custom message. If null, uses exception message.</param>
        /// <param name="code">Optional custom code. If null, uses the exception's type name.</param>
        /// <remarks>
        /// The exception's message, stack trace, source, and type are automatically added to metadata.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
        public static ErrorInfo FromException(Exception ex, string? message = null, string? code = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));

            // Create a mutable dictionary to populate metadata, then convert to immutable
            var metadata = new Dictionary<string, object?>();
            if (ex.Message != null) metadata[Constants.MetadataKeys.ExceptionMessage] = ex.Message;
            if (ex.StackTrace != null) metadata[Constants.MetadataKeys.ExceptionStackTrace] = ex.StackTrace;
            if (ex.Source != null) metadata[Constants.MetadataKeys.ExceptionSource] = ex.Source;
            metadata[Constants.MetadataKeys.ExceptionType] = ex.GetType().FullName;

            return new ErrorInfo(
                ErrorCategory.Exception,
                code ?? ex.GetType().Name ?? Constants.ErrorCodes.Exception,
                message ?? ex!.Message!,
                detail: ex.StackTrace,
                metadata: metadata?.ToImmutableDictionary());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var parts = new List<string> { $"Category: {Category}", $"Code: {Code}", $"Message: {Message}" };
            if (!string.IsNullOrWhiteSpace(Detail))
            {
                parts.Add($"Detail: {Detail}");
            }
            if (Metadata?.Any() == true)
            {
                parts.Add($"Metadata: {{{string.Join(", ", Metadata.Select(kv => $"{kv.Key}={kv.Value}"))}}}");
            }
            if (InnerErrors?.Any() == true)
            {
                parts.Add($"Inner Errors: [{string.Join("; ", InnerErrors.Select(e => e.ToString()))}]");
            }
            return $"ErrorInfo({string.Join(", ", parts)})";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as ErrorInfo);

        /// <inheritdoc />
        public bool Equals(ErrorInfo? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // Check value equality for properties
            return Category == other.Category &&
                   Code == other.Code &&
                   Message == other.Message &&
                   Detail == other.Detail &&
                   (Metadata?.SequenceEqual(other.Metadata) ?? other.Metadata == null) &&
                   (InnerErrors?.SequenceEqual(other.InnerErrors) ?? other.InnerErrors == null);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Category);
            hash.Add(Code);
            hash.Add(Message);
            hash.Add(Detail);

            if (Metadata != null)
            {
                foreach (var kvp in Metadata.OrderBy(k => k.Key))
                {
                    hash.Add(kvp.Key);
                    hash.Add(kvp.Value);
                }
            }
            if (InnerErrors != null)
            {
                foreach (var error in InnerErrors)
                {
                    hash.Add(error);
                }
            }
            return hash.ToHashCode();
        }

        internal static ErrorInfo Unauthorized(string message, string? code) =>
            new ErrorInfo(ErrorCategory.Authentication, code ?? ErrorCodes.Unauthorized, message ?? "Authentication required or failed.");

        internal static ErrorInfo Forbidden(string message, string? code) =>
            new ErrorInfo(ErrorCategory.Authorization, code ?? ErrorCodes.Forbidden, message ?? "You do not have permission to perform this action.");

        /// <inheritdoc />
        public static bool operator ==(ErrorInfo? left, ErrorInfo? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator !=(ErrorInfo? left, ErrorInfo? right) => !(left == right);
    }
}
