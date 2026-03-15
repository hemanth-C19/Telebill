using System;
using System.Text.Json;
using Telebill.Dto.Batch;
using Telebill.Models;
using Telebill.Repositories.Batch;
namespace Telebill.Services.Batch
{
    public class BatchService:IBatchService
    {
        private readonly IBatchRepository _repo;

    public BatchService(IBatchRepository repo)
    {
        _repo = repo;
    }

    public async Task<BatchSummaryDto> CreateBatchAsync(CreateBatchRequestDto dto, int currentUserID)
    {
        var entity = new SubmissionBatch
        {
            BatchDate = dto.BatchDate.ToDateTime(TimeOnly.MinValue),
            ItemCount = 0,
            TotalCharge = 0m,
            Status = "Open"
        };

        entity = await _repo.CreateBatchAsync(entity);

        await _repo.WriteAuditLogAsync(currentUserID, "CREATE_BATCH", $"SubmissionBatch:{entity.BatchId}",
            JsonSerializer.Serialize(new { batchDate = dto.BatchDate }));

        return MapBatchSummary(entity);
    }

    public async Task<BatchListResponseDto> GetBatchesAsync(string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize)
    {
        pageSize = Math.Min(pageSize, 100);
        //(batches, totalCount) = 
        (List<SubmissionBatch> batches, int totalCount) =
        await _repo.GetBatchesPagedAsync(status, dateFrom, dateTo, page, pageSize);
        return new BatchListResponseDto
        {
            TotalCount = totalCount,
            Batches = batches.Select(MapBatchSummary).ToList()
        };
    }

    public async Task<BatchDetailDto> GetBatchDetailAsync(int batchID)
    {
        var batch = await _repo.GetBatchByIdAsync(batchID);
        if (batch == null)
        {
            throw new KeyNotFoundException("Batch not found");
        }

        var refs = await _repo.GetSubmissionRefsByBatchAsync(batchID);
        var claimIds = refs.Where(r => r.ClaimId != null).Select(r => r.ClaimId!.Value).Distinct().ToList();
        var claims = claimIds.Count == 0 ? new List<Claim>() : await _repo.GetClaimsByIdsAsync(claimIds);

        var detail = new BatchDetailDto
        {
            BatchID = batch.BatchId,
            BatchDate = ToDateOnly(batch.BatchDate),
            ItemCount = batch.ItemCount ?? 0,
            TotalCharge = batch.TotalCharge ?? 0m,
            Status = batch.Status ?? string.Empty
        };

        var refsByClaim = refs.GroupBy(r => r.ClaimId ?? 0).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var claim in claims)
        {
            var x12 = await _repo.GetX12RefByClaimIdAsync(claim.ClaimId);
            var claimRefs = refsByClaim.TryGetValue(claim.ClaimId, out var list) ? list : new List<SubmissionRef>();

            detail.Claims.Add(new BatchClaimLineDto
            {
                ClaimID = claim.ClaimId,
                PatientName = claim.Patient?.Name ?? string.Empty,
                PlanName = claim.Plan?.PlanName ?? string.Empty,
                PayerName = claim.Plan?.Payer?.Name ?? string.Empty,
                TotalCharge = claim.TotalCharge ?? 0m,
                ClaimStatus = claim.ClaimStatus ?? string.Empty,
                PayloadUri = x12?.PayloadUri,
                SubmissionRefs = claimRefs.Select(MapSubmissionRef).ToList()
            });
        }

        return detail;
    }

