using System.Net;
using System.Net.Http;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Moq;

using Xunit;

using Zentient.Results;
using Zentient.Results.AspNetCore;
using Zentient.Results.Tests.Helpers;

using static Zentient.Results.Tests.Helpers.AspNetCoreHelpers;
using Zentient.Results.Tests.AspNetCore.Filters;

namespace Zentient.Results.Tests.AspNetCore
{
    /// <summary>
    /// Unit tests for <see cref="ProblemDetailsExtensions"/> to ensure correct conversion of
    /// Zentient Results to <see cref="ProblemDetails"/> and <see cref="ValidationProblemDetails"/>.
    /// These tests cover various scenarios including successful results, validation errors,
    /// database errors, and undefined error categories.
    /// They also verify that the correct HTTP status codes and problem types are generated,
    /// and that the extensions are populated correctly.
    /// </summary>
    public class ProblemDetailsExtensionsTests
    {
        private const string ProblemTypeUri = ProblemDetailsExtensions.DefaultProblemTypeBaseUri;

        [Fact]
        public void ToProblemDetails_ThrowsInvalidOperationException_For_SuccessResult()
        {
            // Arrange
            var result = new SuccessResultStub(new ResultStatusStub(200, "OK"));
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            Action act = () => result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Cannot convert a successful result to ProblemDetails. ProblemDetails are for failure results only.");
        }

        [Fact]
        public void ToProblemDetails_Creates_ValidationProblemDetails_For_ValidationErrorWithCode()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Validation, "VAL-001", "Field 'Name' is required.", "Name");
            var result = new FailureResultStub(
                new[] { error },
                "Validation failed for one or more fields.",
                new ResultStatusStub(400, "Bad Request")
            );
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Should().BeOfType<ValidationProblemDetails>();
            pd.Status.Should().Be(400);
            pd.Title.Should().Be("Bad Request");
            pd.Detail.Should().Be("Validation failed for one or more fields.");
            pd.Instance.Should().Be("/test");
            pd.Type.Should().Be(ProblemTypeUri + "validation");

            var validationPd = (ValidationProblemDetails)pd;
            validationPd.Errors.Should().ContainKey("Name");
            validationPd.Errors["Name"].Should().ContainSingle().Which.Should().Be("Field 'Name' is required.");

