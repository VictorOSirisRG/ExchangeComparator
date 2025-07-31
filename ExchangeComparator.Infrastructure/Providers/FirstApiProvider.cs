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
    public class FirstApiProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;

        public FirstApiProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ExchangeRateResponse> GetExchangeRateResponse(ExchangeRateRequest request)
        {
            try
            {
                var body = new
                {
                    from = request.SourceCurrency,
                    to = request.TargetCurrency,
                    value = request.Amount
                };

                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api1/rate", content);

                if (!response.IsSuccessStatusCode)
                    return ExchangeRateResponse.Failure("Api1", "Non-success status code");

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<Api1Response>(json);

                if (data == null || data.Rate <= 0)
                    return ExchangeRateResponse.Failure("Api1", "Invalid or missing rate");

                return new ExchangeRateResponse("Api1", data.Rate);
            }
            catch (Exception ex)
            {
                return ExchangeRateResponse.Failure("Api1", ex.Message);
            }
        }

        private class Api1Response
        {
            [JsonPropertyName("rate")]
            public decimal Rate { get; set; }
        }
    }
}
