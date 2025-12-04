using System.Collections.Generic;
using System.Web.Mvc;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class ReportScheduleViewModel : ViewModelBase
{
    public ReportType ReportType { get; set; }
    public SelectList Schedules { get; set; }
    public SelectList ContactList { get; set; }
    public string Contact { get; set; }
    public string ReportTypeString { get; set; }
    public string ReportMethodName { get; set; }
    public List<ContactViewModel> Contacts { get; set; }
    public List<ReportScheduleParameterViewModel> ReportScheduleParameters { get; set; }
    public ScheduleViewModel Schedule { get; set; }
}