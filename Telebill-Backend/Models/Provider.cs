using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Provider
{
    public int ProviderId { get; set; }

    public string Npi { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Taxonomy { get; set; }

    public bool? TelehealthEnrolled { get; set; }

    public string? ContactInfo { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Attestation> Attestations { get; set; } = new List<Attestation>();

    public virtual ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
}
