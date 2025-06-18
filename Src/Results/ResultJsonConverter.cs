using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zentient.Results.Serialization
{
    /// <summary>
    /// A custom JSON converter factory for <see cref="Result"/> and <see cref="Result{T}"/> types.
    /// This allows proper serialization/deserialization of these immutable structs using
    /// System.Text.Json, handling their internal structure (success/failure, value, errors, messages, status).
    /// </summary>
    public sealed class ResultJsonConverter : JsonConverterFactory
    {
        /// <inheritdoc/>
        /// <summary>
        /// Determines whether the <paramref name="typeToConvert"/> is a <see cref="Result"/>
        /// or <see cref="Result{T}"/> type and thus can be converted by this factory.
        /// </summary>
        /// <param name="typeToConvert">The type to check for convertibility.</param>
        /// <returns><c>true</c> if the type can be converted; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IResult).IsAssignableFrom(typeToConvert);
        }

        /// <inheritdoc/>
        /// <summary>
        /// Creates a <see cref="JsonConverter"/> for the specified <paramref name="typeToConvert"/>.
        /// Delegates to a non-generic or generic internal converter based on the type.
        /// </summary>
        /// <param name="typeToConvert">The type for which to create the converter.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> being used for serialization.</param>
        /// <returns>A new <see cref="JsonConverter"/> instance for the specified type, or <c>null</c> if not convertible.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="typeToConvert"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an appropriate generic converter cannot be created.</exception>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(typeToConvert, nameof(typeToConvert));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            if (typeToConvert.IsGenericType &&
            (typeToConvert.GetGenericTypeDefinition() == typeof(Result<>) ||
             typeToConvert.GetGenericTypeDefinition() == typeof(IResult<>)))
            {
                Type valueType = typeToConvert.GetGenericArguments()[0];
                Type converterType = typeof(ResultGenericJsonConverter<>).MakeGenericType(valueType);
                return (JsonConverter)Activator.CreateInstance(
                converterType,
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null)!;
            }

            if (typeToConvert == typeof(Result) || typeToConvert == typeof(IResult))
            {
                return new ResultNonGenericJsonConverter(options);
            }

            return null;
        }

        /// <summary>
        /// Internal converter for the non-generic <see cref="Result"/> type.
        /// Handles the serialization and deserialization of <see cref="Result"/> instances.
        /// </summary>
        private sealed class ResultNonGenericJsonConverter : JsonConverter<Result>
        {
            private readonly JsonSerializerOptions _options;

            /// <summary>
            /// Initializes a new instance of the <see cref="ResultNonGenericJsonConverter"/> class.
            /// </summary>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> being used for serialization.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is <c>null</c>.</exception>
            public ResultNonGenericJsonConverter(JsonSerializerOptions options)
            {
                _options = new JsonSerializerOptions(options);

                _options.Converters.Remove(this);
            }

            /// <inheritdoc/>
            /// <summary>
            /// Reads a <see cref="Result"/> object from the JSON.
            /// </summary>
            /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
            /// <param name="typeToConvert">The type of the object to convert.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <returns>A new <see cref="Result"/> instance deserialized from the JSON.</returns>
            /// <exception cref="JsonException">Thrown if the JSON is not in the expected format.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
            public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected StartObject token for Result.");
                }

                IResultStatus? status = null;
                List<ErrorInfo>? errors = null;
                List<string>? messages = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString()!;
                        reader.Read();

                        switch (propertyName.ToLowerInvariant())
                        {
                            case "status":
                                status = ResultNonGenericJsonConverter.ReadStatus(ref reader, options);
                                break;
                            case "messages":
                                messages = JsonSerializer.Deserialize<List<string>>(ref reader, _options);
                                break;
                            case "errors":
                                errors = DeserializeErrorInfoList(ref reader, _options);
                                break;
                            case "issuccess":
                            case "isfailure":
                            case "errormessage":
                                reader.Skip();
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }

                return new Result(status ?? ResultStatuses.Error, messages, errors);
            }

            /// <inheritdoc/>
            /// <summary>
            /// Writes a <see cref="Result"/> object to the JSON.
            /// </summary>
            /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
            /// <param name="value">The <see cref="Result"/> instance to serialize.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="writer"/> is <c>null</c>.</exception>
            public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
            {
                ArgumentNullException.ThrowIfNull(writer, nameof(writer));

                writer.WriteStartObject();

                writer.WritePropertyName(ResultNonGenericJsonConverter.ConvertName(_options, nameof(IResult.IsSuccess)));
                writer.WriteBooleanValue(value.IsSuccess);

                writer.WritePropertyName(ResultNonGenericJsonConverter.ConvertName(_options, nameof(IResult.IsFailure)));
                writer.WriteBooleanValue(value.IsFailure);

                writer.WritePropertyName(ResultNonGenericJsonConverter.ConvertName(_options, nameof(IResult.Status)));
                JsonSerializer.Serialize(writer, value.Status, value.Status.GetType(), _options);

                if (value.Messages.Any())
                {
                    writer.WritePropertyName(ResultNonGenericJsonConverter.ConvertName(_options, nameof(IResult.Messages)));
                    JsonSerializer.Serialize(writer, value.Messages, _options);
                }

                if (value.Errors.Any())
                {
                    writer.WritePropertyName(ResultNonGenericJsonConverter.ConvertName(_options, nameof(IResult.Errors)));
                    JsonSerializer.Serialize(writer, value.Errors, _options);
                }

                if (value.ErrorMessage != null)
                {
                    writer.WritePropertyName(ResultNonGenericJsonConverter.ConvertName(_options, nameof(IResult.ErrorMessage)));
                    writer.WriteStringValue(value.ErrorMessage);
                }

                writer.WriteEndObject();
            }

            /// <summary>
            /// Reads an <see cref="IResultStatus"/> object from the JSON.
            /// </summary>
            /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <returns>A new <see cref="IResultStatus"/> instance deserialized from the JSON, or <c>null</c> if not found or invalid.</returns>
            private static IResultStatus? ReadStatus(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    reader.Skip();
                    return null;
                }

                int code = 0;
                string? description = null;

                using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("code", out JsonElement codeElement))
                    {
                        code = codeElement.GetInt32();
                    }
                    if (root.TryGetProperty("description", out JsonElement descriptionElement))
                    {
                        description = descriptionElement.GetString();
                    }
                }
                return ResultStatuses.GetStatus(code, description ?? string.Empty);
            }

            /// <summary>
            /// Deserializes a list of <see cref="ErrorInfo"/> from a JSON array.
            /// </summary>
            /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <returns>A <see cref="List{ErrorInfo}"/> deserialized from the JSON array, or <c>null</c> if the token is not a StartArray.</returns>
            private List<ErrorInfo>? DeserializeErrorInfoList(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    reader.Skip();
                    return null;
                }

                var errorList = new List<ErrorInfo>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }
                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        ErrorInfo? errorInfo = ReadErrorInfo(ref reader, options);
                        if (errorInfo.HasValue)
                        {
                            errorList.Add(errorInfo.Value);
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                return errorList;
            }

            /// <summary>
            /// Reads a single <see cref="ErrorInfo"/> object from the JSON.
            /// </summary>
            /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <returns>A new <see cref="ErrorInfo"/> instance deserialized from the JSON, or <c>null</c> if not found or invalid.</returns>
            /// <exception cref="JsonException">Thrown if the JSON is malformed for an ErrorInfo object.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
            private ErrorInfo? ReadErrorInfo(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    reader.Skip();
                    return null;
                }

                ErrorCategory category = default;
                string? code = null;
                string? message = null;
                string? detail = null;
                object? data = null;
                List<ErrorInfo>? innerErrors = null;
                Dictionary<string, object?> extensions = new Dictionary<string, object?>();

                using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                {
                    JsonElement root = doc.RootElement;
                    foreach (JsonProperty property in root.EnumerateObject())
                    {
                        switch (property.Name.ToLowerInvariant())
                        {
                            case "category":
                                if (property.Value.ValueKind == JsonValueKind.Number)
                                {
                                    category = (ErrorCategory)property.Value.GetInt32();
                                }
                                else if (property.Value.ValueKind == JsonValueKind.String)
                                {
                                    if (Enum.TryParse(property.Value.GetString(), true, out ErrorCategory parsedCategory))
                                    {
                                        category = parsedCategory;
                                    }
                                }
                                break;
                            case "code":
                                code = property.Value.GetString();
                                break;
                            case "message":
                                message = property.Value.GetString();
                                break;
                            case "detail":
                                detail = property.Value.GetString();
                                break;
                            case "data":
                                data = JsonSerializer.Deserialize<object>(property.Value.GetRawText(), _options);
                                break;
                            case "extensions":
                                extensions = JsonSerializer.Deserialize<Dictionary<string, object?>>(property.Value.GetRawText(), _options) ?? new Dictionary<string, object?>();
                                break;
                            case "innererrors":
                                innerErrors = JsonSerializer.Deserialize<List<ErrorInfo>>(property.Value.GetRawText(), _options);
                                break;
                            default:
                                extensions[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText(), _options);
                                break;
                        }
                    }
                }

                return new ErrorInfo(
                category,
                code ?? string.Empty,
                message ?? string.Empty,
                detail,
                data,
                extensions: extensions.Count == 0 ? extensions : null,
                innerErrors: innerErrors
                );
            }

            /// <summary>
            /// Converts a property name based on the specified JSON naming policy.
            /// </summary>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> containing the naming policy.</param>
            /// <param name="name">The original property name.</param>
            /// <returns>The converted property name.</returns>
            private static string ConvertName(JsonSerializerOptions options, string name)
            {
                return options.PropertyNamingPolicy?.ConvertName(name) ?? name;
            }
        }

        /// <summary>
        /// Internal converter for the generic <see cref="Result{T}"/> type.
        /// Handles the serialization and deserialization of <see cref="Result{T}"/> instances.
        /// </summary>
        /// <typeparam name="TValue">The type of the value held by the result.</typeparam>
        private sealed class ResultGenericJsonConverter<TValue> : JsonConverter<Result<TValue>>
        {
            private readonly JsonSerializerOptions _options;

            /// <summary>
            /// Initializes a new instance of the <see cref="ResultGenericJsonConverter{TValue}"/> class.
            /// </summary>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> being used for serialization.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is <c>null</c>.</exception>
            public ResultGenericJsonConverter(JsonSerializerOptions options)
            {
                _options = new JsonSerializerOptions(options);
                _options.Converters.Remove(this);
            }

            /// <inheritdoc/>
            /// <summary>
            /// Reads a <see cref="Result{TValue}"/> object from the JSON.
            /// </summary>
            /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
            /// <param name="typeToConvert">The type of the object to convert.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <returns>A new <see cref="Result{TValue}"/> instance deserialized from the JSON.</returns>
            /// <exception cref="JsonException">Thrown if the JSON is not in the expected format.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
            public override Result<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected StartObject token for Result<TValue>.");
                }

                TValue? value = default;
                IResultStatus? status = null;
                List<ErrorInfo>? errors = null;
                List<string>? messages = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString()!;
                        reader.Read();

                        switch (propertyName.ToLowerInvariant())
                        {
                            case "value":
                                value = JsonSerializer.Deserialize<TValue>(ref reader, _options);
                                break;
                            case "status":
                                status = ReadStatus(ref reader, _options);
                                break;
                            case "errors":
                                errors = DeserializeErrorInfoList(ref reader, _options);
                                break;
                            case "messages":
                                messages = JsonSerializer.Deserialize<List<string>>(ref reader, _options);
                                break;
                            case "issuccess":
                            case "isfailure":
                            case "errormessage":
                                reader.Skip();
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }

                return new Result<TValue>(value, status ?? ResultStatuses.Error, messages, errors);
            }

            /// <inheritdoc/>
            /// <summary>
            /// Writes a <see cref="Result{TValue}"/> object to the JSON.
            /// </summary>
            /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
            /// <param name="value">The <see cref="Result{TValue}"/> instance to serialize.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="writer"/> is <c>null</c>.</exception>
            public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options)
            {
                ArgumentNullException.ThrowIfNull(writer, nameof(writer));

                writer.WriteStartObject();

                if (value.Value is not null || typeof(TValue).IsValueType && Nullable.GetUnderlyingType(typeof(TValue)) == null)
                {
                    writer.WritePropertyName(ConvertName(_options, nameof(IResult<TValue>.Value)));
                    JsonSerializer.Serialize(writer, value.Value, _options);
                }

                writer.WritePropertyName(ConvertName(_options, nameof(IResult.IsSuccess)));
                writer.WriteBooleanValue(value.IsSuccess);

                writer.WritePropertyName(ConvertName(_options, nameof(IResult.IsFailure)));
                writer.WriteBooleanValue(value.IsFailure);

                writer.WritePropertyName(ConvertName(_options, nameof(IResult.Status)));
                JsonSerializer.Serialize(writer, value.Status, value.Status.GetType(), _options);

                if (value.Messages.Any())
                {
                    writer.WritePropertyName(ConvertName(_options, nameof(IResult.Messages)));
                    JsonSerializer.Serialize(writer, value.Messages, _options);
                }

                if (value.Errors.Any())
                {
                    writer.WritePropertyName(ConvertName(_options, nameof(IResult.Errors)));
                    JsonSerializer.Serialize(writer, value.Errors, _options);
                }

                if (value.ErrorMessage != null)
                {
                    writer.WritePropertyName(ConvertName(_options, nameof(IResult.ErrorMessage)));
                    writer.WriteStringValue(value.ErrorMessage);
                }

                writer.WriteEndObject();
            }

            /// <summary>
            /// Reads an <see cref="IResultStatus"/> object from the JSON.
            /// </summary>
            /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <returns>A new <see cref="IResultStatus"/> instance deserialized from the JSON, or <c>null</c> if not found or invalid.</returns>
            private static IResultStatus? ReadStatus(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    reader.Skip();
                    return null;
                }

                int code = 0;
                string? description = null;

                using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("code", out JsonElement codeElement))
                    {
                        code = codeElement.GetInt32();
                    }
                    if (root.TryGetProperty("description", out JsonElement descriptionElement))
                    {
                        description = descriptionElement.GetString();
                    }
                }

                return ResultStatuses.GetStatus(code, description ?? string.Empty);
            }

            /// <summary>
            /// Deserializes a list of <see cref="ErrorInfo"/> from a JSON array.
            /// </summary>
            /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <returns>A <see cref="List{ErrorInfo}"/> deserialized from the JSON array, or <c>null</c> if the token is not a StartArray.</returns>
            private List<ErrorInfo>? DeserializeErrorInfoList(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    reader.Skip();
                    return null;
                }

                var errorList = new List<ErrorInfo>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }
                    if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        ErrorInfo? errorInfo = ReadErrorInfo(ref reader, options);
                        if (errorInfo.HasValue)
                        {
                            errorList.Add(errorInfo.Value);
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                return errorList;
            }

            /// <summary>
            /// Reads a single <see cref="ErrorInfo"/> object from the JSON.
            /// </summary>
            /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
            /// <returns>A new <see cref="ErrorInfo"/> instance deserialized from the JSON, or <c>null</c> if not found or invalid.</returns>
            /// <exception cref="JsonException">Thrown if the JSON is malformed for an ErrorInfo object.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
            private ErrorInfo? ReadErrorInfo(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    reader.Skip();
                    return null;
                }

                ErrorCategory category = default;
                string? code = null;
                string? message = null;
                string? detail = null;
                object? data = null;
                List<ErrorInfo>? innerErrors = null;
                Dictionary<string, object?> extensions = new Dictionary<string, object?>();

                using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                {
                    JsonElement root = doc.RootElement;
                    foreach (JsonProperty property in root.EnumerateObject())
                    {
                        switch (property.Name.ToLowerInvariant())
                        {
                            case JsonConstants.ErrorInfo.Category:
                                if (property.Value.ValueKind == JsonValueKind.Number)
                                {
                                    category = (ErrorCategory)property.Value.GetInt32();
                                }
                                else if (property.Value.ValueKind == JsonValueKind.String)
                                {
                                    if (Enum.TryParse(property.Value.GetString(), true, out ErrorCategory parsedCategory))
                                    {
                                        category = parsedCategory;
                                    }
                                }
                                break;
                            case JsonConstants.ErrorInfo.Code:
                                code = property.Value.GetString();
                                break;
                            case JsonConstants.ErrorInfo.Message:
                                message = property.Value.GetString();
                                break;
                            case JsonConstants.ErrorInfo.Detail:
                                detail = property.Value.GetString();
                                break;
                            case JsonConstants.ErrorInfo.Data:
                                data = JsonSerializer.Deserialize<object>(property.Value.GetRawText(), _options);
                                break;
                            case JsonConstants.ErrorInfo.Extensions:
                                extensions = JsonSerializer.Deserialize<Dictionary<string, object?>>(property.Value.GetRawText(), _options) ?? new Dictionary<string, object?>();
                                break;
                            case JsonConstants.ErrorInfo.InnerErrors:
                                innerErrors = JsonSerializer.Deserialize<List<ErrorInfo>>(property.Value.GetRawText(), _options);
                                break;
                            default:
                                extensions[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText(), _options);
                                break;
                        }
                    }
                }

                return new ErrorInfo(
                category,
                code ?? string.Empty,
                message ?? string.Empty,
                detail,
                data,
                extensions: extensions.Count == 0 ? extensions : null,
                innerErrors: innerErrors
                );
            }

            /// <summary>
            /// Converts a property name based on the specified JSON naming policy.
            /// </summary>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> containing the naming policy.</param>
            /// <param name="name">The original property name.</param>
            /// <returns>The converted property name.</returns>
            private static string ConvertName(JsonSerializerOptions options, string name)
            {
                return options.PropertyNamingPolicy?.ConvertName(name) ?? name;
            }
        }

        /// <summary>
        /// Internal DTO to deserialize <see cref="IResultStatus"/> as a concrete type.
        /// This is necessary because <see cref="IResultStatus"/> is an interface.
        /// </summary>
        private sealed class ResultStatusInternal : IResultStatus
        {
            /// <inheritdoc/>
            public int Code { get; set; }

            /// <inheritdoc/>
            public string Description { get; set; } = string.Empty;
        }
    }
}
