using System;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class ImajinConfig : EntityBase
    {
        public string ApiUrl { get; set; }
        public string OrganizationId { get; set; }
        public DateTime DateLastSync { get; set; }
        public int ResyncOverlapMinutes { get; set; }


        public string ApiClientId { get; set; }
        public string ApiClientSecret { get; set; }
        public string ApiScope { get; set; }
        public string ApiUsername { get; set; }
        public string ApiPassword { get; set; }
    }
}