using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Telebill.Models;

namespace Telebill.Repositories.Coding
{
    public class DiagnosisRepository : IDiagnosisRepository
    {
        private readonly TeleBillContext _context;

        public DiagnosisRepository(TeleBillContext context)
        {
            _context = context;
        }

        public async Task<List<Diagnosis>> GetDiagnosesByEncounterAsync(int encounterId)
        {
            return await _context.Diagnoses
                .Where(d => d.EncounterId == encounterId)
                .OrderBy(d => d.Sequence)
                .ToListAsync();
        }

        public async Task<List<Diagnosis>> GetActiveDiagnosesByEncounterAsync(int encounterId)
        {
            return await _context.Diagnoses
                .Where(d => d.EncounterId == encounterId && d.Status == "Active")
                .OrderBy(d => d.Sequence)
                .ToListAsync();
        }

        public async Task<Diagnosis?> GetDiagnosisByIdAsync(int dxId)
        {
            return await _context.Diagnoses.FindAsync(dxId);
        }

        public async Task<bool> DiagnosisCodeExistsActiveAsync(int encounterId, string icd10Code)
        {
            return await _context.Diagnoses
                .AnyAsync(d =>
                    d.EncounterId == encounterId &&
                    d.Status == "Active" &&
                    d.Icd10code == icd10Code);
        }

        public async Task<int> GetMaxDiagnosisSequenceAsync(int encounterId)
        {
            var max = await _context.Diagnoses
                .Where(d => d.EncounterId == encounterId && d.Status == "Active")
                .MaxAsync(d => (int?)d.Sequence);

            return max ?? 0;
        }

        public async Task<bool> SequenceTakenAsync(int encounterId, int sequence, int? excludeDxId = null)
        {
            return await _context.Diagnoses
                .AnyAsync(d =>
                    d.EncounterId == encounterId &&
                    d.Sequence == sequence &&
                    d.Status == "Active" &&
                    (excludeDxId == null || d.DxId != excludeDxId.Value));
        }

        public async Task<int> GetActiveDiagnosisCountAsync(int encounterId)
        {
            return await _context.Diagnoses
                .CountAsync(d => d.EncounterId == encounterId && d.Status == "Active");
        }

        public async Task<Diagnosis> AddDiagnosisAsync(Diagnosis diagnosis)
        {
            await _context.Diagnoses.AddAsync(diagnosis);
            await _context.SaveChangesAsync();
            return diagnosis;
        }

        public async Task UpdateDiagnosisAsync(Diagnosis diagnosis)
        {
            _context.Diagnoses.Update(diagnosis);
            await _context.SaveChangesAsync();
        }
    }
}

