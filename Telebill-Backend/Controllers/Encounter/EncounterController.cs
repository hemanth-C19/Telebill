using System;
using Microsoft.AspNetCore.Authorization;
using Telebill.Models;
using Services;
using Repositories;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto;


namespace Telebill.Controllers
{
    [ApiController]
    // [Route("api/[controller]")]
    [Route("api/v1/Encounter/[controller]")]
    [Authorize(Roles = "FrontDesk,Provider,Coder,AR,Admin")]

    public class EncounterController(IEncounterService service) : ControllerBase
    {
        [HttpGet("GetAllEncounters")]
        
        public async Task<IActionResult> GetAll()
        {
            var data = await service.GetAll();
            return Ok(data);
        }


        [HttpGet("GetEncounterById/{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var data = await service.GetById(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost("AddEncounter")]
        [Authorize(Roles = "FrontDesk,Admin")]
        public async Task<IActionResult> Create(AddEncounterDTO encounter)
        {
            var result = await service.Create(encounter);
            return Ok(result);
        }

        [HttpPut("UpdateEncounterById/{id}")]
        [Authorize(Roles = "FrontDesk,Admin")]
        public async Task<IActionResult> Update([FromRoute] int id,  [FromBody] EncounterUpdateDTO dto)
        {
            var result = await service.Update(id, dto);
            return Ok(result);
        }

        

        [HttpDelete("DeleteEncounter/{id}")]
        [Authorize(Roles = "FrontDesk,Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var result = await service.Delete(id);

            if (!result)
                return NotFound();

            return Ok("Deleted Successfully");
        }
    }
}