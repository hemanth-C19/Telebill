using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Dto.Reports;
using Telebill.Models;

namespace Telebill.Repositories.Reports;

public class ReportQueryRepository : IReportQueryRepository
{
    private readonly TeleBillContext _context;

    public ReportQueryRepository(TeleBillContext context)
    {
        _context = context;
    }

    public async Task<List<Claim>> GetClaimsForPeriodAsync(
        DateTime from, DateTime to, int? payerId, int? planId, int? providerId)
    {
        var query = _context.Claims.AsQueryable();

        query = query.Where(c => c.CreatedDate >= from && c.CreatedDate <= to);

        if (payerId.HasValue)
        {
            var planIds = await _context.PayerPlans
                .Where(pp => pp.PayerId == payerId.Value)
                .Select(pp => pp.PlanId)
                .ToListAsync();
            query = query.Where(c => c.PlanId.HasValue && planIds.Contains(c.PlanId.Value));
        }

        if (planId.HasValue)
        {
            query = query.Where(c => c.PlanId == planId.Value);
        }

        if (providerId.HasValue)
        {
            var encounterIds = await _context.Encounters
                .Where(e => e.ProviderId == providerId.Value)
                .Select(e => e.EncounterId)
                .ToListAsync();
            query = query.Where(c => c.EncounterId.HasValue && encounterIds.Contains(c.EncounterId.Value));
        }

        return await query.ToListAsync();
    }

    public Task<List<ScrubIssue>> GetScrubIssuesByClaimIdsAsync(List<int> claimIds)
    {
        return _context.ScrubIssues
            .Where(si => si.ClaimId.HasValue && claimIds.Contains(si.ClaimId.Value))
            .ToListAsync();
    }

    public Task<ScrubRule?> GetScrubRuleByIdAsync(int ruleId)
    {
        return _context.ScrubRules
            .FirstOrDefaultAsync(r => r.RuleId == ruleId);
    }

    public Task<List<SubmissionRef>> GetSubmissionRefsByClaimIdsAsync(List<int> claimIds)
    {
        return _context.SubmissionRefs
            .Where(sr => sr.ClaimId.HasValue && claimIds.Contains(sr.ClaimId.Value))
            .ToListAsync();
    }

    public Task<List<PaymentPost>> GetPaymentPostsByClaimIdsAsync(List<int> claimIds)
    {
        return _context.PaymentPosts
            .Where(pp => pp.ClaimId.HasValue && claimIds.Contains(pp.ClaimId.Value))
            .ToListAsync();
    }

    public Task<List<Encounter>> GetEncountersByIdsAsync(List<int?> encounterIds)
    {
        var ids = encounterIds.Where(id => id.HasValue).Select(id => id!.Value).ToList();
        return _context.Encounters
            .Where(e => ids.Contains(e.EncounterId))
            .ToListAsync();
    }

    public Task<List<Denial>> GetDenialsByClaimIdsAsync(List<int> claimIds)
    {
        return _context.Denials
            .Where(d => d.ClaimId.HasValue && claimIds.Contains(d.ClaimId.Value))
            .ToListAsync();
    }

