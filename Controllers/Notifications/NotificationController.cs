using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Notifications;
using Telebill.Services.Notifications;

namespace Telebill.Controllers.Notifications;

[ApiController]
[Route("api/v1/notifications")]
[Authorize(Roles = "FrontDesk,Coder,Provider,AR,Admin")]
public class NotificationController(INotificationQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] NotificationFilterParams filters)
    {
        if (filters.UserId <= 0)
        {
            return BadRequest("userId is required");
        }

        var result = await queryService.GetByUserIdAsync(filters);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount([FromQuery] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("userId is required");
        }

        var result = await queryService.GetUnreadCountAsync(userId);
        return Ok(result);
    }

    [HttpPatch("{notificationId}/status")]
    public async Task<IActionResult> UpdateStatus(
        int notificationId,
        [FromQuery] int userId,
        [FromBody] UpdateNotificationStatusDto dto)
    {
        var (success, error) = await queryService.UpdateStatusAsync(
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
        var (success, error) = await queryService.MarkAllReadAsync(userId);
        if (!success)
        {
            return BadRequest(error);
        }

        return Ok();
    }
}

