using System;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class FinNumberYearHelper
    {
        private IDateTimeProvider _dateTimeProvider;

        private DateTime AddOneYear
        {
            get { return DateTimeProvider.Today.AddYears(1); }
        }

        public int FinYear
        {
            get
            {
                return GetYear(AddOneYear);
            }
        }

        public int GetYear(DateTime currentDate)
        {
            return IsInPreviousFinancialYear(currentDate) ? currentDate.Year - 1 :
            currentDate.Year;
        }

        private bool IsInPreviousFinancialYear(DateTime currentDate)
        {
            return currentDate.Month < 3;
        }

        public IDateTimeProvider DateTimeProvider
        {
            get { return _dateTimeProvider ?? (_dateTimeProvider = new DefaultDateTimeProvider()); }
            set
            {
                if (_dateTimeProvider != null) throw new InvalidOperationException("DateTimeProvider is already set");
                _dateTimeProvider = value;
            }
        }
    }
}