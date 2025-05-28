using System.Runtime.Serialization;

namespace Zentient.Results
{
    /// <summary>
    /// Represents categories for errors, providing strong typing for common error types.
    /// </summary>
    public enum ErrorCategory
    {
        /// <summary>Represents no specific error category. This is the default value.</summary>
        [EnumMember(Value = "none")] None = 0,

        /// <summary>A general, uncategorized error.</summary>
        [EnumMember(Value = "general")] General,

        /// <summary>An error related to input validation.</summary>
        [EnumMember(Value = "validation")] Validation,

        /// <summary>An error during authentication (e.g., invalid credentials).</summary>
        [EnumMember(Value = "authentication")] Authentication,

        /// <summary>An error due to insufficient authorization.</summary>
        [EnumMember(Value = "authorization")] Authorization,

        /// <summary>A resource was not found.</summary>
        [EnumMember(Value = "not_found")] NotFound,

        /// <summary>A conflict occurred (e.g., a duplicate resource).</summary>
        [EnumMember(Value = "conflict")] Conflict,

        /// <summary>An unhandled exception occurred.</summary>
        [EnumMember(Value = "exception")] Exception,

        /// <summary>An error related to a network issue.</summary>
        [EnumMember(Value = "network")] Network,

        /// <summary>An error related to a database operation.</summary>
        [EnumMember(Value = "database")] Database,

        /// <summary>A timeout error occurred.</summary>
        [EnumMember(Value = "timeout")] Timeout,

        /// <summary>An error related to a security issue.</summary>
        [EnumMember(Value = "security")] Security,

        /// <summary>An error related to a request (e.g., malformed request).</summary>
        [EnumMember(Value = "request")] Request,

        /// <summary>An error indicating that the user is not authorized (unauthenticated).</summary>
        [EnumMember(Value = "unauthorized")] Unauthorized,

        /// <summary>An error indicating that the user is authenticated but does not have permission to perform the action.</summary>
        [EnumMember(Value = "forbidden")] Forbidden,

        /// <summary>An error related to concurrent operations, such as concurrency conflicts.</summary>
        [EnumMember(Value = "concurrency")] Concurrency,

        /// <summary>An error indicating that too many requests have been made in a given amount of time (rate limiting).</summary>
        [EnumMember(Value = "too_many_requests")] TooManyRequests,

        /// <summary>An error related to an external service or dependency.</summary>
        [EnumMember(Value = "external_service")] ExternalService,

        /// <summary>An error related to business logic, such as validation against business rules.</summary>
        [EnumMember(Value = "business_logic")] BusinessLogic,
        ServiceUnavailable,
        InternalServerError
    }
}
