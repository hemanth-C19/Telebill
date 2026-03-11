using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Dto.IdentityAccess;
using Telebill.Models;

namespace Telebill.Repositories.IdentityAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly TeleBillContext _context;

        public UserRepository(TeleBillContext teleBillContext)
        {
            _context = teleBillContext;
        }

        // Matches: Task addUser(UserDTO user);
        public async Task AddUserAsync(UserDTO user)
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

            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();

            // If you want to flow back the generated key into DTO (optional):
            // user.UserId = entity.UserId;
        }

        // Matches: Task updateUser(UserDTO user, int id);
        public async Task UpdateUserAsync(UserDTO user, int id)
        {
            if (user is null) return;

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (existing is null) return;

            // Do NOT modify primary key
            existing.Name = user.Name;
            existing.Role = user.Role;
            existing.Email = user.Email;
            existing.Phone = user.Phone;
            existing.Status = user.Status;

            await _context.SaveChangesAsync();
        }

        // Matches: Task deleteUser(int id);
        public async Task DeleteUserAsync(int id)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (existing is null) return;

            _context.Users.Remove(existing);
            await _context.SaveChangesAsync();
        }

        // Matches: Task<IEnumerable<User>> getAll();
        
        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking() // read-only; better performance
                .Select(u => new UserDTO
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

        public async Task<IEnumerable<UserDTO?>> GetuserbyroleAsync(string role)
        {
            return await _context.Users
                .AsNoTracking() // read-only; better performance
                .Where(u=>u.Role.ToLower()==role.Trim().ToLower())
                .Select(u => new UserDTO
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