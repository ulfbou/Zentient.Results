// <copyright file="ResultJsonConverterTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using FluentAssertions;

using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

using Zentient.Results.Serialization;

namespace Zentient.Results.Tests
{
    public class ResultJsonConverterTests
    {
        private static readonly ErrorInfo SampleError = new ErrorInfo(ErrorCategory.General, "ERR", "Error message");
        private static readonly ErrorInfo[] SampleErrors = { SampleError, new(ErrorCategory.Validation, "VAL", "Validation failed") };

        private class DummyStatus : IResultStatus
        {
            public int Code { get; set; }
            public string Description { get; set; } = string.Empty;
            public override string ToString() => $"{Code} {Description}";
        }

        private static IResultStatus SuccessStatus => new DummyStatus { Code = 200, Description = "OK" };
        private static IResultStatus CreatedStatus => new DummyStatus { Code = 201, Description = "Created" };
        private static IResultStatus NoContentStatus => new DummyStatus { Code = 204, Description = "No Content" };
        private static IResultStatus BadRequestStatus => new DummyStatus { Code = 400, Description = "Bad Request" };
        private static IResultStatus ErrorStatus => new DummyStatus { Code = 500, Description = "Internal Server Error" };

        private static JsonSerializerOptions GetOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            options.Converters.Add(new ResultJsonConverter());
            return options;
        }

        [Fact]
        public void CanConvert_ReturnsTrue_ForIResultAndResultTypes()
        {
            var converter = new ResultJsonConverter();
            converter.CanConvert(typeof(Result)).Should().BeTrue();
            converter.CanConvert(typeof(Result<int>)).Should().BeTrue();
            converter.CanConvert(typeof(IResult)).Should().BeTrue();
            converter.CanConvert(typeof(IResult<int>)).Should().BeTrue();
            converter.CanConvert(typeof(string)).Should().BeFalse();
        }

        [Fact]
        public void Serialize_Successful_NonGeneric_Result()
        {
            var result = Result.Success(SuccessStatus, ["ok"]);
            var json = JsonSerializer.Serialize((Result)result, GetOptions());
            json.Should().Contain("\"isSuccess\":true");
            json.Should().Contain("\"isFailure\":false");
            json.Should().Contain("\"status\"");
            json.Should().Contain("\"messages\"");
            json.Should().Contain("ok");
            json.Should().NotContain("\"errors\"");
        }

        [Fact]
        public void Serialize_Failure_NonGeneric_Result()
        {
            var result = Result.Failure(SampleError, BadRequestStatus);
            var json = JsonSerializer.Serialize(result, GetOptions());
            json.Should().Contain("\"isSuccess\":false");
            json.Should().Contain("\"isFailure\":true");
            json.Should().Contain("\"status\"");
            json.Should().Contain("\"errors\"");
            json.Should().Contain("Error message");
            json.Should().Contain("ERR");
        }

        [Fact]
        public void Serialize_Successful_Generic_Result()
        {
            var result = Result<int>.Success(42, "yay");
            var json = JsonSerializer.Serialize(result, GetOptions());
            json.Should().Contain("\"isSuccess\":true");
            json.Should().Contain("\"isFailure\":false");
            json.Should().Contain("\"status\"");
            json.Should().Contain("\"value\":42");
            json.Should().Contain("\"messages\"");
            json.Should().Contain("yay");
            json.Should().NotContain("\"errors\"");
        }

        [Fact]
        public void Serialize_Failure_Generic_Result()
        {
            var result = Result<int>.Failure(0, SampleError, BadRequestStatus);
            var json = JsonSerializer.Serialize(result, GetOptions());
            json.Should().Contain("\"isSuccess\":false");
            json.Should().Contain("\"isFailure\":true");
            json.Should().Contain("\"status\"");
            json.Should().Contain("\"errors\"");
            json.Should().Contain("Error message");
            json.Should().Contain("ERR");
        }

        [Fact]
        public void Serialize_Generic_Result_With_Messages_And_Errors()
        {
            var result = new Result<int>(123, BadRequestStatus, new[] { "msg1", "msg2" }, SampleErrors);
            var json = JsonSerializer.Serialize(result, GetOptions());
            json.Should().Contain("\"messages\"");
            json.Should().Contain("msg1");
            json.Should().Contain("msg2");
            json.Should().Contain("\"errors\"");
            json.Should().Contain("Error message");
            json.Should().Contain("Validation failed");
        }

