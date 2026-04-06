using System;
using System.Collections.Generic;
using System.Linq;
using Telebill.Dto.Reports;
using Telebill.Models;

namespace Telebill.Services.Reports;

public class KpiService : IKpiService
{
    // NO repository injected — this service is pure math only.

    public KpiResultDto Compute(
        KpiFilterParams filters,
        string? scopeName,
        List<Claim> claims,
        List<ScrubIssue> scrubIssues,
        List<SubmissionRef> submissionRefs,
        List<PaymentPost> paymentPosts,
        List<Encounter> encounters,
        List<Denial> denials)
    {
        // CCR
        var totalClaims = claims.Count;

        var dirtyClaimIds = scrubIssues
            .Where(si => si.ClaimId.HasValue)
            .Select(si => si.ClaimId!.Value)
            .Distinct()
            .ToHashSet();

        var cleanClaims = claims.Count(c => !dirtyClaimIds.Contains(c.ClaimId));
        var ccr = totalClaims > 0 ? Math.Round((double)cleanClaims / totalClaims * 100, 2) : 0;

        // FPAR
        bool IsSubmittedStatus(string? status) =>
            status == "Submitted" ||
            status == "Accepted" ||
            status == "Rejected" ||
            status == "Denied" ||
            status == "Paid" ||
            status == "PartiallyPaid";

        var submitted = claims.Count(c => IsSubmittedStatus(c.ClaimStatus));
        var firstPassAccepted = 0;

        foreach (var claim in claims)
        {
            var acks = submissionRefs
                .Where(sr => sr.ClaimId == claim.ClaimId && sr.AckType == "277CA")
                .OrderBy(sr => sr.AckDate)
                .ToList();

            if (acks.Any() && string.Equals(acks.First().AckStatus, "Accepted", StringComparison.OrdinalIgnoreCase))
            {
                firstPassAccepted++;
            }
        }

        var fpar = submitted > 0 ? Math.Round((double)firstPassAccepted / submitted * 100, 2) : 0;

        // DSO
        var dsoValues = new List<double>();
        foreach (var claim in claims)
        {
            var enc = encounters.FirstOrDefault(e => e.EncounterId == claim.EncounterId);
            if (enc == null)
            {
                continue;
            }

            var posts = paymentPosts.Where(p => p.ClaimId == claim.ClaimId && p.PostedDate.HasValue).ToList();
            if (!posts.Any())
            {
                continue;
            }

            var firstPostDate = posts.Min(p => p.PostedDate)!.Value;
            dsoValues.Add((firstPostDate - enc.EncounterDateTime).TotalDays);
        }

        var dso = dsoValues.Any() ? Math.Round(dsoValues.Average(), 1) : 0;

        // Denial rate
        var submittedForDenial = claims.Count(c => c.ClaimStatus != "Draft" && c.ClaimStatus != "ScrubError");

        var deniedClaimIds = denials
            .Where(d => !string.Equals(d.ReasonCode, "UNDERPAYMENT", StringComparison.OrdinalIgnoreCase) && d.ClaimId.HasValue)
            .Select(d => d.ClaimId!.Value)
            .Distinct()
            .ToHashSet();

        var deniedCount = claims.Count(c => deniedClaimIds.Contains(c.ClaimId));
        var denialRate = submittedForDenial > 0
            ? Math.Round((double)deniedCount / submittedForDenial * 100, 2)
            : 0;

        // TAT
        var tatValues = new List<double>();
        foreach (var claim in claims)
        {
            var initial = submissionRefs
                .Where(sr => sr.ClaimId == claim.ClaimId && sr.AckType == null)
                .OrderBy(sr => sr.SubmitDate)
                .FirstOrDefault();

            var ack = submissionRefs
                .Where(sr => sr.ClaimId == claim.ClaimId && sr.AckType == "277CA" && sr.AckDate.HasValue)
                .OrderBy(sr => sr.AckDate)
                .FirstOrDefault();

            if (initial != null && ack?.AckDate != null)
            {
                tatValues.Add((ack.AckDate.Value - initial.SubmitDate).TotalDays);
            }
        }

        var tat = tatValues.Any() ? Math.Round(tatValues.Average(), 1) : 0;

        var totalAccepted = claims.Count(c =>
            c.ClaimStatus == "Accepted" ||
            c.ClaimStatus == "Paid" ||
            c.ClaimStatus == "PartiallyPaid");

        var totalScrubErrors = scrubIssues.Count(si =>
            string.Equals(si.Status, "Open", StringComparison.OrdinalIgnoreCase));

        return new KpiResultDto
        {
            Scope = filters.Scope,
            ScopeId = filters.ScopeId,
            ScopeName = scopeName,
            CleanClaimRate = ccr,
            FirstPassAcceptance = fpar,
            DaysSalesOutstanding = dso,
            DenialRate = denialRate,
            PayerTurnaroundTime = tat,
            TotalClaims = totalClaims,
            TotalSubmitted = submitted,
            TotalAccepted = totalAccepted,
            TotalDenied = deniedCount,
            TotalScrubErrors = totalScrubErrors,
            ComputedAt = DateTime.UtcNow
        };
    }
}

