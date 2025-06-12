using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using Moq;

using Zentient.Results.AspNetCore.Configuration;

using Zentient.Results.AspNetCore.Filters;

namespace Zentient.Results.Tests.AspNetCore.Filters
{
    /// <summary>
    /// Unit tests for <see cref="Results.AspNetCore.Filters.ZentientResultEndpointFilter"/> to ensure it correctly processes
    /// ZentientResult objects and returns appropriate HTTP results.
    /// These tests cover various scenarios including success, failure, and different HTTP status codes,
    /// as well as ensuring that the filter correctly handles generic and non-generic results.
    /// The tests also verify that the filter integrates properly with ASP.NET Core's ProblemDetailsFactory
    /// and that it generates the expected ProblemDetails responses for failure cases.
    /// </summary>
    public partial class ZentientResultEndpointFilterTests
    {
        [Fact]
        public async Task InvokeAsync_ReturnsOriginalResult_IfNotZentientResult()
        {
            // Arrange
            var pdfMock = new Mock<ProblemDetailsFactory>();
            var options = Microsoft.Extensions.Options.Options.Create(new ZentientProblemDetailsOptions());
            var filter = new ZentientResultEndpointFilter(pdfMock.Object, options);
            var context = new Mock<EndpointFilterInvocationContext>();
            var nonZentientResult = 42;
            var next = new EndpointFilterDelegate(_ => ValueTask.FromResult<object?>(nonZentientResult));

            // Act
            var result = await filter.InvokeAsync(context.Object, next);

            // Assert
            result.Should().Be(nonZentientResult);
        }
    }
}
