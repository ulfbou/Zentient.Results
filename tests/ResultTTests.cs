// <copyright file="ResultTTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using FluentAssertions;

using Microsoft.AspNetCore.Http.HttpResults;

namespace Zentient.Results.Tests
{
    public class ResultTTests
    {
        private static readonly ErrorInfo SampleError = new(ErrorCategory.General, "ERR", "Error message");
        private static readonly ErrorInfo[] SampleErrors = { SampleError, new(ErrorCategory.Validation, "VAL", "Validation failed") };

        private class DummyStatus : IResultStatus
        {
            public int Code { get; set; }
            public string Description { get; set; } = string.Empty;
            public override string ToString() => $"{Code} {Description}";
        }

        [Fact]
        public void Success_Factory_Creates_Successful_Result()
        {
            var result = Result<int>.Success(42, "All good");
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Status.Code.Should().Be(200);
            result.Messages.Should().ContainSingle().Which.Should().Be("All good");
            result.Errors.Should().BeEmpty();
            result.Value.Should().Be(42);
        }

        [Fact]
        public void Created_Factory_Creates_Created_Result()
        {
            var result = Result<string>.Created("abc", "Created!");
            result.IsSuccess.Should().BeTrue();
            result.Status.Code.Should().Be(201);
            result.Messages.Should().Contain("Created!");
            result.Value.Should().Be("abc");
        }

        [Fact]
        public void NoContent_Factory_Creates_NoContent_Result()
        {
            var result = Result<int>.NoContent("No content");
            result.IsSuccess.Should().BeTrue();
            result.Status.Code.Should().Be(204);
            result.Messages.Should().Contain("No content");
            result.Value.Should().Be(default);
        }

        [Fact]
        public void Failure_Factory_Creates_Failure_Result()
        {
            var result = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Status.Code.Should().Be(400);
            result.Errors.Should().ContainSingle().Which.Should().Be(SampleError);
            result.Value.Should().Be(0);
        }

