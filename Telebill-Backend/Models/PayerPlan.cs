using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class PayerPlan
{
    public int PlanId { get; set; }

    public int? PayerId { get; set; }

    public string PlanName { get; set; } = null!;

    public string? NetworkType { get; set; }

    public string? Posdefault { get; set; }

    public string? TelehealthModifiersJson { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public virtual ICollection<Coverage> Coverages { get; set; } = new List<Coverage>();

    public virtual ICollection<FeeSchedule> FeeSchedules { get; set; } = new List<FeeSchedule>();

    public virtual Payer? Payer { get; set; }

    public virtual ICollection<PriorAuth> PriorAuths { get; set; } = new List<PriorAuth>();
}
