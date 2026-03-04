using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class PriorAuth
{
    public int Paid { get; set; }

    public int? ClaimId { get; set; }

    public int? PlanId { get; set; }

    public string? AuthNumber { get; set; }

    public DateOnly? RequestedDate { get; set; }

    public DateOnly? ApprovedFrom { get; set; }

    public DateOnly? ApprovedTo { get; set; }

    public string? Status { get; set; }

    public virtual Claim? Claim { get; set; }

    public virtual PayerPlan? Plan { get; set; }
}
