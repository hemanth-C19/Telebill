using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class FeeSchedule
{
    public int FeeId { get; set; }

    public int? PlanId { get; set; }

    public string CptHcpcs { get; set; } = null!;

    public string? ModifierCombo { get; set; }

    public decimal? AllowedAmount { get; set; }

    public DateOnly? EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public string? Status { get; set; }

    public virtual PayerPlan? Plan { get; set; }
}
