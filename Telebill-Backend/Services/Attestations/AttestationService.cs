using System;

using Telebill.Data;
using Telebill.Dto;
using Telebill.Repositories.Attestations;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace Telebill.Services.Attestations
{
    public class AttestationService(IAttestationRepository repo, TeleBillContext context) : IAttestationService
    {
        private readonly IAttestationRepository _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        private readonly TeleBillContext _context = context ?? throw new ArgumentNullException(nameof(context)); // for cross-entity validations

        public Task<AttestationDTO?> GetByEncounterId(int encounterId)
        {
            if (encounterId <= 0)
                throw new ValidationException("Valid EncounterId is required.");
            return _repo.GetByEncounterId(encounterId);
        }

        public Task<AttestationDTO?> GetById(int attestId)
        {
            if (attestId <= 0)
                throw new ValidationException("Valid AttestId is required.");
            return _repo.GetById(attestId);
        }

        public async Task<AttestationDTO> Add(int encounterId, AttestationCreateDto dto)
        {
            if (encounterId <= 0)
                throw new ValidationException("Valid EncounterId is required.");

            // ProviderId can be required depending on your workflow.
            // If you allow creating a draft w/out provider, remove this check.
            if (dto.ProviderID <= 0)
                throw new ValidationException("Valid ProviderID is required.");

            // Ensure the encounter exists to avoid orphan attestations
            var encounterExists = await _context.Encounters
                .AnyAsync(e => e.EncounterId == encounterId);
            if (!encounterExists)
                throw new ValidationException($"Encounter {encounterId} was not found.");

            // Optional normalization
            if (!string.IsNullOrWhiteSpace(dto.AttestText))
                dto.AttestText = dto.AttestText.Trim();

            return await _repo.Add(encounterId, dto);
        }

        public async Task<AttestationDTO?> Update(int attestId, AttestationUpdateDto dto)
        {
            if (attestId <= 0)
                throw new ValidationException("Valid AttestId is required.");

            if (dto.AttestText is not null && string.IsNullOrWhiteSpace(dto.AttestText))
                throw new ValidationException("Attestation text cannot be only whitespace.");

            if (!string.IsNullOrWhiteSpace(dto.AttestText))
                dto.AttestText = dto.AttestText.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Status))
                dto.Status = dto.Status.Trim();

            return await _repo.Update(attestId, dto);
        }

        public Task<bool> Delete(int attestId)
        {
            if (attestId <= 0)
                throw new ValidationException("Valid AttestId is required.");

            return _repo.Delete(attestId);
        }

        public async Task<bool> Finalize(int attestId, DateTime signedDate, string? status = "Signed")
        {
            if (attestId <= 0)
                throw new ValidationException("Valid AttestId is required.");

            // Guard: constrain acceptable statuses if you want
            // var allowed = new[] { "Signed", "Finalized" };
            // if (status is not null && !allowed.Contains(status, StringComparer.OrdinalIgnoreCase))
            //    throw new ValidationException($"Invalid attestation status '{status}'.");

            // If your domain requires non-empty text to sign, you can enforce it here:
            // Load the attestation to validate text presence before finalizing.
            var current = await _repo.GetById(attestId);
            if (current is null)
                throw new ValidationException($"Attestation {attestId} was not found.");

            if (string.IsNullOrWhiteSpace(current.AttestText))
                throw new ValidationException("Attestation text is required to finalize/sign.");

            return await _repo.Finalize(attestId, signedDate, status);
        }

    }
}