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

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return userRepository.GetAllAsync();
        }

        public Task<IEnumerable<User?>> Getuserbyrole(string role)
        {
            return userRepository.GetuserbyroleAsync(role);
        }

        public Task UpdateAsync(UserAddDTO user, int id)
        {
            return userRepository.UpdateUserAsync(user, id);
        }
    }
    
}