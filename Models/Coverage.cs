using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Coverage
{
    public int CoverageId { get; set; }

    public int? PatientId { get; set; }

    public int? PlanId { get; set; }

    public string MemberId { get; set; } = null!;

    public string? GroupNumber { get; set; }

    public DateOnly? EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<EligibilityRef> EligibilityRefs { get; set; } = new List<EligibilityRef>();

    public virtual Patient? Patient { get; set; }

    public virtual PayerPlan? Plan { get; set; }
}
