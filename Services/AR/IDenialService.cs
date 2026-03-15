using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.AR;

namespace Telebill.Services.AR;

public interface IDenialService
{
    // Worklist & Detail
    Task<List<ArWorklistItemDto>> GetArWorklistAsync(ArWorklistFilterParams filters);
    Task<DenialDetailDto?> GetDenialDetailAsync(int denialId);

    // Denial Actions
    Task<(bool success, string error)> UpdateDenialStatusAsync(
        int denialId, UpdateDenialStatusDto dto);

    Task<(bool success, string error, AttachmentSummaryDto? result)> UploadAppealDocumentAsync(
        UploadAppealDocumentDto dto);

    Task<(bool success, string error, ResetClaimResponseDto? result)> ResetClaimForResubmissionAsync(
        ResetClaimForResubmissionDto dto);
}
