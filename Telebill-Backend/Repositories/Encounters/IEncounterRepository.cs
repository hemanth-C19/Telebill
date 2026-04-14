using System;
using Telebill.Dto;
using Telebill.Models;

namespace Repositories
{
    public interface IEncounterRepository
    {
        Task<List<GetEncounterDTO>> GetAll();

        Task<GetEncounterDTO?> GetById(int id);
        Task<AddEncounterDTO> Add(AddEncounterDTO dto);
        Task<EncounterUpdateDTO?> Update(int id, EncounterUpdateDTO dto);
        Task<bool> Delete(int id);
    }
}