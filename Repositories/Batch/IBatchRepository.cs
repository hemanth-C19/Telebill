using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.Batch
{
    public interface IBatchRepository
    {
        Task<SubmissionBatch?> GetBatchByIdAsync(int batchID);
    Task<(List<SubmissionBatch> batches, int totalCount)> GetBatchesPagedAsync(
        string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize);
    Task<SubmissionBatch> CreateBatchAsync(SubmissionBatch entity);
    Task UpdateBatchAsync(SubmissionBatch entity);


    // Submission refs
    Task<SubmissionRef?> GetSubmissionRefByIdAsync(int submitID);
    Task<List<SubmissionRef>> GetSubmissionRefsByBatchAsync(int batchID);
    Task<List<SubmissionRef>> GetSubmissionRefsByClaimAsync(int claimID);
    Task<SubmissionRef?> GetSubmissionRefByBatchAndClaimAsync(int batchID, int claimID);
    Task<bool> Has999AckForBatchAsync(int batchID);
    Task<bool> Has277CAAckForClaimInBatchAsync(int batchID, int claimID);
    Task<bool> ClaimAlreadyBatchedAsync(int claimID);
    Task<SubmissionRef> CreateSubmissionRefAsync(SubmissionRef entity);
    Task CreateSubmissionRefsAsync(List<SubmissionRef> entities);
    Task UpdateSubmissionRefsAsync(List<SubmissionRef> entities);
    Task DeleteSubmissionRefAsync(int batchID, int claimID);

    // Cross-module
    Task<Claim?> GetClaimByIdAsync(int claimID);
    Task<List<Claim>> GetClaimsByIdsAsync(List<int> claimIDs);
    Task UpdateClaimStatusAsync(int claimID, string newStatus);
    Task UpdateClaimStatusBulkAsync(List<int> claimIDs, string newStatus);
    Task<bool> ClaimExistsAsync(int claimID);
    Task<X12837pRef?> GetX12RefByClaimIdAsync(int claimID);

    // Users / audit / notify
    Task<List<User>> GetUsersByRoleAsync(string role);
    Task WriteAuditLogAsync(int userID, string action, string resource, string? metadata);
    Task CreateNotificationAsync(int userID, string message, string category);
    }
}