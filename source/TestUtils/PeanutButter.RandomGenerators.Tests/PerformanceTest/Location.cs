using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class Location : EntityBase
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string ExchangeAreaCode { get; set; }
        public string ExchangeName { get; set; }
        public string AreaLocation { get; set; }
        public string Country { get; set; }
        public LocationType LocationType { get; set; }
        public Region Region { get; set; }
        public ExchangeArea ExchangeArea { get; set; }
        public string RegionName { get; set; }
        public string LocationName { get; set; }
        public List<Address> Addresses { get; set; }

        public Address GetAddress(AddressType addressType)
        {
            if (Addresses == null) return new Address();
            return Addresses.FirstOrDefault(x => x.AddressType == addressType);
        }
    }
}