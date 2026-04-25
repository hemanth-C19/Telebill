using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Services;
using Telebill.Dto.Batch;
using Telebill.Models;
using Telebill.Repositories.Batch;

namespace Telebill.Services.Batch;

public class BatchService(IBatchRepository repo, IClaimX12Service x12Service) : IBatchService
{
    public async Task<BatchSummaryDto> CreateBatchAsync(CreateBatchRequestDto dto, int currentUserID)
    {
        var entity = new SubmissionBatch
        {
            BatchDate = dto.BatchDate.ToDateTime(TimeOnly.MinValue),
            ItemCount = 0,
            TotalCharge = 0m,
            Status = "Open"
        };

        entity = await repo.CreateBatchAsync(entity);

        await repo.WriteAuditLogAsync(currentUserID, "CREATE_BATCH", $"SubmissionBatch:{entity.BatchId}",
            JsonSerializer.Serialize(new { batchDate = dto.BatchDate }));

        return MapBatchSummary(entity);
    }

    public async Task<BatchListResponseDto> GetBatchesAsync(string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize)
    {
        pageSize = Math.Min(pageSize, 100);
        var (batches, totalCount) = await repo.GetBatchesPagedAsync(status, dateFrom, dateTo, page, pageSize);
        return new BatchListResponseDto
        {
            TotalCount = totalCount,
            Batches = batches.Select(MapBatchSummary).ToList()
        };
    }

