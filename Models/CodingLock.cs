using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class CodingLock
{
    public int CodingLockId { get; set; }

    public int? EncounterId { get; set; }

    public int? CoderId { get; set; }

    public DateTime? LockedDate { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    public virtual User? Coder { get; set; }

    public virtual Encounter? Encounter { get; set; }
}
