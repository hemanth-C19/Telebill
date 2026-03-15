using System;

namespace Telebill.Dto.Batch
{
    public class Record999AckRequestDto
{
    public string ClearinghouseID { get; set; } = string.Empty;
    public string CorrelationID { get; set; } = string.Empty;
    public string AckStatus { get; set; } = string.Empty; // Accepted | Rejected
    public DateOnly AckDate { get; set; }
    public string? Notes { get; set; }
}

public class Record277CAAckRequestDto
{
    public int ClaimID { get; set; }
    public string AckStatus { get; set; } = string.Empty; // Accepted | Rejected
    public DateOnly AckDate { get; set; }
    public string? CorrelationID { get; set; }
    public string? Notes { get; set; }
}

public class MarkSubmittedRequestDto
{
    public string ClearinghouseID { get; set; } = string.Empty;
    public DateOnly SubmitDate { get; set; }
}

public class SubmissionRefDto
{
    public int SubmitID { get; set; }
    public int BatchID { get; set; }
    public int ClaimID { get; set; }
    public string? ClearinghouseID { get; set; }
    public string? CorrelationID { get; set; }
    public DateOnly SubmitDate { get; set; }
    public string? AckType { get; set; }
    public string? AckStatus { get; set; }
    public DateOnly? AckDate { get; set; }
    public string? Notes { get; set; }
}

public class Record999AckResponseDto
{
    public int BatchID { get; set; }
    public string AckStatus { get; set; } = string.Empty;
    public int ClaimsInBatch { get; set; }
    public string BatchStatus { get; set; } = string.Empty;
    public List<SubmissionRefDto> CreatedRefs { get; set; } = new();
}

public class Record277CAAckResponseDto
{
    public int SubmitID { get; set; }
    public int ClaimID { get; set; }
    public string AckStatus { get; set; } = string.Empty;
    public string NewClaimStatus { get; set; } = string.Empty;
}

public class MarkSubmittedResponseDto
{
    public int BatchID { get; set; }
    public string BatchStatus { get; set; } = string.Empty;
    public int ClaimsUpdated { get; set; }
    public List<SubmissionRefDto> SubmissionRefs { get; set; } = new();
}
}