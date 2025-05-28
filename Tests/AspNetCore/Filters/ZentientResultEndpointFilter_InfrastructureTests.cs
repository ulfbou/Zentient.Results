using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
    /// objects and returns appropriate HTTP results.
    /// These tests cover various scenarios including success, failure, and different HTTP status codes,
    /// as well as ensuring that the filter correctly handles generic and non-generic results.
    /// The tests also verify that the filter integrates properly with ASP.NET Core's
    /// <see cref="ProblemDetailsFactory"/> and that it generates the expected
    /// <see cref="ProblemDetails"/> responses for failure cases.
    /// </summary>
    public partial class ZentientResultEndpointFilterTests
    {
        [Fact]
        public async Task InvokeAsync_ResolvesProblemDetailsFactory_FromDI()
        {
            // Arrange
            var errorInfo = new ErrorInfo(ErrorCategory.General, "ERR001", "Test error");
            var result = new ConcreteResult
            {
                IsSuccess = false,
                Status = new MockResultStatus((int)HttpStatusCode.InternalServerError, "Internal Server Error"),
                Errors = new List<ErrorInfo> { errorInfo },
                Error = errorInfo.Message
            };

            var pdf = new Mock<ProblemDetailsFactory>();
            const string testProblemTypeBaseUri = "https://yourdomain.com/errors/";
            string expectedProblemType = $"{testProblemTypeBaseUri}{errorInfo.Code.ToLowerInvariant()}";

            pdf.Setup(x => x.CreateProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns((HttpContext ctx, int? st, string? ti, string? tyPassedToFactory, string? det, string? inst) =>
            {
                return new ProblemDetails
                {
                    Status = st,
                    Title = ti,
                    Type = tyPassedToFactory,
                    Detail = det,
                    Instance = inst
                };
            });

            var sp = CreateServiceProvider(pdf);
            var httpContext = CreateHttpContext(sp);
            var context = CreateContext(httpContext);
            var options = Microsoft.Extensions.Options.Options.Create(new ZentientProblemDetailsOptions { ProblemTypeBaseUri = testProblemTypeBaseUri });
            var filter = new ZentientResultEndpointFilter(pdf.Object, options);
            var next = new EndpointFilterDelegate(_ => ValueTask.FromResult<object?>(result));

            // Act
            var actualResult = await filter.InvokeAsync(context.Object, next);

            // Assert
            actualResult.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();

            var problemHttpResult = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)actualResult!;
            problemHttpResult.ProblemDetails.Should().NotBeNull();
            problemHttpResult.ProblemDetails.Status.Should().Be(500);
            problemHttpResult.ProblemDetails.Title.Should().Be("Internal Server Error");
            problemHttpResult.ProblemDetails.Detail.Should().Be(errorInfo.Message);
            problemHttpResult.ProblemDetails.Type.Should().Be(expectedProblemType);

            pdf.Verify(x => x.CreateProblemDetails(
                httpContext.Object,
                500,
                "Internal Server Error",
                expectedProblemType,
                errorInfo.Message,
                It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ProblemDetailsFactoryReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var result = new ConcreteResult
            {
                IsSuccess = false,
                Status = new MockResultStatus((int)HttpStatusCode.InternalServerError, "Internal Server Error"),
                Errors = new List<ErrorInfo> { new ErrorInfo(ErrorCategory.Exception, "EX001", "Simulated exception") },
                Error = "Internal Server Error"
            };

            var pdf = new Mock<ProblemDetailsFactory>();
            pdf.Setup(x => x.CreateProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns((ProblemDetails)null!);

            var sp = CreateServiceProvider(pdf);
            var httpContext = CreateHttpContext(sp);
            var context = CreateContext(httpContext);
            var options = Microsoft.Extensions.Options.Options.Create(new ZentientProblemDetailsOptions());
            var filter = new ZentientResultEndpointFilter(pdf.Object, options);
            var next = new EndpointFilterDelegate(_ => ValueTask.FromResult<object?>(result));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await filter.InvokeAsync(context.Object, next)
            );

            exception.Message.Should().Be("ProblemDetailsFactory returned null ProblemDetails.");
            pdf.Verify(x => x.CreateProblemDetails(
            It.IsAny<HttpContext>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
            ), Times.Once);
        }
    }
}
