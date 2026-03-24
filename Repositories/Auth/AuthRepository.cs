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
        public async Task<User?> LoginAsync(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(password))
                return null;

            if (role == "Provider")
            {
                var provider = await context.Providers.SingleOrDefaultAsync(p => p.ContactInfo.ToLower() == email);
                return new User{UserId= provider.ProviderId, Email = provider.ContactInfo, Role = "Provider", Name = provider.Name};
            }
            else
            {
                var user = await context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);
                return user;
            }
        }

        public void Logout()
        {
            Console.WriteLine("Logged out");
        }
    }
}