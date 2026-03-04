using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class ChargeLine
{
    public int ChargeId { get; set; }

    public int? EncounterId { get; set; }

    public string CptHcpcs { get; set; } = null!;

    public string? Modifiers { get; set; }

    public int? Units { get; set; }

    public decimal? ChargeAmount { get; set; }

    public string? RevenueCode { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    public virtual Encounter? Encounter { get; set; }
}
