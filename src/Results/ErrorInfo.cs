// <copyright file="ErrorInfo.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using Zentient.Results.Constants;
using Zentient.Results.Serialization; // Assuming this contains MetadataKeys

namespace Zentient.Results
{
    /// <summary>Represents detailed information about an error that occurred during an operation.</summary>
    /// <remarks>
    /// This class is immutable after creation, with properties initialized via `init` setters.
    /// It is designed to be created primarily via its static factory methods for developer ergonomics
    /// and consistency.
    /// </remarks>
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
        /// This is the primary constructor used for full error detail specification, and is specifically
        /// marked for JSON deserialization. It is generally not intended for direct use in application
        /// code; prefer the static factory methods.
        /// </summary>
        /// <param name="category">The category of the error.</param>
        /// <param name="code">A machine-readable code for the error. Cannot be null.</param>
        /// <param name="message">A human-readable message describing the error. Cannot be null.</param>
        /// <param name="detail">Optional, more granular details about the error.</param>
        /// <param name="metadata">Optional dictionary of structured metadata. If null, an empty immutable dictionary is used.</param>
        /// <param name="innerErrors">Optional list of inner errors. If null, an empty immutable list is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="code"/> or <paramref name="message"/> is null.</exception>
        [JsonConstructor]
        private ErrorInfo( // Made private to encourage static factory usage
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

        // Removed the default public constructors:
        // public ErrorInfo() -> Replaced by ErrorInfo.General()
        // public ErrorInfo(string code, string message) -> Replaced by ErrorInfo.General(message, code)
        // public ErrorInfo(ErrorCategory category, string code, string message) -> Replaced by specific category factories

        // --- Static Factory Methods (aligned with ErrorCategory and ErrorCodes) ---

        /// <summary>Creates a general error with a default code and message.</summary>
        /// <param name="message">Optional human-readable message. Defaults to a generic message.</param>
        /// <param name="code">Optional machine-readable code. Defaults to <see cref="ErrorCodes.General"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo General(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.General, code ?? ErrorCodes.General, message ?? "An unspecified error occurred.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates a validation error.</summary>
        /// <param name="message">The validation error message. Defaults to a generic validation message.</param>
        /// <param name="code">Optional code for the validation error. Defaults to <see cref="ErrorCodes.Validation"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata (e.g., field-specific errors).</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo Validation(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.Validation, code ?? ErrorCodes.Validation, message ?? "One or more validation errors occurred.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates a bad request error.</summary>
        /// <param name="message">The bad request error message. Defaults to a generic bad request message.</param>
        /// <param name="code">Optional code for the bad request error. Defaults to <see cref="ErrorCodes.BadRequest"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo BadRequest(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.BadRequest, code ?? ErrorCodes.BadRequest, message ?? "The request was semantically incorrect or malformed.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates an authentication error (e.g., identity could not be established).</summary>
        /// <param name="message">The authentication error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the authentication error. Defaults to <see cref="ErrorCodes.Authentication"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo Authentication(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.Authentication, code ?? ErrorCodes.Authentication, message ?? "Authentication required or failed.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates an authorization error (e.g., identity lacks permission for the operation).</summary>
        /// <param name="message">The authorization error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the authorization error. Defaults to <see cref="ErrorCodes.Authorization"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo Authorization(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.Authorization, code ?? ErrorCodes.Authorization, message ?? "You do not have permission to perform this action.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates a not found error.</summary>
        /// <param name="message">The not found error message. Defaults to a generic not found message.</param>
        /// <param name="code">Optional code for the not found error. Defaults to <see cref="ErrorCodes.NotFound"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo NotFound(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.NotFound, code ?? ErrorCodes.NotFound, message ?? "The requested resource was not found.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates a conflict error (e.g., resource already exists or state conflict).</summary>
        /// <param name="message">The conflict error message. Defaults to a generic conflict message.</param>
        /// <param name="code">Optional code for the conflict error. Defaults to <see cref="ErrorCodes.Conflict"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo Conflict(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.Conflict, code ?? ErrorCodes.Conflict, message ?? "A conflict occurred with the current state of the resource.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates a business rule violation error.</summary>
        /// <param name="message">The business rule error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the business rule error. Defaults to <see cref="ErrorCodes.BusinessRuleViolation"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo BusinessRule(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.BusinessRule, code ?? ErrorCodes.BusinessRuleViolation, message ?? "A business rule was violated.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates a "not implemented" error.</summary>
        /// <param name="message">The not implemented error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the not implemented error. Defaults to <see cref="ErrorCodes.NotImplemented"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo NotImplemented(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.NotImplemented, code ?? ErrorCodes.NotImplemented, message ?? "The requested operation or feature is not implemented.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates an internal error, typically indicating an unexpected system failure or bug.</summary>
        /// <param name="message">The internal error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the internal error. Defaults to <see cref="ErrorCodes.InternalError"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo InternalError(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.InternalError, code ?? ErrorCodes.InternalError, message ?? "An unexpected internal error occurred.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates an external dependency error (e.g., failure communicating with a database or API).</summary>
        /// <param name="message">The external dependency error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the external dependency error. Defaults to <see cref="ErrorCodes.ExternalDependency"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo ExternalDependency(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.ExternalDependency, code ?? ErrorCodes.ExternalDependency, message ?? "An external dependency failed or was unreachable.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates a timeout error.</summary>
        /// <param name="message">The timeout error message. Defaults to a generic timeout message.</param>
        /// <param name="code">Optional code for the timeout error. Defaults to <see cref="ErrorCodes.Timeout"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo Timeout(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.Timeout, code ?? ErrorCodes.Timeout, message ?? "The operation timed out.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates a service unavailable error.</summary>
        /// <param name="message">The service unavailable error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the service unavailable error. Defaults to <see cref="ErrorCodes.ServiceUnavailable"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo ServiceUnavailable(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.ServiceUnavailable, code ?? ErrorCodes.ServiceUnavailable, message ?? "The service is temporarily unavailable.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates a security violation error.</summary>
        /// <param name="message">The security violation error message. Defaults to a generic message.</param>
        /// <param name="code">Optional code for the security violation error. Defaults to <see cref="ErrorCodes.SecurityViolation"/>.</param>
        /// <param name="detail">Optional, more granular details.</param>
        /// <param name="metadata">Optional dictionary of structured metadata.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        public static ErrorInfo SecurityViolation(string? message = null, string? code = null, string? detail = null, IReadOnlyDictionary<string, object?>? metadata = null, IEnumerable<ErrorInfo>? innerErrors = null) =>
            new ErrorInfo(ErrorCategory.SecurityViolation, code ?? ErrorCodes.SecurityViolation, message ?? "A security violation was detected.", detail, metadata?.ToImmutableDictionary(), innerErrors?.ToImmutableList());

        /// <summary>Creates an error from an exception, categorizing it as <see cref="ErrorCategory.InternalError"/>.</summary>
        /// <param name="ex">The exception to convert to an error.</param>
        /// <param name="message">Optional custom message. If null, uses exception message.</param>
        /// <param name="code">Optional custom code. If null, uses the exception's type name.</param>
        /// <param name="metadata">Optional dictionary of additional structured metadata to add.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        /// <remarks>
        /// The exception's message, stack trace, source, and type are automatically added to metadata.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
        public static ErrorInfo FromException(
            Exception ex,
            string? message = null,
            string? code = null,
            IReadOnlyDictionary<string, object?>? metadata = null,
            IEnumerable<ErrorInfo>? innerErrors = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));

            // Create a mutable dictionary to populate metadata, starting with any provided metadata
            var effectiveMetadata = metadata != null
                ? new Dictionary<string, object?>(metadata)
                : new Dictionary<string, object?>();

            // Add exception details to metadata
            if (ex.Message != null) effectiveMetadata[MetadataKeys.ExceptionMessage] = ex.Message;
            if (ex.StackTrace != null) effectiveMetadata[MetadataKeys.ExceptionStackTrace] = ex.StackTrace;
            if (ex.Source != null) effectiveMetadata[MetadataKeys.ExceptionSource] = ex.Source;
            effectiveMetadata[MetadataKeys.ExceptionType] = ex.GetType().FullName;

            return new ErrorInfo(
                ErrorCategory.InternalError,
                code ?? ex.GetType().Name, // Using ex.GetType().Name directly for common cases
                message ?? ex!.Message!,
                detail: ex.StackTrace,
                metadata: effectiveMetadata.ToImmutableDictionary(),
                innerErrors: innerErrors?.ToImmutableList());
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
