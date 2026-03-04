using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public string Message { get; set; } = null!;

    public string? Category { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual User? User { get; set; }
}
