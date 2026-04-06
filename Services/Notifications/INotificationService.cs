using System.Threading.Tasks;
using Telebill.Dto.Notifications;

namespace Telebill.Services.Notifications;

public interface INotificationService
{
    // The only method — called by other modules to fire a notification.
    // Returns the created NotificationId on success.
    Task<int> CreateAsync(CreateNotificationDto dto);
}

