using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Reports;
using Telebill.Repositories.Reports;

namespace Telebill.Services.Reports;

public class AuditSearchService(IAuditRepository auditRepo) : IAuditSearchService
{
    public async Task<AuditLogPagedResultDto> SearchAsync(AuditSearchParams filters)
    {
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1) filters.PageSize = 50;
        if (filters.PageSize > 200) filters.PageSize = 200;

        var (items, totalCount) = await auditRepo.SearchAsync(filters);

        var rows = new List<AuditLogRowDto>();
        foreach (var item in items)
        {
            var userName = item.UserId.HasValue
                ? await auditRepo.GetUserNameByIdAsync(item.UserId.Value)
                : null;

            rows.Add(new AuditLogRowDto
            {
                AuditId = item.AuditId,
                UserId = item.UserId ?? 0,
                UserName = userName,
                Action = item.Action,
                Resource = item.Resource,
                Timestamp = item.Timestamp ?? DateTime.MinValue,
                Metadata = item.Metadata
            });
        }

        return new AuditLogPagedResultDto
        {
            Items = rows,
            TotalCount = totalCount,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
        };
    }

    public async Task<List<AuditLogRowDto>> ExportAsync(AuditSearchParams filters)
    {
        filters.Page = 1;
        filters.PageSize = int.MaxValue;

        var items = await auditRepo.ExportAsync(filters);

        var rows = new List<AuditLogRowDto>();
        foreach (var item in items)
        {
            var userName = item.UserId.HasValue
                ? await auditRepo.GetUserNameByIdAsync(item.UserId.Value)
                : null;

            rows.Add(new AuditLogRowDto
            {
                AuditId = item.AuditId,
                UserId = item.UserId ?? 0,
                UserName = userName,
                Action = item.Action,
                Resource = item.Resource,
                Timestamp = item.Timestamp ?? DateTime.MinValue,
                Metadata = item.Metadata
            });
        }

        return rows;
    }
}

