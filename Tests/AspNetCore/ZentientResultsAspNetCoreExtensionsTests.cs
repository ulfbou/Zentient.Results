using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Zentient.Results.AspNetCore;
using Zentient.Results.AspNetCore.Filters;
using Xunit;

namespace Zentient.Results.Tests.AspNetCore
{
    /// <summary>
    /// Unit tests for <see cref="ZentientResultsAspNetCoreExtensions"/> to ensure proper registration of services 
    /// and configuration of ASP.NET Core options.
    /// These tests verify that the necessary services are registered correctly and that the options
    /// are configured as expected, including the customization of <see cref="ProblemDetails"/> and
    /// <see cref="ValidationProblemDetails"/> responses.
    /// </summary>
    public class ZentientResultsAspNetCoreExtensionsTests
    {
        [Fact]
        public void AddZentientResultsAspNetCore_Registers_ProblemDetailsFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            ZentientResultsAspNetCoreExtensions.AddZentientResultsAspNetCore(services, null, null);
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<ProblemDetailsFactory>().Should().BeOfType<DefaultProblemDetailsFactory>();
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Registers_HttpContextAccessor()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            ZentientResultsAspNetCoreExtensions.AddZentientResultsAspNetCore(services, null, null);
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<IHttpContextAccessor>().Should().NotBeNull();
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Registers_ProblemDetailsResultFilter()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            ZentientResultsAspNetCoreExtensions.AddZentientResultsAspNetCore(services, null, null);
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<ProblemDetailsResultFilter>().Should().NotBeNull();
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Registers_ZentientResultEndpointFilter()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            ZentientResultsAspNetCoreExtensions.AddZentientResultsAspNetCore(services, null, null);
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<ZentientResultEndpointFilter>().Should().NotBeNull();
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ProblemDetailsOptions_Customization()
        {
            // Arrange
            var services = new ServiceCollection();
            string? customKey = null;
            Zentient.Results.AspNetCore.ZentientResultsAspNetCoreExtensions.AddZentientResultsAspNetCore(services, (ProblemDetailsOptions options) =>
            {
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions["custom"] = "value";
                    customKey = "value";
                };
            }, null);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<ProblemDetailsOptions>>().Value;

            // Act
            var httpContext = new DefaultHttpContext();
            var pd = new ProblemDetails();
            var ctx = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = pd
            };
            options.CustomizeProblemDetails!(ctx);

            // Assert
            pd.Extensions.Should().ContainKey("custom");
            pd.Extensions["custom"].Should().Be("value");
            customKey.Should().Be("value");
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ProblemDetailsOptions_TraceId()
        {
            // Arrange
            var services = new ServiceCollection();
            ZentientResultsAspNetCoreExtensions.AddZentientResultsAspNetCore(services, null, null);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<ProblemDetailsOptions>>().Value;

            // Act
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "trace-123";
            var pd = new ProblemDetails();
            var ctx = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = pd
            };
            options.CustomizeProblemDetails!(ctx);

            // Assert
            pd.Extensions.Should().ContainKey("traceId");
            pd.Extensions["traceId"].Should().Be("trace-123");
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ApiBehaviorOptions_InvalidModelStateResponseFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            ZentientResultsAspNetCoreExtensions.AddZentientResultsAspNetCore(services, null, null);
            var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("FieldA", "ErrorA");
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = provider;
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(), modelState);

            // Act
            var result = options.InvalidModelStateResponseFactory(actionContext);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = (ObjectResult)result;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.UnprocessableEntity);
            objectResult.ContentTypes.Should().Contain("application/problem+json");
            objectResult.Value.Should().BeOfType<ValidationProblemDetails>();
            var pd = (ValidationProblemDetails)objectResult.Value!;
            pd.Extensions.Should().ContainKey("zentientErrors");
        }

        [Fact]
        public void AddZentientResultsAspNetCore_DoesNot_Override_Controllers()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddControllers();
            ZentientResultsAspNetCoreExtensions.AddZentientResultsAspNetCore(services, null, null);

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetRequiredService<ProblemDetailsFactory>().Should().NotBeNull();
        }
    }
}
