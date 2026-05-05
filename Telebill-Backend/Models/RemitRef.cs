using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class RemitRef
{
    public int RemitId { get; set; }

    public int? PayerId { get; set; }

    public int? BatchId { get; set; }

    public string? PayloadUri { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public string? Status { get; set; }

    public virtual SubmissionBatch? Batch { get; set; }

    public virtual Payer? Payer { get; set; }
}
