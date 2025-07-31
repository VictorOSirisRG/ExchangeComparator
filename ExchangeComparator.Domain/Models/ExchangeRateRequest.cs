using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeComparator.Domain.Models
{
    public record ExchangeRateRequest
    (
       string SourceCurrency,
       string TargetCurrency, 
       decimal Amount
    );

}