using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Arworkitem> Arworkitems { get; set; } = new List<Arworkitem>();

    public virtual ICollection<AttachmentRef> AttachmentRefs { get; set; } = new List<AttachmentRef>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<CodingLock> CodingLocks { get; set; } = new List<CodingLock>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PaymentPost> PaymentPosts { get; set; } = new List<PaymentPost>();
}
