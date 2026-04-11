using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;
using Telebill.Services.IdentityAccess;
using Telebill.Dto.IdentityAccess;


namespace Telebill.Repositories.IdentityAccess
{
    public interface IUserRepository
    {
        Task AddUserAsync(UserAddDTO user);
        Task UpdateUserAsync(UserAddDTO user, int id);
 
        Task DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User?>> GetuserbyroleAsync(string role);
    }
}