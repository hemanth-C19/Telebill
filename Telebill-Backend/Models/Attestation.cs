using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Attestation
{
    public int AttestId { get; set; }

    public int? EncounterId { get; set; }

    public int? ProviderId { get; set; }

    public string? AttestText { get; set; }

    public DateTime? AttestDate { get; set; }

    public string? Status { get; set; }

    public virtual Encounter? Encounter { get; set; }

    public virtual Provider? Provider { get; set; }
}
