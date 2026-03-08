using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;
using Telebill.Dto.MasterData;

namespace Telebill.Repositories.MasterData
{
    public class ProviderRepository: IProviderRepository
    {
        private readonly TeleBillContext _context;

        public ProviderRepository(TeleBillContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Provider>> GetAllProvidersAsync()
        {
            return await _context.Providers.ToListAsync();
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
            _context.Providers.Add(newProvider);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProviderTelehealthAsync(int Pid, CreateUpdateProviderDTO dto)
        {

            var existingProvider = await _context.Providers.FirstOrDefaultAsync(p => p.ProviderId == Pid);
            
            existingProvider.Name = dto.ProviderName;
            existingProvider.Npi = dto.ProviderNpi;
            existingProvider.Taxonomy = dto.ProviderTaxonomy;
            existingProvider.TelehealthEnrolled = dto.ProviderEnrolled;
            existingProvider.ContactInfo = dto.ProviderContact;
            existingProvider.Status = dto.ProviderStatus;

            _context.Providers.Update(existingProvider);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProviderActiveInfo>> GetActiveProvidersAsync()
        {
            var Providers = await _context.Providers
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
            var prd = await _context.Providers.FirstOrDefaultAsync(p => p.Name == ProviderName);
            if(prd == null){
                return null;
            }
            return prd;
        }

        public async Task<Provider> GetProviderByNPIAsync(string NpiId)
        {
            var prd = await _context.Providers.FirstOrDefaultAsync(p => p.Npi == NpiId);
            if(prd == null){
                return null;
            }
            return prd;
        }

        public async Task DeleteProviderByIdAsync(int Pid)
        {
            var prd = await _context.Providers.FirstOrDefaultAsync(p=> p.ProviderId == Pid);
            _context.Providers.Remove(prd);
            await _context.SaveChangesAsync();
        }
    }
}