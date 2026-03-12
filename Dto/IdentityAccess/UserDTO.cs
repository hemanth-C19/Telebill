using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Dto.IdentityAccess
{
    public class UserDTO
    {
        public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Status { get; set; }

    }
}