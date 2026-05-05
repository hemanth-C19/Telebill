using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class BillingReport
{
    public int ReportId { get; set; }

    public string? Scope { get; set; }

    public string? MetricsJson { get; set; }

    public DateTime? GeneratedDate { get; set; }
}
