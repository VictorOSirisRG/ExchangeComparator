using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeComparator.Domain.Models
{
    public record ExchangeRateResponse(string ProviderName,decimal Rate, bool IsSuccess = true,string? ErrorMessage=null)
    {
        public static ExchangeRateResponse Failure(string provider, string? error=null)=> 
             new ExchangeRateResponse(provider, 0m, false, error);
    }

    
}
