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
        public const string None = "None";
        public const string General = "GENERAL_ERROR";
        public const string Validation = "VALIDATION_ERROR";
        public const string BadRequest = "BAD_REQUEST_ERROR";
        public const string Authentication = "AUTHENTICATION_ERROR";
        public const string Authorization = "AUTHORIZATION_ERROR";
        public const string NotFound = "RESOURCE_NOT_FOUND_ERROR";
        public const string Conflict = "RESOURCE_CONFLICT_ERROR";
        public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION_ERROR";
        public const string NotImplemented = "NOT_IMPLEMENTED_ERROR";
        public const string InternalError = "INTERNAL_ERROR";
        public const string ExternalDependency = "EXTERNAL_DEPENDENCY_ERROR";
        public const string Timeout = "OPERATION_TIMEOUT_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE_ERROR";
        public const string SecurityViolation = "SECURITY_VIOLATION_ERROR";
    }
}
