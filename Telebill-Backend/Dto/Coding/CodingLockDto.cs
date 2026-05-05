using System;
using System.Collections.Generic;

namespace Telebill.Dto.Coding
{
    public class ApplyCodingLockDto
    {
        public int EncounterId { get; set; }
        public string? Notes { get; set; }
    }

    public class UnlockCodingDto
    {
        public int EncounterId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class CodingLockResultDto
    {
        public int CodingLockId { get; set; }
        public int EncounterId { get; set; }
        public int CoderId { get; set; }
        public string? CoderName { get; set; }
        public DateTime LockedDate { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
    }

    public class ApplyCodingLockResponseDto
    {
        public CodingLockResultDto? CodingLock { get; set; }
        public string? EncounterStatus { get; set; }
        public bool ClaimBuildTriggered { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }

    public class CodingValidationResultDto
    {
        public int EncounterId { get; set; }
        public bool CanLock { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class UnlockCodingResponseDto
    {
        public int EncounterId { get; set; }
        public string? EncounterStatus { get; set; }
        public int PreviousLockId { get; set; }
    }
}

