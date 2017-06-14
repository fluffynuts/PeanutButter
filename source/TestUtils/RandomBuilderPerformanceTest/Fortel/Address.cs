namespace RandomBuilderPerformanceTest.Fortel
{
    public class Address : EntityBase
    {
        public string UnitNumber { get; set; }
        public string Complex { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public AddressType AddressType { get; set; }
    }
}