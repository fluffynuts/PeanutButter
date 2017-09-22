namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public static class FinNumberExtensions
    {
        public static string AsFinNumberString(this FinNumber finNumber)
        {
            if (finNumber == null) return string.Empty;
            var yearDifference = finNumber.Year > 2000 ? finNumber.Year - 2000 : 0;
            return $"FIN-{yearDifference}-{finNumber.SequenceNumber:D4}";
        }

        public static string AsInvoiceFinNumberString(this FinNumber finNumber)
        {
            return finNumber == null ? "" : ComputeFinNumber("I", finNumber);
        }

        public static string AsJobFinNumberString(this FinNumber finNumber)
        {
            return finNumber == null ? "" : ComputeFinNumber("J", finNumber);
        }

        public static string AsReconFinNumberString(this FinNumber finNumber)
        {
            return finNumber == null ? "" : ComputeFinNumber("R", finNumber);
        }

        private static string ComputeFinNumber(string typeIdentifier, FinNumber finNumber)
        {
            return $"{typeIdentifier}{finNumber.SequenceNumber:D4}";
        }

        public static FinNumber AsInvoiceNumber(this string strFinNumber)
        {
            FinNumber finNumber;
            if (strFinNumber.Contains("-"))
                return null;
            var identifier = strFinNumber.Substring(0, 1);
            var sequenceNumber = strFinNumber.Substring(1, strFinNumber.Length - 1);
            finNumber = new FinNumber
            {
                TypeIdentifier = identifier,
                Year = 0,
                SequenceNumber = int.Parse(sequenceNumber)
            };
            return finNumber;
        }

        public static FinNumber AsFinNumber(this string strFinNumber)
        {
            FinNumber finNumber = null;
            if (strFinNumber.Contains("-"))
            {
                var splitFinNumber = strFinNumber.Trim().Split('-');
                var newYear = 0;
                if (int.Parse(splitFinNumber[1]) != 0)
                    newYear = int.Parse("20" + splitFinNumber[1]);
                finNumber = new FinNumber
                {
                    TypeIdentifier = null,
                    Year = newYear,
                    SequenceNumber = int.Parse(splitFinNumber[2])
                };
            }
            return finNumber;
        }
    }
}