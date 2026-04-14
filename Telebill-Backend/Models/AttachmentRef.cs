using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class AttachmentRef
{
    public int AttachId { get; set; }

    public int? ClaimId { get; set; }

    public string? FileType { get; set; }

    public string? FileUri { get; set; }

    public string? Notes { get; set; }

    public int? UploadedBy { get; set; }

    public DateTime? UploadedDate { get; set; }

    public string? Status { get; set; }

    public virtual Claim? Claim { get; set; }

    public virtual User? UploadedByNavigation { get; set; }
}
