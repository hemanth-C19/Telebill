using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telebill.Dto.Coding;
using Telebill.Models;
using Telebill.Repositories.Coding;

namespace Telebill.Services.Coding
{
    public class CoderWorklistService(
        ICodingEncounterRepository encounterRepo,
        IDiagnosisRepository diagnosisRepo,
        ICodingLockRepository lockRepo) : ICoderWorklistService
    {
        public async Task<List<CodingWorklistItemDto>> GetCodingWorklistAsync(int? providerId, int? planId)
        {
            var encounters = await encounterRepo.GetReadyForCodingEncountersAsync(providerId, planId);
            var list = new List<CodingWorklistItemDto>();

            foreach (var enc in encounters)
            {
                var patient = enc.PatientId.HasValue
                    ? await encounterRepo.GetPatientByIdAsync(enc.PatientId.Value)
                    : null;

                var provider = enc.ProviderId.HasValue
                    ? await encounterRepo.GetProviderByIdAsync(enc.ProviderId.Value)
                    : null;

                var chargeLines = await encounterRepo.GetChargeLinesByEncounterAsync(enc.EncounterId);
                var diagnoses = await diagnosisRepo.GetActiveDiagnosesByEncounterAsync(enc.EncounterId);

                var coverage = enc.PatientId.HasValue
                    ? await encounterRepo.GetCoverageByPatientIdAsync(enc.PatientId.Value)
                    : null;

                PayerPlan? plan = null;
                if (coverage?.PlanId.HasValue == true)
                {
                    plan = await encounterRepo.GetPayerPlanByIdAsync(coverage.PlanId.Value);
                }

                var dto = new CodingWorklistItemDto
                {
                    EncounterId = enc.EncounterId,
                    PatientName = patient?.Name,
                    ProviderId = enc.ProviderId,
                    ProviderName = provider?.Name,
                    EncounterDateTime = enc.EncounterDateTime,
                    VisitType = enc.VisitType,
                    PlanId = plan?.PlanId,
                    PlanName = plan?.PlanName,
                    ChargeLineCount = chargeLines.Count,
                    TotalCharge = chargeLines.Sum(c => c.ChargeAmount ?? 0m),
                    DiagnosisCount = diagnoses.Count,
                    HasPrimaryDiagnosis = diagnoses.Any(d => d.Sequence == 1),
                    Status = enc.Status
                };

                list.Add(dto);
            }

            return list;
        }

        public async Task<WorklistFiltersDto> GetWorklistFiltersAsync()
        {
            var (providers, plans) = await encounterRepo.GetWorklistFiltersAsync();

            return new WorklistFiltersDto
            {
                Providers = providers
                    .Select(p => new WorklistFilterOptionDto { Id = p.ProviderId, Name = p.Name ?? string.Empty })
                    .ToList(),
                Plans = plans
                    .Select(p => new WorklistFilterOptionDto { Id = p.PlanId, Name = p.PlanName ?? string.Empty })
                    .ToList()
            };
        }

        public async Task<CodingEncounterCardDto?> GetCodingEncounterCardAsync(int encounterId)
        {
            var enc = await encounterRepo.GetEncounterByIdAsync(encounterId);
            if (enc == null)
            {
                throw new KeyNotFoundException("Encounter not found");
            }

            var provider = enc.ProviderId.HasValue
                ? await encounterRepo.GetProviderByIdAsync(enc.ProviderId.Value)
                : null;

            var attestation = await encounterRepo.GetAttestedAttestationAsync(enc.EncounterId);
            var patient = enc.PatientId.HasValue
                ? await encounterRepo.GetPatientByIdAsync(enc.PatientId.Value)
                : null;

            var allChargeLines = await encounterRepo.GetChargeLinesByEncounterAsync(enc.EncounterId);
            var finalizedLines = allChargeLines.Where(c => c.Status == "Finalized").ToList();

            // Loose coverage for plan display (no date-window restriction)
            var displayCoverage = enc.PatientId.HasValue
                ? await encounterRepo.GetCoverageByPatientIdAsync(enc.PatientId.Value)
                : null;

            // Strict coverage for the warning flag (must be active within encounter date window)
            var activeCoverage = enc.PatientId.HasValue
                ? await encounterRepo.GetActiveCoverageForEncounterAsync(enc.PatientId.Value, enc.EncounterDateTime)
                : null;

            PayerPlan? plan = null;
            if (displayCoverage?.PlanId.HasValue == true)
            {
                plan = await encounterRepo.GetPayerPlanByIdAsync(displayCoverage.PlanId.Value);
            }

            var diagnoses = await diagnosisRepo.GetActiveDiagnosesByEncounterAsync(enc.EncounterId);
            var activeLockEntity = await lockRepo.GetActiveCodingLockAsync(encounterId);

            var acceptedModifiers = new List<string>();
            var requiredModifiers = new List<string>();

            if (plan?.TelehealthModifiersJson != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(plan.TelehealthModifiersJson);
                    if (doc.RootElement.TryGetProperty("accepted", out var acc))
                    {
                        acceptedModifiers = acc.Deserialize<List<string>>() ?? new List<string>();
                    }

                    if (doc.RootElement.TryGetProperty("required", out var req))
                    {
                        requiredModifiers = req.Deserialize<List<string>>() ?? new List<string>();
                    }
                }
                catch
                {
                    acceptedModifiers = new List<string>();
                    requiredModifiers = new List<string>();
                }
            }

            var chargeLineDtos = new List<ChargeLineInfoDto>();

            foreach (var line in finalizedLines)
            {
                var modifierList = new List<string>();
                if (!string.IsNullOrEmpty(line.Modifiers))
                {
                    try
                    {
                        modifierList = JsonSerializer.Deserialize<List<string>>(line.Modifiers) ?? new List<string>();
                    }
                    catch
                    {
                        modifierList = new List<string>();
                    }
                }

                var modifiersValid = acceptedModifiers.Count == 0 ||
                                     modifierList.All(m => acceptedModifiers.Contains(m));

                var dto = new ChargeLineInfoDto
                {
                    ChargeId = line.ChargeId,
                    CptHcpcs = line.CptHcpcs,
                    Modifiers = line.Modifiers,
                    ModifierList = modifierList,
                    Units = line.Units,
                    ChargeAmount = line.ChargeAmount,
                    Notes = line.Notes,
                    Status = line.Status,
                    ModifiersValid = modifiersValid
                };

                chargeLineDtos.Add(dto);
            }

            var diagnosisDtos = diagnoses
                .Select(d => new DiagnosisResultDto
                {
                    DxId = d.DxId,
                    EncounterId = d.EncounterId ?? 0,
                    ICD10Code = d.Icd10code,
                    Description = d.Description,
                    Sequence = d.Sequence ?? 0,
                    Status = d.Status
                })
                .ToList();

            var card = new CodingEncounterCardDto
            {
                EncounterId = enc.EncounterId,
                Status = enc.Status,
                EncounterDateTime = enc.EncounterDateTime,
                VisitType = enc.VisitType,
                Pos = enc.Pos,
                DocumentationUri = enc.DocumentationUri,
                Provider = provider == null
                    ? null
                    : new ProviderInfoDto
                    {
                        ProviderId = provider.ProviderId,
                        Name = provider.Name,
                        Npi = provider.Npi,
                        Taxonomy = provider.Taxonomy
                    },
                Attestation = attestation == null
                    ? null
                    : new AttestationInfoDto
                    {
                        AttestId = attestation.AttestId,
                        AttestText = attestation.AttestText,
                        AttestDate = attestation.AttestDate,
                        Status = attestation.Status
                    },
                Patient = patient == null
                    ? null
                    : new PatientInfoDto
                    {
                        PatientId = patient.PatientId,
                        Name = patient.Name,
                        Mrn = patient.Mrn,
                        Dob = patient.Dob,
                        Gender = patient.Gender
                    },
                ChargeLines = chargeLineDtos,
                PrimaryPayerPlan = plan == null
                    ? null
                    : new PayerPlanInfoDto
                    {
                        PlanId = plan.PlanId,
                        PlanName = plan.PlanName,
                        NetworkType = plan.NetworkType,
                        Posdefault = plan.Posdefault,
                        RequiredModifiers = requiredModifiers,
                        AcceptedModifiers = acceptedModifiers,
                        MemberID = displayCoverage?.MemberId
                    },
                CoverageWarning = activeCoverage == null,
                Diagnoses = diagnosisDtos,
                ActiveLock = activeLockEntity == null
                    ? null
                    : new CodingLockInfoDto
                    {
                        CodingLockId = activeLockEntity.CodingLockId,
                        CoderName = null,
                        LockedDate = activeLockEntity.LockedDate ?? DateTime.UtcNow,
                        Status = activeLockEntity.Status
                    }
            };

            return card;
        }

