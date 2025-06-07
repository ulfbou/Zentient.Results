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
using Microsoft.AspNetCore.Mvc.Abstractions;
using Zentient.Results.AspNetCore.Configuration;
using Microsoft.AspNetCore.Routing;

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

        // Inside ZentientResultsAspNetCoreExtensionsTests class
        [Fact]
        public Task AddZentientResultsAspNetCore_Configures_ApiBehaviorOptions_InvalidModelStateResponseFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            // Add minimal MVC services to enable ApiBehaviorOptions
            services.AddControllers().AddZentientResultsForMvc(); // Or just AddControllers() for the base test
            services.AddLogging(); // Often needed for HttpContext setup
            services.AddOptions(); // Ensure options are registered

            // THIS IS THE LINE TO TEST:
            services.AddZentientResultsAspNetCore(); // The canonical registration point

            var serviceProvider = services.BuildServiceProvider();
            var mvcOptions = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;

            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
            httpContext.Request.Path = "/test-path"; // Needed for instance URI

            // Simulate Model State Errors
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Field1", "Error message for Field1");
            modelState.AddModelError("Field2", "Error message for Field2");

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);

            // Act
            var result = mvcOptions.InvalidModelStateResponseFactory(actionContext);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult.Value);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);

            // Expect 400 Bad Request as per the canonical implementation's design intent
            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode); // FIX: Assert 400
            Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status); // FIX: Assert 400

            Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
            Assert.Equal($"{ZentientProblemDetailsOptions.FallbackProblemDetailsBaseUri}validation", problemDetails.Type);
            Assert.Equal("/test-path", problemDetails.Instance);

            // Assert zentientErrors extension
            Assert.True(problemDetails.Extensions.TryGetValue("zentientErrors", out var zentientErrors));
            var errorsList = Assert.IsAssignableFrom<List<Dictionary<string, object?>>>(zentientErrors);
            Assert.Contains(errorsList, e => e.TryGetValue("code", out var code) && code?.ToString() == "Field1");
            Assert.Contains(errorsList, e => e.TryGetValue("code", out var code) && code?.ToString() == "Field2");
            return Task.CompletedTask;
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
