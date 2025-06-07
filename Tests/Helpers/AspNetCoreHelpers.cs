using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

using System.Text.Json;

using Zentient.Results.AspNetCore;
using Zentient.Results.AspNetCore.Configuration;
using Zentient.Results.AspNetCore.Filters;

namespace Zentient.Results.Tests.Helpers
{
    /// <summary>
    /// Provides helper methods for setting up ASP.NET Core-related mocks and test objects.
    /// </summary>
    internal static class AspNetCoreHelpers
    {
        // Existing helpers (copy-pasted for completeness, assuming they are in the same file)
        private const string ProblemTypeUri = ZentientProblemDetailsOptions.FallbackProblemDetailsBaseUri;

        /// <summary>
        /// Creates a mocked <see cref="ProblemDetailsFactory"/> for use in unit tests.
        /// </summary>
        /// <returns>A mock implementation of <see cref="ProblemDetailsFactory"/>.</returns>
        /// <summary>
        /// Creates a mocked <see cref="ProblemDetailsFactory"/> for use in unit tests.
        /// </summary>
        /// <returns>A mock implementation of <see cref="ProblemDetailsFactory"/>.</returns>
        public static ProblemDetailsFactory CreateFactory()
        {
            var mockFactory = new Mock<ProblemDetailsFactory>();

            // Setup for CreateProblemDetails
            mockFactory.Setup(f => f.CreateProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()
            ))
            .Returns((HttpContext context, int? statusCode, string title, string type, string detail, string instance) => new ProblemDetails
            {
                Status = statusCode, // This should correctly assign the passed statusCode
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance ?? context.Request.Path.Value,
                Extensions = new Dictionary<string, object?>()
            });

            // Setup for CreateValidationProblemDetails
            mockFactory.Setup(f => f.CreateValidationProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<ModelStateDictionary>(),
                It.IsAny<int?>(), // This parameter will be 400
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>() // Instance is nullable
            ))
            .Returns((HttpContext context, ModelStateDictionary modelState, int? statusCode, string title, string type, string detail, string instance) => new ValidationProblemDetails(modelState)
            {
                Status = statusCode, // This should correctly assign the passed statusCode (400)
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance ?? context.Request.Path.Value,
                Extensions = new Dictionary<string, object?>()
            });

