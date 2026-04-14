export type EncounterStatus = 'Open' | 'ReadyForCoding' | 'Finalized'
export type ChargeStatus = 'Draft' | 'Finalized'

export type Encounter = {
    encounterId: number
    patientId: number
    patientName: string
    providerId: number
    providerName: string
    encounterDate: string
    pos: string
    notes: string
    status: EncounterStatus
}

export type ChargeLine = {
    chargeId: number
    encounterId: number
    lineNo: number
    cptCode: string
    modifiers: string
    units: number
    chargeAmount: number
    dxPointers: string
    status: ChargeStatus
}

export type Diagnosis = {
    diagnosisId: number
    encounterId: number
    icdCode: string
    description: string
    sequence: number
}

export type Attestation = {
    attestId: number
    encounterId: number
    providerId: number
    providerName: string
    attestedDate: string
    signatureNote: string
}