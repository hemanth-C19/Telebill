using System.Threading.Tasks;
using Telebill.Dto.Notifications;

namespace Telebill.Services.Notifications;

public interface INotificationService
{
    Task<int> CreateAsync(CreateNotificationDto dto);
}

