using System;
using System.Collections.Generic;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class Order : EntityBase
{
    public FinNumber FinNumber { get; set; }
    public DateTime? OrderDate { get; set; }
    public string PoNumber { get; set; }
    public string SapNumber { get; set; }
    public string ReferenceNumber { get; set; }
    public string WorkDescription { get; set; }
    public string NetworkNumber { get; set; }
    public DateTime? ContractStart { get; set; }
    public DateTime? ContractEnd { get; set; }
    public bool IsClosed { get; set; }
    public Location SiteLocation { get; set; }
    public Location BaseLocation { get; set; }
    public Company Customer { get; set; }
    public string ContractNumber { get; set; }
    public Contact CustomerRep { get; set; }
    public Contact SeniorCustomerRep { get; set; }
    public Contact Manager { get; set; }
    public Contact SeniorManager { get; set; }
    public Contact Executive { get; set; }
    public Domain DomainCategory { get; set; }
    public string VendorCode { get; set; }

    public WorkType WorkType { get; set; }
    public ReceiptMethod ReceiptMethod { get; set; }
    public List<ServiceItem> ServiceItems { get; set; }
    public List<SiteDiary> SiteDiaries { get; set; }
    public string Domain { get; set; }
    public List<Job> Jobs { get; set; }
    public List<Recon> Recons { get; set; }
    public List<Invoice> Invoices { get; set; }
    public List<InvoicePaymentRecon> InvoicePaymentRecons { get; set; }
    public DateTime? RateSelectionDate { get; set; }
    public DateTime EffectiveDate => RateSelectionDate ?? ContractStart ?? DateCreated;
}