using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

using System.Net;

using Zentient.Results.AspNetCore;
using Zentient.Results.AspNetCore.Configuration;
using Zentient.Results.AspNetCore.Filters;
using Zentient.Results.Tests.Helpers;

namespace Zentient.Results.Tests.AspNetCore
{
    /// <summary>
    /// Unit tests for <see cref="ZentientResultsAspNetCoreExtensions"/> focusing on
    /// service registration and configuration aspects of the <c>AddZentientResultsAspNetCore</c> method.
    /// </summary>
    public class ZentientResultsAspNetCoreExtensionsRegistrationTests
    {
        private const string CustomProblemTypeBaseUri = "https://example.com/custom-problems/";

        [Fact]
        public void AddZentientResultsAspNetCore_Registers_ZentientProblemDetailsOptions_Default()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            // Assert
            var options = provider.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>().Value;
            options.Should().NotBeNull();
            options.ProblemTypeBaseUri.Should().Be(ZentientProblemDetailsOptions.FallbackProblemDetailsBaseUri);
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ZentientProblemDetailsOptions_Custom()
        {
            // Arrange
            var services = new ServiceCollection();
            string configuredUri = null;

            // Act
            services.AddZentientResultsAspNetCore(
                configureZentientProblemDetails: options =>
                {
                    options.ProblemTypeBaseUri = CustomProblemTypeBaseUri;
                    configuredUri = options.ProblemTypeBaseUri; // Capture for direct assertion
                });
            var provider = services.BuildServiceProvider();

            // Assert
            var options = provider.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>().Value;
            options.Should().NotBeNull();
            options.ProblemTypeBaseUri.Should().Be(CustomProblemTypeBaseUri);
            configuredUri.Should().Be(CustomProblemTypeBaseUri); // Verify action was invoked
        }

        [Fact]
        public void AddZentientResultsAspNetCore_DoesNotRegister_ProblemDetailsFactory_IfAlreadyRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var existingFactoryMock = new Mock<ProblemDetailsFactory>();
            services.AddSingleton(existingFactoryMock.Object);

            // Act
            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            // Assert
            var factory = provider.GetRequiredService<ProblemDetailsFactory>();
            factory.Should().BeSameAs(existingFactoryMock.Object, "Should use the already registered factory instance.");

            var factoryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ProblemDetailsFactory));
            factoryDescriptor.Should().NotBeNull();
#if NET9_0_OR_GREATER
            factoryDescriptor.ImplementationInstance.Should().BeSameAs(existingFactoryMock.Object);
#else
                factoryDescriptor.ImplementationInstance.Should().BeSameAs(existingFactoryMock.Object);
