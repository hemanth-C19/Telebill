using System;
using System.Collections.Generic;

namespace Telebill.Dto.AR;

// Full detail view of a single denial + parent claim
public class DenialDetailDto
{
    public int DenialId { get; set; }
    public string? DenialStatus { get; set; }
    public string? ReasonCode { get; set; }
    public string? RemarkCode { get; set; }
    public DateOnly DenialDate { get; set; }
    public decimal AmountDenied { get; set; }

    public ClaimSummaryForArDto? Claim { get; set; }
    public List<PaymentPostSummaryDto> PaymentHistory { get; set; } = new();
    public List<SubmissionHistoryDto> SubmissionHistory { get; set; } = new();
    public List<AttachmentSummaryDto> Attachments { get; set; } = new();
}

public class ClaimSummaryForArDto
{
    public int ClaimId { get; set; }
    public string? ClaimStatus { get; set; }
    public decimal TotalCharge { get; set; }
    public string? PatientName { get; set; }
    public string? PayerName { get; set; }
    public string? PlanName { get; set; }
    public DateTime EncounterDateTime { get; set; }
    public string? Pos { get; set; }
    public List<ClaimLineSummaryDto> Lines { get; set; } = new();
}

public class ClaimLineSummaryDto
{
    public int ClaimLineId { get; set; }
    public int LineNo { get; set; }
    public string? CptHcpcs { get; set; }
    public string? Modifiers { get; set; }
    public int? Units { get; set; }
    public decimal? ChargeAmount { get; set; }
    public string? DxPointers { get; set; }
}

public class PaymentPostSummaryDto
{
    public int PaymentPostId { get; set; }
    public int? ClaimLineId { get; set; }
    public decimal AmountPaid { get; set; }
    public string? AdjustmentJson { get; set; }
    public DateTime PostedDate { get; set; }
    public string? Status { get; set; }
}

public class SubmissionHistoryDto
{
    public int SubmissionRefId { get; set; }
    public DateTime SubmitDate { get; set; }
    public string? AckType { get; set; }
    public string? AckStatus { get; set; }
    public DateTime? AckDate { get; set; }
    public string? CorrelationId { get; set; }
}

public class AttachmentSummaryDto
{
    public int AttachId { get; set; }
    public string? FileType { get; set; }
    public string? FileUri { get; set; }
    public string? Notes { get; set; }
    public DateTime UploadedDate { get; set; }
}

// Update denial status (Open → Appealed / Resolved / WrittenOff)
public class UpdateDenialStatusDto
{
    public string NewStatus { get; set; } = string.Empty;
    // Allowed values: "Appealed" | "Resolved" | "WrittenOff"
    public string? Notes { get; set; }
    // Optional notes for the status change
}

// Upload an appeal document for a denial
public class UploadAppealDocumentDto
{
    public int DenialId { get; set; }
    public string FileType { get; set; } = string.Empty;  // "PDF" | "Doc" etc.
    public string FileUri { get; set; } = string.Empty;   // object storage path
    public string? Notes { get; set; }
    public int UploadedBy { get; set; }                   // FK → Users.UserId
}

// Reset claim for resubmission — core AR action
public class ResetClaimForResubmissionDto
{
    public int DenialId { get; set; }
    // The DenialId implies the ClaimId. Service resolves it.
    public string? Notes { get; set; }
}

public class ResetClaimResponseDto
{
    public int ClaimId { get; set; }
    public string? ClaimStatus { get; set; }   // will be "Draft"
    public int DenialId { get; set; }
    public string? DenialStatus { get; set; }  // will be "Resolved"
}
