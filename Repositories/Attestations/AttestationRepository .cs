using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Data;
using Telebill.Dto;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;

namespace Telebill.Repositories.Attestations
{
    public class AttestationRepository : IAttestationRepository
    {

        private readonly TeleBillContext _context;

        public AttestationRepository(TeleBillContext context)
        {
            _context = context;
        }

        public async Task<AttestationDTO?> GetByEncounterId(int encounterId)
        {
            return await _context.Attestations
                .Where(a => a.EncounterId == encounterId)
                .OrderByDescending(a => a.AttestDate) // latest if multiple (rare)
                .Select(a => new AttestationDTO
                {
                    AttestId = a.AttestId,
                    EncounterId = a.EncounterId,
                    ProviderId = a.ProviderId,
                    AttestText = a.AttestText,
                    AttestDate = a.AttestDate,
                    Status = a.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AttestationDTO?> GetById(int attestId)
        {
            return await _context.Attestations
                .Where(a => a.AttestId == attestId)
                .Select(a => new AttestationDTO
                {
                    AttestId = a.AttestId,
                    EncounterId = a.EncounterId,
                    ProviderId = a.ProviderId,
                    AttestText = a.AttestText,
                    AttestDate = a.AttestDate,
                    Status = a.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AttestationDTO> Add(int encounterId, AttestationCreateDto dto)
        {
            var entity = new Attestation
            {
                EncounterId = encounterId,
                ProviderId = dto.ProviderID,
                AttestText = dto.AttestText,
                AttestDate = DateTime.UtcNow,
                Status = "Draft"
            };

            _context.Attestations.Add(entity);
            await _context.SaveChangesAsync();

            return new AttestationDTO
            {
                AttestId = entity.AttestId,
                EncounterId = entity.EncounterId,
                ProviderId = entity.ProviderId,
                AttestText = entity.AttestText,
                AttestDate = entity.AttestDate,
                Status = entity.Status
            };
        }

        public async Task<AttestationDTO?> Update(int attestId, AttestationUpdateDto dto)
        {
            var entity = await _context.Attestations.FirstOrDefaultAsync(a => a.AttestId == attestId);
            if (entity == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.AttestText))
                entity.AttestText = dto.AttestText;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                entity.Status = dto.Status;

            await _context.SaveChangesAsync();

            return new AttestationDTO
            {
                AttestId = entity.AttestId,
                EncounterId = entity.EncounterId,
                ProviderId = entity.ProviderId,
                AttestText = entity.AttestText,
                AttestDate = entity.AttestDate,
                Status = entity.Status
            };
        }

        public async Task<bool> Delete(int attestId)
        {
            var entity = await _context.Attestations.FindAsync(attestId);
            if (entity == null) return false;

            _context.Attestations.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Finalize(int attestId, DateTime signedDate, string? status = "Signed")
        {
            var entity = await _context.Attestations.FindAsync(attestId);
            if (entity == null) return false;

            entity.AttestDate = signedDate;
            entity.Status = status ?? "Signed";
            await _context.SaveChangesAsync();
            return true;
        }

    }
}