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
        // Matches: Task addUser(UserDTO user);
        public async Task AddUserAsync(UserAddDTO user)
        {
            if (user is null) return;

            // Map DTO -> Entity
            var entity = new User
            {
                // Do NOT set UserId here if it's identity/DB-generated
                Name = user.Name,
                Role = user.Role,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status
            };

            await context.Users.AddAsync(entity);
            await context.SaveChangesAsync();

            // If you want to flow back the generated key into DTO (optional):
            // user.UserId = entity.UserId;
        }

        // Matches: Task updateUser(UserDTO user, int id);
        public async Task UpdateUserAsync(UserAddDTO user, int id)
        {
            if (user is null) return;

            var existing = await context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (existing is null) return;

            existing.Name = user.Name;
            existing.Role = user.Role;
            existing.Email = user.Email;
            existing.Phone = user.Phone;
            existing.Status = user.Status;

            context.Users.Update(existing);
            await context.SaveChangesAsync();
        }

        // Matches: Task deleteUser(int id);
        public async Task DeleteUserAsync(int id)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (existing is null) return;

            context.Users.Remove(existing);
            await context.SaveChangesAsync();
        }

        // Matches: Task<IEnumerable<User>> getAll();
        
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await context.Users
                .AsNoTracking() // read-only; better performance
                .Select(u => new User
                {
                    UserId = u.UserId,
                    Name   = u.Name,
                    Role   = u.Role,
                    Email  = u.Email,
                    Phone  = u.Phone,
                    Status = u.Status
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<User?>> GetuserbyroleAsync(string role)
        {
            return await context.Users
                .AsNoTracking() // read-only; better performance
                .Where(u=>u.Role.ToLower()==role.Trim().ToLower())
                .Select(u => new User
                {
                    UserId = u.UserId,
                    Name   = u.Name,
                    Role   = u.Role,
                    Email  = u.Email,
                    Phone  = u.Phone,
                    Status = u.Status
                })
                .ToListAsync();
        }
    }
}