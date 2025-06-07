using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

using System;
using System.Linq;
using System.Net;

using Xunit;

using Zentient.Results.AspNetCore;
using Zentient.Results.AspNetCore.Configuration;
using Zentient.Results.AspNetCore.Filters;
using Zentient.Results.Tests.Helpers;

namespace Zentient.Results.Tests.AspNetCore
{
    /// <summary>
    /// Unit tests for <see cref="ZentientResultsExtensions"/> focusing on
    /// the <c>AddZentientResults</c> method, verifying combined service registration and configuration.
    /// </summary>
    public class ZentientResultsExtensionsCombinedTests
    {
        private const string CustomProblemTypeBaseUri = "https://example.com/combined-problems/";
        private const string CustomMvcProblemDetailsTitle = "Custom MVC Problem";

        [Fact]
        public void AddZentientResults_RegistersAllExpectedServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddControllers(); // Required for MvcOptions and ApiBehaviorOptions
            services.AddZentientResults();
            var provider = services.BuildServiceProvider();

            // Assert
            provider.GetService<ProblemDetailsFactory>().Should().NotBeNull("ProblemDetailsFactory should be registered.");
            provider.GetService<IHttpContextAccessor>().Should().NotBeNull("IHttpContextAccessor should be registered.");
            provider.GetService<ProblemDetailsResultFilter>().Should().NotBeNull("ProblemDetailsResultFilter should be registered.");
            provider.GetService<ZentientResultEndpointFilter>().Should().NotBeNull("ZentientResultEndpointFilter should be registered.");

            // Verify registrations with correct lifetimes (optional, but good for thoroughness)
            services.Should().Contain(sd => sd.ServiceType == typeof(ProblemDetailsFactory) && sd.Lifetime == ServiceLifetime.Singleton);
            services.Should().Contain(sd => sd.ServiceType == typeof(IHttpContextAccessor) && sd.Lifetime == ServiceLifetime.Singleton);
            services.Should().Contain(sd => sd.ServiceType == typeof(ProblemDetailsResultFilter) && sd.Lifetime == ServiceLifetime.Scoped);
            services.Should().Contain(sd => sd.ServiceType == typeof(ZentientResultEndpointFilter) && sd.Lifetime == ServiceLifetime.Scoped);
        }

        [Fact]
        public void AddZentientResults_ConfiguresProblemDetailsOptions_DefaultProblemTypeBaseUri()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddZentientResults();
            var provider = services.BuildServiceProvider();

