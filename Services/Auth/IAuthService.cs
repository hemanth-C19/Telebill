using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Telebill.Services.Auth
{
    public interface IAuthService
    {
        Task<bool> Login(string email);
        void Logout();
    }
}