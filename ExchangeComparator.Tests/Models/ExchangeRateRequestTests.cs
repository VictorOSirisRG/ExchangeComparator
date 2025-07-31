using ExchangeComparator.Domain.Models;
using FluentAssertions;
using Xunit;

namespace ExchangeComparator.Tests.Models
{
    public class ExchangeRateRequestTests
    {
        [Fact]
        public void ExchangeRateRequest_WithValidData_ShouldCreateSuccessfully()
        {
            // Arrange & Act
            var request = new ExchangeRateRequest("USD", "EUR", 100.0m);

            // Assert
            request.Should().NotBeNull();
            request.SourceCurrency.Should().Be("USD");
            request.TargetCurrency.Should().Be("EUR");
            request.Amount.Should().Be(100.0m);
        }

        [Fact]
        public void ExchangeRateRequest_WithZeroAmount_ShouldBeValid()
        {
            // Arrange & Act
            var request = new ExchangeRateRequest("USD", "EUR", 0m);

            // Assert
            request.Should().NotBeNull();
            request.Amount.Should().Be(0m);
        }

        [Fact]
        public void ExchangeRateRequest_WithNegativeAmount_ShouldBeValid()
        {
            // Arrange & Act
            var request = new ExchangeRateRequest("USD", "EUR", -50.0m);

            // Assert
            request.Should().NotBeNull();
            request.Amount.Should().Be(-50.0m);
        }

        [Fact]
        public void ExchangeRateRequest_WithEmptyCurrencies_ShouldBeValid()
        {
            // Arrange & Act
            var request = new ExchangeRateRequest("", "", 100.0m);

            // Assert
            request.Should().NotBeNull();
            request.SourceCurrency.Should().Be("");
            request.TargetCurrency.Should().Be("");
        }

        [Fact]
        public void ExchangeRateRequest_WithNullCurrencies_ShouldBeValid()
        {
            // Arrange & Act
            var request = new ExchangeRateRequest(null, null, 100.0m);

            // Assert
            request.Should().NotBeNull();
            request.SourceCurrency.Should().BeNull();
            request.TargetCurrency.Should().BeNull();
        }

        [Fact]
        public void ExchangeRateRequest_WithLargeAmount_ShouldBeValid()
        {
            // Arrange & Act
            var request = new ExchangeRateRequest("USD", "EUR", decimal.MaxValue);

            // Assert
            request.Should().NotBeNull();
            request.Amount.Should().Be(decimal.MaxValue);
        }

        [Fact]
        public void ExchangeRateRequest_WithDecimalPrecision_ShouldBeValid()
        {
            // Arrange & Act
            var request = new ExchangeRateRequest("USD", "EUR", 100.123456789m);

            // Assert
            request.Should().NotBeNull();
            request.Amount.Should().Be(100.123456789m);
        }
    }
} 