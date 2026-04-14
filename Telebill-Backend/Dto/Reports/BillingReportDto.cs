using System;

namespace Telebill.Dto.Reports;

// Stored BillingReport row (returned to UI from DB)
public class BillingReportListItemDto
{
    public int ReportId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string MetricsJson { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
}

// Parsed detail view of a single BillingReport row
public class BillingReportDetailDto
{
    public int ReportId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }

    // Parsed from MetricsJson
    public double? Ccr { get; set; }
    public double? Fpar { get; set; }
    public double? Dso { get; set; }
    public double? DenialRate { get; set; }
    public double? Tat { get; set; }
}

// Filter params for listing stored reports
public class BillingReportFilterParams
{
    public string? Scope { get; set; }
    public DateTime? GeneratedFrom { get; set; }
    public DateTime? GeneratedTo { get; set; }
}

// Input to manually trigger report generation for a scope
public class GenerateReportRequestDto
{
    public string Scope { get; set; } = string.Empty;
    public int? ScopeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

