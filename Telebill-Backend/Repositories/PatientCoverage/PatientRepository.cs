using System;
using Telebill.Models;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.DTOs;

namespace Telebill.Repositories.PatientCoverage
{
    public class PatientRepository(TeleBillContext context) : IPatientRepository
    {

        // Patient Logic
        public async Task<IEnumerable<Patient>> GetAllPatientsAsync(string? search, int page, int limit)
        {

            var query = context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Mrn.Contains(search));
            }

            var patients = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return patients;
        }

        public async Task<IEnumerable<ActivePatients>> GetPatientNamesAsync()
        {
            var patients = await context.Patients.Where(p => p.Status == "Active").Select(p => new ActivePatients
            {
                PatientId = p.PatientId,
                Name = p.Name,
            }).ToListAsync();

            return patients;
        }


        public async Task AddPatientAsync(Patient patient) => await context.Patients.AddAsync(patient);
        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            // FindAsync is the most efficient way to get a record by its Primary Key
            return await context.Patients.FindAsync(id);
        }
        public void UpdatePatient(Patient patient) => context.Patients.Update(patient);
        public async Task DeletePatientAsync(int id)
        {
            var p = await context.Patients.FindAsync(id);
            if (p != null) context.Patients.Remove(p);
        }

        // Coverage Logic
        public async Task<IEnumerable<Coverage>> GetCoveragesByPatientIdAsync(int patientId)
            => await context.Coverages.Where(c => c.PatientId == patientId).ToListAsync();
        public async Task AddCoverageAsync(Coverage coverage) => await context.Coverages.AddAsync(coverage);
        public void UpdateCoverage(Coverage coverage) => context.Coverages.Update(coverage);

        public async Task<Coverage> GetCoverageByIdAsync(int id) => await context.Coverages.FindAsync(id);

        public async Task DeleteCoverageAsync(int CoverageId)
        {
            var coverage = await context.Coverages.FindAsync(CoverageId);
            if (coverage != null)
            {
                context.Coverages.Remove(coverage);
            }
        }

        public async Task DeleteCoverageByCoverageIdAsync(int patientId, int CoverageId)
        {
            var coverage = await context.Coverages
                .FirstOrDefaultAsync(c => c.CoverageId == CoverageId && c.PatientId == patientId);
            if (coverage != null)
            {
                context.Coverages.Remove(coverage);
                await context.SaveChangesAsync();
            }
        }

        // Eligibility Logic
        public async Task AddEligibilityRefAsync(EligibilityRef eligibility) => await context.EligibilityRefs.AddAsync(eligibility);
        public async Task<IEnumerable<EligibilityRef>> GetHistoryByCoverageIdAsync(int coverageId)
            => await context.EligibilityRefs.Where(e => e.CoverageId == coverageId).ToListAsync();

        public async Task SaveChangesAsync() => await context.SaveChangesAsync();
    }
}