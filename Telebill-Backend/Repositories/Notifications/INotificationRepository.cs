using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.Notifications;
using Telebill.Models;

namespace Telebill.Repositories.Notifications;

public interface INotificationRepository
{
    // ── WRITE ─────────────────────────────────────────────────
    Task AddAsync(Notification notification);

    // ── READ ──────────────────────────────────────────────────
    Task<Notification?> GetByIdAsync(int notificationId);

    // Paged list for a user with optional Status + Category filters
    Task<(List<Notification> Items, int TotalCount)> GetByUserIdAsync(
        NotificationFilterParams filters);

    // Count of Unread notifications for a user (for badge)
    Task<int> GetUnreadCountAsync(int userId);

    // ── STATUS UPDATES ────────────────────────────────────────
    Task UpdateStatusAsync(Notification notification);

    // Bulk: mark ALL Unread notifications for a user as Read
    Task MarkAllReadAsync(int userId);
}

