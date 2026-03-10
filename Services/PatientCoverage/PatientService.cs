using System.Text.Json;
using Telebill.DTOs;
using Telebill.Models;
using Telebill.Repositories.PatientCoverage;

namespace Telebill.Services.PatientCoverage;


public class PatientService : IPatientService {
    private readonly IPatientRepository _repo;
    public PatientService(IPatientRepository repo) => _repo = repo;

    public async Task<Patient> RegisterPatient(PatientDto dto) {
        var patient = new Patient {
            Name = dto.Name,
            Dob = DateOnly.FromDateTime(dto.DOB),
            Gender = dto.Gender,
            ContactInfo = dto.ContactInfo,
            Mrn = "PT-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
            Status = "Active",
            AddressJson = JsonSerializer.Serialize(new { dto.Street, dto.Area, dto.City })
        };
        await _repo.AddPatientAsync(patient);
        await _repo.SaveChangesAsync();
        return patient;
    }

public async Task<Patient> GetPatientById(int id)
{
    var patient = await _repo.GetPatientByIdAsync(id);
    if (patient == null)
    {
        // You can throw a custom exception here or handle it in the controller
        return null;
    }
    return patient;
}
    public async Task<Coverage> AddInsurance(CoverageDto dto) {
        var coverage = new Coverage {
            PatientId = dto.PatientID,
            PlanId = dto.PlanID,
            MemberId = dto.MemberID,
            GroupNumber = dto.GroupNumber,
            
            EffectiveFrom = DateOnly.FromDateTime(dto.EffectiveFrom),
            EffectiveTo = DateOnly.FromDateTime(dto.EffectiveFrom),
            Status = "Active"
        };
        await _repo.AddCoverageAsync(coverage);
        await _repo.SaveChangesAsync();
        return coverage;
    }

    public async Task<EligibilityRef> VerifyInsurance(int coverageId) {
        // Phase-1 Mock Logic: Automatically return "Eligible" for Indian References
        var result = new EligibilityRef {
            CoverageId = coverageId,
            CheckedDate = DateTime.Now,
            Result = "Eligible",
            Notes = "Verified via TeleBill Phase-1 Mocking System"
        };
        await _repo.AddEligibilityRefAsync(result);
        await _repo.SaveChangesAsync();
        return result;
    }

    // Standard Read/Update methods...
    public async Task<Coverage> GetCoverageDetailsAsync(int id) => await _repo.GetCoverageByIdAsync(id);
    public async Task<IEnumerable<Patient>> ListAllPatients() => await _repo.GetAllPatientsAsync();
    public async Task<IEnumerable<Coverage>> GetPatientInsurance(int patientId) => await _repo.GetCoveragesByPatientIdAsync(patientId);
    public async Task UpdatePatient(int id, PatientDto dto) {
        var p = await _repo.GetPatientByIdAsync(id);
        if (p == null) return;
        p.Name = dto.Name;
        p.ContactInfo = dto.ContactInfo;
        _repo.UpdatePatient(p);
        await _repo.SaveChangesAsync();
    }
public async Task<bool> RemovePatient(int patientId)
{
    // 1. Get all coverages associated with this patient
    var coverages = await _repo.GetCoveragesByPatientIdAsync(patientId);

    // 2. Delete each coverage first
    foreach (var coverage in coverages)
    {
        await _repo.DeleteCoverageAsync(coverage.CoverageId);
    }

    // 3. Now it is safe to delete the patient
    await _repo.DeletePatientAsync(patientId);

    // 4. Save all changes together in one transaction
    await _repo.SaveChangesAsync();
    
    return true;
}
public async Task<bool> RemoveCoverage(int patientId ,int CoverageId)
{
    await _repo.DeleteCoverageByCoverageIdAsync(patientId, CoverageId);
    await _repo.SaveChangesAsync();
    return true;
}

    public Task<bool> DeleteCoverageAsync(int coverageId)
    {
        throw new NotImplementedException();
    }
}