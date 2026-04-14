namespace Telebill.Validations.Coding;
public enum EncounterWorkflowStatus
{
    Open,
    ReadyForCoding,
    Finalized
}

public enum DiagnosisRecordStatus
{
    Active,
    Removed
}

public enum CodingLockRecordStatus
{
    Locked,
    Unlocked
}

public enum ChargeLineRecordStatus
{
    Draft,
    Finalized
}

public enum AttestationRecordStatus
{
    Attested,
    Voided
}

public enum TelehealthVisitType
{
    Video,
    Audio,
    Async
}
