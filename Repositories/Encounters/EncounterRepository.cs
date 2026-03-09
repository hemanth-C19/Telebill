using System;
using Telebill.Models;
using Telebill.Data;
using Microsoft.EntityFrameworkCore;
using Telebill.Dto;


namespace Repositories
{
    public class EncounterRepository : IEncounterRepository
    {
        private readonly TeleBillContext _context;

        public EncounterRepository(TeleBillContext context)
        {
            _context = context;
        }

        // public async Task<List<Encounter>> GetAll()
        // {
        //     return await _context.Encounters.ToListAsync();
        // }

        
        public async Task<List<EncounterDTO>> GetAll()
        {
            return await _context.Encounters
                .Select(e => new EncounterDTO
                {
                    EncounterId       = e.EncounterId,
                    PatientId         = e.PatientId,
                    ProviderId        = e.ProviderId,
                    EncounterDateTime = e.EncounterDateTime,
                    VisitType         = e.VisitType,
                    Pos               = e.Pos,
                    DocumentationUri  = e.DocumentationUri,
                    Status            = e.Status
                })
                .ToListAsync();
        }


        public async Task<EncounterDTO?> GetById(int id)
        {
            return await _context.Encounters
                .Select(e => new EncounterDTO
                        {
                            EncounterId       = e.EncounterId,
                            PatientId         = e.PatientId,
                            ProviderId        = e.ProviderId,
                            EncounterDateTime = e.EncounterDateTime,
                            VisitType         = e.VisitType,
                            Pos               = e.Pos,
                            DocumentationUri  = e.DocumentationUri,
                            Status            = e.Status
                        })
                        .FirstOrDefaultAsync();

        }

        // public async Task<IEnumerable<EncounterSpecificDTO>> GetSpecDetails()
        // {
        //     return await _context.Encounters.Select(e => new EncounterSpecificDTO
        //     {
        //         Status = e.Status,
        //         VisitType = e.VisitType
        //     }).ToListAsync();
        // }

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