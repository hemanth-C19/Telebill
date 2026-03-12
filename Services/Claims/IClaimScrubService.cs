using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto;

namespace Services;

public interface IClaimScrubService
{
    Task<ScrubResultDto?> ScrubClaimAsync(int claimID);
    Task<List<ScrubIssueDto>> GetScrubIssuesAsync(int claimID, string statusFilter);
    Task<ResolveIssueResponseDto?> ResolveIssueAsync(int claimID, int issueID, ResolveIssueRequestDto dto);
    Task<ScrubBatchResultDto> RunScrubBatchAsync();
}

