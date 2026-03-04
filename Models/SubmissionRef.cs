using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class SubmissionRef
{
    public int SubmitId { get; set; }

    public int? BatchId { get; set; }

    public int? ClaimId { get; set; }

    public string? ClearinghouseId { get; set; }

    public string? CorrelationId { get; set; }

    public DateTime? SubmitDate { get; set; }

    public string? AckType { get; set; }

    public string? AckStatus { get; set; }

    public DateTime? AckDate { get; set; }

    public string? Notes { get; set; }

    public virtual SubmissionBatch? Batch { get; set; }

    public virtual Claim? Claim { get; set; }
}
