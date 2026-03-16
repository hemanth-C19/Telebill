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
    public class EncounterUpdateDTO
    {
        public DateTime EncounterDateTime { get; set; }
        public string? VisitType { get; set; }
        public string? Pos { get; set; }
        public string? DocumentationUri { get; set; }
        public string? Status { get; set; }

    }


    
public class ChargeLineDTO
    {
        public int ChargeId { get; set; }
        public int? EncounterId { get; set; }
        public string? CPT_HCPCS { get; set; } = string.Empty;
        public string? Modifiers { get; set; } // or List<string> if you mapped value converter
        public int? Units { get; set; }
        public decimal? ChargeAmount { get; set; }
        public string? RevenueCode { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; } = "Draft";
    }

    public class ChargeLineCreateDto
    {
        public string CptHcpcs { get; set; } = string.Empty;
        public string? Modifiers { get; set; }
        public int Units { get; set; }
        public decimal? ChargeAmount { get; set; }
        public string? RevenueCode { get; set; }
        public string? Notes { get; set; }
    }

    public class ChargeLineUpdateDto
    {
        public string? Modifiers { get; set; }
        public int? Units { get; set; }
        public decimal? ChargeAmount { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
    }

    public class AttestationDTO
    {
        public int AttestId { get; set; }
        public int? EncounterId { get; set; }
        public int? ProviderId { get; set; }
        public string? AttestText { get; set; } = string.Empty;
        public DateTime? AttestDate { get; set; }
        public string? Status { get; set; } = "Draft";
    }

    public class AttestationCreateDto
    {
        public int ProviderID { get; set; }
        public string AttestText { get; set; } = string.Empty;
    }

    public class AttestationUpdateDto
    {
        public string? AttestText { get; set; }
        public string? Status { get; set; }
    }


}