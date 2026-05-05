using System;
using Microsoft.AspNetCore.Authorization;
using Telebill.Models;
using Telebill.Repositories.PatientCoverage;
using Telebill.Services.PatientCoverage;
using Microsoft.AspNetCore.Mvc;
using Telebill.DTOs;

namespace Telebill.Controllers.PatientCoverage
{
    [ApiController]
    [Route("api/v1/PatientCoverage/[controller]")]
    [Authorize(Roles = "FrontDesk,AR,Admin")]
    public class CoverageController(IPatientService service) : ControllerBase
    {

        [HttpDelete("DeleteCoverage/{patientId}/{CoverageId}")]
        [Authorize(Roles = "FrontDesk,Admin")]
        public async Task<IActionResult> DeleteCoverage([FromRoute] int patientId, [FromRoute] int CoverageId)
        {
            var success = await service.RemoveCoverage(patientId, CoverageId);
            if (!success) return NotFound();
            return Ok(new { message = $"Insurance coverage {CoverageId} deleted." });
        }

        [HttpGet("GetCoverageById/{patientId}")]
        public async Task<IActionResult> GetCoverage(int patientId) => Ok(await service.GetPatientInsurance(patientId));

        [HttpPost("AddCoverage")]
        [Authorize(Roles = "FrontDesk,Admin")]
        public async Task<IActionResult> AddCoverage([FromBody] CoverageDto dto) => Ok(await service.AddInsurance(dto));


        [HttpGet("coverageDetails/{CoverageId}")]
        public async Task<IActionResult> GetCoverageDetails(int CoverageId)
        {
            var coverage = await service.GetCoverageDetailsAsync(CoverageId);
            if (coverage == null) return NotFound($"Coverage {CoverageId} record not found");
            return Ok(coverage);
        }
        [HttpPut("UpdateCoverage/{coverageId}")] // This is the Update action for Coverage/Insurance
        [Authorize(Roles = "FrontDesk,Admin")]
        public async Task<IActionResult> UpdateCoverage([FromBody] CoverageDto dto)
        {
            // Add logic in your service to update coverage
            return Ok(new { message = "Insurance Updated" });
        }

        // --- ELIGIBILITY ACTIONS ---

        [HttpPost("VerifyInsurance/{coverageId}")]
        [Authorize(Roles = "FrontDesk,Admin")]
        public async Task<IActionResult> Verify(int coverageId) => Ok(await service.VerifyInsurance(coverageId));
    }
}
