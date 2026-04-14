using System;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Dto.IdentityAccess
{
    public class AuditLogDTO
    {
        public int AuditId { get; set; }        // DB-generated PK

        public int? UserId { get; set; }        // FK to existing User

        public string Action { get; set; } = string.Empty;

        public string? Resource { get; set; }

        public DateTime? Timestamp { get; set; } // If null → DB GETDATE() default kicks in

        public string? Metadata { get; set; }
    }
}