using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Arworkitem
{
    public int WorkId { get; set; }

    public int? ClaimId { get; set; }

    public string? Priority { get; set; }

    public int? AssignedTo { get; set; }

    public DateOnly? NextActionDate { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    public virtual User? AssignedToNavigation { get; set; }

    public virtual Claim? Claim { get; set; }
}
