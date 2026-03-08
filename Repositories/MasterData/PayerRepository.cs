using System;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;

namespace Telebill.Repositories.MasterData
{
    public class PayerRepository : IPayerRepository
    {
        private readonly TeleBillContext _context;

        public PayerRepository(TeleBillContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payer>> GetAllPayersAsync()
        {
            return await _context.Payers.ToListAsync();
        }

        public async Task<IEnumerable<PayerNamesDTO>> GetAllPayersNames()
        {
            return await _context.Payers.Select(p => new PayerNamesDTO
            {
                PayerId = p.PayerId,
                PayerName = p.Name,
                PayerCode = p.PayerCode
            }).ToListAsync();
        }

        public async Task<IEnumerable<PayerPlan>> GetPlansByPayerIdAsync(int payerId)
        {
            return await _context.PayerPlans
                .Where(pp => pp.PayerId == payerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PlanNamesDTO>> GetPlanNamesByPayerIdAsync(int payerId)
        {
            return await _context.PayerPlans.Where(pp => pp.PayerId == payerId && pp.Status == "Active").Select(pp => new PlanNamesDTO
            {
                PlanId = pp.PlanId,
                PlanName = pp.PlanName
            }).ToListAsync();
        }

        public async Task AddPayerAsync(PayerDTO payer)
        {
            var newPayer = new Payer{
                Name = payer.Name,
                PayerCode = payer.PayerCode,
                ClearinghouseCode = payer.ClearinghouseCode,
                ContactInfo = payer.ContactInfo,
                Status = payer.Status
            };

            await _context.Payers.AddAsync(newPayer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePayerAsync(PayerDTO payer)
        {
            var newPayer = new Payer{
                Name = payer.Name,
                PayerCode = payer.PayerCode,
                ClearinghouseCode = payer.ClearinghouseCode,
                ContactInfo = payer.ContactInfo,
                Status = payer.Status
            };

            _context.Payers.Update(newPayer);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePayerAsync(int payerId)
        {
            var payer = await _context.Payers.FindAsync(payerId);
            if (payer != null)
            {
                _context.Payers.Remove(payer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddPlanAsync(PayerPlanDTO plan)
        {
            var newPlan = new PayerPlan{
                PayerId = plan.PayerId,
                PlanName = plan.PlanName,
                NetworkType = plan.NetworkType,
                Posdefault = plan.Posdefault,
                TelehealthModifiersJson = plan.TelehealthModifiersJson,
                Status = plan.Status
            };
            await _context.PayerPlans.AddAsync(newPlan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePlanAsync(PayerPlanDTO plan)
        {
            var newPlan = new PayerPlan{
                PayerId = plan.PayerId,
                PlanName = plan.PlanName,
                NetworkType = plan.NetworkType,
                Posdefault = plan.Posdefault,
                TelehealthModifiersJson = plan.TelehealthModifiersJson,
                Status = plan.Status
            };
            _context.PayerPlans.Update(newPlan);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePlanAsync(int planId)
        {
            var plan = await _context.PayerPlans.FindAsync(planId);
            if (plan != null)
            {
                _context.PayerPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddFeeAsync(FeeDTO fee)
        {
            var newFee = new FeeSchedule{
                PlanId = fee.PlanId,
                CptHcpcs = fee.CptHcpcs,
                ModifierCombo = fee.ModifierCombo,
                AllowedAmount = fee.AllowedAmount,
                EffectiveFrom = fee.EffectiveFrom,
                EffectiveTo = fee.EffectiveTo,
                Status = fee.Status
            };
            await _context.FeeSchedules.AddAsync(newFee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFeeAsync(FeeDTO fee)
        {
            var newFee = new FeeSchedule{
                PlanId = fee.PlanId,
                CptHcpcs = fee.CptHcpcs,
                ModifierCombo = fee.ModifierCombo,
                AllowedAmount = fee.AllowedAmount,
                EffectiveFrom = fee.EffectiveFrom,
                EffectiveTo = fee.EffectiveTo,
                Status = fee.Status
            };
            _context.FeeSchedules.Update(newFee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFeeAsync(int feeId)
        {
            var fee = await _context.FeeSchedules.FindAsync(feeId);
            if (fee != null)
            {
                _context.FeeSchedules.Remove(fee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<FeeSchedule>> GetFeesByPlanIdAsync(int planId)
        {
            return await _context.FeeSchedules
                .Where(f => f.PlanId == planId)
                .ToListAsync();
        }
    }
}