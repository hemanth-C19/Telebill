using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;

namespace Telebill.Repositories.MasterData
{
    public class MasterdataRepository : IMasterdataRepository
    {
        private readonly TeleBillContext _context;

        public MasterdataRepository()
        {
            _context = new TeleBillContext();
        }

        public async Task<IEnumerable<Provider>> GetAllProvidersAsync()
        {
            return await _context.Providers.ToListAsync();
        }

        public async Task<Provider> RegisterProviderAsync(Provider provider)
        {
            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();
            return provider;
        }

        public async Task<Provider?> UpdateProviderTelehealthAsync(int id, bool enrolled)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null) return null;
            provider.TelehealthEnrolled = enrolled;
            await _context.SaveChangesAsync();
            return provider;
        }

        public async Task<IEnumerable<Payer>> GetAllPayersAsync()
        {
            return await _context.Payers.Include(p => p.PayerPlans).ToListAsync();
        }

        public async Task<Payer> AddPayerAsync(Payer payer)
        {
            _context.Payers.Add(payer);
            await _context.SaveChangesAsync();
            return payer;
        }

        public async Task<PayerPlan?> AddPlanAsync(int payerId, PayerPlan plan)
        {
            var payer = await _context.Payers.FindAsync(payerId);
            if (payer == null) return null;
            plan.PayerId = payerId;
            _context.PayerPlans.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<decimal?> LookupFeeAsync(string cptCode, int planId)
        {
            var fee = await _context.FeeSchedules
                .Where(f => f.CptHcpcs == cptCode && f.PlanId == planId)
                .Select(f => (decimal?)f.AllowedAmount)
                .FirstOrDefaultAsync();
            return fee;
        }

        public async Task UploadFeeSchedulesAsync(IEnumerable<FeeSchedule> rates)
        {
            _context.FeeSchedules.AddRange(rates);
            await _context.SaveChangesAsync();
        }
    }
}