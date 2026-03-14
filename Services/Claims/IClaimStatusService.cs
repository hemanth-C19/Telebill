using System.Threading.Tasks;
using Telebill.Dto;

namespace Services;

public interface IClaimStatusService
{
    Task<UpdateClaimStatusResponseDto?> UpdateClaimStatusAsync(int claimID, UpdateClaimStatusRequestDto dto);
}

