using System;
using Telebill.Models;
using Repositories;

namespace Services
{
    public class EncounterService : IEncounterService
    {
        private readonly IEncounterRepository _repo;

        public EncounterService(IEncounterRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Encounter>> GetAll()
        {
            return await _repo.GetAll();
        }

        public async Task<Encounter?> GetById(int id)
        {
            return await _repo.GetById(id);
        }

        public async Task<Encounter> Create(Encounter encounter)
        {
            encounter.Status = "Open";
            return await _repo.Add(encounter);
        }

        public async Task<Encounter> Update(Encounter encounter)
        {
            return await _repo.Update(encounter);
        }

        public async Task<bool> Delete(int id)
        {
            return await _repo.Delete(id);
        }
    }
}