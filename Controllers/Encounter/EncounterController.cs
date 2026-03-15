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

    public class EncounterController : ControllerBase
    {
        private readonly IEncounterService _service;

        public EncounterController(IEncounterService service)
        {
            _service = service;
        }

        [HttpGet("GetAllEncounters")]
        
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAll();
            return Ok(data);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetById(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // [HttpPost("AddEncounter")]
        // public async Task<IActionResult> Create(Encounter encounter)
        // {
        //     var result = await _service.Create(encounter);
        //     return Ok(result);
        // }

        [HttpPut("UpdateEncounter")]
        public async Task<IActionResult> Update(int id,  [FromBody] EncounterUpdateDTO dto)
        {
            var result = await _service.Update(id, dto);
            return Ok(result);
        }

        [HttpDelete("DeleteEncounter/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);

            if (!result)
                return NotFound();

            return Ok("Deleted Successfully");
        }
    }
}