            // Assert
            var zentientOptions = provider.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>().Value;
            zentientOptions.Should().NotBeNull();
            zentientOptions.ProblemTypeBaseUri.Should().Be(ZentientProblemDetailsOptions.FallbackProblemDetailsBaseUri, "Default ProblemTypeBaseUri should be the RFC fallback URI.");
        }

        [Fact]
        public void AddZentientResults_ConfiguresProblemDetailsOptions_CustomZentientOptions()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddZentientResults(
                configureZentientProblemDetails: options =>
                {
                    options.ProblemTypeBaseUri = CustomProblemTypeBaseUri;
                });
            var provider = services.BuildServiceProvider();

            // Assert
            var zentientOptions = provider.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>().Value;
            zentientOptions.Should().NotBeNull();
            zentientOptions.ProblemTypeBaseUri.Should().Be(CustomProblemTypeBaseUri, "ZentientProblemDetailsOptions should be customized.");
        }

        [Fact]
        public void AddZentientResults_ConfiguresProblemDetailsOptions_CustomMvcProblemDetailsOptions()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddZentientResults(
                configureProblemDetails: options =>
                {
                    options.CustomizeProblemDetails = ctx => ctx.ProblemDetails.Title = CustomMvcProblemDetailsTitle;
                });
            var provider = services.BuildServiceProvider();

            // Assert
            var mvcProblemDetailsOptions = provider.GetRequiredService<IOptions<Microsoft.AspNetCore.Http.ProblemDetailsOptions>>().Value;
            mvcProblemDetailsOptions.Should().NotBeNull();

            var problemDetails = new ProblemDetails();
            var httpContext = AspNetCoreHelpers.CreateHttpContext(); // Use helper to create HttpContext
            var context = AspNetCoreHelpers.CreateProblemDetailsContext(httpContext, problemDetails);

            // Simulate the customization application
            mvcProblemDetailsOptions.CustomizeProblemDetails?.Invoke(context);
            problemDetails.Title.Should().Be(CustomMvcProblemDetailsTitle, "Mvc ProblemDetailsOptions should be customized.");
        }

        [Fact]
        public void AddZentientResults_AppliesTraceIdCustomization()
        {
            // Arrange
            var services = new ServiceCollection();
            string expectedTraceId = Guid.NewGuid().ToString(); // Simulate a trace ID

            // Act
            services.AddZentientResults();
            var provider = services.BuildServiceProvider();

            var mvcProblemDetailsOptions = provider.GetRequiredService<IOptions<Microsoft.AspNetCore.Http.ProblemDetailsOptions>>().Value;
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = expectedTraceId; // Set the trace ID on HttpContext
            var problemDetails = new ProblemDetails();
            var context = AspNetCoreHelpers.CreateProblemDetailsContext(httpContext, problemDetails);

            // Simulate the customization application (Zentient's PostConfigure will apply it)
            mvcProblemDetailsOptions.CustomizeProblemDetails?.Invoke(context);

            // Assert
            problemDetails.Extensions.Should().ContainKey("traceId");
            problemDetails.Extensions["traceId"].Should().Be(expectedTraceId, "traceId from HttpContext should be added to ProblemDetails extensions.");
        }

        [Fact]
        public void AddZentientResults_AddsProblemDetailsResultFilterToMvcOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddControllers(); // Necessary for MvcOptions to be available

            // Act
            services.AddZentientResults();
            var provider = services.BuildServiceProvider();

            // Assert
            var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
            mvcOptions.Should().NotBeNull();
            mvcOptions.Filters.Should().Contain(f => f.GetType() == typeof(ServiceFilterAttribute) && (f as ServiceFilterAttribute).ServiceType == typeof(ProblemDetailsResultFilter), "ProblemDetailsResultFilter should be added as a ServiceFilter to MvcOptions.");
        }

        [Fact]
        public void AddZentientResults_ConfiguresApiBehaviorOptions_InvalidModelStateResponseFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddControllers(); // Required for ApiBehaviorOptions
            services.AddSingleton(AspNetCoreHelpers.CreateFactory()); // Ensure ProblemDetailsFactory is available

            // Act
            services.AddZentientResults(
                configureZentientProblemDetails: options => options.ProblemTypeBaseUri = CustomProblemTypeBaseUri
            );
            var provider = services.BuildServiceProvider();

            var apiBehaviorOptions = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
            var httpContext = new DefaultHttpContext { RequestServices = provider };
            var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
            modelState.AddModelError("Field1", "Error message for Field1"); // Add a specific error
            var actionContext = AspNetCoreHelpers.CreateActionContext(httpContext, modelState);

            // Act
            var result = apiBehaviorOptions.InvalidModelStateResponseFactory(actionContext);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = (ObjectResult)result;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.ContentTypes.Should().Contain("application/problem+json");
            objectResult.Value.Should().BeOfType<ValidationProblemDetails>();
            var problemDetails = (ValidationProblemDetails)objectResult.Value!;

            problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
            problemDetails.Type.Should().StartWith(CustomProblemTypeBaseUri);
            problemDetails.Type.Should().EndWith("validation");

            // Assert that the standard ValidationProblemDetails.Errors contains the error
            problemDetails.Errors.Should().ContainKey("Field1");
            problemDetails.Errors["Field1"].Should().Contain("Error message for Field1");

            // Assert the custom "zentientErrors" extension
            problemDetails.Extensions.Should().ContainKey("zentientErrors");
            var zentientErrors = problemDetails.Extensions["zentientErrors"].As<IEnumerable<ErrorInfo>>();

            // FIX: Use 'Data' property instead of 'Key'
            zentientErrors.Should().ContainSingle(e => (string?)e.Data == "Field1" && e.Message == "Error message for Field1");
        }
    }
}
