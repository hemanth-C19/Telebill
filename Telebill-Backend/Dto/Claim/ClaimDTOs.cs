using System;
using System.Collections.Generic;

namespace Telebill.Dto;

// Request DTOs

public class BuildClaimRequestDto
{
    public int EncounterID { get; set; }
}

public class UpdateClaimStatusRequestDto
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public int? BatchID { get; set; }
}

public class ResolveIssueRequestDto
{
    public string? Resolution { get; set; }
}

// Response / list DTOs

public class ClaimSummaryDto
{
    public int ClaimID { get; set; }
    public int EncounterID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public decimal TotalCharge { get; set; }
    public string ClaimStatus { get; set; } = string.Empty;
    public int OpenScrubErrors { get; set; }
    public int OpenScrubWarnings { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ClaimDetailDto
{
    public int ClaimID { get; set; }
    public int EncounterID { get; set; }
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int PlanID { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string SubscriberRel { get; set; } = string.Empty;
    public decimal TotalCharge { get; set; }
    public string ClaimStatus { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public List<ClaimLineDto> ClaimLines { get; set; } = new();
    public List<ScrubIssueDto> ScrubIssues { get; set; } = new();
    public X12RefDto? X12Ref { get; set; }
}

public class ClaimLineDto
{
    public int ClaimLineID { get; set; }
    public int LineNo { get; set; }
    public string CPT_HCPCS { get; set; } = string.Empty;
    public List<string> Modifiers { get; set; } = new();
    public int Units { get; set; }
    public decimal ChargeAmount { get; set; }
    public decimal? AllowedAmount { get; set; }
    public List<int> DxPointers { get; set; } = new();
    public string POS { get; set; } = string.Empty;
    public string LineStatus { get; set; } = string.Empty;
    public int OpenIssues { get; set; }
}

public class ClaimStatusSummaryDto
{
    public int ClaimID { get; set; }
    public string ClaimStatus { get; set; } = string.Empty;
    public decimal TotalCharge { get; set; }
    public int OpenScrubErrors { get; set; }
    public int OpenScrubWarnings { get; set; }
    public bool HasPriorAuth { get; set; }
    public bool Has837PRef { get; set; }
}

public class ClaimListResponseDto
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<ClaimSummaryDto> Claims { get; set; } = new();
}

public class BuildClaimResponseDto
{
    public int ClaimID { get; set; }
    public int EncounterID { get; set; }
    public string ClaimStatus { get; set; } = string.Empty;
    public decimal TotalCharge { get; set; }
    public List<ClaimLineDto> ClaimLines { get; set; } = new();
    public bool ScrubTriggered { get; set; }
}

public class UpdateClaimStatusResponseDto
{
    public int ClaimID { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

