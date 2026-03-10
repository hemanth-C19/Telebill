using System;
using Telebill.Models;
using Telebill.Repositories.PatientCoverage;
using Telebill.Services.PatientCoverage;
using Microsoft.AspNetCore.Mvc;
using Telebill.DTOs;

namespace Telebill.Controllers.PatientCoverage
{
    [ApiController]
    [Route("api/Coverage")]
    public class CoverageController : ControllerBase
    {
        private readonly IPatientService? _service;

        public CoverageController(IPatientService service) => _service = service;

        [HttpDelete("DeleteCoverage/{patientId}/{CoverageId}")]
        public async Task<IActionResult> DeleteCoverage(int patientId, int CoverageId)
        {
            var success = await _service.RemoveCoverage(patientId, CoverageId);
            if (!success) return NotFound();
            return Ok(new { message = $"Insurance coverage {CoverageId} deleted." });
        }

        [HttpGet("GetCoverageById/{patientId}")]
        public async Task<IActionResult> GetCoverage(int patientId) => Ok(await _service.GetPatientInsurance(patientId));

        [HttpPost("AddCoverage")]
        public async Task<IActionResult> AddCoverage([FromBody] CoverageDto dto) => Ok(await _service.AddInsurance(dto));


        [HttpGet("coverageDetails/{CoverageId}")]
        public async Task<IActionResult> GetCoverageDetails(int CoverageId)
        {
            var coverage = await _service.GetCoverageDetailsAsync(CoverageId);
            if (coverage == null) return NotFound($"Coverage {CoverageId} record not found");
            return Ok(coverage);
        }
        [HttpPut("UpdateCoverage/{coverageId}")] // This is the Update action for Coverage/Insurance
        public async Task<IActionResult> UpdateCoverage([FromBody] CoverageDto dto)
        {
            // Add logic in your service to update coverage
            return Ok(new { message = "Insurance Updated" });
        }

        // --- ELIGIBILITY ACTIONS ---

        [HttpPost("VerifyInsurance/{coverageId}")]
        public async Task<IActionResult> Verify(int coverageId) => Ok(await _service.VerifyInsurance(coverageId));
    }
}
