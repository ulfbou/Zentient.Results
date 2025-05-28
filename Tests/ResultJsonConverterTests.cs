using FluentAssertions;

using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            var result = Result.Success(SuccessStatus, "ok");
            var json = JsonSerializer.Serialize((Result)result, GetOptions()); // Explicitly cast to Result
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
            var original = Result.Success(SuccessStatus, "ok");
            var json = JsonSerializer.Serialize(original, GetOptions());
            var deserialized = JsonSerializer.Deserialize<Result>(json, GetOptions());
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
            deserialized.IsFailure.Should().BeTrue();
            deserialized.Status.Code.Should().Be(ResultStatuses.Error.Code);
            deserialized.Errors.Should().ContainSingle();
        }

        [Fact]
        public void Deserialize_Handles_Missing_Value_As_Default()
        {
            var json = "{\"isSuccess\":true,\"isFailure\":false,\"status\":{\"code\":200,\"description\":\"OK\"}}";
            var deserialized = JsonSerializer.Deserialize<Result<int>>(json, GetOptions());
            deserialized.IsSuccess.Should().BeTrue();
            deserialized.Value.Should().Be(0);
        }

        [Fact]
        public void Serialize_And_Deserialize_Complex_Generic_Result()
        {
            var error = new ErrorInfo(ErrorCategory.Database, "DB", "DB error", new { Table = "Users" }, new[] { SampleError });
            var result = new Result<List<string>>(new List<string> { "a", "b" }, BadRequestStatus, new[] { "msg" }, new[] { error });
            var json = JsonSerializer.Serialize(result, GetOptions());
            var deserialized = JsonSerializer.Deserialize<Result<List<string>>>(json, GetOptions());
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

            // Use a local function that takes the reader by ref and call it directly
            Action act = () =>
            {
                var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
                var conv = (JsonConverter<Result>)converter.CreateConverter(typeof(Result), options)!;
                reader.Read(); // Move to first token
                conv.Read(ref reader, typeof(Result), options);
            };

            act.Should().Throw<JsonException>();
        }
    }
}
