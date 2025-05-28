using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Zentient.Results
{
    /// <summary>
    /// Represents detailed information about an error.
    /// </summary>
    [DataContract]
    public readonly struct ErrorInfo
    {
        /// <summary>Gets the category of the error.</summary>
        [DataMember(Order = 1)]
        [JsonPropertyName("category")]
        [JsonInclude]
        public ErrorCategory Category { get; }

        /// <summary>Gets a specific code for the error (e.g., "USER-001", "EMAIL_INVALID").</summary>
        [DataMember(Order = 2)]
        [JsonPropertyName("code")]
        [JsonInclude]
        public string Code { get; }

        /// <summary>Gets a human-readable message describing the error.</summary>
        [DataMember(Order = 3)]
        [JsonPropertyName("message")]
        [JsonInclude]
        public string Message { get; }

        /// <summary>Gets optional, additional data related to the error (e.g., property name for validation errors).</summary>
        [DataMember(Order = 4)]
        [JsonPropertyName("data")]
        [JsonInclude]
        public object? Data { get; }

        /// <summary>Gets a list of inner errors, useful for hierarchical error reporting (e.g., aggregated validation errors).</summary>
        [DataMember(Order = 5)]
        [JsonPropertyName("innerErrors")]
        [JsonInclude]
        public IReadOnlyList<ErrorInfo> InnerErrors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorInfo"/> struct.
        /// </summary>
        /// <param name="category">The category of the error.</param>
        /// <param name="code">A specific code for the error.</param>
        /// <param name="message">A human-readable message describing the error.</param>
        /// <param name="data">Optional, additional data related to the error.</param>
        /// <param name="innerErrors">A collection of inner errors.</param>
        [JsonConstructor]
        public ErrorInfo(
            ErrorCategory Category,
            string Code,
            string Message,
            object? Data = null,
            IEnumerable<ErrorInfo>? InnerErrors = null)
        {
            this.Category = Category;
            this.Code = Code;
            this.Message = Message;
            this.Data = Data;
            this.InnerErrors = InnerErrors?.ToArray() ?? Array.Empty<ErrorInfo>();
        }

        /// <summary>
        /// Creates an aggregated <see cref="ErrorInfo"/> instance, typically for validation errors
        /// that combine multiple specific errors.
        /// </summary>
        /// <param name="code">The aggregate error code.</param>
        /// <param name="message">The aggregate error message.</param>
        /// <param name="innerErrors">The collection of individual errors that form this aggregate.</param>
        /// <param name="data">Optional, additional data related to the aggregate error.</param>
        /// <returns>A new <see cref="ErrorInfo"/> representing an aggregation of errors.</returns>
        public static ErrorInfo Aggregate(string code, string message, IEnumerable<ErrorInfo> innerErrors, object? data = null) =>
            new(ErrorCategory.Validation, code, message, data, innerErrors);

        /// <summary>
        /// Returns a string representation of the <see cref="ErrorInfo"/>.
        /// </summary>
        /// <returns>A string containing the error code, message, and category.</returns>
        public override string ToString() => $"[{Category}:{Code}] {Message}";
    }
}
