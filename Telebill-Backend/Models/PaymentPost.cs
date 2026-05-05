using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class PaymentPost
{
    public int PaymentId { get; set; }

    public int? ClaimId { get; set; }

    public int? ClaimLineId { get; set; }

    public decimal? AmountPaid { get; set; }

    public string? AdjustmentJson { get; set; }

    public DateTime? PostedDate { get; set; }

    public int? PostedBy { get; set; }

    public string? Status { get; set; }

    public virtual Claim? Claim { get; set; }

    public virtual ClaimLine? ClaimLine { get; set; }

    public virtual User? PostedByNavigation { get; set; }
}