        [Fact]
        public void Serialize_NonGeneric_Result_With_Error_Property()
        {
            var result = Result.Failure(SampleError, BadRequestStatus);
            var json = JsonSerializer.Serialize(result, GetOptions());
            json.Should().Contain("Error message");
        }

        [Fact]
        public void Serialize_Generic_Result_With_Error_Property()
        {
            var result = Result<int>.Failure(0, SampleError, BadRequestStatus);
            var json = JsonSerializer.Serialize(result, GetOptions());
            json.Should().Contain("Error message");
        }

        [Fact]
        public void Deserialize_Successful_NonGeneric_Result()
        {
            var original = Result.Success("ok");
            var json = JsonSerializer.Serialize(original, GetOptions());
            var deserialized = JsonSerializer.Deserialize<Result>(json, GetOptions());
            deserialized.Should().NotBeNull();
            deserialized.IsSuccess.Should().BeTrue();
            deserialized.Messages.Should().Contain("ok");
            deserialized.Status.Code.Should().Be(200);
        }

        [Fact]
        public void Deserialize_Failure_NonGeneric_Result()
        {
            var original = Result.Failure(SampleError, BadRequestStatus);
            var json = JsonSerializer.Serialize(original, GetOptions());
            var deserialized = JsonSerializer.Deserialize<Result>(json, GetOptions());
            deserialized.Should().NotBeNull();
            deserialized.IsFailure.Should().BeTrue();
            deserialized.Errors.Should().ContainSingle();
            deserialized.Errors[0].Message.Should().Be("Error message");
            deserialized.Status.Code.Should().Be(400);
        }

        [Fact]
        public void Deserialize_Successful_Generic_Result()
        {
            var original = Result<string>.Success("abc", "ok");
            var json = JsonSerializer.Serialize(original, GetOptions());
            var deserialized = JsonSerializer.Deserialize<Result<string>>(json, GetOptions());
            deserialized.Should().NotBeNull();
            deserialized.IsSuccess.Should().BeTrue();
            deserialized.Value.Should().Be("abc");
            deserialized.Messages.Should().Contain("ok");
        }

        [Fact]
        public void Deserialize_Failure_Generic_Result()
        {
            var original = Result<int>.Failure(0, SampleError, BadRequestStatus);
            var json = JsonSerializer.Serialize(original, GetOptions());
            var deserialized = JsonSerializer.Deserialize<Result<int>>(json, GetOptions());
            deserialized.Should().NotBeNull();
            deserialized.IsFailure.Should().BeTrue();
            deserialized.Errors.Should().ContainSingle();
            deserialized.Errors[0].Message.Should().Be("Error message");
            deserialized.Status.Code.Should().Be(400);
        }

        [Fact]
        public void Deserialize_Handles_Missing_Status_As_Error()
        {
            var json = "{\"isSuccess\":false,\"isFailure\":true,\"errors\":[{\"category\":0,\"code\":\"ERR\",\"message\":\"Error message\"}]}";
            var deserialized = JsonSerializer.Deserialize<Result>(json, GetOptions());
            deserialized.Should().NotBeNull();
            deserialized.IsFailure.Should().BeTrue();
            deserialized.Status.Code.Should().Be(ResultStatuses.Error.Code);
            deserialized.Errors.Should().ContainSingle();
        }

        [Fact]
        public void Deserialize_Handles_Missing_Value_As_Default()
        {
            var json = "{\"isSuccess\":true,\"isFailure\":false,\"status\":{\"code\":200,\"description\":\"OK\"}}";
            var deserialized = JsonSerializer.Deserialize<Result<int>>(json, GetOptions());
            deserialized.Should().NotBeNull();
            deserialized.IsSuccess.Should().BeTrue();
            deserialized.Value.Should().Be(0);
        }

