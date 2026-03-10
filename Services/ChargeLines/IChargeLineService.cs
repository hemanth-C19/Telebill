using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto;

namespace Telebill.Services.ChargeLines
{
    public interface IChargeLineService
    {
        
Task<List<ChargeLineDTO>> GetByEncounterId(int encounterId);
        Task<ChargeLineDTO?> GetById(int chargeId);

        /// <summary>
        /// Adds a new charge line to an encounter.
        /// </summary>
        Task<ChargeLineDTO> Add(int encounterId, ChargeLineCreateDto dto);

        /// <summary>
        /// Updates an existing charge line by id.
        /// </summary>
        Task<ChargeLineDTO?> Update(int chargeId, ChargeLineUpdateDto dto);

        /// <summary>
        /// Deletes a charge line by id.
        /// </summary>
        Task<bool> Delete(int chargeId);

        /// <summary>
        /// Sets status for a charge line (e.g., Draft/Finalized).
        /// </summary>
        Task<bool> SetStatus(int chargeId, string status);

        /// <summary>
        /// Checks if any charge lines exist for an encounter.
        /// </summary>
        Task<bool> ExistsForEncounter(int encounterId);

    }
}