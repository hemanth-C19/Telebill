using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.PreCert;

public interface IPreCertRepository
{
    // PriorAuth
    Task<PriorAuth?> GetPriorAuthByIdAsync(int paid);
    Task<List<PriorAuth>> GetPriorAuthsAsync(int? claimID, int? planID, string? status, bool? expiringSoon);
    Task<List<PriorAuth>> GetPriorAuthsByClaimAsync(int claimID);
    Task<bool> ActivePriorAuthExistsForClaimAsync(int claimID);
    Task<bool> HasApprovedPriorAuthAsync(int claimID, DateOnly encounterDate);
    Task<PriorAuth> CreatePriorAuthAsync(PriorAuth entity);
    Task UpdatePriorAuthAsync(PriorAuth entity);
    Task<List<PriorAuth>> GetExpiredPriorAuthsAsync();
    Task<List<PriorAuth>> GetExpiringSoonPriorAuthsAsync(int daysAhead);

    // AttachmentRef
    Task<AttachmentRef?> GetAttachmentByIdAsync(int attachId);
    Task<List<AttachmentRef>> GetAttachmentsByClaimAsync(int claimID, string status);
    Task<AttachmentRef> CreateAttachmentAsync(AttachmentRef entity);
    Task UpdateAttachmentAsync(AttachmentRef entity);

    // Cross-module reads
    Task<bool> ClaimExistsAsync(int claimID);
    Task<bool> PayerPlanExistsAsync(int planID);
    Task<PayerPlan?> GetPayerPlanWithPayerAsync(int planID);
    Task<User?> GetUserByIdAsync(int userID);
    Task<List<User>> GetUsersByRoleAsync(string role);

    // Audit / notifications
    Task WriteAuditLogAsync(int userID, string action, string resource, string? metadata);
    Task CreateNotificationAsync(int userID, string message, string category);
}

