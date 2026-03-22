using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telebill.Dto;
using Telebill.Models;
using Telebill.Repositories.Claims;

namespace Services;

public class ClaimScrubService(IClaimRepository repo, IClaimX12Service x12Service) : IClaimScrubService
{
    public async Task<ScrubResultDto?> ScrubClaimAsync(int claimID)
    {
        var claim = await repo.GetByIdWithLinesAsync(claimID);
        if (claim == null)
        {
            throw new KeyNotFoundException("Claim not found");
        }

        var rules = await repo.GetActiveScrubRulesAsync();
        var encounter = await repo.GetEncounterByIdAsync(claim.EncounterId ?? 0);
        Coverage? coverage = null;
        PayerPlan? plan = null;
        Provider? provider = null;
        List<Diagnosis> diagnoses = new();

        if (encounter != null)
        {
            if (encounter.PatientId.HasValue)
            {
                coverage = await repo.GetActiveCoverageForEncounterAsync(
                    encounter.PatientId.Value,
                    encounter.EncounterDateTime);

                if (coverage?.PlanId != null)
                {
                    plan = await repo.GetPayerPlanByIdAsync(coverage.PlanId.Value);
                }
            }

            if (encounter.ProviderId.HasValue)
            {
                provider = await repo.GetProviderByIdAsync(encounter.ProviderId.Value);
            }

            diagnoses = await repo.GetActiveDiagnosesByEncounterAsync(encounter.EncounterId);
        }

        var existingIssues = await repo.GetIssuesByClaimIDAsync(claimID, "Open");

        int totalRulesEvaluated = 0;
        int newIssues = 0;
        int autoResolved = 0;

        foreach (var rule in rules)
        {
            totalRulesEvaluated++;

            var type = GetRuleType(rule.ExpressionJson);
            switch (type)
            {
                case "not_null":
                    (newIssues, autoResolved) = await EvaluateNotNullRule(rule, claim, encounter, claim.ClaimLines, existingIssues, newIssues, autoResolved);
                    break;
                case "valid_pos":
                    (newIssues, autoResolved) = await EvaluateValidPosRule(rule, claim, claim.ClaimLines, existingIssues, newIssues, autoResolved);
                    break;
                case "valid_npi":
                    (newIssues, autoResolved) = await EvaluateValidNpiRule(rule, provider, existingIssues, claimID, newIssues, autoResolved);
                    break;
                case "coverage_date":
                    (newIssues, autoResolved) = await EvaluateCoverageDateRule(rule, encounter, coverage, existingIssues, claimID, newIssues, autoResolved);
                    break;
                case "dx_pointer_required":
                    (newIssues, autoResolved) = await EvaluateDxPointerRequiredRule(rule, claim, claim.ClaimLines, existingIssues, newIssues, autoResolved);
                    break;
                case "primary_diagnosis":
                    (newIssues, autoResolved) = await EvaluatePrimaryDiagnosisRule(rule, diagnoses, existingIssues, claimID, newIssues, autoResolved);
                    break;
                case "modifier_accepted":
                    (newIssues, autoResolved) = await EvaluateModifierAcceptedRule(rule, plan, claim, claim.ClaimLines, existingIssues, newIssues, autoResolved);
                    break;
                case "fee_schedule_missing":
                    (newIssues, autoResolved) = await EvaluateFeeScheduleMissingRule(rule, plan, encounter, claim.ClaimLines, claim.ClaimId, existingIssues, newIssues, autoResolved);
                    break;
                case "no_coverage":
                    (newIssues, autoResolved) = await EvaluateNoCoverageRule(rule, coverage, existingIssues, claimID, newIssues, autoResolved);
                    break;
            }
        }

        var openErrors = await repo.CountOpenErrorsAsync(claimID);
        var openWarnings = await repo.CountOpenWarningsAsync(claimID);

        if (openErrors == 0)
        {
            await repo.UpdateStatusAsync(claimID, "Ready");
            await x12Service.Generate837PAsync(claimID);
            claim.ClaimStatus = "Ready";
        }
        else
        {
            await repo.UpdateStatusAsync(claimID, "ScrubError");
            claim.ClaimStatus = "ScrubError";
        }

        var allIssues = await repo.GetIssuesByClaimIDAsync(claimID, "all");

        return new ScrubResultDto
        {
            ClaimID = claimID,
            ScrubRunAt = DateTime.UtcNow,
            ClaimStatus = claim.ClaimStatus ?? string.Empty,
            TotalRulesEvaluated = totalRulesEvaluated,
            NewIssuesCreated = newIssues,
            IssuesAutoResolved = autoResolved,
            OpenErrors = openErrors,
            OpenWarnings = openWarnings,
            Issues = allIssues.Select(i => new ScrubIssueDto
            {
                IssueID = i.IssueId,
                ClaimID = i.ClaimId ?? claimID,
                ClaimLineID = i.ClaimLineId,
                LineNo = claim.ClaimLines.FirstOrDefault(l => l.ClaimLineId == i.ClaimLineId)?.LineNo,
                RuleID = i.RuleId ?? 0,
                RuleName = i.Rule?.Name ?? string.Empty,
                Severity = i.Rule?.Severity ?? string.Empty,
                Message = i.Message ?? string.Empty,
                DetectedDate = i.DetectedDate ?? DateTime.MinValue,
                Status = i.Status ?? string.Empty
            }).ToList()
        };
    }

