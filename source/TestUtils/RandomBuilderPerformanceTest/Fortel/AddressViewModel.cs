namespace RandomBuilderPerformanceTest.Fortel
{
    public class AddressViewModel : ViewModelBase
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

        public string FormattedAddress => $"{Number}{FormattedName()}{FormattedAddressPiece(Suburb)}{FormattedAddressPiece(City)}{FormattedAddressPiece(PostalCode)}";

        private string FormattedName()
        {
            if (string.IsNullOrEmpty(Name)) return string.Empty;
            return (string.IsNullOrEmpty(Number) ? Name : " " + Name);
        }

        private static string FormattedAddressPiece(string piece)
        {
            return string.IsNullOrEmpty(piece) ? string.Empty : ", " + piece;
        }
    }
}