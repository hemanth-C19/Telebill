using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto;

namespace Telebill.Repositories.Attestations
{
    public interface IAttestationRepository
    {

        Task<AttestationDTO?> GetByEncounterId(int encounterId);
        Task<AttestationDTO?> GetById(int attestId);

        Task<AttestationDTO> Add(int encounterId, AttestationCreateDto dto);
        Task<AttestationDTO?> Update(int attestId, AttestationUpdateDto dto);

        Task<bool> Delete(int attestId);

        // Optional workflow helper
        Task<bool> Finalize(int attestId, DateTime signedDate, string? status = "Signed");

    }
}