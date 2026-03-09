using System;
using Telebill.Dto;
using Telebill.Models;

namespace Repositories
{
    public interface IEncounterRepository
    {
        Task<List<EncounterDTO>> GetAll();

        Task<EncounterDTO?> GetById(int id);
        Task<Encounter> Add(Encounter encounter);
        Task<Encounter> Update(Encounter encounter);
        Task<bool> Delete(int id);
    }
}