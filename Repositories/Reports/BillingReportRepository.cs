using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Dto.Reports;
using Telebill.Models;

namespace Telebill.Repositories.Reports;

public class BillingReportRepository : IBillingReportRepository
{
    private readonly TeleBillContext _context;

    public BillingReportRepository(TeleBillContext context)
    {
        _context = context;
    }

    public async Task<List<BillingReport>> GetAllAsync(BillingReportFilterParams filters)
    {
        var query = _context.BillingReports.AsQueryable();

        if (!string.IsNullOrEmpty(filters.Scope))
            query = query.Where(r => r.Scope == filters.Scope);
        if (filters.GeneratedFrom.HasValue)
            query = query.Where(r => r.GeneratedDate >= filters.GeneratedFrom.Value);
        if (filters.GeneratedTo.HasValue)
            query = query.Where(r => r.GeneratedDate <= filters.GeneratedTo.Value);

        return await query
            .OrderByDescending(r => r.GeneratedDate)
            .ToListAsync();
    }

    public Task<BillingReport?> GetByIdAsync(int reportId)
    {
        return _context.BillingReports
            .FirstOrDefaultAsync(r => r.ReportId == reportId);
    }

    public async Task AddAsync(BillingReport report)
    {
        await _context.BillingReports.AddAsync(report);
        await _context.SaveChangesAsync();
    }
}

