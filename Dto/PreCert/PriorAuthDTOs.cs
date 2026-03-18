using System;
using System.Collections.Generic;

namespace Telebill.Dto.PreCert;

public class CreatePriorAuthRequestDto
{
    public int ClaimID { get; set; }
    public int PlanID { get; set; }
    public DateOnly RequestedDate { get; set; }
}

public class UpdatePriorAuthRequestDto
{
    public string? AuthNumber { get; set; }
    public DateOnly? ApprovedFrom { get; set; }
    public DateOnly? ApprovedTo { get; set; }
    public string? Status { get; set; } // Requested | Approved | Denied | Expired
}

public class PriorAuthDto
{
    public int PAID { get; set; }
    public int ClaimID { get; set; }
    public int PlanID { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string? AuthNumber { get; set; }
    public DateOnly RequestedDate { get; set; }
    public DateOnly? ApprovedFrom { get; set; }
    public DateOnly? ApprovedTo { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsExpiringSoon { get; set; }
}

public class PriorAuthListResponseDto
{
    public int TotalCount { get; set; }
    public List<PriorAuthDto> PriorAuths { get; set; } = new();
}

