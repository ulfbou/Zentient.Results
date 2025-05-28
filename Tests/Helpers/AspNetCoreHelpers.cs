using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

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
        private const string ProblemTypeUri = ProblemDetailsExtensions.DefaultProblemTypeBaseUri; // Use the default base URI

        /// <summary>
        /// Creates a mocked <see cref="ProblemDetailsFactory"/> for use in unit tests.
        /// </summary>
        /// <returns>A mock implementation of <see cref="ProblemDetailsFactory"/>.</returns>
        public static ProblemDetailsFactory CreateFactory()
        {
            var mockFactory = new Mock<ProblemDetailsFactory>();

            mockFactory.Setup(f => f.CreateProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()
            )).Returns((HttpContext context, int? statusCode, string title, string type, string detail, string instance) => new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance ?? context.Request.Path.Value,
                Extensions = new Dictionary<string, object?>()
            });

            mockFactory.Setup(f => f.CreateValidationProblemDetails(
                It.IsAny<HttpContext>(),
                It.IsAny<ModelStateDictionary>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns((HttpContext context, ModelStateDictionary modelState, int? statusCode, string title, string type, string detail, string instance) => new ValidationProblemDetails(modelState)
            {
                Status = statusCode,
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
            services.AddMock(problemDetailsFactoryMock);
            services.AddSingleton(problemDetailsFactoryMock.Object);

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Creates a new instance of <see cref="ProblemDetailsResultFilter"/> with the specified <see cref="ProblemDetailsFactory"/>.
        /// If no factory is provided, a default instance is created using the default options.
        /// </summary>
        /// <param name="factory">Optional custom <see cref="ProblemDetailsFactory"/> to use.</param>
        /// <returns>A new instance of <see cref="ProblemDetailsResultFilter"/>.</returns>
        public static ProblemDetailsResultFilter CreateFilter(
            ProblemDetailsFactory? factory = null)
        {
            var pdFactory = factory ?? new DefaultProblemDetailsFactory(
                Options.Create(new Microsoft.AspNetCore.Mvc.ApiBehaviorOptions()),
                Options.Create(new ProblemDetailsOptions()));
            var options = Options.Create(new ProblemDetailsOptions());
            var zentientOptions = Options.Create(new ZentientProblemDetailsOptions
            {
                ProblemTypeBaseUri = ProblemTypeUri
            });
            return new ProblemDetailsResultFilter(pdFactory, options, zentientOptions);
        }
    }
}
