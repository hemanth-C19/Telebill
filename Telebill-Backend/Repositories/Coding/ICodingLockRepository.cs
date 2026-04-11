using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.Coding
{
    public interface ICodingLockRepository
    {
        Task<CodingLock?> GetActiveCodingLockAsync(int encounterId);
        Task<List<CodingLock>> GetCodingLockHistoryAsync(int encounterId);
        Task<CodingLock> AddCodingLockAsync(CodingLock codingLock);
        Task UpdateCodingLockAsync(CodingLock codingLock);

        Task<bool> ClaimExistsForEncounterAsync(int encounterId);
        Task<string?> GetClaimStatusForEncounterAsync(int encounterId);
    }
}

