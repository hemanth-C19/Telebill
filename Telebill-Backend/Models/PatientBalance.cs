using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class PatientBalance
{
    public int BalanceId { get; set; }

    public int? PatientId { get; set; }

    public int? ClaimId { get; set; }

    public decimal? AmountDue { get; set; }

    public string? AgingBucket { get; set; }

    public DateOnly? LastStatementDate { get; set; }

    public string? Status { get; set; }

    public virtual Claim? Claim { get; set; }

    public virtual Patient? Patient { get; set; }
}
