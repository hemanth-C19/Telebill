using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Encounter
{
    public int EncounterId { get; set; }

    public int? PatientId { get; set; }

    public int? ProviderId { get; set; }

    public DateTime EncounterDateTime { get; set; }

    public string? VisitType { get; set; }

    public string? Pos { get; set; }

    public string? DocumentationUri { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Attestation> Attestations { get; set; } = new List<Attestation>();

    public virtual ICollection<ChargeLine> ChargeLines { get; set; } = new List<ChargeLine>();

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public virtual ICollection<CodingLock> CodingLocks { get; set; } = new List<CodingLock>();

    public virtual ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

    public virtual Patient? Patient { get; set; }

    public virtual Provider? Provider { get; set; }
}
