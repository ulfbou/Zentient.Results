using FluentAssertions;

using System;
using System.Collections.Generic;

using Xunit;

namespace Zentient.Results.Tests
{
    public class ResultTests
    {
        private static readonly ErrorInfo SampleError = new(ErrorCategory.General, "ERR", "Error message");
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

        [Fact]
        public void Success_Factory_Creates_Successful_Result()
        {
            var result = Result.Success(SuccessStatus, "All good");
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Status.Code.Should().Be(200);
            result.Messages.Should().ContainSingle().Which.Should().Be("All good");
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Created_Factory_Creates_Created_Result()
        {
            var result = Result.Created("Created!");
            result.IsSuccess.Should().BeTrue();
            result.Status.Code.Should().Be(201);
            result.Messages.Should().Contain("Created!");
        }

        [Fact]
        public void NoContent_Factory_Creates_NoContent_Result()
        {
            var result = Result.NoContent("No content");
            result.IsSuccess.Should().BeTrue();
            result.Status.Code.Should().Be(204);
            result.Messages.Should().Contain("No content");
        }

        [Fact]
        public void Failure_Factory_Creates_Failure_Result()
        {
            var result = Result.Failure(SampleError, BadRequestStatus);
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Status.Code.Should().Be(400);
            result.Errors.Should().ContainSingle().Which.Should().Be(SampleError);
        }

        [Fact]
        public void Failure_Factory_Throws_On_NullOrEmpty_Errors()
        {
            Action actNull = () => Result.Failure((IEnumerable<ErrorInfo>)null!);
            Action actEmpty = () => Result.Failure(Array.Empty<ErrorInfo>());
            actNull.Should().Throw<ArgumentException>();
            actEmpty.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Validation_Factory_Uses_UnprocessableEntity_Status()
        {
            var result = Result.Validation(SampleErrors);
            result.IsFailure.Should().BeTrue();
            result.Status.Code.Should().Be(ResultStatuses.UnprocessableEntity.Code);
            result.Errors.Should().BeEquivalentTo(SampleErrors);
        }

        [Fact]
        public void Unauthorized_Forbidden_NotFound_Conflict_InternalError_Factories()
        {
            Result.Unauthorized().Status.Code.Should().Be(ResultStatuses.Unauthorized.Code);
            Result.Forbidden().Status.Code.Should().Be(ResultStatuses.Forbidden.Code);
            Result.NotFound().Status.Code.Should().Be(ResultStatuses.NotFound.Code);
            Result.Conflict().Status.Code.Should().Be(ResultStatuses.Conflict.Code);
            Result.InternalError().Status.Code.Should().Be(ResultStatuses.Error.Code);
        }

        [Fact]
        public void FromException_Creates_ErrorResult_With_ExceptionInfo()
        {
            var ex = new InvalidOperationException("fail!");
            var result = Result.FromException(ex);
            result.IsFailure.Should().BeTrue();
            result.Status.Code.Should().Be(ResultStatuses.Error.Code);
            result.Errors.Should().ContainSingle();
            result.Errors[0].Category.Should().Be(ErrorCategory.Exception);
            result.Errors[0].Message.Should().Be("fail!");
            result.Errors[0].Data.Should().Be(ex);
        }

        [Fact]
        public void Implicit_Conversion_From_ErrorInfo_Creates_Failure()
        {
            Result result = SampleError;
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(SampleError);
        }

        [Fact]
        public void Generic_Success_Created_NoContent_Factories_Forward_To_ResultT()
        {
            var r1 = Result.Success(42, "ok");
            r1.IsSuccess.Should().BeTrue();
            r1.Messages.Should().Contain("ok");
            r1.Value.Should().Be(42);

            var r2 = Result.Created("created");
            r2.IsSuccess.Should().BeTrue();
            r2.Messages.Should().Contain("created");

            var r3 = Result.NoContent<int>("none");
            r3.IsSuccess.Should().BeTrue();
            r3.Messages.Should().Contain("none");
        }

        [Fact]
        public void Generic_Failure_Validation_Unauthorized_Forbidden_NotFound_Conflict_InternalError_FromException()
        {
            var err = new ErrorInfo(ErrorCategory.General, "E", "fail");
            var r1 = Result.Failure<int>(err);
            r1.IsFailure.Should().BeTrue();
            r1.Errors.Should().Contain(err);

            var r2 = Result.Validation<int>(SampleErrors);
            r2.IsFailure.Should().BeTrue();
            r2.Errors.Should().BeEquivalentTo(SampleErrors);

            var r3 = Result.Unauthorized<int>();
            r3.Status.Code.Should().Be(ResultStatuses.Unauthorized.Code);

            var r4 = Result.Forbidden<int>();
            r4.Status.Code.Should().Be(ResultStatuses.Forbidden.Code);

            var r5 = Result.NotFound<int>();
            r5.Status.Code.Should().Be(ResultStatuses.NotFound.Code);

            var r6 = Result.Conflict<int>();
            r6.Status.Code.Should().Be(ResultStatuses.Conflict.Code);

            var r7 = Result.InternalError<int>();
            r7.Status.Code.Should().Be(ResultStatuses.Error.Code);

            var ex = new InvalidOperationException("fail!");
            var r8 = Result.FromException<int>(ex);
            r8.IsFailure.Should().BeTrue();
            r8.Errors[0].Message.Should().Be("fail!");
        }

        [Fact]
        public void IsSuccess_True_If_Status_2xx_And_NoErrors()
        {
            var result = Result.Success(SuccessStatus);
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
        }

        [Fact]
        public void IsSuccess_False_If_Status_2xx_But_HasErrors()
        {
            // Use the public Failure factory to create a result with errors and a 2xx status
            var result = Result.Failure(SampleError, SuccessStatus);
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Error_Returns_First_Error_Message_Or_Null()
        {
            var result = Result.Failure(new[] { SampleError, new ErrorInfo(ErrorCategory.General, "E2", "Second") }, BadRequestStatus);
            result.Error.Should().Be("Error message");

            var success = Result.Success(SuccessStatus);
            success.Error.Should().BeNull();
        }

        [Fact]
        public void Messages_And_Errors_Default_To_Empty()
        {
            var result = Result.Success(SuccessStatus);
            result.Messages.Should().BeEmpty();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ToString_Formats_Success_And_Failure()
        {
            var success = Result.Success(SuccessStatus, "yay");
            success.ToString().Should().Contain("Success").And.Contain("yay");

            var failure = Result.Failure(SampleError, BadRequestStatus);
            failure.ToString().Should().Contain("Failure").And.Contain("Error message");
        }

        [Fact]
        public void Guard_AgainstDefault_Throws_On_Default_ErrorInfo()
        {
            Action act = () => Result.Failure(default(ErrorInfo));
            act.Should().Throw<ArgumentException>();
        }
    }
}
