using System;
using Telebill.Models;
using Repositories;
using Telebill.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Services
{
    public class EncounterService : IEncounterService
    {
        private readonly IEncounterRepository _repo;

        public EncounterService(IEncounterRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<EncounterDTO>> GetAll()
        {
            return await _repo.GetAll();
        }
    
        public async Task<EncounterDTO?> GetById(int id)
        {
            return await _repo.GetById(id);
        }

        public async Task<Encounter> Create(Encounter encounter)
        {
            encounter.Status = "Open";
            return await _repo.Add(encounter);
        }

        public async Task<EncounterUpdateDTO?> Update( int id,  [FromBody] EncounterUpdateDTO dto)
        {
            return await _repo.Update(id, dto);
        }

        public async Task<bool> Delete(int id)
        {
            return await _repo.Delete(id);
        }
    }
}