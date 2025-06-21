// <copyright file="ResultTryExtensionsTests.cs" company="Zentient Framework Team">
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
    public class ResultTryExtensionsTests
    {
        // Helper to create ErrorInfo instances, replacing assumed static factories in ErrorInfo
        private static ErrorInfo CreateErrorInfo(string code, string message, ErrorCategory category = ErrorCategory.General)
        {
            return new ErrorInfo(category, code, message);
        }

        #region Try Methods (Exception Handling)

        [Fact]
        public void Try_Action_Success_ReturnsSuccessResult()
        {
            // Arrange
            var executed = false;
            Action action = () => executed = true;

            // Act
            IResult result = action.Try();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            executed.Should().BeTrue();
        }

        [Fact]
        public void Try_Action_ThrowsException_ReturnsFailureResultWithExceptionError()
        {
            // Arrange
            var testException = new InvalidOperationException("Something went wrong in the action.");
            Action action = () => throw testException;

            // Act
            IResult result = action.Try();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().ContainSingle(e =>
                e.Category == ErrorCategory.Exception &&
                e.Code == testException.GetType().Name &&
                e.Message == testException.Message);
            result.ErrorMessage.Should().Contain(testException.Message);
            result.Status.Should().Be(ResultStatuses.Error);
        }

        [Fact]
        public void Try_Action_NullAction_ThrowsArgumentNullException()
        {
            // Arrange
            Action action = null!;

            // Act
            Action act = () => action.Try();

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("action");
        }

        [Fact]
        public void Try_Func_Success_ReturnsSuccessResultWithValue()
        {
            // Arrange
            Func<int> func = () => 123;

            // Act
            IResult<int> result = func.Try();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(123);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Try_Func_ThrowsException_ReturnsFailureResultWithExceptionErrorAndDefaultValue()
        {
            // Arrange
            var testException = new FormatException("Invalid format.");
            Func<string> func = () => throw testException;

            // Act
            IResult<string> result = func.Try();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Value.Should().BeNull();
            result.Errors.Should().ContainSingle(e =>
                e.Category == ErrorCategory.Exception &&
                e.Code == testException.GetType().Name &&
                e.Message == testException.Message
            );
            result.Status.Should().Be(ResultStatuses.Error);
        }

        [Fact]
        public void Try_Func_NullFunc_ThrowsArgumentNullException()
        {
            // Arrange
            Func<string> func = null!;

            // Act
            Action act = () => func.Try();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("func");
        }

        #endregion
    }
}
