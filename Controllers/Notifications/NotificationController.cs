using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Notifications;
using Telebill.Services.Notifications;

namespace Telebill.Controllers.Notifications;

[ApiController]
[Route("api/v1/notifications")]
public class NotificationController : ControllerBase
{
    private readonly INotificationQueryService _queryService;

    public NotificationController(INotificationQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] NotificationFilterParams filters)
    {
        if (filters.UserId <= 0)
        {
            return BadRequest("userId is required");
        }

        var result = await _queryService.GetByUserIdAsync(filters);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount([FromQuery] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("userId is required");
        }

        var result = await _queryService.GetUnreadCountAsync(userId);
        return Ok(result);
    }

    [HttpPatch("{notificationId}/status")]
    public async Task<IActionResult> UpdateStatus(
        int notificationId,
        [FromQuery] int userId,
        [FromBody] UpdateNotificationStatusDto dto)
    {
        var (success, error) = await _queryService.UpdateStatusAsync(
            notificationId, userId, dto);
        if (!success)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllRead([FromQuery] int userId)
    {
        var (success, error) = await _queryService.MarkAllReadAsync(userId);
        if (!success)
        {
            return BadRequest(error);
        }

        return Ok();
    }
}

