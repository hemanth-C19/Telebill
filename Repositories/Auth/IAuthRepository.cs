using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Telebill.Repositories.Auth
{
    public interface IAuthRepository
    {
        bool Login(string email);
        void Logout();
    }
}