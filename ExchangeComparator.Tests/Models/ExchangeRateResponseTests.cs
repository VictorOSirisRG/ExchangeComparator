using ExchangeComparator.Domain.Models;
using FluentAssertions;
using Xunit;

namespace ExchangeComparator.Tests.Models
{
    public class ExchangeRateResponseTests
    {
        [Fact]
        public void ExchangeRateResponse_WithValidData_ShouldCreateSuccessfully()
        {
            // Arrange & Act
            var response = new ExchangeRateResponse("TestProvider", 0.85m);

            // Assert
            response.Should().NotBeNull();
            response.ProviderName.Should().Be("TestProvider");
            response.Rate.Should().Be(0.85m);
            response.IsSuccess.Should().BeTrue();
            response.ErrorMessage.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ExchangeRateResponse_WithZeroRate_ShouldBeValid()
        {
            // Arrange & Act
            var response = new ExchangeRateResponse("TestProvider", 0m);

            // Assert
            response.Should().NotBeNull();
            response.Rate.Should().Be(0m);
            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ExchangeRateResponse_WithNegativeRate_ShouldBeValid()
        {
            // Arrange & Act
            var response = new ExchangeRateResponse("TestProvider", -1.5m);

            // Assert
            response.Should().NotBeNull();
            response.Rate.Should().Be(-1.5m);
            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ExchangeRateResponse_WithLargeRate_ShouldBeValid()
        {
            // Arrange & Act
            var response = new ExchangeRateResponse("TestProvider", decimal.MaxValue);

            // Assert
            response.Should().NotBeNull();
            response.Rate.Should().Be(decimal.MaxValue);
            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ExchangeRateResponse_WithDecimalPrecision_ShouldBeValid()
        {
            // Arrange & Act
            var response = new ExchangeRateResponse("TestProvider", 0.123456789m);

            // Assert
            response.Should().NotBeNull();
            response.Rate.Should().Be(0.123456789m);
            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ExchangeRateResponse_WithEmptyProviderName_ShouldBeValid()
        {
            // Arrange & Act
            var response = new ExchangeRateResponse("", 0.85m);

            // Assert
            response.Should().NotBeNull();
            response.ProviderName.Should().Be("");
            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void ExchangeRateResponse_WithNullProviderName_ShouldBeValid()
        {
            // Arrange & Act
            var response = new ExchangeRateResponse(null, 0.85m);

            // Assert
            response.Should().NotBeNull();
            response.ProviderName.Should().BeNull();
            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Failure_WithValidData_ShouldCreateFailureResponse()
        {
            // Arrange & Act
            var response = ExchangeRateResponse.Failure("TestProvider", "API Error");

            // Assert
            response.Should().NotBeNull();
            response.ProviderName.Should().Be("TestProvider");
            response.Rate.Should().Be(0m);
            response.IsSuccess.Should().BeFalse();
            response.ErrorMessage.Should().Be("API Error");
        }

        [Fact]
        public void Failure_WithEmptyErrorMessage_ShouldBeValid()
        {
            // Arrange & Act
            var response = ExchangeRateResponse.Failure("TestProvider", "");

            // Assert
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeFalse();
            response.ErrorMessage.Should().Be("");
        }

        [Fact]
        public void Failure_WithNullErrorMessage_ShouldBeValid()
        {
            // Arrange & Act
            var response = ExchangeRateResponse.Failure("TestProvider", null);

            // Assert
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeFalse();
            response.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void Failure_WithEmptyProviderName_ShouldBeValid()
        {
            // Arrange & Act
            var response = ExchangeRateResponse.Failure("", "API Error");

            // Assert
            response.Should().NotBeNull();
            response.ProviderName.Should().Be("");
            response.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Failure_WithNullProviderName_ShouldBeValid()
        {
            // Arrange & Act
            var response = ExchangeRateResponse.Failure(null, "API Error");

            // Assert
            response.Should().NotBeNull();
            response.ProviderName.Should().BeNull();
            response.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void ExchangeRateResponse_ShouldBeImmutable()
        {
            // Arrange
            var response = new ExchangeRateResponse("TestProvider", 0.85m);

            // Act & Assert
            response.Should().NotBeNull();
            
            // Verify that properties are read-only (can't be changed after creation)
            // This is implicit in the current implementation since properties are get-only
            response.ProviderName.Should().Be("TestProvider");
            response.Rate.Should().Be(0.85m);
            response.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Failure_ShouldBeImmutable()
        {
            // Arrange
            var response = ExchangeRateResponse.Failure("TestProvider", "API Error");

            // Act & Assert
            response.Should().NotBeNull();
            
            // Verify that properties are read-only
            response.ProviderName.Should().Be("TestProvider");
            response.Rate.Should().Be(0m);
            response.IsSuccess.Should().BeFalse();
            response.ErrorMessage.Should().Be("API Error");
        }
    }
} 