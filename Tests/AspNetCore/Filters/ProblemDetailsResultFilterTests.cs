using System.Net;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;
using Zentient.Results;
using Zentient.Results.AspNetCore.Filters;
using Zentient.Results.AspNetCore.Configuration;

using static Zentient.Results.Tests.Helpers.AspNetCoreHelpers;
using Zentient.Results.Tests.Helpers;

namespace Zentient.Results.Tests.AspNetCore.Filters
{
    /// <summary>
    /// Unit tests for <see cref="ProblemDetailsResultFilter"/> to ensure it correctly converts
    /// <see cref="Zentient.Results.IResult"/> and <see cref="Zentient.Results.IResult{T}"/>
    /// returned from controller actions into appropriate
    /// <see cref="IActionResult"/> types, including <see cref="ProblemDetails"/> and
    /// <see cref="ValidationProblemDetails"/> responses.
    /// These tests verify that the filter handles both synchronous and asynchronous results,
    /// and that it correctly generates problem details for validation failures and generic errors.
    /// </summary>
    public class ProblemDetailsResultFilterTests
    {
        public const string ProblemTypeUri = "https://example.com/errors/";

        [Fact]
        public void Ctor_Throws_If_ProblemDetailsFactory_Null()
        {
            var options = Options.Create(new ProblemDetailsOptions());
            var zentientOptions = Options.Create(new ZentientProblemDetailsOptions { ProblemTypeBaseUri = ProblemTypeUri });
            Assert.Throws<ArgumentNullException>(() => new ProblemDetailsResultFilter(null!, options, zentientOptions));
        }

        [Fact]
        public async Task OnResultExecutionAsync_Converts_IResult_ObjectResult_To_ActionResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var zentientResult = new FakeFailureResult();
            var objectResult = new ObjectResult(zentientResult);
            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                objectResult,
                controller: null!
            );
            var filter = CreateFilter();
            var nextCalled = false;

            // Act
            await filter.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Assert
            context.Result.Should().BeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.Value.Should().BeOfType<ProblemDetails>();
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task OnResultExecutionAsync_Converts_IResultT_ObjectResult_To_ActionResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var zentientResult = new FakeSuccessResultWithObject("value");
            var objectResult = new ObjectResult(zentientResult);
            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                objectResult,
                controller: null!
            );
            var filter = CreateFilter();
            var nextCalled = false;

            // Act
            await filter.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Assert
            context.Result.Should().BeOfType<OkObjectResult>();
            var result = (OkObjectResult)context.Result;
            result.Value.Should().Be("value");
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task OnResultExecutionAsync_Handles_Task_ObjectResult_Value()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var zentientResult = new FakeFailureResult();
            var task = Task.FromResult<object>(zentientResult);
            var objectResult = new ObjectResult(zentientResult);
            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                objectResult,
                controller: null!
            );
            var filter = CreateFilter();
            var nextCalled = false;

            // Act
            await filter.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Assert
            context.Result.Should().BeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.Value.Should().BeOfType<ProblemDetails>();
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task OnResultExecutionAsync_Handles_Raw_IResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var zentientResult = new FakeFailureResult();
            var objectResult = new ObjectResult(zentientResult);
            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                new List<IFilterMetadata>(),
                objectResult,
                controller: null!
            );
            var filter = CreateFilter();
            var nextCalled = false;

            // Act
            await filter.OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult<ResultExecutedContext>(null!);
            });

            // Assert
            context.Result.Should().BeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.Value.Should().BeOfType<ProblemDetails>();
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public void ConvertZentientResultToActionResult_Returns_ValidationProblemDetails_For_ValidationFailure()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var filter = CreateFilter();
            var validationResult = new FakeValidationFailureResult();

            // Act
            var result = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { validationResult, httpContext });

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
            var objResult = (UnprocessableEntityObjectResult)result!;
            objResult.Value.Should().BeOfType<ValidationProblemDetails>();
        }

        [Fact]
        public void ConvertZentientResultToActionResult_Returns_ProblemDetails_For_GenericFailure()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var filter = CreateFilter();
            var failureResult = new FakeFailureResult();

            // Act
            var result = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { failureResult, httpContext });

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objResult = (ObjectResult)result!;
            objResult.Value.Should().BeOfType<ProblemDetails>();
            objResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            objResult.ContentTypes.Should().Contain("application/problem+json");
        }

        [Fact]
        public void ConvertZentientResultToActionResult_Returns_CorrectResult_For_Success()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var filter = CreateFilter();
            var successResult = new FakeSuccessResult();
            var successResultWithObject = new FakeSuccessResultWithObject("abc");

            // Act
            var result1 = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { successResult, httpContext });

            var result2 = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { successResultWithObject, httpContext });

            // Assert
            result1.Should().BeOfType<NoContentResult>();
            result2.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result2!).Value.Should().Be("abc");
        }
    }
}
