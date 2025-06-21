using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

using Zentient.Results;
using Zentient.Results.AspNetCore.Configuration;

namespace Zentient.Results.AspNetCore.Filters
{
    /// <summary>
    /// An ASP.NET Core result filter that converts <see cref="Zentient.Results.IResult"/> and <see cref="Zentient.Results.IResult{T}"/>
    /// returned from controller actions into appropriate <see cref="IActionResult"/> types.
    /// On failure, it automatically generates <see cref="ProblemDetails"/> or <see cref="ValidationProblemDetails"/> responses,
    /// adhering to RFC 7807.
    /// </summary>
    /// <remarks>
    /// This filter should be registered globally in <c>MvcOptions.Filters</c> using <c>AddService&lt;ProblemDetailsResultFilter&gt;()</c>
    /// to ensure its dependencies are correctly resolved from the Dependency Injection container.
    /// </remarks>
    public class ProblemDetailsResultFilter : IAsyncResultFilter
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory;
        private readonly ProblemDetailsOptions _problemDetailsOptions;
        private readonly string _problemTypeBaseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsResultFilter"/> class.
        /// </summary>
        /// <param name="problemDetailsFactory">The factory used to create <see cref="ProblemDetails"/> instances.
        /// This dependency is resolved from the ASP.NET Core Dependency Injection container.</param>
        /// <param name="options">Configuration options for problem details, not used in this filter but can be injected for future extensibility.</param>
        /// <param name="zentientProblemDetailsOptions">Configuration options for Zentient problem details, not used in this filter but can be injected for future extensibility.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="problemDetailsFactory"/> is null.</exception>
        public ProblemDetailsResultFilter(
            ProblemDetailsFactory problemDetailsFactory,
            IOptions<ProblemDetailsOptions> options,
            IOptions<ZentientProblemDetailsOptions> zentientProblemDetailsOptions)
        {
            _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
            _problemDetailsOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _problemTypeBaseUri = zentientProblemDetailsOptions.Value.ProblemTypeBaseUri
                ?? ZentientProblemDetailsOptions.FallbackProblemDetailsBaseUri;
        }

        /// <summary>
        /// Executes the result filter asynchronously. This method intercepts the action's result
        /// and converts <see cref="Zentient.Results.IResult"/> values (both synchronous and asynchronous)
        /// into appropriate <see cref="IActionResult"/> types.
        /// </summary>
        /// <param name="context">The <see cref="ResultExecutingContext"/> for the current request,
        /// providing access to the action's result and HTTP context.</param>
        /// <param name="next">The delegate to execute the next filter or the action's result itself.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult)
            {
                IResult? zentientResult = null;
                if (objectResult.Value is IResult directResult)
                {
                    zentientResult = directResult;
                }
                else if (objectResult.Value is Task<object> valueTask)
                {
                    var awaitedValue = await valueTask;

                    if (awaitedValue is IResult awaitedResult)
                    {
                        zentientResult = awaitedResult;
                    }
                }
                else if (objectResult.Value is Task<IResult> resultTask)
                {
                    zentientResult = await resultTask;
                }

                if (zentientResult != null)
                {
                    context.Result = ConvertZentientResultToActionResult(zentientResult, context.HttpContext);
                }
            }

            await next();
        }

        /// <summary>
        /// Converts a <see cref="Zentient.Results.IResult"/> instance into an appropriate <see cref="IActionResult"/>.
        /// </summary>
        /// <param name="zentientResult">The Zentient result to convert.</param>
        /// <param name="httpContext">The current HTTP context, necessary for <see cref="ProblemDetailsFactory"/>.</param>
        /// <returns>An <see cref="IActionResult"/> representing the converted result.</returns>
        private IActionResult ConvertZentientResultToActionResult(Zentient.Results.IResult zentientResult, HttpContext httpContext)
        {
            if (zentientResult.IsSuccess)
            {
                object? value = null;
                bool hasValue = false;
                var zentientResultType = zentientResult.GetType();
                var iResultGenericInterface = zentientResultType
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Zentient.Results.IResult<>));

                if (iResultGenericInterface != null)
                {
                    var valueProp = zentientResultType.GetProperty("Value");

                    if (valueProp != null && valueProp.CanRead)
                    {
                        value = valueProp.GetValue(zentientResult);
                        hasValue = true;
                    }
                }

                return zentientResult.Status.Code switch
                {
                    (int)HttpStatusCode.OK => hasValue ? new OkObjectResult(value) : new NoContentResult(),
                    (int)HttpStatusCode.Created => hasValue ? new CreatedResult(string.Empty, value) : new StatusCodeResult((int)HttpStatusCode.Created),
                    (int)HttpStatusCode.Accepted => hasValue ? new AcceptedResult(string.Empty, value) : new StatusCodeResult((int)HttpStatusCode.Accepted),
                    (int)HttpStatusCode.NoContent => new NoContentResult(),
                    _ => hasValue
                        ? new ObjectResult(value) { StatusCode = (int)zentientResult.Status.ToHttpStatusCode() }
                        : new StatusCodeResult((int)zentientResult.Status.ToHttpStatusCode())
                };
            }

            var problemDetails = zentientResult.ToProblemDetails(_problemDetailsFactory, httpContext, new ZentientProblemDetailsOptions { ProblemTypeBaseUri = _problemTypeBaseUri });

            if (problemDetails is ValidationProblemDetails validationProblemDetails)
            {
                return new UnprocessableEntityObjectResult(validationProblemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            }
            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError,
                ContentTypes = { "application/problem+json" }
            };
        }
    }
}
