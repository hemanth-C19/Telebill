using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.AR;
using Telebill.Models;
using Telebill.Repositories.AR;

namespace Telebill.Services.AR;

public class DenialService : IDenialService
{
    private readonly IArRepository _repo;

    public DenialService(IArRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ArWorklistItemDto>> GetArWorklistAsync(ArWorklistFilterParams filters)
    {
        var denials = await _repo.GetDenialsAsync(filters);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var result = new List<ArWorklistItemDto>();

        foreach (var denial in denials)
        {
            if (!denial.ClaimId.HasValue)
            {
                continue;
            }

            var claim = await _repo.GetClaimByIdAsync(denial.ClaimId.Value);
            if (claim == null)
            {
                continue;
            }

            var patient = claim.PatientId.HasValue
                ? await _repo.GetPatientByIdAsync(claim.PatientId.Value)
                : null;

            PayerPlan? plan = claim.PlanId.HasValue
                ? await _repo.GetPayerPlanByIdAsync(claim.PlanId.Value)
                : null;

            Payer? payer = plan != null
                ? await _repo.GetPayerByPlanIdAsync(plan.PlanId)
                : null;

            var subs = await _repo.GetSubmissionRefsByClaimIdAsync(denial.ClaimId.Value);
            var enc = await _repo.GetEncounterByClaimIdAsync(denial.ClaimId.Value);

            var denialDate = denial.DenialDate ?? DateOnly.FromDateTime(DateTime.Today);
            var days = today.DayNumber - denialDate.DayNumber;

            var aging = days <= 30 ? "0-30"
                : days <= 60 ? "31-60"
                : days <= 90 ? "61-90"
                : "90+";

            result.Add(new ArWorklistItemDto
            {
                DenialId = denial.DenialId,
                ClaimId = denial.ClaimId.Value,
                PatientName = patient?.Name,
                PayerName = payer?.Name,
                PlanName = plan?.PlanName,
                EncounterDateTime = enc?.EncounterDateTime ?? DateTime.MinValue,
                TotalCharge = claim.TotalCharge ?? 0m,
                AmountDenied = denial.AmountDenied ?? 0m,
                ReasonCode = denial.ReasonCode,
                RemarkCode = denial.RemarkCode,
                DenialStatus = denial.Status,
                DenialDate = denialDate,
                DaysSinceDenial = days,
                AgingBucket = aging,
                ClaimStatus = claim.ClaimStatus,
                SubmissionCount = subs.Count(s => s.AckType == null)
            });
        }

        return result
            .OrderByDescending(x => x.DaysSinceDenial)
            .ToList();
    }

    public async Task<DenialDetailDto?> GetDenialDetailAsync(int denialId)
    {
        var denial = await _repo.GetDenialByIdAsync(denialId);
        if (denial == null || !denial.ClaimId.HasValue)
        {
            return null;
        }

        var claim = await _repo.GetClaimByIdAsync(denial.ClaimId.Value);
        if (claim == null)
        {
            return null;
        }

        var lines = await _repo.GetClaimLinesByClaimIdAsync(claim.ClaimId);
        var patient = claim.PatientId.HasValue
            ? await _repo.GetPatientByIdAsync(claim.PatientId.Value)
            : null;

        PayerPlan? plan = claim.PlanId.HasValue
            ? await _repo.GetPayerPlanByIdAsync(claim.PlanId.Value)
            : null;

        Payer? payer = plan != null
            ? await _repo.GetPayerByPlanIdAsync(plan.PlanId)
            : null;

        var enc = await _repo.GetEncounterByClaimIdAsync(claim.ClaimId);
        var payments = await _repo.GetPaymentPostsByClaimIdAsync(claim.ClaimId);
        var submissions = await _repo.GetSubmissionRefsByClaimIdAsync(claim.ClaimId);
        var attachments = await _repo.GetAttachmentsByClaimIdAsync(claim.ClaimId);

        var claimSummary = new ClaimSummaryForArDto
        {
            ClaimId = claim.ClaimId,
            ClaimStatus = claim.ClaimStatus,
            TotalCharge = claim.TotalCharge ?? 0m,
            PatientName = patient?.Name,
            PayerName = payer?.Name,
            PlanName = plan?.PlanName,
            EncounterDateTime = enc?.EncounterDateTime ?? DateTime.MinValue,
            Pos = enc?.Pos,
            Lines = lines.Select(l => new ClaimLineSummaryDto
            {
                ClaimLineId = l.ClaimLineId,
                LineNo = l.LineNo ?? 0,
                CptHcpcs = l.CptHcpcs,
                Modifiers = l.Modifiers,
                Units = l.Units,
                ChargeAmount = l.ChargeAmount,
                DxPointers = l.DxPointers
            }).ToList()
        };

        var paymentHistory = payments.Select(p => new PaymentPostSummaryDto
        {
            PaymentPostId = p.PaymentId,
            ClaimLineId = p.ClaimLineId,
            AmountPaid = p.AmountPaid ?? 0m,
            AdjustmentJson = p.AdjustmentJson,
            PostedDate = p.PostedDate ?? DateTime.MinValue,
            Status = p.Status
        }).ToList();

        var submissionHistory = submissions
            .Where(s => s.AckType != null)
            .Select(s => new SubmissionHistoryDto
            {
                SubmissionRefId = s.SubmitId,
                SubmitDate = s.SubmitDate ?? DateTime.MinValue,
                AckType = s.AckType,
                AckStatus = s.AckStatus,
                AckDate = s.AckDate,
                CorrelationId = s.CorrelationId
            }).ToList();

        var attachmentDtos = attachments.Select(a => new AttachmentSummaryDto
        {
            AttachId = a.AttachId,
            FileType = a.FileType,
            FileUri = a.FileUri,
            Notes = a.Notes,
            UploadedDate = a.UploadedDate ?? DateTime.MinValue
        }).ToList();

        return new DenialDetailDto
        {
            DenialId = denial.DenialId,
            DenialStatus = denial.Status,
            ReasonCode = denial.ReasonCode,
            RemarkCode = denial.RemarkCode,
            DenialDate = denial.DenialDate ?? DateOnly.FromDateTime(DateTime.Today),
            AmountDenied = denial.AmountDenied ?? 0m,
            Claim = claimSummary,
            PaymentHistory = paymentHistory,
            SubmissionHistory = submissionHistory,
            Attachments = attachmentDtos
        };
    }

    public async Task<(bool success, string error)> UpdateDenialStatusAsync(
        int denialId, UpdateDenialStatusDto dto)
    {
        var allowed = new[] { "Appealed", "Resolved", "WrittenOff" };
        if (string.IsNullOrWhiteSpace(dto.NewStatus) ||
            !allowed.Contains(dto.NewStatus, StringComparer.OrdinalIgnoreCase))
        {
            return (false,
                $"Invalid status '{dto.NewStatus}'. Allowed: Appealed, Resolved, WrittenOff");
        }

        var denial = await _repo.GetDenialByIdAsync(denialId);
        if (denial == null)
        {
            return (false, "Denial not found");
        }

        if (string.Equals(denial.Status, "Resolved", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(denial.Status, "WrittenOff", StringComparison.OrdinalIgnoreCase))
        {
            return (false, $"Denial is already {denial.Status} and cannot be changed");
        }

        var current = denial.Status ?? "Open";
        var target = dto.NewStatus;

        bool validTransition = current switch
        {
            "Open" => target is "Appealed" or "Resolved" or "WrittenOff",
            "Appealed" => target is "Resolved" or "WrittenOff",
            _ => false
        };

        if (!validTransition)
        {
            return (false, $"Cannot change status from {current} to {target}");
        }

        denial.Status = target;
        await _repo.UpdateDenialAsync(denial);

        return (true, string.Empty);
    }

    public async Task<(bool success, string error, AttachmentSummaryDto? result)> UploadAppealDocumentAsync(
        UploadAppealDocumentDto dto)
    {
        var denial = await _repo.GetDenialByIdAsync(dto.DenialId);
        if (denial == null)
        {
            return (false, "Denial not found", null);
        }

        var status = denial.Status ?? "Open";
        if (!string.Equals(status, "Open", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(status, "Appealed", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Can only attach documents to Open or Appealed denials", null);
        }

        if (string.IsNullOrWhiteSpace(dto.FileUri))
        {
            return (false, "FileUri is required", null);
        }

        if (!denial.ClaimId.HasValue)
        {
            return (false, "Denial does not have a linked claim", null);
        }

        var attachment = new AttachmentRef
        {
            ClaimId = denial.ClaimId,
            FileType = dto.FileType,
            FileUri = dto.FileUri,
            Notes = dto.Notes,
            UploadedBy = dto.UploadedBy,
            UploadedDate = DateTime.UtcNow,
            Status = "Active"
        };

        attachment = await _repo.AddAttachmentAsync(attachment);

        if (string.Equals(status, "Open", StringComparison.OrdinalIgnoreCase))
        {
            denial.Status = "Appealed";
            await _repo.UpdateDenialAsync(denial);
        }

        var result = new AttachmentSummaryDto
        {
            AttachId = attachment.AttachId,
            FileType = attachment.FileType,
            FileUri = attachment.FileUri,
            Notes = attachment.Notes,
            UploadedDate = attachment.UploadedDate ?? DateTime.MinValue
        };

        return (true, string.Empty, result);
    }

    public async Task<(bool success, string error, ResetClaimResponseDto? result)> ResetClaimForResubmissionAsync(
        ResetClaimForResubmissionDto dto)
    {
        var denial = await _repo.GetDenialByIdAsync(dto.DenialId);
        if (denial == null || !denial.ClaimId.HasValue)
        {
            return (false, "Denial not found", null);
        }

        if (string.Equals(denial.Status, "Resolved", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(denial.Status, "WrittenOff", StringComparison.OrdinalIgnoreCase))
        {
            return (false, $"Denial is already {denial.Status}. Cannot resubmit.", null);
        }

        var claim = await _repo.GetClaimByIdAsync(denial.ClaimId.Value);
        if (claim == null)
        {
            return (false, "Parent claim not found", null);
        }

        var status = claim.ClaimStatus ?? string.Empty;
        var resettable = new[] { "Denied", "Rejected", "ScrubError" };
        if (!resettable.Contains(status, StringComparer.OrdinalIgnoreCase))
        {
            return (false,
                $"Claim status '{status}' cannot be reset. " +
                "Only Denied, Rejected, or ScrubError claims can be resubmitted.", null);
        }

        await _repo.UpdateClaimStatusAsync(denial.ClaimId.Value, "Draft");
        denial.Status = "Resolved";
        await _repo.UpdateDenialAsync(denial);

        var response = new ResetClaimResponseDto
        {
            ClaimId = denial.ClaimId.Value,
            ClaimStatus = "Draft",
            DenialId = denial.DenialId,
            DenialStatus = "Resolved"
        };

        return (true, string.Empty, response);
    }
}

