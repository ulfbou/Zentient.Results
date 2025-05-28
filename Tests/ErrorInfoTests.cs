using FluentAssertions;

namespace Zentient.Results.Tests
{
    public class ErrorInfoTests
    {
        [Fact]
        public void Constructor_Sets_All_Properties()
        {
            // Arrange
            var category = ErrorCategory.Database;
            var code = "DB-001";
            var message = "Database error occurred.";
            var data = new { Table = "Users" };
            var innerErrors = new List<ErrorInfo>
            {
                new ErrorInfo(ErrorCategory.Validation, "VAL-001", "Validation failed.")
            };

            // Act
            var error = new ErrorInfo(category, code, message, data, innerErrors);

            // Assert
            error.Category.Should().Be(category);
            error.Code.Should().Be(code);
            error.Message.Should().Be(message);
            error.Data.Should().Be(data);
            error.InnerErrors.Should().BeEquivalentTo(innerErrors);
        }

        [Fact]
        public void Constructor_Handles_Null_InnerErrors_As_Empty()
        {
            // Act
            var error = new ErrorInfo(ErrorCategory.General, "GEN-001", "General error.", null, null);

            // Assert
            error.InnerErrors.Should().NotBeNull();
            error.InnerErrors.Should().BeEmpty();
        }

        [Fact]
        public void Aggregate_Creates_Validation_Error_With_InnerErrors()
        {
            // Arrange
            var inner = new[]
            {
                new ErrorInfo(ErrorCategory.Validation, "VAL-1", "First error"),
                new ErrorInfo(ErrorCategory.Validation, "VAL-2", "Second error")
            };

            // Act
            var agg = ErrorInfo.Aggregate("AGG-001", "Aggregate error", inner);

            // Assert
            agg.Category.Should().Be(ErrorCategory.Validation);
            agg.Code.Should().Be("AGG-001");
            agg.Message.Should().Be("Aggregate error");
            agg.InnerErrors.Should().BeEquivalentTo(inner);
        }

        [Fact]
        public void Aggregate_Can_Include_Optional_Data()
        {
            // Arrange
            var inner = new[]
            {
                new ErrorInfo(ErrorCategory.Validation, "VAL-1", "First error")
            };
            var data = new { Field = "Email" };

            // Act
            var agg = ErrorInfo.Aggregate("AGG-002", "Aggregate error", inner, data);

            // Assert
            agg.Data.Should().Be(data);
        }

        [Fact]
        public void ToString_Returns_Expected_Format()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Security, "SEC-001", "Security violation");

            // Act
            var str = error.ToString();

            // Assert
            str.Should().Be("[Security:SEC-001] Security violation");
        }

        [Fact]
        public void InnerErrors_Can_Be_Nested()
        {
            // Arrange
            var leaf = new ErrorInfo(ErrorCategory.Request, "REQ-001", "Bad request");
            var mid = new ErrorInfo(ErrorCategory.Validation, "VAL-002", "Validation failed", null, new[] { leaf });
            var root = new ErrorInfo(ErrorCategory.Exception, "EX-001", "Exception occurred", null, new[] { mid });

            // Assert
            root.InnerErrors.Should().HaveCount(1);
            root.InnerErrors[0].InnerErrors.Should().HaveCount(1);
            root.InnerErrors[0].InnerErrors[0].Code.Should().Be("REQ-001");
        }

        [Fact]
        public void Properties_Are_Immutable()
        {
            // Arrange
            var error = new ErrorInfo(ErrorCategory.Conflict, "CON-001", "Conflict error");

            // Assert
            typeof(ErrorInfo).GetProperty(nameof(ErrorInfo.Code))!.CanWrite.Should().BeFalse();
            typeof(ErrorInfo).GetProperty(nameof(ErrorInfo.Message))!.CanWrite.Should().BeFalse();
            typeof(ErrorInfo).GetProperty(nameof(ErrorInfo.Category))!.CanWrite.Should().BeFalse();
            typeof(ErrorInfo).GetProperty(nameof(ErrorInfo.Data))!.CanWrite.Should().BeFalse();
            typeof(ErrorInfo).GetProperty(nameof(ErrorInfo.InnerErrors))!.CanWrite.Should().BeFalse();
        }

        [Fact]
        public void Supports_All_ErrorCategory_Values()
        {
            foreach (ErrorCategory category in Enum.GetValues(typeof(ErrorCategory)))
            {
                var error = new ErrorInfo(category, "CODE", "Message");
                error.Category.Should().Be(category);
            }
        }

        [Fact]
        public void Can_Handle_Empty_Strings_And_Null_Data()
        {
            // Act
            var error = new ErrorInfo(ErrorCategory.General, "", "", null, null);

            // Assert
            error.Code.Should().BeEmpty();
            error.Message.Should().BeEmpty();
            error.Data.Should().BeNull();
            error.InnerErrors.Should().BeEmpty();
        }

        [Fact]
        public void Aggregate_NotThrows_If_InnerErrors_Is_Null()
        {
            // Act
            var act = () => ErrorInfo.Aggregate("AGG-003", "Aggregate error", null!);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Aggregate_NotThrows_If_Code_Or_Message_Is_Null()
        {
            // Arrange
            var inner = new[] { new ErrorInfo(ErrorCategory.General, "C", "M") };

            // Act
            Action act1 = () => ErrorInfo.Aggregate(null!, "msg", inner);
            Action act2 = () => ErrorInfo.Aggregate("code", null!, inner);

            // Assert
            act1.Should().NotThrow();
            act2.Should().NotThrow();
        }
    }
}
