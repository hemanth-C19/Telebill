using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Coding;

namespace Telebill.Services.Coding
{
    public interface ICoderWorklistService
    {
        // Worklist
        Task<List<CodingWorklistItemDto>> GetCodingWorklistAsync(int? providerId, int? planId);
        Task<WorklistFiltersDto> GetWorklistFiltersAsync();
        Task<CodingEncounterCardDto?> GetCodingEncounterCardAsync(int encounterId);
        Task<(bool success, string error)> UpdateEncounterPosAsync(int encounterId, UpdateEncounterPosDto dto, int userId);
        Task<(bool success, string error, ChargeLineInfoDto? updated)> UpdateChargeLineModifiersAsync(int encounterId, int chargeId, UpdateChargeLineModifiersDto dto, int userId);

        // Diagnosis
        Task<(bool success, string error, DiagnosisResultDto? result)> AddDiagnosisAsync(AddDiagnosisDto dto, int userId);
        Task<List<DiagnosisResultDto>> GetDiagnosesByEncounterAsync(int encounterId);
        Task<(bool success, string error, DiagnosisResultDto? result)> UpdateDiagnosisAsync(int dxId, UpdateDiagnosisDto dto, int userId);
        Task<(bool success, string error)> RemoveDiagnosisAsync(int dxId, int userId);
    }
}

