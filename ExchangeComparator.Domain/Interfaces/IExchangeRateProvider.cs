using ExchangeComparator.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeComparator.Domain.Interfaces
{
    public  interface IExchangeRateProvider
    {
        Task<ExchangeRateResponse> GetExchangeRateResponse(ExchangeRateRequest request);
    }
}
