using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using Zentient.Results.AspNetCore.Configuration;
using Zentient.Results.AspNetCore.Filters;
using Zentient.Results.Tests.Helpers;

using static Zentient.Results.Tests.Helpers.AspNetCoreHelpers;

namespace Zentient.Results.Tests.AspNetCore.Filters
{
    /// <summary>
    /// Unit tests for <see cref="ZentientResultEndpointFilter"/> to ensure it correctly processes
    /// <see cref="Zentient.Results.IResult"/> and <see cref="Zentient.Results.IResult{T}"/> 
    /// returned from controller actions into appropriate 
    /// <see cref="ProblemDetails"/> and <see cref="ValidationProblemDetails"/> responses.
    /// These tests verify that the filter handles both synchronous and asynchronous results,
    /// and that it correctly generates problem details for validation failures and generic errors.
    /// </summary>
    public partial class ZentientResultEndpointFilterTests
    {
        [Fact]
        public async Task InvokeAsync_ToProblemDetails_AddsErrorInfoExtensions()
        {
            // Arrange
            var innerErrorInfo = new ErrorInfo(ErrorCategory.Security, "S001", "Inner security error");
            var errorInfo = new ErrorInfo(ErrorCategory.BusinessLogic, "B001", "Business rule violation", Data: 123, InnerErrors: new List<ErrorInfo> { innerErrorInfo });

            var statusCode = (int)HttpStatusCode.BadRequest;
            var resultDescription = ResultStatuses.BadRequest.Description;
            var resultErrorDetail = "Detailed error message.";

            var result = new ConcreteResult
            {
                IsSuccess = false,
                Status = new MockResultStatus(statusCode, resultDescription),
                Errors = new List<ErrorInfo> { errorInfo },
                Error = resultErrorDetail
            };

            var pdf = new Mock<ProblemDetailsFactory>();
            pdf.Setup(x => x.CreateProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null
            )).Returns((HttpContext ctx, int? st, string? ti, string? ty, string? det, string? inst) =>
            {
                return new ProblemDetails
                {
                    Status = st,
                    Title = ti,
                    Type = ty,
                    Detail = det,
                    Instance = inst
                };
            });

            var sp = CreateServiceProvider(pdf);
            var httpContext = CreateHttpContext(sp, "/api/errors");
            var context = CreateContext(httpContext);
            var options = Microsoft.Extensions.Options.Options.Create(new ZentientProblemDetailsOptions());
            var filter = new ZentientResultEndpointFilter(pdf.Object, options);
            var next = new EndpointFilterDelegate(_ => ValueTask.FromResult<object?>(result));

            // Act
            var actualResult = await filter.InvokeAsync(context.Object, next);

            // Assert
            actualResult.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
            var problemResult = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)actualResult!;

            problemResult.ProblemDetails.Extensions.Should().ContainKey("zentientErrors");
            var zentientErrors = problemResult.ProblemDetails.Extensions["zentientErrors"].Should().BeAssignableTo<List<Dictionary<string, object?>>>().Subject;
            zentientErrors.Should().HaveCount(1);

            var firstErrorDict = zentientErrors.First();

            firstErrorDict.Should().Contain(new Dictionary<string, object?>
            {
                { "category", errorInfo.Category.ToString().ToLowerInvariant() },
                { "code", "B001" },
                { "message", "Business rule violation" },
                { "data", 123 }
            });

