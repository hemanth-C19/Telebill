using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.Coding
{
    public interface ICodingEncounterRepository
    {
        // Encounter
        Task<Encounter?> GetEncounterByIdAsync(int encounterId);
        Task<List<Encounter>> GetEncountersByProviderAsync(int providerId, string? status);
        Task<List<Encounter>> GetReadyForCodingEncountersAsync(int? providerId, int? planId);
        Task UpdateEncounterStatusAsync(int encounterId, string newStatus);
        Task UpdateEncounterPosAsync(int encounterId, string pos);
        Task UpdateEncounterDocumentationUriAsync(int encounterId, string uri);

        // ChargeLine
        Task<List<ChargeLine>> GetChargeLinesByEncounterAsync(int encounterId);
        Task<ChargeLine?> GetChargeLineByIdAsync(int chargeId);
        Task UpdateChargeLineModifiersAsync(int chargeId, string modifiersJson);

        // Attestation
        Task<Attestation?> GetAttestedAttestationAsync(int encounterId);

        // Provider
        Task<Provider?> GetProviderByIdAsync(int providerId);

        // Patient
        Task<Patient?> GetPatientByIdAsync(int patientId);

        // Coverage + PayerPlan
        Task<Coverage?> GetActiveCoverageForEncounterAsync(int patientId, DateTime encounterDateTime);
        Task<PayerPlan?> GetPayerPlanByIdAsync(int planId);
    }
}

