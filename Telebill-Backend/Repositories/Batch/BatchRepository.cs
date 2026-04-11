using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Models;

namespace Telebill.Repositories.Batch;

public class BatchRepository(TeleBillContext context) : IBatchRepository
{
    public Task<SubmissionBatch?> GetBatchByIdAsync(int batchID)
    {
        return context.SubmissionBatches.FirstOrDefaultAsync(b => b.BatchId == batchID);
    }

    public async Task<(List<SubmissionBatch> batches, int totalCount)> GetBatchesPagedAsync(
        string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize)
    {
        var query = context.SubmissionBatches.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(b => b.Status == status);
        }

        if (dateFrom.HasValue)
        {
            var from = dateFrom.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(b => b.BatchDate != null && b.BatchDate >= from);
        }

        if (dateTo.HasValue)
        {
            var to = dateTo.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(b => b.BatchDate != null && b.BatchDate <= to);
        }

        var totalCount = await query.CountAsync();
        var batches = await query
            .OrderByDescending(b => b.BatchDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (batches, totalCount);
    }

    public async Task<SubmissionBatch> CreateBatchAsync(SubmissionBatch entity)
    {
        context.SubmissionBatches.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateBatchAsync(SubmissionBatch entity)
    {
        context.SubmissionBatches.Update(entity);
        await context.SaveChangesAsync();
    }

    public Task<SubmissionRef?> GetSubmissionRefByIdAsync(int submitID)
    {
        return context.SubmissionRefs.FirstOrDefaultAsync(r => r.SubmitId == submitID);
    }

    public Task<List<SubmissionRef>> GetSubmissionRefsByBatchAsync(int batchID)
    {
        return context.SubmissionRefs
            .Where(r => r.BatchId == batchID)
            .OrderBy(r => r.SubmitId)
            .ToListAsync();
    }

    public Task<List<SubmissionRef>> GetSubmissionRefsByClaimAsync(int claimID)
    {
        return context.SubmissionRefs
            .Where(r => r.ClaimId == claimID)
            .OrderByDescending(r => r.SubmitId)
            .ToListAsync();
    }

    public Task<SubmissionRef?> GetSubmissionRefByBatchAndClaimAsync(int batchID, int claimID)
    {
        return context.SubmissionRefs.FirstOrDefaultAsync(r => r.BatchId == batchID && r.ClaimId == claimID);
    }

    public Task<bool> Has999AckForBatchAsync(int batchID)
    {
        return context.SubmissionRefs.AnyAsync(r => r.BatchId == batchID && r.AckType == "999");
    }

    public Task<bool> Has277CAAckForClaimInBatchAsync(int batchID, int claimID)
    {
        return context.SubmissionRefs.AnyAsync(r => r.BatchId == batchID && r.ClaimId == claimID && r.AckType == "277CA");
    }

    public Task<bool> ClaimAlreadyBatchedAsync(int claimID)
    {
        return context.SubmissionRefs.AnyAsync(sr =>
            sr.ClaimId == claimID &&
            sr.Batch != null &&
            sr.Batch.Status != "Failed");
    }

    public async Task<SubmissionRef> CreateSubmissionRefAsync(SubmissionRef entity)
    {
        context.SubmissionRefs.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task CreateSubmissionRefsAsync(List<SubmissionRef> entities)
    {
        await context.SubmissionRefs.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }

    public async Task UpdateSubmissionRefsAsync(List<SubmissionRef> entities)
    {
        context.SubmissionRefs.UpdateRange(entities);
        await context.SaveChangesAsync();
    }

    public async Task DeleteSubmissionRefAsync(int batchID, int claimID)
    {
        var entity = await context.SubmissionRefs.FirstOrDefaultAsync(r =>
            r.BatchId == batchID && r.ClaimId == claimID && r.AckType == null);

        if (entity != null)
        {
            context.SubmissionRefs.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    public Task<Claim?> GetClaimByIdAsync(int claimID)
    {
        return context.Claims
            .Include(c => c.Patient)
            .Include(c => c.Plan)
            .ThenInclude(p => p.Payer)
            .FirstOrDefaultAsync(c => c.ClaimId == claimID);
    }

    public Task<List<Claim>> GetClaimsByIdsAsync(List<int> claimIDs)
    {
        return context.Claims
            .Include(c => c.Patient)
            .Include(c => c.Plan)
            .ThenInclude(p => p.Payer)
            .Where(c => claimIDs.Contains(c.ClaimId))
            .ToListAsync();
    }

    public async Task UpdateClaimStatusAsync(int claimID, string newStatus)
    {
        var claim = await context.Claims.FirstOrDefaultAsync(c => c.ClaimId == claimID);
        if (claim == null) return;
        claim.ClaimStatus = newStatus;
        await context.SaveChangesAsync();
    }

    public async Task UpdateClaimStatusBulkAsync(List<int> claimIDs, string newStatus)
    {
        var claims = await context.Claims.Where(c => claimIDs.Contains(c.ClaimId)).ToListAsync();
        foreach (var c in claims)
        {
            c.ClaimStatus = newStatus;
        }
        await context.SaveChangesAsync();
    }

    public Task<bool> ClaimExistsAsync(int claimID)
    {
        return context.Claims.AnyAsync(c => c.ClaimId == claimID);
    }

    public Task<X12837pRef?> GetX12RefByClaimIdAsync(int claimID)
    {
        return context.X12837pRefs.FirstOrDefaultAsync(x => x.ClaimId == claimID && x.Status == "Generated");
    }

    public Task<List<User>> GetUsersByRoleAsync(string role)
    {
        return context.Users.Where(u => u.Role == role && u.Status == "Active").ToListAsync();
    }

    public async Task WriteAuditLogAsync(int userID, string action, string resource, string? metadata)
    {
        context.AuditLogs.Add(new AuditLog
        {
            UserId = userID,
            Action = action,
            Resource = resource,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata
        });
        await context.SaveChangesAsync();
    }

    public async Task CreateNotificationAsync(int userID, string message, string category)
    {
        context.Notifications.Add(new Notification
        {
            UserId = userID,
            Message = message,
            Category = category,
            Status = "Unread",
            CreatedDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }
}

