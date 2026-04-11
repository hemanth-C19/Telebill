using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Payer
{
    public int PayerId { get; set; }

    public string Name { get; set; } = null!;

    public string? PayerCode { get; set; }

    public string? ClearinghouseCode { get; set; }

    public string? ContactInfo { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<PayerPlan> PayerPlans { get; set; } = new List<PayerPlan>();

    public virtual ICollection<RemitRef> RemitRefs { get; set; } = new List<RemitRef>();
}
