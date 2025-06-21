// <copyright file="ResultSynchronousBindExtensions.cs" company="Zentient Framework Team">
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
    public class ResultSynchronousBindExtensions
    {
        // Helper to create ErrorInfo instances, replacing assumed static factories in ErrorInfo
        private static ErrorInfo CreateErrorInfo(string code, string message, ErrorCategory category = ErrorCategory.General)
        {
            return new ErrorInfo(category, code, message);
        }

        #region Synchronous Bind Methods

        [Fact]
        public void Bind_GenericToNonGeneric_Success_ExecutesBinderAndReturnsResult()
        {
            // Arrange
            IResult<string> initialResult = Result<string>.Success("input_data");
            var binderExecuted = false;
            Func<string, IResult> binder = s =>
            {
                binderExecuted = true;
                s.Should().Be("input_data");
                return Result.Success($"Processed: {s}");
            };

            // Act
            IResult finalResult = initialResult.Bind(binder);

            // Assert
            finalResult.IsSuccess.Should().BeTrue();
            finalResult.Messages.Should().ContainSingle("Processed: input_data");
            binderExecuted.Should().BeTrue();
        }

        [Fact]
        public void Bind_GenericToNonGeneric_Failure_PropagatesFailure()
        {
            // Arrange
            var originalError = CreateErrorInfo("ORIGINAL_ERROR", "Failed at step 1.", ErrorCategory.General);
            IResult<string> initialResult = Result<string>.Failure(originalError);
            var binderExecuted = false;
            Func<string, IResult> binder = s => { binderExecuted = true; return Result.Success(); };

            // Act
            IResult finalResult = initialResult.Bind(binder);

            // Assert
            finalResult.IsFailure.Should().BeTrue();
            finalResult.Errors.Should().ContainSingle(e => e.Equals(originalError));
            binderExecuted.Should().BeFalse();
        }

        [Fact]
        public void Bind_GenericToNonGeneric_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            IResult<string> initialResult = null!;
            Func<string, IResult> binder = s => Result.Success();

            // Act
            Action act = () => initialResult.Bind(binder);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void Bind_GenericToNonGeneric_NullBinder_ThrowsArgumentNullException()
        {
            // Arrange
            IResult<string> initialResult = Result<string>.Success("data");
            Func<string, IResult> binder = null!;

            // Act
            Action act = () => initialResult.Bind(binder);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("binder");
        }

        [Fact]
        public void Bind_NonGenericToNonGeneric_Success_ExecutesBinderAndReturnsResult()
        {
            // Arrange
            IResult initialResult = Result.Success();
            var binderExecuted = false;
            Func<IResult> binder = () =>
            {
                binderExecuted = true;
                return Result.Success("Step 2 completed.");
            };

            // Act
            IResult finalResult = initialResult.Bind(binder);

            // Assert
            finalResult.IsSuccess.Should().BeTrue();
            finalResult.Messages.Should().ContainSingle("Step 2 completed.");
            binderExecuted.Should().BeTrue();
        }

        [Fact]
        public void Bind_NonGenericToNonGeneric_Failure_PropagatesFailure()
        {
            // Arrange
            var originalError = CreateErrorInfo("ERROR_STEP1", "First step failed.");
            IResult initialResult = Result.Failure(originalError);
            var binderExecuted = false;
            Func<IResult> binder = () => { binderExecuted = true; return Result.Success(); };

            // Act
            IResult finalResult = initialResult.Bind(binder);

            // Assert
            finalResult.IsFailure.Should().BeTrue();
            finalResult.Errors.Should().ContainSingle(e => e.Equals(originalError));
            binderExecuted.Should().BeFalse();
        }

        [Fact]
        public void Bind_NonGenericToNonGeneric_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            IResult initialResult = null!;
            Func<IResult> binder = () => Result.Success();

            // Act
            Action act = () => initialResult.Bind(binder);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void Bind_NonGenericToNonGeneric_NullBinder_ThrowsArgumentNullException()
        {
            // Arrange
            IResult initialResult = Result.Success();
            Func<IResult> binder = null!;

            // Act
            Action act = () => initialResult.Bind(binder);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("binder");
        }

        [Fact]
        public void Bind_NonGenericToGeneric_Success_ExecutesBinderAndReturnsResult()
        {
            // Arrange
            IResult initialResult = Result.Success();
            var binderExecuted = false;
            Func<IResult<int>> binder = () =>
            {
                binderExecuted = true;
                return Result<int>.Success(99);
            };

            // Act
            IResult<int> finalResult = initialResult.Bind(binder);

            // Assert
            finalResult.IsSuccess.Should().BeTrue();
            finalResult.Value.Should().Be(99);
            binderExecuted.Should().BeTrue();
        }

        [Fact]
        public void Bind_NonGenericToGeneric_Failure_PropagatesFailure()
        {
            // Arrange
            var originalError = CreateErrorInfo("PERMISSION_DENIED", "No access.", ErrorCategory.Authorization);
            IResult initialResult = Result.Failure(originalError);
            var binderExecuted = false;
            Func<IResult<string>> binder = () => { binderExecuted = true; return Result<string>.Success("data"); };

            // Act
            IResult<string> finalResult = initialResult.Bind(binder);

            // Assert
            finalResult.IsFailure.Should().BeTrue();
            finalResult.Value.Should().BeNull();
            finalResult.Errors.Should().ContainSingle(e => e.Equals(originalError));
            binderExecuted.Should().BeFalse();
        }

        [Fact]
        public void Bind_NonGenericToGeneric_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            IResult initialResult = null!;
            Func<IResult<int>> binder = () => Result<int>.Success(1);

            // Act
            Action act = () => initialResult.Bind(binder);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void Bind_NonGenericToGeneric_NullBinder_ThrowsArgumentNullException()
        {
            // Arrange
            IResult initialResult = Result.Success();
            Func<IResult<int>> binder = null!;

            // Act
            Action act = () => initialResult.Bind(binder);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("binder");
        }

        #endregion
    }
}
