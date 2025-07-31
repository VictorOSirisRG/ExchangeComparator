using ExchangeComparator.Domain.Interfaces;
using ExchangeComparator.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExchangeComparator.Infrastructure.Providers
{
    public class SecondApiProvider : IExchangeRateProvider
    {

        private readonly HttpClient _httpClient;

        public SecondApiProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async  Task<ExchangeRateResponse> GetExchangeRateResponse(ExchangeRateRequest request)
        {
            try
            {
                var xmlRequest = new XElement("XML",
                    new XElement("From", request.SourceCurrency),
                    new XElement("To", request.TargetCurrency),
                    new XElement("Amount", request.Amount)
                );

                var content = new StringContent(xmlRequest.ToString(), Encoding.UTF8, "application/xml");
                var response = await _httpClient.PostAsync("/api2/rate", content);

                if (!response.IsSuccessStatusCode)
                    return ExchangeRateResponse.Failure("Api2", "Non-success status code");

                var responseString = await response.Content.ReadAsStringAsync();
                var xml = XElement.Parse(responseString);
                var resultValue = xml.Element("Result")?.Value;

                if (decimal.TryParse(resultValue, out var rate))
                    return new ExchangeRateResponse("Api2", rate);

                return ExchangeRateResponse.Failure("Api2", "Invalid XML or missing result");
            }
            catch (Exception ex)
            {
                return ExchangeRateResponse.Failure("Api2", ex.Message);
            }
        }
    }
}
