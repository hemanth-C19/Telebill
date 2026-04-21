using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Data;
using Telebill.Dto;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;



namespace Telebill.Repositories.ChargeLines
{
    public class ChargeLineRepository(TeleBillContext context) : IChargeLineRepository
    {
        public async Task<List<ChargeLineDTO>> GetByEncounterId(int encounterId)
        {
            return await context.ChargeLines
                .Where(c => c.EncounterId == encounterId)
                .Select(c => new ChargeLineDTO
                {
                    ChargeId = c.ChargeId,
                    EncounterId = c.EncounterId,
                    CPT_HCPCS = c.CptHcpcs,
                    Modifiers = c.Modifiers,  // JSON → string or List<string> depending on your entity
                    Units = c.Units,
                    ChargeAmount = c.ChargeAmount,
                    RevenueCode = c.RevenueCode,
                    Notes = c.Notes,
                    Status = c.Status
                })
                .ToListAsync();
        }

        public async Task<ChargeLineDTO?> GetById(int chargeId)
        {
            return await context.ChargeLines
                .Where(c => c.ChargeId == chargeId)
                .Select(c => new ChargeLineDTO
                {
                    ChargeId = c.ChargeId,
                    EncounterId = c.EncounterId,
                    CPT_HCPCS = c.CptHcpcs,
                    Modifiers = c.Modifiers,
                    Units = c.Units,
                    ChargeAmount = c.ChargeAmount,
                    RevenueCode = c.RevenueCode,
                    Notes = c.Notes,
                    Status = c.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ChargeLineDTO> Add(int encounterId, ChargeLineCreateDto dto)
        {
            var entity = new ChargeLine
            {
                EncounterId = encounterId,
                CptHcpcs  = dto.CptHcpcs,
                Modifiers = dto.Modifiers, // If your entity stores JSON string, serialize here
                Units = dto.Units,
                ChargeAmount = dto.ChargeAmount,
                RevenueCode = dto.RevenueCode,
                Notes = dto.Notes,
                Status = "Finalized"
            };

            context.ChargeLines.Add(entity);
            await context.SaveChangesAsync();

            return new ChargeLineDTO
            {
                ChargeId = entity.ChargeId,
                EncounterId = entity.EncounterId,
                CPT_HCPCS = entity.CptHcpcs,
                Modifiers = entity.Modifiers,
                Units = entity.Units,
                ChargeAmount = entity.ChargeAmount,
                RevenueCode = entity.RevenueCode,
                Notes = entity.Notes,
                Status = entity.Status
            };
        }

        public async Task<ChargeLineDTO?> Update(int chargeId, ChargeLineUpdateDto dto)
        {
            var entity = await context.ChargeLines.FirstOrDefaultAsync(c => c.ChargeId == chargeId);
            if (entity == null) return null;

            entity.Modifiers = dto.Modifiers ?? entity.Modifiers;
            entity.Units = dto.Units;
            entity.ChargeAmount = dto.ChargeAmount;
            entity.Notes = dto.Notes;
            if (!string.IsNullOrWhiteSpace(dto.Status))
                entity.Status = dto.Status;

            await context.SaveChangesAsync();

            return new ChargeLineDTO
            {
                ChargeId = entity.ChargeId,
                EncounterId = entity.EncounterId,
                CPT_HCPCS = entity.CptHcpcs,
                Modifiers = entity.Modifiers,
                Units = entity.Units,
                ChargeAmount = entity.ChargeAmount,
                RevenueCode = entity.RevenueCode,
                Notes = entity.Notes,
                Status = entity.Status
            };
        }

        public async Task<bool> Delete(int chargeId)
        {
            var entity = await context.ChargeLines.FindAsync(chargeId);
            if (entity == null) return false;

            context.ChargeLines.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetStatus(int chargeId, string status)
        {
            var entity = await context.ChargeLines.FindAsync(chargeId);
            if (entity == null) return false;

            entity.Status = status;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsForEncounter(int encounterId)
        {
            return await context.ChargeLines.AnyAsync(c => c.EncounterId == encounterId);
        }

    }
}