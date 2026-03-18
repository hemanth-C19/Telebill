using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.Coding;
using Telebill.Repositories.Coding;

namespace Telebill.Services.Coding
{
    public class ProviderCodingService : IProviderCodingService
    {
        private readonly ICodingEncounterRepository _encounterRepo;

        public ProviderCodingService(ICodingEncounterRepository encounterRepo)
        {
            _encounterRepo = encounterRepo;
        }

        public async Task<List<ProviderEncounterSummaryDto>> GetProviderEncountersAsync(int providerId, string? status)
        {
            var encounters = await _encounterRepo.GetEncountersByProviderAsync(providerId, status);
            var result = new List<ProviderEncounterSummaryDto>();

            foreach (var enc in encounters)
            {
                var patient = enc.PatientId.HasValue
                    ? await _encounterRepo.GetPatientByIdAsync(enc.PatientId.Value)
                    : null;

                var chargeLines = await _encounterRepo.GetChargeLinesByEncounterAsync(enc.EncounterId);
                var attestation = await _encounterRepo.GetAttestedAttestationAsync(enc.EncounterId);

                var allChargesFinalized = chargeLines.All(c => c.Status == "Finalized");
                var totalCharge = chargeLines.Sum(c => c.ChargeAmount ?? 0m);

                var dto = new ProviderEncounterSummaryDto
                {
                    EncounterId = enc.EncounterId,
                    PatientId = enc.PatientId,
                    PatientName = patient?.Name,
                    EncounterDateTime = enc.EncounterDateTime,
                    VisitType = enc.VisitType,
                    Pos = enc.Pos,
                    DocumentationUri = enc.DocumentationUri,
                    Status = enc.Status,
                    HasAttestation = attestation != null,
                    AllChargesFinalized = allChargesFinalized,
                    ChargeLineCount = chargeLines.Count,
                    TotalCharge = totalCharge
                };

                dto.ReadyToHandOff = dto.HasAttestation && dto.AllChargesFinalized && dto.Status == "Open";

                result.Add(dto);
            }

            return result;
        }

        public async Task<ProviderEncounterSummaryDto?> GetProviderEncounterDetailAsync(int encounterId, int providerId)
        {
            var enc = await _encounterRepo.GetEncounterByIdAsync(encounterId);
            if (enc == null)
            {
                throw new KeyNotFoundException("Encounter not found");
            }

            if (enc.ProviderId != providerId)
            {
                throw new UnauthorizedAccessException("This encounter does not belong to you");
            }

            var list = await GetProviderEncountersAsync(providerId, null);
            return list.FirstOrDefault(e => e.EncounterId == encounterId)
                   ?? throw new KeyNotFoundException("Encounter not found");
        }

        public async Task<ProviderEncounterSummaryDto?> SetDocumentationUriAsync(int encounterId, SetDocumentationUriDto dto, int providerId)
        {
            var enc = await _encounterRepo.GetEncounterByIdAsync(encounterId);
            if (enc == null)
            {
                throw new KeyNotFoundException("Encounter not found");
            }

            if (enc.ProviderId != providerId)
            {
                throw new UnauthorizedAccessException("This encounter does not belong to you");
            }

            var uri = dto.DocumentationUri?.Trim() ?? string.Empty;
            await _encounterRepo.UpdateEncounterDocumentationUriAsync(encounterId, uri);

            return await GetProviderEncounterDetailAsync(encounterId, providerId);
        }

        public async Task<(bool success, string error)> MarkReadyForCodingAsync(int encounterId, int providerId)
        {
            var enc = await _encounterRepo.GetEncounterByIdAsync(encounterId);
            if (enc == null)
            {
                throw new KeyNotFoundException("Encounter not found");
            }

            if (enc.ProviderId != providerId)
            {
                throw new UnauthorizedAccessException("This encounter does not belong to you");
            }

            if (!string.Equals(enc.Status, "Open", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only Open encounters can be marked as ReadyForCoding");
            }

            var attestation = await _encounterRepo.GetAttestedAttestationAsync(encounterId);
            if (attestation == null)
            {
                throw new InvalidOperationException("No attested attestation found. Complete attestation first.");
            }

            var chargeLines = await _encounterRepo.GetChargeLinesByEncounterAsync(encounterId);
            var draftCount = chargeLines.Count(c => c.Status != "Finalized");
            if (draftCount > 0)
            {
                throw new InvalidOperationException($"{draftCount} charge line(s) are still in Draft. Finalize all charges first.");
            }

            await _encounterRepo.UpdateEncounterStatusAsync(encounterId, "ReadyForCoding");

            return (true, string.Empty);
        }
    }
}

