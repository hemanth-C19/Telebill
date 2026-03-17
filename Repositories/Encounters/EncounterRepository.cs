using System;
using Telebill.Models;
using Telebill.Data;
using Microsoft.EntityFrameworkCore;
using Telebill.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


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


        public async Task<List<GetEncounterDTO>> GetAll()
        {
            return await _context.Encounters
                .Select(e => new GetEncounterDTO
                {
                    EncounterId = e.EncounterId,
                    PatientId = e.PatientId,
                    ProviderId = e.ProviderId,
                    EncounterDateTime = e.EncounterDateTime,
                    VisitType = e.VisitType,
                    Pos = e.Pos,
                    DocumentationUri = e.DocumentationUri,
                    Status = e.Status
                })
                .ToListAsync();
        }


        public async Task<GetEncounterDTO?> GetById(int id)
        {
            return await _context.Encounters
                .Where(e => e.EncounterId == id)
                .Select(e => new GetEncounterDTO
                {
                    EncounterId = e.EncounterId,
                    PatientId = e.PatientId,
                    ProviderId = e.ProviderId,
                    EncounterDateTime = e.EncounterDateTime,
                    VisitType = e.VisitType,
                    Pos = e.Pos,
                    DocumentationUri = e.DocumentationUri,
                    Status = e.Status
                })
                        .FirstOrDefaultAsync();

        }


        public async Task<AddEncounterDTO> Add(AddEncounterDTO dto)
        {  
            var entity = new Encounter
                {
                    PatientId = dto.PatientId,
                    ProviderId = dto.ProviderId,
                    EncounterDateTime = dto.EncounterDateTime,
                    VisitType = dto.VisitType,
                    Pos = dto.Pos,
                    DocumentationUri = dto.DocumentationUri,
                    Status = dto.Status ?? "Open"
                };

                _context.Encounters.Add(entity);
                await _context.SaveChangesAsync();


                
                return new AddEncounterDTO
                {
                    // EncounterId = entity.EncounterId,
                    PatientId = (int) entity.PatientId,
                    ProviderId = (int) entity.ProviderId,
                    EncounterDateTime = entity.EncounterDateTime,
                    VisitType = entity.VisitType,
                    Pos = entity.Pos,
                    DocumentationUri = entity.DocumentationUri,
                    Status = entity.Status
                };
        }

        public async Task<EncounterUpdateDTO?> Update(int id, EncounterUpdateDTO dto)
        {
            var encounter = await _context.Encounters.FirstOrDefaultAsync(e => e.EncounterId == id);
            if (encounter == null) return null;

            encounter.EncounterDateTime = dto.EncounterDateTime;
            encounter.VisitType = dto.VisitType;
            encounter.Status = dto.Status;
            encounter.Pos = dto.Pos;
            encounter.DocumentationUri = dto.DocumentationUri;

            await _context.SaveChangesAsync();

            var enc = new EncounterUpdateDTO
            {
                EncounterDateTime = encounter.EncounterDateTime,
                VisitType = encounter.VisitType,
                Status = encounter.Status,
                Pos = encounter.Pos,
                DocumentationUri = encounter.DocumentationUri
            };
            return enc;
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