using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Telebill.Dto.PreCert;
using Telebill.Models;
using Telebill.Repositories.PreCert;

namespace Telebill.Services.PreCert;

public class PreCertService(IPreCertRepository repo) : IPreCertService
{
    public async Task<PriorAuthDto> CreatePriorAuthAsync(CreatePriorAuthRequestDto dto, int currentUserID)
    {
        if (!await repo.ClaimExistsAsync(dto.ClaimID))
        {
            throw new KeyNotFoundException("Claim not found");
        }

        var plan = await repo.GetPayerPlanWithPayerAsync(dto.PlanID);
        if (plan == null)
        {
            throw new KeyNotFoundException("Payer plan not found");
        }

        if (await repo.ActivePriorAuthExistsForClaimAsync(dto.ClaimID))
        {
            throw new InvalidOperationException("An active PA request already exists for this claim");
        }

        var entity = new PriorAuth
        {
            ClaimId = dto.ClaimID,
            PlanId = dto.PlanID,
            AuthNumber = null,
            RequestedDate = dto.RequestedDate,
            ApprovedFrom = null,
            ApprovedTo = null,
            Status = "Requested"
        };

        entity = await repo.CreatePriorAuthAsync(entity);

        await repo.WriteAuditLogAsync(
            currentUserID,
            "CREATE_PRIOR_AUTH",
            $"PriorAuth:{entity.Paid}",
            JsonSerializer.Serialize(new
            {
                claimID = dto.ClaimID,
                planID = dto.PlanID,
                requestedDate = dto.RequestedDate
            }));

        return MapPriorAuth(entity, plan);
    }

    public async Task<PriorAuthListResponseDto> GetPriorAuthsAsync(int? claimID, int? planID, string? status, bool? expiringSoon)
    {
        var items = await repo.GetPriorAuthsAsync(claimID, planID, status, expiringSoon);
        var mapped = items.Select(pa => MapPriorAuth(pa, pa.Plan)).ToList();
        return new PriorAuthListResponseDto
        {
            TotalCount = mapped.Count,
            PriorAuths = mapped
        };
    }

    public async Task<PriorAuthDto> GetPriorAuthByIdAsync(int paid)
    {
        var entity = await repo.GetPriorAuthByIdAsync(paid);
        if (entity == null)
        {
            throw new KeyNotFoundException("PA not found");
        }

        return MapPriorAuth(entity, entity.Plan);
    }

    public async Task<List<PriorAuthDto>> GetPriorAuthsByClaimAsync(int claimID)
    {
        if (!await repo.ClaimExistsAsync(claimID))
        {
            throw new KeyNotFoundException("Claim not found");
        }

        var items = await repo.GetPriorAuthsByClaimAsync(claimID);
        return items.Select(pa => MapPriorAuth(pa, pa.Plan)).ToList();
    }

    public async Task<PriorAuthDto> UpdatePriorAuthAsync(int paid, UpdatePriorAuthRequestDto dto, int currentUserID)
    {
        var entity = await repo.GetPriorAuthByIdAsync(paid);
        if (entity == null)
        {
            throw new KeyNotFoundException("PA not found");
        }

        var previousStatus = entity.Status ?? "Requested";
        var newStatus = dto.Status ?? previousStatus;

        if (dto.Status != null && !IsAllowedPriorAuthTransition(previousStatus, newStatus))
        {
            throw new ArgumentException($"Invalid status transition from {previousStatus} to {newStatus}");
        }

        if (string.Equals(dto.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(dto.AuthNumber))
            {
                throw new ArgumentException("AuthNumber is required when approving a PA");
            }

            if (dto.ApprovedFrom == null)
            {
                throw new ArgumentException("ApprovedFrom is required when approving a PA");
            }

            if (dto.ApprovedTo == null)
            {
                throw new ArgumentException("ApprovedTo is required when approving a PA");
            }

            if (dto.ApprovedTo.Value < dto.ApprovedFrom.Value)
            {
                throw new ArgumentException("ApprovedTo must be after ApprovedFrom");
            }
        }

        if (dto.AuthNumber != null) entity.AuthNumber = dto.AuthNumber;
        if (dto.ApprovedFrom != null) entity.ApprovedFrom = dto.ApprovedFrom;
        if (dto.ApprovedTo != null) entity.ApprovedTo = dto.ApprovedTo;
        if (dto.Status != null) entity.Status = dto.Status;

        await repo.UpdatePriorAuthAsync(entity);

        await repo.WriteAuditLogAsync(
            currentUserID,
            "UPDATE_PRIOR_AUTH",
            $"PriorAuth:{paid}",
            JsonSerializer.Serialize(new
            {
                previousStatus,
                newStatus = entity.Status,
                authNumber = dto.AuthNumber
            }));

        return MapPriorAuth(entity, entity.Plan);
    }

