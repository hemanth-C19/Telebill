using System;
using Telebill.Models;
using Repositories;
using Telebill.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Services
{
    public class EncounterService(IEncounterRepository repo) : IEncounterService
    {
        public async Task<List<GetEncounterDTO>> GetAll()
        {
            return await repo.GetAll();
        }
    
        public async Task<GetEncounterDTO?> GetById(int id)
        {
            return await repo.GetById(id);
        }

        public async Task<AddEncounterDTO> Create(AddEncounterDTO encounter)
        {
            encounter.Status = "Open";
            return await repo.Add(encounter);
        }

        public async Task<EncounterUpdateDTO?> Update( int id,  [FromBody] EncounterUpdateDTO dto)
        {
            return await repo.Update(id, dto);
        }

        public async Task<bool> Delete(int id)
        {
            return await repo.Delete(id);
        }
    }
}