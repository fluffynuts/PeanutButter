using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public enum ContactNumberType
{
    [Display(Name = "Landline Number")] [Description("Landline Number")] LandlineNumber,
    [Display(Name = "Fax Number")] [Description("Fax Number")] FaxNumber,
    [Display(Name = "Cellphone Number")] [Description("Cellphone Number")] CellphoneNumber
}