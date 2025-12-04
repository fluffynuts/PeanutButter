using System.IO;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public interface IUploadedFile
{
    bool HasFile { get; }
    string FileName { get; }
    Stream InputStream { get; }
}