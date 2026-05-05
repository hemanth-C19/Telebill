using System;
using System.Collections.Generic;

namespace Telebill.Dto.Posting;

public class CreateRemitRefRequestDto
{
    public int PayerID { get; set; }
    public int? BatchID { get; set; }
    public string PayloadUri { get; set; } = string.Empty;
    public DateOnly ReceivedDate { get; set; }
}

public class UpdateRemitRefStatusRequestDto
{
    public string Status { get; set; } = string.Empty; // Loaded | Posted | Failed
}

public class RemitRefDto
{
    public int RemitID { get; set; }
    public int PayerID { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public int? BatchID { get; set; }
    public string PayloadUri { get; set; } = string.Empty;
    public DateOnly ReceivedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RemitRefListResponseDto
{
    public int TotalCount { get; set; }
    public List<RemitRefDto> RemitRefs { get; set; } = new();
}

