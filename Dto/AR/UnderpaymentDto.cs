using System;
using System.Collections.Generic;

namespace Telebill.Dto.AR;

// List item for underpayment worklist
public class UnderpaymentItemDto
{
    public int ClaimId { get; set; }
    public string? PatientName { get; set; }
    public string? PayerName { get; set; }
    public string? PlanName { get; set; }
    public DateTime EncounterDateTime { get; set; }
    public decimal TotalCharge { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalAllowed { get; set; }    // sum of FeeSchedule.AllowedAmount per line
    public decimal UnderpaymentAmount { get; set; }  // TotalAllowed - TotalPaid (if positive)
    public string? ClaimStatus { get; set; }
    public List<LineUnderpaymentDto> Lines { get; set; } = new();
}

public class LineUnderpaymentDto
{
    public int ClaimLineId { get; set; }
    public int LineNo { get; set; }
    public string? CptHcpcs { get; set; }
    public string? Modifiers { get; set; }
    public decimal ChargeAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal? AllowedAmount { get; set; }  // from FeeSchedule, nullable if not found
    public decimal? Variance { get; set; }        // AllowedAmount - AmountPaid
    public bool IsPotentialUnderpayment { get; set; }  // true if Variance > 0
}

// Flag a PartiallyPaid claim as an underpayment dispute
// This creates a new Denial record with ReasonCode = "UNDERPAYMENT"
public class FlagUnderpaymentDto
{
    public int ClaimId { get; set; }
    public string? Notes { get; set; }
}
