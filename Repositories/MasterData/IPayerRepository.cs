using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.MasterData
{
    public interface IPayerRepository
    {
        Task<List<Payer>> GetAllAsync();
        Task<Payer?> GetPayerByIdAsync(int payerId);
        Task<bool> ExistsAsync(int payerId);
        Task AddAsync(Payer payer);
        Task UpdateAsync(Payer payer);
        Task DeleteAsync(Payer payer);
    }
}