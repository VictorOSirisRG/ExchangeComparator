using ExchangeComparator.Domain.Models;
using FluentAssertions;
using Moq;
using System.Net;
using System.Text;

namespace ExchangeComparator.Tests
{
    public abstract class TestBase
    {
        protected ExchangeRateRequest CreateValidRequest()
        {
            return new ExchangeRateRequest("USD", "EUR", 100.0m);
        }

        protected ExchangeRateRequest CreateInvalidRequest()
        {
            return new ExchangeRateRequest("", "", -1.0m);
        }

        protected HttpResponseMessage CreateSuccessResponse(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }

        protected HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("Error", Encoding.UTF8, "application/json")
            };
        }

        protected void AssertSuccessResponse(ExchangeRateResponse response, string expectedProviderName)
        {
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.ProviderName.Should().Be(expectedProviderName);
            response.Rate.Should().BeGreaterThanOrEqualTo(0);
            response.ErrorMessage.Should().BeNullOrEmpty();
        }

        protected void AssertSuccessResponseWithPositiveRate(ExchangeRateResponse response, string expectedProviderName)
        {
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.ProviderName.Should().Be(expectedProviderName);
            response.Rate.Should().BeGreaterThan(0);
            response.ErrorMessage.Should().BeNullOrEmpty();
        }

        protected void AssertFailureResponse(ExchangeRateResponse response, string expectedProviderName)
        {
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeFalse();
            response.ProviderName.Should().Be(expectedProviderName);
            response.Rate.Should().Be(0);
            response.ErrorMessage.Should().NotBeNullOrEmpty();
        }
    }
} 