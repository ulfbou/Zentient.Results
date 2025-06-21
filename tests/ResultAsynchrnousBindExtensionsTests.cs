// <copyright file="ResultAsynchrnousBindExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

namespace Zentient.Results.Tests
{
    public class ResultAsynchrnousBindExtensionsTests
    {
        // Helper to create ErrorInfo instances, replacing assumed static factories in ErrorInfo
        private static ErrorInfo CreateErrorInfo(string code, string message, ErrorCategory category = ErrorCategory.General)
        {
            return new ErrorInfo(category, code, message);
        }

        #region Asynchronous Bind Methods

        [Fact]
        public async Task Bind_Async_GenericToGeneric_Success_ExecutesAsyncBinder()
        {
            // Arrange
            IResult<string> initialResult = Result<string>.Success("async_input");
            var binderExecuted = false;
            Func<string, Task<IResult<int>>> next = async s =>
            {
                await Task.Delay(1);
                binderExecuted = true;
                return Result<int>.Success(s.Length);
            };

            // Act
            IResult<int> finalResult = await initialResult.Bind(next);

            // Assert
            finalResult.IsSuccess.Should().BeTrue();
            finalResult.Value.Should().Be("async_input".Length);
            binderExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Bind_Async_GenericToGeneric_Failure_PropagatesFailureWithoutExecutingBinder()
        {
            // Arrange
            var originalError = CreateErrorInfo("NETWORK_DOWN", "Network unavailable.", ErrorCategory.Network);
            IResult<string> initialResult = Result<string>.Failure(originalError);
            var binderExecuted = false;
            Func<string, Task<IResult<int>>> next = async s =>
            {
                binderExecuted = true;
                await Task.Delay(1);
                return Result<int>.Success(0);
            };

            // Act
            IResult<int> finalResult = await initialResult.Bind(next);

            // Assert
            finalResult.IsFailure.Should().BeTrue();
            finalResult.Value.Should().Be(default(int));
            finalResult.Errors.Should().ContainSingle(e => e.Equals(originalError));
            binderExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Bind_Async_GenericToGeneric_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            IResult<string> initialResult = null!;
            Func<string, Task<IResult<int>>> next = s => Task.FromResult(Result<int>.Success(1));

            // Act
            Func<Task> act = async () => await initialResult.Bind(next);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("result");
        }

        [Fact]
        public async Task Bind_Async_GenericToGeneric_NullBinder_ThrowsArgumentNullException()
        {
            // Arrange
            IResult<string> initialResult = Result<string>.Success("data");
            Func<string, Task<IResult<int>>> next = null!;

            // Act
            Func<Task> act = async () => await initialResult.Bind(next);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("next");
        }


        [Fact]
        public async Task Bind_Async_NonGenericToNonGeneric_Success_ExecutesAsyncBinder()
        {
            // Arrange
            IResult initialResult = Result.Success();
            var binderExecuted = false;
            Func<Task<IResult>> next = async () =>
            {
                await Task.Delay(1);
                binderExecuted = true;
                return Result.Success("Async non-generic done.");
            };

            // Act
            IResult finalResult = await initialResult.Bind(next);

            // Assert
            finalResult.IsSuccess.Should().BeTrue();
            finalResult.Messages.Should().ContainSingle("Async non-generic done.");
            binderExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Bind_Async_NonGenericToNonGeneric_Failure_PropagatesFailureWithoutExecutingBinder()
        {
            // Arrange
            var originalError = CreateErrorInfo("API_ERROR", "API call failed.", ErrorCategory.Timeout);
            IResult initialResult = Result.Failure(originalError);
            var binderExecuted = false;
            Func<Task<IResult>> next = async () =>
            {
                binderExecuted = true;
                await Task.Delay(1);
                return Result.Success();
            };

            // Act
            IResult finalResult = await initialResult.Bind(next);

            // Assert
            finalResult.IsFailure.Should().BeTrue();
            finalResult.Errors.Should().ContainSingle(e => e.Equals(originalError));
            binderExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Bind_Async_NonGenericToNonGeneric_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            IResult initialResult = null!;
            Func<Task<IResult>> next = () => Task.FromResult(Result.Success());

            // Act
            Func<Task> act = async () => await initialResult.Bind(next);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("result");
        }

        [Fact]
        public async Task Bind_Async_NonGenericToNonGeneric_NullBinder_ThrowsArgumentNullException()
        {
            // Arrange
            IResult initialResult = Result.Success();
            Func<Task<IResult>> next = null!;

            // Act
            Func<Task> act = async () => await initialResult.Bind(next);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("next");
        }

        #endregion
    }
}
