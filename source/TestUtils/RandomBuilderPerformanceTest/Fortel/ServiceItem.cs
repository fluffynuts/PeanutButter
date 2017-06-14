using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class ServiceItem : EntityBase
    {
        public string ServiceNumber { get; set; }
        public string ContractItemNumber { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string BoqDefinition { get; set; }
        public decimal EstimatedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public string Domain { get; set; }
        public List<Rate> Rates { get; set; }
        public string ContractNumber { get; set; }
        public string WorkType { get; set; }

        public Rate LatestRateFor(DateTime? date)
        {
            var rates = date == null ? Rates : Rates?.Where(x => x.EffectiveDate.Date <= date);
           return rates?.OrderByDescending(x => x.EffectiveDate).FirstOrDefault() ?? new Rate(); 
        }

        public decimal ActualAmountFor(DateTime? date)
        {
            var rate = LatestRateFor(date);
            return Math.Round(ActualQuantity * rate.Value,2);
        }
        public decimal EstimatedAmountFor(DateTime? date)
        {
            var rate = LatestRateFor(date);
            return Math.Round(rate.Value * EstimatedQuantity, 2);
        }
    }
}