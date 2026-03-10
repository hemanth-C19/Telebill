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
        Task<EncounterUpdateDTO?> Update(int id, EncounterUpdateDTO dto);
        Task<bool> Delete(int id);
    }
}