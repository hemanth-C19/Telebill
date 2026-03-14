using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telebill.Dto;
using Telebill.Models;
using Telebill.Repositories.Claims;

namespace Services;

public class ClaimScrubService : IClaimScrubService
{
    private readonly IClaimRepository _repo;
    private readonly IClaimX12Service _x12Service;

    public ClaimScrubService(IClaimRepository repo, IClaimX12Service x12Service)
    {
        _repo = repo;
        _x12Service = x12Service;
    }

    public async Task<ScrubResultDto?> ScrubClaimAsync(int claimID)
    {
        var claim = await _repo.GetByIdWithLinesAsync(claimID);
        if (claim == null)
        {
            return null;
        }

        var rules = await _repo.GetActiveScrubRulesAsync();
        var encounter = await _repo.GetEncounterByIdAsync(claim.EncounterId ?? 0);
        Coverage? coverage = null;
        PayerPlan? plan = null;
        Provider? provider = null;
        List<Diagnosis> diagnoses = new();

        if (encounter != null)
        {
            if (encounter.PatientId.HasValue)
            {
                coverage = await _repo.GetActiveCoverageForEncounterAsync(
                    encounter.PatientId.Value,
                    encounter.EncounterDateTime);

                if (coverage?.PlanId != null)
                {
                    plan = await _repo.GetPayerPlanByIdAsync(coverage.PlanId.Value);
                }
            }

            if (encounter.ProviderId.HasValue)
            {
                provider = await _repo.GetProviderByIdAsync(encounter.ProviderId.Value);
            }

            diagnoses = await _repo.GetActiveDiagnosesByEncounterAsync(encounter.EncounterId);
        }

        var existingIssues = await _repo.GetIssuesByClaimIDAsync(claimID, "Open");

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
                    EvaluateNotNullRule(rule, claim, encounter, claim.ClaimLines, existingIssues, ref newIssues, ref autoResolved);
                    break;
                case "valid_pos":
                    EvaluateValidPosRule(rule, claim, claim.ClaimLines, existingIssues, ref newIssues, ref autoResolved);
                    break;
                case "valid_npi":
                    EvaluateValidNpiRule(rule, provider, existingIssues, claimID, ref newIssues, ref autoResolved);
                    break;
                case "coverage_date":
                    EvaluateCoverageDateRule(rule, encounter, coverage, existingIssues, claimID, ref newIssues, ref autoResolved);
                    break;
                case "dx_pointer_required":
                    EvaluateDxPointerRequiredRule(rule, claim, claim.ClaimLines, existingIssues, ref newIssues, ref autoResolved);
                    break;
                case "primary_diagnosis":
                    EvaluatePrimaryDiagnosisRule(rule, diagnoses, existingIssues, claimID, ref newIssues, ref autoResolved);
                    break;
                case "modifier_accepted":
                    EvaluateModifierAcceptedRule(rule, plan, claim, claim.ClaimLines, existingIssues, ref newIssues, ref autoResolved);
                    break;
                case "fee_schedule_missing":
                    EvaluateFeeScheduleMissingRule(rule, plan, encounter, claim.ClaimLines, existingIssues, ref newIssues, ref autoResolved);
                    break;
                case "no_coverage":
                    EvaluateNoCoverageRule(rule, coverage, existingIssues, claimID, ref newIssues, ref autoResolved);
                    break;
            }
        }

        var openErrors = await _repo.CountOpenErrorsAsync(claimID);
        var openWarnings = await _repo.CountOpenWarningsAsync(claimID);

        if (openErrors == 0)
        {
            await _repo.UpdateStatusAsync(claimID, "Ready");
            await _x12Service.Generate837PAsync(claimID);
            claim.ClaimStatus = "Ready";
        }
        else
        {
            await _repo.UpdateStatusAsync(claimID, "ScrubError");
            claim.ClaimStatus = "ScrubError";
        }

        var allIssues = await _repo.GetIssuesByClaimIDAsync(claimID, "all");

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
        var issues = await _repo.GetIssuesByClaimIDAsync(claimID, statusFilter);
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
        var issue = await _repo.GetIssueByIdAsync(issueID);
        if (issue == null || issue.ClaimId != claimID)
        {
            return null;
        }

        if (issue.Status == "Resolved")
        {
            throw new ArgumentException("Issue already resolved");
        }

        await _repo.ResolveIssueAsync(issueID);

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
        var allDraftOrError = (await _repo.GetClaimsPagedAsync(new ClaimFilterParams
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

    private void EvaluateNotNullRule(
        ScrubRule rule,
        Claim claim,
        Encounter? encounter,
        IEnumerable<ClaimLine> claimLines,
        List<ScrubIssue> existing,
        ref int newIssues,
        ref int autoResolved)
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

            HandleIssue(rule, claim.ClaimId, null, nullOrEmpty, existing, ref newIssues, ref autoResolved,
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

                HandleIssue(rule, claim.ClaimId, line.ClaimLineId, nullOrEmpty, existing, ref newIssues, ref autoResolved,
                    $"Claim Line {line.LineNo}: Field '{field}' is required.");
            }
        }
    }

    private void EvaluateValidPosRule(
        ScrubRule rule,
        Claim claim,
        IEnumerable<ClaimLine> claimLines,
        List<ScrubIssue> existing,
        ref int newIssues,
        ref int autoResolved)
    {
        foreach (var line in claimLines)
        {
            var pos = line.Pos;
            var isValid = pos == "02" || pos == "10";
            HandleIssue(rule, claim.ClaimId, line.ClaimLineId, !isValid, existing, ref newIssues, ref autoResolved,
                $"Claim Line {line.LineNo}: Invalid POS '{pos}'. Must be 02 or 10.");
        }
    }

    private void EvaluateValidNpiRule(
        ScrubRule rule,
        Provider? provider,
        List<ScrubIssue> existing,
        int claimID,
        ref int newIssues,
        ref int autoResolved)
    {
        var npi = provider?.Npi ?? string.Empty;
        var isValid = npi.Length == 10 && npi.All(char.IsDigit);
        HandleIssue(rule, claimID, null, !isValid, existing, ref newIssues, ref autoResolved,
            $"Provider NPI '{npi}' is not valid. Must be 10 numeric digits.");
    }

    private void EvaluateCoverageDateRule(
        ScrubRule rule,
        Encounter? encounter,
        Coverage? coverage,
        List<ScrubIssue> existing,
        int claimID,
        ref int newIssues,
        ref int autoResolved)
    {
        if (encounter == null || coverage == null)
        {
            return;
        }

        var serviceDate = encounter.EncounterDateTime;
        var ok = coverage.EffectiveFrom <= serviceDate &&
                 (coverage.EffectiveTo == null || coverage.EffectiveTo >= serviceDate);

        var message =
            $"Service date {serviceDate:yyyy-MM-dd} is outside coverage effective dates ({coverage.EffectiveFrom:yyyy-MM-dd} to {coverage.EffectiveTo:yyyy-MM-dd}).";

        HandleIssue(rule, claimID, null, !ok, existing, ref newIssues, ref autoResolved, message);
    }

    private void EvaluateDxPointerRequiredRule(
        ScrubRule rule,
        Claim claim,
        IEnumerable<ClaimLine> claimLines,
        List<ScrubIssue> existing,
        ref int newIssues,
        ref int autoResolved)
    {
        foreach (var line in claimLines)
        {
            var dx = ClaimJsonHelper.SafeDeserializeIntList(line.DxPointers);
            var missing = dx.Count == 0;
            HandleIssue(rule, claim.ClaimId, line.ClaimLineId, missing, existing, ref newIssues, ref autoResolved,
                $"Claim Line {line.LineNo}: No diagnosis pointer assigned.");
        }
    }

    private void EvaluatePrimaryDiagnosisRule(
        ScrubRule rule,
        List<Diagnosis> diagnoses,
        List<ScrubIssue> existing,
        int claimID,
        ref int newIssues,
        ref int autoResolved)
    {
        var hasPrimary = diagnoses.Any(d => d.Sequence == 1);
        HandleIssue(rule, claimID, null, !hasPrimary, existing, ref newIssues, ref autoResolved,
            "No primary diagnosis (Sequence 1) found for this encounter.");
    }

    private void EvaluateModifierAcceptedRule(
        ScrubRule rule,
        PayerPlan? plan,
        Claim claim,
        IEnumerable<ClaimLine> claimLines,
        List<ScrubIssue> existing,
        ref int newIssues,
        ref int autoResolved)
    {
        if (plan == null || string.IsNullOrWhiteSpace(plan.TelehealthModifiersJson))
        {
            return;
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
                    HandleIssue(rule, claim.ClaimId, line.ClaimLineId, true, existing, ref newIssues, ref autoResolved,
                        msg);
                }
            }
        }
    }

    private void EvaluateFeeScheduleMissingRule(
        ScrubRule rule,
        PayerPlan? plan,
        Encounter? encounter,
        IEnumerable<ClaimLine> claimLines,
        List<ScrubIssue> existing,
        ref int newIssues,
        ref int autoResolved)
    {
        if (plan == null || encounter == null)
        {
            return;
        }

        foreach (var line in claimLines)
        {
            var found = _repo.GetFeeScheduleAsync(
                plan.PlanId,
                line.CptHcpcs ?? string.Empty,
                line.Modifiers,
                encounter.EncounterDateTime).Result;

            if (found == null)
            {
                var msg =
                    $"Claim Line {line.LineNo}: No fee schedule found for CPT {line.CptHcpcs} under {plan.PlanName}.";
                HandleIssue(rule, claim.ClaimId, line.ClaimLineId, true, existing, ref newIssues, ref autoResolved,
                    msg);
            }
        }
    }

    private void EvaluateNoCoverageRule(
        ScrubRule rule,
        Coverage? coverage,
        List<ScrubIssue> existing,
        int claimID,
        ref int newIssues,
        ref int autoResolved)
    {
        var missing = coverage == null;
        HandleIssue(rule, claimID, null, missing, existing, ref newIssues, ref autoResolved,
            "No active coverage found for patient on service date.");
    }

    private async void HandleIssue(
        ScrubRule rule,
        int claimID,
        int? claimLineID,
        bool failed,
        List<ScrubIssue> existing,
        ref int newIssues,
        ref int autoResolved,
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
                await _repo.CreateIssueAsync(issue);
                newIssues++;
            }
        }
        else
        {
            if (existingIssue != null && existingIssue.Status == "Open")
            {
                await _repo.ResolveIssueByRuleAsync(claimID, rule.RuleId, claimLineID);
                autoResolved++;
            }
        }
    }
}

