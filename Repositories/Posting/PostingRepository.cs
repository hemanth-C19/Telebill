using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Dto.Posting;
using Telebill.Models;

namespace Telebill.Repositories.Posting;

public class PostingRepository : IPostingRepository
{
    private readonly TeleBillContext _context;

    public PostingRepository(TeleBillContext context)
    {
        _context = context;
    }

    public Task<RemitRef?> GetRemitRefByIdAsync(int remitID)
    {
        return _context.RemitRefs.Include(r => r.Payer).FirstOrDefaultAsync(r => r.RemitId == remitID);
    }

    public async Task<(List<RemitRef> items, int totalCount)> GetRemitRefsPagedAsync(int? payerID, string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize)
    {
        var query = _context.RemitRefs.Include(r => r.Payer).AsQueryable();

        if (payerID.HasValue)
        {
            query = query.Where(r => r.PayerId == payerID.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (dateFrom.HasValue)
        {
            var from = dateFrom.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(r => r.ReceivedDate != null && r.ReceivedDate >= from);
        }

        if (dateTo.HasValue)
        {
            var to = dateTo.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(r => r.ReceivedDate != null && r.ReceivedDate <= to);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReceivedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<RemitRef> CreateRemitRefAsync(RemitRef entity)
    {
        _context.RemitRefs.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateRemitRefAsync(RemitRef entity)
    {
        _context.RemitRefs.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task<PaymentPost?> GetPaymentPostByIdAsync(int paymentID)
    {
        return _context.PaymentPosts
            .Include(p => p.PostedByNavigation)
            .Include(p => p.ClaimLine)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentID);
    }

    public Task<List<PaymentPost>> GetPaymentPostsByClaimAsync(int claimID)
    {
        return _context.PaymentPosts
            .Include(p => p.PostedByNavigation)
            .Include(p => p.ClaimLine)
            .Where(p => p.ClaimId == claimID)
            .OrderByDescending(p => p.PostedDate)
            .ToListAsync();
    }

    public Task<bool> ActivePostExistsForLineAsync(int claimID, int? claimLineID)
    {
        return _context.PaymentPosts.AnyAsync(p =>
            p.ClaimId == claimID &&
            p.ClaimLineId == claimLineID &&
            p.Status == "Active");
    }

    public async Task<PaymentPost> CreatePaymentPostAsync(PaymentPost entity)
    {
        _context.PaymentPosts.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdatePaymentPostAsync(PaymentPost entity)
    {
        _context.PaymentPosts.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task<PatientBalance?> GetPatientBalanceByIdAsync(int balanceID)
    {
        return _context.PatientBalances.Include(b => b.Patient).FirstOrDefaultAsync(b => b.BalanceId == balanceID);
    }

    public Task<PatientBalance?> GetPatientBalanceByClaimAsync(int claimID)
    {
        return _context.PatientBalances.Include(b => b.Patient).FirstOrDefaultAsync(b => b.ClaimId == claimID);
    }

    public Task<List<PatientBalance>> GetPatientBalancesByPatientAsync(int patientID)
    {
        return _context.PatientBalances.Include(b => b.Patient).Where(b => b.PatientId == patientID).ToListAsync();
    }

    public async Task<(List<PatientBalance> items, int totalCount)> GetPatientBalancesPagedAsync(PatientBalanceFilterParams filters)
    {
        var query = _context.PatientBalances.Include(b => b.Patient).AsQueryable();

        if (filters.PatientID.HasValue)
        {
            query = query.Where(b => b.PatientId == filters.PatientID.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.AgingBucket))
        {
            query = query.Where(b => b.AgingBucket == filters.AgingBucket);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            query = query.Where(b => b.Status == filters.Status);
        }

        if (filters.MinAmount.HasValue)
        {
            query = query.Where(b => (b.AmountDue ?? 0m) >= filters.MinAmount.Value);
        }

        if (filters.MaxAmount.HasValue)
        {
            query = query.Where(b => (b.AmountDue ?? 0m) <= filters.MaxAmount.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.AmountDue)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<AgingSummaryDto> GetAgingSummaryAsync()
    {
        var openBalances = await _context.PatientBalances.Where(b => b.Status == "Open").ToListAsync();
        decimal Sum(string bucket) => openBalances.Where(b => b.AgingBucket == bucket).Sum(b => b.AmountDue ?? 0m);

        return new AgingSummaryDto
        {
            Bucket0To30 = Sum("0-30"),
            Bucket31To60 = Sum("31-60"),
            Bucket61To90 = Sum("61-90"),
            Bucket90Plus = Sum("90+"),
            TotalOutstanding = openBalances.Sum(b => b.AmountDue ?? 0m),
            OpenBalanceCount = openBalances.Count
        };
    }

    public Task<List<PatientBalance>> GetAllOpenBalancesAsync()
    {
        return _context.PatientBalances.Where(b => b.Status == "Open").ToListAsync();
    }

    public async Task<PatientBalance> CreatePatientBalanceAsync(PatientBalance entity)
    {
        _context.PatientBalances.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdatePatientBalanceAsync(PatientBalance entity)
    {
        _context.PatientBalances.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SaveAllPatientBalancesAsync(List<PatientBalance> entities)
    {
        _context.PatientBalances.UpdateRange(entities);
        await _context.SaveChangesAsync();
    }

    public Task<Statement?> GetStatementByIdAsync(int statementID)
    {
        return _context.Statements.Include(s => s.Patient).FirstOrDefaultAsync(s => s.StatementId == statementID);
    }

    public Task<bool> StatementExistsForPeriodAsync(int patientID, DateOnly periodStart, DateOnly periodEnd)
    {
        return _context.Statements.AnyAsync(s =>
            s.PatientId == patientID &&
            s.PeriodStart == periodStart &&
            s.PeriodEnd == periodEnd);
    }

    public async Task<(List<Statement> items, int totalCount)> GetStatementsPagedAsync(int? patientID, string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize)
    {
        var query = _context.Statements.Include(s => s.Patient).AsQueryable();

        if (patientID.HasValue)
        {
            query = query.Where(s => s.PatientId == patientID.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.Status == status);
        }

        if (dateFrom.HasValue)
        {
            var from = dateFrom.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(s => s.GeneratedDate != null && s.GeneratedDate >= from);
        }

        if (dateTo.HasValue)
        {
            var to = dateTo.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(s => s.GeneratedDate != null && s.GeneratedDate <= to);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.GeneratedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Statement> CreateStatementAsync(Statement entity)
    {
        _context.Statements.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateStatementAsync(Statement entity)
    {
        _context.Statements.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task<Claim?> GetClaimByIdAsync(int claimID)
    {
        return _context.Claims
            .Include(c => c.Patient)
            .Include(c => c.Encounter)
            .FirstOrDefaultAsync(c => c.ClaimId == claimID);
    }

    public Task<List<ClaimLine>> GetActiveClaimLinesByClaimAsync(int claimID)
    {
        return _context.ClaimLines.Where(l => l.ClaimId == claimID && l.LineStatus == "Active").ToListAsync();
    }

    public Task<ClaimLine?> GetClaimLineByIdAsync(int claimLineID)
    {
        return _context.ClaimLines.FirstOrDefaultAsync(l => l.ClaimLineId == claimLineID);
    }

    public async Task UpdateClaimStatusAsync(int claimID, string newStatus)
    {
        var claim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimId == claimID);
        if (claim == null) return;
        claim.ClaimStatus = newStatus;
        await _context.SaveChangesAsync();
    }

    public Task<Encounter?> GetEncounterByIdAsync(int encounterID)
    {
        return _context.Encounters.FirstOrDefaultAsync(e => e.EncounterId == encounterID);
    }

    public Task<Patient?> GetPatientByIdAsync(int patientID)
    {
        return _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientID);
    }

    public Task<bool> PayerExistsAsync(int payerID)
    {
        return _context.Payers.AnyAsync(p => p.PayerId == payerID);
    }

    public Task<bool> BatchExistsAsync(int batchID)
    {
        return _context.SubmissionBatches.AnyAsync(b => b.BatchId == batchID);
    }

    public Task<Payer?> GetPayerByIdAsync(int payerID)
    {
        return _context.Payers.FirstOrDefaultAsync(p => p.PayerId == payerID);
    }

    public Task<DateTime?> GetFirstPostingDateForClaimAsync(int claimID)
    {
        return _context.PaymentPosts
            .Where(p => p.ClaimId == claimID && p.Status == "Active")
            .MinAsync(p => (DateTime?)p.PostedDate);
    }

    public async Task<Denial> CreateDenialAsync(Denial entity)
    {
        _context.Denials.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public Task<List<User>> GetUsersByRoleAsync(string role)
    {
        return _context.Users.Where(u => u.Role == role && u.Status == "Active").ToListAsync();
    }

    public Task<List<int>> GetDistinctPatientIDsWithOpenBalancesAsync()
    {
        return _context.PatientBalances
            .Where(b => b.Status == "Open" && (b.AmountDue ?? 0m) > 0m)
            .Select(b => b.PatientId ?? 0)
            .Where(id => id != 0)
            .Distinct()
            .ToListAsync();
    }

    public async Task WriteAuditLogAsync(int userID, string action, string resource, string? metadata)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userID,
            Action = action,
            Resource = resource,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata
        });
        await _context.SaveChangesAsync();
    }

    public async Task CreateNotificationAsync(int userID, string message, string category)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userID,
            Message = message,
            Category = category,
            Status = "Unread",
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}

