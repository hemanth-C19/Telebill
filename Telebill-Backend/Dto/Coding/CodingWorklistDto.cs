using System;
using System.Collections.Generic;

namespace Telebill.Dto.Coding
{
    public class CodingWorklistItemDto
    {
        public int EncounterId { get; set; }
        public string? PatientName { get; set; }
        public int? ProviderId { get; set; }
        public string? ProviderName { get; set; }
        public DateTime EncounterDateTime { get; set; }
        public string? VisitType { get; set; }
        public int? PlanId { get; set; }
        public string? PlanName { get; set; }
        public int ChargeLineCount { get; set; }
        public decimal TotalCharge { get; set; }
        public int DiagnosisCount { get; set; }
        public bool HasPrimaryDiagnosis { get; set; }
        public string? Status { get; set; }
    }

    public class WorklistFilterOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class WorklistFiltersDto
    {
        public List<WorklistFilterOptionDto> Providers { get; set; } = new();
        public List<WorklistFilterOptionDto> Plans { get; set; } = new();
    }

    public class CodingEncounterCardDto
    {
        public int EncounterId { get; set; }
        public string? Status { get; set; }
        public DateTime EncounterDateTime { get; set; }
        public string? VisitType { get; set; }
        public string? Pos { get; set; }
        public string? DocumentationUri { get; set; }

        public ProviderInfoDto? Provider { get; set; }
        public AttestationInfoDto? Attestation { get; set; }
        public PatientInfoDto? Patient { get; set; }
        public List<ChargeLineInfoDto> ChargeLines { get; set; } = new();
        public PayerPlanInfoDto? PrimaryPayerPlan { get; set; }
        public bool CoverageWarning { get; set; }
        public List<DiagnosisResultDto> Diagnoses { get; set; } = new();
        public CodingLockInfoDto? ActiveLock { get; set; }
    }

    public class ProviderInfoDto
    {
        public int ProviderId { get; set; }
        public string? Name { get; set; }
        public string? Npi { get; set; }
        public string? Taxonomy { get; set; }
    }

    public class AttestationInfoDto
    {
        public int AttestId { get; set; }
        public string? AttestText { get; set; }
        public DateTime? AttestDate { get; set; }
        public string? Status { get; set; }
    }

    public class PatientInfoDto
    {
        public int PatientId { get; set; }
        public string? Name { get; set; }
        public string? Mrn { get; set; }
        public DateOnly Dob { get; set; }
        public string? Gender { get; set; }
    }

    public class ChargeLineInfoDto
    {
        public int ChargeId { get; set; }
        public string? CptHcpcs { get; set; }
        public string? Modifiers { get; set; }
        public List<string> ModifierList { get; set; } = new();
        public int? Units { get; set; }
        public decimal? ChargeAmount { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public bool ModifiersValid { get; set; }
    }

    public class PayerPlanInfoDto
    {
        public int PlanId { get; set; }
        public string? PlanName { get; set; }
        public string? NetworkType { get; set; }
        public string? Posdefault { get; set; }
        public List<string> RequiredModifiers { get; set; } = new();
        public List<string> AcceptedModifiers { get; set; } = new();
        public string? MemberID { get; set; }
    }

    public class CodingLockInfoDto
    {
        public int CodingLockId { get; set; }
        public string? CoderName { get; set; }
        public DateTime LockedDate { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateEncounterPosDto
    {
        public string Pos { get; set; } = string.Empty;
    }

    public class UpdateChargeLineModifiersDto
    {
        public List<string> Modifiers { get; set; } = new();
    }
}

