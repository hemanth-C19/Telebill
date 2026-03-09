using System;

using Telebill.Data;
using Telebill.Dto;
using Telebill.Repositories.ChargeLines;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace Telebill.Services.ChargeLines
{
    public class ChargeLineService : IChargeLineService
    {
        
private readonly IChargeLineRepository _repo;
        private readonly TeleBillContext _context; // optional: for cross-entity validations

        public ChargeLineService(IChargeLineRepository repo, TeleBillContext context)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<ChargeLineDTO>> GetByEncounterId(int encounterId)
            => _repo.GetByEncounterId(encounterId);

        public Task<ChargeLineDTO?> GetById(int chargeId)
            => _repo.GetById(chargeId);

        public async Task<ChargeLineDTO> Add(int encounterId, ChargeLineCreateDto dto)
        {
            // Domain validations (lightweight)
            if (encounterId <= 0)
                throw new ValidationException("Valid EncounterId is required.");

            if (string.IsNullOrWhiteSpace(dto.CptHcpcs))
                throw new ValidationException("CPT/HCPCS code is required.");

            // Ensure the encounter exists (prevents orphan charge lines)
            var encounterExists = await _context.Encounters.AnyAsync(e => e.EncounterId == encounterId);
            if (!encounterExists)
                throw new ValidationException($"Encounter {encounterId} was not found.");

            // (Optional) normalize inputs
            dto.CptHcpcs = dto.CptHcpcs.Trim().ToUpperInvariant();

            // Persist via repository
            return await _repo.Add(encounterId, dto);
        }

        public async Task<ChargeLineDTO?> Update(int chargeId, ChargeLineUpdateDto dto)
        {
            if (chargeId <= 0)
                throw new ValidationException("Valid ChargeId is required.");

            // Basic guardrails
            if (dto.Units.HasValue && dto.Units.Value < 0)
                throw new ValidationException("Units cannot be negative.");

            if (dto.ChargeAmount.HasValue && dto.ChargeAmount.Value < 0)
                throw new ValidationException("ChargeAmount cannot be negative.");

            // (Optional) normalize status
            if (!string.IsNullOrWhiteSpace(dto.Status))
                dto.Status = dto.Status.Trim();

            // Persist via repository
            return await _repo.Update(chargeId, dto);
        }

        public Task<bool> Delete(int chargeId)
        {
            if (chargeId <= 0)
                throw new ValidationException("Valid ChargeId is required.");

            return _repo.Delete(chargeId);
        }

        public async Task<bool> SetStatus(int chargeId, string status)
        {
            if (chargeId <= 0)
                throw new ValidationException("Valid ChargeId is required.");

            if (string.IsNullOrWhiteSpace(status))
                throw new ValidationException("Status is required.");

            status = status.Trim();

            // (Optional) whitelist statuses
            // var allowed = new[] { "Draft", "Finalized" };
            // if (!allowed.Contains(status, StringComparer.OrdinalIgnoreCase))
            //     throw new ValidationException($"Invalid status '{status}'.");

            return await _repo.SetStatus(chargeId, status);
        }

        public Task<bool> ExistsForEncounter(int encounterId)
        {
            if (encounterId <= 0)
                throw new ValidationException("Valid EncounterId is required.");

            return _repo.ExistsForEncounter(encounterId);
        }

    }
}