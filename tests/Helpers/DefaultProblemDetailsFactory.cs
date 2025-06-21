// Dummy classes for testing purposes, assuming they exist in Zentient.Results.AspNetCore or Zentient.Results
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Net;
using System.Threading.Tasks;

using static Zentient.Results.ResultStatuses;
using static Microsoft.AspNetCore.Http.Results;

namespace Zentient.Results.Tests.Helpers
{
    //public class ProblemDetailsResultFilter : IActionFilter
    //{
    //    public void OnActionExecuting(ActionExecutingContext context) { }
    //    public void OnActionExecuted(ActionExecutedContext context)
    //    {
    //        if (context.Result is ObjectResult objectResult && objectResult.Value is Zentient.Results.Result resultValue)
    //        {
    //            var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
    //            var zentientOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ZentientProblemDetailsOptions>>();

    //            var problemDetails = resultValue.ToProblemDetails(problemDetailsFactory, context.HttpContext);

    //            context.Result = new ObjectResult(problemDetails)
    //            {
    //                StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError, 
    //                ContentTypes = { "application/problem+json" }
    //            };
    //        }
    //    }
    //}

    public class DefaultProblemDetailsFactory : ProblemDetailsFactory
    {
        public override Microsoft.AspNetCore.Mvc.ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null)
        {
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode);
            return problemDetails;
        }

        public override Microsoft.AspNetCore.Mvc.ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null)
        {
            var problemDetails = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(modelStateDictionary)
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode);
            return problemDetails;
        }

        private void ApplyProblemDetailsDefaults(HttpContext httpContext, Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails, int? statusCode)
        {
            // Default behavior, can be customized via options.CustomizeProblemDetails
            problemDetails.Status ??= statusCode;
            problemDetails.Title ??= GetDefaultTitle(problemDetails.Status.GetValueOrDefault());
        }

        private string? GetDefaultTitle(int v)
        {
            return v switch
            {
                400 => ResultStatuses.BadRequest.ToString(),
                401 => ResultStatuses.Unauthorized.ToString(),
                403 => ResultStatuses.Forbidden.ToString(),
                404 => ResultStatuses.NotFound.ToString(),
                500 => ResultStatuses.Error.ToString(),
                _ => "Error"
            };
        }
    }
}