        [Fact]
        public void Serialize_And_Deserialize_Complex_Generic_Result()
        {
            var error = new ErrorInfo(
                ErrorCategory.Database,
                "DB",
                "DB error",
                null,
                ImmutableDictionary.CreateRange<string, object?>(new[] { new KeyValuePair<string, object?>("Table", "Users") }),
                ImmutableList.Create(SampleError));
            var result = new Result<List<string>>(new List<string> { "a", "b" }, BadRequestStatus, new[] { "msg" }, new[] { error });
            var json = JsonSerializer.Serialize(result, GetOptions());
            Result<List<string>>? deserialized = JsonSerializer.Deserialize<Result<List<string>>>(json, GetOptions());
            deserialized.Should().NotBeNull();
            deserialized.IsFailure.Should().BeTrue();
            deserialized.Errors.Should().ContainSingle();
            deserialized.Errors[0].Category.Should().Be(ErrorCategory.Database);
            deserialized.Errors[0].InnerErrors.Should().Contain(SampleError);
            deserialized.Messages.Should().Contain("msg");
            deserialized.Value.Should().BeEquivalentTo(new List<string> { "a", "b" });
        }

        [Fact]
        public void Write_Throws_On_Null_Writer()
        {
            var converter = new ResultJsonConverter();
            IResult result = Result.Success();
            Action act = () => ((JsonConverter<Result>)converter.CreateConverter(typeof(Result), GetOptions())!).Write(null!, (Result)result, GetOptions());
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Read_Throws_On_Invalid_Token()
        {
            var converter = new ResultJsonConverter();
            var options = GetOptions();
            var json = "\"not an object\"";

            Action act = () =>
            {
                var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
                var conv = (JsonConverter<Result>)converter.CreateConverter(typeof(Result), options)!;
                reader.Read();
                conv.Read(ref reader, typeof(Result), options);
            };

            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void Serialize_Successful_Generic_Result_With_Null_Value_Of_Nullable_Type()
        {
            var result = Result<string?>.Success(value: null, messages: ["no value"]);
            var json = JsonSerializer.Serialize(result, GetOptions());
            json.Should().Contain("\"isSuccess\":true");
            json.Should().Contain("\"isFailure\":false");
            json.Should().Contain("\"value\":null");
            json.Should().Contain("\"messages\":[\"no value\"]");
            json.Should().NotContain("\"errors\"");
        }

        [Fact]
        public void Deserialize_Successful_Generic_Result_With_Null_Value_Of_Nullable_Type()
        {
            var json = "{\"isSuccess\":true,\"isFailure\":false,\"status\":{\"code\":200,\"description\":\"OK\"},\"value\":null,\"messages\":[\"test\"]}";
            var deserialized = JsonSerializer.Deserialize<Result<string?>>(json, GetOptions());
            deserialized.Should().NotBeNull();
            deserialized.IsSuccess.Should().BeTrue();
            deserialized.Value.Should().BeNull();
            deserialized.Messages.Should().Contain("test");
        }

        [Fact]
        public void Serialize_Successful_Generic_Result_With_Custom_Object_Value()
        {
            var customObject = new { Name = "Test", Id = 123 };
            var result = Result<object>.Success(customObject, "Data found");
            var json = JsonSerializer.Serialize(result, GetOptions());

            json.Should().Contain("\"isSuccess\":true");
            json.Should().Contain("\"value\":{\"name\":\"Test\",\"id\":123}");
            json.Should().Contain("\"messages\":[\"Data found\"]");
        }

        [Fact]
        public void Deserialize_Successful_Generic_Result_With_Custom_Object_Value()
        {
            var json = "{\"isSuccess\":true,\"isFailure\":false,\"status\":{\"code\":200,\"description\":\"OK\"},\"value\":{\"name\":\"Test\",\"id\":123},\"messages\":[]}";
            var deserialized = JsonSerializer.Deserialize<Result<MyDeserializedObject>>(json, GetOptions());

            deserialized.Should().NotBeNull();
            deserialized.IsSuccess.Should().BeTrue();
            deserialized.Value.Should().NotBeNull();
            deserialized.Value!.Name.Should().Be("Test");
            deserialized.Value.Id.Should().Be(123);
        }

        private class MyDeserializedObject
        {
            public string Name { get; set; } = string.Empty;
            public int Id { get; set; }
        }

        [Fact]
        public void Serialize_And_Deserialize_IResult_Interface()
        {
            IResult originalSuccess = Result.Success("Interface success");
            var jsonSuccess = JsonSerializer.Serialize(originalSuccess, typeof(IResult), GetOptions());
            var deserializedSuccess = JsonSerializer.Deserialize<IResult>(jsonSuccess, GetOptions());
            deserializedSuccess.Should().NotBeNull();
            deserializedSuccess.IsSuccess.Should().BeTrue();
            deserializedSuccess.Messages.Should().Contain("Interface success");

            IResult originalFailure = Result.Failure(SampleError, BadRequestStatus);
            var jsonFailure = JsonSerializer.Serialize(originalFailure, typeof(IResult), GetOptions());
            var deserializedFailure = JsonSerializer.Deserialize<IResult>(jsonFailure, GetOptions());
            deserializedFailure.Should().NotBeNull();
            deserializedFailure.IsFailure.Should().BeTrue();
            deserializedFailure.Errors.Should().ContainSingle().Which.Message.Should().Be("Error message");
        }

        [Fact]
        public void Serialize_And_Deserialize_IResultT_Interface()
        {
            IResult<int> originalSuccess = Result<int>.Success(123, "Interface success");
            var jsonSuccess = JsonSerializer.Serialize(originalSuccess, typeof(IResult<int>), GetOptions());
            var deserializedSuccess = JsonSerializer.Deserialize<IResult<int>>(jsonSuccess, GetOptions());
            deserializedSuccess.Should().NotBeNull();
            deserializedSuccess.IsSuccess.Should().BeTrue();
            deserializedSuccess.Value.Should().Be(123);

            IResult<string> originalFailure = Result<string>.Failure("default", SampleError, BadRequestStatus);
            var jsonFailure = JsonSerializer.Serialize(originalFailure, typeof(IResult<string>), GetOptions());
            var deserializedFailure = JsonSerializer.Deserialize<IResult<string>>(jsonFailure, GetOptions());
            deserializedFailure.Should().NotBeNull();
            deserializedFailure.IsFailure.Should().BeTrue();
            deserializedFailure.Errors.Should().ContainSingle().Which.Message.Should().Be("Error message");
            deserializedFailure.Value.Should().Be("default");
        }

        [Fact]
        public void Serialize_Null_Result_Object()
        {
            Result? nullResult = null;
            var json = JsonSerializer.Serialize(nullResult, GetOptions());
            json.Should().Be("null");

            Result<int>? nullGenericResult = null;
            var jsonGeneric = JsonSerializer.Serialize(nullGenericResult, GetOptions());
            jsonGeneric.Should().Be("null");
        }

        [Fact]
        public void Deserialize_Null_Json_String()
        {
            var deserialized = JsonSerializer.Deserialize<Result>("null", GetOptions());
            deserialized.Should().BeNull();

            var deserializedGeneric = JsonSerializer.Deserialize<Result<int>>("null", GetOptions());
            deserializedGeneric.Should().BeNull();
        }

        [Fact]
        public void Deserialize_Handles_Missing_IsSuccess_And_IsFailure_Flags()
        {
            var jsonWithErrorsNoFlags = "{\"status\":{\"code\":400,\"description\":\"Bad Request\"},\"errors\":[{\"category\":0,\"code\":\"ERR\",\"message\":\"Test\"}]}";
            var deserializedErrors = JsonSerializer.Deserialize<Result>(jsonWithErrorsNoFlags, GetOptions());
            deserializedErrors.Should().NotBeNull();
            deserializedErrors.IsFailure.Should().BeTrue();

            var jsonWithMessagesNoFlags = "{\"status\":{\"code\":200,\"description\":\"OK\"},\"messages\":[\"Test\"]}";
            var deserializedMessages = JsonSerializer.Deserialize<Result>(jsonWithMessagesNoFlags, GetOptions());
            deserializedMessages.Should().NotBeNull();
            deserializedMessages.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Deserialize_Handles_Inconsistent_IsSuccess_And_IsFailure_Flags()
        {
            var jsonBothTrue = "{\"isSuccess\":true,\"isFailure\":true,\"status\":{\"code\":200,\"description\":\"OK\"}}";
            var deserializedBothTrue = JsonSerializer.Deserialize<Result>(jsonBothTrue, GetOptions());
            deserializedBothTrue.Should().NotBeNull();
            deserializedBothTrue.IsSuccess.Should().BeTrue();

            var jsonBothFalse = "{\"isSuccess\":false,\"isFailure\":false,\"status\":{\"code\":500,\"description\":\"Error\"},\"errors\":[{\"category\":0,\"code\":\"ERR\",\"message\":\"Test\"}]}";
            var deserializedBothFalse = JsonSerializer.Deserialize<Result>(jsonBothFalse, GetOptions());
            deserializedBothFalse.Should().NotBeNull();
            deserializedBothFalse.IsFailure.Should().BeTrue();
        }
    }
}
