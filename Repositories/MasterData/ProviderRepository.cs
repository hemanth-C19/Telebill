using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;
using Telebill.Dto.MasterData;
using Telebill.Data;

namespace Telebill.Repositories.MasterData
{
    public class ProviderRepository(TeleBillContext context) : IProviderRepository
    {
        public async Task<IEnumerable<Provider>> GetAllProvidersAsync()
        {
            return await context.Providers.ToListAsync();
        }

        public async Task RegisterProviderAsync(CreateUpdateProviderDTO obj)
        {
            var newProvider = new Provider{
                Name = obj.ProviderName,
                Npi = obj.ProviderNpi,
                Taxonomy = obj.ProviderTaxonomy,
                TelehealthEnrolled = obj.ProviderEnrolled,
                ContactInfo = obj.ProviderContact,
                Status = obj.ProviderStatus
            };
            context.Providers.Add(newProvider);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProviderTelehealthAsync(int Pid, CreateUpdateProviderDTO dto)
        {

            var existingProvider = await context.Providers.FirstOrDefaultAsync(p => p.ProviderId == Pid);
            
            existingProvider.Name = dto.ProviderName;
            existingProvider.Npi = dto.ProviderNpi;
            existingProvider.Taxonomy = dto.ProviderTaxonomy;
            existingProvider.TelehealthEnrolled = dto.ProviderEnrolled;
            existingProvider.ContactInfo = dto.ProviderContact;
            existingProvider.Status = dto.ProviderStatus;

            context.Providers.Update(existingProvider);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProviderActiveInfo>> GetActiveProvidersAsync()
        {
            var Providers = await context.Providers
                .Where(p => p.Status == "Active" && p.TelehealthEnrolled == true)
                .Select(p => new ProviderActiveInfo{
                    ProviderId = p.ProviderId,
                    ProviderName = p.Name
                })
                .ToListAsync();
            return Providers;
        }

        public async Task<Provider> GetProviderByNameAsync(string ProviderName)
        {
            var prd = await context.Providers.FirstOrDefaultAsync(p => p.Name == ProviderName);
            if(prd == null){
                return null;
            }
            return prd;
        }

        public async Task<Provider> GetProviderByNPIAsync(string NpiId)
        {
            var prd = await context.Providers.FirstOrDefaultAsync(p => p.Npi == NpiId);
            if(prd == null){
                return null;
            }
            return prd;
        }

        public async Task DeleteProviderByIdAsync(int Pid)
        {
            var prd = await context.Providers.FirstOrDefaultAsync(p=> p.ProviderId == Pid);
            context.Providers.Remove(prd);
            await context.SaveChangesAsync();
        }
    }
}