using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class ScrubIssue
{
    public int IssueId { get; set; }

    public int? ClaimId { get; set; }

    public int? ClaimLineId { get; set; }

    public int? RuleId { get; set; }

    public string? Message { get; set; }

    public DateTime? DetectedDate { get; set; }

    public string? Status { get; set; }

    public virtual Claim? Claim { get; set; }

    public virtual ClaimLine? ClaimLine { get; set; }

    public virtual ScrubRule? Rule { get; set; }
}