#endif
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_MvcOptions_AddsProblemDetailsResultFilter()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddControllers();

            // Act
            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            // Assert
            var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
            mvcOptions.Should().NotBeNull();
            mvcOptions.Filters.Should().Contain(f => f.GetType() == typeof(ServiceFilterAttribute) && (f as ServiceFilterAttribute).ServiceType == typeof(ProblemDetailsResultFilter));
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ApiBehaviorOptions_InvalidModelStateResponseFactory_HandlesEmptyModelState()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(AspNetCoreHelpers.CreateFactory());
            services.AddHttpContextAccessor();
            services.AddOptions<ZentientProblemDetailsOptions>().Configure(o => o.ProblemTypeBaseUri = CustomProblemTypeBaseUri);

            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            var apiBehaviorOptions = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
            var httpContext = new DefaultHttpContext { RequestServices = provider };
            // ModelState with no errors, but still considered invalid by MVC's [ApiController] if no other handler exists
            var modelState = new ModelStateDictionary();
            var actionContext = AspNetCoreHelpers.CreateActionContext(httpContext, modelState);

            // Act
            var result = apiBehaviorOptions.InvalidModelStateResponseFactory(actionContext);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = (ObjectResult)result;
            objectResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest); // Default for empty model state by RFC
            objectResult.ContentTypes.Should().Contain("application/problem+json");
            objectResult.Value.Should().BeOfType<ValidationProblemDetails>();
            var problemDetails = (ValidationProblemDetails)objectResult.Value!;

            problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
            problemDetails.Errors.Should().BeEmpty(); // No specific model errors
            problemDetails.Extensions.Should().NotContainKey("zentientErrors"); // No Zentient errors added for empty model state
            problemDetails.Type.Should().Be($"{CustomProblemTypeBaseUri}validation"); // Still uses validation type
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ApiBehaviorOptions_InvalidModelStateResponseFactory_UsesProblemTypeBaseUri()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(AspNetCoreHelpers.CreateFactory());
            services.AddHttpContextAccessor();
            // Configure custom base URI via ZentientProblemDetailsOptions
            services.AddOptions<ZentientProblemDetailsOptions>().Configure(o => o.ProblemTypeBaseUri = CustomProblemTypeBaseUri);

            services.AddZentientResultsAspNetCore();
            var provider = services.BuildServiceProvider();

            var apiBehaviorOptions = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
            var httpContext = new DefaultHttpContext { RequestServices = provider };
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Name", "Name is required.");
            var actionContext = AspNetCoreHelpers.CreateActionContext(httpContext, modelState);

            // Act
            var result = apiBehaviorOptions.InvalidModelStateResponseFactory(actionContext);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = (ObjectResult)result;
            var problemDetails = (ValidationProblemDetails)objectResult.Value!;

            problemDetails.Type.Should().StartWith(CustomProblemTypeBaseUri);
            problemDetails.Type.Should().EndWith("validation");
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ProblemDetailsOptions_Customization_Chainable()
        {
            // Arrange
            var services = new ServiceCollection();
            bool originalCustomizeInvoked = false;
            bool newCustomizeInvoked = false;

            // 1. Capture the initial ProblemDetailsOptions.CustomizeProblemDetails delegate
            //    We use a local variable to hold the reference to the FIRST delegate set.
            Action<ProblemDetailsContext>? initialProblemDetailsCustomization = null;

            // Pre-configure ProblemDetailsOptions with an existing customization
            services.PostConfigure<ProblemDetailsOptions>(options =>
            {
                // Store the reference to the delegate that Zentient will add internally.
                // This is tricky because Zentient's customization is added AFTER this PostConfigure if it's chained.
                // A better approach for "chaining" is to provide a specific custom action
                // *to ZentientResultsAspNetCore*, which then itself chains.

                // Let's assume the scenario where you have a customization BEFORE Zentient.
                // We'll set a mock initial customization.
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions["originalKey"] = "originalValue";
                    originalCustomizeInvoked = true;
                };

                // Capture this specific delegate instance before Zentient possibly overrides/wraps it.
                // Zentient will likely wrap this, so we need to test that its wrapper correctly calls the original.
                initialProblemDetailsCustomization = options.CustomizeProblemDetails;
            });

            // Act - Add ZentientResultsAspNetCore with its own customization
            // The key here is to test that Zentient's internal logic correctly wraps
            // any *pre-existing* CustomizeProblemDetails delegate.
            // So, we let Zentient configure its own chain.
            services.AddZentientResultsAspNetCore(
                configureMvcProblemDetails: options =>
                {
                    // This is YOUR custom action that Zentient's AddZentientAspNetCore takes.
                    // Inside this action, you define additional customizations.
                    // When Zentient applies its options, it will call this.
                    // If you want to explicitly chain here, you need to be careful.
                    // Zentient's internal logic *should* handle the chaining of its traceId logic
                    // with whatever was previously there or what you provide here.

                    // The common pattern for chaining with PostConfigure is to store the "previous"
                    // action, and then call it within your new action.
                    var previousCustomize = options.CustomizeProblemDetails; // This captures whatever was set immediately before Zentient's specific configureMvcProblemDetails

                    options.CustomizeProblemDetails = ctx =>
                    {
                        previousCustomize?.Invoke(ctx); // Call the previous customization (which might be the one set by Zentient's internal traceId logic, or the original if Zentient chains correctly)
                        ctx.ProblemDetails.Extensions["newKey"] = "newValue";
                        newCustomizeInvoked = true;
                    };
                });
            var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<ProblemDetailsOptions>>().Value;
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "TEST_TRACE_ID_FROM_HTTP_CONTEXT"; // Ensure a trace ID is available
            var problemDetails = new ProblemDetails(); // Start with a fresh ProblemDetails

            var context = AspNetCoreHelpers.CreateProblemDetailsContext(httpContext, problemDetails);

            // Simulate the final customization being applied by the framework
            options.CustomizeProblemDetails!(context);

            // Assert
            problemDetails.Extensions.Should().ContainKey("originalKey").And.ContainKey("newKey");
            problemDetails.Extensions["originalKey"].Should().Be("originalValue");
            problemDetails.Extensions["newKey"].Should().Be("newValue");

            // Zentient should add traceId if it's not present, or if it's configured to overwrite.
            // Given the test, it should be present.
            problemDetails.Extensions.Should().ContainKey("traceId");
            problemDetails.Extensions["traceId"].Should().Be("TEST_TRACE_ID_FROM_HTTP_CONTEXT"); // Verify Zentient's traceId logic works

            originalCustomizeInvoked.Should().BeTrue("The original customization delegate should have been invoked.");
            newCustomizeInvoked.Should().BeTrue("The new customization delegate should have been invoked.");
        }

        [Fact]
        public void AddZentientResultsAspNetCore_Configures_ProblemDetailsOptions_TraceId_AlreadyPresent()
        {
            // Arrange
            var services = new ServiceCollection();
            string preExistingTraceId = "PRE-EXISTING-TRACE";

            // Act
            services.AddZentientResultsAspNetCore(); // Zentient adds traceId
            var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<ProblemDetailsOptions>>().Value;
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "NEW-TRACE-ID"; // Simulate new trace ID
            var problemDetails = new ProblemDetails();
            problemDetails.Extensions["traceId"] = preExistingTraceId; // Manually add existing trace ID

            var context = AspNetCoreHelpers.CreateProblemDetailsContext(httpContext, problemDetails);

            // Simulate the customization being applied
            options.CustomizeProblemDetails!(context);

            // Assert
            problemDetails.Extensions.Should().ContainKey("traceId");
            problemDetails.Extensions["traceId"].Should().Be(preExistingTraceId); // Should NOT be overwritten
            problemDetails.Extensions["traceId"].Should().NotBe("NEW-TRACE-ID");
        }
    }
}