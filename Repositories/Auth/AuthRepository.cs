using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Models;
using Telebill.Data;


namespace Telebill.Repositories.Auth
{
    public class AuthRepository(TeleBillContext context) : IAuthRepository
    {
        public async Task<User?> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return null;

            var user = await context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null)
                return null;

            if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
                return null;

            // When User.PasswordHash (or similar) exists, verify: PasswordHasher.VerifyHashedPassword(...)
            return user;
        }

        public void Logout()
        {
            // Stateless JWT: client discards token. Hook for future refresh-token / server-side blocklists.
        }
    }
}