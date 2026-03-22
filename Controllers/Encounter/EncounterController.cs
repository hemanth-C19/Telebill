using System;
using Telebill.Models;
using Services;
using Repositories;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto;


namespace Telebill.Controllers
{
    [ApiController]
    // [Route("api/[controller]")]
    [Route("EncounterModule/[controller]")]

    public class EncounterController(IEncounterService service) : ControllerBase
    {
        [HttpGet("GetAllEncounters")]
        
        public async Task<IActionResult> GetAll()
        {
            var data = await service.GetAll();
            return Ok(data);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await service.GetById(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost("AddEncounter")]
        public async Task<IActionResult> Create(AddEncounterDTO encounter)
        {
            var result = await service.Create(encounter);
            return Ok(result);
        }

        [HttpPut("UpdateEncounter")]
        public async Task<IActionResult> Update(int id,  [FromBody] EncounterUpdateDTO dto)
        {
            var result = await service.Update(id, dto);
            return Ok(result);
        }

        

        [HttpDelete("DeleteEncounter/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await service.Delete(id);

            if (!result)
                return NotFound();

            return Ok("Deleted Successfully");
        }
    }
}