using System;
using Telebill.Models;

namespace Telebill.Repositories.Encounters
{
    public interface IEncounterRepository
    {
        Task<List<Encounter>> GetAll();
        Task<Encounter?> GetById(int id);
        Task<Encounter> Add(Encounter encounter);
        Task<Encounter> Update(Encounter encounter);
        Task<bool> Delete(int id);
    }
}