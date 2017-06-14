using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class Company : EntityBase
    {
        public string Code { get; set; }
        public string TradingName { get; set; }
        public string RegisteredName { get; set; }
        public string RegistrationNumber { get; set; }
        public string VatNumber { get; set; }
        public string EmailAddress { get; set; }
        public List<CompanyBankingDetails> CompanyBankingDetails { get; set; }
        public List<ContractDetails> ContractDetails { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsSupplier { get; set; }

        public List<Address> Addresses { get; set; }
        public List<Contact> Contacts { get; set; }

        public List<ContactNumber> ContactNumbers { get; set; }
        public List<ServiceItem> ServiceItems { get; set; }

        public bool IsTelkom => TradingName?.ToLower() == "telkom sa soc ltd";
        public bool IsFortel => !IsCustomer && !IsSupplier;

        public Address GetAddress(AddressType addressType)
        {
            if (Addresses == null) return new Address();
            return Addresses.FirstOrDefault(x => x.AddressType == addressType);
        }
    }
}