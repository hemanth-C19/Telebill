using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Appeal
{
    public int AppealId { get; set; }

    public int? DenialId { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public string? Method { get; set; }

    public string? AttachmentUri { get; set; }

    public string? Outcome { get; set; }

    public DateOnly? OutcomeDate { get; set; }

    public string? Notes { get; set; }

    public virtual Denial? Denial { get; set; }
}
