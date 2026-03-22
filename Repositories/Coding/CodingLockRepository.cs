using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Models;

namespace Telebill.Repositories.Coding
{
    public class CodingLockRepository(TeleBillContext context) : ICodingLockRepository
    {
        public async Task<CodingLock?> GetActiveCodingLockAsync(int encounterId)
        {
            return await context.CodingLocks
                .FirstOrDefaultAsync(cl =>
                    cl.EncounterId == encounterId &&
                    cl.Status == "Locked");
        }

        public async Task<List<CodingLock>> GetCodingLockHistoryAsync(int encounterId)
        {
            return await context.CodingLocks
                .Where(cl => cl.EncounterId == encounterId)
                .OrderByDescending(cl => cl.LockedDate)
                .ToListAsync();
        }

        public async Task<CodingLock> AddCodingLockAsync(CodingLock codingLock)
        {
            await context.CodingLocks.AddAsync(codingLock);
            await context.SaveChangesAsync();
            return codingLock;
        }

        public async Task UpdateCodingLockAsync(CodingLock codingLock)
        {
            context.CodingLocks.Update(codingLock);
            await context.SaveChangesAsync();
        }

        public async Task<bool> ClaimExistsForEncounterAsync(int encounterId)
        {
            return await context.Claims
                .AnyAsync(c => c.EncounterId == encounterId);
        }

        public async Task<string?> GetClaimStatusForEncounterAsync(int encounterId)
        {
            var claim = await context.Claims
                .Where(c => c.EncounterId == encounterId)
                .OrderByDescending(c => c.CreatedDate)
                .FirstOrDefaultAsync();

            return claim?.ClaimStatus;
        }
    }
}

