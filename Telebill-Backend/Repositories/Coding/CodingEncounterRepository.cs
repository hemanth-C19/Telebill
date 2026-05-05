using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Models;

namespace Telebill.Repositories.Coding
{
    public class CodingEncounterRepository(TeleBillContext context) : ICodingEncounterRepository
    {
        public async Task<Encounter?> GetEncounterByIdAsync(int encounterId)
        {
            return await context.Encounters.FindAsync(encounterId);
        }

        public async Task<List<Encounter>> GetEncountersByProviderAsync(int providerId, string? status)
        {
            var query = context.Encounters
                .Where(e => e.ProviderId == providerId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(e => e.Status == status);
            }

            return await query
                .OrderByDescending(e => e.EncounterDateTime)
                .ToListAsync();
        }

        public async Task<List<Encounter>> GetReadyForCodingEncountersAsync(int? providerId, int? planId)
        {
            var query = context.Encounters
                .Where(e => e.Status == "ReadyForCoding");

            if (providerId.HasValue)
            {
                query = query.Where(e => e.ProviderId == providerId.Value);
            }

            if (planId.HasValue)
            {
                var patientIds = await context.Coverages
                    .Where(c => c.PlanId == planId.Value && c.Status == "Active")
                    .Select(c => c.PatientId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(e => patientIds.Contains(e.PatientId));
            }

            return await query
                .OrderBy(e => e.EncounterDateTime)
                .ToListAsync();
        }

        public async Task UpdateEncounterStatusAsync(int encounterId, string newStatus)
        {
            var enc = await context.Encounters.FindAsync(encounterId);
            if (enc == null) return;

            enc.Status = newStatus;
            await context.SaveChangesAsync();
        }

        public async Task UpdateEncounterPosAsync(int encounterId, string pos)
        {
            var enc = await context.Encounters.FindAsync(encounterId);
            if (enc == null) return;

            enc.Pos = pos;
            await context.SaveChangesAsync();
        }

        public async Task UpdateEncounterDocumentationUriAsync(int encounterId, string uri)
        {
            var enc = await context.Encounters.FindAsync(encounterId);
            if (enc == null) return;

            enc.DocumentationUri = uri;
            await context.SaveChangesAsync();
        }

        public async Task<List<ChargeLine>> GetChargeLinesByEncounterAsync(int encounterId)
        {
            return await context.ChargeLines
                .Where(c => c.EncounterId == encounterId)
                .ToListAsync();
        }

        public async Task<ChargeLine?> GetChargeLineByIdAsync(int chargeId)
        {
            return await context.ChargeLines.FindAsync(chargeId);
        }

        public async Task UpdateChargeLineModifiersAsync(int chargeId, string modifiersJson)
        {
            var line = await context.ChargeLines.FindAsync(chargeId);
            if (line == null) return;

            line.Modifiers = modifiersJson;
            await context.SaveChangesAsync();
        }

        public async Task<Attestation?> GetAttestedAttestationAsync(int encounterId)
        {
            return await context.Attestations
                .Where(a => a.EncounterId == encounterId && a.Status == "Attested")
                .OrderByDescending(a => a.AttestDate)
                .FirstOrDefaultAsync();
        }

        public async Task<Provider?> GetProviderByIdAsync(int providerId)
        {
            return await context.Providers.FindAsync(providerId);
        }

        public async Task<Patient?> GetPatientByIdAsync(int patientId)
        {
            return await context.Patients.FindAsync(patientId);
        }

        public async Task<Coverage?> GetActiveCoverageForEncounterAsync(int patientId, DateTime encounterDate)
        {
            var encounterDateOnly = DateOnly.FromDateTime(encounterDate);
            return await context.Coverages
                .Where(c =>
                    c.PatientId == patientId &&
                    c.Status == "Active" &&
                    c.EffectiveFrom <= encounterDateOnly &&
                    (c.EffectiveTo == null || c.EffectiveTo >= encounterDateOnly))
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<Coverage?> GetCoverageByPatientIdAsync(int patientId)
        {
            return await context.Coverages
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.Status == "Active")
                .ThenByDescending(c => c.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<PayerPlan?> GetPayerPlanByIdAsync(int planId)
        {
            return await context.PayerPlans.FindAsync(planId);
        }

        public async Task<(List<Provider> Providers, List<PayerPlan> Plans)> GetWorklistFiltersAsync()
        {
            var providers = await context.Providers
                .OrderBy(p => p.Name)
                .ToListAsync();

            var plans = await context.PayerPlans
                .OrderBy(p => p.PlanName)
                .ToListAsync();

            return (providers, plans);
        }
    }
}

