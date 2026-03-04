using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class ScrubRule
{
    public int RuleId { get; set; }

    public string Name { get; set; } = null!;

    public string? ExpressionJson { get; set; }

    public string? Severity { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<ScrubIssue> ScrubIssues { get; set; } = new List<ScrubIssue>();
}
