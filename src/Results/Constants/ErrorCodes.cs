// <copyright file="FileName.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zentient.Results.Constants
{
    /// <summary>Provides constant string codes for common error types within the Zentient.Results library.</summary>
    public static class ErrorCodes
    {
        public const string General = "GENERAL_ERROR";
        public const string Validation = "VALIDATION_ERROR";
        public const string NotFound = "RESOURCE_NOT_FOUND";
        public const string Unauthorized = "UNAUTHORIZED_ACCESS";
        public const string Forbidden = "FORBIDDEN_ACCESS";
        public const string Conflict = "RESOURCE_CONFLICT";
        public const string Exception = "UNHANDLED_EXCEPTION";
        public const string Timeout = "OPERATION_TIMEOUT";
        public const string BadGateway = "BAD_GATEWAY";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        public const string NotImplemented = "NOT_IMPLEMENTED";
        public const string Concurrency = "CONCURRENCY_VIOLATION";
        public const string TooManyRequests = "TOO_MANY_REQUESTS";
        public const string ExternalService = "EXTERNAL_SERVICE_ERROR";
        public const string BusinessLogic = "BUSINESS_LOGIC_ERROR";
        public const string Request = "BAD_REQUEST_FORMAT";
        public const string InternalServerError = "INTERNAL_SERVER_ERROR";
        public const string ResourceGone = "RESOURCE_GONE";
    }
}