    public async Task<List<ScrubIssueDto>> GetScrubIssuesAsync(int claimID, string statusFilter)
    {
        var issues = await repo.GetIssuesByClaimIDAsync(claimID, statusFilter);
        return issues.Select(i => new ScrubIssueDto
        {
            IssueID = i.IssueId,
            ClaimID = i.ClaimId ?? claimID,
            ClaimLineID = i.ClaimLineId,
            RuleID = i.RuleId ?? 0,
            RuleName = i.Rule?.Name ?? string.Empty,
            Severity = i.Rule?.Severity ?? string.Empty,
            Message = i.Message ?? string.Empty,
            DetectedDate = i.DetectedDate ?? DateTime.MinValue,
            Status = i.Status ?? string.Empty
        }).ToList();
    }

    public async Task<ResolveIssueResponseDto?> ResolveIssueAsync(int claimID, int issueID, ResolveIssueRequestDto dto)
    {
        var issue = await repo.GetIssueByIdAsync(issueID);
        if (issue == null || issue.ClaimId != claimID)
        {
            throw new KeyNotFoundException("Scrub issue not found");
        }

        if (issue.Status == "Resolved")
        {
            throw new ArgumentException("Issue already resolved");
        }

        await repo.ResolveIssueAsync(issueID);

        var scrubResult = await ScrubClaimAsync(claimID);

        return new ResolveIssueResponseDto
        {
            IssueID = issueID,
            Status = "Resolved",
            ClaimID = claimID,
            NewClaimStatus = scrubResult?.ClaimStatus ?? string.Empty,
            RemainingOpenErrors = scrubResult?.OpenErrors ?? 0,
            RemainingOpenWarnings = scrubResult?.OpenWarnings ?? 0
        };
    }

    public async Task<ScrubBatchResultDto> RunScrubBatchAsync()
    {
        var started = DateTime.UtcNow;
        var allDraftOrError = (await repo.GetClaimsPagedAsync(new ClaimFilterParams
        {
            Page = 1,
            PageSize = int.MaxValue
        })).claims.Where(c =>
            c.ClaimStatus == "Draft" ||
            c.ClaimStatus == "ScrubError").ToList();

        int movedToReady = 0;
        int stillInError = 0;

        foreach (var claim in allDraftOrError)
        {
            var result = await ScrubClaimAsync(claim.ClaimId);
            if (result != null && result.ClaimStatus == "Ready")
            {
                movedToReady++;
            }
            else
            {
                stillInError++;
            }
        }

        var completed = DateTime.UtcNow;

        return new ScrubBatchResultDto
        {
            JobID = Guid.NewGuid().ToString("N"),
            StartedAt = started,
            CompletedAt = completed,
            ClaimsProcessed = allDraftOrError.Count,
            ClaimsMovedToReady = movedToReady,
            ClaimsStillInError = stillInError,
            DurationSeconds = (completed - started).TotalSeconds
        };
    }

    private static string GetRuleType(string? expressionJson)
    {
        if (string.IsNullOrWhiteSpace(expressionJson))
        {
            return string.Empty;
        }

        try
        {
            using var doc = JsonDocument.Parse(expressionJson);
            if (doc.RootElement.TryGetProperty("type", out var typeProp))
            {
                return typeProp.GetString() ?? string.Empty;
            }
        }
        catch
        {
        }

        return string.Empty;
    }