    public Task<Patient?> GetPatientByIdAsync(int? patientId)
    {
        if (!patientId.HasValue)
        {
            return Task.FromResult<Patient?>(null);
        }

        return _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId.Value);
    }

    public Task<Provider?> GetProviderByIdAsync(int providerId)
    {
        return _context.Providers.FirstOrDefaultAsync(p => p.ProviderId == providerId);
    }

    public Task<Payer?> GetPayerByIdAsync(int payerId)
    {
        return _context.Payers.FirstOrDefaultAsync(p => p.PayerId == payerId);
    }

    public Task<PayerPlan?> GetPlanByIdAsync(int planId)
    {
        return _context.PayerPlans.FirstOrDefaultAsync(pp => pp.PlanId == planId);
    }

    public async Task<int?> GetPayerIdByPlanIdAsync(int? planId)
    {
        if (!planId.HasValue)
        {
            return null;
        }

        var plan = await _context.PayerPlans.FirstOrDefaultAsync(pp => pp.PlanId == planId.Value);
        return plan?.PayerId;
    }

    public async Task<int?> GetProviderIdByEncounterIdAsync(int? encounterId)
    {
        if (!encounterId.HasValue)
        {
            return null;
        }

        var enc = await _context.Encounters.FirstOrDefaultAsync(e => e.EncounterId == encounterId.Value);
        return enc?.ProviderId;
    }

    public async Task<List<Claim>> GetClaimsForExportAsync(ExportFilterParams filters)
    {
        var query = _context.Claims.AsQueryable();

        if (filters.DateFrom.HasValue)
            query = query.Where(c => c.CreatedDate >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            query = query.Where(c => c.CreatedDate <= filters.DateTo.Value);
        if (filters.PlanId.HasValue)
            query = query.Where(c => c.PlanId == filters.PlanId.Value);
        if (!string.IsNullOrEmpty(filters.Status))
            query = query.Where(c => c.ClaimStatus == filters.Status);
        if (filters.PayerId.HasValue)
        {
            var planIds = await _context.PayerPlans
                .Where(pp => pp.PayerId == filters.PayerId.Value)
                .Select(pp => pp.PlanId).ToListAsync();
            query = query.Where(c => c.PlanId.HasValue && planIds.Contains(c.PlanId.Value));
        }
        if (filters.ProviderId.HasValue)
        {
            var encounterIds = await _context.Encounters
                .Where(e => e.ProviderId == filters.ProviderId.Value)
                .Select(e => e.EncounterId).ToListAsync();
            query = query.Where(c => c.EncounterId.HasValue && encounterIds.Contains(c.EncounterId.Value));
        }

        return await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
    }

    public async Task<List<ScrubIssue>> GetScrubIssuesForExportAsync(ExportFilterParams filters)
    {
        var query = _context.ScrubIssues.AsQueryable();

        if (filters.DateFrom.HasValue)
            query = query.Where(si => si.DetectedDate >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            query = query.Where(si => si.DetectedDate <= filters.DateTo.Value);
        if (!string.IsNullOrEmpty(filters.Status))
            query = query.Where(si => si.Status == filters.Status);

        return await query.OrderByDescending(si => si.DetectedDate).ToListAsync();
    }

    public async Task<List<Denial>> GetDenialsForExportAsync(ExportFilterParams filters)
    {
        var query = _context.Denials.AsQueryable();

        if (filters.DateFrom.HasValue)
            query = query.Where(d => d.DenialDate >= DateOnly.FromDateTime(filters.DateFrom.Value));
        if (filters.DateTo.HasValue)
            query = query.Where(d => d.DenialDate <= DateOnly.FromDateTime(filters.DateTo.Value));
        if (!string.IsNullOrEmpty(filters.Status))
            query = query.Where(d => d.Status == filters.Status);
        if (filters.PayerId.HasValue)
        {
            var planIds = await _context.PayerPlans
                .Where(pp => pp.PayerId == filters.PayerId.Value)
                .Select(pp => pp.PlanId).ToListAsync();
            var claimIds = await _context.Claims
                .Where(c => c.PlanId.HasValue && planIds.Contains(c.PlanId.Value))
                .Select(c => c.ClaimId).ToListAsync();
            query = query.Where(d => d.ClaimId.HasValue && claimIds.Contains(d.ClaimId.Value));
        }

        return await query.OrderBy(d => d.DenialDate).ToListAsync();
    }

    public async Task<List<Statement>> GetStatementsForExportAsync(ExportFilterParams filters)
    {
        var query = _context.Statements.AsQueryable();

        if (filters.DateFrom.HasValue)
            query = query.Where(s => s.GeneratedDate >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            query = query.Where(s => s.GeneratedDate <= filters.DateTo.Value);
        if (!string.IsNullOrEmpty(filters.Status))
            query = query.Where(s => s.Status == filters.Status);

        return await query.OrderByDescending(s => s.GeneratedDate).ToListAsync();
    }

    public async Task<List<RemitRef>> GetRemitRefsForExportAsync(ExportFilterParams filters)
    {
        var query = _context.RemitRefs.AsQueryable();

        if (filters.DateFrom.HasValue)
            query = query.Where(r => r.ReceivedDate >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            query = query.Where(r => r.ReceivedDate <= filters.DateTo.Value);
        if (filters.PayerId.HasValue)
            query = query.Where(r => r.PayerId == filters.PayerId.Value);
        if (!string.IsNullOrEmpty(filters.Status))
            query = query.Where(r => r.Status == filters.Status);

        return await query.OrderByDescending(r => r.ReceivedDate).ToListAsync();
    }

    public async Task<decimal> GetTotalPostedByRemitIdAsync(int remitId)
    {
        var remit = await _context.RemitRefs.FirstOrDefaultAsync(r => r.RemitId == remitId);
        if (remit == null || !remit.BatchId.HasValue)
        {
            return 0m;
        }

        var claimIds = await _context.SubmissionRefs
            .Where(sr => sr.BatchId == remit.BatchId.Value)
            .Select(sr => sr.ClaimId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToListAsync();

        return await _context.PaymentPosts
            .Where(pp => pp.ClaimId.HasValue && claimIds.Contains(pp.ClaimId.Value))
            .SumAsync(pp => pp.AmountPaid ?? 0m);
    }
}

