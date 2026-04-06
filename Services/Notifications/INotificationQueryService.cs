using System.Threading.Tasks;
using Telebill.Dto.Notifications;

namespace Telebill.Services.Notifications;

public interface INotificationQueryService
{
    // List with pagination + filters
    Task<NotificationPagedResultDto> GetByUserIdAsync(NotificationFilterParams filters);

    // Unread badge count
    Task<UnreadCountDto> GetUnreadCountAsync(int userId);

    // Mark one notification as Read or Dismissed
    Task<(bool success, string error)> UpdateStatusAsync(
        int notificationId, int userId, UpdateNotificationStatusDto dto);

    // Bulk mark all Unread → Read for a user
    Task<(bool success, string error)> MarkAllReadAsync(int userId);
}

