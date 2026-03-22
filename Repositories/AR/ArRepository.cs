using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Dto.AR;
using Telebill.Models;

namespace Telebill.Repositories.AR;

public class ArRepository(TeleBillContext context) : IArRepository
{
    // ── DENIAL ───────────────────────────────────────────────────

    public async Task<List<Denial>> GetDenialsAsync(ArWorklistFilterParams filters)
    {
        var query = context.Denials.AsQueryable();

        if (!string.IsNullOrEmpty(filters.DenialStatus))
        {
            query = query.Where(d => d.Status == filters.DenialStatus);
        }

        if (!string.IsNullOrEmpty(filters.ReasonCode))
        {
            query = query.Where(d => d.ReasonCode == filters.ReasonCode);
        }

        if (filters.DenialDateFrom.HasValue)
        {
            var from = DateOnly.FromDateTime(filters.DenialDateFrom.Value);
            query = query.Where(d => d.DenialDate >= from);
        }

        if (filters.DenialDateTo.HasValue)
        {
            var to = DateOnly.FromDateTime(filters.DenialDateTo.Value);
            query = query.Where(d => d.DenialDate <= to);
        }

        if (filters.PayerId.HasValue)
        {
            var planIds = await context.PayerPlans
                .Where(pp => pp.PayerId == filters.PayerId.Value)
                .Select(pp => pp.PlanId)
                .ToListAsync();

            var claimIds = await context.Claims
                .Where(c => c.PlanId.HasValue && planIds.Contains(c.PlanId.Value))
                .Select(c => c.ClaimId)
                .ToListAsync();

            query = query.Where(d => d.ClaimId.HasValue && claimIds.Contains(d.ClaimId.Value));
        }

        var denials = await query
            .OrderBy(d => d.DenialDate)
            .ToListAsync();

        if (!string.IsNullOrEmpty(filters.AgingBucket))
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            denials = denials.Where(d =>
            {
                if (d.DenialDate == null) return false;
                var days = today.DayNumber - d.DenialDate.Value.DayNumber;
                return filters.AgingBucket switch
                {
                    "0-30" => days >= 0 && days <= 30,
                    "31-60" => days >= 31 && days <= 60,
                    "61-90" => days >= 61 && days <= 90,
                    "90+" => days > 90,
                    _ => true
                };
            }).ToList();
        }

