using System;

namespace Telebill.Dto;

public class X12RefDto
{
    public int X12ID { get; set; }
    public int ClaimID { get; set; }
    public string PayloadURI { get; set; } = string.Empty;
    public string PreSignedURL { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

