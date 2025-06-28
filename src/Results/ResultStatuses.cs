// <copyright file="ResultStatuses.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Collections.Generic; // Used for HashCode.Combine if needed elsewhere, but struct ResultStatus handles its own.

namespace Zentient.Results
{
    /// <summary>
    /// Provides a comprehensive set of common, predefined result statuses, where the numerical code typically aligns with HTTP status codes.
    /// This class serves as a central registry for standard and custom result statuses.
    /// </summary>
    public static class ResultStatuses
    {
        private static readonly ConcurrentDictionary<int, IResultStatus> _statuses;

        // --- 2xx Success Statuses ---
        public static readonly IResultStatus Ok = new ResultStatus(200, "OK");
        public static readonly IResultStatus Success = Ok; // Alias for clarity
        public static readonly IResultStatus Created = new ResultStatus(201, "Created");
        public static readonly IResultStatus Accepted = new ResultStatus(202, "Accepted");
        public static readonly IResultStatus NoContent = new ResultStatus(204, "No Content");

        // --- 3xx Redirection Statuses ---
        public static readonly IResultStatus MultipleChoices = new ResultStatus(300, "Multiple Choices");
        public static readonly IResultStatus MovedPermanently = new ResultStatus(301, "Moved Permanently");
        public static readonly IResultStatus Found = new ResultStatus(302, "Found");
        public static readonly IResultStatus SeeOther = new ResultStatus(303, "See Other");
        public static readonly IResultStatus NotModified = new ResultStatus(304, "Not Modified");
        public static readonly IResultStatus TemporaryRedirect = new ResultStatus(307, "Temporary Redirect");
        public static readonly IResultStatus PermanentRedirect = new ResultStatus(308, "Permanent Redirect");

        // --- 4xx Client Error Statuses ---
        public static readonly IResultStatus BadRequest = new ResultStatus(400, "Bad Request");
        public static readonly IResultStatus Unauthorized = new ResultStatus(401, "Unauthorized");
        public static readonly IResultStatus PaymentRequired = new ResultStatus(402, "Payment Required");
        public static readonly IResultStatus Forbidden = new ResultStatus(403, "Forbidden");
        public static readonly IResultStatus NotFound = new ResultStatus(404, "Not Found");
        public static readonly IResultStatus MethodNotAllowed = new ResultStatus(405, "Method Not Allowed");
        public static readonly IResultStatus NotAcceptable = new ResultStatus(406, "Not Acceptable");
        public static readonly IResultStatus RequestTimeout = new ResultStatus(408, "Request Timeout");
        public static readonly IResultStatus Conflict = new ResultStatus(409, "Conflict");
        public static readonly IResultStatus Gone = new ResultStatus(410, "Gone");
        public static readonly IResultStatus LengthRequired = new ResultStatus(411, "Length Required");
        public static readonly IResultStatus PreconditionFailed = new ResultStatus(412, "Precondition Failed");
        public static readonly IResultStatus PayloadTooLarge = new ResultStatus(413, "Payload Too Large");
        public static readonly IResultStatus UriTooLong = new ResultStatus(414, "URI Too Long");
        public static readonly IResultStatus UnsupportedMediaType = new ResultStatus(415, "Unsupported Media Type");
        public static readonly IResultStatus RangeNotSatisfiable = new ResultStatus(416, "Range Not Satisfiable");
        public static readonly IResultStatus ExpectationFailed = new ResultStatus(417, "Expectation Failed");
        public static readonly IResultStatus ImATeapot = new ResultStatus(418, "I'm a Teapot"); // RFC 2324
        public static readonly IResultStatus UnprocessableEntity = new ResultStatus(422, "Unprocessable Entity");
        public static readonly IResultStatus Locked = new ResultStatus(423, "Locked");
        public static readonly IResultStatus FailedDependency = new ResultStatus(424, "Failed Dependency");
        public static readonly IResultStatus TooEarly = new ResultStatus(425, "Too Early");
        public static readonly IResultStatus UpgradeRequired = new ResultStatus(426, "Upgrade Required");
        public static readonly IResultStatus PreconditionRequired = new ResultStatus(428, "Precondition Required");
        public static readonly IResultStatus TooManyRequests = new ResultStatus(429, "Too Many Requests");
        public static readonly IResultStatus RequestHeaderFieldsTooLarge = new ResultStatus(431, "Request Header Fields Too Large");
        public static readonly IResultStatus UnavailableForLegalReasons = new ResultStatus(451, "Unavailable For Legal Reasons");

