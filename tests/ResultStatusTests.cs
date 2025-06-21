// <copyright file="ResultStatusTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Net;

using FluentAssertions;

using Xunit;

using Zentient.Results;
using Zentient.Results.Constants;

namespace Zentient.Results.Tests
{
    public class ResultStatusTests
    {
        #region Constructor and Custom Factory

        [Fact]
        public void Constructor_InitializesCodeAndDescription()
        {
            // Arrange
            var code = 200;
            var description = "OK";

            // Act
            var status = new ResultStatus(code, description);

            // Assert
            status.Code.Should().Be(code);
            status.Description.Should().Be(description);
        }

        [Fact]
        public void Custom_FactoryMethod_InitializesCodeAndDescription()
        {
            // Arrange
            var code = 1000;
            var description = "Custom Status";

            // Act
            var status = ResultStatus.Custom(code, description);

            // Assert
            status.Code.Should().Be(code);
            status.Description.Should().Be(description);
        }

        #endregion

        #region FromHttpStatusCode Method

        [Fact]
        public void FromHttpStatusCode_ValidCode_ReturnsCorrectStatus()
        {
            // Arrange
            var httpCode = (int)HttpStatusCode.OK;

            // Act
            IResultStatus status = ResultStatus.FromHttpStatusCode(httpCode);

            // Assert
            status.Code.Should().Be(httpCode);
            status.Description.Should().Be(HttpStatusCode.OK.ToString());
        }

        [Fact]
        public void FromHttpStatusCode_AnotherValidCode_ReturnsCorrectStatus()
        {
            // Arrange
            var httpCode = (int)HttpStatusCode.NotFound;

            // Act
            IResultStatus status = ResultStatus.FromHttpStatusCode(httpCode);

            // Assert
            status.Code.Should().Be(httpCode);
            status.Description.Should().Be(ResultStatusConstants.Description.NotFound);
        }

        [Fact]
        public void FromHttpStatusCode_UnknownCode_ReturnsStatusWithNumericDescription()
        {
            // Arrange
            var unknownCode = 999;

            // Act
            IResultStatus status = ResultStatus.FromHttpStatusCode(unknownCode);

            // Assert
            status.Code.Should().Be(unknownCode);
            status.Description.Should().Be(((HttpStatusCode)unknownCode).ToString());
        }

        [Fact]
        public void FromHttpStatusCode_NegativeCode_HandlesGracefully()
        {
            // Arrange
            var negativeCode = -1;

            // Act
            IResultStatus status = ResultStatus.FromHttpStatusCode(negativeCode);

            // Assert
            status.Code.Should().Be(negativeCode);
            status.Description.Should().Be(negativeCode.ToString());
        }

        #endregion

        #region ToString() Override

        [Fact]
        public void ToString_FormatsCorrectly()
        {
            // Arrange
            var status = new ResultStatus(400, "Bad Request");

            // Act
            var result = status.ToString();

            // Assert
            result.Should().Be("(400) Bad Request");
        }

        #endregion

        #region Equality (IEquatable<ResultStatus>)

        [Fact]
        public void Equals_SameCodeAndDescription_ReturnsTrue()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");
            var status2 = new ResultStatus(200, "OK");

            // Assert
            status1.Equals(status2).Should().BeTrue();
            status1.Equals((object)status2).Should().BeTrue();
        }

        [Fact]
        public void Equals_DifferentCode_ReturnsFalse()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");
            var status2 = new ResultStatus(400, "OK");

            // Assert
            status1.Equals(status2).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentDescription_ReturnsFalse()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");
            var status2 = new ResultStatus(200, "Created");

            // Assert
            status1.Equals(status2).Should().BeFalse();
        }

        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");
            var status2 = status1;

            // Assert
            status1.Equals(status2).Should().BeTrue();
        }

        [Fact]
        public void Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");

            // Assert
            status1.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentObjectType_ReturnsFalse()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");
            var otherObject = new object();

            // Assert
            status1.Equals(otherObject).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_ConsistentForEqualObjects()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");
            var status2 = new ResultStatus(200, "OK");

            // Assert
            status1.GetHashCode().Should().Be(status2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_DifferentForUnequalObjects()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");
            var status2 = new ResultStatus(400, "Bad Request");

            // Assert
            status1.GetHashCode().Should().NotBe(status2.GetHashCode());
        }

        [Fact]
        public void EqualityOperator_SameCodeAndDescription_ReturnsTrue()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");
            var status2 = new ResultStatus(200, "OK");

            // Assert
            (status1 == status2).Should().BeTrue();
        }

        [Fact]
        public void InequalityOperator_DifferentCodeOrDescription_ReturnsTrue()
        {
            // Arrange
            var status1 = new ResultStatus(200, "OK");
            var status2 = new ResultStatus(200, "Created");

            // Assert
            (status1 != status2).Should().BeTrue();
        }

        [Fact]
        public void EqualityOperator_Nullability_HandlesNullCorrectly()
        {
            // Arrange
            ResultStatus? status1 = new ResultStatus(200, "OK");
            ResultStatus? status2 = null;
            ResultStatus? status3 = null;

            // Assert
            (status1 == status2).Should().BeFalse();
            (status2 == status1).Should().BeFalse();
            (status2 == status3).Should().BeTrue();
        }

        [Fact]
        public void InequalityOperator_Nullability_HandlesNullCorrectly()
        {
            // Arrange
            ResultStatus? status1 = new ResultStatus(200, "OK");
            ResultStatus? status2 = null;
            ResultStatus? status3 = null;

            // Assert
            (status1 != status2).Should().BeTrue();
            (status2 != status1).Should().BeTrue();
            (status2 != status3).Should().BeFalse();
        }

        #endregion
    }
}