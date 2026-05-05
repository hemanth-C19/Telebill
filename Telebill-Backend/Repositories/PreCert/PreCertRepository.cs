using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Models;

namespace Telebill.Repositories.PreCert;

public class PreCertRepository(TeleBillContext context) : IPreCertRepository
{
    public Task<PriorAuth?> GetPriorAuthByIdAsync(int paid)
    {
        return context.PriorAuths
            .Include(p => p.Plan)
            .ThenInclude(pl => pl.Payer)
            .FirstOrDefaultAsync(p => p.Paid == paid);
    }

    public async Task<List<PriorAuth>> GetPriorAuthsAsync(int? claimID, int? planID, string? status, bool? expiringSoon)
    {
        var query = context.PriorAuths
            .Include(p => p.Plan)
            .ThenInclude(pl => pl.Payer)
            .AsQueryable();

        if (claimID.HasValue)
        {
            query = query.Where(p => p.ClaimId == claimID.Value);
        }

        if (planID.HasValue)
        {
            query = query.Where(p => p.PlanId == planID.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (expiringSoon == true)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var sevenDaysLater = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
            query = query.Where(p =>
                p.Status == "Approved" &&
                p.ApprovedTo != null &&
                p.ApprovedTo >= today &&
                p.ApprovedTo <= sevenDaysLater);
        }

        return await query.OrderByDescending(p => p.Paid).ToListAsync();
    }

    public Task<List<PriorAuth>> GetPriorAuthsByClaimAsync(int claimID)
    {
        return context.PriorAuths
            .Include(p => p.Plan)
            .ThenInclude(pl => pl.Payer)
            .Where(p => p.ClaimId == claimID)
            .OrderByDescending(p => p.Paid)
            .ToListAsync();
    }

    public Task<bool> ActivePriorAuthExistsForClaimAsync(int claimID)
    {
        return context.PriorAuths.AnyAsync(p =>
            p.ClaimId == claimID &&
            (p.Status == "Requested" || p.Status == "Approved"));
    }

    public Task<bool> HasApprovedPriorAuthAsync(int claimID, DateOnly encounterDate)
    {
        return context.PriorAuths.AnyAsync(p =>
            p.ClaimId == claimID &&
            p.Status == "Approved" &&
            p.ApprovedFrom != null &&
            p.ApprovedTo != null &&
            p.ApprovedFrom <= encounterDate &&
            p.ApprovedTo >= encounterDate);
    }

    public async Task<PriorAuth> CreatePriorAuthAsync(PriorAuth entity)
    {
        context.PriorAuths.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdatePriorAuthAsync(PriorAuth entity)
    {
        context.PriorAuths.Update(entity);
        await context.SaveChangesAsync();
    }

    public Task<List<PriorAuth>> GetExpiredPriorAuthsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return context.PriorAuths
            .Where(p => p.Status == "Approved" && p.ApprovedTo != null && p.ApprovedTo < today)
            .ToListAsync();
    }

    public Task<List<PriorAuth>> GetExpiringSoonPriorAuthsAsync(int daysAhead)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysAhead));
        return context.PriorAuths
            .Where(p =>
                p.Status == "Approved" &&
                p.ApprovedTo != null &&
                p.ApprovedTo >= today &&
                p.ApprovedTo <= cutoff)
            .ToListAsync();
    }

    public Task<AttachmentRef?> GetAttachmentByIdAsync(int attachId)
    {
        return context.AttachmentRefs
            .Include(a => a.UploadedByNavigation)
            .FirstOrDefaultAsync(a => a.AttachId == attachId);
    }

    public async Task<List<AttachmentRef>> GetAttachmentsByClaimAsync(int claimID, string status)
    {
        var query = context.AttachmentRefs
            .Include(a => a.UploadedByNavigation)
            .Where(a => a.ClaimId == claimID);

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(a => a.Status == status);
        }

        return await query.OrderByDescending(a => a.UploadedDate).ToListAsync();
    }

    public async Task<AttachmentRef> CreateAttachmentAsync(AttachmentRef entity)
    {
        context.AttachmentRefs.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAttachmentAsync(AttachmentRef entity)
    {
        context.AttachmentRefs.Update(entity);
        await context.SaveChangesAsync();
    }

    public Task<bool> ClaimExistsAsync(int claimID)
    {
        return context.Claims.AnyAsync(c => c.ClaimId == claimID);
    }

    public Task<bool> PayerPlanExistsAsync(int planID)
    {
        return context.PayerPlans.AnyAsync(p => p.PlanId == planID);
    }

    public Task<PayerPlan?> GetPayerPlanWithPayerAsync(int planID)
    {
        return context.PayerPlans
            .Include(p => p.Payer)
            .FirstOrDefaultAsync(p => p.PlanId == planID);
    }

    public Task<User?> GetUserByIdAsync(int userID)
    {
        return context.Users.FirstOrDefaultAsync(u => u.UserId == userID);
    }

    public Task<List<User>> GetUsersByRoleAsync(string role)
    {
        return context.Users
            .Where(u => u.Role == role && u.Status == "Active")
            .ToListAsync();
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

