using System;
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class ExchangeArea
{
    public string Id { get; set; }

    public string ExchangeCode { get; set; }
    public string SiteLocation { get; set; }
    public string Turf { get; set; }
    public string AreaLocation { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
}

public class BsonRepresentationAttribute : Attribute
{
    public BsonRepresentationAttribute(object objectId)
    {
        throw new NotImplementedException();
    }
}