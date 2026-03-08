using System;
using Telebill.Models;
using Telebill.Data;
using Microsoft.EntityFrameworkCore;


namespace Telebill.Repositories.Encounters
{
    public class EncounterRepository : IEncounterRepository
    {
        private readonly TeleBillContext _context;

        public EncounterRepository(TeleBillContext context)
        {
            _context = context;
        }

        public async Task<List<Encounter>> GetAll()
        {
            return await _context.Encounters
                .Include(e => e.Patient)
                .Include(e => e.Provider)
                .ToListAsync();
        }

        public async Task<Encounter?> GetById(int id)
        {
            return await _context.Encounters
                .Include(e => e.ChargeLines)
                .Include(e => e.Attestations)
                .FirstOrDefaultAsync(e => e.EncounterId == id);
        }

        public async Task<Encounter> Add(Encounter encounter)
        {
            _context.Encounters.Add(encounter);
            await _context.SaveChangesAsync();
            return encounter;
        }

        public async Task<Encounter> Update(Encounter encounter)
        {
            _context.Encounters.Update(encounter);
            await _context.SaveChangesAsync();
            return encounter;
        }

        public async Task<bool> Delete(int id)
        {
            var enc = await _context.Encounters.FindAsync(id);

            if (enc == null) return false;

            _context.Encounters.Remove(enc);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}