using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Data;
using Telebill.Models;
using Services;

namespace Telebill.Repositories.Claims;

public interface IClaimRepository
{
    // Claim
    Task<Claim?> GetByIdAsync(int claimID);
    Task<Claim?> GetByIdWithLinesAsync(int claimID);
    Task<Claim?> GetByEncounterIDAsync(int encounterID);
    Task<(List<Claim> claims, int totalCount)> GetClaimsPagedAsync(ClaimFilterParams filters);
    Task<Claim> CreateAsync(Claim claim);
    Task UpdateStatusAsync(int claimID, string newStatus);
    Task<bool> ExistsForEncounterAsync(int encounterID);

    // Claim line
    Task<List<ClaimLine>> GetLinesByClaimIDAsync(int claimID);
    Task CreateLinesAsync(List<ClaimLine> lines);
    Task<ClaimLine?> GetLineByIdAsync(int claimLineID);

    // Scrub rules
    Task<List<ScrubRule>> GetActiveScrubRulesAsync();
    Task<List<ScrubRule>> GetScrubRulesFilteredAsync(string? severity, string? status);
    Task<ScrubRule?> GetScrubRuleByIdAsync(int ruleID);
    Task<ScrubRule> CreateScrubRuleAsync(ScrubRule rule);
    Task<ScrubRule> UpdateScrubRuleAsync(ScrubRule rule);
    Task<bool> DeleteScrubRuleAsync(int ruleID);

    // Scrub issues
    Task<List<ScrubIssue>> GetIssuesByClaimIDAsync(int claimID, string statusFilter);
    Task<ScrubIssue?> GetIssueByIdAsync(int issueID);
    Task<ScrubIssue?> GetOpenIssueByRuleAsync(int claimID, int ruleID, int? claimLineID);
    Task<int> CountOpenErrorsAsync(int claimID);
    Task<int> CountOpenWarningsAsync(int claimID);
    Task CreateIssueAsync(ScrubIssue issue);
    Task ResolveIssueAsync(int issueID);
    Task ResolveIssueByRuleAsync(int claimID, int ruleID, int? claimLineID);

    // X12 reference
    Task<X12837pRef?> GetX12RefByClaimIDAsync(int claimID);
    Task<X12837pRef> CreateX12RefAsync(X12837pRef x12Ref);
    Task UpdateX12RefAsync(X12837pRef x12Ref);

    // Cross-module reads
    Task<Encounter?> GetEncounterByIdAsync(int encounterID);
    Task<List<ChargeLine>> GetFinalizedChargeLinesByEncounterAsync(int encounterID);
    Task<List<Diagnosis>> GetActiveDiagnosesByEncounterAsync(int encounterID);
    Task<Coverage?> GetActiveCoverageForEncounterAsync(int patientID, DateTime encounterDate);
    Task<PayerPlan?> GetPayerPlanByIdAsync(int planID);
    Task<FeeSchedule?> GetFeeScheduleAsync(int planID, string cptHcpcs, string? modifierCombo, DateTime serviceDate);
    Task<Provider?> GetProviderByIdAsync(int providerID);
    Task<CodingLock?> GetActiveCodingLockAsync(int encounterID);
    Task<bool> HasApprovedPriorAuthAsync(int claimID);

    // (Audit / notifications removed for now)
}

