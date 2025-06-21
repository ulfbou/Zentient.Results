// <copyright file="ResultThrowIfFailureExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Xunit;

using Zentient.Results; // Assuming your Result classes are in this namespace

// NOTE TO USER:
// 1. This test file has been updated to reflect the renamed boolean-based
//    extension methods (e.g., 'AsResult') to resolve ambiguities
//    and adhere to better naming conventions.
//    Ensure your 'ResultExtensions' class in your main library also reflects these changes.
//
// 2. The CS1503 errors (e.g., "cannot convert from 'Zentient.Results.ErrorInfo' to 'string'")
//    still point to your 'Result.Failure' static methods. These tests assume
//    your 'Result' class has 'Failure' methods that accept 'ErrorInfo' or
//    'IEnumerable<ErrorInfo>' arguments. If your 'Result.Failure' currently
//    expects a 'string', you will need to update its signature in your library.

namespace Zentient.Results.Tests
{
    public class ResultThrowIfFailureExtensionsTests
    {
        // Helper to create ErrorInfo instances, replacing assumed static factories in ErrorInfo
        private static ErrorInfo CreateErrorInfo(string code, string message, ErrorCategory category = ErrorCategory.General)
        {
            return new ErrorInfo(category, code, message);
        }

        #region ThrowIfFailure Method

        [Fact]
        public void ThrowIfFailure_SuccessResult_DoesNotThrow()
        {
            // Arrange
            IResult successResult = Result.Success();

            // Act
            Action act = () => successResult.ThrowIfFailure();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ThrowIfFailure_FailureResult_ThrowsResultExceptionWithErrors()
        {
            // Arrange
            var errors = new[]
            {
                CreateErrorInfo("APP_ERROR", "Application logic failed."),
                CreateErrorInfo("DB_ERROR", "Database connection lost.")
            };
            IResult failureResult = Result.Failure(errors);

            // Act
            Action act = () => failureResult.ThrowIfFailure();

            // Assert
            act.Should().Throw<ResultException>()
                .Where(ex => ex.Errors.SequenceEqual(errors));
        }

        [Fact]
        public void ThrowIfFailure_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            IResult result = null!;

            // Act
            Action act = () => result.ThrowIfFailure();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("result");
        }

        #endregion
    }
}
