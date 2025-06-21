// <copyright file="ResultExceptionTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Xunit;

using Zentient.Results;

namespace Zentient.Results.Tests
{
    public class ResultExceptionTests
    {
        // Helper to create ErrorInfo instances
        private static ErrorInfo CreateErrorInfo(string code, string message, ErrorCategory category = ErrorCategory.General)
        {
            return new ErrorInfo(category, code, message);
        }

        [Fact]
        public void Constructor_ErrorsOnly_SetsErrorsAndGeneratesDefaultMessage()
        {
            // Arrange
            var errors = new List<ErrorInfo>
            {
                CreateErrorInfo("CODE_001", "This is error one."),
                CreateErrorInfo("CODE_002", "This is error two.")
            };
            var expectedMessage = "One or more errors occurred: This is error one.; This is error two.";

            // Act
            var ex = new ResultException(errors);

            // Assert
            ex.Errors.Should().BeEquivalentTo(errors);
            ex.Message.Should().Be(expectedMessage);
            ex.InnerException.Should().BeNull();
        }

        [Fact]
        public void Constructor_ErrorsOnly_SingleError_GeneratesDefaultMessageCorrectly()
        {
            // Arrange
            var errors = new List<ErrorInfo> { CreateErrorInfo("SINGLE_ERROR", "Just one error.") };
            var expectedMessage = "One or more errors occurred: Just one error.";

            // Act
            var ex = new ResultException(errors);

            // Assert
            ex.Errors.Should().BeEquivalentTo(errors);
            ex.Message.Should().Be(expectedMessage);
        }

        [Fact]
        public void Constructor_ErrorsOnly_EmptyErrors_GeneratesDefaultMessageCorrectly()
        {
            // Arrange
            var errors = new List<ErrorInfo>();
            var expectedMessage = "One or more errors occurred: ";

            // Act
            var ex = new ResultException(errors);

            // Assert
            ex.Errors.Should().BeEmpty();
            ex.Message.Should().Be(expectedMessage);
        }

        [Fact]
        public void Constructor_ErrorsOnly_NullErrors_ThrowsArgumentNullException()
        {
            // Arrange
            IReadOnlyList<ErrorInfo> errors = null!;

            // Act
            Action act = () => new ResultException(errors);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("errors");
        }

        [Fact]
        public void Constructor_CustomMessageAndErrors_SetsMessageAndErrors()
        {
            // Arrange
            var customMessage = "An operation failed with specific issues.";
            var errors = new List<ErrorInfo> { CreateErrorInfo("NOT_FOUND", "Resource missing.") };

            // Act
            var ex = new ResultException(customMessage, errors);

            // Assert
            ex.Message.Should().Be(customMessage);
            ex.Errors.Should().BeEquivalentTo(errors);
            ex.InnerException.Should().BeNull();
        }

        [Fact]
        public void Constructor_CustomMessageAndErrors_NullErrors_ThrowsArgumentNullException()
        {
            // Arrange
            string customMessage = "Some message.";
            IReadOnlyList<ErrorInfo> errors = null!;

            // Act
            Action act = () => new ResultException(customMessage, errors);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("errors");
        }

        [Fact]
        public void Constructor_InnerExceptionAndErrors_SetsInnerExceptionAndErrors()
        {
            // Arrange
            var customMessage = "Failed due to an underlying system issue.";
            var innerEx = new InvalidOperationException("System was not ready.");
            var errors = new List<ErrorInfo> { CreateErrorInfo("SYSTEM_ERROR", "System fault.") };

            // Act
            var ex = new ResultException(customMessage, innerEx, errors);

            // Assert
            ex.Message.Should().Be(customMessage);
            ex.InnerException.Should().BeSameAs(innerEx);
            ex.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void Constructor_InnerExceptionAndErrors_NullErrors_ThrowsArgumentNullException()
        {
            // Arrange
            string customMessage = "Some message.";
            var innerEx = new Exception("Inner.");
            IReadOnlyList<ErrorInfo> errors = null!;

            // Act
            Action act = () => new ResultException(customMessage, innerEx, errors);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("errors");
        }
    }
}