    public async Task SoftDeletePriorAuthAsync(int paid, int currentUserID)
    {
        var entity = await repo.GetPriorAuthByIdAsync(paid);
        if (entity == null)
        {
            throw new KeyNotFoundException("PA not found");
        }

        var previousStatus = entity.Status;
        entity.Status = "Expired";
        await repo.UpdatePriorAuthAsync(entity);

        await repo.WriteAuditLogAsync(
            currentUserID,
            "SOFT_DELETE_PRIOR_AUTH",
            $"PriorAuth:{paid}",
            JsonSerializer.Serialize(new { previousStatus, newStatus = "Expired" }));
    }

    public async Task RunExpiryCheckJobAsync()
    {
        var systemUserId = 0;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var overdue = await repo.GetExpiredPriorAuthsAsync();
        foreach (var pa in overdue)
        {
            pa.Status = "Expired";
            await repo.UpdatePriorAuthAsync(pa);
            await repo.WriteAuditLogAsync(systemUserId, "AUTO_EXPIRE_PRIOR_AUTH", $"PriorAuth:{pa.Paid}", null);
        }

        var expiringSoon = await repo.GetExpiringSoonPriorAuthsAsync(7);
        if (expiringSoon.Count == 0)
        {
            return;
        }

        var frontDeskUsers = await repo.GetUsersByRoleAsync("FrontDesk");
        foreach (var pa in expiringSoon)
        {
            var msg =
                $"Prior Auth {pa.AuthNumber ?? "(no auth #)"} for Claim #{pa.ClaimId} expires on {pa.ApprovedTo}. Please renew or use before expiry.";

            foreach (var user in frontDeskUsers)
            {
                await repo.CreateNotificationAsync(user.UserId, msg, "Scrub");
            }
        }
    }

    public Task<bool> HasApprovedPriorAuthAsync(int claimID, DateOnly encounterDate)
    {
        return repo.HasApprovedPriorAuthAsync(claimID, encounterDate);
    }

    public async Task<AttachmentDto> CreateAttachmentAsync(CreateAttachmentRequestDto dto, int currentUserID)
    {
        if (!await repo.ClaimExistsAsync(dto.ClaimID))
        {
            throw new KeyNotFoundException("Claim not found");
        }

        if (!IsValidFileType(dto.FileType))
        {
            throw new ArgumentException("Invalid file type");
        }

        if (string.IsNullOrWhiteSpace(dto.FileUri))
        {
            throw new ArgumentException("FileUri is required");
        }

        var entity = new AttachmentRef
        {
            ClaimId = dto.ClaimID,
            FileType = dto.FileType,
            FileUri = dto.FileUri,
            Notes = dto.Notes,
            UploadedBy = currentUserID,
            UploadedDate = DateTime.UtcNow,
            Status = "Active"
        };

        entity = await repo.CreateAttachmentAsync(entity);

        await repo.WriteAuditLogAsync(
            currentUserID,
            "CREATE_ATTACHMENT",
            $"AttachmentRef:{entity.AttachId}",
            JsonSerializer.Serialize(new { claimID = dto.ClaimID, fileType = dto.FileType, notes = dto.Notes }));

        var user = await repo.GetUserByIdAsync(currentUserID);

        return MapAttachment(entity, user);
    }

    public async Task<AttachmentListResponseDto> GetAttachmentsByClaimAsync(int claimID, string status)
    {
        if (!await repo.ClaimExistsAsync(claimID))
        {
            throw new KeyNotFoundException("Claim not found");
        }

        var effectiveStatus = string.IsNullOrWhiteSpace(status) ? "Active" : status;
        var items = await repo.GetAttachmentsByClaimAsync(claimID, effectiveStatus);

        var mapped = items.Select(a => MapAttachment(a, a.UploadedByNavigation)).ToList();
        return new AttachmentListResponseDto
        {
            TotalCount = mapped.Count,
            Attachments = mapped
        };
    }

    public async Task<AttachmentDto> GetAttachmentByIdAsync(int attachId)
    {
        var entity = await repo.GetAttachmentByIdAsync(attachId);
        if (entity == null)
        {
            throw new KeyNotFoundException("Attachment not found");
        }

        return MapAttachment(entity, entity.UploadedByNavigation);
    }

    public async Task<AttachmentDto> UpdateAttachmentStatusAsync(int attachId, UpdateAttachmentStatusRequestDto dto, int currentUserID)
    {
        var entity = await repo.GetAttachmentByIdAsync(attachId);
        if (entity == null)
        {
            throw new KeyNotFoundException("Attachment not found");
        }

        if (!IsValidAttachmentStatus(dto.Status))
        {
            throw new ArgumentException("Invalid status value");
        }

        var previousStatus = entity.Status;
        entity.Status = dto.Status;
        await repo.UpdateAttachmentAsync(entity);

        await repo.WriteAuditLogAsync(
            currentUserID,
            "UPDATE_ATTACHMENT_STATUS",
            $"AttachmentRef:{attachId}",
            JsonSerializer.Serialize(new { previousStatus, newStatus = dto.Status }));

        return MapAttachment(entity, entity.UploadedByNavigation);
    }

    private static bool IsAllowedPriorAuthTransition(string current, string next)
    {
        if (string.Equals(current, next, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var allowed = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Requested"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Approved", "Denied" },
            ["Approved"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Expired", "Denied" }
        };

        if (string.Equals(current, "Expired", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(current, "Denied", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return allowed.TryGetValue(current, out var targets) && targets.Contains(next);
    }

    private static PriorAuthDto MapPriorAuth(PriorAuth entity, PayerPlan? plan)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var soonCutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        var isExpiringSoon = string.Equals(entity.Status, "Approved", StringComparison.OrdinalIgnoreCase) &&
                             entity.ApprovedTo != null &&
                             entity.ApprovedTo >= today &&
                             entity.ApprovedTo <= soonCutoff;

        return new PriorAuthDto
        {
            PAID = entity.Paid,
            ClaimID = entity.ClaimId ?? 0,
            PlanID = entity.PlanId ?? 0,
            PlanName = plan?.PlanName ?? string.Empty,
            PayerName = plan?.Payer?.Name ?? string.Empty,
            AuthNumber = entity.AuthNumber,
            RequestedDate = entity.RequestedDate ?? today,
            ApprovedFrom = entity.ApprovedFrom,
            ApprovedTo = entity.ApprovedTo,
            Status = entity.Status ?? "Requested",
            IsExpiringSoon = isExpiringSoon
        };
    }

    private static AttachmentDto MapAttachment(AttachmentRef entity, User? user)
    {
        return new AttachmentDto
        {
            AttachId = entity.AttachId,
            ClaimID = entity.ClaimId ?? 0,
            FileType = entity.FileType ?? string.Empty,
            FileUri = entity.FileUri ?? string.Empty,
            Notes = entity.Notes,
            UploadedBy = entity.UploadedBy ?? 0,
            UploadedByName = user?.Name ?? string.Empty,
            UploadedDate = entity.UploadedDate ?? DateTime.MinValue,
            Status = entity.Status ?? string.Empty
        };
    }

    private static bool IsValidFileType(string fileType)
    {
        return string.Equals(fileType, "PDF", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(fileType, "Image", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(fileType, "Doc", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidAttachmentStatus(string status)
    {
        return string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "Deleted", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "Archived", StringComparison.OrdinalIgnoreCase);
    }
}

