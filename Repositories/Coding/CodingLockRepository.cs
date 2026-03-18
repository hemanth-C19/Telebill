using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Models;

namespace Telebill.Repositories.Coding
{
    public class CodingLockRepository : ICodingLockRepository
    {
        private readonly TeleBillContext _context;

        public CodingLockRepository(TeleBillContext context)
        {
            _context = context;
        }

        public async Task<CodingLock?> GetActiveCodingLockAsync(int encounterId)
        {
            return await _context.CodingLocks
                .FirstOrDefaultAsync(cl =>
                    cl.EncounterId == encounterId &&
                    cl.Status == "Locked");
        }

        public async Task<List<CodingLock>> GetCodingLockHistoryAsync(int encounterId)
        {
            return await _context.CodingLocks
                .Where(cl => cl.EncounterId == encounterId)
                .OrderByDescending(cl => cl.LockedDate)
                .ToListAsync();
        }

        public async Task<CodingLock> AddCodingLockAsync(CodingLock codingLock)
        {
            await _context.CodingLocks.AddAsync(codingLock);
            await _context.SaveChangesAsync();
            return codingLock;
        }

        public async Task UpdateCodingLockAsync(CodingLock codingLock)
        {
            _context.CodingLocks.Update(codingLock);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ClaimExistsForEncounterAsync(int encounterId)
        {
            return await _context.Claims
                .AnyAsync(c => c.EncounterId == encounterId);
        }

        public async Task<string?> GetClaimStatusForEncounterAsync(int encounterId)
        {
            var claim = await _context.Claims
                .Where(c => c.EncounterId == encounterId)
                .OrderByDescending(c => c.CreatedDate)
                .FirstOrDefaultAsync();

            return claim?.ClaimStatus;
        }
    }
}

