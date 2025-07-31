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
    public class ThirdApiProviderTests : TestBase, IDisposable
    {
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;
        private readonly HttpClient _httpClient;
        private readonly ThirdApiProvider _provider;

        public ThirdApiProviderTests()
        {
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpHandler.Object)
            {
                BaseAddress = new Uri("https://api3.example.com")
            };
            _provider = new ThirdApiProvider(_httpClient);
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithValidRequest_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var expectedRate = 0.85m;
            var responseContent = JsonSerializer.Serialize(new
            {
                statusCode = 200,
                message = "Success",
                data = new { total = expectedRate }
            });

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
            AssertSuccessResponse(result, "ThirdApiProvider");
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
            AssertFailureResponse(result, "ThirdApiProvider");
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
            AssertFailureResponse(result, "ThirdApiProvider");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithZeroTotal_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var responseContent = JsonSerializer.Serialize(new
            {
                statusCode = 200,
                message = "Success",
                data = new { total = 0 }
            });

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
            AssertFailureResponse(result, "ThirdApiProvider");
            result.ErrorMessage.Should().Be("Invalid data or missing total");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithNegativeTotal_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var responseContent = JsonSerializer.Serialize(new
            {
                statusCode = 200,
                message = "Success",
                data = new { total = -1.5m }
            });

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
            AssertFailureResponse(result, "ThirdApiProvider");
            result.ErrorMessage.Should().Be("Invalid data or missing total");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithNullData_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var responseContent = JsonSerializer.Serialize(new
            {
                statusCode = 200,
                message = "Success",
                data = (object?)null
            });

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
            AssertFailureResponse(result, "ThirdApiProvider");
            result.ErrorMessage.Should().Be("Invalid data or missing total");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithMissingDataProperty_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var responseContent = JsonSerializer.Serialize(new
            {
                statusCode = 200,
                message = "Success"
            });

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
            AssertFailureResponse(result, "ThirdApiProvider");
            result.ErrorMessage.Should().Be("Invalid data or missing total");
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
            AssertFailureResponse(result, "ThirdApiProvider");
            result.ErrorMessage.Should().Be("Network error");
        }

        [Fact]
        public async Task GetExchangeRateResponse_ShouldSendCorrectRequestFormat()
        {
            // Arrange
            var request = CreateValidRequest();
            var expectedRate = 0.85m;
            var responseContent = JsonSerializer.Serialize(new
            {
                statusCode = 200,
                message = "Success",
                data = new { total = expectedRate }
            });

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
            capturedRequest.RequestUri!.PathAndQuery.Should().Be("/ThirdApiProvider/rate");
            capturedRequest.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");

            // Verify JSON content structure
            var content = await capturedRequest.Content.ReadAsStringAsync();
            var requestData = JsonSerializer.Deserialize<JsonElement>(content);
            requestData.GetProperty("exchange").GetProperty("sourceCurrency").GetString().Should().Be(request.SourceCurrency);
            requestData.GetProperty("exchange").GetProperty("targetCurrency").GetString().Should().Be(request.TargetCurrency);
            requestData.GetProperty("exchange").GetProperty("quantity").GetDecimal().Should().Be(request.Amount);
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithDifferentStatusCode_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var expectedRate = 0.85m;
            var responseContent = JsonSerializer.Serialize(new
            {
                statusCode = 100,
                message = "Success",
                data = new { total = expectedRate }
            });

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
            AssertSuccessResponse(result, "ThirdApiProvider");
            result.Rate.Should().Be(expectedRate);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
} 