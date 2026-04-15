using Telebill.Models;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Dto.MasterData;

namespace Telebill.Repositories.MasterData
{
    public class PayerRepository : IPayerRepository, IPayerPlanRepository, IFeeScheduleRepository
    {
        private readonly TeleBillContext _context;

        public PayerRepository(TeleBillContext context)
        {
            _context = context;
        }

        // ── PAYER ─────────────────────────────────────────────────

        public async Task<List<Payer>> GetAllAsync(string? search, int page, int limit)
        {
            var query = _context.Payers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            var payers = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return payers;
        }

        public async Task<List<PayerNamesDTO>> GetPayerNamesAsync()
        {
            var payers = await _context.Payers.Select(p => new PayerNamesDTO
            {
                PayerId = p.PayerId,
                PayerName = p.Name,
                PayerCode = p.PayerCode
            }).ToListAsync();

            return payers;
        }

        public Task<Payer?> GetPayerByIdAsync(int payerId)
        {
            return _context.Payers.FirstOrDefaultAsync(p => p.PayerId == payerId);
        }

        public Task<bool> ExistsAsync(int payerId)
        {
            return _context.Payers.AnyAsync(p => p.PayerId == payerId);
        }

        public async Task AddAsync(Payer payer)
        {
            _context.Payers.Add(payer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payer payer)
        {
            _context.Payers.Update(payer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Payer payer)
        {
            _context.Payers.Remove(payer);
            await _context.SaveChangesAsync();
        }

        // ── PLAN ──────────────────────────────────────────────────

        public async Task<List<PayerPlan>> GetByPayerIdAsync(int payerId, string? search)
        {
            var query = _context.PayerPlans.AsQueryable();

            query = query.Where(p => p.PayerId == payerId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.PlanName.Contains(search));
            }

            var payerPlans = await query.ToListAsync();

            return payerPlans;
        }

        public Task<List<PayerPlan>> GetActiveByPayerIdAsync(int payerId)
        {
            return _context.PayerPlans
                .Where(pp => pp.PayerId == payerId && pp.Status == "Active")
                .ToListAsync();
        }

        public Task<PayerPlan?> GetPayerPlanByIdAsync(int planId)
        {
            return _context.PayerPlans.FirstOrDefaultAsync(pp => pp.PlanId == planId);
        }

        Task<bool> IPayerPlanRepository.ExistsAsync(int planId)
        {
            return _context.PayerPlans.AnyAsync(pp => pp.PlanId == planId);
        }

        public async Task AddAsync(PayerPlan plan)
        {
            _context.PayerPlans.Add(plan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PayerPlan plan)
        {
            _context.PayerPlans.Update(plan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PayerPlan plan)
        {
            _context.PayerPlans.Remove(plan);
            await _context.SaveChangesAsync();
        }

        // ── FEE SCHEDULE ──────────────────────────────────────────

        public Task<List<FeeSchedule>> GetByPlanIdAsync(int planId)
        {
            return _context.FeeSchedules.Where(f => f.PlanId == planId).ToListAsync();
        }

        public Task<FeeSchedule?> GetFeeByIdAsync(int feeId)
        {
            return _context.FeeSchedules.FirstOrDefaultAsync(f => f.FeeId == feeId);
        }

        Task<bool> IFeeScheduleRepository.ExistsAsync(int feeId)
        {
            return _context.FeeSchedules.AnyAsync(f => f.FeeId == feeId);
        }

        public Task<bool> PlanExistsAsync(int planId)
        {
            return _context.PayerPlans.AnyAsync(p => p.PlanId == planId);
        }

        public async Task AddAsync(FeeSchedule fee)
        {
            _context.FeeSchedules.Add(fee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FeeSchedule fee)
        {
            _context.FeeSchedules.Update(fee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(FeeSchedule fee)
        {
            _context.FeeSchedules.Remove(fee);
            await _context.SaveChangesAsync();
        }
    }
}