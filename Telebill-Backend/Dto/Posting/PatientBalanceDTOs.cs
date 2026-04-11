using System;
using System.Collections.Generic;

namespace Telebill.Dto.Posting;

public class UpdatePatientBalanceStatusRequestDto
{
    public string Status { get; set; } = string.Empty; // Paid | WrittenOff
    public string? Reason { get; set; }
}

public class PatientBalanceDto
{
    public int BalanceID { get; set; }
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MRN { get; set; } = string.Empty;
    public int ClaimID { get; set; }
    public decimal AmountDue { get; set; }
    public string AgingBucket { get; set; } = string.Empty;
    public DateOnly? LastStatementDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PatientBalanceListResponseDto
{
    public int TotalCount { get; set; }
    public decimal TotalAmountDue { get; set; }
    public List<PatientBalanceDto> Balances { get; set; } = new();
}

public class AgingSummaryDto
{
    public decimal Bucket0To30 { get; set; }
    public decimal Bucket31To60 { get; set; }
    public decimal Bucket61To90 { get; set; }
    public decimal Bucket90Plus { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int OpenBalanceCount { get; set; }
}

public class AgingBatchJobResultDto
{
    public string JobID { get; set; } = string.Empty;
    public DateTime RunAt { get; set; }
    public int BalancesUpdated { get; set; }
    public double DurationSeconds { get; set; }
}