    public async Task<BatchDetailDto> GetBatchDetailAsync(int batchID)
    {
        var batch = await repo.GetBatchByIdAsync(batchID);
        if (batch == null)
        {
            throw new KeyNotFoundException("Batch not found");
        }

        var refs = await repo.GetSubmissionRefsByBatchAsync(batchID);
        var claimIds = refs.Where(r => r.ClaimId != null).Select(r => r.ClaimId!.Value).Distinct().ToList();
        var claims = claimIds.Count == 0 ? new List<Claim>() : await repo.GetClaimsByIdsAsync(claimIds);

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
            var x12 = await repo.GetX12RefByClaimIdAsync(claim.ClaimId);
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
        var batch = await repo.GetBatchByIdAsync(batchID);
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
            var claim = await repo.GetClaimByIdAsync(claimId);
            if (claim == null)
            {
                failed.Add(claimId);
                reasons.Add("Claim not found");
                continue;
            }

            if (!string.Equals(claim.ClaimStatus, "Ready", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(claim.ClaimStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                failed.Add(claimId);
                reasons.Add($"Claim #{claimId} is not in Ready or Rejected status (current: {claim.ClaimStatus})");
                continue;
            }

            if (await repo.ClaimAlreadyBatchedAsync(claimId))
            {
                failed.Add(claimId);
                reasons.Add($"Claim #{claimId} already assigned to a batch");
                continue;
            }

            var x12 = await repo.GetX12RefByClaimIdAsync(claimId);
            if (x12 == null)
            {
                try { await x12Service.Generate837PAsync(claimId); }
                catch
                {
                    failed.Add(claimId);
                    reasons.Add($"Claim #{claimId} does not have an 837P payload and auto-generation failed");
                    continue;
                }
            }

            await repo.UpdateClaimStatusAsync(claimId, "Batched");

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
            await repo.CreateSubmissionRefsAsync(newRefs);
        }

        batch.ItemCount = (batch.ItemCount ?? 0) + success;
        batch.TotalCharge = (batch.TotalCharge ?? 0m) + runningTotal;
        await repo.UpdateBatchAsync(batch);

        await repo.WriteAuditLogAsync(currentUserID, "ADD_CLAIMS_TO_BATCH", $"SubmissionBatch:{batchID}",
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
        var batch = await repo.GetBatchByIdAsync(batchID);
        if (batch == null)
        {
            throw new KeyNotFoundException("Batch not found");
        }

        if (!string.Equals(batch.Status, "Open", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Claims can only be removed from an Open batch");
        }

        var link = await repo.GetSubmissionRefByBatchAndClaimAsync(batchID, claimID);
        if (link == null)
        {
            throw new KeyNotFoundException("Claim is not in this batch");
        }

        var claim = await repo.GetClaimByIdAsync(claimID);
        if (claim == null)
        {
            throw new KeyNotFoundException("Claim not found");
        }

        await repo.UpdateClaimStatusAsync(claimID, "Ready");
        await repo.DeleteSubmissionRefAsync(batchID, claimID);

        batch.ItemCount = Math.Max(0, (batch.ItemCount ?? 0) - 1);
        batch.TotalCharge = (batch.TotalCharge ?? 0m) - (claim.TotalCharge ?? 0m);
        await repo.UpdateBatchAsync(batch);

        await repo.WriteAuditLogAsync(currentUserID, "REMOVE_CLAIM_FROM_BATCH", $"SubmissionBatch:{batchID}",
            JsonSerializer.Serialize(new { claimID, claimTotalCharge = claim.TotalCharge ?? 0m }));
    }

    public async Task<BatchSummaryDto> GenerateBatchAsync(int batchID, int currentUserID)
    {
        var batch = await repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");

        if (!string.Equals(batch.Status, "Open", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only Open batches can be generated");
        }

        if ((batch.ItemCount ?? 0) == 0)
        {
            throw new ArgumentException("Cannot generate an empty batch");
        }

        var refs = await repo.GetSubmissionRefsByBatchAsync(batchID);
        var claimIds = refs.Where(r => r.ClaimId != null).Select(r => r.ClaimId!.Value).Distinct().ToList();
        var missing = new List<int>();
        foreach (var claimId in claimIds)
        {
            var x12 = await repo.GetX12RefByClaimIdAsync(claimId);
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
        await repo.UpdateBatchAsync(batch);

        await repo.WriteAuditLogAsync(currentUserID, "GENERATE_BATCH", $"SubmissionBatch:{batchID}",
            JsonSerializer.Serialize(new { itemCount = batch.ItemCount ?? 0, totalCharge = batch.TotalCharge ?? 0m }));

        var frontDesk = await repo.GetUsersByRoleAsync("FrontDesk");
        foreach (var u in frontDesk)
        {
            await repo.CreateNotificationAsync(u.UserId,
                $"Batch #{batchID} is ready for submission. {(batch.ItemCount ?? 0)} claims, ${(batch.TotalCharge ?? 0m)} total.",
                "Submission");
        }

        return MapBatchSummary(batch);
    }

    public async Task<MarkSubmittedResponseDto> MarkBatchSubmittedAsync(int batchID, MarkSubmittedRequestDto dto, int currentUserID)
    {
        var batch = await repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");

        if (!string.Equals(batch.Status, "Generated", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only Generated batches can be submitted");
        }

        batch.Status = "Submitted";
        await repo.UpdateBatchAsync(batch);

        var refs = await repo.GetSubmissionRefsByBatchAsync(batchID);
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
            await repo.UpdateSubmissionRefsAsync(membershipRefs);
        }

        await repo.UpdateClaimStatusBulkAsync(claimIds, "Submitted");

        await repo.WriteAuditLogAsync(currentUserID, "MARK_BATCH_SUBMITTED", $"SubmissionBatch:{batchID}",
            JsonSerializer.Serialize(new { clearinghouseID = dto.ClearinghouseID, submitDate = dto.SubmitDate, claimsCount = claimIds.Count }));

        var frontDesk = await repo.GetUsersByRoleAsync("FrontDesk");
        foreach (var u in frontDesk)
        {
            await repo.CreateNotificationAsync(u.UserId,
                $"Batch #{batchID} submitted to clearinghouse. {claimIds.Count} claims, total ${(batch.TotalCharge ?? 0m)}",
                "Submission");
        }

        var updatedRefs = await repo.GetSubmissionRefsByBatchAsync(batchID);
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
        var batch = await repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");

        if (!string.Equals(batch.Status, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("999 ACK can only be recorded for Submitted batches");
        }

        if (await repo.Has999AckForBatchAsync(batchID))
        {
            throw new ArgumentException("999 ACK already recorded for this batch");
        }

        var refs = await repo.GetSubmissionRefsByBatchAsync(batchID);
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

        await repo.CreateSubmissionRefsAsync(created);

        if (string.Equals(dto.AckStatus, "Accepted", StringComparison.OrdinalIgnoreCase))
        {
            batch.Status = "Acked";
            var frontDesk = await repo.GetUsersByRoleAsync("FrontDesk");
            foreach (var u in frontDesk)
            {
                await repo.CreateNotificationAsync(u.UserId,
                    $"Batch #{batchID} accepted by clearinghouse",
                    "Ack");
            }
        }
        else
        {
            batch.Status = "Failed";
            await repo.UpdateClaimStatusBulkAsync(claimIds, "Rejected");
            var arUsers = await repo.GetUsersByRoleAsync("AR");
            foreach (var u in arUsers)
            {
                await repo.CreateNotificationAsync(u.UserId,
                    $"Batch #{batchID} rejected by clearinghouse (999). {claimIds.Count} claim(s) set to Rejected — correct and resubmit.",
                    "Ack");
            }
        }
        await repo.UpdateBatchAsync(batch);

        await repo.WriteAuditLogAsync(currentUserID, "RECORD_999_ACK", $"SubmissionBatch:{batchID}",
            JsonSerializer.Serialize(new { ackStatus = dto.AckStatus, correlationID = dto.CorrelationID, claimsAffected = claimIds.Count }));

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
        var batch = await repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");

        if (!string.Equals(batch.Status, "Submitted", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(batch.Status, "Acked", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("277CA ACK can only be recorded for Submitted or Acked batches");
        }

        var claim = await repo.GetClaimByIdAsync(claimID);
        if (claim == null) throw new KeyNotFoundException("Claim not found");

        if (await repo.Has277CAAckForClaimInBatchAsync(batchID, claimID))
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

        created = await repo.CreateSubmissionRefAsync(created);

        var accepted = string.Equals(dto.AckStatus, "Accepted", StringComparison.OrdinalIgnoreCase);
        var newClaimStatus = accepted ? "Accepted" : "Rejected";
        await repo.UpdateClaimStatusAsync(claimID, newClaimStatus);

        await repo.WriteAuditLogAsync(currentUserID, "RECORD_277CA_ACK", $"Claim:{claimID}",
            JsonSerializer.Serialize(new { batchID, ackStatus = dto.AckStatus, correlationID = dto.CorrelationID, notes = dto.Notes }));

        if (accepted)
        {
            var frontDesk = await repo.GetUsersByRoleAsync("FrontDesk");
            foreach (var u in frontDesk)
            {
                await repo.CreateNotificationAsync(u.UserId,
                    $"Claim #{claimID} accepted by payer (277CA) — awaiting adjudication.",
                    "Ack");
            }
        }
        else
        {
            var frontDesk = await repo.GetUsersByRoleAsync("FrontDesk");
            foreach (var u in frontDesk)
            {
                await repo.CreateNotificationAsync(u.UserId,
                    $"Claim #{claimID} rejected by payer (277CA). Correct and resubmit in a new batch.",
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
        var batch = await repo.GetBatchByIdAsync(batchID);
        if (batch == null) throw new KeyNotFoundException("Batch not found");
        var refs = await repo.GetSubmissionRefsByBatchAsync(batchID);
        return refs.Select(MapSubmissionRef).ToList();
    }

    public async Task<List<SubmissionRefDto>> GetSubmissionRefsByClaimAsync(int claimID)
    {
        if (!await repo.ClaimExistsAsync(claimID))
        {
            throw new KeyNotFoundException("Claim not found");
        }
        var refs = await repo.GetSubmissionRefsByClaimAsync(claimID);
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

