using System;
using System.Collections.Generic;

namespace Telebill.Dto.Reports;

// Search filter params — all fields optional
public class AuditSearchParams
{
    public int? UserId { get; set; }
    public string? Action { get; set; }   // "CREATE" | "UPDATE" | "DELETE" | "LOGIN" | "EXPORT"
    public string? Resource { get; set; } // "Claim" | "Encounter" | "Denial" | etc.
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

// Single audit log row returned to UI
public class AuditLogRowDto
{
    public int AuditId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }   // joined from User table
    public string? Action { get; set; }
    public string? Resource { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Metadata { get; set; }   // raw JSON string — pass through, do not parse
}

// Paged result wrapper
public class AuditLogPagedResultDto
{
    public List<AuditLogRowDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

