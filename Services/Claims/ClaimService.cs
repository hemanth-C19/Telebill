using System.Threading.Tasks;
using Telebill.Dto;

namespace Services;

public class ClaimService : IClaimService
{
    private readonly IClaimBuildService _buildService;
    private readonly IClaimQueryService _queryService;
    private readonly IClaimStatusService _statusService;
    private readonly IClaimScrubService _scrubService;
    private readonly IClaimRuleService _ruleService;
    private readonly IClaimX12Service _x12Service;

    public ClaimService(
        IClaimBuildService buildService,
        IClaimQueryService queryService,
        IClaimStatusService statusService,
        IClaimScrubService scrubService,
        IClaimRuleService ruleService,
        IClaimX12Service x12Service)
    {
        _buildService = buildService;
        _queryService = queryService;
        _statusService = statusService;
        _scrubService = scrubService;
        _ruleService = ruleService;
        _x12Service = x12Service;
    }

    public Task<BuildClaimResponseDto> BuildClaimAsync(BuildClaimRequestDto dto) =>
        _buildService.BuildClaimAsync(dto);

    public Task<ClaimListResponseDto> GetClaimsAsync(ClaimFilterParams filters) =>
        _queryService.GetClaimsAsync(filters);

    public Task<ClaimDetailDto?> GetClaimDetailAsync(int claimID) =>
        _queryService.GetClaimDetailAsync(claimID);

    public Task<ClaimStatusSummaryDto?> GetClaimSummaryAsync(int claimID) =>
        _queryService.GetClaimSummaryAsync(claimID);

    public Task<UpdateClaimStatusResponseDto?> UpdateClaimStatusAsync(int claimID, UpdateClaimStatusRequestDto dto) =>
        _statusService.UpdateClaimStatusAsync(claimID, dto);

    public Task<ScrubResultDto?> ScrubClaimAsync(int claimID) =>
        _scrubService.ScrubClaimAsync(claimID);

    public Task<System.Collections.Generic.List<ScrubIssueDto>> GetScrubIssuesAsync(int claimID, string statusFilter) =>
        _scrubService.GetScrubIssuesAsync(claimID, statusFilter);

    public Task<ResolveIssueResponseDto?> ResolveIssueAsync(int claimID, int issueID, ResolveIssueRequestDto dto) =>
        _scrubService.ResolveIssueAsync(claimID, issueID, dto);

    public Task<ScrubBatchResultDto> RunScrubBatchAsync() =>
        _scrubService.RunScrubBatchAsync();

    public Task<System.Collections.Generic.List<ScrubRuleDto>> GetScrubRulesAsync(string? severity, string? status) =>
        _ruleService.GetScrubRulesAsync(severity, status);

    public Task<ScrubRuleDto?> CreateScrubRuleAsync(CreateScrubRuleRequestDto dto) =>
        _ruleService.CreateScrubRuleAsync(dto);

    public Task<ScrubRuleDto?> UpdateScrubRuleAsync(int ruleID, UpdateScrubRuleRequestDto dto) =>
        _ruleService.UpdateScrubRuleAsync(ruleID, dto);

    public Task<X12RefDto?> Generate837PAsync(int claimID) =>
        _x12Service.Generate837PAsync(claimID);

    public Task<X12RefDto?> Get837PRefAsync(int claimID) =>
        _x12Service.Get837PRefAsync(claimID);

    public Task TriggerClaimBuildFromEncounterAsync(int encounterID) =>
        _buildService.TriggerClaimBuildFromEncounterAsync(encounterID);
}

