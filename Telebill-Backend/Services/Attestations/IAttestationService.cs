using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto;

namespace Telebill.Services.Attestations
{
    public interface IAttestationService
    {
        
Task<AttestationDTO?> GetByEncounterId(int encounterId);
        Task<AttestationDTO?> GetById(int attestId);

        /// <summary>
        /// Create an attestation for an encounter (usually starts in Draft).
        /// </summary>
        Task<AttestationDTO> Add(int encounterId, AttestationCreateDto dto);

        /// <summary>
        /// Update an attestation (text and/or status).
        /// </summary>
        Task<AttestationDTO?> Update(int attestId, AttestationUpdateDto dto);

        /// <summary>
        /// Delete an attestation.
        /// </summary>
        Task<bool> Delete(int attestId);

        /// <summary>
        /// Finalize/sign an attestation. By default sets status = "Signed".
        /// </summary>
        Task<bool> Finalize(int attestId, DateTime signedDate, string? status = "Signed");

    }
}