using System;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;

namespace Telebill.Repositories.MasterData
{
    public class PayerRepository(TeleBillContext context) : IPayerRepository
    {
        public async Task<IEnumerable<Payer>> GetAllPayersAsync()
        {
            return await context.Payers.ToListAsync();
        }

        public async Task<IEnumerable<PayerNamesDTO>> GetAllPayersNames()
        {
            return await context.Payers.Select(p => new PayerNamesDTO
            {
                PayerId = p.PayerId,
                PayerName = p.Name,
                PayerCode = p.PayerCode
            }).ToListAsync();
        }

        public async Task<IEnumerable<PayerPlan>> GetPlansByPayerIdAsync(int payerId)
        {
            return await context.PayerPlans
                .Where(pp => pp.PayerId == payerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PlanNamesDTO>> GetPlanNamesByPayerIdAsync(int payerId)
        {
            return await context.PayerPlans.Where(pp => pp.PayerId == payerId && pp.Status == "Active").Select(pp => new PlanNamesDTO
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

            await context.Payers.AddAsync(newPayer);
            await context.SaveChangesAsync();
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

            context.Payers.Update(newPayer);
            await context.SaveChangesAsync();
        }

        public async Task DeletePayerAsync(int payerId)
        {
            var payer = await context.Payers.FindAsync(payerId);
            if (payer != null)
            {
                context.Payers.Remove(payer);
                await context.SaveChangesAsync();
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
            await context.PayerPlans.AddAsync(newPlan);
            await context.SaveChangesAsync();
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
            context.PayerPlans.Update(newPlan);
            await context.SaveChangesAsync();
        }

        public async Task DeletePlanAsync(int planId)
        {
            var plan = await context.PayerPlans.FindAsync(planId);
            if (plan != null)
            {
                context.PayerPlans.Remove(plan);
                await context.SaveChangesAsync();
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
            await context.FeeSchedules.AddAsync(newFee);
            await context.SaveChangesAsync();
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
            context.FeeSchedules.Update(newFee);
            await context.SaveChangesAsync();
        }

        public async Task DeleteFeeAsync(int feeId)
        {
            var fee = await context.FeeSchedules.FindAsync(feeId);
            if (fee != null)
            {
                context.FeeSchedules.Remove(fee);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<FeeSchedule>> GetFeesByPlanIdAsync(int planId)
        {
            return await context.FeeSchedules
                .Where(f => f.PlanId == planId)
                .ToListAsync();
        }
    }
}