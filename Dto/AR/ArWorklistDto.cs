using System;
using System.Collections.Generic;

namespace Telebill.Dto.AR;

// Summary card shown in the AR worklist for each open denial
public class ArWorklistItemDto
{
    public int DenialId { get; set; }
    public int ClaimId { get; set; }
    public string? PatientName { get; set; }
    public string? PayerName { get; set; }
    public string? PlanName { get; set; }
    public DateTime EncounterDateTime { get; set; }
    public decimal TotalCharge { get; set; }
    public decimal AmountDenied { get; set; }
    public string? ReasonCode { get; set; }   // CARC code
    public string? RemarkCode { get; set; }   // REMARK code
    public string? DenialStatus { get; set; } // Open / Appealed / Resolved / WrittenOff
    public DateOnly DenialDate { get; set; }
    public int DaysSinceDenial { get; set; }  // computed: today - DenialDate
    public string? AgingBucket { get; set; }  // "0-30" | "31-60" | "61-90" | "90+"
    public string? ClaimStatus { get; set; }  // from Claim.ClaimStatus
    public int SubmissionCount { get; set; }  // how many times this claim was batched
}

public class ArWorklistFilterParams
{
    public string? DenialStatus { get; set; }   // null = all, "Open", "Appealed" etc.
    public string? ReasonCode { get; set; }
    public int? PayerId { get; set; }
    public string? AgingBucket { get; set; }
    public DateTime? DenialDateFrom { get; set; }
    public DateTime? DenialDateTo { get; set; }
}
