using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Models;

namespace Telebill.Repositories.Coding
{
    public class DiagnosisRepository(TeleBillContext context) : IDiagnosisRepository
    {
        public async Task<List<Diagnosis>> GetDiagnosesByEncounterAsync(int encounterId)
        {
            return await context.Diagnoses
                .Where(d => d.EncounterId == encounterId)
                .OrderBy(d => d.Sequence)
                .ToListAsync();
        }

        public async Task<List<Diagnosis>> GetActiveDiagnosesByEncounterAsync(int encounterId)
        {
            return await context.Diagnoses
                .Where(d => d.EncounterId == encounterId && d.Status == "Active")
                .OrderBy(d => d.Sequence)
                .ToListAsync();
        }

        public async Task<Diagnosis?> GetDiagnosisByIdAsync(int dxId)
        {
            return await context.Diagnoses.FindAsync(dxId);
        }

        public async Task<bool> DiagnosisCodeExistsActiveAsync(int encounterId, string icd10Code)
        {
            return await context.Diagnoses
                .AnyAsync(d =>
                    d.EncounterId == encounterId &&
                    d.Status == "Active" &&
                    d.Icd10code == icd10Code);
        }

        public async Task<int> GetMaxDiagnosisSequenceAsync(int encounterId)
        {
            var max = await context.Diagnoses
                .Where(d => d.EncounterId == encounterId && d.Status == "Active")
                .MaxAsync(d => (int?)d.Sequence);

            return max ?? 0;
        }

        public async Task<bool> SequenceTakenAsync(int encounterId, int sequence, int? excludeDxId = null)
        {
            return await context.Diagnoses
                .AnyAsync(d =>
                    d.EncounterId == encounterId &&
                    d.Sequence == sequence &&
                    d.Status == "Active" &&
                    (excludeDxId == null || d.DxId != excludeDxId.Value));
        }

        public async Task<int> GetActiveDiagnosisCountAsync(int encounterId)
        {
            return await context.Diagnoses
                .CountAsync(d => d.EncounterId == encounterId && d.Status == "Active");
        }

        public async Task<Diagnosis> AddDiagnosisAsync(Diagnosis diagnosis)
        {
            await context.Diagnoses.AddAsync(diagnosis);
            await context.SaveChangesAsync();
            return diagnosis;
        }

        public async Task UpdateDiagnosisAsync(Diagnosis diagnosis)
        {
            context.Diagnoses.Update(diagnosis);
            await context.SaveChangesAsync();
        }
    }
}

