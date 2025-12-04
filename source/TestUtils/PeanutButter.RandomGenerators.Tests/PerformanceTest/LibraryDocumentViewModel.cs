using System;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class LibraryDocumentViewModel
{
    public string FileId { get; set; }
    public DateTime? DateGenerated { get; set; }
    public string GeneratedBy { get; set; }
    public string DocumentType { get; set; }
    public LibraryDocumentDetailsViewModel Details { get; set; }
}