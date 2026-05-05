using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Repositories.IdentityAccess;
using Telebill.Services.IdentityAccess;

using Telebill.Models;
using Telebill.Dto.IdentityAccess;

namespace Telebill.Services.IdentityAccess
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public Task AddUserAsync(UserAddDTO user)
        {
            return userRepository.AddUserAsync(user);
        }

        public Task DeleteAsync(int id)
        {
            return userRepository.DeleteUserAsync(id);
        }

        public Task<IEnumerable<User>> GetAllAsync(string? search, string? role, int page, int limit)
        {
            return userRepository.GetAllAsync(search, role, page, limit);
        }
        public Task UpdateAsync(UserUpdateDTO user)
        {
            return userRepository.UpdateUserAsync(user);
        }
    }
    
}