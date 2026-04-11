using System.Collections.Generic;

namespace Telebill.Dto.Coding
{
    public class AddDiagnosisDto
    {
        public int EncounterId { get; set; }
        public string ICD10Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? Sequence { get; set; }
    }

    public class UpdateDiagnosisDto
    {
        public string? ICD10Code { get; set; }
        public string? Description { get; set; }
        public int? Sequence { get; set; }
    }

    public class DiagnosisResultDto
    {
        public int DxId { get; set; }
        public int EncounterId { get; set; }
        public string? ICD10Code { get; set; }
        public string? Description { get; set; }
        public int Sequence { get; set; }
        public string? Status { get; set; }
    }
}

