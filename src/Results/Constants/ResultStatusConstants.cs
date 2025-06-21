// <copyright file="ResultStatusConstants.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

namespace Zentient.Results.Constants
{
    /// <summary>
    /// Provides constant values for common result codes (often HTTP status codes) and their descriptions.
    /// These constants facilitate consistent status reporting within the Zentient.Results library.
    /// </summary>
    internal static class ResultStatusConstants
    {
        /// <summary>Contains integer constants representing standard HTTP status codes.</summary>
        internal static class Code
        {
            // --- 2xx Success Codes ---
            public const int Ok = 200;
            public const int Created = 201;
            public const int Accepted = 202;
            public const int NoContent = 204;

            // --- 3xx Redirection Codes (Optional, but useful if results can represent redirects) ---
            public const int MultipleChoices = 300;
            public const int MovedPermanently = 301;
            public const int Found = 302;
            public const int SeeOther = 303;
            public const int NotModified = 304;
            public const int TemporaryRedirect = 307;
            public const int PermanentRedirect = 308;

            // --- 4xx Client Error Codes ---
            public const int BadRequest = 400;
            public const int Unauthorized = 401; // Corresponds to ErrorCategory.Authentication
            public const int PaymentRequired = 402;
            public const int Forbidden = 403;    // Corresponds to ErrorCategory.Authorization
            public const int NotFound = 404;     // Corresponds to ErrorCategory.NotFound
            public const int MethodNotAllowed = 405;
            public const int NotAcceptable = 406;
            public const int RequestTimeout = 408; // Corresponds to ErrorCategory.Timeout
            public const int Conflict = 409;     // Corresponds to ErrorCategory.Conflict
            public const int Gone = 410;         // Corresponds to ErrorCategory.ResourceGone
            public const int LengthRequired = 411;
            public const int PreconditionFailed = 412;
            public const int PayloadTooLarge = 413;
            public const int UriTooLong = 414;
            public const int UnsupportedMediaType = 415;
            public const int RangeNotSatisfiable = 416;
            public const int ExpectationFailed = 417;
            public const int ImATeapot = 418; // Just for fun, if you want it!
            public const int UnprocessableEntity = 422; // Often used for semantic validation errors (e.g., ErrorCategory.Validation, ErrorCategory.BusinessLogic)
            public const int Locked = 423;
            public const int FailedDependency = 424;
            public const int TooEarly = 425;
            public const int UpgradeRequired = 426;
            public const int PreconditionRequired = 428;
            public const int TooManyRequests = 429; // Corresponds to ErrorCategory.TooManyRequests
            public const int RequestHeaderFieldsTooLarge = 431;
            public const int UnavailableForLegalReasons = 451;

            // --- 5xx Server Error Codes ---
            public const int InternalServerError = 500; // Corresponds to ErrorCategory.InternalServerError, General, Exception
            public const int NotImplemented = 501;      // Corresponds to ErrorCategory.NotImplemented
            public const int BadGateway = 502;
            public const int ServiceUnavailable = 503;  // Corresponds to ErrorCategory.ServiceUnavailable, Network, ExternalService
            public const int GatewayTimeout = 504;
            public const int HttpVersionNotSupported = 505;
            public const int VariantAlsoNegotiates = 506;
            public const int InsufficientStorage = 507;
            public const int LoopDetected = 508;
            public const int NotExtended = 510;
            public const int NetworkAuthenticationRequired = 511;
        }

        /// <summary>Contains string constants representing standard HTTP status descriptions.</summary>
        internal static class Description
        {
            // --- 2xx Success Descriptions ---
            public const string Ok = "OK";
            public const string Created = "Created";
            public const string Accepted = "Accepted";
            public const string NoContent = "No Content";

            // --- 3xx Redirection Descriptions ---
            public const string MultipleChoices = "Multiple Choices";
            public const string MovedPermanently = "Moved Permanently";
            public const string Found = "Found";
            public const string SeeOther = "See Other";
            public const string NotModified = "Not Modified";
            public const string TemporaryRedirect = "Temporary Redirect";
            public const string PermanentRedirect = "Permanent Redirect";

            // --- 4xx Client Error Descriptions ---
            public const string BadRequest = "Bad Request";
            public const string Unauthorized = "Unauthorized";
            public const string PaymentRequired = "Payment Required";
            public const string Forbidden = "Forbidden";
            public const string NotFound = "Not Found";
            public const string MethodNotAllowed = "Method Not Allowed";
            public const string NotAcceptable = "Not Acceptable";
            public const string RequestTimeout = "Request Timeout";
            public const string Conflict = "Conflict";
            public const string Gone = "Gone";
            public const string LengthRequired = "Length Required";
            public const string PreconditionFailed = "Precondition Failed";
            public const string PayloadTooLarge = "Payload Too Large";
            public const string UriTooLong = "URI Too Long";
            public const string UnsupportedMediaType = "Unsupported Media Type";
            public const string RangeNotSatisfiable = "Range Not Satisfiable";
            public const string ExpectationFailed = "Expectation Failed";
            public const string ImATeapot = "I'm a Teapot";
            public const string UnprocessableEntity = "Unprocessable Entity";
            public const string Locked = "Locked";
            public const string FailedDependency = "Failed Dependency";
            public const string TooEarly = "Too Early";
            public const string UpgradeRequired = "Upgrade Required";
            public const string PreconditionRequired = "Precondition Required";
            public const string TooManyRequests = "Too Many Requests";
            public const string RequestHeaderFieldsTooLarge = "Request Header Fields Too Large";
            public const string UnavailableForLegalReasons = "Unavailable For Legal Reasons";

            // --- 5xx Server Error Descriptions ---
            public const string InternalServerError = "Internal Server Error";
            public const string NotImplemented = "Not Implemented";
            public const string BadGateway = "Bad Gateway";
            public const string ServiceUnavailable = "Service Unavailable";
            public const string GatewayTimeout = "Gateway Timeout";
            public const string HttpVersionNotSupported = "HTTP Version Not Supported";
            public const string VariantAlsoNegotiates = "Variant Also Negotiates";
            public const string InsufficientStorage = "Insufficient Storage";
            public const string LoopDetected = "Loop Detected";
            public const string NotExtended = "Not Extended";
            public const string NetworkAuthenticationRequired = "Network Authentication Required";

            // --- Descriptions for specific Problem Details/Custom Error Categories (not direct HTTP statuses) ---
            public const string ProblemDetails = "Problem Details";
            public const string BusinessLogicError = "Business Logic Error";
        }
    }
}
