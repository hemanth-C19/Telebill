using System;
using System.Collections.Generic;

namespace Telebill.Dto;

// Request DTOs

public class CreateScrubRuleRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string ExpressionJSON { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

public class UpdateScrubRuleRequestDto
{
    public string? Name { get; set; }
    public string? ExpressionJSON { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
}

// Response DTOs

public class ScrubIssueDto
{
    public int IssueID { get; set; }
    public int ClaimID { get; set; }
    public int? ClaimLineID { get; set; }
    public int? LineNo { get; set; }
    public int RuleID { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime DetectedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ScrubResultDto
{
    public int ClaimID { get; set; }
    public DateTime ScrubRunAt { get; set; }
    public string ClaimStatus { get; set; } = string.Empty;
    public int TotalRulesEvaluated { get; set; }
    public int NewIssuesCreated { get; set; }
    public int IssuesAutoResolved { get; set; }
    public int OpenErrors { get; set; }
    public int OpenWarnings { get; set; }
    public List<ScrubIssueDto> Issues { get; set; } = new();
}

public class ResolveIssueResponseDto
{
    public int IssueID { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ClaimID { get; set; }
    public string NewClaimStatus { get; set; } = string.Empty;
    public int RemainingOpenErrors { get; set; }
    public int RemainingOpenWarnings { get; set; }
}

public class ScrubRuleDto
{
    public int RuleID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExpressionJSON { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ScrubBatchResultDto
{
    public string JobID { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public int ClaimsProcessed { get; set; }
    public int ClaimsMovedToReady { get; set; }
    public int ClaimsStillInError { get; set; }
    public double DurationSeconds { get; set; }
}

