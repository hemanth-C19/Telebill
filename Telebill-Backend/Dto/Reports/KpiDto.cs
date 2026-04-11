using System;
using Telebill.Models;

namespace Telebill.Dto.Reports;

// Filter parameters for KPI computation
public class KpiFilterParams
{
    public string Scope { get; set; } = string.Empty;
    // Allowed: "Payer" | "Plan" | "Provider" | "Period"

    public int? ScopeId { get; set; }
    // PayerId if Scope="Payer", PlanId if Scope="Plan",
    // ProviderId if Scope="Provider", null if Scope="Period"

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

// Single KPI result set
public class KpiResultDto
{
    public string Scope { get; set; } = string.Empty;
    public int? ScopeId { get; set; }
    public string? ScopeName { get; set; }   // PayerName / PlanName / ProviderName / Period label

    // The 5 KPIs
    public double CleanClaimRate { get; set; }       // CCR  — percentage 0–100
    public double FirstPassAcceptance { get; set; }  // FPAR — percentage 0–100
    public double DaysSalesOutstanding { get; set; } // DSO  — average days (decimal)
    public double DenialRate { get; set; }           // percentage 0–100
    public double PayerTurnaroundTime { get; set; }  // TAT  — average days (decimal)

    // Supporting counts (for context / drill-down)
    public int TotalClaims { get; set; }
    public int TotalSubmitted { get; set; }
    public int TotalAccepted { get; set; }
    public int TotalDenied { get; set; }
    public int TotalScrubErrors { get; set; }

    public DateTime ComputedAt { get; set; }
}

