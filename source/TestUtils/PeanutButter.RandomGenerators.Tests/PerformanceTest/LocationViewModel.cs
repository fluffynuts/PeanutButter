using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class LocationViewModel : ViewModelBase
{
    [Required]
    [DisplayFormat(DataFormatString = "{0:N4}")]
    public decimal Latitude { get; set; }

    [Required]
    [DisplayFormat(DataFormatString = "{0:N4}")]
    public decimal Longitude { get; set; }

    public AddressViewModel PhysicalAddressViewModel { get; set; }

    [Display(Name = "Exchange area code")]
    public string ExchangeAreaCode { get; set; }


    public SelectList ExchangeNameSelectList { get; set; }

    [Display(Name = "Exchange name")]
    [Required(ErrorMessage = "Exchange name is a required field")]
    public string ExchangeNameId { get; set; }

    [Display(Name = "Area location")]
    public string AreaLocation { get; set; }

    public string Country { get; set; }

    [Display(Name = "Location type")]
    public LocationType LocationType { get; set; }

    public SelectList RegionSelectList { get; set; }

    [Display(Name = "Region")]
    public string RegionId { get; set; }

    public Region Region { get; set; }

    public string RegionName { get; set; }

    [Display(Name = "Location Name")]
    public string LocationName { get; set; }
}