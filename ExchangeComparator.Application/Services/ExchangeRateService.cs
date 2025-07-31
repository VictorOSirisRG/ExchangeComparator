using ExchangeComparator.Domain.Interfaces;
using ExchangeComparator.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeComparator.Application.Services
{
    public class ExchangeRateService
    {
        private readonly IEnumerable<IExchangeRateProvider> _providers;
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(IEnumerable<IExchangeRateProvider> providers, ILogger<ExchangeRateService> logger)
        {
            _providers = providers;
            _logger = logger;
        }

        public async Task<ExchangeRateResponse> GetBestRateAsync(ExchangeRateRequest request)
        {
            if (request == null)
            {
                _logger.LogWarning("Null request received");
                return ExchangeRateResponse.Failure("AllProviders", "Request cannot be null.");
            }

            _logger.LogInformation("Starting exchange rate comparison for {SourceCurrency} to {TargetCurrency}, amount: {Amount}", 
                request.SourceCurrency, request.TargetCurrency, request.Amount);

            var stopwatch = Stopwatch.StartNew();
            
            var tasks = _providers.Select(async p =>
            {
                var providerName = p.GetType().Name;
                try
                {
                    _logger.LogDebug("Querying provider: {ProviderName}", providerName);
                    var result = await p.GetExchangeRateResponse(request);
                    
                    if (result.IsSuccess)
                    {
                        _logger.LogDebug("Provider {ProviderName} returned rate: {Rate}", providerName, result.Rate);
                    }
                    else
                    {
                        _logger.LogWarning("Provider {ProviderName} failed: {ErrorMessage}", providerName, result.ErrorMessage);
                    }
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Provider {ProviderName} threw an exception", providerName);
                    return ExchangeRateResponse.Failure(providerName, "Provider exception");
                }
            });

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            var successfulResponses = results.Where(r => r.IsSuccess).ToList();

            if (!successfulResponses.Any())
            {
                _logger.LogWarning("All providers failed to return valid rates. Total time: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return ExchangeRateResponse.Failure("AllProviders", "All providers failed to return a valid rate.");
            }

            var best = successfulResponses.OrderByDescending(r => r.Rate).First();

            _logger.LogInformation("Best rate found: {Rate} from {ProviderName}. Total time: {ElapsedMs}ms", 
                best.Rate, best.ProviderName, stopwatch.ElapsedMilliseconds);

            return best;
        }
    }
}
