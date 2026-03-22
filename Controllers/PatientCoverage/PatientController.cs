using System;
using Telebill.Models;
using Telebill.Repositories.PatientCoverage;
using Telebill.Services.PatientCoverage;
using Microsoft.AspNetCore.Mvc;
using Telebill.DTOs;

namespace Telebill.Controllers
{
    [ApiController]
    [Route("api/Patient")] // One base route for the whole module
    public class PatientController(IPatientService service) : ControllerBase
    {

        // --- PATIENT ACTIONS ---

        [HttpGet("GetAllPatients")]
        public async Task<IActionResult> GetAllPatients() => Ok(await service.ListAllPatients());

        [HttpPost("RegisterPatient")]
        public async Task<IActionResult> CreatePatient([FromBody] PatientDto dto) => Ok(await service.RegisterPatient(dto));

        [HttpGet("GetPatientById/{PatientId}")]
        public async Task<IActionResult> GetPatient(int patientid)
        {
            var patient = await service.GetPatientById(patientid);

            if (patient == null)
            {
                return NotFound(new { message = $"Patient with ID {patientid} not found." });
            }

            return Ok(patient);
        }

        [HttpPut("UpdatePatientById/{PatientId}")] // {id} makes the 'id' box appear in Swagger
        public async Task<IActionResult> UpdatePatient(int patientId, [FromBody] PatientDto dto)
        {
            if (patientId <= 0) return BadRequest("Invalid Patient ID");

            // Call the service to actually save changes to the DB
            await service.UpdatePatient(patientId, dto);

            return Ok(new { message = $"Patient with ID {patientId} updated successfully" });
        }


        // DELETE: api/patient-management/patients/3
[HttpDelete("DeletePatientByID/{PatientId}")]
public async Task<IActionResult> DeletePatient(int patientId)
{
    var success = await service.RemovePatient(patientId);
    if (!success) return NotFound();
    return Ok(new { message = $"Patient {patientId} and all associated data deleted." });
}



}
}


