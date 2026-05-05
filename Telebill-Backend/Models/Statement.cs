using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Statement
{
    public int StatementId { get; set; }

    public int? PatientId { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public DateTime? GeneratedDate { get; set; }

    public decimal? AmountDue { get; set; }

    public string? SummaryJson { get; set; }

    public string? Status { get; set; }

    public virtual Patient? Patient { get; set; }
}
