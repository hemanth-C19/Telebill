using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Batch;

namespace Telebill.Services.Batch;

public interface IBatchService
{
    Task<BatchSummaryDto> CreateBatchAsync(CreateBatchRequestDto dto, int currentUserID);
    Task<BatchListResponseDto> GetBatchesAsync(string? status, DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize);
    Task<BatchDetailDto> GetBatchDetailAsync(int batchID);
    Task<AddClaimsResponseDto> AddClaimsToBatchAsync(int batchID, AddClaimsToBatchRequestDto dto, int currentUserID);
    Task RemoveClaimFromBatchAsync(int batchID, int claimID, int currentUserID);
    Task<BatchSummaryDto> GenerateBatchAsync(int batchID, int currentUserID);
    Task<MarkSubmittedResponseDto> MarkBatchSubmittedAsync(int batchID, MarkSubmittedRequestDto dto, int currentUserID);

    Task<Record999AckResponseDto> Record999AckAsync(int batchID, Record999AckRequestDto dto, int currentUserID);
    Task<Record277CAAckResponseDto> Record277CAAckAsync(int batchID, int claimID, Record277CAAckRequestDto dto, int currentUserID);

    Task<List<SubmissionRefDto>> GetSubmissionRefsForBatchAsync(int batchID);
    Task<List<SubmissionRefDto>> GetSubmissionRefsByClaimAsync(int claimID);
}