        public async Task<(bool success, string error)> UpdateEncounterPosAsync(int encounterId, UpdateEncounterPosDto dto, int userId)
        {
            var enc = await encounterRepo.GetEncounterByIdAsync(encounterId);
            if (enc == null)
            {
                throw new KeyNotFoundException("Encounter not found");
            }

            if (dto.Pos != "02" && dto.Pos != "10")
            {
                throw new ArgumentException("Invalid POS. Must be 02 or 10.");
            }

            await encounterRepo.UpdateEncounterPosAsync(encounterId, dto.Pos);
            return (true, string.Empty);
        }

        public async Task<(bool success, string error, ChargeLineInfoDto? updated)> UpdateChargeLineModifiersAsync(
            int encounterId,
            int chargeId,
            UpdateChargeLineModifiersDto dto,
            int userId)
        {
            var enc = await encounterRepo.GetEncounterByIdAsync(encounterId);
            if (enc == null)
            {
                throw new KeyNotFoundException("Encounter not found");
            }

            if (!string.Equals(enc.Status, "ReadyForCoding", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Encounter is not ReadyForCoding");
            }

            var chargeLine = await encounterRepo.GetChargeLineByIdAsync(chargeId);
            if (chargeLine == null)
            {
                throw new KeyNotFoundException("Charge line not found");
            }

            if (chargeLine.EncounterId != encounterId)
            {
                throw new ArgumentException("Charge line does not belong to this encounter");
            }

            if (dto.Modifiers.Count > 4)
            {
                throw new ArgumentException("Maximum 4 modifiers per charge line (CMS rule)");
            }

            var coverage = enc.PatientId.HasValue
                ? await encounterRepo.GetActiveCoverageForEncounterAsync(enc.PatientId.Value, enc.EncounterDateTime)
                : null;

            var acceptedModifiers = new List<string>();

            if (coverage != null && coverage.PlanId.HasValue)
            {
                var plan = await encounterRepo.GetPayerPlanByIdAsync(coverage.PlanId.Value);
                if (plan?.TelehealthModifiersJson != null)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(plan.TelehealthModifiersJson);
                        if (doc.RootElement.TryGetProperty("accepted", out var acc))
                        {
                            acceptedModifiers = acc.Deserialize<List<string>>() ?? new List<string>();
                        }
                    }
                    catch
                    {
                        acceptedModifiers = new List<string>();
                    }
                }
            }