        return denials;
    }

    public Task<Denial?> GetDenialByIdAsync(int denialId)
    {
        return context.Denials
            .FirstOrDefaultAsync(d => d.DenialId == denialId);
    }

    public Task<List<Denial>> GetDenialsByClaimIdAsync(int claimId)
    {
        return context.Denials
            .Where(d => d.ClaimId == claimId)
            .OrderByDescending(d => d.DenialDate)
            .ToListAsync();
    }

    public async Task UpdateDenialAsync(Denial denial)
    {
        context.Denials.Update(denial);
        await context.SaveChangesAsync();
    }

    public async Task<Denial> AddDenialAsync(Denial denial)
    {
        await context.Denials.AddAsync(denial);
        await context.SaveChangesAsync();
        return denial;
    }

    // ── CLAIM ─────────────────────────────────────────────────────

    public Task<Claim?> GetClaimByIdAsync(int claimId)
    {
        return context.Claims
            .FirstOrDefaultAsync(c => c.ClaimId == claimId);
    }

    public Task<List<ClaimLine>> GetClaimLinesByClaimIdAsync(int claimId)
    {
        return context.ClaimLines
            .Where(cl => cl.ClaimId == claimId)
            .OrderBy(cl => cl.LineNo)
            .ToListAsync();
    }

    public async Task UpdateClaimStatusAsync(int claimId, string newStatus)
    {
        var claim = await context.Claims.FindAsync(claimId);
        if (claim != null)
        {
            claim.ClaimStatus = newStatus;
            await context.SaveChangesAsync();
        }
    }

    // ── PAYMENT POST ─────────────────────────────────────────────

    public Task<List<PaymentPost>> GetPaymentPostsByClaimIdAsync(int claimId)
    {
        return context.PaymentPosts
            .Where(pp => pp.ClaimId == claimId && pp.Status == "Active")
            .OrderBy(pp => pp.PostedDate)
            .ToListAsync();
    }

    // ── SUBMISSION HISTORY ────────────────────────────────────────

    public Task<List<SubmissionRef>> GetSubmissionRefsByClaimIdAsync(int claimId)
    {
        return context.SubmissionRefs
            .Where(sr => sr.ClaimId == claimId)
            .OrderByDescending(sr => sr.SubmitDate)
            .ToListAsync();
    }

    // ── ATTACHMENT ────────────────────────────────────────────────

    public Task<List<AttachmentRef>> GetAttachmentsByClaimIdAsync(int claimId)
    {
        return context.AttachmentRefs
            .Where(a => a.ClaimId == claimId && a.Status != "Deleted")
            .OrderByDescending(a => a.UploadedDate)
            .ToListAsync();
    }

    public async Task<AttachmentRef> AddAttachmentAsync(AttachmentRef attachment)
    {
        await context.AttachmentRefs.AddAsync(attachment);
        await context.SaveChangesAsync();
        return attachment;
    }

    // ── FEE SCHEDULE ──────────────────────────────────────────────

    public Task<FeeSchedule?> GetFeeScheduleAsync(int planId, string cptHcpcs,
                                                  string? modifierCombo, DateOnly serviceDate)
    {
        var query = context.FeeSchedules
            .Where(f =>
                f.PlanId == planId &&
                f.CptHcpcs == cptHcpcs &&
                f.Status == "Active" &&
                f.EffectiveFrom <= serviceDate &&
                (f.EffectiveTo == null || f.EffectiveTo >= serviceDate));

        if (!string.IsNullOrWhiteSpace(modifierCombo))
        {
            query = query.Where(f => f.ModifierCombo == modifierCombo);
        }

        return query.FirstOrDefaultAsync();
    }

    // ── CROSS-MODULE READS ────────────────────────────────────────

    public async Task<Encounter?> GetEncounterByClaimIdAsync(int claimId)
    {
        var claim = await context.Claims
            .FirstOrDefaultAsync(c => c.ClaimId == claimId);
        if (claim == null || !claim.EncounterId.HasValue) return null;

        return await context.Encounters
            .FirstOrDefaultAsync(e => e.EncounterId == claim.EncounterId.Value);
    }

    public Task<Patient?> GetPatientByIdAsync(int patientId)
    {
        return context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == patientId);
    }

    public async Task<Payer?> GetPayerByPlanIdAsync(int planId)
    {
        var plan = await context.PayerPlans
            .FirstOrDefaultAsync(pp => pp.PlanId == planId);
        if (plan == null || !plan.PayerId.HasValue) return null;

        return await context.Payers
            .FirstOrDefaultAsync(p => p.PayerId == plan.PayerId.Value);
    }

    public Task<PayerPlan?> GetPayerPlanByIdAsync(int planId)
    {
        return context.PayerPlans
            .FirstOrDefaultAsync(pp => pp.PlanId == planId);
    }

    public Task<List<Claim>> GetPartiallyPaidClaimsAsync()
    {
        return context.Claims
            .Where(c => c.ClaimStatus == "PartiallyPaid")
            .ToListAsync();
    }

    // ── DASHBOARD ─────────────────────────────────────────────────

    public Task<List<Denial>> GetAllOpenDenialsAsync()
    {
        return context.Denials
            .Where(d => d.Status == "Open" || d.Status == "Appealed")
            .ToListAsync();
    }

    public async Task<int> GetTotalClaimsSubmittedByPayerAsync(int payerId)
    {
        var planIds = await context.PayerPlans
            .Where(pp => pp.PayerId == payerId)
            .Select(pp => pp.PlanId)
            .ToListAsync();

        return await context.Claims
            .Where(c => c.PlanId.HasValue && planIds.Contains(c.PlanId.Value))
            .CountAsync();
    }

    // ── SAVE ──────────────────────────────────────────────────────

    public Task SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }
}
