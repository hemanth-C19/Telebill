using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class X12837pRef
{
    public int X12id { get; set; }

    public int? ClaimId { get; set; }

    public string? PayloadUri { get; set; }

    public DateTime? GeneratedDate { get; set; }

    public string? Version { get; set; }

    public string? Status { get; set; }

    public virtual Claim? Claim { get; set; }
}
