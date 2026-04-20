using Telebill.Models;

namespace Telebill.Repositories.PatientCoverage
{
    public interface IPatientRepository
    {
        // Patient Table
        Task<IEnumerable<Patient>> GetAllPatientsAsync(string? search, int page, int limit);
        Task<Patient> GetPatientByIdAsync(int id);

        Task AddPatientAsync(Patient patient);
        void UpdatePatient(Patient patient);
        Task DeletePatientAsync(int id);

        // Coverage Table
        Task<IEnumerable<Coverage>> GetCoveragesByPatientIdAsync(int patientId);
        Task AddCoverageAsync(Coverage coverage);
        Task<Coverage> GetCoverageByIdAsync(int id);
        void UpdateCoverage(Coverage coverage);

        // Task DeleteCoverageAsync(int id);

        Task DeleteCoverageByCoverageIdAsync(int patientId ,int CoverageId);

        // EligibilityRef Table
        Task AddEligibilityRefAsync(EligibilityRef eligibility);
        Task<IEnumerable<EligibilityRef>> GetHistoryByCoverageIdAsync(int coverageId);

        Task SaveChangesAsync();
        Task DeleteCoverageAsync(int coverageId);
    }
}