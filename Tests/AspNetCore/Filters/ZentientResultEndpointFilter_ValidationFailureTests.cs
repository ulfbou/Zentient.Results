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
    /// objects that represent validation failures, including those with multiple validation errors.
    /// These tests verify that the filter generates appropriate <see cref="ValidationProblemDetails"/>
    /// responses with the expected status code, title, detail, and error messages,
    /// and that it correctly maps validation errors to the ProblemDetails.Errors dictionary.
    /// The tests also ensure that the filter integrates properly with ASP.NET Core's
    /// <see cref="ProblemDetailsFactory"/> to create the validation problem details
    /// and that it handles both generic and non-generic results correctly.
    /// </summary>
    public partial class ZentientResultEndpointFilter_ValidationFailureTests
    {
        [Fact]
        public async Task InvokeAsync_FailureResult_WithValidationErrors_CallsCreateValidationProblemDetails()
        {
            // Arrange
            var validationErrors = new List<ErrorInfo>
            {
                new ErrorInfo(ErrorCategory.Validation, "field1", "Error message 1", Data: "field1"),
                new ErrorInfo(ErrorCategory.Validation, "", "Error message 2", Data: "GeneralError"),
                new ErrorInfo(ErrorCategory.Validation, null!, "Error message 3")
            };
            var statusCode = (int)HttpStatusCode.BadRequest;
            var resultDescription = ResultStatuses.BadRequest.Description;
            var resultErrorDetail = "Validation failed.";

            var result = new ConcreteResult
            {
                IsSuccess = false,
                Status = new MockResultStatus(statusCode, resultDescription),
                Errors = validationErrors,
                Error = resultErrorDetail
            };

            var expectedProblem = new ProblemDetails { Title = "Validation Failed", Status = statusCode };
            var pdf = new Mock<ProblemDetailsFactory>();
            pdf.Setup(x => x.CreateValidationProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<ModelStateDictionary>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns((HttpContext ctx, ModelStateDictionary ms, int? st, string? ti, string? ty, string? det, string? inst) =>
            {
                return new ValidationProblemDetails(ms)
                {
                    Status = st,
                    Title = ti,
                    Type = ty,
                    Detail = det,
                    Instance = inst
                };
            });

            var sp = CreateServiceProvider(pdf);
            var httpContext = CreateHttpContext(sp, "/api/validate");
            var context = CreateContext(httpContext);
            var options = Microsoft.Extensions.Options.Options.Create(new ZentientProblemDetailsOptions());
            var filter = new ZentientResultEndpointFilter(pdf.Object, options);
            var next = new EndpointFilterDelegate(_ => ValueTask.FromResult<object?>(result));

            // Act
            var actualResult = await filter.InvokeAsync(context.Object, next);

            // Assert
            actualResult.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();

            var problemResult = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)actualResult!;
            problemResult.ProblemDetails.Should().BeOfType<ValidationProblemDetails>();

            var validationProblemDetails = (ValidationProblemDetails)problemResult.ProblemDetails;
            validationProblemDetails.Status.Should().Be(statusCode);
            validationProblemDetails.Title.Should().Be(resultDescription);
            validationProblemDetails.Detail.Should().Be(resultErrorDetail);
            validationProblemDetails.Type.Should().StartWith("https://");
            validationProblemDetails.Errors.Should().ContainKey("field1")
                .And.ContainKey("GeneralError")
                .And.ContainKey("General");

            validationProblemDetails.Errors["field1"].Should().Contain("Error message 1");
            validationProblemDetails.Errors["GeneralError"].Should().Contain("Error message 2");
            validationProblemDetails.Errors["General"].Should().Contain("Error message 3");

            pdf.Verify(x => x.CreateValidationProblemDetails(
                httpContext.Object,
                It.Is<ModelStateDictionary>(ms =>
                    ms.Count == 3 &&
                    ms["field1"]!.Errors.Any(e => e.ErrorMessage == "Error message 1") &&
                    ms["GeneralError"]!.Errors.Any(e => e.ErrorMessage == "Error message 2") &&
                    ms["General"]!.Errors.Any(e => e.ErrorMessage == "Error message 3")
                ),
                statusCode,
                resultDescription,
                It.IsAny<string>(),
                resultErrorDetail,
                It.IsAny<string>()
            ), Times.Once);
            pdf.Verify(x => x.CreateProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_FailureResult_UnprocessableEntityStatus_CallsCreateValidationProblemDetails()
        {
            // Arrange
            var statusCode = (int)HttpStatusCode.UnprocessableEntity;
            var resultDescription = ResultStatuses.UnprocessableEntity.Description;
            var resultErrorDetail = "Request entity is unprocessable.";

            var result = new ConcreteResult
            {
                IsSuccess = false,
                Status = new MockResultStatus(statusCode, resultDescription),
                Errors = Array.Empty<ErrorInfo>(),
                Error = resultErrorDetail
            };

            var expectedProblem = new ProblemDetails { Title = "Request Error", Status = statusCode };

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
            pdf.Setup(x => x.CreateValidationProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<ModelStateDictionary>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null
            ))
            .Returns((HttpContext ctx, ModelStateDictionary ms, int? st, string? ti, string? ty, string? det, string? inst) =>
            {
                return new ValidationProblemDetails(ms)
                {
                    Status = st,
                    Title = ti,
                    Type = ty,
                    Detail = det,
                    Instance = inst
                };
            });

            var sp = CreateServiceProvider(pdf);
            var httpContext = CreateHttpContext(sp, "/api/unprocessable");
            var context = CreateContext(httpContext);
            var options = Microsoft.Extensions.Options.Options.Create(new ZentientProblemDetailsOptions());
            var filter = new ZentientResultEndpointFilter(pdf.Object, options);
            var next = new EndpointFilterDelegate(_ => ValueTask.FromResult<object?>(result));

            // Act
            var actualResult = await filter.InvokeAsync(context.Object, next);

            // Assert
            actualResult.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();

            var problemResult = (Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)actualResult!;
            problemResult.ProblemDetails.Should().BeOfType<ValidationProblemDetails>();

            pdf.Verify(x => x.CreateProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null
            ), Times.Never);
            pdf.Verify(x => x.CreateValidationProblemDetails(
                httpContext.Object,
                It.Is<ModelStateDictionary>(ms => !ms.Any()),
                statusCode,
                resultDescription,
                It.IsAny<string>(),
                resultErrorDetail,
                It.IsAny<string>()
            ), Times.Once);
            pdf.Verify(x => x.CreateValidationProblemDetails(
                httpContext.Object,
                It.Is<ModelStateDictionary>(ms => !ms.Any()),
                statusCode,
                resultDescription,
                It.IsAny<string>(),
                resultErrorDetail,
                null
            ), Times.Once);
        }
    }
}
