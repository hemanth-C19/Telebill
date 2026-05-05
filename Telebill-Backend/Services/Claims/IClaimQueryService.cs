using System.Threading.Tasks;
using Telebill.Dto;

namespace Services;

public interface IClaimQueryService
{
    Task<ClaimListResponseDto> GetClaimsAsync(ClaimFilterParams filters);
    Task<ClaimDetailDto?> GetClaimDetailAsync(int claimID);
    Task<ClaimStatusSummaryDto?> GetClaimSummaryAsync(int claimID);
}

