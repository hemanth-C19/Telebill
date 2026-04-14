using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class SubmissionBatch
{
    public int BatchId { get; set; }

    public DateTime? BatchDate { get; set; }

    public int? ItemCount { get; set; }

    public decimal? TotalCharge { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<RemitRef> RemitRefs { get; set; } = new List<RemitRef>();

    public virtual ICollection<SubmissionRef> SubmissionRefs { get; set; } = new List<SubmissionRef>();
}
