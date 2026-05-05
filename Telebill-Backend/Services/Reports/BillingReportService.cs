using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telebill.Dto.Reports;
using Telebill.Models;
using Telebill.Repositories.Reports;

namespace Telebill.Services.Reports;

public class BillingReportService(
    IReportQueryRepository queryRepo,
    IBillingReportRepository reportRepo,
    IKpiService kpiService) : IBillingReportService
{
    public async Task<(bool success, string error, KpiResultDto? result)> GenerateAndStoreAsync(
        GenerateReportRequestDto dto)
    {
        var allowedScopes = new[] { "Payer", "Plan", "Provider", "Period" };
        if (!allowedScopes.Contains(dto.Scope))
        {
            return (false, $"Invalid Scope '{dto.Scope}'. Allowed: Payer, Plan, Provider, Period", null);
        }

        if (!string.Equals(dto.Scope, "Period", StringComparison.OrdinalIgnoreCase) && !dto.ScopeId.HasValue)
        {
            return (false, $"ScopeId is required when Scope is '{dto.Scope}'", null);
        }

        if (dto.PeriodStart >= dto.PeriodEnd)
        {
            return (false, "PeriodStart must be before PeriodEnd", null);
        }

        var filters = new KpiFilterParams
        {
            Scope = dto.Scope,
            ScopeId = dto.ScopeId,
            PeriodStart = dto.PeriodStart,
            PeriodEnd = dto.PeriodEnd
        };

        string? scopeName = null;
        int? payerId = null;
        int? planId = null;
        int? providerId = null;

        switch (dto.Scope)
        {
            case "Payer":
                var payer = await queryRepo.GetPayerByIdAsync(dto.ScopeId!.Value);
                if (payer == null) return (false, "Payer not found", null);
                scopeName = payer.Name;
                payerId = dto.ScopeId;
                break;
            case "Plan":
                var plan = await queryRepo.GetPlanByIdAsync(dto.ScopeId!.Value);
                if (plan == null) return (false, "Plan not found", null);
                scopeName = plan.PlanName;
                planId = dto.ScopeId;
                break;
            case "Provider":
                var provider = await queryRepo.GetProviderByIdAsync(dto.ScopeId!.Value);
                if (provider == null) return (false, "Provider not found", null);
                scopeName = provider.Name;
                providerId = dto.ScopeId;
                break;
            case "Period":
                scopeName = $"{dto.PeriodStart:yyyy-MM-dd} to {dto.PeriodEnd:yyyy-MM-dd}";
                break;
        }

        var claims = await queryRepo.GetClaimsForPeriodAsync(
            dto.PeriodStart, dto.PeriodEnd, payerId, planId, providerId);

        var claimIds = claims.Select(c => c.ClaimId).ToList();
        var encounterIds = claims.Select(c => c.EncounterId).Distinct().ToList();

        var scrubIssues = claimIds.Any()
            ? await queryRepo.GetScrubIssuesByClaimIdsAsync(claimIds)
            : new List<ScrubIssue>();
        var submissionRefs = claimIds.Any()
            ? await queryRepo.GetSubmissionRefsByClaimIdsAsync(claimIds)
            : new List<SubmissionRef>();
        var paymentPosts = claimIds.Any()
            ? await queryRepo.GetPaymentPostsByClaimIdsAsync(claimIds)
            : new List<PaymentPost>();
        var encounters = encounterIds.Any()
            ? await queryRepo.GetEncountersByIdsAsync(encounterIds)
            : new List<Encounter>();
        var denials = claimIds.Any()
            ? await queryRepo.GetDenialsByClaimIdsAsync(claimIds)
            : new List<Denial>();

        var kpiResult = kpiService.Compute(
            filters, scopeName, claims, scrubIssues,
            submissionRefs, paymentPosts, encounters, denials);

        var metricsJson = JsonSerializer.Serialize(new
        {
            ScopeName = kpiResult.ScopeName,
            CCR = kpiResult.CleanClaimRate,
            FPAR = kpiResult.FirstPassAcceptance,
            DSO = kpiResult.DaysSalesOutstanding,
            DenialRate = kpiResult.DenialRate,
            TAT = kpiResult.PayerTurnaroundTime,
            TotalClaims = kpiResult.TotalClaims,
            TotalSubmitted = kpiResult.TotalSubmitted,
            TotalAccepted = kpiResult.TotalAccepted,
            TotalDenied = kpiResult.TotalDenied,
            TotalScrubErrors = kpiResult.TotalScrubErrors
        });

        var report = new BillingReport
        {
            Scope = dto.Scope,
            MetricsJson = metricsJson,
            GeneratedDate = DateTime.UtcNow
        };

        await reportRepo.AddAsync(report);

        return (true, string.Empty, kpiResult);
    }

    public async Task<List<BillingReportListItemDto>> GetAllAsync(BillingReportFilterParams filters)
    {
        var reports = await reportRepo.GetAllAsync(filters);

        return reports.Select(r => new BillingReportListItemDto
        {
            ReportId = r.ReportId,
            Scope = r.Scope ?? string.Empty,
            MetricsJson = r.MetricsJson ?? string.Empty,
            GeneratedDate = r.GeneratedDate ?? DateTime.MinValue
        }).ToList();
    }

    public async Task<BillingReportDetailDto?> GetByIdAsync(int reportId)
    {
        var report = await reportRepo.GetByIdAsync(reportId);
        if (report == null)
        {
            return null;
        }

        var detail = new BillingReportDetailDto
        {
            ReportId = report.ReportId,
            Scope = report.Scope ?? string.Empty,
            GeneratedDate = report.GeneratedDate ?? DateTime.MinValue
        };

        try
        {
            if (!string.IsNullOrWhiteSpace(report.MetricsJson))
            {
                using var doc = JsonDocument.Parse(report.MetricsJson);
                var root = doc.RootElement;
                detail.Ccr = root.TryGetProperty("CCR", out var v1) ? v1.GetDouble() : null;
                detail.Fpar = root.TryGetProperty("FPAR", out var v2) ? v2.GetDouble() : null;
                detail.Dso = root.TryGetProperty("DSO", out var v3) ? v3.GetDouble() : null;
                detail.DenialRate = root.TryGetProperty("DenialRate", out var v4) ? v4.GetDouble() : null;
                detail.Tat = root.TryGetProperty("TAT", out var v5) ? v5.GetDouble() : null;
            }
        }
        catch
        {
            // MetricsJson malformed — return partial result with nulls
        }

        return detail;
    }
}