        // --- 5xx Server Error Statuses ---
        public static readonly IResultStatus Error = new ResultStatus(500, "Internal Server Error");
        public static readonly IResultStatus InternalServerError = Error; // Alias for clarity
        public static readonly IResultStatus NotImplemented = new ResultStatus(501, "Not Implemented");
        public static readonly IResultStatus BadGateway = new ResultStatus(502, "Bad Gateway");
        public static readonly IResultStatus ServiceUnavailable = new ResultStatus(503, "Service Unavailable");
        public static readonly IResultStatus GatewayTimeout = new ResultStatus(504, "Gateway Timeout");
        public static readonly IResultStatus HttpVersionNotSupported = new ResultStatus(505, "HTTP Version Not Supported");
        public static readonly IResultStatus VariantAlsoNegotiates = new ResultStatus(506, "Variant Also Negotiates");
        public static readonly IResultStatus InsufficientStorage = new ResultStatus(507, "Insufficient Storage");
        public static readonly IResultStatus LoopDetected = new ResultStatus(508, "Loop Detected");
        public static readonly IResultStatus NotExtended = new ResultStatus(510, "Not Extended");
        public static readonly IResultStatus NetworkAuthenticationRequired = new ResultStatus(511, "Network Authentication Required");

        /// <summary>
        /// Initializes the <see cref="ResultStatuses"/> class by populating the internal dictionary
        /// with all predefined result status instances.
        /// </summary>
        static ResultStatuses()
        {
            _statuses = new ConcurrentDictionary<int, IResultStatus>();

            // Use a helper method to add to avoid redundancy, or manually add as initially done.
            // For brevity, I'll keep the direct adds, but for very large lists, a helper can be cleaner.
            // Ensure all static readonly fields above are added here.
            AddStatus(Ok);
            AddStatus(Created);
            AddStatus(Accepted);
            AddStatus(NoContent);

            AddStatus(MultipleChoices);
            AddStatus(MovedPermanently);
            AddStatus(Found);
            AddStatus(SeeOther);
            AddStatus(NotModified);
            AddStatus(TemporaryRedirect);
            AddStatus(PermanentRedirect);

            AddStatus(BadRequest);
            AddStatus(Unauthorized);
            AddStatus(PaymentRequired);
            AddStatus(Forbidden);
            AddStatus(NotFound);
            AddStatus(MethodNotAllowed);
            AddStatus(NotAcceptable);
            AddStatus(RequestTimeout);
            AddStatus(Conflict);
            AddStatus(Gone);
            AddStatus(LengthRequired);
            AddStatus(PreconditionFailed);
            AddStatus(PayloadTooLarge);
            AddStatus(UriTooLong);
            AddStatus(UnsupportedMediaType);
            AddStatus(RangeNotSatisfiable);
            AddStatus(ExpectationFailed);
            AddStatus(ImATeapot);
            AddStatus(UnprocessableEntity);
            AddStatus(Locked);
            AddStatus(FailedDependency);
            AddStatus(TooEarly);
            AddStatus(UpgradeRequired);
            AddStatus(PreconditionRequired);
            AddStatus(TooManyRequests);
            AddStatus(RequestHeaderFieldsTooLarge);
            AddStatus(UnavailableForLegalReasons);

            AddStatus(Error); // Also covers InternalServerError alias
            AddStatus(NotImplemented);
            AddStatus(BadGateway);
            AddStatus(ServiceUnavailable);
            AddStatus(GatewayTimeout);
            AddStatus(HttpVersionNotSupported);
            AddStatus(VariantAlsoNegotiates);
            AddStatus(InsufficientStorage);
            AddStatus(LoopDetected);
            AddStatus(NotExtended);
            AddStatus(NetworkAuthenticationRequired);
        }

        /// <summary>
        /// Retrieves a result status by its code. If the status is one of the predefined ones,
        /// the cached instance is returned. Otherwise, a new <see cref="ResultStatus"/>
        /// is created and returned (and potentially cached).
        /// </summary>
        /// <param name="code">The status code (e.g., HTTP status code).</param>
        /// <param name="description">An optional description for custom statuses. If null, a default description is used.</param>
        /// <returns>An instance of <see cref="IResultStatus"/> matching the code.</returns>
        public static IResultStatus GetStatus(int code, string? description = null)
        {
            // Use the public struct ResultStatus for all instances, whether predefined or custom.
            return _statuses.GetOrAdd(code, _ => new ResultStatus(code, description ?? $"Custom Status: {code}"));
        }

        /// <summary>
        /// Helper method to add a status to the dictionary, avoiding redundancy.
        /// </summary>
        private static void AddStatus(IResultStatus status)
        {
            _statuses.TryAdd(status.Code, status);
        }
    }
}
