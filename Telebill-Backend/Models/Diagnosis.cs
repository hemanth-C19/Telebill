using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Diagnosis
{
    public int DxId { get; set; }

    public int? EncounterId { get; set; }

    public string Icd10code { get; set; } = null!;

    public string? Description { get; set; }

    public int? Sequence { get; set; }

    public string? Status { get; set; }

    public virtual Encounter? Encounter { get; set; }
}