            return mockFactory.Object;
        }

        /// <summary>
        /// Creates a default <see cref="HttpContext"/> with a preset request path.
        /// </summary>
        /// <returns>A new <see cref="DefaultHttpContext"/> instance.</returns>
        public static HttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/test";
            context.TraceIdentifier = Guid.NewGuid().ToString();
            return context;
        }

        /// <summary>
        /// Creates a mocked <see cref="EndpointFilterInvocationContext"/> with the specified <see cref="HttpContext"/> mock and optional request path.
        /// </summary>
        /// <param name="httpContextMock">The mock <see cref="HttpContext"/> to use.</param>
        /// <param name="requestPath">Optional request path to set on the context.</param>
        /// <returns>A mock of <see cref="EndpointFilterInvocationContext"/>.</returns>
        public static Mock<EndpointFilterInvocationContext> CreateContext(Mock<HttpContext> httpContextMock, string? requestPath = null)
        {
            var ctx = new Mock<EndpointFilterInvocationContext>();
            ctx.SetupGet(c => c.HttpContext).Returns(httpContextMock.Object);

            if (requestPath != null)
            {
                var httpRequestMock = new Mock<HttpRequest>();
                httpRequestMock.Setup(x => x.Path).Returns(new PathString(requestPath));
                httpContextMock.SetupGet(x => x.Request).Returns(httpRequestMock.Object);
            }

            ctx.SetupGet(c => c.Arguments).Returns(Array.Empty<object>());

            return ctx;
        }

        /// <summary>
        /// Creates a mocked <see cref="HttpContext"/> with the specified <see cref="IServiceProvider"/> and optional request path.
        /// </summary>
        /// <param name="sp">The service provider to assign to the context.</param>
        /// <param name="requestPath">Optional request path to set on the context.</param>
        /// <returns>A mock of <see cref="HttpContext"/>.</returns>
        public static Mock<HttpContext> CreateHttpContext(IServiceProvider sp, string? requestPath = null)
        {
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(h => h.RequestServices).Returns(sp);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(x => x.Scheme).Returns("http");
            httpRequestMock.Setup(x => x.Host).Returns(new HostString("localhost"));
            httpRequestMock.Setup(x => x.PathBase).Returns(PathString.Empty);
            httpRequestMock.Setup(x => x.Path).Returns(new PathString(requestPath ?? string.Empty));
            httpRequestMock.Setup(x => x.QueryString).Returns(QueryString.Empty);
            httpContext.SetupGet(x => x.Request).Returns(httpRequestMock.Object);
            httpContext.SetupGet(x => x.TraceIdentifier).Returns(Guid.NewGuid().ToString());

            return httpContext;
        }

        /// <summary>
        /// Creates a service provider and registers the provided <see cref="ProblemDetailsFactory"/> mock as a singleton.
        /// </summary>
        /// <param name="problemDetailsFactoryMock">The mock <see cref="ProblemDetailsFactory"/> to register.</param>
        /// <returns>An <see cref="IServiceProvider"/> with the mock registered.</returns>
        public static IServiceProvider CreateServiceProvider(Mock<ProblemDetailsFactory> problemDetailsFactoryMock)
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging();
            services.AddSingleton(problemDetailsFactoryMock.Object);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddOptions<ZentientProblemDetailsOptions>()
                .Configure(options => options.ProblemTypeBaseUri = ZentientProblemDetailsOptions.FallbackProblemDetailsBaseUri);
            services.AddOptions<ProblemDetailsOptions>();

            return services.BuildServiceProvider();
        }

        public static ProblemDetailsResultFilter CreateFilter()
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
                    null
                ))
                .Returns((HttpContext httpContext, ModelStateDictionary modelState, int? statusCode, string title, string type, string detail) =>
                {
                    var vpd = new ValidationProblemDetails(modelState)
                    {
                        Status = statusCode,
                        Title = title,
                        Type = type,
                        Detail = detail
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

        /// <summary>
        /// Creates a <see cref="ProblemDetailsContext"/> instance for testing ProblemDetailsOptions customization.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="problemDetails">The ProblemDetails instance to customize.</param>
        /// <returns>A new <see cref="ProblemDetailsContext"/>.</returns>
        public static ProblemDetailsContext CreateProblemDetailsContext(HttpContext httpContext, ProblemDetails problemDetails)
        {
            return new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            };
        }

        /// <summary>
        /// Creates an <see cref="ApiBehaviorOptions"/> instance with necessary dependencies resolved from a service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies from.</param>
        /// <returns>A new <see cref="ApiBehaviorOptions"/> instance.</returns>
        public static ApiBehaviorOptions CreateApiBehaviorOptions(IServiceProvider serviceProvider)
        {
            // ApiBehaviorOptions constructor might need to be setup with mocks or actual instances
            // depending on what it consumes internally. A common pattern is to use PostConfigure.
            // For testing purposes, we can create a simple one or mock it if its internal logic is complex.
            // For now, let's create a basic one. The important part is setting InvalidModelStateResponseFactory.

            var options = new ApiBehaviorOptions();
            // This is typically set by the framework, and we want to test that YOUR extension sets it.
            // So we'll let your extension's PostConfigure handle it.
            // If you need a fully configured ApiBehaviorOptions, you might need to build a ServiceCollection
            // and configure MVC services on it, then resolve it.
            return options;
        }

        /// <summary>
        /// Creates an <see cref="ActionContext"/> for testing MVC filters or factories.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the action.</param>
        /// <param name="modelState">Optional model state dictionary.</param>
        /// <returns>A new <see cref="ActionContext"/>.</returns>
        public static ActionContext CreateActionContext(HttpContext httpContext, ModelStateDictionary? modelState = null)
        {
            return new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor(),
                modelState ?? new ModelStateDictionary()
            );
        }

        /// <summary>
        /// Simulates the invocation of an <see cref="EndpointFilter"/> for testing.
        /// </summary>
        /// <param name="filter">The endpoint filter to invoke.</param>
        /// <param name="context">The invocation context.</param>
        /// <param name="nextResult">The result that the 'next' delegate would return.</param>
        /// <returns>The result returned by the filter.</returns>
        public static async ValueTask<object?> SimulateEndpointFilterInvocation(
            IEndpointFilter filter,
            EndpointFilterInvocationContext context,
            object? nextResult)
        {
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult(nextResult);

            return await filter.InvokeAsync(context, next);
        }

        /// <summary>
        /// Creates an enhanced <see cref="WebApplicationFactory{TEntryPoint}"/> for integration tests,
        /// allowing configuration of services and a custom test server.
        /// </summary>
        /// <typeparam name="TEntryPoint">The startup class of the application under test.</typeparam>
        /// <param name="configureServices">An action to configure services for the test application.</param>
        /// <returns>A new <see cref="WebApplicationFactory{TEntryPoint}"/> instance.</returns>
        public static WebApplicationFactory<TEntryPoint> CreateWebApplicationFactory<TEntryPoint>(
            Action<IServiceCollection>? configureServices = null)
            where TEntryPoint : class
        {
            return new WebApplicationFactory<TEntryPoint>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        configureServices?.Invoke(services);
                    });
                });
        }

        /// <summary>
        /// Deserializes an <see cref="HttpResponseMessage"/> content into a <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/>.
        /// </summary>
        /// <param name="response">The HTTP response message.</param>
        /// <returns>The deserialized ProblemDetails object.</returns>
        /// <exception cref="JsonException">Thrown if deserialization fails.</exception>
        public static async Task<ProblemDetails> GetProblemDetailsFromResponse(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(content, options);

                if (validationProblemDetails != null && validationProblemDetails.Errors != null && validationProblemDetails.Errors.Count > 0)
                {
                    return validationProblemDetails;
                }
            }
            catch (JsonException)
            {
                // Not a ValidationProblemDetails, try as ProblemDetails
            }

            return JsonSerializer.Deserialize<ProblemDetails>(content, options)
                ?? throw new JsonException("Could not deserialize response content to ProblemDetails.");
        }

        /// <summary>
        /// Creates an <see cref="IOptions{ProblemDetailsOptions}"/> instance for testing.
        /// </summary>
        /// <param name="configure">Optional action to configure the <see cref="ProblemDetailsOptions"/>.</param>
        /// <returns>An <see cref="IOptions{ProblemDetailsOptions}"/> instance.</returns>
        public static IOptions<ProblemDetailsOptions> CreateMockProblemDetailsOptions(Action<ProblemDetailsOptions>? configure = null)
        {
            var options = new ProblemDetailsOptions();
            configure?.Invoke(options);
            return Options.Create(options);
        }

        /// <summary>
        /// Creates an <see cref="IOptions{ZentientProblemDetailsOptions}"/> instance for testing.
        /// </summary>
        /// <param name="configure">Optional action to configure the <see cref="ZentientProblemDetailsOptions"/>.</param>
        /// <returns>An <see cref="IOptions{ZentientProblemDetailsOptions}"/> instance.</returns>
        public static IOptions<ZentientProblemDetailsOptions> CreateMockZentientProblemDetailsOptions(Action<ZentientProblemDetailsOptions>? configure = null)
        {
            var options = new ZentientProblemDetailsOptions { ProblemTypeBaseUri = ProblemTypeUri };
            configure?.Invoke(options);
            return Options.Create(options);
        }
    }
}
