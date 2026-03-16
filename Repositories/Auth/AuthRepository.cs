using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Models;
using Telebill.Data;


namespace Telebill.Repositories.Auth
{
    public class AuthRepository : IAuthRepository
    {
        TeleBillContext context;

        public AuthRepository(TeleBillContext _context)
        {
            this.context = _context;
        }

        public async Task<bool> Login(string email)
        {
            var user = await context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                Console.WriteLine("Logged in");
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Logout()
        {
            Console.WriteLine("Logged out");
        }
    }
}