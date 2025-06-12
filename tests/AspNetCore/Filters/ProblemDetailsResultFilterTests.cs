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
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        public const string ProblemTypeUri = "https://your.api/problems/";

        private ProblemDetailsResultFilter CreateFilter()
        {
            var problemDetailsFactoryMock = new Mock<ProblemDetailsFactory>();
            problemDetailsFactoryMock
                .Setup(f => f.CreateProblemDetails(
                    It.IsAny<HttpContext>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()
                ))
                .Returns((HttpContext httpContext, int? statusCode, string title, string type, string detail, string instance) =>
                {
                    var pd = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = title,
                        Type = type,
                        Detail = detail,
                        Instance = instance
                    };
                    return pd;
                });

            problemDetailsFactoryMock
               .Setup(f => f.CreateValidationProblemDetails(
                   It.IsAny<HttpContext>(),
                   It.IsAny<ModelStateDictionary>(),
                   It.IsAny<int?>(),
                   It.IsAny<string?>(),
                   It.IsAny<string?>(),
                   It.IsAny<string?>(),
                   It.IsAny<string?>() // Corrected: Added It.IsAny<string?>() for the 'instance' parameter
               ))
               .Returns((HttpContext httpContext, ModelStateDictionary modelState, int? statusCode, string title, string type, string detail, string instance) =>
               {
                   var vpd = new ValidationProblemDetails(modelState)
                   {
                       Status = statusCode,
                       Title = title,
                       Type = type,
                       Detail = detail,
                       Instance = instance
                   };
                   return vpd;
               });

            var options = Options.Create(new ProblemDetailsOptions());
            var zentientOptions = Options.Create(new ZentientProblemDetailsOptions { ProblemTypeBaseUri = ProblemTypeUri });

            return new ProblemDetailsResultFilter(
                problemDetailsFactoryMock.Object,
                options,
                zentientOptions
            );
        }

        [Fact]
        public void Ctor_Throws_If_ProblemDetailsFactory_Null()
        {
            var options = Options.Create(new ProblemDetailsOptions());
            var zentientOptions = Options.Create(new ZentientProblemDetailsOptions { ProblemTypeBaseUri = ProblemTypeUri });
            Assert.Throws<ArgumentNullException>(() => new ProblemDetailsResultFilter(null!, options, zentientOptions));
        }

        // In Zentient.Results.Tests.AspNetCore.Filters.ProblemDetailsResultFilterTests.cs

        [Fact]
        public async Task OnResultExecutionAsync_Converts_IResult_ObjectResult_To_ActionResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            // FIX: Explicitly create a failure result with a status code of 500 (Internal Server Error)
            var zentientResult = Result.Failure(
                new ErrorInfo(ErrorCategory.General, "GEN-001", "Something went wrong."),
                ResultStatuses.Error // <-- Pass ResultStatuses.Error here to ensure 500
            );
            var objectResult = new ObjectResult(zentientResult);

            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor()),
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
                return Task.FromResult<ResultExecutedContext>(new ResultExecutedContext(context, context.Filters, context.Result, controller: new object()));
            });

            // Assert
            context.Result.Should().BeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.Value.Should().BeOfType<ProblemDetails>();
            result.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            result.ContentTypes.Should().Contain("application/problem+json");

            var problemDetails = (ProblemDetails)result.Value;
            problemDetails.Status.Should().Be((int)HttpStatusCode.InternalServerError);
            problemDetails.Type.Should().Be($"{ProblemTypeUri}gen-001"); // Assuming this is now the correct expected type
            problemDetails.Detail.Should().Be("Something went wrong.");

            problemDetails.Extensions.Should().ContainKey("zentientErrors");

            // FIX START: Correctly cast the zentientErrors extension
            // Cast to List<Dictionary<string, object>>
            var zentientErrors = (List<Dictionary<string, object>>)problemDetails.Extensions["zentientErrors"]!;

            // Now you can assert against the correctly typed list
            zentientErrors.Should().HaveCount(1);
            // Access the dictionary element and then its key
            ((Dictionary<string, object>)zentientErrors[0])["code"].Should().Be("GEN-001");
            // FIX END

            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task OnResultExecutionAsync_Converts_IResultT_ObjectResult_To_ActionResult_Success()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var zentientResult = Result<string>.Success("SomeValue", message: "Operation successful.");
            var objectResult = new ObjectResult(zentientResult);

            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor()),
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
                return Task.FromResult<ResultExecutedContext>(new ResultExecutedContext(context, context.Filters, context.Result, controller: new object()));
            });

            // Assert
            context.Result.Should().BeOfType<OkObjectResult>();
            var result = (OkObjectResult)context.Result;
            result.Value.Should().Be("SomeValue");
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            nextCalled.Should().BeTrue();
        }

        // In Zentient.Results.Tests.AspNetCore.Filters.ProblemDetailsResultFilterTests.cs

        [Fact]
        public async Task OnResultExecutionAsync_Handles_Task_ObjectResult_Value_Refactored()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();

            // Define the error message for the status
            const string RequestTimeoutErrorMessage = "Request timed out."; // Consistent with ErrorInfo.Message

            // Use GetStatus to explicitly define the 408 Request Timeout status
            var zentientResultInsideTask = Result.Failure(
                new ErrorInfo(ErrorCategory.Timeout, "TIM-001", RequestTimeoutErrorMessage),
                ResultStatuses.GetStatus((int)HttpStatusCode.RequestTimeout, RequestTimeoutErrorMessage)
            );
            var taskResult = Task.FromResult<object>(zentientResultInsideTask);
            var objectResult = new ObjectResult(taskResult);

            var context = new ResultExecutingContext(
                new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor()),
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
                return Task.FromResult<ResultExecutedContext>(new ResultExecutedContext(context, context.Filters, context.Result, controller: new object()));
            });

            // Assert
            context.Result.Should().BeOfType<ObjectResult>();
            var result = (ObjectResult)context.Result;
            result.Value.Should().BeOfType<ProblemDetails>();
            result.StatusCode.Should().Be((int)HttpStatusCode.RequestTimeout); // Expected 408
            result.ContentTypes.Should().Contain("application/problem+json");

            var problemDetails = (ProblemDetails)result.Value;
            problemDetails.Status.Should().Be((int)HttpStatusCode.RequestTimeout); // Expected 408
            problemDetails.Type.Should().StartWith(ProblemTypeUri).And.EndWith("tim-001");
            problemDetails.Detail.Should().Be(RequestTimeoutErrorMessage); // Use the constant
            problemDetails.Extensions.Should().ContainKey("zentientErrors");

            nextCalled.Should().BeTrue();
        }

        [Fact]
        public void ConvertZentientResultToActionResult_Returns_ValidationProblemDetails_For_ValidationFailure()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var filter = CreateFilter();
            var validationError = new ErrorInfo(ErrorCategory.Validation, "Name", "Name is required.", Data: "Name");
            var validationResult = Result.Validation(new[] { validationError });

            // Act (using reflection for internal method)
            var result = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { validationResult, httpContext });

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
            var objResult = (UnprocessableEntityObjectResult)result!;
            objResult.StatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
            objResult.ContentTypes.Should().Contain("application/problem+json");

            objResult.Value.Should().BeOfType<ValidationProblemDetails>();
            var validationProblemDetails = (ValidationProblemDetails)objResult.Value;
            validationProblemDetails.Status.Should().Be((int)HttpStatusCode.UnprocessableEntity);
            validationProblemDetails.Title.Should().Be(ResultStatuses.UnprocessableEntity.Description); // Should use correct title
            validationProblemDetails.Type.Should().Be($"{ProblemTypeUri}validation");
            validationProblemDetails.Errors.Should().ContainKey("Name");
            validationProblemDetails.Errors["Name"].Should().Contain("Name is required.");
            validationProblemDetails.Extensions.Should().ContainKey("zentientErrors");
        }

        [Fact]
        public void ConvertZentientResultToActionResult_Returns_ProblemDetails_For_GenericFailure()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var filter = CreateFilter();

            // Explicitly create a failure result with a status code of 500 (Internal Server Error).
            // The ErrorInfo includes a specific code "GEN-002".
            var failureResult = Result.Failure(
                new ErrorInfo(ErrorCategory.General, "GEN-002", "General error message."), // ErrorCode "GEN-002"
                ResultStatuses.Error // Status 500
            );

            // Act (using reflection for internal method)
            var result = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { failureResult, httpContext });

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objResult = (ObjectResult)result!;
            objResult.Value.Should().BeOfType<ProblemDetails>();
            objResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            objResult.ContentTypes.Should().Contain("application/problem+json");

            var problemDetails = (ProblemDetails)objResult.Value;
            problemDetails.Status.Should().Be((int)HttpStatusCode.InternalServerError);
            problemDetails.Title.Should().Be(ResultStatuses.Error.Description);
            problemDetails.Detail.Should().Be("General error message.");
            // FIXED ASSERTION:
            // The ProblemDetails.Type is derived from the ErrorInfo.Code ("GEN-002") due to the filter's logic prioritization.
            problemDetails.Type.Should().Be($"{ProblemTypeUri}gen-002"); // <-- Updated to expect 'gen-002'

            problemDetails.Extensions.Should().ContainKey("zentientErrors");
        }

        [Fact]
        public void ConvertZentientResultToActionResult_Returns_CorrectResult_For_Success()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var filter = CreateFilter();

            // Act 1: Non-generic success (Result.Success() which has status 200 OK)
            // The filter's logic translates this to NoContentResult (HTTP 204)
            var successResult = Result.Success();
            var result1 = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { successResult, httpContext });

            // Act 2: Generic success with value (Result<string>.Success("test-value") which has status 200 OK)
            // The filter's logic translates this to OkObjectResult (HTTP 200) with the value
            var successResultWithObject = Result<string>.Success("test-value");
            var result2 = filter.GetType()
                .GetMethod("ConvertZentientResultToActionResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(filter, new object[] { successResultWithObject, httpContext });

            // Assert 1
            result1.Should().BeOfType<NoContentResult>();
            ((NoContentResult)result1!).StatusCode.Should().Be((int)HttpStatusCode.NoContent);

            // Assert 2
            result2.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result2!).Value.Should().Be("test-value");
            ((OkObjectResult)result2!).StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}
