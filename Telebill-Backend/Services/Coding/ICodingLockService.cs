using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Coding;

namespace Telebill.Services.Coding
{
    public interface ICodingLockService
    {
        Task<CodingValidationResultDto> ValidateCodingLockAsync(int encounterId);
        Task<ApplyCodingLockResponseDto> ApplyCodingLockAsync(ApplyCodingLockDto dto, int userId);
        Task<(bool success, string error, UnlockCodingResponseDto? result)> UnlockCodingAsync(UnlockCodingDto dto, int userId);
        Task<List<CodingLockResultDto>> GetCodingLockHistoryAsync(int encounterId);
    }
}