    private async Task<(int newIssues, int autoResolved)> EvaluateNotNullRule(
        ScrubRule rule,
        Claim claim,
        Encounter? encounter,
        IEnumerable<ClaimLine> claimLines,
        List<ScrubIssue> existing,
        int newIssues,
        int autoResolved)
    {
        var type = "claim";
        var field = "SubscriberRel";

        try
        {
            using var doc = JsonDocument.Parse(rule.ExpressionJson ?? "{}");
            if (doc.RootElement.TryGetProperty("target", out var t))
            {
                type = t.GetString() ?? type;
            }
            if (doc.RootElement.TryGetProperty("field", out var f))
            {
                field = f.GetString() ?? field;
            }
        }
        catch
        {
        }

        if (string.Equals(type, "claim", StringComparison.OrdinalIgnoreCase))
        {
            bool nullOrEmpty = field switch
            {
                "SubscriberRel" => string.IsNullOrWhiteSpace(claim.SubscriberRel),
                _ => false
            };

            (newIssues, autoResolved) = await HandleIssue(rule, claim.ClaimId, null, nullOrEmpty, existing, newIssues, autoResolved,
                $"Claim field '{field}' is required.");
        }
        else if (string.Equals(type, "claim_line", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var line in claimLines)
            {
                bool nullOrEmpty = field switch
                {
                    "POS" => string.IsNullOrWhiteSpace(line.Pos),
                    _ => false
                };

                (newIssues, autoResolved) = await HandleIssue(rule, claim.ClaimId, line.ClaimLineId, nullOrEmpty, existing, newIssues, autoResolved,
                    $"Claim Line {line.LineNo}: Field '{field}' is required.");
            }
        }
        return (newIssues, autoResolved);
    }
    private async Task<(int newIssues, int autoResolved)> EvaluateValidPosRule(
        ScrubRule rule,
        Claim claim,
        IEnumerable<ClaimLine> claimLines,
        List<ScrubIssue> existing,
        int newIssues,
        int autoResolved)
    {
        foreach (var line in claimLines)
        {
            var pos = line.Pos;
            var isValid = pos == "02" || pos == "10";
            (newIssues, autoResolved) = await HandleIssue(rule, claim.ClaimId, line.ClaimLineId, !isValid, existing, newIssues, autoResolved,
                $"Claim Line {line.LineNo}: Invalid POS '{pos}'. Must be 02 or 10.");
        }
        return (newIssues, autoResolved);
    }
    private async Task<(int newIssues, int autoResolved)> EvaluateValidNpiRule(
        ScrubRule rule,
        Provider? provider,
        List<ScrubIssue> existing,
        int claimID,
        int newIssues,
        int autoResolved)
    {
        var npi = provider?.Npi ?? string.Empty;
        var isValid = npi.Length == 10 && npi.All(char.IsDigit);
        (newIssues, autoResolved) = await HandleIssue(rule, claimID, null, !isValid, existing, newIssues, autoResolved,
            $"Provider NPI '{npi}' is not valid. Must be 10 numeric digits.");
        return (newIssues, autoResolved);
    }
    private async Task<(int newIssues, int autoResolved)> EvaluateCoverageDateRule(
        ScrubRule rule,
        Encounter? encounter,
        Coverage? coverage,
        List<ScrubIssue> existing,
        int claimID,
        int newIssues,
        int autoResolved)
    {
        if (encounter == null || coverage == null)
        {
            return (newIssues, autoResolved);
        }

        var serviceDate = encounter.EncounterDateTime;
        var serviceDateOnly = DateOnly.FromDateTime(serviceDate);
        var ok = coverage.EffectiveFrom <= serviceDateOnly &&
                 (coverage.EffectiveTo == null || coverage.EffectiveTo >= serviceDateOnly);

        var message =
            $"Service date {serviceDate:yyyy-MM-dd} is outside coverage effective dates ({coverage.EffectiveFrom:yyyy-MM-dd} to {coverage.EffectiveTo:yyyy-MM-dd}).";

        (newIssues, autoResolved) = await HandleIssue(rule, claimID, null, !ok, existing, newIssues, autoResolved, message);
        return (newIssues, autoResolved);
    }
    private async Task<(int newIssues, int autoResolved)> EvaluateDxPointerRequiredRule(
        ScrubRule rule,
        Claim claim,
        IEnumerable<ClaimLine> claimLines,
        List<ScrubIssue> existing,
        int newIssues,
        int autoResolved)
    {
        foreach (var line in claimLines)
        {
            var dx = ClaimJsonHelper.SafeDeserializeIntList(line.DxPointers);
            var missing = dx.Count == 0;
            (newIssues, autoResolved) = await HandleIssue(rule, claim.ClaimId, line.ClaimLineId, missing, existing, newIssues, autoResolved,
                $"Claim Line {line.LineNo}: No diagnosis pointer assigned.");
        }
        return (newIssues, autoResolved);
    }
    private async Task<(int newIssues, int autoResolved)> EvaluatePrimaryDiagnosisRule(
        ScrubRule rule,
        List<Diagnosis> diagnoses,
        List<ScrubIssue> existing,
        int claimID,
        int newIssues,
        int autoResolved)
    {
        var hasPrimary = diagnoses.Any(d => d.Sequence == 1);
        (newIssues, autoResolved) = await HandleIssue(rule, claimID, null, !hasPrimary, existing, newIssues, autoResolved,
            "No primary diagnosis (Sequence 1) found for this encounter.");
        return (newIssues, autoResolved);
    }
    private async Task<(int newIssues, int autoResolved)> EvaluateModifierAcceptedRule(
        ScrubRule rule,
        PayerPlan? plan,
        Claim claim,
        IEnumerable<ClaimLine> claimLines,
        List<ScrubIssue> existing,
        int newIssues,
        int autoResolved)
    {
        if (plan == null || string.IsNullOrWhiteSpace(plan.TelehealthModifiersJson))
        {
            return (newIssues, autoResolved);
        }

        List<string> accepted = new();
        try
        {
            using var doc = JsonDocument.Parse(plan.TelehealthModifiersJson);
            if (doc.RootElement.TryGetProperty("accepted", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var m in arr.EnumerateArray())
                {
                    accepted.Add(m.GetString() ?? string.Empty);
                }
            }
        }
        catch
        {
        }

        foreach (var line in claimLines)
        {
            var mods = ClaimJsonHelper.SafeDeserializeStringList(line.Modifiers);
            foreach (var mod in mods)
            {
                if (!accepted.Contains(mod))
                {
                    var msg =
                        $"Claim Line {line.LineNo}: Modifier '{mod}' is not accepted by {plan.PlanName}.";
                    (newIssues, autoResolved) = await HandleIssue(rule, claim.ClaimId, line.ClaimLineId, true, existing, newIssues, autoResolved,
                        msg);
                }
            }
        }
        return (newIssues, autoResolved);
    }
    private async Task<(int newIssues, int autoResolved)> EvaluateFeeScheduleMissingRule(
        ScrubRule rule,
        PayerPlan? plan,
        Encounter? encounter,
        IEnumerable<ClaimLine> claimLines,
        int claimID,
        List<ScrubIssue> existing,
        int newIssues,
        int autoResolved)
    {
        if (plan == null || encounter == null)
        {
            return (newIssues, autoResolved);
        }

        foreach (var line in claimLines)
        {
            var found = await repo.GetFeeScheduleAsync(
                plan.PlanId,
                line.CptHcpcs ?? string.Empty,
                line.Modifiers,
                encounter.EncounterDateTime);

            if (found == null)
            {
                var msg =
                    $"Claim Line {line.LineNo}: No fee schedule found for CPT {line.CptHcpcs} under {plan.PlanName}.";
                (newIssues, autoResolved) = await HandleIssue(rule, claimID, line.ClaimLineId, true, existing, newIssues, autoResolved,
                    msg);
            }
        }
        return (newIssues, autoResolved);
    }
    private async Task<(int newIssues, int autoResolved)> EvaluateNoCoverageRule(
        ScrubRule rule,
        Coverage? coverage,
        List<ScrubIssue> existing,
        int claimID,
        int newIssues,
        int autoResolved)
    {
        var missing = coverage == null;
        (newIssues, autoResolved) = await HandleIssue(rule, claimID, null, missing, existing, newIssues, autoResolved,
            "No active coverage found for patient on service date.");
        return (newIssues, autoResolved);
    }
    private async Task<(int newIssues, int autoResolved)> HandleIssue(
        ScrubRule rule,
        int claimID,
        int? claimLineID,
        bool failed,
        List<ScrubIssue> existing,
        int newIssues,
        int autoResolved,
        string message)
    {
        var existingIssue = existing.FirstOrDefault(i =>
            i.ClaimId == claimID &&
            i.RuleId == rule.RuleId &&
            i.ClaimLineId == claimLineID);

        if (failed)
        {
            if (existingIssue == null)
            {
                var issue = new ScrubIssue
                {
                    ClaimId = claimID,
                    ClaimLineId = claimLineID,
                    RuleId = rule.RuleId,
                    Message = message,
                    DetectedDate = DateTime.UtcNow,
                    Status = "Open"
                };
                await repo.CreateIssueAsync(issue);
                newIssues++;
            }
        }
        else
        {
            if (existingIssue != null && existingIssue.Status == "Open")
            {
                await repo.ResolveIssueByRuleAsync(claimID, rule.RuleId, claimLineID);
                autoResolved++;
            }
        }
        return (newIssues, autoResolved);
    }
}

