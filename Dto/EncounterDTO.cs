using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Telebill.Dto
{
    public class EncounterDTO
    {
        
    public int EncounterId { get; set; }
    public int? PatientId { get; set; }
    public int? ProviderId { get; set; }
    public DateTime EncounterDateTime { get; set; }
    public string? VisitType { get; set; }
    public string? Pos { get; set; }
    public string? DocumentationUri { get; set; }
    public string? Status { get; set; }

    }

}