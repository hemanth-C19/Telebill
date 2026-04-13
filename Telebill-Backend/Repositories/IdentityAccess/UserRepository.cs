using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Dto.IdentityAccess;
using Telebill.Models;

namespace Telebill.Repositories.IdentityAccess
{
    public class UserRepository(TeleBillContext context) : IUserRepository
    {
        public async Task AddUserAsync(UserAddDTO user)
        {
            if (user is null) return;

            var entity = new User
            {
                Name = user.Name,
                Role = user.Role,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status
            };

            await context.Users.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UserUpdateDTO user)
        {
            if (user is null) return;

            var updatedUser = new User
            {
                UserId= user.UserId,
                Name= user.Name,
                Role = user.Role,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status
            };

            context.Users.Update(updatedUser);
            await context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (existing is null) return;

            context.Users.Remove(existing);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync(string? search, string? role, int page, int limit)
        {

            var query = context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.Name == search);
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            var users = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return users;
        }
    }
}