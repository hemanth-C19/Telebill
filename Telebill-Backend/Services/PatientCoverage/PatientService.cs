using System.Text.Json;
using Telebill.DTOs;
using Telebill.Models;
using Telebill.Repositories.PatientCoverage;

namespace Telebill.Services.PatientCoverage;


public class PatientService(IPatientRepository repo) : IPatientService
{

    public async Task<Patient> RegisterPatient(PatientDto dto)
    {
        var patient = new Patient
        {
            Name = dto.Name,
            Dob = dto.DOB,
            Gender = dto.Gender,
            ContactInfo = dto.ContactInfo,
            Mrn = "PT-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
            Status = dto.Status,
            AddressJson = JsonSerializer.Serialize(new { dto.Street, dto.Area, dto.City })
        };
        await repo.AddPatientAsync(patient);
        await repo.SaveChangesAsync();
        return patient;
    }

    public async Task<Patient> GetPatientById(int id)
    {
        var patient = await repo.GetPatientByIdAsync(id);
        if (patient == null)
        {
            throw new KeyNotFoundException("Patient not found");
        }
        return patient;
    }
    public async Task<Coverage> AddInsurance(CoverageDto dto)
    {
        var coverage = new Coverage
        {
            PatientId = dto.PatientID,
            PlanId = dto.PlanID,
            MemberId = dto.MemberID,
            GroupNumber = dto.GroupNumber,

            EffectiveFrom = DateOnly.FromDateTime(dto.EffectiveFrom),
            EffectiveTo = DateOnly.FromDateTime(dto.EffectiveFrom),
            Status = "Active"
        };
        await repo.AddCoverageAsync(coverage);
        await repo.SaveChangesAsync();
        return coverage;
    }

    public async Task<EligibilityRef> VerifyInsurance(int coverageId)
    {
        var result = new EligibilityRef
        {
            CoverageId = coverageId,
            CheckedDate = DateTime.Now,
            Result = "Eligible",
            Notes = "Verified via TeleBill Phase-1 Mocking System"
        };
        await repo.AddEligibilityRefAsync(result);
        await repo.SaveChangesAsync();
        return result;
    }

    // Standard Read/Update methods...
    public async Task<Coverage> GetCoverageDetailsAsync(int id)
    {
        var coverage = await repo.GetCoverageByIdAsync(id);
        if (coverage == null)
        {
            throw new KeyNotFoundException("Coverage not found");
        }
        return coverage;
    }
    public async Task<IEnumerable<Patient>> ListAllPatients(string? search, int page, int limit) => await repo.GetAllPatientsAsync(search, page, limit);
    public async Task<IEnumerable<Coverage>> GetPatientInsurance(int patientId) => await repo.GetCoveragesByPatientIdAsync(patientId);
    public async Task UpdatePatient(int id, PatientDto dto)
    {


        var p = await repo.GetPatientByIdAsync(id);
        if (p == null)
            throw new KeyNotFoundException("Patient not found");

        Console.WriteLine("----------------------------------------------------");
        Console.WriteLine(dto);

        p.Name = dto.Name!;
        p.Dob = dto.DOB;
        p.Gender = dto.Gender;
        p.ContactInfo = dto.ContactInfo;
        p.Status = dto.Status;

        var addressObj = new
        {
            Street = dto.Street,
            Area = dto.Area,
            City = dto.City
        };

        p.AddressJson = JsonSerializer.Serialize(addressObj);

        p.AddressJson = JsonSerializer.Serialize(addressObj);

        repo.UpdatePatient(p);
        await repo.SaveChangesAsync();
    }
    public async Task<bool> RemovePatient(int patientId)
    {
        // 1. Get all coverages associated with this patient
        var coverages = await repo.GetCoveragesByPatientIdAsync(patientId);

        // 2. Delete each coverage first
        foreach (var coverage in coverages)
        {
            await repo.DeleteCoverageAsync(coverage.CoverageId);
        }

        // 3. Now it is safe to delete the patient
        await repo.DeletePatientAsync(patientId);

        // 4. Save all changes together in one transaction
        await repo.SaveChangesAsync();

        return true;
    }
    public async Task<bool> RemoveCoverage(int patientId, int CoverageId)
    {
        await repo.DeleteCoverageByCoverageIdAsync(patientId, CoverageId);
        await repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCoverageAsync(int coverageId)
    {
        await repo.DeleteCoverageAsync(coverageId);
        await repo.SaveChangesAsync();
        return true;
    }
}