using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.Coding;
using Telebill.Models;
using Telebill.Repositories.Coding;

namespace Telebill.Services.Coding
{
    public class CodingLockService : ICodingLockService
    {
        private readonly ICodingEncounterRepository _encounterRepo;
        private readonly IDiagnosisRepository _diagnosisRepo;
        private readonly ICodingLockRepository _lockRepo;

        public CodingLockService(
            ICodingEncounterRepository encounterRepo,
            IDiagnosisRepository diagnosisRepo,
            ICodingLockRepository lockRepo)
        {
            _encounterRepo = encounterRepo;
            _diagnosisRepo = diagnosisRepo;
            _lockRepo = lockRepo;
        }

        public async Task<CodingValidationResultDto> ValidateCodingLockAsync(int encounterId)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            var enc = await _encounterRepo.GetEncounterByIdAsync(encounterId);
            if (enc == null)
            {
                errors.Add("Encounter not found");
            }
            else
            {
                if (!string.Equals(enc.Status, "ReadyForCoding", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add("Encounter is not in ReadyForCoding status");
                }

                var existingLock = await _lockRepo.GetActiveCodingLockAsync(encounterId);
                if (existingLock != null)
                {
                    errors.Add("Encounter is already locked");
                }

                var activeDx = await _diagnosisRepo.GetActiveDiagnosesByEncounterAsync(encounterId);

                if (!activeDx.Any(d => d.Sequence == 1))
                {
                    errors.Add("No primary diagnosis (Sequence 1) assigned");
                }

                if (activeDx.Count == 0)
                {
                    errors.Add("No diagnoses entered");
                }

                if (activeDx.Count > 12)
                {
                    errors.Add($"Too many diagnoses ({activeDx.Count}). Max 12.");
                }

                var dupes = activeDx
                    .GroupBy(d => d.Icd10code)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (dupes.Any())
                {
                    errors.Add($"Duplicate codes: {string.Join(", ", dupes)}");
                }

                if (enc.Pos != "02" && enc.Pos != "10")
                {
                    errors.Add($"Invalid POS '{enc.Pos}'. Must be 02 or 10.");
                }

                var attest = await _encounterRepo.GetAttestedAttestationAsync(encounterId);
                if (attest == null)
                {
                    errors.Add("No attested attestation found");
                }

                var chargeLines = await _encounterRepo.GetChargeLinesByEncounterAsync(encounterId);
                var draftCount = chargeLines.Count(c => c.Status == "Draft");
                if (draftCount > 0)
                {
                    errors.Add($"{draftCount} charge line(s) still in Draft");
                }

                var coverage = enc.PatientId.HasValue
                    ? await _encounterRepo.GetActiveCoverageForEncounterAsync(enc.PatientId.Value, enc.EncounterDateTime)
                    : null;

                if (coverage == null)
                {
                    warnings.Add("No active coverage found. Claim will fail scrub in Module 6.");
                }

                if (string.IsNullOrEmpty(enc.DocumentationUri))
                {
                    warnings.Add("No DocumentationUri set on this encounter.");
                }
            }

            return new CodingValidationResultDto
            {
                EncounterId = encounterId,
                CanLock = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }

        public async Task<ApplyCodingLockResponseDto> ApplyCodingLockAsync(ApplyCodingLockDto dto, int userId)
        {
            var validation = await ValidateCodingLockAsync(dto.EncounterId);
            if (!validation.CanLock)
            {
                return new ApplyCodingLockResponseDto
                {
                    EncounterStatus = "ReadyForCoding",
                    ClaimBuildTriggered = false,
                    ValidationErrors = validation.Errors
                };
            }

            var lockEntity = new CodingLock
            {
                EncounterId = dto.EncounterId,
                CoderId = userId,
                LockedDate = DateTime.UtcNow,
                Notes = dto.Notes,
                Status = "Locked"
            };

            lockEntity = await _lockRepo.AddCodingLockAsync(lockEntity);
            await _encounterRepo.UpdateEncounterStatusAsync(dto.EncounterId, "Finalized");

            // TODO: trigger claim build in Module 6
            var claimBuildTriggered = false;

            var result = new CodingLockResultDto
            {
                CodingLockId = lockEntity.CodingLockId,
                EncounterId = lockEntity.EncounterId ?? 0,
                CoderId = lockEntity.CoderId ?? 0,
                CoderName = null,
                LockedDate = lockEntity.LockedDate ?? DateTime.UtcNow,
                Notes = lockEntity.Notes,
                Status = lockEntity.Status
            };

            return new ApplyCodingLockResponseDto
            {
                CodingLock = result,
                EncounterStatus = "Finalized",
                ClaimBuildTriggered = claimBuildTriggered,
                ValidationErrors = new List<string>()
            };
        }

        public async Task<(bool success, string error, UnlockCodingResponseDto? result)> UnlockCodingAsync(
            UnlockCodingDto dto,
            int userId)
        {
            var enc = await _encounterRepo.GetEncounterByIdAsync(dto.EncounterId);
            if (enc == null)
            {
                return (false, "Encounter not found", null);
            }

            var activeLock = await _lockRepo.GetActiveCodingLockAsync(dto.EncounterId);
            if (activeLock == null)
            {
                return (false, "Encounter is not currently locked", null);
            }

            var claimStatus = await _lockRepo.GetClaimStatusForEncounterAsync(dto.EncounterId);
            if (!string.IsNullOrEmpty(claimStatus) &&
                !string.Equals(claimStatus, "Draft", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(claimStatus, "ScrubError", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Cannot unlock — claim is already in progress. Contact Admin.", null);
            }

            activeLock.Status = "Unlocked";
            await _lockRepo.UpdateCodingLockAsync(activeLock);
            await _encounterRepo.UpdateEncounterStatusAsync(dto.EncounterId, "ReadyForCoding");

            var result = new UnlockCodingResponseDto
            {
                EncounterId = dto.EncounterId,
                EncounterStatus = "ReadyForCoding",
                PreviousLockId = activeLock.CodingLockId
            };

            return (true, string.Empty, result);
        }

        public async Task<List<CodingLockResultDto>> GetCodingLockHistoryAsync(int encounterId)
        {
            var list = await _lockRepo.GetCodingLockHistoryAsync(encounterId);

            return list
                .Select(cl => new CodingLockResultDto
                {
                    CodingLockId = cl.CodingLockId,
                    EncounterId = cl.EncounterId ?? 0,
                    CoderId = cl.CoderId ?? 0,
                    CoderName = null,
                    LockedDate = cl.LockedDate ?? DateTime.UtcNow,
                    Notes = cl.Notes,
                    Status = cl.Status
                })
                .ToList();
        }
    }
}

