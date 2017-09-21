using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class ContactViewModel : ViewModelBase
    {
        [Required(ErrorMessage = @"Title is required")]
        public string Title { get; set; }

        [Display(Name = @"First Names")]
        [Required(ErrorMessage = @"First Names is required")]
        public string FirstNames { get; set; }

        [Required(ErrorMessage = @"Initials is required")]
        public string Initials { get; set; }

        [Required(ErrorMessage = @"Surname is required")]
        public string Surname { get; set; }

        [Display(Name = @"ID Number")]
        public string IdNumber { get; set; }

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

        //Contact Number Fields
        public List<ContactNumberViewModel> ContactNumbers { get; set; }

        public ContactNumberType ContactNumberType { get; set; }
        public string Number { get; set; }

        public string Email { get; set; }

        public Designation Designation { get; set; }
        public string CompanyId { get; set; }
        public bool CanShowDetails { get; set; }

        public SelectList TeamSelectList { get; set; }

        public string TeamId { get; set; }
        public List<TeamViewModel> Teams { get; set; }
        public List<string> TeamIds { get; set; }

        [Display(Name = @"Personnel Code")]
        public string PersonnelCode { get; set; }

        public string FullName => $"{FirstNames ?? Initials} {Surname}".Trim();

        public Attachment IdCopy { get; set; }
        public Attachment Signature { get; set; }
        public Attachment BankDetailsProof { get; set; }
        public Attachment AttachmentFile { get; set; }

        public bool IsWingbat { get; set; }

        public WageViewModel WageViewModel { get; set; }

        public BankingDetailsViewModel BankingDetailsViewModel { get; set; }

        public List<BankingDetailsViewModel> BankingDetails { get; set; }
    }
}