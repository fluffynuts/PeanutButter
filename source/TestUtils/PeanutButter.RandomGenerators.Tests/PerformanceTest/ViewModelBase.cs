using System;
using System.ComponentModel.DataAnnotations;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public class ViewModelBase
{
    public string Id { get; set; }

    [Display(Name = "Date created")]
    public DateTime DateCreated { get; set; }

    [Display(Name = "Created by")]
    public string CreatedUsername { get; set; }

    [Display(Name = "Date last modified")]
    public DateTime DateLastModified { get; set; }

    [Display(Name = "Last modified by")]
    public string LastModifiedUsername { get; set; }

    public Status Status { get; set; }

    public bool CanEdit => Status == Status.Open;
}