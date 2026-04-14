using System.Threading.Tasks;
using Telebill.Dto;

namespace Services;

public class ClaimService(
    IClaimBuildService buildService,
    IClaimQueryService queryService,
    IClaimStatusService statusService,
    IClaimScrubService scrubService,
    IClaimRuleService ruleService,
    IClaimX12Service x12Service) : IClaimService
{
    public Task<BuildClaimResponseDto> BuildClaimAsync(BuildClaimRequestDto dto) =>
        buildService.BuildClaimAsync(dto);

    public Task<ClaimListResponseDto> GetClaimsAsync(ClaimFilterParams filters) =>
        queryService.GetClaimsAsync(filters);

    public Task<ClaimDetailDto?> GetClaimDetailAsync(int claimID) =>
        queryService.GetClaimDetailAsync(claimID);

    public Task<ClaimStatusSummaryDto?> GetClaimSummaryAsync(int claimID) =>
        queryService.GetClaimSummaryAsync(claimID);

    public Task<UpdateClaimStatusResponseDto?> UpdateClaimStatusAsync(int claimID, UpdateClaimStatusRequestDto dto) =>
        statusService.UpdateClaimStatusAsync(claimID, dto);

    public Task<ScrubResultDto?> ScrubClaimAsync(int claimID) =>
        scrubService.ScrubClaimAsync(claimID);

    public Task<System.Collections.Generic.List<ScrubIssueDto>> GetScrubIssuesAsync(int claimID, string statusFilter) =>
        scrubService.GetScrubIssuesAsync(claimID, statusFilter);

    public Task<ResolveIssueResponseDto?> ResolveIssueAsync(int claimID, int issueID, ResolveIssueRequestDto dto) =>
        scrubService.ResolveIssueAsync(claimID, issueID, dto);

    public Task<ScrubBatchResultDto> RunScrubBatchAsync() =>
        scrubService.RunScrubBatchAsync();

    public Task<System.Collections.Generic.List<ScrubRuleDto>> GetScrubRulesAsync(string? severity, string? status) =>
        ruleService.GetScrubRulesAsync(severity, status);

    public Task<ScrubRuleDto?> CreateScrubRuleAsync(CreateScrubRuleRequestDto dto) =>
        ruleService.CreateScrubRuleAsync(dto);

    public Task<ScrubRuleDto?> UpdateScrubRuleAsync(int ruleID, UpdateScrubRuleRequestDto dto) =>
        ruleService.UpdateScrubRuleAsync(ruleID, dto);

    public Task<X12RefDto?> Generate837PAsync(int claimID) =>
        x12Service.Generate837PAsync(claimID);

    public Task<X12RefDto?> Get837PRefAsync(int claimID) =>
        x12Service.Get837PRefAsync(claimID);

    public Task TriggerClaimBuildFromEncounterAsync(int encounterID) =>
        buildService.TriggerClaimBuildFromEncounterAsync(encounterID);
}
