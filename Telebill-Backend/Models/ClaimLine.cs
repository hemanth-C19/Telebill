using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class ClaimLine
{
    public int ClaimLineId { get; set; }

    public int? ClaimId { get; set; }

    public int? LineNo { get; set; }

    public string? CptHcpcs { get; set; }

    public string? Modifiers { get; set; }

    public int? Units { get; set; }

    public decimal? ChargeAmount { get; set; }

    public string? DxPointers { get; set; }

    public string? Pos { get; set; }

    public string? LineStatus { get; set; }

    public virtual Claim? Claim { get; set; }

    public virtual ICollection<Denial> Denials { get; set; } = new List<Denial>();

    public virtual ICollection<PaymentPost> PaymentPosts { get; set; } = new List<PaymentPost>();

    public virtual ICollection<ScrubIssue> ScrubIssues { get; set; } = new List<ScrubIssue>();
}
