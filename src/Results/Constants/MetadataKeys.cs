// <copyright file=".cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

namespace Zentient.Results.Constants
{
    /// <summary>Provides constant string keys for common metadata properties in ErrorInfo.</summary>
    public static class MetadataKeys
    {
        /// <summary>Metadata keys for error messages.</summary>
        public const string ExceptionMessage = "exceptionMessage";

        /// <summary>Metadata keys for stack trace.</summary>
        public const string ExceptionStackTrace = "exceptionStackTrace";

        /// <summary>Metadata keys for source of the exception.</summary>
        public const string ExceptionSource = "exceptionSource";

        /// <summary>Metadata keys for the type of the exception.</summary>
        public const string ExceptionType = "exceptionType";

        /// <summary>Metadata keys for the HTTP status code.</summary>
        public const string ValidationErrors = "validationErrors";

        /// <summary>Metadata keys for the original request URI.</summary>
        public const string OriginalRequestUri = "originalRequestUri";
    }
}