            pd.Extensions.Should().ContainKey("zentientErrors");
            var zentientErrors = pd.Extensions["zentientErrors"] as IEnumerable<object>;
            zentientErrors.Should().NotBeNull().And.ContainSingle();
            var errorObject = zentientErrors.First().Should().BeAssignableTo<Dictionary<string, object?>>().Subject;
            errorObject["category"].Should().Be("validation");
            errorObject["code"].Should().Be("VAL-001");
            errorObject["message"].Should().Be("Field 'Name' is required.");
            errorObject["data"].Should().Be("Name");
        }

        [Fact]
        public void ToProblemDetails_Creates_ValidationProblemDetails_For_ValidationErrorWithoutCode_UsesCategory()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Validation, "", "Invalid input data.");
            var result = new FailureResultStub(
                new[] { error },
                "Input processing failed.",
                new ResultStatusStub(400, "Bad Request")
            );
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Should().BeOfType<ValidationProblemDetails>();
            pd.Status.Should().Be(400);
            pd.Type.Should().Be($"{ProblemTypeUri}validation"); // Fallback to category

            var validationPd = (ValidationProblemDetails)pd;
            validationPd.Errors.Should().ContainKey("General"); // Fallback key
            validationPd.Errors["General"].Should().ContainSingle().Which.Should().Be("Invalid input data.");

            pd.Extensions.Should().ContainKey("zentientErrors");
        }

        [Fact]
        public void ToProblemDetails_Creates_ProblemDetails_For_DatabaseErrorWithCode()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Database, "DB-001", "Cannot connect to database.");
            var result = new FailureResultStub(
                new[] { error },
                "A database error occurred.",
                new ResultStatusStub(500, "Internal Server Error")
            );
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Should().BeOfType<ProblemDetails>(); // Not ValidationProblemDetails
            pd.Status.Should().Be(500);
            pd.Title.Should().Be("Internal Server Error");
            pd.Detail.Should().Be("A database error occurred.");
            pd.Instance.Should().Be("/test");
            pd.Type.Should().Be($"{ProblemTypeUri}db-001"); // Specific code in type

            pd.Extensions.Should().ContainKey("zentientErrors");
            var zentientErrors = pd.Extensions["zentientErrors"] as IEnumerable<object>;
            zentientErrors.Should().NotBeNull().And.ContainSingle();
            var errorObject = zentientErrors.First().Should().BeAssignableTo<Dictionary<string, object?>>().Subject;
            errorObject["category"].Should().Be("database");
            errorObject["code"].Should().Be("DB-001");
        }

        [Fact]
        public void ToProblemDetails_Creates_ProblemDetails_For_UndefinedErrorCategory_FallsBackToStatusCode() // Renamed for clarity
        {
            // Arrange
            var error = new ErrorInfo((ErrorCategory)999, "", "An unknown error occurred.");
            var result = new FailureResultStub(
                new[] { error },
                "General server fault.",
                new ResultStatusStub(500, "Internal Server Error")
            );
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Should().BeOfType<ProblemDetails>();
            pd.Status.Should().Be(500);
            pd.Type.Should().Be($"{ProblemTypeUri}500"); // <--- FIX THIS ASSERTION
            pd.Extensions.Should().ContainKey("zentientErrors");
        }

        [Fact]
        public void ToProblemDetails_Creates_ProblemDetails_For_MultipleValidationErrors()
        {
            // Arrange
            var error1 = new ErrorInfo(ErrorCategory.Validation, "VAL-002", "Field 'Email' is invalid.", "Email");
            var error2 = new ErrorInfo(ErrorCategory.Validation, "VAL-003", "Field 'Age' must be positive.", "Age");
            var result = new FailureResultStub(
                new[] { error1, error2 },
                "Multiple validation issues detected.",
                new ResultStatusStub(400, "Bad Request")
            );
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Should().BeOfType<ValidationProblemDetails>();
            var validationPd = (ValidationProblemDetails)pd;
            validationPd.Errors.Should().HaveCount(2);
            validationPd.Errors.Should().ContainKey("Email").And.ContainKey("Age");

            pd.Extensions.Should().ContainKey("zentientErrors");
            var zentientErrors = pd.Extensions["zentientErrors"] as IEnumerable<object>;
            zentientErrors.Should().NotBeNull().And.HaveCount(2);
        }

        [Fact]
        public void ToProblemDetails_UsesDefaultProblemTypeBaseUri_IfProvidedBaseUriIsNullOrEmpty()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Request, "REQ-001", "Invalid request format.");
            var result = new FailureResultStub(
                new[] { error },
                "Request error.",
                new ResultStatusStub(400, "Bad Request")
            );
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd1 = result.ToProblemDetails(factory, context, "");
            var pd2 = result.ToProblemDetails(factory, context, null!);

            // Assert
            pd1.Type.Should().StartWith(ProblemDetailsExtensions.DefaultProblemTypeBaseUri);
            pd2.Type.Should().StartWith(ProblemDetailsExtensions.DefaultProblemTypeBaseUri);
            pd1.Type.Should().EndWith("req-001");
            pd2.Type.Should().EndWith("req-001");
        }

        [Fact]
        public void ToProblemDetails_EnsuresTrailingSlashInBaseUri()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Database, "DB-002", "Connection lost.");
            var result = new FailureResultStub(
                new[] { error },
                "Database connection error.",
                new ResultStatusStub(500, "Internal Server Error")
            );
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, "https://example.com/problems");

            // Assert
            pd.Type.Should().Be("https://example.com/problems/db-002");
        }

        [Fact]
        public void ToProblemDetails_FallsBackToStandardHttpTitleAndDetail_IfResultPropertiesAreEmpty()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Security, "SEC-001", "Unauthorized access.");
            var result = new FailureResultStub(
                new[] { error },
                "", // Empty error detail
                new ResultStatusStub(403, "")
            );
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Status.Should().Be(403);
            pd.Title.Should().Be("HTTP 403 Error");
            pd.Detail.Should().Be("An error occurred with status code 403.");
        }

        [Fact]
        public void ToProblemDetails_CreatesProblemDetails_For_NotFoundError()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.NotFound, "RES-404", "Resource not found.");
            var result = new FailureResultStub(
                new[] { error },
                "The requested resource could not be found.",
                new ResultStatusStub(404, "Not Found")
            );
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Should().BeOfType<ProblemDetails>();
            pd.Status.Should().Be(404);
            pd.Title.Should().Be("Not Found");
            pd.Type.Should().Be($"{ProblemTypeUri}res-404");
            pd.Detail.Should().Be("The requested resource could not be found.");
            pd.Extensions.Should().ContainKey("zentientErrors");
        }

        [Fact]
        public void ToProblemDetails_Throws_On_Success_Result()
        {
            // Arrange
            var result = new SuccessResultStub(new ResultStatusStub(200, "OK"));
            var factory = CreateFactory();
            var context = CreateHttpContext();
            var problemTypeUri = "https://example.com/problems";

            // Act
            Action act = () => result.ToProblemDetails(factory, context, problemTypeUri);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ToProblemDetails_Creates_ProblemDetails_For_Failure()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Database, "DB-001", "Database error");
            var result = new FailureResultStub(new[] { error }, "Database error", new ResultStatusStub(500, "Internal Error"));
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Should().NotBeNull();
            pd.Status.Should().Be(500);
            pd.Title.Should().Be("Internal Error");
            pd.Type.Should().ContainEquivalentOf("DB-001");
            pd.Detail.Should().Be("Database error");
            pd.Instance.Should().Be("/test");
            pd.Extensions.Should().ContainKey("zentientErrors");
            var zentientErrors = pd.Extensions["zentientErrors"] as IEnumerable<object>;
            zentientErrors.Should().NotBeNull();
            zentientErrors.Should().ContainSingle();
        }

        [Fact]
        public void ToProblemDetails_Creates_ValidationProblemDetails_For_ValidationError()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Validation, "VAL-001", "Validation failed", "FieldA");
            var result = new FailureResultStub(new[] { error }, "Validation failed", new ResultStatusStub(400, "Bad Request"));
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Should().BeOfType<ValidationProblemDetails>();
            pd.Status.Should().Be(400);
            pd.Title.Should().Be("Bad Request");
            pd.Type.Should().Be(ProblemTypeUri + "validation");
            pd.Detail.Should().Be("Validation failed");
            pd.Instance.Should().Be("/test");
            pd.Extensions.Should().ContainKey("zentientErrors");
            var zentientErrors = pd.Extensions["zentientErrors"] as IEnumerable<object>;
            zentientErrors.Should().NotBeNull();
            zentientErrors.Should().ContainSingle();
        }

        [Fact]
        public void ToProblemDetails_Recursively_Handles_InnerErrors_And_Data()
        {
            // Arrange
            var leaf = new ErrorInfo(ErrorCategory.Request, "REQ-001", "Bad request", new { Field = "Leaf" });
            var mid = new ErrorInfo(ErrorCategory.Validation, "VAL-002", "Validation failed", new { Field = "Mid" }, new[] { leaf });
            var root = new ErrorInfo(ErrorCategory.Exception, "EX-001", "Exception occurred", new { Field = "Root" }, new[] { mid });
            var result = new FailureResultStub(new[] { root }, "Exception occurred", new ResultStatusStub(500, "Internal Error"));
            var factory = CreateFactory();
            var context = CreateHttpContext();

            // Act
            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Extensions.Should().ContainKey("zentientErrors");
            // Fix: Use IEnumerable<object> instead of List<object> for compatibility with the actual type
            var zentientErrors = pd.Extensions["zentientErrors"] as IEnumerable<object>;
            zentientErrors.Should().NotBeNull();
            zentientErrors.Should().HaveCount(1);

            var rootDict = zentientErrors!.First() as IDictionary<string, object?>;
            rootDict.Should().ContainKey("data");
            rootDict.Should().ContainKey("innerErrors");

            var midList = rootDict["innerErrors"] as IEnumerable<object>;
            midList.Should().NotBeNull();
            midList.Should().HaveCount(1);

            var midDict = midList!.First() as IDictionary<string, object?>;
            midDict.Should().ContainKey("data");
            midDict.Should().ContainKey("innerErrors");

            var leafList = midDict["innerErrors"] as IEnumerable<object>;
            leafList.Should().NotBeNull();
            leafList.Should().HaveCount(1);

            var leafDict = leafList!.First() as IDictionary<string, object?>;
            leafDict.Should().ContainKey("data");
            leafDict.Should().NotContainKey("innerErrors");
        }

        [Fact]
        public void ToProblemDetailsT_Delegates_To_NonGeneric()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Validation, "VAL-001", "Validation failed");
            IResult<int> result = new FailureResultStub<int>(new[] { error }, "Validation failed", 0, new ResultStatusStub(400, "Bad Request"));
            var factory = CreateFactory();
            var context = CreateHttpContext();

            var pd = result.ToProblemDetails(factory, context, ProblemTypeUri);

            // Assert
            pd.Should().BeOfType<ValidationProblemDetails>();
            pd.Status.Should().Be(400);
            pd.Title.Should().Be("Bad Request");
            pd.Type.Should().Be($"{ProblemTypeUri}validation");
            pd.Detail.Should().Be("Validation failed");
        }

        [Fact]
        public void ToHttpStatusCode_Maps_ErrorCategory()
        {
            // Arrange
            var categories = new Dictionary<ErrorCategory, HttpStatusCode>
            {
                { ErrorCategory.NotFound, HttpStatusCode.NotFound },
                { ErrorCategory.Validation, HttpStatusCode.BadRequest },
                { ErrorCategory.Conflict, HttpStatusCode.Conflict },
                { ErrorCategory.Authentication, HttpStatusCode.Unauthorized },
                { ErrorCategory.Network, HttpStatusCode.ServiceUnavailable },
                { ErrorCategory.Timeout, HttpStatusCode.RequestTimeout },
                { ErrorCategory.Security, HttpStatusCode.Forbidden },
                { ErrorCategory.Request, HttpStatusCode.BadRequest },
                { ErrorCategory.Database, HttpStatusCode.InternalServerError }
            };

            foreach (var kvp in categories)
            {
                var error = new ErrorInfo(kvp.Key, "CODE", "Message");
                var result = new FailureResultStub(new[] { error }, "Message", new ResultStatusStub(500, "Error"));
                var code = result.ToHttpStatusCode();
                code.Should().Be(kvp.Value);
            }
        }

        [Fact]
        public void ToHttpStatusCode_Returns_Ok_On_Success()
        {
            // Arrange
            var result = new SuccessResultStub(new ResultStatusStub((int)HttpStatusCode.OK, "OK"));

            // Act
            var code = result.ToHttpStatusCode();

            // Assert
            code.Should().Be(HttpStatusCode.OK);
        }
    }
}
