using System;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class FinNumberYearHelper
{
    private IDateTimeProvider _dateTimeProvider;

    private DateTime AddOneYear => DateTimeProvider.Today.AddYears(1);

    public int FinYear => GetYear(AddOneYear);

    public IDateTimeProvider DateTimeProvider
    {
        get => _dateTimeProvider ?? (_dateTimeProvider = new DefaultDateTimeProvider());
        set
        {
            if (_dateTimeProvider != null) throw new InvalidOperationException("DateTimeProvider is already set");
            _dateTimeProvider = value;
        }
    }

    public int GetYear(DateTime currentDate)
    {
        return IsInPreviousFinancialYear(currentDate) ? currentDate.Year - 1 : currentDate.Year;
    }

    private bool IsInPreviousFinancialYear(DateTime currentDate)
    {
        return currentDate.Month < 3;
    }
}