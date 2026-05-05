using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Coding;

namespace Telebill.Services.Coding
{
    public interface IProviderCodingService
    {
        Task<List<ProviderEncounterSummaryDto>> GetProviderEncountersAsync(int providerId, string? status);
        Task<ProviderEncounterSummaryDto?> GetProviderEncounterDetailAsync(int encounterId, int providerId);
        Task<ProviderEncounterSummaryDto?> SetDocumentationUriAsync(int encounterId, SetDocumentationUriDto dto, int providerId);
        Task<(bool success, string error)> MarkReadyForCodingAsync(int encounterId, int providerId);
    }
}

