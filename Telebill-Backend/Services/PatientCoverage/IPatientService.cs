using System;
using Telebill.Models;
using Telebill.DTOs;

namespace Telebill.Services.PatientCoverage
{
    public interface IPatientService
    {
        // CRUD Patient
        Task<Patient> RegisterPatient(PatientDto dto);
        Task<IEnumerable<Patient>> ListAllPatients(string? search, int page, int limit);
        Task<Patient> GetPatientById(int id);
        Task UpdatePatient(int id, PatientDto dto);
        Task<bool> RemovePatient(int id);

        Task<IEnumerable<ActivePatients>> GetActivePatientsAsync();

        // CRUD Coverage
        Task<Coverage> AddInsurance(CoverageDto dto);
        Task<IEnumerable<Coverage>> GetPatientInsurance(int patientId);

        Task<bool> RemoveCoverage(int patientId ,int CoverageId);
        Task<bool> DeleteCoverageAsync(int coverageId);

        // Action: Verify Eligibility
        Task<EligibilityRef> VerifyInsurance(int coverageId);
        Task<Coverage> GetCoverageDetailsAsync(int id);
    }
}