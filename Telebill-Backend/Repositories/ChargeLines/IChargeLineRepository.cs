using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto;

namespace Telebill.Repositories.ChargeLines
{
    public interface IChargeLineRepository
    {

        Task<List<ChargeLineDTO>> GetByEncounterId(int encounterId);
        Task<ChargeLineDTO?> GetById(int chargeId);

        Task<ChargeLineDTO> Add(int encounterId, ChargeLineCreateDto dto);
        Task<ChargeLineDTO?> Update(int chargeId, ChargeLineUpdateDto dto);

        Task<bool> Delete(int chargeId);

        // Optional workflow helpers
        Task<bool> SetStatus(int chargeId, string status); // Draft/Finalized
        Task<bool> ExistsForEncounter(int encounterId);

    }
}