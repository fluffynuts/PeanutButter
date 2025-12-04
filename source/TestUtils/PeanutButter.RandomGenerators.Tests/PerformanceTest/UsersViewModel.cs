using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class UsersViewModel : ViewModelBase
{
    [Required]
    [Display(Name = "User Role")]
    public UserRole UserRole { get; set; }

    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; }

    [Display(Name = "Default Password")]
    public string DefaultPassword => "12345";

    public ContactViewModel ContactViewModel { get; set; }

    [Required]
    [Display(Name = "Associated Contact")]
    public string ContactId { get; set; }

    public SelectList ContactSelectList { get; set; }
    public string FirstNames { get; set; }
    public string Surname { get; set; }
    public string FullName => $"{FirstNames} {Surname}";
}