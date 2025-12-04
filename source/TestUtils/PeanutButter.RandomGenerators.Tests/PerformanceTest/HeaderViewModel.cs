using System;
using System.ComponentModel.DataAnnotations;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class HeaderViewModel : ViewModelBase
{
    public FinNumberViewModel FinNumberViewModel { get; set; }

    [Display(Name = "REF: ")]
    public string ReferenceNumber { get; set; }

    [Display(Name = "PO number: ")]
    public string PoNumber { get; set; }

    [Display(Name = "Contract start: ")]
    public DateTime? ContractStart { get; set; }

    [Display(Name = "Contract end: ")]
    public DateTime? ContractEnd { get; set; }

    public string OrderId { get; set; }
    public decimal PaymentAmount { get; set; }
    public string OrderCreatorFullname { get; set; }
    public bool OrderIsClosed { get; set; }

    public SecurityViewModel Security { get; set; }
}