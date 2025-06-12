using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zentient.Results
{
    /// <summary>
    /// A custom <see cref="JsonConverterFactory"/> for serializing and deserializing <see cref="IResult"/> and <see cref="IResult{T}"/> types.
    /// Handles both generic and non-generic result types, including error and status information.
    /// </summary>
    public class ResultJsonConverter : JsonConverterFactory
    {
        /// <summary>Determines whether the specified type can be converted by this converter.</summary>
        /// <param name="typeToConvert">The type to check for convertibility.</param>
        /// <returns><c>true</c> if the type implements <see cref="IResult"/>; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IResult).IsAssignableFrom(typeToConvert);
        }

        /// <summary>Creates a <see cref="JsonConverter"/> for the specified type.</summary>
        /// <param name="typeToConvert">The type to create a converter for.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>A <see cref="JsonConverter"/> instance for the type, or <c>null</c> if not supported.</returns>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.IsGenericType &&
                (typeToConvert.GetGenericTypeDefinition() == typeof(Result<>) ||
                 typeToConvert.GetGenericTypeDefinition() == typeof(IResult<>)))
            {
                Type valueType = typeToConvert.GetGenericArguments()[0];
                return (JsonConverter)Activator.CreateInstance(
                    typeof(ResultGenericJsonConverter<>).MakeGenericType(valueType),
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
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

        /// <summary>Handles serialization and deserialization for non-generic <see cref="Result"/> types.</summary>
        private class ResultNonGenericJsonConverter : JsonConverter<Result>
        {
            private readonly JsonSerializerOptions _options;

            /// <summary>Initializes a new instance of the <see cref="ResultNonGenericJsonConverter"/> class.</summary>
            /// <param name="options">The serializer options.</param>
            public ResultNonGenericJsonConverter(JsonSerializerOptions options)
            {
                _options = new JsonSerializerOptions(options);
                _options.Converters.Remove(this);
            }

            /// <inheritdoc/>
            public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected StartObject token.");
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

                        switch (propertyName)
                        {
                            case "status":
                                status = ReadStatus(ref reader, options);
                                break;
                            case "messages":
                                messages = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                                break;
                            case "errors":
                                errors = DeserializeErrorInfoList(ref reader, options);
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
            public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
            {
                if (writer == null)
                {
                    throw new ArgumentNullException(nameof(writer));
                }

                writer.WriteStartObject();
                writer.WritePropertyName(ConvertName(options, nameof(IResult.IsSuccess)));
                writer.WriteBooleanValue(value.IsSuccess);
                writer.WritePropertyName(ConvertName(options, nameof(IResult.IsFailure)));
                writer.WriteBooleanValue(value.IsFailure);
                writer.WritePropertyName(ConvertName(options, nameof(IResult.Status)));
                JsonSerializer.Serialize(writer, value.Status, value.Status.GetType(), _options);

                if (value.Messages.Any())
                {
                    writer.WritePropertyName(ConvertName(options, nameof(IResult.Messages)));
                    JsonSerializer.Serialize(writer, value.Messages, _options);
                }

                if (value.Errors.Any())
                {
                    writer.WritePropertyName(ConvertName(options, nameof(IResult.Errors)));
                    JsonSerializer.Serialize(writer, value.Errors, _options);
                }

                writer.WriteEndObject();
            }

            /// <summary>Reads a <see cref="IResultStatus"/> object from the JSON reader.</summary>
            /// <param name="reader">The JSON reader.</param>
            /// <param name="options">The serializer options.</param>
            /// <returns>The deserialized <see cref="IResultStatus"/>.</returns>
            private IResultStatus? ReadStatus(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    reader.Skip();
                    return null;
                }

                int code = 0;
                string? description = null;

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

                        switch (propertyName)
                        {
                            case "code":
                                code = reader.GetInt32();
                                break;
                            case "description":
                                description = reader.GetString();
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }

                return new ResultStatus(code, description ?? string.Empty);
            }

            /// <summary>Deserializes a list of <see cref="ErrorInfo"/> objects from the JSON reader.</summary>
            /// <param name="reader">The JSON reader.</param>
            /// <param name="options">The serializer options.</param>
            /// <returns>A list of <see cref="ErrorInfo"/> objects, or <c>null</c> if not present.</returns>
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

            /// <summary>Reads a single <see cref="ErrorInfo"/> object from the JSON reader.</summary>
            /// <param name="reader">The JSON reader.</param>
            /// <param name="options">The serializer options.</param>
            /// <returns>The deserialized <see cref="ErrorInfo"/>, or <c>null</c> if not present.</returns>
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
                object? data = null;
                List<ErrorInfo>? innerErrors = null;

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

                        switch (propertyName)
                        {
                            case "category":
                                if (reader.TokenType == JsonTokenType.Number)
                                {
                                    category = (ErrorCategory)reader.GetInt32();
                                }
                                break;
                            case "code":
                                code = reader.GetString();
                                break;
                            case "message":
                                message = reader.GetString();
                                break;
                            case "data":
                                data = JsonSerializer.Deserialize<object>(ref reader, options);
                                break;
                            case "innerErrors":
                                innerErrors = DeserializeErrorInfoList(ref reader, options);
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }
                return new ErrorInfo(category, code ?? string.Empty, message ?? string.Empty, data: data, innerErrors: innerErrors);
            }

            /// <summary>Converts a property name using the serializer's naming policy, if any.</summary>
            /// <param name="options">The serializer options.</param>
            /// <param name="name">The property name to convert.</param>
            /// <returns>The converted property name.</returns>
            private string ConvertName(JsonSerializerOptions options, string name)
            {
                return options.PropertyNamingPolicy?.ConvertName(name) ?? name;
            }
        }

        /// <summary>Handles serialization and deserialization for generic <see cref="Result{TValue}"/> types.</summary>
        /// <typeparam name="TValue">The type of the value encapsulated by the result.</typeparam>
        private class ResultGenericJsonConverter<TValue> : JsonConverter<Result<TValue>>
        {
            private readonly JsonSerializerOptions _options;

            /// <summary>Initializes a new instance of the <see cref="ResultGenericJsonConverter{TValue}"/> class.</summary>
            /// <param name="options">The serializer options.</param>
            public ResultGenericJsonConverter(JsonSerializerOptions options)
            {
                _options = new JsonSerializerOptions(options);
                _options.Converters.Remove(this);
            }

            /// <inheritdoc/>
            public override Result<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected StartObject token.");
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

                        switch (propertyName)
                        {
                            case "value":
                                value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                                break;
                            case "status":
                                status = ReadStatus(ref reader, options);
                                break;
                            case "errors":
                                errors = DeserializeErrorInfoList(ref reader, options);
                                break;
                            case "messages":
                                messages = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }

                if (status == null)
                {
                    status = ResultStatuses.Error;
                    errors ??= new List<ErrorInfo> { new ErrorInfo(ErrorCategory.General, "DeserializationError", "Could not determine result status during deserialization.") };
                }

                return new Result<TValue>(value, status, messages, errors);
            }

            /// <inheritdoc/>
            public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(ConvertName(options, nameof(IResult.IsSuccess)));
                writer.WriteBooleanValue(value.IsSuccess);
                writer.WritePropertyName(ConvertName(options, nameof(IResult.IsFailure)));
                writer.WriteBooleanValue(value.IsFailure);
                writer.WritePropertyName(ConvertName(options, nameof(IResult.Status)));
                JsonSerializer.Serialize(writer, value.Status, value.Status.GetType(), _options);

                if (value.Messages.Any())
                {
                    writer.WritePropertyName(ConvertName(options, nameof(IResult.Messages)));
                    JsonSerializer.Serialize(writer, value.Messages, _options);
                }

                if (value.Errors.Any())
                {
                    writer.WritePropertyName(ConvertName(options, nameof(IResult.Errors)));
                    JsonSerializer.Serialize(writer, value.Errors, _options);
                }

                writer.WritePropertyName(ConvertName(options, nameof(IResult<TValue>.Value)));
                JsonSerializer.Serialize(writer, value.Value, _options);

                writer.WriteEndObject();
            }

            /// <summary>Reads a <see cref="IResultStatus"/> object from the JSON reader.</summary>
            /// <param name="reader">The JSON reader.</param>
            /// <param name="options">The serializer options.</param>
            /// <returns>The deserialized <see cref="IResultStatus"/>.</returns>
            private IResultStatus? ReadStatus(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    reader.Skip();
                    return null;
                }

                int code = 0;
                string? description = null;

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

                        switch (propertyName)
                        {
                            case "code":
                                code = reader.GetInt32();
                                break;
                            case "description":
                                description = reader.GetString();
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }

                return new ResultStatus(code, description ?? string.Empty);
            }

            /// <summary>Deserializes a list of <see cref="ErrorInfo"/> objects from the JSON reader.</summary>
            /// <param name="reader">The JSON reader.</param>
            /// <param name="options">The serializer options.</param>
            /// <returns>A list of <see cref="ErrorInfo"/> objects, or <c>null</c> if not present.</returns>
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

            /// <summary>Reads a single <see cref="ErrorInfo"/> object from the JSON reader.</summary>
            /// <param name="reader">The JSON reader.</param>
            /// <param name="options">The serializer options.</param>
            /// <returns>The deserialized <see cref="ErrorInfo"/>, or <c>null</c> if not present.</returns>
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
                object? data = null;
                List<ErrorInfo>? innerErrors = null;

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

                        switch (propertyName)
                        {
                            case "category":
                                if (reader.TokenType == JsonTokenType.Number)
                                {
                                    category = (ErrorCategory)reader.GetInt32();
                                }
                                break;
                            case "code":
                                code = reader.GetString();
                                break;
                            case "message":
                                message = reader.GetString();
                                break;
                            case "data":
                                data = JsonSerializer.Deserialize<object>(ref reader, options);
                                break;
                            case "innerErrors":
                                innerErrors = DeserializeErrorInfoList(ref reader, options);
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }
                return new ErrorInfo(category, code ?? string.Empty, message ?? string.Empty, data: data, innerErrors: innerErrors);
            }

            /// <summary>Converts a property name using the serializer's naming policy, if any.</summary>
            /// <param name="options">The serializer options.</param>
            /// <param name="name">The property name to convert.</param>
            /// <returns>The converted property name.</returns>
            private string ConvertName(JsonSerializerOptions options, string name)
                => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
        }
    }
}
