using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Repositories.Auth;

namespace Telebill.Services.Auth
{
    public class AuthService : IAuthService
    {
        IAuthRepository _authRepository = new AuthRepository();

        public bool Login(string email)
        {
            return _authRepository.Login(email);
        }

        public void Logout()
        {
            _authRepository.Logout();
        }
    }
}