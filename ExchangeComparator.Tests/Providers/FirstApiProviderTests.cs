using ExchangeComparator.Domain.Models;
using ExchangeComparator.Infrastructure.Providers;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ExchangeComparator.Tests.Providers
{
    public class FirstApiProviderTests : TestBase, IDisposable
    {
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;
        private readonly HttpClient _httpClient;
        private readonly FirstApiProvider _provider;

        public FirstApiProviderTests()
        {
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpHandler.Object)
            {
                BaseAddress = new Uri("https://api1.example.com")
            };
            _provider = new FirstApiProvider(_httpClient);
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithValidRequest_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var expectedRate = 0.85m;
            var responseContent = JsonSerializer.Serialize(new { rate = expectedRate });

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(responseContent));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertSuccessResponse(result, "Api1");
            result.Rate.Should().Be(expectedRate);
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithHttpError_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateErrorResponse(HttpStatusCode.InternalServerError));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertFailureResponse(result, "Api1");
            result.ErrorMessage.Should().Be("Non-success status code");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithInvalidJson_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var invalidJson = "{ invalid json }";

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(invalidJson));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertFailureResponse(result, "Api1");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithZeroRate_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var responseContent = JsonSerializer.Serialize(new { rate = 0 });

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(responseContent));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertFailureResponse(result, "Api1");
            result.ErrorMessage.Should().Be("Invalid or missing rate");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithNegativeRate_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var responseContent = JsonSerializer.Serialize(new { rate = -1.5m });

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(responseContent));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertFailureResponse(result, "Api1");
            result.ErrorMessage.Should().Be("Invalid or missing rate");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithNullResponse_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var responseContent = JsonSerializer.Serialize((object?)null);

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(responseContent));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertFailureResponse(result, "Api1");
            result.ErrorMessage.Should().Be("Invalid or missing rate");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithHttpException_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertFailureResponse(result, "Api1");
            result.ErrorMessage.Should().Be("Network error");
        }

        [Fact]
        public async Task GetExchangeRateResponse_ShouldSendCorrectRequestFormat()
        {
            // Arrange
            var request = CreateValidRequest();
            var expectedRate = 0.85m;
            var responseContent = JsonSerializer.Serialize(new { rate = expectedRate });

            HttpRequestMessage? capturedRequest = null;

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(CreateSuccessResponse(responseContent));

            // Act
            await _provider.GetExchangeRateResponse(request);

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Post);
            capturedRequest.RequestUri!.PathAndQuery.Should().Be("/api1/rate");
            capturedRequest.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
} 