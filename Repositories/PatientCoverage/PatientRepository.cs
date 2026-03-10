using System;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;

namespace Telebill.Repositories.PatientCoverage
{
    public class PatientRepository : IPatientRepository
    {
        private readonly TeleBillContext _context;
        public PatientRepository(TeleBillContext context) => _context = context;

        // Patient Logic
        public async Task<IEnumerable<Patient>> GetAllPatientsAsync() => await _context.Patients.ToListAsync();

        public async Task AddPatientAsync(Patient patient) => await _context.Patients.AddAsync(patient);
        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            // FindAsync is the most efficient way to get a record by its Primary Key
            return await _context.Patients.FindAsync(id);
        }
        public void UpdatePatient(Patient patient) => _context.Patients.Update(patient);
        public async Task DeletePatientAsync(int id)
        {
            var p = await _context.Patients.FindAsync(id);
            if (p != null) _context.Patients.Remove(p);
        }

        // Coverage Logic
        public async Task<IEnumerable<Coverage>> GetCoveragesByPatientIdAsync(int patientId)
            => await _context.Coverages.Where(c => c.PatientId == patientId).ToListAsync();
        public async Task AddCoverageAsync(Coverage coverage) => await _context.Coverages.AddAsync(coverage);
        public void UpdateCoverage(Coverage coverage) => _context.Coverages.Update(coverage);

        public async Task<Coverage> GetCoverageByIdAsync(int id) => await _context.Coverages.FindAsync(id);

        public async Task DeleteCoverageAsync(int CoverageId)
        {
            var coverage = await _context.Coverages.FindAsync(CoverageId);
            if (coverage != null)
            {
                _context.Coverages.Remove(coverage);
            }
        }

        public async Task DeleteCoverageByCoverageIdAsync(int patientId ,int CoverageId)
        {
            var patient = await _context.Patients.Include(p=> p.Coverages).FirstOrDefaultAsync(p => p.PatientId == patientId);
            var coverage = patient.Coverages.FirstOrDefault(c=> c.CoverageId == CoverageId);
            _context.Coverages.Remove(coverage);
            await _context.SaveChangesAsync();
        }

        // Eligibility Logic
        public async Task AddEligibilityRefAsync(EligibilityRef eligibility) => await _context.EligibilityRefs.AddAsync(eligibility);
        public async Task<IEnumerable<EligibilityRef>> GetHistoryByCoverageIdAsync(int coverageId)
            => await _context.EligibilityRefs.Where(e => e.CoverageId == coverageId).ToListAsync();

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}