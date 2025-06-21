// <copyright file="ResultExtensionsConversionTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using FluentAssertions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zentient.Results.Tests
{
    public class ResultExtensionsConversionTests
    {
        // Helper to create ErrorInfo instances, replacing assumed static factories in ErrorInfo
        private static ErrorInfo CreateErrorInfo(string code, string message, ErrorCategory category = ErrorCategory.General)
        {
            return new ErrorInfo(category, code, message);
        }

        #region Conversion (AsResult/AsSuccess/AsError) Methods

        [Fact]
        public void AsResult_True_ReturnsSuccessResult()
        {
            // Act
            IResult result = true.AsResult();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Messages.Should().BeEmpty();
        }

        [Fact]
        public void AsResult_True_WithMessage_ReturnsSuccessResultWithMessage()
        {
            // Arrange
            var message = "Operation completed.";

            // Act
            IResult result = true.AsResult(successMessage: message);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Messages.Should().ContainSingle(e => e.Equals(message));
        }

        [Fact]
        public void AsResult_False_ReturnsFailureResultWithDefaultError()
        {
            // Act
            IResult result = false.AsResult();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle(e =>
                e.Code == "GeneralError" && e.Message == "Operation failed.");
            result.Status.Should().Be(ResultStatuses.BadRequest);
        }

        [Fact]
        public void AsResult_False_WithErrorInfo_ReturnsFailureResultWithErrorInfo()
        {
            // Arrange
            var customError = CreateErrorInfo("CUSTOM_FAIL", "Something very specific failed.");

            // Act
            IResult result = false.AsResult(failureError: customError);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Equals(customError));
        }

        [Fact]
        public void AsResult_Generic_True_ReturnsSuccessResultWithValue()
        {
            // Arrange
            var value = new { Name = "Test" };

            // Act
            var result = true.AsResult(value: value);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(value);
        }

        [Fact]
        public void AsResult_Generic_False_ReturnsFailureResultWithDefaultErrorAndDefaultValue()
        {
            // Arrange
            var value = "some_value";

            // Act
            IResult<string> result = false.AsResult(value: value);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Errors.Should().ContainSingle(e =>
                e.Code == "GeneralError" && e.Message == "Operation failed.");
        }

        [Fact]
        public void ToResult_Value_ReturnsSuccessResultWithValue()
        {
            // Arrange
            var value = 42;

            // Act
            IResult<int> result = value.AsSuccess();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
        }

        [Fact]
        public void ToResult_Value_WithMessage_ReturnsSuccessResultWithValueAndMessage()
        {
            // Arrange
            var value = "data";
            var message = "Data retrieved.";

            // Act
            IResult<string> result = value.AsSuccess(message);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("data");
            result.Messages.Should().ContainSingle(e => e.Equals(message));
        }

        [Fact]
        public void ToResult_Exception_NonGeneric_ReturnsFailureFromException()
        {
            // Arrange
            var ex = new UnauthorizedAccessException("Access denied.");

            // Act
            IResult result = ex.AsError();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle(e =>
                e.Category == ErrorCategory.Exception && e.Message == ex.Message);
            result.Status.Should().Be(ResultStatuses.Error);
        }

        [Fact]
        public void ToResult_Exception_Generic_ReturnsFailureFromExceptionWithDefaultValue()
        {
            // Arrange
            var ex = new TimeoutException("Request timed out.");

            // Act
            IResult<int> result = ex.AsError<int>();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Value.Should().Be(default(int));
            result.Errors.Should().ContainSingle(e => e.Category == ErrorCategory.Exception);
        }

        [Fact]
        public void ToResult_Exception_Generic_WithValue_ReturnsFailureFromExceptionWithValue()
        {
            // Arrange
            var ex = new DataMisalignedException("Data corruption.");
            var partialValue = "corrupted_record";

            // Act
            IResult<string> result = ex.AsError<string>(partialValue);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Value.Should().Be(partialValue);
            result.Errors.Should().ContainSingle(e => e.Category == ErrorCategory.Exception);
        }

        [Fact]
        public void ToSuccessResult_String_ReturnsSuccessResultWithMessage()
        {
            // Arrange
            var message = "Operation done.";

            // Act
            IResult result = message.AsSuccess();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Messages.Should().ContainSingle(e => e.Equals(message));
        }

        [Fact]
        public void ToErrorResult_String_ReturnsFailureResultWithGeneralError()
        {
            // Arrange
            var message = "Something went wrong.";

            // Act
            IResult result = message.AsError();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle(e =>
                e.Category == ErrorCategory.General && e.Code == "GeneralError" && e.Message == message);
        }

        [Fact]
        public void ToErrorResult_String_WithCustomCode_ReturnsFailureResultWithSpecificGeneralError()
        {
            // Arrange
            var message = "Specific issue.";
            var code = "SPECIFIC_ISSUE";

            // Act
            IResult result = message.AsError(code);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle(e =>
                e.Category == ErrorCategory.General && e.Code == code && e.Message == message);
        }

        [Fact]
        public void ToErrorResult_String_Generic_WithValue_ReturnsFailureResultWithValueAndGeneralError()
        {
            // Arrange
            var message = "User not found.";
            var value = 0;
            var code = "USER_NOT_FOUND";

            // Act
            IResult<int> result = message.AsError(value, code);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Value.Should().Be(value);
            result.Errors.Should().ContainSingle(e =>
                e.Category == ErrorCategory.General && e.Code == code && e.Message == message);
        }

        [Fact]
        public void AsError_ErrorInfo_ReturnsFailureResultWithSpecificError()
        {
            // Arrange
            var error = CreateErrorInfo("INVALID_INPUT", "Invalid input.", ErrorCategory.Validation);

            // Act
            IResult result = error.AsError();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Equals(error));
        }

        [Fact]
        public void AsError_ErrorInfo_Generic_WithValue_ReturnsFailureResultWithValueAndSpecificError()
        {
            // Arrange
            var error = CreateErrorInfo("ACCESS_FORBIDDEN", "Access forbidden.", ErrorCategory.Authentication);
            var value = "ForbiddenData";

            // Act
            IResult<string> result = error.AsError<string>(value);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Value.Should().Be(value);
            result.Errors.Should().ContainSingle(e => e.Equals(error));
        }

        [Fact]
        public void AsError_IEnumerableErrorInfo_ReturnsFailureResultWithMultipleErrors()
        {
            // Arrange
            var errors = new[] {
                CreateErrorInfo("REQUIRED_FIELD_A", "Field A is required.", ErrorCategory.Validation),
                CreateErrorInfo("FIELD_B_TOO_LONG", "Field B is too long.", ErrorCategory.Validation)
            };

            // Act
            IResult result = errors.AsError();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void AsError_IEnumerableErrorInfo_Generic_WithValue_ReturnsFailureResultWithValueAndMultipleErrors()
        {
            // Arrange
            var errors = new[] {
                CreateErrorInfo("RECORD_EXISTS", "Record already exists.", ErrorCategory.Conflict),
                CreateErrorInfo("DB_ERROR", "Database error.", ErrorCategory.General)
            };
            var value = new { Id = 1, Name = "Partial" };

            // Act
            IResult<object> result = errors.AsError((object)value);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Value.Should().Be(value);
            result.Errors.Should().BeEquivalentTo(errors);
        }

        #endregion
    }
}
