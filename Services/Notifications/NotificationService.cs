using System;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.Notifications;
using Telebill.Models;
using Telebill.Repositories.Notifications;

namespace Telebill.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> CreateAsync(CreateNotificationDto dto)
    {
        var allowedCategories = new[]
        {
            "Scrub", "Submission", "Ack", "Remit", "Denial", "Statement"
        };

        if (string.IsNullOrWhiteSpace(dto.Category) ||
            !allowedCategories.Contains(dto.Category))
        {
            Console.WriteLine($"Invalid notification category: {dto.Category}");
            return 0;
        }

        if (dto.UserId <= 0)
        {
            return 0;
        }

        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            return 0;
        }

        var notification = new Notification
        {
            UserId = dto.UserId,
            Message = dto.Message,
            Category = dto.Category,
            Status = "Unread",
            CreatedDate = DateTime.UtcNow
        };

        await _repo.AddAsync(notification);

        return notification.NotificationId;
    }
}

