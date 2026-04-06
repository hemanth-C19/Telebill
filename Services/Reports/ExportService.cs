using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.Reports;
using Telebill.Repositories.Reports;

namespace Telebill.Services.Reports;

public class ExportService : IExportService
{
    private readonly IReportQueryRepository _queryRepo;

    public ExportService(IReportQueryRepository queryRepo)
    {
        _queryRepo = queryRepo;
    }

    public async Task<List<ClaimsListingRowDto>> GetClaimsListingAsync(ExportFilterParams filters)
    {
        var claims = await _queryRepo.GetClaimsForExportAsync(filters);
        var result = new List<ClaimsListingRowDto>();

        foreach (var claim in claims)
        {
            var patient = await _queryRepo.GetPatientByIdAsync(claim.PatientId);
            var payerId = await _queryRepo.GetPayerIdByPlanIdAsync(claim.PlanId);
            var payer = payerId.HasValue
                ? await _queryRepo.GetPayerByIdAsync(payerId.Value)
                : null;
            var plan = claim.PlanId.HasValue
                ? await _queryRepo.GetPlanByIdAsync(claim.PlanId.Value)
                : null;
            var provId = await _queryRepo.GetProviderIdByEncounterIdAsync(claim.EncounterId);
            var provider = provId.HasValue
                ? await _queryRepo.GetProviderByIdAsync(provId.Value)
                : null;
            var encList = await _queryRepo.GetEncountersByIdsAsync(new List<int?> { claim.EncounterId });
            var enc = encList.FirstOrDefault();

            result.Add(new ClaimsListingRowDto
            {
                ClaimId = claim.ClaimId,
                PatientName = patient?.Name,
                ProviderName = provider?.Name,
                PayerName = payer?.Name,
                PlanName = plan?.PlanName,
                EncounterDate = enc?.EncounterDateTime ?? DateTime.MinValue,
                TotalCharge = claim.TotalCharge ?? 0m,
                ClaimStatus = claim.ClaimStatus,
                CreatedDate = claim.CreatedDate
            });
        }

        return result;
    }

    public async Task<List<ScrubIssueExportRowDto>> GetScrubIssuesAsync(ExportFilterParams filters)
    {
        var issues = await _queryRepo.GetScrubIssuesForExportAsync(filters);
        var result = new List<ScrubIssueExportRowDto>();

        foreach (var issue in issues)
        {
            var rule = issue.RuleId.HasValue
                ? await _queryRepo.GetScrubRuleByIdAsync(issue.RuleId.Value)
                : null;

            var allClaims = await _queryRepo.GetClaimsForExportAsync(new ExportFilterParams());
            var claim = allClaims.FirstOrDefault(c => c.ClaimId == issue.ClaimId);

            var patient = claim != null
                ? await _queryRepo.GetPatientByIdAsync(claim.PatientId)
                : null;
            var payerId = claim != null
                ? await _queryRepo.GetPayerIdByPlanIdAsync(claim.PlanId)
                : null;
            var payer = payerId.HasValue
                ? await _queryRepo.GetPayerByIdAsync(payerId.Value)
                : null;

            result.Add(new ScrubIssueExportRowDto
            {
                IssueId = issue.IssueId,
                ClaimId = issue.ClaimId ?? 0,
                ClaimLineId = issue.ClaimLineId,
                RuleName = rule?.Name,
                Severity = issue.Severity,
                Message = issue.Message,
                DetectedDate = issue.DetectedDate ?? DateTime.MinValue,
                IssueStatus = issue.Status,
                PatientName = patient?.Name,
                PayerName = payer?.Name
            });
        }

        return result;
    }

    public async Task<List<ArAgingRowDto>> GetArAgingAsync(ExportFilterParams filters)
    {
        var denials = await _queryRepo.GetDenialsForExportAsync(filters);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var result = new List<ArAgingRowDto>();

        foreach (var denial in denials)
        {
            var allClaims = await _queryRepo.GetClaimsForExportAsync(new ExportFilterParams());
            var claim = allClaims.FirstOrDefault(c => c.ClaimId == denial.ClaimId);
            var patient = claim != null
                ? await _queryRepo.GetPatientByIdAsync(claim.PatientId)
                : null;
            var payerId = claim != null
                ? await _queryRepo.GetPayerIdByPlanIdAsync(claim.PlanId)
                : null;
            var payer = payerId.HasValue
                ? await _queryRepo.GetPayerByIdAsync(payerId.Value)
                : null;

            var days = today.DayNumber - denial.DenialDate.DayNumber;
            var bucket = days switch
            {
                <= 30 => "0-30",
                <= 60 => "31-60",
                <= 90 => "61-90",
                _ => "90+"
            };

            result.Add(new ArAgingRowDto
            {
                DenialId = denial.DenialId,
                ClaimId = denial.ClaimId ?? 0,
                PatientName = patient?.Name,
                PayerName = payer?.Name,
                ReasonCode = denial.ReasonCode,
                AmountDenied = denial.AmountDenied,
                DenialDate = denial.DenialDate,
                DaysSinceDenial = days,
                AgingBucket = bucket,
                DenialStatus = denial.Status
            });
        }

        return result
            .OrderByDescending(r => r.DaysSinceDenial)
            .ToList();
    }

    public async Task<List<StatementSummaryRowDto>> GetStatementsSummaryAsync(ExportFilterParams filters)
    {
        var statements = await _queryRepo.GetStatementsForExportAsync(filters);
        var result = new List<StatementSummaryRowDto>();

        foreach (var statement in statements)
        {
            var patient = await _queryRepo.GetPatientByIdAsync(statement.PatientId);

            result.Add(new StatementSummaryRowDto
            {
                StatementId = statement.StatementId,
                PatientName = patient?.Name,
                PeriodStart = statement.PeriodStart?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                PeriodEnd = statement.PeriodEnd?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.MinValue,
                GeneratedDate = statement.GeneratedDate ?? DateTime.MinValue,
                AmountDue = statement.AmountDue ?? 0m,
                StatementStatus = statement.Status
            });
        }

        return result;
    }

    public async Task<List<RemitSummaryRowDto>> GetRemitSummaryAsync(ExportFilterParams filters)
    {
        var remits = await _queryRepo.GetRemitRefsForExportAsync(filters);
        var result = new List<RemitSummaryRowDto>();

        foreach (var remit in remits)
        {
            var payer = remit.PayerId.HasValue
                ? await _queryRepo.GetPayerByIdAsync(remit.PayerId.Value)
                : null;
            var totalPosted = await _queryRepo.GetTotalPostedByRemitIdAsync(remit.RemitId);

            result.Add(new RemitSummaryRowDto
            {
                RemitId = remit.RemitId,
                PayerName = payer?.Name,
                BatchId = remit.BatchId,
                PayloadUri = remit.PayloadUri,
                ReceivedDate = remit.ReceivedDate ?? DateTime.MinValue,
                RemitStatus = remit.Status,
                TotalPosted = totalPosted
            });
        }

        return result;
    }
}

