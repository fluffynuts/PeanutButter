using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class CompanyViewModel : ViewModelBase
{
    public string Code { get; set; }

    [Required]
    [Display(Name = "Trading Name")]
    public string TradingName { get; set; }

    [Display(Name = "Registration Number")]
    public string RegistrationNumber { get; set; }

    [Display(Name = "VAT Number")]
    public string VatNumber { get; set; }


    //Contact Number Fields
    public List<ContactNumberViewModel> ContactNumbers { get; set; }

    public List<ContractDetailsViewModel> ContractDetails { get; set; }
    public ContractDetailsViewModel ContractDetail { get; set; }

    public ContactNumberType ContactNumberType { get; set; }

    //Address
    //Address Fields

    public List<AddressViewModel> Address { get; set; }
    public AddressType AddressType { get; set; }
    public string UnitNumber { get; set; }
    public string Complex { get; set; }
    public string Name { get; set; }
    public string Suburb { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string CountryCode { get; set; }
    public string Number { get; set; }


    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; }

    [Display(Name = "Is Customer")]
    public bool IsCustomer { get; set; }

    [Display(Name = "Is Supplier")]
    public bool IsSupplier { get; set; }

    public List<ServiceItemViewModel> ServiceItemViewModels { get; set; }
    public List<ContactViewModel> ContactViewModels { get; set; }
    public bool IsFrobNozzle { get; set; }

    [Required]
    [Display(Name = @"Registered Name")]
    public string RegisteredName { get; set; }

    public CompanyBankingDetailsViewModel CompanyBankingDetailsViewModels { get; set; }
    public List<CompanyBankingDetailsViewModel> CompanyBankingDetails { get; set; }
}