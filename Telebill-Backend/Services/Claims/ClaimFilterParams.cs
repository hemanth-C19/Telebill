using System;

namespace Services;

public class ClaimFilterParams
{
    public string? ClaimStatus { get; set; }
    public int? PatientID { get; set; }
    public int? PlanID { get; set; }
    public int? ProviderID { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? HasScrubErrors { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = "CreatedDate";
    public string SortOrder { get; set; } = "desc";
}

