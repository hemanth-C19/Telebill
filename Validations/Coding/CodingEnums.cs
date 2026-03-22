namespace Telebill.Validations.Coding;

/// <summary>Encounter.Status (Document 2).</summary>
public enum EncounterWorkflowStatus
{
    Open,
    ReadyForCoding,
    Finalized
}

/// <summary>Diagnosis.Status (Document 2).</summary>
public enum DiagnosisRecordStatus
{
    Active,
    Removed
}

/// <summary>CodingLock.Status (Document 2).</summary>
public enum CodingLockRecordStatus
{
    Locked,
    Unlocked
}

/// <summary>ChargeLine.Status (Document 2).</summary>
public enum ChargeLineRecordStatus
{
    Draft,
    Finalized
}

/// <summary>Attestation.Status (Document 2).</summary>
public enum AttestationRecordStatus
{
    Attested,
    Voided
}

/// <summary>Encounter.VisitType for telehealth (Document 2).</summary>
public enum TelehealthVisitType
{
    Video,
    Audio,
    Async
}
