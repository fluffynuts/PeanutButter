using System;
using NUnit.Framework;

namespace PeanutButter.EasyArgs.Tests;

[SetUpFixture]
public class GlobalSetup
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Environment.SetEnvironmentVariable("OVERRIDE_COLUMNS", "123");
    }
}