    public async Task<AddClaimsResponseDto> AddClaimsToBatchAsync(int batchID, AddClaimsToBatchRequestDto dto, int currentUserID)
    {
        var batch = await _repo.GetBatchByIdAsync(batchID);
        if (batch == null)
        {
            throw new KeyNotFoundException("Batch not found");
        }

        if (!string.Equals(batch.Status, "Open", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Claims can only be added to an Open batch");
        }

        int success = 0;
        decimal runningTotal = 0m;
        var failed = new List<int>();
        var reasons = new List<string>();

        var newRefs = new List<SubmissionRef>();

        foreach (var claimId in dto.ClaimIDs.Distinct())
        {
            var claim = await _repo.GetClaimByIdAsync(claimId);
            if (claim == null)
            {
                failed.Add(claimId);
                reasons.Add("Claim not found");
                continue;
            }

            if (!string.Equals(claim.ClaimStatus, "Ready", StringComparison.OrdinalIgnoreCase))
            {
                failed.Add(claimId);
                reasons.Add($"Claim #{claimId} is not in Ready status (current: {claim.ClaimStatus})");
                continue;
            }

            if (await _repo.ClaimAlreadyBatchedAsync(claimId))
            {
                failed.Add(claimId);
                reasons.Add($"Claim #{claimId} already assigned to a batch");
                continue;
            }

            var x12 = await _repo.GetX12RefByClaimIdAsync(claimId);
            if (x12 == null)
            {
                failed.Add(claimId);
                reasons.Add($"Claim #{claimId} does not have a generated 837P payload");
                continue;
            }

            await _repo.UpdateClaimStatusAsync(claimId, "Batched");

            newRefs.Add(new SubmissionRef
            {
                BatchId = batchID,
                ClaimId = claimId,
                SubmitDate = DateTime.UtcNow.Date,
                AckType = null,
                AckStatus = null,
                AckDate = null,
                ClearinghouseId = null,
                CorrelationId = null,
                Notes = null
            });

            success++;
            runningTotal += claim.TotalCharge ?? 0m;
        }

        if (newRefs.Count > 0)
        {
            await _repo.CreateSubmissionRefsAsync(newRefs);
        }

        batch.ItemCount = (batch.ItemCount ?? 0) + success;
        batch.TotalCharge = (batch.TotalCharge ?? 0m) + runningTotal;
        await _repo.UpdateBatchAsync(batch);

        await _repo.WriteAuditLogAsync(currentUserID, "ADD_CLAIMS_TO_BATCH", $"SubmissionBatch:{batchID}",
            JsonSerializer.Serialize(new { claimsAdded = success, failedCount = failed.Count }));

        return new AddClaimsResponseDto
        {
            BatchID = batchID,
            ClaimsAdded = success,
            FailedClaimIDs = failed,
            FailureReasons = reasons,
            UpdatedItemCount = batch.ItemCount ?? 0,
            UpdatedTotalCharge = batch.TotalCharge ?? 0m
        };
    }

    public async Task RemoveClaimFromBatchAsync(int batchID, int claimID, int currentUserID)
    {
        var batch = await _repo.GetBatchByIdAsync(batchID);
        if (batch == null)
        {
            throw new KeyNotFoundException("Batch not found");
        }

        if (!string.Equals(batch.Status, "Open", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Claims can only be removed from an Open batch");
        }

        var link = await _repo.GetSubmissionRefByBatchAndClaimAsync(batchID, claimID);
        if (link == null)
        {
            throw new KeyNotFoundException("Claim is not in this batch");
        }

        var claim = await _repo.GetClaimByIdAsync(claimID);
        if (claim == null)
        {
            throw new KeyNotFoundException("Claim not found");
        }

        await _repo.UpdateClaimStatusAsync(claimID, "Ready");
        await _repo.DeleteSubmissionRefAsync(batchID, claimID);

        batch.ItemCount = Math.Max(0, (batch.ItemCount ?? 0) - 1);
        batch.TotalCharge = (batch.TotalCharge ?? 0m) - (claim.TotalCharge ?? 0m);
        await _repo.UpdateBatchAsync(batch);

        await _repo.WriteAuditLogAsync(currentUserID, "REMOVE_CLAIM_FROM_BATCH", $"SubmissionBatch:{batchID}",
            JsonSerializer.Serialize(new { claimID, claimTotalCharge = claim.TotalCharge ?? 0m }));
    }

    public async Task<BatchSummaryDto> GenerateBatchAsync(int batchID, int currentUserID)
    {
        var batch = await _repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");

        if (!string.Equals(batch.Status, "Open", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only Open batches can be generated");
        }

        if ((batch.ItemCount ?? 0) == 0)
        {
            throw new ArgumentException("Cannot generate an empty batch");
        }

        var refs = await _repo.GetSubmissionRefsByBatchAsync(batchID);
        var claimIds = refs.Where(r => r.ClaimId != null).Select(r => r.ClaimId!.Value).Distinct().ToList();
        var missing = new List<int>();
        foreach (var claimId in claimIds)
        {
            var x12 = await _repo.GetX12RefByClaimIdAsync(claimId);
            if (x12 == null)
            {
                missing.Add(claimId);
            }
        }

        if (missing.Count > 0)
        {
            throw new ArgumentException($"Claims {string.Join(",", missing)} do not have a generated 837P payload. Generate them first.");
        }

        batch.Status = "Generated";
        await _repo.UpdateBatchAsync(batch);

        await _repo.WriteAuditLogAsync(currentUserID, "GENERATE_BATCH", $"SubmissionBatch:{batchID}",
            JsonSerializer.Serialize(new { itemCount = batch.ItemCount ?? 0, totalCharge = batch.TotalCharge ?? 0m }));

        var frontDesk = await _repo.GetUsersByRoleAsync("FrontDesk");
        foreach (var u in frontDesk)
        {
            await _repo.CreateNotificationAsync(u.UserId,
                $"Batch #{batchID} is ready for submission. {(batch.ItemCount ?? 0)} claims, ${(batch.TotalCharge ?? 0m)} total.",
                "Submission");
        }

        return MapBatchSummary(batch);
    }

    public async Task<MarkSubmittedResponseDto> MarkBatchSubmittedAsync(int batchID, MarkSubmittedRequestDto dto, int currentUserID)
    {
        var batch = await _repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");

        if (!string.Equals(batch.Status, "Generated", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only Generated batches can be submitted");
        }

        batch.Status = "Submitted";
        await _repo.UpdateBatchAsync(batch);

        var refs = await _repo.GetSubmissionRefsByBatchAsync(batchID);
        var claimIds = refs.Where(r => r.ClaimId != null).Select(r => r.ClaimId!.Value).Distinct().ToList();

        // Update existing membership refs (AckType == null) to become the initial "submitted" refs
        var membershipRefs = refs.Where(r => r.AckType == null).ToList();
        foreach (var r in membershipRefs)
        {
            r.ClearinghouseId = dto.ClearinghouseID;
            r.SubmitDate = dto.SubmitDate.ToDateTime(TimeOnly.MinValue);
        }
        if (membershipRefs.Count > 0)
        {
            await _repo.UpdateSubmissionRefsAsync(membershipRefs);
        }

        await _repo.UpdateClaimStatusBulkAsync(claimIds, "Submitted");

        await _repo.WriteAuditLogAsync(currentUserID, "MARK_BATCH_SUBMITTED", $"SubmissionBatch:{batchID}",
            JsonSerializer.Serialize(new { clearinghouseID = dto.ClearinghouseID, submitDate = dto.SubmitDate, claimsCount = claimIds.Count }));

        var frontDesk = await _repo.GetUsersByRoleAsync("FrontDesk");
        foreach (var u in frontDesk)
        {
            await _repo.CreateNotificationAsync(u.UserId,
                $"Batch #{batchID} marked as submitted to {dto.ClearinghouseID}. Awaiting 999 acknowledgement.",
                "Submission");
        }

        var updatedRefs = await _repo.GetSubmissionRefsByBatchAsync(batchID);
        return new MarkSubmittedResponseDto
        {
            BatchID = batchID,
            BatchStatus = "Submitted",
            ClaimsUpdated = claimIds.Count,
            SubmissionRefs = updatedRefs.Where(r => r.AckType == null).Select(MapSubmissionRef).ToList()
        };
    }

    public async Task<Record999AckResponseDto> Record999AckAsync(int batchID, Record999AckRequestDto dto, int currentUserID)
    {
        var batch = await _repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");

        if (!string.Equals(batch.Status, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("999 ACK can only be recorded for Submitted batches");
        }

        if (await _repo.Has999AckForBatchAsync(batchID))
        {
            throw new ArgumentException("999 ACK already recorded for this batch");
        }

        var refs = await _repo.GetSubmissionRefsByBatchAsync(batchID);
        var claimIds = refs.Where(r => r.AckType == null && r.ClaimId != null).Select(r => r.ClaimId!.Value).Distinct().ToList();

        var created = claimIds.Select(claimId => new SubmissionRef
        {
            BatchId = batchID,
            ClaimId = claimId,
            ClearinghouseId = dto.ClearinghouseID,
            CorrelationId = dto.CorrelationID,
            SubmitDate = DateTime.UtcNow.Date,
            AckType = "999",
            AckStatus = dto.AckStatus,
            AckDate = dto.AckDate.ToDateTime(TimeOnly.MinValue),
            Notes = dto.Notes
        }).ToList();

        await _repo.CreateSubmissionRefsAsync(created);

        if (string.Equals(dto.AckStatus, "Accepted", StringComparison.OrdinalIgnoreCase))
        {
            batch.Status = "Acked";
        }
        else
        {
            batch.Status = "Failed";
            await _repo.UpdateClaimStatusBulkAsync(claimIds, "Rejected");
        }
        await _repo.UpdateBatchAsync(batch);

        await _repo.WriteAuditLogAsync(currentUserID, "RECORD_999_ACK", $"SubmissionBatch:{batchID}",
            JsonSerializer.Serialize(new { ackStatus = dto.AckStatus, correlationID = dto.CorrelationID, claimsAffected = claimIds.Count }));

        var frontDesk = await _repo.GetUsersByRoleAsync("FrontDesk");
        foreach (var u in frontDesk)
        {
            var msg = string.Equals(dto.AckStatus, "Rejected", StringComparison.OrdinalIgnoreCase)
                ? $"Batch #{batchID} REJECTED by clearinghouse (999 ACK). All {claimIds.Count} claims have been set to Rejected. Reason: {dto.Notes}. Please correct and resubmit."
                : $"Batch #{batchID} accepted by clearinghouse (999 ACK). Awaiting payer 277CA claim-level acknowledgements.";
            await _repo.CreateNotificationAsync(u.UserId, msg, "Ack");
        }

        return new Record999AckResponseDto
        {
            BatchID = batchID,
            AckStatus = dto.AckStatus,
            ClaimsInBatch = claimIds.Count,
            BatchStatus = batch.Status ?? string.Empty,
            CreatedRefs = created.Select(MapSubmissionRef).ToList()
        };
    }

    public async Task<Record277CAAckResponseDto> Record277CAAckAsync(int batchID, int claimID, Record277CAAckRequestDto dto, int currentUserID)
    {
        var batch = await _repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");

        if (!string.Equals(batch.Status, "Submitted", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(batch.Status, "Acked", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("277CA ACK can only be recorded for Submitted or Acked batches");
        }

        var claim = await _repo.GetClaimByIdAsync(claimID);
        if (claim == null) throw new KeyNotFoundException("Claim not found");

        if (await _repo.Has277CAAckForClaimInBatchAsync(batchID, claimID))
        {
            throw new ArgumentException("277CA ACK already recorded for this claim");
        }

        var created = new SubmissionRef
        {
            BatchId = batchID,
            ClaimId = claimID,
            ClearinghouseId = null,
            CorrelationId = dto.CorrelationID,
            SubmitDate = DateTime.UtcNow.Date,
            AckType = "277CA",
            AckStatus = dto.AckStatus,
            AckDate = dto.AckDate.ToDateTime(TimeOnly.MinValue),
            Notes = dto.Notes
        };

        created = await _repo.CreateSubmissionRefAsync(created);

        var newClaimStatus = string.Equals(dto.AckStatus, "Accepted", StringComparison.OrdinalIgnoreCase) ? "Accepted" : "Rejected";
        await _repo.UpdateClaimStatusAsync(claimID, newClaimStatus);

        await _repo.WriteAuditLogAsync(currentUserID, "RECORD_277CA_ACK", $"Claim:{claimID}",
            JsonSerializer.Serialize(new { batchID, ackStatus = dto.AckStatus, correlationID = dto.CorrelationID, notes = dto.Notes }));

        if (string.Equals(dto.AckStatus, "Accepted", StringComparison.OrdinalIgnoreCase))
        {
            var arUsers = await _repo.GetUsersByRoleAsync("AR");
            foreach (var u in arUsers)
            {
                await _repo.CreateNotificationAsync(u.UserId,
                    $"Claim #{claimID} accepted by payer (277CA). Awaiting ERA/payment posting.",
                    "Ack");
            }
        }
        else
        {
            var frontDesk = await _repo.GetUsersByRoleAsync("FrontDesk");
            foreach (var u in frontDesk)
            {
                await _repo.CreateNotificationAsync(u.UserId,
                    $"Claim #{claimID} REJECTED by payer (277CA). Reason: {dto.Notes}. Please correct and resubmit.",
                    "Ack");
            }
        }

        return new Record277CAAckResponseDto
        {
            SubmitID = created.SubmitId,
            ClaimID = claimID,
            AckStatus = dto.AckStatus,
            NewClaimStatus = newClaimStatus
        };
    }

    public async Task<List<SubmissionRefDto>> GetSubmissionRefsForBatchAsync(int batchID)
    {
        var batch = await _repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");
        var refs = await _repo.GetSubmissionRefsByBatchAsync(batchID);
        return refs.Select(MapSubmissionRef).ToList();
    }

    public async Task<List<SubmissionRefDto>> GetSubmissionRefsByClaimAsync(int claimID)
    {
        if (!await _repo.ClaimExistsAsync(claimID))
        {
            throw new KeyNotFoundException("Claim not found");
        }
        var refs = await _repo.GetSubmissionRefsByClaimAsync(claimID);
        return refs.Select(MapSubmissionRef).ToList();
    }

    private static BatchSummaryDto MapBatchSummary(SubmissionBatch b)
    {
        return new BatchSummaryDto
        {
            BatchID = b.BatchId,
            BatchDate = ToDateOnly(b.BatchDate),
            ItemCount = b.ItemCount ?? 0,
            TotalCharge = b.TotalCharge ?? 0m,
            Status = b.Status ?? string.Empty
        };
    }

    private static SubmissionRefDto MapSubmissionRef(SubmissionRef r)
    {
        return new SubmissionRefDto
        {
            SubmitID = r.SubmitId,
            BatchID = r.BatchId ?? 0,
            ClaimID = r.ClaimId ?? 0,
            ClearinghouseID = r.ClearinghouseId,
            CorrelationID = r.CorrelationId,
            SubmitDate = ToDateOnly(r.SubmitDate),
            AckType = r.AckType,
            AckStatus = r.AckStatus,
            AckDate = r.AckDate == null ? null : ToDateOnly(r.AckDate),
            Notes = r.Notes
        };
    }

    private static DateOnly ToDateOnly(DateTime? dt)
    {
        return dt == null ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.FromDateTime(dt.Value);
    }

    }
}