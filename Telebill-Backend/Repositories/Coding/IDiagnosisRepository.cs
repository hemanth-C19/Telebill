using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Models;

namespace Telebill.Repositories.Coding
{
    public interface IDiagnosisRepository
    {
        Task<List<Diagnosis>> GetDiagnosesByEncounterAsync(int encounterId);
        Task<List<Diagnosis>> GetActiveDiagnosesByEncounterAsync(int encounterId);
        Task<Diagnosis?> GetDiagnosisByIdAsync(int dxId);
        Task<bool> DiagnosisCodeExistsActiveAsync(int encounterId, string icd10Code);
        Task<int> GetMaxDiagnosisSequenceAsync(int encounterId);
        Task<bool> SequenceTakenAsync(int encounterId, int sequence, int? excludeDxId = null);
        Task<int> GetActiveDiagnosisCountAsync(int encounterId);
        Task<Diagnosis> AddDiagnosisAsync(Diagnosis diagnosis);
        Task UpdateDiagnosisAsync(Diagnosis diagnosis);
    }
}

