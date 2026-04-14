// FrontDesk shared types — patients, coverage, encounters (UI + forms).

/** Patient / coverage (Patients page) */
export type Patient = {
  patientId: number
  name: string
  dob: string
  gender: string
  contactInfo: string
  street: string
  area: string
  city: string
  mrn: string
  status: string
}

export type Coverage = {
  coverageId: number
  patientId: number
  planId: number
  planName: string
  memberId: string
  groupNumber: string
  effectiveFrom: string
  effectiveTo: string
  status: string
}

export type RegisterFormValues = {
  name: string
  dob: string
  gender: string
  contactInfo: string
  street: string
  area: string
  city: string
}

export type CoverageFormValues = {
  planId: string
  memberId: string
  groupNumber: string
  effectiveFrom: string
  effectiveTo: string
}

export type PatientEditFormValues = {
  name: string
  contactInfo: string
}

export type PayerPlanOption = {
  planId: number
  planName: string
}

/** Encounters page */
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
  chargeLineCount: number
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

export type Attestation = {
  attestId: number
  encounterId: number
  attestedBy: string
  attestedDate: string
  status: 'Attested' | 'NotAttested'
}

export type CreateEncounterFormValues = {
  patientId: string
  providerId: string
  encounterDate: string
  pos: string
  notes: string
}

export type EditEncounterFormValues = {
  encounterDate: string
  pos: string
  notes: string
}

export type AddChargeFormValues = {
  cptCode: string
  modifiers: string
  units: string
  chargeAmount: string
  dxPointers: string
}

export type EditChargeFormValues = AddChargeFormValues

export type PosOption = {
  value: string
  label: string
}

export type EncounterPatientOption = {
  patientId: number
  name: string
  mrn: string
}

export type EncounterProviderOption = {
  providerId: number
  name: string
  specialty: string
}
