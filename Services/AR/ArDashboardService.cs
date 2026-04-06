using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.AR;
using Telebill.Repositories.AR;

namespace Telebill.Services.AR;

public class ArDashboardService : IArDashboardService
{
    private readonly IArRepository _repo;

    public ArDashboardService(IArRepository repo)
    {
        _repo = repo;
    }

    public async Task<ArDashboardSummaryDto> GetArDashboardAsync()
    {
        var openDenials = await _repo.GetAllOpenDenialsAsync();
        var partialClaims = await _repo.GetPartiallyPaidClaimsAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Aging breakdown
        var agingBuckets = new[] { "0-30", "31-60", "61-90", "90+" };
        var agingDict = agingBuckets.ToDictionary(
            b => b,
            b => new AgingBucketSummaryDto { Bucket = b, Count = 0, Amount = 0m });

        foreach (var d in openDenials)
        {
            if (d.DenialDate == null) continue;
            var days = today.DayNumber - d.DenialDate.Value.DayNumber;
            var bucket = days <= 30 ? "0-30"
                : days <= 60 ? "31-60"
                : days <= 90 ? "61-90"
                : "90+";

            var dto = agingDict[bucket];
            dto.Count += 1;
            dto.Amount += d.AmountDenied ?? 0m;
        }

        var agingList = agingDict.Values.ToList();

        // Payer breakdown
        var byPayer = new List<PayerDenialSummaryDto>();
        var claimsById = partialClaims
            .Concat(openDenials.Where(d => d.ClaimId.HasValue)
                .Select(d => new Claim { ClaimId = d.ClaimId!.Value }))
            .GroupBy(c => c.ClaimId)
            .ToDictionary(g => g.Key, g => g.First());

        var planIds = openDenials
            .Where(d => d.ClaimId.HasValue)
            .Select(d => d.ClaimId!.Value)
            .Distinct()
            .ToList();

        var claimEntities = new List<Models.Claim>();
        foreach (var id in planIds)
        {
            var claim = await _repo.GetClaimByIdAsync(id);
            if (claim != null)
            {
                claimEntities.Add(claim);
            }
        }

        var payerGroups = claimEntities
            .Where(c => c.PlanId.HasValue)
            .GroupBy(c => c.PlanId!.Value)
            .ToList();

        foreach (var group in payerGroups)
        {
            var planId = group.Key;
            var plan = await _repo.GetPayerPlanByIdAsync(planId);
            var payer = plan != null ? await _repo.GetPayerByPlanIdAsync(plan.PlanId) : null;
            if (payer == null) continue;

            var payerId = payer.PayerId;

            var payerDenials = openDenials
                .Where(d => d.ClaimId.HasValue && group.Any(c => c.ClaimId == d.ClaimId.Value))
                .ToList();

            var denialCount = payerDenials.Count;
            var totalDenied = payerDenials.Sum(d => d.AmountDenied ?? 0m);
            var totalSubmitted = await _repo.GetTotalClaimsSubmittedByPayerAsync(payerId);
            var rate = totalSubmitted > 0
                ? Math.Round((double)denialCount / totalSubmitted * 100d, 1)
                : 0d;

            byPayer.Add(new PayerDenialSummaryDto
            {
                PayerId = payerId,
                PayerName = payer.Name,
                DenialCount = denialCount,
                TotalAmountDenied = totalDenied,
                DenialRate = rate
            });
        }

        // Reason code breakdown
        var reasonDict = new Dictionary<string, DenialReasonSummaryDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var d in openDenials)
        {
            var code = d.ReasonCode ?? string.Empty;
            if (!reasonDict.TryGetValue(code, out var summary))
            {
                summary = new DenialReasonSummaryDto
                {
                    ReasonCode = code,
                    Description = GetReasonDescription(code),
                    Count = 0,
                    TotalAmount = 0m
                };
                reasonDict[code] = summary;
            }

            summary.Count += 1;
            summary.TotalAmount += d.AmountDenied ?? 0m;
        }

        var byReason = reasonDict.Values.ToList();

        return new ArDashboardSummaryDto
        {
            TotalOpenDenials = openDenials.Count(d => string.Equals(d.Status, "Open", StringComparison.OrdinalIgnoreCase)),
            TotalAmountAtRisk = openDenials
                .Where(d => string.Equals(d.Status, "Open", StringComparison.OrdinalIgnoreCase))
                .Sum(d => d.AmountDenied ?? 0m),
            TotalAppealedDenials = openDenials.Count(d => string.Equals(d.Status, "Appealed", StringComparison.OrdinalIgnoreCase)),
            TotalUnderpayments = partialClaims.Count,
            AgingBreakdown = agingList,
            ByPayer = byPayer,
            ByReasonCode = byReason
        };
    }

    private static string GetReasonDescription(string code)
    {
        return code switch
        {
            "4" => "Service dates not covered by plan",
            "16" => "Claim lacks required information",
            "50" => "Not covered by plan",
            "96" => "Non-covered charge",
            "97" => "Bundled with another service",
            "181" => "Procedure inconsistent with modifier",
            "UNDERPAYMENT" => "Potential underpayment dispute",
            _ => string.IsNullOrWhiteSpace(code) ? "Other" : $"Other: {code}"
        };
    }
}

