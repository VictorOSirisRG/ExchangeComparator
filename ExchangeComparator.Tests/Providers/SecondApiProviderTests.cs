using ExchangeComparator.Domain.Models;
using ExchangeComparator.Infrastructure.Providers;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace ExchangeComparator.Tests.Providers
{
    public class SecondApiProviderTests : TestBase, IDisposable
    {
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;
        private readonly HttpClient _httpClient;
        private readonly SecondApiProvider _provider;

        public SecondApiProviderTests()
        {
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpHandler.Object)
            {
                BaseAddress = new Uri("https://api2.example.com")
            };
            _provider = new SecondApiProvider(_httpClient);
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithValidRequest_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var expectedRate = 0.85m;
            var xmlResponse = new XElement("Response",
                new XElement("Result", expectedRate.ToString())
            ).ToString();

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(xmlResponse, "application/xml"));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertSuccessResponse(result, "Api2");
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
            AssertFailureResponse(result, "Api2");
            result.ErrorMessage.Should().Be("Non-success status code");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithInvalidXml_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var invalidXml = "<invalid>xml</invalid>";

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(invalidXml, "application/xml"));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertFailureResponse(result, "Api2");
            result.ErrorMessage.Should().Be("Invalid XML or missing result");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithMissingResultElement_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var xmlResponse = new XElement("Response",
                new XElement("Status", "OK")
            ).ToString();

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(xmlResponse, "application/xml"));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertFailureResponse(result, "Api2");
            result.ErrorMessage.Should().Be("Invalid XML or missing result");
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithInvalidRateValue_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var xmlResponse = new XElement("Response",
                new XElement("Result", "invalid_rate")
            ).ToString();

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(xmlResponse, "application/xml"));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertFailureResponse(result, "Api2");
            result.ErrorMessage.Should().Be("Invalid XML or missing result");
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
            AssertFailureResponse(result, "Api2");
            result.ErrorMessage.Should().Be("Network error");
        }

        [Fact]
        public async Task GetExchangeRateResponse_ShouldSendCorrectXmlRequestFormat()
        {
            // Arrange
            var request = CreateValidRequest();
            var expectedRate = 0.85m;
            var xmlResponse = new XElement("Response",
                new XElement("Result", expectedRate.ToString())
            ).ToString();

            HttpRequestMessage? capturedRequest = null;

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(CreateSuccessResponse(xmlResponse, "application/xml"));

            // Act
            await _provider.GetExchangeRateResponse(request);

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.Method.Should().Be(HttpMethod.Post);
            capturedRequest.RequestUri!.PathAndQuery.Should().Be("/api2/rate");
            capturedRequest.Content!.Headers.ContentType!.MediaType.Should().Be("application/xml");

            // Verify XML content structure
            var content = await capturedRequest.Content.ReadAsStringAsync();
            var xml = XElement.Parse(content);
            xml.Name.LocalName.Should().Be("XML");
            xml.Element("From")?.Value.Should().Be(request.SourceCurrency);
            xml.Element("To")?.Value.Should().Be(request.TargetCurrency);
            xml.Element("Amount")?.Value.Should().Be(request.Amount.ToString());
        }

        [Fact]
        public async Task GetExchangeRateResponse_WithZeroRate_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = CreateValidRequest();
            var expectedRate = 0m;
            var xmlResponse = new XElement("Response",
                new XElement("Result", expectedRate.ToString())
            ).ToString();

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessResponse(xmlResponse, "application/xml"));

            // Act
            var result = await _provider.GetExchangeRateResponse(request);

            // Assert
            AssertSuccessResponse(result, "Api2");
            result.Rate.Should().Be(expectedRate);
        }

        private HttpResponseMessage CreateSuccessResponse(string content, string mediaType)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, mediaType)
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
} 