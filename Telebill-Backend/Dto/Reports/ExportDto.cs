using System;
using System.Collections.Generic;

namespace Telebill.Dto.Reports;

// Shared filter params used across all 5 export types
public class ExportFilterParams
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? PayerId { get; set; }
    public int? PlanId { get; set; }
    public int? ProviderId { get; set; }
    public string? Status { get; set; }
}

// ── 1. Claims Listing ────────────────────────────────────────
public class ClaimsListingRowDto
{
    public int ClaimId { get; set; }
    public string? PatientName { get; set; }
    public string? ProviderName { get; set; }
    public string? PayerName { get; set; }
    public string? PlanName { get; set; }
    public DateTime EncounterDate { get; set; }
    public decimal TotalCharge { get; set; }
    public string? ClaimStatus { get; set; }
    public DateTime CreatedDate { get; set; }
}

// ── 2. Scrub Issues Report ───────────────────────────────────
public class ScrubIssueExportRowDto
{
    public int IssueId { get; set; }
    public int ClaimId { get; set; }
    public int? ClaimLineId { get; set; }
    public string? RuleName { get; set; }
    public string? Severity { get; set; }
    public string? Message { get; set; }
    public DateTime? DetectedDate { get; set; }
    public string? IssueStatus { get; set; }
    public string? PatientName { get; set; }
    public string? PayerName { get; set; }
}

// ── 3. AR Aging Report ───────────────────────────────────────
public class ArAgingRowDto
{
    public int DenialId { get; set; }
    public int ClaimId { get; set; }
    public string? PatientName { get; set; }
    public string? PayerName { get; set; }
    public string? ReasonCode { get; set; }
    public decimal AmountDenied { get; set; }
    public DateOnly DenialDate { get; set; }
    public int DaysSinceDenial { get; set; }   // computed: today - DenialDate
    public string? AgingBucket { get; set; }   // "0-30" | "31-60" | "61-90" | "90+"
    public string? DenialStatus { get; set; }
}

// ── 4. Statements Summary ────────────────────────────────────
public class StatementSummaryRowDto
{
    public int StatementId { get; set; }
    public string? PatientName { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedDate { get; set; }
    public decimal AmountDue { get; set; }
    public string? StatementStatus { get; set; }
}

// ── 5. Remit Summary ─────────────────────────────────────────
public class RemitSummaryRowDto
{
    public int RemitId { get; set; }
    public string? PayerName { get; set; }
    public int? BatchId { get; set; }
    public string? PayloadUri { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? RemitStatus { get; set; }
    public decimal TotalPosted { get; set; }   // SUM of PaymentPosts for this ERA
}