            firstErrorDict.Should().ContainKey("innerErrors");
            var innerErrors = firstErrorDict["innerErrors"].Should().BeAssignableTo<List<Dictionary<string, object?>>>().Subject;
            innerErrors.Should().HaveCount(1);
            innerErrors.First().Should().Contain(new Dictionary<string, object?>
            {
                { "category", innerErrorInfo.Category.ToString().ToLowerInvariant() },
                { "code", "S001" },
                { "message", "Inner security error" }
            });
        }

        [Fact]
        public async Task InvokeAsync_FailureResult_ReturnsProblemWithProblemDetails()
        {
            var firstError = new ErrorInfo(ErrorCategory.General, "GEN001", "Something went wrong.");
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var resultDescription = "Internal Server Error";
            var resultErrorDetail = "An unhandled exception occurred.";
            var result = new ConcreteResult
            {
                IsSuccess = false,
                Status = new MockResultStatus(statusCode, resultDescription),
                Errors = new List<ErrorInfo> { firstError },
                Error = resultErrorDetail
            };

            const string expectedProblemTypeBaseUri = "https://yourdomain.com/errors/";
            var expectedProblem = new ProblemDetails
            {
                Title = resultDescription,
                Status = statusCode,
                Detail = resultErrorDetail,
                Type = $"{expectedProblemTypeBaseUri}{firstError.Code.ToLowerInvariant()}"
            };
            expectedProblem.Extensions["zentientErrors"] = new List<Dictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                { "category", firstError.Category.ToString().ToLowerInvariant() },
                { "code", firstError.Code },
                { "message", firstError.Message }
                }
            };

            var pdf = new Mock<ProblemDetailsFactory>();
            pdf.Setup(x => x.CreateProblemDetails(
            It.IsAny<HttpContext>(),
            It.IsAny<int?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
            )).Returns((HttpContext ctx, int? st, string? ti, string? ty, string? det, string? inst) =>
            {
                return new ProblemDetails
                {
                    Status = st,
                    Title = ti,
                    Type = ty,
                    Detail = det,
                    Instance = inst
                };
            });

            var sp = CreateServiceProvider(pdf);
            var httpContext = CreateHttpContext(sp, "/api/test");
            var context = CreateContext(httpContext);

            var options = Microsoft.Extensions.Options.Options.Create(new ZentientProblemDetailsOptions { ProblemTypeBaseUri = expectedProblemTypeBaseUri });
            var filter = new ZentientResultEndpointFilter(pdf.Object, options);
            var next = new EndpointFilterDelegate(_ => ValueTask.FromResult<object?>(result));

            // Act
            var actualResult = await filter.InvokeAsync(context.Object, next);

            // Assert
            actualResult.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();

            var problemResult = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)actualResult!;
            problemResult.ProblemDetails.Should().NotBeNull();
            problemResult.ProblemDetails.Status.Should().Be(statusCode);
            problemResult.ProblemDetails.Title.Should().Be(resultDescription);
            problemResult.ProblemDetails.Detail.Should().Be(resultErrorDetail);
            problemResult.ProblemDetails.Type.Should().Be($"{expectedProblemTypeBaseUri}{firstError.Code.ToLowerInvariant()}");
            problemResult.ProblemDetails.Instance.Should().Be("/api/test");
            problemResult.ProblemDetails.Extensions.Should().ContainKey("zentientErrors");

            var zentientErrors = problemResult.ProblemDetails.Extensions["zentientErrors"].Should().BeAssignableTo<List<Dictionary<string, object?>>>().Subject;
            zentientErrors.Should().HaveCount(1);
            zentientErrors.First()["category"].Should().Be(firstError.Category.ToString().ToLowerInvariant());
            zentientErrors.First()["code"].Should().Be(firstError.Code);
            zentientErrors.First()["message"].Should().Be(firstError.Message);

            pdf.Verify(x => x.CreateProblemDetails(
                httpContext.Object,
                statusCode,
                resultDescription,
                $"{expectedProblemTypeBaseUri}{firstError.Code.ToLowerInvariant()}",
                resultErrorDetail,
                It.IsAny<string>()
            ), Times.Once);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound, ErrorCategory.NotFound)]
        [InlineData(HttpStatusCode.Unauthorized, ErrorCategory.Authentication)]
        [InlineData(HttpStatusCode.Forbidden, ErrorCategory.Security)]
        [InlineData(HttpStatusCode.InternalServerError, ErrorCategory.Exception)]
        [InlineData(HttpStatusCode.BadGateway, ErrorCategory.Network)]
        public async Task InvokeAsync_FailureResult_OtherErrorCategories_CallsCreateProblemDetails(HttpStatusCode httpStatusCode, ErrorCategory errorCategory)
        {
            // Arrange
            var errorInfo = new ErrorInfo(errorCategory, "ERR_CODE", "Error message.");
            var statusCode = (int)httpStatusCode;
            var resultDescription = httpStatusCode.ToString();
            var resultErrorDetail = "Generic error detail.";

            var result = new ConcreteResult
            {
                IsSuccess = false,
                Status = new MockResultStatus(statusCode, resultDescription),
                Errors = new List<ErrorInfo> { errorInfo },
                Error = resultErrorDetail
            };

            var pdf = new Mock<ProblemDetailsFactory>();
            pdf.Setup(x => x.CreateProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns((HttpContext ctx, int? st, string? ti, string? ty, string? det, string? inst) =>
            {
                return new ProblemDetails
                {
                    Status = st,
                    Title = ti,
                    Type = ty,
                    Detail = det,
                    Instance = inst
                };
            });

            var sp = CreateServiceProvider(pdf);
            var httpContext = CreateHttpContext(sp, $"/api/{httpStatusCode.ToString().ToLower()}");
            var context = CreateContext(httpContext);
            var options = Microsoft.Extensions.Options.Options.Create(new ZentientProblemDetailsOptions());
            var filter = new ZentientResultEndpointFilter(pdf.Object, options);
            var next = new EndpointFilterDelegate(_ => ValueTask.FromResult<object?>(result));

            // Act
            var actualResult = await filter.InvokeAsync(context.Object, next);

            // Assert
            actualResult.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
            var problemResult = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)actualResult!;

            problemResult.ProblemDetails.Status.Should().Be(statusCode);
            problemResult.ProblemDetails.Title.Should().Be(resultDescription);
            problemResult.ProblemDetails.Detail.Should().Be(resultErrorDetail);
            problemResult.ProblemDetails.Type.Should().StartWith("https://");
            problemResult.ProblemDetails.Instance.Should().Be($"/api/{httpStatusCode.ToString().ToLower()}");

            pdf.Verify(x => x.CreateProblemDetails(
                httpContext.Object,
                statusCode,
                resultDescription,
                It.IsAny<string>(),
                resultErrorDetail,
                It.IsAny<string>()
            ), Times.Once);
            pdf.Verify(x => x.CreateValidationProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<ModelStateDictionary>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_FailureGenericResult_ReturnsProblemWithProblemDetails()
        {
            var errorInfo = new ErrorInfo(ErrorCategory.NotFound, "ITEM_NOT_FOUND", "The requested item could not be found.");
            var statusCode = (int)HttpStatusCode.NotFound;
            var resultDescription = ResultStatuses.NotFound.Description;
            var resultErrorDetail = "Detailed message about item not found.";
            var result = new ConcreteResult<string>
            {
                IsSuccess = false,
                Status = new MockResultStatus(statusCode, resultDescription),
                Errors = new List<ErrorInfo> { errorInfo },
                Error = resultErrorDetail,
                Value = null
            };

            var pdf = new Mock<ProblemDetailsFactory>();
            pdf.Setup(x => x.CreateProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns((HttpContext ctx, int? st, string? ti, string? ty, string? det, string? inst) =>
            {
                return new ProblemDetails
                {
                    Status = st,
                    Title = ti,
                    Type = ty,
                    Detail = det,
                    Instance = inst
                };
            });

            var sp = CreateServiceProvider(pdf);

            const string expectedBaseUri = "https://yourdomain.com/errors/";
            var filter = new ZentientResultEndpointFilter(
                pdf.Object,
                Microsoft.Extensions.Options.Options.Create(new ZentientProblemDetailsOptions { ProblemTypeBaseUri = expectedBaseUri })
            );

            var httpContext = CreateHttpContext(sp, "/api/items/123");
            var context = CreateContext(httpContext);
            var next = new EndpointFilterDelegate(_ => ValueTask.FromResult<object?>(result));

            // Act
            var actualResult = await filter.InvokeAsync(context.Object, next);

            // Assert
            actualResult.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();

            var problemResult = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)actualResult!;
            problemResult.ProblemDetails.Status.Should().Be(statusCode);
            problemResult.ProblemDetails.Title.Should().Be(resultDescription);
            problemResult.ProblemDetails.Detail.Should().Be(resultErrorDetail);
            problemResult.ProblemDetails.Type.Should().Be($"{expectedBaseUri}{errorInfo.Code.ToLowerInvariant()}");
            problemResult.ProblemDetails.Instance.Should().Be("/api/items/123");

            pdf.Verify(x => x.CreateProblemDetails(
                httpContext.Object,
                statusCode,
                resultDescription,
                $"{expectedBaseUri}{errorInfo.Code.ToLowerInvariant()}",
                resultErrorDetail,
                It.IsAny<string>()
            ), Times.Once);
        }
    }
}
