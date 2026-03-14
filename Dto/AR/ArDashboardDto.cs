using System;
using System.Collections.Generic;

namespace Telebill.Dto.AR;

// Top-level AR dashboard summary
public class ArDashboardSummaryDto
{
    public int TotalOpenDenials { get; set; }
    public decimal TotalAmountAtRisk { get; set; }   // sum of AmountDenied on Open denials
    public int TotalAppealedDenials { get; set; }
    public int TotalUnderpayments { get; set; }

    public List<AgingBucketSummaryDto> AgingBreakdown { get; set; } = new();
    public List<PayerDenialSummaryDto> ByPayer { get; set; } = new();
    public List<DenialReasonSummaryDto> ByReasonCode { get; set; } = new();
}

public class AgingBucketSummaryDto
{
    public string Bucket { get; set; } = string.Empty;  // "0-30" | "31-60" | "61-90" | "90+"
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class PayerDenialSummaryDto
{
    public int PayerId { get; set; }
    public string? PayerName { get; set; }
    public int DenialCount { get; set; }
    public decimal TotalAmountDenied { get; set; }
    public double DenialRate { get; set; }  // DenialCount / TotalClaimsSubmitted * 100
}

public class DenialReasonSummaryDto
{
    public string? ReasonCode { get; set; }  // CARC code
    public string? Description { get; set; } // Human-readable label
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

