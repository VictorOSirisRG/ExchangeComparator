using ExchangeComparator.Application.Services;
using ExchangeComparator.Domain.Interfaces;
using ExchangeComparator.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ExchangeComparator.Tests.Services
{
    public class ExchangeRateServiceTests : TestBase
    {
        private readonly Mock<IExchangeRateProvider> _mockProvider1;
        private readonly Mock<IExchangeRateProvider> _mockProvider2;
        private readonly Mock<IExchangeRateProvider> _mockProvider3;
        private readonly ExchangeRateService _service;

        public ExchangeRateServiceTests()
        {
            _mockProvider1 = new Mock<IExchangeRateProvider>();
            _mockProvider2 = new Mock<IExchangeRateProvider>();
            _mockProvider3 = new Mock<IExchangeRateProvider>();

            var providers = new List<IExchangeRateProvider>
            {
                _mockProvider1.Object,
                _mockProvider2.Object,
                _mockProvider3.Object
            };

            var mockLogger = new Mock<ILogger<ExchangeRateService>>();
            _service = new ExchangeRateService(providers, mockLogger.Object);
        }

        [Fact]
        public async Task GetBestRateAsync_WithAllProvidersSuccess_ShouldReturnHighestRate()
        {
            // Arrange
            var request = CreateValidRequest();
            var rate1 = 0.85m;
            var rate2 = 0.90m;
            var rate3 = 0.88m;

            _mockProvider1.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider1", rate1));

            _mockProvider2.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider2", rate2));

            _mockProvider3.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider3", rate3));

            // Act
            var result = await _service.GetBestRateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Rate.Should().Be(rate2); // Highest rate
            result.ProviderName.Should().Be("Provider2");
        }

        [Fact]
        public async Task GetBestRateAsync_WithSomeProvidersFailing_ShouldReturnBestFromSuccessfulOnes()
        {
            // Arrange
            var request = CreateValidRequest();
            var rate1 = 0.85m;
            var rate3 = 0.88m;

            _mockProvider1.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider1", rate1));

            _mockProvider2.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(ExchangeRateResponse.Failure("Provider2", "API Error"));

            _mockProvider3.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider3", rate3));

            // Act
            var result = await _service.GetBestRateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Rate.Should().Be(rate3); // Highest rate from successful providers
            result.ProviderName.Should().Be("Provider3");
        }

        [Fact]
        public async Task GetBestRateAsync_WithAllProvidersFailing_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();

            _mockProvider1.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(ExchangeRateResponse.Failure("Provider1", "API Error 1"));

            _mockProvider2.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(ExchangeRateResponse.Failure("Provider2", "API Error 2"));

            _mockProvider3.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(ExchangeRateResponse.Failure("Provider3", "API Error 3"));

            // Act
            var result = await _service.GetBestRateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ProviderName.Should().Be("AllProviders");
            result.ErrorMessage.Should().Be("All providers failed to return a valid rate.");
            result.Rate.Should().Be(0);
        }

        [Fact]
        public async Task GetBestRateAsync_WithEqualRates_ShouldReturnFirstProvider()
        {
            // Arrange
            var request = CreateValidRequest();
            var rate = 0.85m;

            _mockProvider1.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider1", rate));

            _mockProvider2.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider2", rate));

            _mockProvider3.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider3", rate));

            // Act
            var result = await _service.GetBestRateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Rate.Should().Be(rate);
            result.ProviderName.Should().Be("Provider1"); // First in the list
        }

        [Fact]
        public async Task GetBestRateAsync_WithZeroRates_ShouldReturnFirstNonZeroRate()
        {
            // Arrange
            var request = CreateValidRequest();
            var rate1 = 0m;
            var rate2 = 0.88m;
            var rate3 = 0m;

            _mockProvider1.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider1", rate1));

            _mockProvider2.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider2", rate2));

            _mockProvider3.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider3", rate3));

            // Act
            var result = await _service.GetBestRateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Rate.Should().Be(rate2);
            result.ProviderName.Should().Be("Provider2");
        }

        [Fact]
        public async Task GetBestRateAsync_WithProviderException_ShouldHandleGracefully()
        {
            // Arrange
            var request = CreateValidRequest();
            var rate1 = 0.85m;
            var rate3 = 0.88m;

            _mockProvider1.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider1", rate1));

            _mockProvider2.Setup(p => p.GetExchangeRateResponse(request))
                .ThrowsAsync(new Exception("Provider exception"));

            _mockProvider3.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider3", rate3));

            // Act
            var result = await _service.GetBestRateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Rate.Should().Be(rate3); // Highest rate from successful providers
            result.ProviderName.Should().Be("Provider3");
        }

        [Fact]
        public async Task GetBestRateAsync_WithNullRequest_ShouldHandleGracefully()
        {
            // Arrange
            ExchangeRateRequest? request = null;

            _mockProvider1.Setup(p => p.GetExchangeRateResponse(request!))
                .ReturnsAsync(ExchangeRateResponse.Failure("Provider1", "Invalid request"));

            _mockProvider2.Setup(p => p.GetExchangeRateResponse(request!))
                .ReturnsAsync(ExchangeRateResponse.Failure("Provider2", "Invalid request"));

            _mockProvider3.Setup(p => p.GetExchangeRateResponse(request!))
                .ReturnsAsync(ExchangeRateResponse.Failure("Provider3", "Invalid request"));

            // Act
            var result = await _service.GetBestRateAsync(request!);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ProviderName.Should().Be("AllProviders");
        }

        [Fact]
        public async Task GetBestRateAsync_ShouldCallAllProviders()
        {
            // Arrange
            var request = CreateValidRequest();
            var rate = 0.85m;

            _mockProvider1.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider1", rate));

            _mockProvider2.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider2", rate));

            _mockProvider3.Setup(p => p.GetExchangeRateResponse(request))
                .ReturnsAsync(new ExchangeRateResponse("Provider3", rate));

            // Act
            await _service.GetBestRateAsync(request);

            // Assert
            _mockProvider1.Verify(p => p.GetExchangeRateResponse(request), Times.Once);
            _mockProvider2.Verify(p => p.GetExchangeRateResponse(request), Times.Once);
            _mockProvider3.Verify(p => p.GetExchangeRateResponse(request), Times.Once);
        }

        [Fact]
        public async Task GetBestRateAsync_WithEmptyProvidersList_ShouldReturnFailureResponse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ExchangeRateService>>();
            var emptyService = new ExchangeRateService(new List<IExchangeRateProvider>(), mockLogger.Object);
            var request = CreateValidRequest();

            // Act
            var result = await emptyService.GetBestRateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ProviderName.Should().Be("AllProviders");
            result.ErrorMessage.Should().Be("All providers failed to return a valid rate.");
        }
    }
} 