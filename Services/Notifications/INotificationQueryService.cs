using System.Threading.Tasks;
using Telebill.Dto.Notifications;

namespace Telebill.Services.Notifications;

public interface INotificationQueryService
{
    Task<NotificationPagedResultDto> GetByUserIdAsync(NotificationFilterParams filters);

    Task<UnreadCountDto> GetUnreadCountAsync(int userId);

    Task<(bool success, string error)> UpdateStatusAsync(
        int notificationId, int userId, UpdateNotificationStatusDto dto);

    Task<(bool success, string error)> MarkAllReadAsync(int userId);
}

