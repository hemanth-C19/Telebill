using System;
using System.Collections.Generic;

namespace Telebill.Dto.Posting;

public class AdjustmentEntryDto
{
    public string Group { get; set; } = string.Empty; // CO | PR | OA | PI
    public string Carc { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CreatePaymentPostRequestDto
{
    public int ClaimID { get; set; }
    public int? ClaimLineID { get; set; }
    public decimal AmountPaid { get; set; }
    public List<AdjustmentEntryDto> Adjustments { get; set; } = new();
}

public class VoidPaymentPostRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class PaymentPostDto
{
    public int PaymentID { get; set; }
    public int ClaimID { get; set; }
    public int? ClaimLineID { get; set; }
    public int? LineNo { get; set; }
    public string? CptHcpcs { get; set; }
    public decimal ChargeAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public List<AdjustmentEntryDto> Adjustments { get; set; } = new();
    public decimal TotalAdjusted { get; set; }
    public decimal PatientResponsibility { get; set; }
    public DateTime PostedDate { get; set; }
    public int PostedBy { get; set; }
    public string PostedByName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class PostingResultDto
{
    public int ClaimID { get; set; }
    public string PreviousClaimStatus { get; set; } = string.Empty;
    public string NewClaimStatus { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }
    public decimal TotalCharge { get; set; }
    public decimal TotalPatientResponsibility { get; set; }
    public PatientBalanceDto PatientBalance { get; set; } = new();
    public bool DenialCreated { get; set; }
    public PaymentPostDto CreatedPost { get; set; } = new();
}

public class ClaimPaymentSummaryDto
{
    public int ClaimID { get; set; }
    public string ClaimStatus { get; set; } = string.Empty;
    public decimal TotalCharge { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalContractualAdjustment { get; set; }
    public decimal TotalPatientResponsibility { get; set; }
    public List<PaymentPostDto> PaymentPosts { get; set; } = new();
}

