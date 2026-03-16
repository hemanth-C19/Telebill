using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Services.MasterData;
using Telebill.Models;
using Telebill.Dto.MasterData;

namespace Telebill.Controllers.MasterData
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService _service;

        public ProviderController(IProviderService service)
        {
            _service = service;
        }

        [HttpGet("GetAllProviders")]
        public async Task<IActionResult> GetProviders()
        {
            var result = await _service.GetAllProvidersAsync();
            return Ok(result);
        }

        [HttpGet("GetProviderByNPI")]
        public async Task<IActionResult> GetProviderByNPI(string NpiId)
        {
            var result = await _service.GetProviderByNPIAsync(NpiId);
            return Ok(result);
        }

        [HttpGet("GetProviderByName")]
        public async Task<IActionResult> GetProviderByName(string ProviderName)
        {
            var result = await _service.GetProviderByNameAsync(ProviderName);
            return Ok(result);
        }

        [HttpGet("GetActiveProviders")]
        public async Task<IActionResult> GetActiveProviders(){
            var result = await _service.GetActiveProvidersAsync();
            return Ok(result);
        }

        [HttpPost("CreateProvider")]
        public async Task<IActionResult> RegisterProvider(CreateUpdateProviderDTO obj)
        {
            await _service.RegisterProviderAsync(obj);
            return StatusCode(201, "Created Provider");
        }

        [HttpPut("UpdateProviderById")]
        public async Task<IActionResult> UpdateProviderById(int Pid, CreateUpdateProviderDTO dto)
        {
            await _service.UpdateProviderByIdAsync(Pid, dto);
            return StatusCode(200, "Updated Provider");
        }

        [HttpDelete("DeleteProviderById")]
        public async Task<IActionResult> DeleteProviderById(int Pid)
        {
            await _service.DeleteProviderByIdAsync(Pid);
            return StatusCode(200, "Provider Deletion Successfull");
        }
    }
}