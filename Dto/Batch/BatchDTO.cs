namespace Telebill.Dto.Batch
{
    public class CreateBatchRequestDto
    {
        public DateOnly BatchDate { get; set; }
    }

    public class AddClaimsToBatchRequestDto
    {
        public List<int> ClaimIDs { get; set; } = new();
    }

    public class RemoveClaimFromBatchRequestDto
    {
        public int ClaimID { get; set; }
    }

    public class BatchSummaryDto
    {
        public int BatchID { get; set; }
        public DateOnly BatchDate { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalCharge { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class BatchDetailDto
    {
        public int BatchID { get; set; }
        public DateOnly BatchDate { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalCharge { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<BatchClaimLineDto> Claims { get; set; } = new();
    }

    public class BatchClaimLineDto
    {
        public int ClaimID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string PayerName { get; set; } = string.Empty;
        public decimal TotalCharge { get; set; }
        public string ClaimStatus { get; set; } = string.Empty;
        public string? PayloadUri { get; set; }
        public List<SubmissionRefDto> SubmissionRefs { get; set; } = new();
    }

    public class BatchListResponseDto
    {
        public int TotalCount { get; set; }
        public List<BatchSummaryDto> Batches { get; set; } = new();
    }

    public class AddClaimsResponseDto
    {
        public int BatchID { get; set; }
        public int ClaimsAdded { get; set; }
        public List<int> FailedClaimIDs { get; set; } = new();
        public List<string> FailureReasons { get; set; } = new();
        public int UpdatedItemCount { get; set; }
        public decimal UpdatedTotalCharge { get; set; }
    }
}