            if (acceptedModifiers.Count > 0)
            {
                var invalid = dto.Modifiers
                    .Where(m => !acceptedModifiers.Contains(m))
                    .ToList();

                if (invalid.Any())
                {
                    throw new ArgumentException($"Modifiers not accepted: {string.Join(", ", invalid)}");
                }
            }

            var modJson = JsonSerializer.Serialize(dto.Modifiers);
            await encounterRepo.UpdateChargeLineModifiersAsync(chargeId, modJson);

            var updated = await encounterRepo.GetChargeLineByIdAsync(chargeId);
            if (updated == null)
            {
                throw new KeyNotFoundException("Charge line not found");
            }

            var modifierList = dto.Modifiers;
            var modifiersValid = acceptedModifiers.Count == 0 ||
                                 modifierList.All(m => acceptedModifiers.Contains(m));

            var dtoResult = new ChargeLineInfoDto
            {
                ChargeId = updated.ChargeId,
                CptHcpcs = updated.CptHcpcs,
                Modifiers = updated.Modifiers,
                ModifierList = modifierList,
                Units = updated.Units,
                ChargeAmount = updated.ChargeAmount,
                Notes = updated.Notes,
                Status = updated.Status,
                ModifiersValid = modifiersValid
            };

            return (true, string.Empty, dtoResult);
        }

        public async Task<(bool success, string error, DiagnosisResultDto? result)> AddDiagnosisAsync(AddDiagnosisDto dto, int userId)
        {
            var enc = await encounterRepo.GetEncounterByIdAsync(dto.EncounterId);
            if (enc == null)
            {
                throw new KeyNotFoundException("Encounter not found");
            }

            if (!string.Equals(enc.Status, "ReadyForCoding", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Encounter must be ReadyForCoding to add diagnoses");
            }

            var upperCode = dto.ICD10Code.Trim().ToUpperInvariant();

            var exists = await diagnosisRepo.DiagnosisCodeExistsActiveAsync(dto.EncounterId, upperCode);
            if (exists)
            {
                throw new InvalidOperationException($"Diagnosis {upperCode} already exists for this encounter");
            }

            var count = await diagnosisRepo.GetActiveDiagnosisCountAsync(dto.EncounterId);
            if (count >= 12)
            {
                throw new InvalidOperationException("Maximum 12 diagnoses reached");
            }

            int sequence;
            if (dto.Sequence.HasValue)
            {
                if (dto.Sequence.Value < 1 || dto.Sequence.Value > 12)
                {
                    throw new ArgumentException("Sequence must be between 1 and 12");
                }

                var taken = await diagnosisRepo.SequenceTakenAsync(dto.EncounterId, dto.Sequence.Value);
                if (taken)
                {
                    throw new InvalidOperationException($"Sequence {dto.Sequence} is already taken");
                }

                sequence = dto.Sequence.Value;
            }
            else
            {
                var maxSeq = await diagnosisRepo.GetMaxDiagnosisSequenceAsync(dto.EncounterId);
                sequence = maxSeq + 1;
                if (sequence > 12)
                {
                    throw new InvalidOperationException("Cannot exceed 12 active diagnoses (CMS limit)");
                }
            }

            var dx = new Diagnosis
            {
                EncounterId = dto.EncounterId,
                Icd10code = upperCode,
                Description = dto.Description,
                Sequence = sequence,
                Status = "Active"
            };

            dx = await diagnosisRepo.AddDiagnosisAsync(dx);

            var result = new DiagnosisResultDto
            {
                DxId = dx.DxId,
                EncounterId = dx.EncounterId ?? 0,
                ICD10Code = dx.Icd10code,
                Description = dx.Description,
                Sequence = dx.Sequence ?? 0,
                Status = dx.Status
            };

            return (true, string.Empty, result);
        }

        public async Task<List<DiagnosisResultDto>> GetDiagnosesByEncounterAsync(int encounterId)
        {
            var list = await diagnosisRepo.GetDiagnosesByEncounterAsync(encounterId);

            return list
                .Select(d => new DiagnosisResultDto
                {
                    DxId = d.DxId,
                    EncounterId = d.EncounterId ?? 0,
                    ICD10Code = d.Icd10code,
                    Description = d.Description,
                    Sequence = d.Sequence ?? 0,
                    Status = d.Status
                })
                .ToList();
        }

        public async Task<(bool success, string error, DiagnosisResultDto? result)> UpdateDiagnosisAsync(
            int dxId,
            UpdateDiagnosisDto dto,
            int userId)
        {
            var dx = await diagnosisRepo.GetDiagnosisByIdAsync(dxId);
            if (dx == null)
            {
                throw new KeyNotFoundException("Diagnosis not found");
            }

            if (dx.EncounterId == null)
            {
                throw new InvalidOperationException("Diagnosis is not linked to an encounter");
            }

            var enc = await encounterRepo.GetEncounterByIdAsync(dx.EncounterId.Value);
            if (enc == null)
            {
                throw new KeyNotFoundException("Encounter not found");
            }

            if (!string.Equals(enc.Status, "ReadyForCoding", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Encounter must be ReadyForCoding to update diagnoses");
            }

            if (!string.IsNullOrWhiteSpace(dto.ICD10Code))
            {
                var upper = dto.ICD10Code.Trim().ToUpperInvariant();

                var exists = await diagnosisRepo.DiagnosisCodeExistsActiveAsync(dx.EncounterId.Value, upper);
                if (exists && !string.Equals(upper, dx.Icd10code, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Diagnosis {upper} already exists for this encounter");
                }

                dx.Icd10code = upper;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                dx.Description = dto.Description;
            }

            if (dto.Sequence.HasValue)
            {
                if (dto.Sequence.Value < 1 || dto.Sequence.Value > 12)
                {
                    throw new ArgumentException("Sequence must be between 1 and 12");
                }

                var taken = await diagnosisRepo.SequenceTakenAsync(dx.EncounterId.Value, dto.Sequence.Value, dxId);
                if (taken)
                {
                    throw new InvalidOperationException($"Sequence {dto.Sequence} is already taken");
                }

                dx.Sequence = dto.Sequence.Value;
            }

            await diagnosisRepo.UpdateDiagnosisAsync(dx);

            var result = new DiagnosisResultDto
            {
                DxId = dx.DxId,
                EncounterId = dx.EncounterId ?? 0,
                ICD10Code = dx.Icd10code,
                Description = dx.Description,
                Sequence = dx.Sequence ?? 0,
                Status = dx.Status
            };

            return (true, string.Empty, result);
        }

        public async Task<(bool success, string error)> RemoveDiagnosisAsync(int dxId, int userId)
        {
            var dx = await diagnosisRepo.GetDiagnosisByIdAsync(dxId);
            if (dx == null)
            {
                throw new KeyNotFoundException("Diagnosis not found");
            }

            dx.Status = "Inactive";
            await diagnosisRepo.UpdateDiagnosisAsync(dx);

            return (true, string.Empty);
        }
    }
}

