// <copyright file="ResultJsonConverter.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Zentient.Results.Constants;

namespace Zentient.Results.Serialization
{
    /// <summary>
    /// A custom JSON converter factory for <see cref="Result"/> and <see cref="Result{T}"/> types.
    /// This allows proper serialization/deserialization of these immutable classes using
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
        public override bool CanConvert(Type typeToConvert) =>
            typeof(IResult).IsAssignableFrom(typeToConvert);

        /// <inheritdoc/>
        /// <summary>
        /// Creates a <see cref="JsonConverter"/> for the specified <paramref name="typeToConvert"/>.
        /// Delegates to a non-generic or generic internal converter based on the type.
        /// </summary>
        /// <param name="typeToConvert">The type for which to create the converter.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> being used for serialization.</param>
        /// <returns>A new <see cref="JsonConverter"/> instance for the specified type, or <c>null</c> if not convertible.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="typeToConvert"/> or <paramref name="options"/> is <c>null</c>.</exception>
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
            [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "JSON property names are typically camelCase.")]
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
                            case JsonConstants.Result.Status:
                                status = ResultJsonConverter.ReadStatus(ref reader, _options);
                                break;
                            case JsonConstants.Result.Messages:
                                messages = JsonSerializer.Deserialize<List<string>>(ref reader, _options);
                                break;
                            case JsonConstants.Result.Errors:
                                errors = ResultJsonConverter.DeserializeErrorInfoList(ref reader, _options);
                                break;
                            case JsonConstants.Result.IsSuccess:
                            case JsonConstants.Result.IsFailure:
                            case JsonConstants.Result.ErrorMessage:
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

                writer.WriteBoolean(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.IsSuccess), value.IsSuccess);
                writer.WriteBoolean(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.IsFailure), value.IsFailure);
                writer.WritePropertyName(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.Status));
                JsonSerializer.Serialize(writer, value.Status, value.Status.GetType(), _options);

                if (value.Messages != null && value.Messages.Any())
                {
                    writer.WritePropertyName(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.Messages));
                    JsonSerializer.Serialize(writer, value.Messages, _options);
                }

                if (value.IsFailure && value.Errors != null && value.Errors.Any())
                {
                    writer.WritePropertyName(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.Errors));
                    JsonSerializer.Serialize(writer, value.Errors, _options);
                }

                if (value.ErrorMessage != null)
                {
                    writer.WriteString(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.ErrorMessage), value.ErrorMessage);
                }

                writer.WriteEndObject();
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
            [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "JSON property names are typically camelCase.")]
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
                            case JsonConstants.Result.Value:
                                value = JsonSerializer.Deserialize<TValue>(ref reader, _options);
                                break;
                            case JsonConstants.Result.Status:
                                status = ResultJsonConverter.ReadStatus(ref reader, _options);
                                break;
                            case JsonConstants.Result.Errors:
                                errors = ResultJsonConverter.DeserializeErrorInfoList(ref reader, _options);
                                break;
                            case JsonConstants.Result.Messages:
                                messages = JsonSerializer.Deserialize<List<string>>(ref reader, _options);
                                break;
                            case JsonConstants.Result.IsSuccess:
                            case JsonConstants.Result.IsFailure:
                            case JsonConstants.Result.ErrorMessage:
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

                writer.WritePropertyName(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.Value));

                if (value.Value == null && (typeof(TValue).IsClass || Nullable.GetUnderlyingType(typeof(TValue)) != null))
                {
                    writer.WriteNullValue();
                }
                else
                {
                    JsonSerializer.Serialize(writer, value.Value, _options);
                }

                writer.WriteBoolean(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.IsSuccess), value.IsSuccess);
                writer.WriteBoolean(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.IsFailure), value.IsFailure);

                writer.WritePropertyName(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.Status));
                JsonSerializer.Serialize(writer, value.Status, value.Status.GetType(), _options);

                if (value.Messages != null && value.Messages.Any())
                {
                    writer.WritePropertyName(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.Messages));
                    JsonSerializer.Serialize(writer, value.Messages, _options);
                }

                if (value.IsFailure && value.Errors != null && value.Errors.Any())
                {
                    writer.WritePropertyName(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.Errors));
                    JsonSerializer.Serialize(writer, value.Errors, _options);
                }

                if (value.ErrorMessage != null)
                {
                    writer.WriteString(ResultJsonConverter.ConvertName(_options, JsonConstants.Result.ErrorMessage), value.ErrorMessage);
                }

                writer.WriteEndObject();
            }
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

            // Using JsonSerializer.Deserialize for ResultStatusInternal directly
            // This relies on ResultStatusInternal having public setters or init properties
            // and a parameterless constructor, or a suitable [JsonConstructor].
            return JsonSerializer.Deserialize<ResultStatusInternal>(ref reader, options);
        }

        /// <summary>
        /// Deserializes a list of <see cref="ErrorInfo"/> from a JSON array.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
        /// <returns>A <see cref="List{ErrorInfo}"/> deserialized from the JSON array, or <c>null</c> if the token is not a StartArray.</returns>
        private static List<ErrorInfo>? DeserializeErrorInfoList(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            // The existing `Deserialize<List<ErrorInfo>>` call is already correct and
            // will leverage `System.Text.Json`'s default deserialization for ErrorInfo.
            // This is the cleanest way given ErrorInfo's `init` properties.
            return JsonSerializer.Deserialize<List<ErrorInfo>>(ref reader, options);
        }

        /// <summary>
        /// Reads a single <see cref="ErrorInfo"/> object from the JSON.
        /// This method is now simplified as `DeserializeErrorInfoList` directly handles `List<ErrorInfo>`.
        /// However, if you explicitly needed to deserialize a *single* `ErrorInfo` object at a time,
        /// you would use `JsonSerializer.Deserialize<ErrorInfo>(ref reader, options);`
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use.</param>
        /// <returns>A new <see cref="ErrorInfo"/> instance deserialized from the JSON, or <c>null</c> if not found or invalid.</returns>
        private static ErrorInfo? ReadErrorInfo(ref Utf8JsonReader reader, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<ErrorInfo>(ref reader, options);


        /// <summary>
        /// Converts a property name based on the specified JSON naming policy.
        /// </summary>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> containing the naming policy.</param>
        /// <param name="name">The original property name.</param>
        /// <returns>The converted property name.</returns>
        private static string ConvertName(JsonSerializerOptions options, string name) =>
            options.PropertyNamingPolicy?.ConvertName(name) ?? name;

        /// <summary>
        /// Internal DTO to deserialize <see cref="IResultStatus"/> as a concrete type.
        /// This is necessary because <see cref="IResultStatus"/> is an interface.
        /// </summary>
        private sealed class ResultStatusInternal : IResultStatus
        {
            /// <inheritdoc/>
            public int Code { get; init; }

            /// <inheritdoc/>
            public string Description { get; init; } = string.Empty;
        }
    }
}
