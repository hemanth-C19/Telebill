using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Denial
{
    public int DenialId { get; set; }

    public int? ClaimId { get; set; }

    public int? ClaimLineId { get; set; }

    public string? ReasonCode { get; set; }

    public string? RemarkCode { get; set; }

    public DateOnly? DenialDate { get; set; }

    public decimal? AmountDenied { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();

    public virtual Claim? Claim { get; set; }

    public virtual ClaimLine? ClaimLine { get; set; }
}
