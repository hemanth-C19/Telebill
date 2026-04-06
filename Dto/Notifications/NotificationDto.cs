using System;
using System.Collections.Generic;

namespace Telebill.Dto.Notifications;

// Single notification row returned to the UI
public class NotificationItemDto
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string? Message { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedDate { get; set; }
}

// Filter params for listing notifications
public class NotificationFilterParams
{
    public int UserId { get; set; }         // REQUIRED — always filter by user
    public string? Status { get; set; }     // null = all | "Unread" | "Read" | "Dismissed"
    public string? Category { get; set; }   // null = all | "Scrub" | "Submission" | etc.
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// Paged result wrapper
public class NotificationPagedResultDto
{
    public List<NotificationItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// Unread badge count
public class UnreadCountDto
{
    public int UserId { get; set; }
    public int UnreadCount { get; set; }
}

// Status update request (mark read or dismiss)
public class UpdateNotificationStatusDto
{
    public string NewStatus { get; set; } = string.Empty;
    // Allowed values: "Read" | "Dismissed"
}

// Internal DTO — used by INotificationService (called by other modules)
// NOT exposed via HTTP
public class CreateNotificationDto
{
    public int UserId { get; set; }    // FK → User (recipient)
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    // Allowed Category values: Scrub / Submission / Ack / Remit / Denial / Statement
}

