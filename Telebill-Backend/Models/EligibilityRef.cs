using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class EligibilityRef
{
    public int EligibilityId { get; set; }

    public int? CoverageId { get; set; }

    public string? RequestPayloadUri { get; set; }

    public string? ResponsePayloadUri { get; set; }

    public DateTime? CheckedDate { get; set; }

    public string? Result { get; set; }

    public string? Notes { get; set; }

    public virtual Coverage? Coverage { get; set; }
}
