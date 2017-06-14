using System.IO;

namespace RandomBuilderPerformanceTest.Fortel
{
    public interface IUploadedFile
    {
        bool HasFile { get; }
        string FileName { get; }
        Stream InputStream { get; }
    }
}