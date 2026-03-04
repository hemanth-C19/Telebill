using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.Auth
{
    public class AuthRepository : IAuthRepository
    {
        TeleBillContext _context = new TeleBillContext();

        public bool Login(string email)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);
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