        [Fact]
        public void Failure_Factory_Throws_On_NullOrEmpty_Errors()
        {
            Action actNull = () => Result<int>.Failure(0, errors: null!, status: ResultStatuses.BadRequest);
            Action actEmpty = () => Result<int>.Failure(0, Array.Empty<ErrorInfo>(), ResultStatuses.BadRequest);
            actNull.Should().Throw<ArgumentNullException>();
            actEmpty.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Validation_Factory_Uses_UnprocessableEntity_Status()
        {
            var result = Result<int>.Validation(SampleErrors);
            result.IsFailure.Should().BeTrue();
            result.Status.Code.Should().Be(ResultStatuses.UnprocessableEntity.Code);
            result.Errors.Should().BeEquivalentTo(SampleErrors);
        }

        [Fact]
        public void FromException_Creates_ErrorResult_With_ExceptionInfo()
        {
            var ex = new InvalidOperationException("fail!");
            var result = Result<int>.FromException(0, ex);
            result.IsFailure.Should().BeTrue();
            result.Status.Code.Should().Be(ResultStatuses.Error.Code);
            result.Errors.Should().ContainSingle();
            result.Errors[0].Category.Should().Be(ErrorCategory.Exception);
            result.Errors[0].Message.Should().Be("fail!");
        }

        [Fact]
        public void Implicit_Conversion_From_Value_Creates_Success()
        {
            Result<int> result = 123;
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(123);
        }

        [Fact]
        public void IsSuccess_True_If_Status_2xx_And_NoErrors()
        {
            var result = new Result<int>(42, ResultStatuses.Success);
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
        }

        [Fact]
        public void IsSuccess_False_If_Status_2xx_But_HasErrors()
        {
            var result = new Result<int>(42, ResultStatuses.Success, null, new[] { SampleError });
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Error_Returns_First_Error_Message_Or_Null()
        {
            var result = new Result<int>(0, ResultStatuses.Success, null, new[] { SampleError, new ErrorInfo(ErrorCategory.General, "E2", "Second") });
            result.ErrorMessage.Should().Be("Error message");

            var success = Result<int>.Success(1);
            success.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void Messages_And_Errors_Default_To_Empty()
        {
            var result = new Result<int>(42, ResultStatuses.Success, null, null);
            result.Messages.Should().BeEmpty();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ToString_Formats_Success_And_Failure()
        {
            var success = Result<int>.Success(1, "yay");
            success.ToString().Should().Contain("Success").And.Contain("yay");

            var failure = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            failure.ToString().Should().Contain("Failure").And.Contain("Error message");
        }

        [Fact]
        public void Map_Transforms_Value_On_Success()
        {
            var result = Result<int>.Success(2);
            var mapped = result.Map(i => i * 2);
            mapped.IsSuccess.Should().BeTrue();
            mapped.Value.Should().Be(4);
        }

        [Fact]
        public void Map_Propagates_Failure()
        {
            var result = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            var mapped = result.Map(i => i * 2);
            mapped.IsFailure.Should().BeTrue();
            mapped.Errors.Should().BeEquivalentTo(result.Errors);
        }

        [Fact]
        public void Bind_Chains_On_Success()
        {
            var result = Result<int>.Success(2);
            var bound = result.Bind(i => Result<string>.Success($"Value: {i}"));
            bound.IsSuccess.Should().BeTrue();
            bound.Value.Should().Be("Value: 2");
        }

        [Fact]
        public void Bind_Propagates_Failure()
        {
            var result = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            var bound = result.Bind(i => Result<string>.Success($"Value: {i}"));
            bound.IsFailure.Should().BeTrue();
            bound.Errors.Should().BeEquivalentTo(result.Errors);
        }

        [Fact]
        public void Tap_Executes_Action_On_Success()
        {
            int tapped = 0;
            var result = Result<int>.Success(5);
            result.Tap(i => tapped = i);
            tapped.Should().Be(5);
        }

        [Fact]
        public void Tap_Does_Not_Execute_On_Failure()
        {
            int tapped = 0;
            var result = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            result.Tap(i => tapped = i);
            tapped.Should().Be(0);
        }

        [Fact]
        public void OnSuccess_Executes_Action_On_Success()
        {
            int called = 0;
            var result = Result<int>.Success(7);
            result.OnSuccess(i => called = i);
            called.Should().Be(7);
        }

        [Fact]
        public void OnSuccess_Does_Not_Execute_On_Failure()
        {
            int called = 0;
            var result = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            result.OnSuccess(i => called = i);
            called.Should().Be(0);
        }

        [Fact]
        public void OnFailure_Executes_Action_On_Failure()
        {
            IReadOnlyList<ErrorInfo>? errors = null;
            var result = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            result.OnFailure(e => errors = e);
            errors.Should().BeEquivalentTo(result.Errors);
        }

        [Fact]
        public void OnFailure_Does_Not_Execute_On_Success()
        {
            bool called = false;
            var result = Result<int>.Success(1);
            result.OnFailure(e => called = true);
            called.Should().BeFalse();
        }

        [Fact]
        public void Match_Executes_OnSuccess_Or_OnFailure()
        {
            var result = Result<int>.Success(10);
            var value = result.Match(i => i * 2, errs => -1);
            value.Should().Be(20);

            var fail = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            var value2 = fail.Match(i => i * 2, errs => -1);
            value2.Should().Be(-1);
        }

        [Fact]
        public void GetValueOrThrow_Returns_Value_On_Success()
        {
            var result = Result<int>.Success(99);
            result.GetValueOrThrow().Should().Be(99);
        }

        [Fact]
        public void GetValueOrThrow_Throws_On_Failure()
        {
            var result = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            Action act = () => result.GetValueOrThrow();
            act.Should().Throw<InvalidOperationException>().WithMessage("*Error message*");
        }

        [Fact]
        public void GetValueOrThrow_With_Message_Throws_On_Failure()
        {
            var result = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            Action act = () => result.GetValueOrThrow("custom");
            act.Should().Throw<InvalidOperationException>().WithMessage("custom");
        }

        [Fact]
        public void GetValueOrThrow_With_Factory_Throws_On_Failure()
        {
            var result = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            Action act = () => result.GetValueOrThrow(() => new ApplicationException("fail"));
            act.Should().Throw<ApplicationException>().WithMessage("fail");
        }

        [Fact]
        public void GetValueOrDefault_Returns_Value_Or_Fallback()
        {
            var result = Result<int>.Success(123);
            result.GetValueOrDefault(999).Should().Be(123);

            var fail = Result<int>.Failure(0, SampleError, ResultStatuses.BadRequest);
            fail.GetValueOrDefault(999).Should().Be(999);
        }

        [Fact]
        public void Supports_All_ErrorCategory_Values()
        {
            foreach (ErrorCategory category in Enum.GetValues(typeof(ErrorCategory)))
            {
                var error = new ErrorInfo(category, "CODE", "Message");
                var result = Result<int>.Failure(0, error, ResultStatuses.BadRequest);
                result.Errors[0].Category.Should().Be(category);
            }
        }

        [Fact]
        public void Can_Handle_Empty_Strings_And_Null_Data()
        {
            var error = new ErrorInfo(ErrorCategory.General, "", "", null, null);
            var result = Result<int>.Failure(0, error, ResultStatuses.BadRequest);
            result.Errors[0].Code.Should().BeEmpty();
            result.Errors[0].Message.Should().BeEmpty();
            result.Errors[0].InnerErrors.Should().BeEmpty();
        }
    }
}
