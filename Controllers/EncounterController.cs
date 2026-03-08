using System;
using Microsoft.AspNetCore.Mvc;


namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncounterController : ControllerBase
    {
        private readonly IEncounterService _service;

        public EncounterController(IEncounterService service)
        {
            _service = service;
        }

        [HttpGet]
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

        [HttpPost]
        public async Task<IActionResult> Create(Encounter encounter)
        {
            var result = await _service.Create(encounter);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(Encounter encounter)
        {
            var result = await _service.Update(encounter);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);

            if (!result)
                return NotFound();

            return Ok("Deleted Successfully");
        }
    }
}