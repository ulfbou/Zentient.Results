// <copyright file="ResultErrorCheckingExtensions.cs" company="Zentient Framework Team">
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
    public class ResultErrorCheckingExtensions
    {
        // Helper to create ErrorInfo instances, replacing assumed static factories in ErrorInfo
        private static ErrorInfo CreateErrorInfo(string code, string message, ErrorCategory category = ErrorCategory.General)
        {
            return new ErrorInfo(category, code, message);
        }

        #region Error Checking (HasErrorCategory, HasErrorCode)

        [Fact]
        public void HasErrorCategory_FailureWithMatchingCategory_ReturnsTrue()
        {
            // Arrange
            IResult result = Result.Failure(CreateErrorInfo("C1", "M1", ErrorCategory.Validation));

            // Act
            var hasCategory = result.HasErrorCategory(ErrorCategory.Validation);

            // Assert
            hasCategory.Should().BeTrue();
        }

        [Fact]
        public void HasErrorCategory_FailureWithoutMatchingCategory_ReturnsFalse()
        {
            // Arrange
            IResult result = Result.Failure(CreateErrorInfo("C1", "M1", ErrorCategory.Network));

            // Act
            var hasCategory = result.HasErrorCategory(ErrorCategory.Validation);

            // Assert
            hasCategory.Should().BeFalse();
        }

        [Fact]
        public void HasErrorCategory_SuccessResult_ReturnsFalse()
        {
            // Arrange
            IResult result = Result.Success();

            // Act
            var hasCategory = result.HasErrorCategory(ErrorCategory.Validation);

            // Assert
            hasCategory.Should().BeFalse();
        }

        [Fact]
        public void HasErrorCategory_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            IResult result = null!;

            // Act
            Action act = () => result.HasErrorCategory(ErrorCategory.General);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("result");
        }

        [Fact]
        public void HasErrorCode_FailureWithMatchingCode_ReturnsTrue()
        {
            // Arrange
            IResult result = Result.Failure(CreateErrorInfo("REQUIRED_FIELD", "Field is required."));

            // Act
            var hasCode = result.HasErrorCode("REQUIRED_FIELD");

            // Assert
            hasCode.Should().BeTrue();
        }

        [Fact]
        public void HasErrorCode_FailureWithoutMatchingCode_ReturnsFalse()
        {
            // Arrange
            IResult result = Result.Failure(CreateErrorInfo("INVALID_FORMAT", "Bad data."));

            // Act
            var hasCode = result.HasErrorCode("NOT_FOUND");

            // Assert
            hasCode.Should().BeFalse();
        }

        [Fact]
        public void HasErrorCode_SuccessResult_ReturnsFalse()
        {
            // Arrange
            IResult result = Result.Success();

            // Act
            var hasCode = result.HasErrorCode("ANY_CODE");

            // Assert
            hasCode.Should().BeFalse();
        }

        [Fact]
        public void HasErrorCode_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            IResult result = null!;

            // Act
            Action act = () => result.HasErrorCode("CODE");

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("result");
        }

        [Fact]
        public void HasErrorCode_NullErrorCode_ThrowsArgumentNullException()
        {
            // Arrange
            IResult result = Result.Failure(CreateErrorInfo("A", "B"));
            string errorCode = null!;

            // Act
            Action act = () => result.HasErrorCode(errorCode);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("errorCode");
        }

        #endregion
    }
}
