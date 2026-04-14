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
        Task UpdateUserAsync(UserUpdateDTO user);
 
        Task DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetAllAsync(string? search, string? role, int page, int limit);
    }
}