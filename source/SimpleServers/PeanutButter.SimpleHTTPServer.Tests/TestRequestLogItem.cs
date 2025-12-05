using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.SimpleHTTPServer.Tests;

[TestFixture]
public class TestRequestLogItem
{
    [Test]
    public void Construct_ShouldCopyParametersToProperties()
    {
        //---------------Set up test pack-------------------
        var path = GetRandomString();
        var code = GetRandom<HttpStatusCode>();
        var message = GetRandomString();
        var method = GetRandomString();
        var headers = new Dictionary<string, string>();

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var sut = new RequestLogItem(path, code, method, message, headers);

        //---------------Test Result -----------------------
        Expect(sut.Path)
            .To.Equal(path);
        Expect(sut.StatusCode)
            .To.Equal(code);
        Expect(sut.Method)
            .To.Equal(method);
        Expect(sut.Message)
            .To.Equal(message);
    }

    [Test]
    public void Construct_GivenNullMessage_ShouldSetMessageFromStatusCodeString()
    {
        //---------------Set up test pack-------------------
        var path = GetRandomString();
        var code = GetRandom<HttpStatusCode>();
        var method = GetRandomString();

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var sut = new RequestLogItem(path, code, method, null, null);

        //---------------Test Result -----------------------
        Expect(sut.Path)
            .To.Equal(path);
        Expect(sut.StatusCode)
            .To.Equal(code);
        Expect(sut.Method)
            .To.Equal(method);
        Expect(sut.Message)
            .To.Equal(code.ToString());
        Expect(sut.Headers)
            .Not.To.Be.Null();
    }

}