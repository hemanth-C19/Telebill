using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Repositories.Auth;

namespace Telebill.Services.Auth
{
    public class AuthService : IAuthService
    {
        IAuthRepository authRepository;

        public AuthService(IAuthRepository _authRepository)
        {
            this.authRepository = _authRepository;
        }

        public async Task<bool> Login(string email)
        {
            return await authRepository.Login(email);
        }

        public void Logout()
        {
            authRepository.Logout();
        }
    }
}