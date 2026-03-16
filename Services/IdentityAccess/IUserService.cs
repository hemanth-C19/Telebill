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
        Task AddAsync(UserDTO user);  
        Task UpdateAsync(UserDTO user, int id);
 
        Task DeleteAsync(int id);
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<IEnumerable<UserDTO?>> Getuserbyrole(string role);
    }
}