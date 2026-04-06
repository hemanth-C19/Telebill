using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Dto.Notifications;
using Telebill.Models;

namespace Telebill.Repositories.Notifications;

public class NotificationRepository : INotificationRepository
{
    private readonly TeleBillContext _context;

    public NotificationRepository(TeleBillContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public Task<Notification?> GetByIdAsync(int notificationId)
    {
        return _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
    }

    public async Task<(List<Notification> Items, int TotalCount)> GetByUserIdAsync(
        NotificationFilterParams filters)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == filters.UserId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filters.Status))
        {
            query = query.Where(n => n.Status == filters.Status);
        }

        if (!string.IsNullOrEmpty(filters.Category))
        {
            query = query.Where(n => n.Category == filters.Category);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(n => n.CreatedDate)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<int> GetUnreadCountAsync(int userId)
    {
        return _context.Notifications
            .CountAsync(n => n.UserId == userId && n.Status == "Unread");
    }

    public async Task UpdateStatusAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(int userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && n.Status == "Unread")
            .ToListAsync();

        foreach (var n in unread)
        {
            n.Status = "Read";
        }

        await _context.SaveChangesAsync();
    }
}

