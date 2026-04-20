using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Telebill.DTOs
{
    public class PatientDto {
    // public int PatientID { get; set; }
    public string? Name { get; set; }
    public DateOnly DOB { get; set; }
    public string? Gender { get; set; }
    public string? ContactInfo { get; set; }
    // Structured address for the UI
    public string? Street { get; set; }
    public string? Area { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; } // <-- Add this line

    public string? Status {get; set;}
}

public class CoverageDto {
    // public int CoverageID { get; set; }
    public int PatientID { get; set; }
    public int PlanID { get; set; } // From Module 2 Master List
    public string? MemberID { get; set; }
    public string? GroupNumber { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime EffectiveTo { get; set; }
}


}