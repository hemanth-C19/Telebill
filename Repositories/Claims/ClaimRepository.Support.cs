using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Models;
 
namespace Telebill.Repositories.Claims;
 
public partial class ClaimRepository
{
    public Task<X12837pRef?> GetX12RefByClaimIDAsync(int claimID)
    {
        return _context.X12837pRefs.FirstOrDefaultAsync(x => x.ClaimId == claimID);
    }
 
    public async Task<X12837pRef> CreateX12RefAsync(X12837pRef x12Ref)
    {
        _context.X12837pRefs.Add(x12Ref);
        await _context.SaveChangesAsync();
        return x12Ref;
    }
 
    public async Task UpdateX12RefAsync(X12837pRef x12Ref)
    {
        _context.X12837pRefs.Update(x12Ref);
        await _context.SaveChangesAsync();
    }
 
    public Task<Encounter?> GetEncounterByIdAsync(int encounterID)
    {
        return _context.Encounters.FirstOrDefaultAsync(e => e.EncounterId == encounterID);
    }
 
    public Task<List<ChargeLine>> GetFinalizedChargeLinesByEncounterAsync(int encounterID)
    {
        return _context.ChargeLines
            .Where(c => c.EncounterId == encounterID && c.Status == "Finalized")
            .ToListAsync();
    }
 
    public Task<List<Diagnosis>> GetActiveDiagnosesByEncounterAsync(int encounterID)
    {
        return _context.Diagnoses
            .Where(d => d.EncounterId == encounterID && d.Status == "Active")
            .ToListAsync();
    }
 
    public Task<Coverage?> GetActiveCoverageForEncounterAsync(int patientID, DateTime encounterDate)
    {
        var encounterDateOnly = DateOnly.FromDateTime(encounterDate);
        return _context.Coverages
            .Where(c => c.PatientId == patientID &&
                        c.Status == "Active" &&
                        c.EffectiveFrom <= encounterDateOnly &&
                        (c.EffectiveTo == null || c.EffectiveTo >= encounterDateOnly))
            .OrderByDescending(c => c.EffectiveFrom)
            .FirstOrDefaultAsync();
    }
 
    public Task<PayerPlan?> GetPayerPlanByIdAsync(int planID)
    {
        return _context.PayerPlans
            .Include(p => p.Payer)
            .FirstOrDefaultAsync(p => p.PlanId == planID);
    }
 
    public Task<FeeSchedule?> GetFeeScheduleAsync(int planID, string cptHcpcs, string? modifierCombo, DateTime serviceDate)
    {
        var serviceDateOnly = DateOnly.FromDateTime(serviceDate);
        var query = _context.FeeSchedules
            .Where(f =>
                f.PlanId == planID &&
                f.CptHcpcs == cptHcpcs &&
                f.Status == "Active" &&
                f.EffectiveFrom <= serviceDateOnly &&
                (f.EffectiveTo == null || f.EffectiveTo >= serviceDateOnly));
 
        if (!string.IsNullOrWhiteSpace(modifierCombo))
        {
            query = query
                .OrderByDescending(f => f.ModifierCombo == modifierCombo)
                .ThenBy(f => f.ModifierCombo == null);
        }
 
        return query.FirstOrDefaultAsync();
    }
 
    public Task<Provider?> GetProviderByIdAsync(int providerID)
    {
        return _context.Providers.FirstOrDefaultAsync(p => p.ProviderId == providerID);
    }
 
    public Task<CodingLock?> GetActiveCodingLockAsync(int encounterID)
    {
        return _context.CodingLocks
            .FirstOrDefaultAsync(c => c.EncounterId == encounterID && c.Status == "Locked");
    }
 
    public Task<bool> HasApprovedPriorAuthAsync(int claimID)
    {
        return _context.PriorAuths.AnyAsync(p => p.ClaimId == claimID && p.Status == "Approved");
    }
 
}