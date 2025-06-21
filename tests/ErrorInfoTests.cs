// <copyright file="ErrorInfoTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using FluentAssertions;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Xunit;

namespace Zentient.Results.Tests
{
    public class ErrorInfoTests
    {
        [Fact]
        public void Constructor_Sets_All_Properties()
        {
            var category = ErrorCategory.Database;
            var code = "DB-001";
            var message = "Database error occurred.";
            var data = new { Table = "Users" };
            var metadata = new Dictionary<string, object?> { { "Query", "SELECT * FROM Users" } };
            var innerErrors = new List<ErrorInfo>
            {
                new ErrorInfo(ErrorCategory.Validation, "VAL-001", "Validation failed.")
            };

            // Adjusted constructor call to match the new ErrorInfo signature
            var error = new ErrorInfo(category, code, message,
                                      detail: "Detailed DB error",
                                      metadata: metadata.ToImmutableDictionary(),
                                      innerErrors: innerErrors.ToImmutableArray());

            error.Category.Should().Be(category);
            error.Code.Should().Be(code);
            error.Message.Should().Be(message);
            error.Detail.Should().Be("Detailed DB error");
            error.Metadata.Should().BeEquivalentTo(metadata);
            error.InnerErrors.Should().BeEquivalentTo(innerErrors);
        }

        [Fact]
        public void Constructor_Handles_Null_InnerErrors_As_Empty()
        {
            // Adjusted constructor call to match the new ErrorInfo signature
            var error = new ErrorInfo(ErrorCategory.General, "GEN-001", "General error.", detail: null, metadata: null, innerErrors: null);
            error.InnerErrors.Should().NotBeNull();
            error.InnerErrors.Should().BeEmpty();
        }

        [Fact]
        public void ToString_Returns_Expected_Format()
        {
            var error = new ErrorInfo(ErrorCategory.Security, "SEC-001", "Security violation");
            var str = error.ToString();
            str.Should().Be("ErrorInfo(Category: Security, Code: SEC-001, Message: Security violation)");

            var errorWithDetail = new ErrorInfo(ErrorCategory.Timeout, "TIMEOUT-001", "Request timed out", detail: "Network latency");
            errorWithDetail.ToString().Should().Be("ErrorInfo(Category: Timeout, Code: TIMEOUT-001, Message: Request timed out, Detail: Network latency)");

            var errorWithMetadata = new ErrorInfo(
                ErrorCategory.General,
                "META-001",
                "Meta info",
                detail: null,
                metadata: ImmutableDictionary.CreateRange<string, object?>(new[] { new KeyValuePair<string, object?>("SessionId", "123") }));

            errorWithMetadata.ToString().Should().Contain("Metadata: {SessionId=123}");
            var errorWithInner = new ErrorInfo(ErrorCategory.General, "Parent", "Parent error", innerErrors: ImmutableArray.Create<ErrorInfo>(new[] { new ErrorInfo(ErrorCategory.General, "Child", "Child error") }));
            errorWithInner.ToString().Should().Contain("Inner Errors: [ErrorInfo(Category: General, Code: Child, Message: Child error)]");
        }

        [Fact]
        public void InnerErrors_Can_Be_Nested()
        {
            var leaf = new ErrorInfo(ErrorCategory.Request, "REQ-001", "Bad request");
            var mid = new ErrorInfo(ErrorCategory.Validation, "VAL-002", "Validation failed", innerErrors: ImmutableList.Create(leaf));
            var root = new ErrorInfo(ErrorCategory.Exception, "EX-001", "Exception occurred", innerErrors: ImmutableList.Create(mid));

            root.InnerErrors.Should().HaveCount(1);
            root.InnerErrors[0].InnerErrors.Should().HaveCount(1);
            root.InnerErrors[0].InnerErrors![0].Code.Should().Be("REQ-001");
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
            var error = new ErrorInfo(ErrorCategory.General, "", "", detail: null, metadata: null, innerErrors: null);
            error.Code.Should().BeEmpty();
            error.Message.Should().BeEmpty();
            error.Detail.Should().BeNull();
            error.Metadata.Should().NotBeNull();
            error.Metadata.Should().BeEmpty();
            error.InnerErrors.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Throws_On_Null_Code_Or_Message()
        {
            Action act1 = () => new ErrorInfo(ErrorCategory.General, null!, "message");
            Action act2 = () => new ErrorInfo(ErrorCategory.General, "code", null!);
            act1.Should().Throw<ArgumentNullException>().WithParameterName("code");
            act2.Should().Throw<ArgumentNullException>().WithParameterName("message");
        }
    }
}
