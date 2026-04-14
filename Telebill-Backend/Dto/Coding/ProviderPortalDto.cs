using System;

namespace Telebill.Dto.Coding
{
    public class SetDocumentationUriDto
    {
        public string DocumentationUri { get; set; } = string.Empty;
    }

    public class ProviderEncounterSummaryDto
    {
        public int EncounterId { get; set; }
        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
        public DateTime EncounterDateTime { get; set; }
        public string? VisitType { get; set; }
        public string? Pos { get; set; }
        public string? DocumentationUri { get; set; }
        public string? Status { get; set; }
        public bool HasAttestation { get; set; }
        public bool AllChargesFinalized { get; set; }
        public bool ReadyToHandOff { get; set; }
        public int ChargeLineCount { get; set; }
        public decimal TotalCharge { get; set; }
    }
}

