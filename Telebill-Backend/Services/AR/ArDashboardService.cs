using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.AR;
using Telebill.Models;
using Telebill.Repositories.AR;

namespace Telebill.Services.AR;

public class ArDashboardService(IArRepository repo) : IArDashboardService
{
    public async Task<ArDashboardSummaryDto> GetArDashboardAsync()
    {
        var openDenials = await repo.GetAllOpenDenialsAsync();
        var partialClaims = await repo.GetPartiallyPaidClaimsAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        // ── Aging breakdown ───────────────────────────────────────────────────
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
            agingDict[bucket].Count++;
            agingDict[bucket].Amount += d.AmountDenied ?? 0m;
        }

        // ── Payer breakdown ───────────────────────────────────────────────────
        var denialClaimIds = openDenials
            .Where(d => d.ClaimId.HasValue)
            .Select(d => d.ClaimId!.Value)
            .Distinct()
            .ToList();

        var claimEntities = new List<Claim>();
        foreach (var id in denialClaimIds)
        {
            var claim = await repo.GetClaimByIdAsync(id);
            if (claim != null) claimEntities.Add(claim);
        }

        var byPayer = new List<PayerDenialSummaryDto>();

        var payerGroups = claimEntities
            .Where(c => c.PlanId.HasValue)
            .GroupBy(c => c.PlanId!.Value)
            .ToList();

        foreach (var group in payerGroups)
        {
            var payer = await repo.GetPayerByPlanIdAsync(group.Key);
            if (payer == null) continue;

            var groupClaimIds = group.Select(c => c.ClaimId).ToHashSet();
            var payerDenials = openDenials
                .Where(d => d.ClaimId.HasValue && groupClaimIds.Contains(d.ClaimId.Value))
                .ToList();

            var totalSubmitted = await repo.GetTotalClaimsSubmittedByPayerAsync(payer.PayerId);

            byPayer.Add(new PayerDenialSummaryDto
            {
                PayerId = payer.PayerId,
                PayerName = payer.Name,
                DenialCount = payerDenials.Count,
                TotalAmountDenied = payerDenials.Sum(d => d.AmountDenied ?? 0m),
                DenialRate = totalSubmitted > 0
                    ? Math.Round((double)payerDenials.Count / totalSubmitted * 100d, 1)
                    : 0d
            });
        }

        // ── Reason code breakdown ─────────────────────────────────────────────
        var byReason = openDenials
            .GroupBy(d => d.ReasonCode ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Select(g => new DenialReasonSummaryDto
            {
                ReasonCode = g.Key,
                Description = GetReasonDescription(g.Key),
                Count = g.Count(),
                TotalAmount = g.Sum(d => d.AmountDenied ?? 0m)
            })
            .ToList();

        return new ArDashboardSummaryDto
        {
            TotalOpenDenials = openDenials.Count(d =>
                string.Equals(d.Status, "Open", StringComparison.OrdinalIgnoreCase)),
            TotalAmountAtRisk = openDenials
                .Where(d => string.Equals(d.Status, "Open", StringComparison.OrdinalIgnoreCase))
                .Sum(d => d.AmountDenied ?? 0m),
            TotalAppealedDenials = openDenials.Count(d =>
                string.Equals(d.Status, "Appealed", StringComparison.OrdinalIgnoreCase)),
            TotalUnderpayments = partialClaims.Count,
            AgingBreakdown = agingDict.Values.ToList(),
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

