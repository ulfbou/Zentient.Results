// <copyright file="JsonConstants.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

namespace Zentient.Results.Constants
{
    /// <summary>Constants for JSON serialization of results and error information.</summary>
    internal static class JsonConstants
    {
        /// <summary>Constants for JSON serialization of error categories.</summary>
        internal static class ErrorInfo
        {
            public const string Category = "category";
            public const string Code = "code";
            public const string Message = "message";
            public const string Detail = "detail";
            public const string Metadata = "metadata";
            public const string Extensions = "extensions";
            public const string InnerErrors = "innererrors";
        }

        /// <summary>Constants for JSON serialization of result objects.</summary>
        internal static class Result
        {
            public const string Value = "value";
            public const string Status = "status";
            public const string Messages = "messages";
            public const string Errors = "errors";
            public const string IsSuccess = "isSuccess";
            public const string IsFailure = "isFailure";
            public const string ErrorMessage = "errorMessage";
        }
    }
}
