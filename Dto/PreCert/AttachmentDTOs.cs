using System;
using System.Collections.Generic;

namespace Telebill.Dto.PreCert;

public class CreateAttachmentRequestDto
{
    public int ClaimID { get; set; }
    public string FileType { get; set; } = string.Empty; // PDF | Image | Doc
    public string FileUri { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateAttachmentStatusRequestDto
{
    public string Status { get; set; } = string.Empty; // Active | Deleted | Archived
}

public class AttachmentDto
{
    public int AttachId { get; set; }
    public int ClaimID { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string FileUri { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int UploadedBy { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AttachmentListResponseDto
{
    public int TotalCount { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
}

