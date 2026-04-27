using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Reports;
using Telebill.Models;

namespace Telebill.Repositories.Reports;

public interface IReportQueryRepository
{
    // ── KPI source data ───────────────────────────────────────

    // Returns claims in period, optionally filtered by payerId / planId / providerId
    Task<List<Claim>> GetClaimsForPeriodAsync(
        DateTime from, DateTime to, int? payerId, int? planId, int? providerId);

    // Returns ScrubIssues for a list of ClaimIds
    Task<List<ScrubIssue>> GetScrubIssuesByClaimIdsAsync(List<int> claimIds);

    // Returns the ScrubRule name for a given RuleId
    Task<ScrubRule?> GetScrubRuleByIdAsync(int ruleId);

    // Returns SubmissionRefs for a list of ClaimIds (AckType="277CA" for TAT + FPAR)
    Task<List<SubmissionRef>> GetSubmissionRefsByClaimIdsAsync(List<int> claimIds);

    // Returns PaymentPosts for a list of ClaimIds (for DSO)
    Task<List<PaymentPost>> GetPaymentPostsByClaimIdsAsync(List<int> claimIds);

    // Returns Encounters for a list of EncounterIds (for DSO date calc)
    Task<List<Encounter>> GetEncountersByIdsAsync(List<int?> encounterIds);

    // Returns Denials for a list of ClaimIds (for DenialRate)
    Task<List<Denial>> GetDenialsByClaimIdsAsync(List<int> claimIds);

    // ── Lookup helpers (resolve names for DTOs) ───────────────

    Task<Patient?> GetPatientByIdAsync(int? patientId);
    Task<Provider?> GetProviderByIdAsync(int providerId);
    Task<Payer?> GetPayerByIdAsync(int payerId);
    Task<PayerPlan?> GetPlanByIdAsync(int planId);

    // Resolve PayerId from PlanId (via PayerPlan)
    Task<int?> GetPayerIdByPlanIdAsync(int? planId);

    // Resolve ProviderId from EncounterId (via Encounter)
    Task<int?> GetProviderIdByEncounterIdAsync(int? encounterId);

    // ── Export source queries ─────────────────────────────────

    // Claims listing export
    Task<List<Claim>> GetClaimsForExportAsync(ExportFilterParams filters);

    // Scrub issues export
    Task<List<ScrubIssue>> GetScrubIssuesForExportAsync(ExportFilterParams filters);

    // AR aging export — open + appealed denials
    Task<List<Denial>> GetDenialsForExportAsync(ExportFilterParams filters);

    // Statements export
    Task<List<Statement>> GetStatementsForExportAsync(ExportFilterParams filters);

    // Remit summary export
    Task<List<RemitRef>> GetRemitRefsForExportAsync(ExportFilterParams filters);

    // SUM of AmountPaid across all PaymentPosts linked to claims for a RemitRef
    Task<decimal> GetTotalPostedByRemitIdAsync(int remitId);

    // FrontDesk dashboard summary counts
    Task<FrontDeskSummaryDto> GetFrontDeskSummaryAsync();
}

