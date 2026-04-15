using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.MasterData;

public interface IPayerPlanRepository
{
    Task<List<PayerPlan>> GetByPayerIdAsync(int payerId, string? search);
    Task<List<PayerPlan>> GetActiveByPayerIdAsync(int payerId);
    Task<PayerPlan?> GetPayerPlanByIdAsync(int planId);
    Task<bool> ExistsAsync(int planId);
    Task AddAsync(PayerPlan plan);
    Task UpdateAsync(PayerPlan plan);
    Task DeleteAsync(PayerPlan plan);
}

