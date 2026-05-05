using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.PreCert;

namespace Telebill.Services.PreCert;

public interface IPreCertService
{
    // PriorAuth
    Task<PriorAuthDto> CreatePriorAuthAsync(CreatePriorAuthRequestDto dto, int currentUserID);
    Task<PriorAuthListResponseDto> GetPriorAuthsAsync(int? claimID, int? planID, string? status, bool? expiringSoon);
    Task<PriorAuthDto> GetPriorAuthByIdAsync(int paid);
    Task<List<PriorAuthDto>> GetPriorAuthsByClaimAsync(int claimID);
    Task<PriorAuthDto> UpdatePriorAuthAsync(int paid, UpdatePriorAuthRequestDto dto, int currentUserID);
    Task SoftDeletePriorAuthAsync(int paid, int currentUserID);
    Task RunExpiryCheckJobAsync();

    // AttachmentRef
    Task<AttachmentDto> CreateAttachmentAsync(CreateAttachmentRequestDto dto, int currentUserID);
    Task<AttachmentListResponseDto> GetAttachmentsByClaimAsync(int claimID, string status);
    Task<AttachmentDto> GetAttachmentByIdAsync(int attachId);
    Task<AttachmentDto> UpdateAttachmentStatusAsync(int attachId, UpdateAttachmentStatusRequestDto dto, int currentUserID);

    // Internal use
    Task<bool> HasApprovedPriorAuthAsync(int claimID, DateOnly encounterDate);
}

