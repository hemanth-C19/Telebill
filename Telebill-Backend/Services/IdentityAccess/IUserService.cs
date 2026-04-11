using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto.IdentityAccess;
using Telebill.Models;
using Telebill.Services.IdentityAccess;


namespace Telebill.Services.IdentityAccess
{
    public interface IUserService
    {
        Task AddUserAsync(UserAddDTO user);  
        Task UpdateAsync(UserAddDTO user, int id);
 
        Task DeleteAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User?>> Getuserbyrole(string role);
    }
}