using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto;

namespace Services;

public interface IClaimService
{
    // Claim management
    Task<BuildClaimResponseDto> BuildClaimAsync(BuildClaimRequestDto dto);
    Task<ClaimListResponseDto> GetClaimsAsync(ClaimFilterParams filters);
    Task<ClaimDetailDto?> GetClaimDetailAsync(int claimID);
    Task<ClaimStatusSummaryDto?> GetClaimSummaryAsync(int claimID);
    Task<UpdateClaimStatusResponseDto?> UpdateClaimStatusAsync(int claimID, UpdateClaimStatusRequestDto dto);

    // Scrubbing
    Task<ScrubResultDto?> ScrubClaimAsync(int claimID);
    Task<List<ScrubIssueDto>> GetScrubIssuesAsync(int claimID, string statusFilter);
    Task<ResolveIssueResponseDto?> ResolveIssueAsync(int claimID, int issueID, ResolveIssueRequestDto dto);
    Task<ScrubBatchResultDto> RunScrubBatchAsync();

    // Scrub rules
    Task<List<ScrubRuleDto>> GetScrubRulesAsync(string? severity, string? status);
    Task<ScrubRuleDto?> CreateScrubRuleAsync(CreateScrubRuleRequestDto dto);
    Task<ScrubRuleDto?> UpdateScrubRuleAsync(int ruleID, UpdateScrubRuleRequestDto dto);

    // 837P
    Task<X12RefDto?> Generate837PAsync(int claimID);
    Task<X12RefDto?> Get837PRefAsync(int claimID);

    // Internal
    Task TriggerClaimBuildFromEncounterAsync(int encounterID);
}

