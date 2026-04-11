using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Models;
using Services;

namespace Telebill.Repositories.Claims;

public partial class ClaimRepository
{
    public async Task<Claim?> GetByIdAsync(int claimID)
    {
        return await context.Claims.FindAsync(claimID);
    }

    public async Task<Claim?> GetByIdWithLinesAsync(int claimID)
    {
        return await context.Claims
            .Include(c => c.ClaimLines)
            .Include(c => c.ScrubIssues)
            .ThenInclude(i => i.Rule)
            .Include(c => c.X12837pRefs)
            .FirstOrDefaultAsync(c => c.ClaimId == claimID);
    }

    public async Task<Claim?> GetByEncounterIDAsync(int encounterID)
    {
        return await context.Claims.FirstOrDefaultAsync(c => c.EncounterId == encounterID);
    }

    public async Task<(List<Claim> claims, int totalCount)> GetClaimsPagedAsync(ClaimFilterParams filters)
    {
        var query = context.Claims
            .Include(c => c.Patient)
            .Include(c => c.Plan)
            .ThenInclude(p => p.Payer)
            .Include(c => c.Encounter)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.ClaimStatus))
        {
            query = query.Where(c => c.ClaimStatus == filters.ClaimStatus);
        }

        if (filters.PatientID.HasValue)
        {
            query = query.Where(c => c.PatientId == filters.PatientID);
        }

        if (filters.PlanID.HasValue)
        {
            query = query.Where(c => c.PlanId == filters.PlanID);
        }

        if (filters.ProviderID.HasValue)
        {
            query = query.Where(c => c.Encounter != null && c.Encounter.ProviderId == filters.ProviderID);
        }

        if (filters.DateFrom.HasValue)
        {
            query = query.Where(c => c.Encounter != null && c.Encounter.EncounterDateTime >= filters.DateFrom);
        }

        if (filters.DateTo.HasValue)
        {
            query = query.Where(c => c.Encounter != null && c.Encounter.EncounterDateTime <= filters.DateTo);
        }

        if (filters.HasScrubErrors.HasValue)
        {
            bool hasErrors = filters.HasScrubErrors.Value;
            query = query.Where(c =>
                context.ScrubIssues
                    .Join(context.ScrubRules, i => i.RuleId, r => r.RuleId, (i, r) => new { i, r })
                    .Any(j => j.i.ClaimId == c.ClaimId &&
                              j.i.Status == "Open" &&
                              j.r.Severity == "Error") == hasErrors);
        }

        var totalCount = await query.CountAsync();

        bool desc = string.Equals(filters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = filters.SortBy switch
        {
            "TotalCharge" => desc
                ? query.OrderByDescending(c => c.TotalCharge)
                : query.OrderBy(c => c.TotalCharge),
            "ClaimStatus" => desc
                ? query.OrderByDescending(c => c.ClaimStatus)
                : query.OrderBy(c => c.ClaimStatus),
            _ => desc
                ? query.OrderByDescending(c => c.CreatedDate)
                : query.OrderBy(c => c.CreatedDate)
        };

        var skip = (filters.Page - 1) * filters.PageSize;
        var claims = await query.Skip(skip).Take(filters.PageSize).ToListAsync();

        return (claims, totalCount);
    }

    public async Task<Claim> CreateAsync(Claim claim)
    {
        context.Claims.Add(claim);
        await context.SaveChangesAsync();
        return claim;
    }

    public async Task UpdateStatusAsync(int claimID, string newStatus)
    {
        var claim = await context.Claims.FindAsync(claimID);
        if (claim == null)
        {
            return;
        }

        claim.ClaimStatus = newStatus;
        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsForEncounterAsync(int encounterID)
    {
        return await context.Claims.AnyAsync(c => c.EncounterId == encounterID);
    }

    public async Task<List<ClaimLine>> GetLinesByClaimIDAsync(int claimID)
    {
        return await context.ClaimLines
            .Where(l => l.ClaimId == claimID)
            .ToListAsync();
    }

    public async Task CreateLinesAsync(List<ClaimLine> lines)
    {
        context.ClaimLines.AddRange(lines);
        await context.SaveChangesAsync();
    }

    public async Task<ClaimLine?> GetLineByIdAsync(int claimLineID)
    {
        return await context.ClaimLines.FindAsync(claimLineID);
    }
}

