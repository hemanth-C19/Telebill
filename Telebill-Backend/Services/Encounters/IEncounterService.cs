using System;
using Telebill.Dto;
using Telebill.Models;


namespace Services
{
    public interface IEncounterService
    {
        Task<List<GetEncounterDTO>> GetAll();
        Task<GetEncounterDTO?> GetById(int id);

        Task<AddEncounterDTO> Create(AddEncounterDTO encounter);
        Task<EncounterUpdateDTO?> Update( int id, EncounterUpdateDTO dto);
        Task<bool> Delete(int id);
    }
}