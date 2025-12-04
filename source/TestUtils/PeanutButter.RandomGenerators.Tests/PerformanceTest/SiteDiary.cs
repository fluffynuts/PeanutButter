using System;
using System.Collections.Generic;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class SiteDiary : EntityBase
{
    public string Description { get; set; }
    public List<Attachment> Attachments { get; set; }
    public DateTime SiteDiaryDate { get; set; }
}