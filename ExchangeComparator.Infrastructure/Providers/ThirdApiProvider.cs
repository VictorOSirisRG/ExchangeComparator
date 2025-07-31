using ExchangeComparator.Domain.Interfaces;
using ExchangeComparator.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ExchangeComparator.Infrastructure.Providers
{
    public class ThirdApiProvider : IExchangeRateProvider
    {

        private readonly HttpClient _httpClient;

        public ThirdApiProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ExchangeRateResponse> GetExchangeRateResponse(ExchangeRateRequest request)
        {
            try
            {
                var body = new
                {
                    exchange = new
                    {
                        sourceCurrency = request.SourceCurrency,
                        targetCurrency = request.TargetCurrency,
                        quantity = request.Amount
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/ThirdApiProvider/rate", content);

                if (!response.IsSuccessStatusCode)
                    return ExchangeRateResponse.Failure("ThirdApiProvider", "Non-success status code");

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ThirdApiProviderResponse>(json);

                if (data?.Data?.Total > 0 && data.StatusCode >= 100)
                    return new ExchangeRateResponse("ThirdApiProvider", data.Data.Total);

                return ExchangeRateResponse.Failure("ThirdApiProvider", "Invalid data or missing total");
            }
            catch (Exception ex)
            {
                return ExchangeRateResponse.Failure("ThirdApiProvider", ex.Message);
            }
        }

        private class ThirdApiProviderResponse
        {
            [JsonPropertyName("statusCode")]
            public int StatusCode { get; set; }
            public string? Message { get; set; }
            [JsonPropertyName("data")]
            public ThirdApiProviderData Data { get; set; } = new();
        }

        private class ThirdApiProviderData
        {
            [JsonPropertyName("total")]
            public decimal Total { get; set; }
        }
    }
}
