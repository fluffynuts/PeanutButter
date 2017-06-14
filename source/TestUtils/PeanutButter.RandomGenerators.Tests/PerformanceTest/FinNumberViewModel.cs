using System;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class FinNumberViewModel
    {
        private const string Prefix = "FIN";
        private const string Separator = "-";
        private FinNumberYearHelper _finNumberYearHelper;
        public int Year { get; set; }
        public int SequenceNumber { get; set; }
        public string TypeIdentifier { get; set; }

        public string DefaultFinNumber
        {
            get
            {
                var yearDifference = 0;
                if (FinNumberYearHelper.FinYear > 2000)
                    yearDifference = FinNumberYearHelper.FinYear - 2000;
                return $"{Prefix}{Separator}{yearDifference}{Separator}****";
            }
        }

        public string FormattedNumber
        {
            get
            {
                var yearDifference = 0;
                if (Year > 2000)
                    yearDifference = Year - 2000;
                return $"{Prefix}{Separator}{yearDifference}{Separator}{SequenceNumber:D4}";
            }
        }

        public string FormattedReconNumber => $"R{SequenceNumber:D4}";

        public string FormattedInvoiceNumber => $"I{SequenceNumber:D4}";

        public string FormattedJobNumber => $"J{SequenceNumber:D4}";

        public FinNumberYearHelper FinNumberYearHelper
        {
            get => _finNumberYearHelper ?? (_finNumberYearHelper = new FinNumberYearHelper());
            set
            {
                if (_finNumberYearHelper != null)
                    throw new InvalidOperationException("FinNumberYearHelper is already set");
                _finNumberYearHelper = value;
            }
        }
    }
}