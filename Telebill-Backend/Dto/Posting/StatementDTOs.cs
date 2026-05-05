using System;
using System.Collections.Generic;

namespace Telebill.Dto.Posting;

public class GenerateStatementRequestDto
{
    public int PatientID { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}

public class GenerateStatementBatchRequestDto
{
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string SchedulerKey { get; set; } = string.Empty;
}

public class UpdateStatementStatusRequestDto
{
    public string Status { get; set; } = string.Empty; // Open | Closed
}

public class StatementLineItemDto
{
    public int ClaimID { get; set; }
    public DateOnly ServiceDate { get; set; }
    public string CptHcpcs { get; set; } = string.Empty;
    public decimal Billed { get; set; }
    public decimal InsurancePaid { get; set; }
    public decimal Adjustment { get; set; }
    public decimal PatientDue { get; set; }
}

public class StatementDto
{
    public int StatementID { get; set; }
    public int PatientID { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MRN { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateTime GeneratedDate { get; set; }
    public decimal AmountDue { get; set; }
    public List<StatementLineItemDto> LineItems { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class StatementListResponseDto
{
    public int TotalCount { get; set; }
    public List<StatementDto> Statements { get; set; } = new();
}

public class StatementBatchResultDto
{
    public string JobID { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public int StatementsGenerated { get; set; }
    public int PatientsProcessed { get; set; }
    public decimal TotalAmountBilled { get; set; }
    public double DurationSeconds { get; set; }
}

