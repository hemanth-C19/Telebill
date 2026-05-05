using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Dto.Reports;
using Telebill.Models;

namespace Telebill.Repositories.Reports;

public class AuditRepository(TeleBillContext context) : IAuditRepository
{
    public async Task<(List<AuditLog> Items, int TotalCount)> SearchAsync(AuditSearchParams filters)
    {
        var query = BuildAuditQuery(filters);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<AuditLog>> ExportAsync(AuditSearchParams filters)
    {
        var query = BuildAuditQuery(filters);
        return await query
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<string?> GetUserNameByIdAsync(int userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        return user?.Name;
    }

    private IQueryable<AuditLog> BuildAuditQuery(AuditSearchParams filters)
    {
        var query = context.AuditLogs.AsQueryable();

        if (filters.UserId.HasValue)
            query = query.Where(a => a.UserId == filters.UserId.Value);
        if (!string.IsNullOrEmpty(filters.Action))
            query = query.Where(a => a.Action == filters.Action);
        if (!string.IsNullOrEmpty(filters.Resource))
            query = query.Where(a => a.Resource == filters.Resource);
        if (filters.DateFrom.HasValue)
            query = query.Where(a => a.Timestamp >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            query = query.Where(a => a.Timestamp <= filters.DateTo.Value);

        return query;
    }
}

