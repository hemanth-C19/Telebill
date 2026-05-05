using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto;
using Telebill.Repositories.Claims;

namespace Services;

public class ClaimQueryService(IClaimRepository repo) : IClaimQueryService
{
    public async Task<ClaimListResponseDto> GetClaimsAsync(ClaimFilterParams filters)
    {
        var (claims, totalCount) = await repo.GetClaimsPagedAsync(filters);

        var items = new List<ClaimSummaryDto>();

        foreach (var c in claims)
        {
            var encounter = c.Encounter;
            var plan = c.Plan;
            var payerName = plan?.Payer?.Name ?? string.Empty;
            var providerName = encounter?.Provider?.Name ?? string.Empty;

            var openErrors = await repo.CountOpenErrorsAsync(c.ClaimId);
            var openWarnings = await repo.CountOpenWarningsAsync(c.ClaimId);

            items.Add(new ClaimSummaryDto
            {
                ClaimID = c.ClaimId,
                EncounterID = c.EncounterId ?? 0,
                PatientName = c.Patient?.Name ?? string.Empty,
                PlanName = plan?.PlanName ?? string.Empty,
                PayerName = payerName,
                ProviderName = providerName,
                ServiceDate = encounter?.EncounterDateTime ?? DateTime.MinValue,
                TotalCharge = c.TotalCharge ?? 0m,
                ClaimStatus = c.ClaimStatus ?? string.Empty,
                OpenScrubErrors = openErrors,
                OpenScrubWarnings = openWarnings,
                CreatedDate = c.CreatedDate ?? DateTime.MinValue
            });
        }

        return new ClaimListResponseDto
        {
            TotalCount = totalCount,
            Page = filters.Page,
            PageSize = filters.PageSize,
            Claims = items
        };
    }

    public async Task<ClaimDetailDto?> GetClaimDetailAsync(int claimID)
    {
        var claim = await repo.GetByIdWithLinesAsync(claimID);
        if (claim == null)
        {
            throw new KeyNotFoundException("Claim not found");
        }

        var detail = new ClaimDetailDto
        {
            ClaimID = claim.ClaimId,
            EncounterID = claim.EncounterId ?? 0,
            PatientID = claim.PatientId ?? 0,
            PatientName = claim.Patient?.Name ?? string.Empty,
            PlanID = claim.PlanId ?? 0,
            PlanName = claim.Plan?.PlanName ?? string.Empty,
            PayerName = claim.Plan?.Payer?.Name ?? string.Empty,
            SubscriberRel = claim.SubscriberRel ?? string.Empty,
            TotalCharge = claim.TotalCharge ?? 0m,
            ClaimStatus = claim.ClaimStatus ?? string.Empty,
            CreatedDate = claim.CreatedDate ?? DateTime.MinValue
        };

        foreach (var line in claim.ClaimLines)
        {
            var dxPointers = ClaimJsonHelper.SafeDeserializeIntList(line.DxPointers);
            var modifiers = ClaimJsonHelper.SafeDeserializeStringList(line.Modifiers);

            var openIssues = claim.ScrubIssues.Count(i =>
                i.ClaimLineId == line.ClaimLineId &&
                i.Status == "Open");

            detail.ClaimLines.Add(new ClaimLineDto
            {
                ClaimLineID = line.ClaimLineId,
                LineNo = line.LineNo ?? 0,
                CPT_HCPCS = line.CptHcpcs ?? string.Empty,
                Modifiers = modifiers,
                Units = line.Units ?? 0,
                ChargeAmount = line.ChargeAmount ?? 0m,
                DxPointers = dxPointers,
                POS = line.Pos ?? string.Empty,
                LineStatus = line.LineStatus ?? string.Empty,
                OpenIssues = openIssues
            });
        }

        foreach (var issue in claim.ScrubIssues)
        {
            detail.ScrubIssues.Add(new ScrubIssueDto
            {
                IssueID = issue.IssueId,
                ClaimID = issue.ClaimId ?? claim.ClaimId,
                ClaimLineID = issue.ClaimLineId,
                LineNo = claim.ClaimLines.FirstOrDefault(l => l.ClaimLineId == issue.ClaimLineId)?.LineNo,
                RuleID = issue.RuleId ?? 0,
                RuleName = issue.Rule?.Name ?? string.Empty,
                Severity = issue.Rule?.Severity ?? string.Empty,
                Message = issue.Message ?? string.Empty,
                DetectedDate = issue.DetectedDate ?? DateTime.MinValue,
                Status = issue.Status ?? string.Empty
            });
        }

        var x12 = claim.X12837pRefs.FirstOrDefault();
        if (x12 != null)
        {
            detail.X12Ref = new X12RefDto
            {
                X12ID = x12.X12id,
                ClaimID = claim.ClaimId,
                PayloadURI = x12.PayloadUri ?? string.Empty,
                PreSignedURL = x12.PayloadUri ?? string.Empty,
                GeneratedDate = x12.GeneratedDate ?? DateTime.MinValue,
                Version = x12.Version ?? string.Empty,
                Status = x12.Status ?? string.Empty
            };
        }

        return detail;
    }

    public async Task<ClaimStatusSummaryDto?> GetClaimSummaryAsync(int claimID)
    {
        var claim = await repo.GetByIdAsync(claimID);
        if (claim == null)
        {
            throw new KeyNotFoundException("Claim not found");
        }

        var openErrors = await repo.CountOpenErrorsAsync(claimID);
        var openWarnings = await repo.CountOpenWarningsAsync(claimID);
        var hasPriorAuth = await repo.HasApprovedPriorAuthAsync(claimID);
        var x12 = await repo.GetX12RefByClaimIDAsync(claimID);

        return new ClaimStatusSummaryDto
        {
            ClaimID = claim.ClaimId,
            ClaimStatus = claim.ClaimStatus ?? string.Empty,
            TotalCharge = claim.TotalCharge ?? 0m,
            OpenScrubErrors = openErrors,
            OpenScrubWarnings = openWarnings,
            HasPriorAuth = hasPriorAuth,
            Has837PRef = x12 != null
        };
    }
}

