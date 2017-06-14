namespace RandomBuilderPerformanceTest.Fortel
{
    public static class AddressExtensions
    {
        public static string AsFormattedAddress(this Address address, string separator = "\n")
        {
            if (address == null) return "";
            var formattedAddress = "";
            formattedAddress += string.IsNullOrEmpty(address.UnitNumber) ? "" : address.UnitNumber + separator;
            formattedAddress += string.IsNullOrEmpty(address.Complex) ? "" : address.Complex + separator;
            formattedAddress += string.IsNullOrEmpty(address.Number) ? "" : address.Number + separator;
            formattedAddress += string.IsNullOrEmpty(address.Name) ? "" : address.Name + separator;
            formattedAddress += string.IsNullOrEmpty(address.Suburb) ? "" : address.Suburb + separator;
            formattedAddress += string.IsNullOrEmpty(address.City) ? "" : address.City + separator;
            return formattedAddress.TrimEnd(separator.ToCharArray());
        }
    }
}