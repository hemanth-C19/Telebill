using System;
using Telebill.Dto;
using Telebill.Models;


namespace Services
{
    public interface IEncounterService
    {
        Task<List<EncounterDTO>> GetAll();
        Task<EncounterDTO?> GetById(int id);

        // Task<IEnumerable<EncounterSpecificDTO>> GetSpecDetails();
        Task<Encounter> Create(Encounter encounter);
        Task<Encounter> Update(Encounter encounter);
        Task<bool> Delete(int id);
    }
}