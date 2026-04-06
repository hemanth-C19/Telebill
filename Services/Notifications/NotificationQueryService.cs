using System;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.Notifications;
using Telebill.Repositories.Notifications;

namespace Telebill.Services.Notifications;

public class NotificationQueryService : INotificationQueryService
{
    private readonly INotificationRepository _repo;

    public NotificationQueryService(INotificationRepository repo)
    {
        _repo = repo;
    }

    public async Task<NotificationPagedResultDto> GetByUserIdAsync(NotificationFilterParams filters)
    {
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1) filters.PageSize = 20;
        if (filters.PageSize > 100) filters.PageSize = 100;

        var (items, totalCount) = await _repo.GetByUserIdAsync(filters);

        var dtoItems = items.Select(n => new NotificationItemDto
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId ?? 0,
            Message = n.Message,
            Category = n.Category,
            Status = n.Status,
            CreatedDate = n.CreatedDate ?? DateTime.MinValue
        }).ToList();

        return new NotificationPagedResultDto
        {
            Items = dtoItems,
            TotalCount = totalCount,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
        };
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(int userId)
    {
        var count = await _repo.GetUnreadCountAsync(userId);

        return new UnreadCountDto
        {
            UserId = userId,
            UnreadCount = count
        };
    }

    public async Task<(bool success, string error)> UpdateStatusAsync(
        int notificationId, int userId, UpdateNotificationStatusDto dto)
    {
        var allowed = new[] { "Read", "Dismissed" };
        if (!allowed.Contains(dto.NewStatus))
        {
            return (false, $"Invalid status '{dto.NewStatus}'. Allowed: Read, Dismissed");
        }

        var notification = await _repo.GetByIdAsync(notificationId);
        if (notification == null)
        {
            return (false, "Notification not found");
        }

        if (notification.UserId != userId)
        {
            return (false, "Notification does not belong to this user");
        }

        if (notification.Status == "Dismissed")
        {
            return (false, "Notification is already Dismissed and cannot be changed");
        }

        notification.Status = dto.NewStatus;
        await _repo.UpdateStatusAsync(notification);

        return (true, string.Empty);
    }

    public async Task<(bool success, string error)> MarkAllReadAsync(int userId)
    {
        if (userId <= 0)
        {
            return (false, "Invalid userId");
        }

        await _repo.MarkAllReadAsync(userId);

        return (true, string.Empty);
    }
}

