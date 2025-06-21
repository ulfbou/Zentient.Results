// <copyright file="ErrorCategory.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Runtime.Serialization; // Required for [EnumMember]

namespace Zentient.Results
{
    /// <summary>
    /// Represents categories for errors, providing strong typing for common error types.
    /// This enum is crucial for structured error handling and mapping errors to appropriate
    /// responses or logging behaviors.
    /// </summary>
    public enum ErrorCategory
    {
        /// <summary>Represents no specific error category. This is the default value.</summary>
        [EnumMember(Value = "none")] None = 0,

        /// <summary>A general, uncategorized error. Use when a more specific category isn't applicable.</summary>
        [EnumMember(Value = "general")] General,

        /// <summary>An error related to input validation, indicating that provided data is invalid or missing.</summary>
        [EnumMember(Value = "validation")] Validation,

        /// <summary>An error during authentication, such as invalid credentials or missing authentication tokens.</summary>
        [EnumMember(Value = "authentication")] Authentication,

        /// <summary>An error due to insufficient authorization, meaning the authenticated user lacks the necessary permissions.</summary>
        [EnumMember(Value = "authorization")] Authorization,

        /// <summary>A requested resource was not found.</summary>
        [EnumMember(Value = "not_found")] NotFound,

        /// <summary>A conflict occurred, typically when attempting to create a resource that already exists, or a state conflict.</summary>
        [EnumMember(Value = "conflict")] Conflict,

        /// <summary>An unhandled or unexpected exception occurred within the application logic.</summary>
        [EnumMember(Value = "exception")] Exception,

        /// <summary>An error related to network connectivity or communication issues with external services.</summary>
        [EnumMember(Value = "network")] Network,

        /// <summary>An error related to a database operation, such as connection issues, query failures, or constraint violations.</summary>
        [EnumMember(Value = "database")] Database,

        /// <summary>A timeout error occurred, indicating an operation did not complete within a specified time limit.</summary>
        [EnumMember(Value = "timeout")] Timeout,

        /// <summary>An error related to a security vulnerability or a security-related policy violation.</summary>
        [EnumMember(Value = "security")] Security,

        /// <summary>An error related to the client's request itself, such as a malformed request body or invalid headers.</summary>
        [EnumMember(Value = "request")] Request,

        /// <summary>An error related to concurrent operations, such as optimistic concurrency conflicts when updating data.</summary>
        [EnumMember(Value = "concurrency")] Concurrency,

        /// <summary>An error indicating that too many requests have been made in a given amount of time (rate limiting).</summary>
        [EnumMember(Value = "too_many_requests")] TooManyRequests,

        /// <summary>An error related to an issue with an external service or a dependency.</summary>
        [EnumMember(Value = "external_service")] ExternalService,

        /// <summary>An error related to business logic rules or constraints that were violated.</summary>
        [EnumMember(Value = "business_logic")] BusinessLogic,

        /// <summary>An error indicating that the requested resource is no longer available at the origin server.</summary>
        [EnumMember(Value = "gone")] ResourceGone,

        /// <summary>An error indicating that the requested operation or feature is not implemented.</summary>
        [EnumMember(Value = "not_implemented")] NotImplemented,

        /// <summary>An error indicating an internal server issue or unexpected condition.</summary>
        /// <remarks>This is often a catch-all for server-side failures not covered by more specific categories.</remarks>
        [EnumMember(Value = "internal_server_error")] InternalServerError,

        /// <summary>An error indicating that the service is temporarily unavailable, usually due to maintenance or overload.</summary>
        [EnumMember(Value = "service_unavailable")] ServiceUnavailable,

        /// <summary>An error indicating a problem with the API specification or a generic problem details response structure.</summary>
        [EnumMember(Value = "problem_details")] ProblemDetails,

        /// <summary>An error indicating that the client has exceeded the allowed rate limit for requests.</summary>
        [EnumMember(Value = "rate_limit")] RateLimit,
    }
}
