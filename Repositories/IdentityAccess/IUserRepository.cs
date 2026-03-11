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
        Task AddUserAsync(UserDTO user);
        Task UpdateUserAsync(UserDTO user, int id);
 
        Task DeleteUserAsync(int id);
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<IEnumerable<UserDTO?>> GetuserbyroleAsync(string role);
    }
}