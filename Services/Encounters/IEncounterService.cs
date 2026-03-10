using System;
using Telebill.Dto;
using Telebill.Models;


namespace Services
{
    public interface IEncounterService
    {
        Task<List<EncounterDTO>> GetAll();
        Task<EncounterDTO?> GetById(int id);

        Task<Encounter> Create(Encounter encounter);
        Task<EncounterUpdateDTO?> Update( int id, EncounterUpdateDTO dto);
        Task<bool> Delete(int id);
    }
}