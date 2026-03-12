using System.Threading.Tasks;
using Telebill.Dto;

namespace Services;

public interface IClaimBuildService
{
    Task<BuildClaimResponseDto> BuildClaimAsync(BuildClaimRequestDto dto);
    Task TriggerClaimBuildFromEncounterAsync(int encounterID);
}

