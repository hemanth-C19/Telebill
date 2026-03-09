using System;
using Telebill.Models;


namespace Services
{
    public interface IEncounterService
    {
        Task<List<Encounter>> GetAll();
        Task<Encounter?> GetById(int id);
        Task<Encounter> Create(Encounter encounter);
        Task<Encounter> Update(Encounter encounter);
        Task<bool> Delete(int id);
    }
}