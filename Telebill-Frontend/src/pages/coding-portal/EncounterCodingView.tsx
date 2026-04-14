import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import { Dialog } from '../../components/shared/ui/Dialog'

type EncounterCard = {
  encounterId: number
  status: string
  encounterDate: string
  pos: string
  documentationUri: string
  provider: { providerId: number; name: string; npi: string; taxonomy: string }
  attestation: { attestId: number; attestText: string; attestDate: string; status: string } | null
  patient: { patientId: number; name: string; mrn: string; gender: string; dob: string }
  chargeLines: ChargeLineInfo[]
  primaryPayerPlan: {
    planId: number
    planName: string
    networkType: string
    requiredModifiers: string[]
    acceptedModifiers: string[]
    memberId: string
  }
  coverageWarning: string
  activeLock: { codingLockId: number; coderName: string; lockedDate: string; status: string } | null
}

type ChargeLineInfo = {
  chargeId: number
  cptHcpcs: string
  modifiers: string
  modifierList: string[]
  units: number
  chargeAmount: number
  notes: string
  status: string
  modifiersValid: boolean
}

type Diagnosis = {
  dxId: number
  encounterId: number
  icd10Code: string
  description: string
  sequence: number
  status: 'Active' | 'Inactive'
}

type ValidationResult = {
  encounterId: number
  canLock: boolean
  errors: string[]
  warnings: string[]
}

const DUMMY_ENCOUNTER_CARDS: Record<number, EncounterCard> = {
  1002: {
    encounterId: 1002,
    status: 'ReadyForCoding',
    encounterDate: '2024-11-08T14:00',
    pos: '10',
    documentationUri: '',
    provider: { providerId: 202, name: 'Dr. James Patel', npi: '2345678901', taxonomy: 'Psychiatry' },
    attestation: { attestId: 9002, attestText: 'I attest that the documentation accurately reflects the services rendered.', attestDate: '2024-11-08', status: 'Attested' },
    patient: { patientId: 2, name: 'Bob Martinez', mrn: 'PT-E5F6G7H8', gender: 'Male', dob: '1972-07-22' },
    chargeLines: [
      { chargeId: 5003, cptHcpcs: '99214', modifiers: '25', modifierList: ['25'], units: 1, chargeAmount: 200, notes: '', status: 'Finalized', modifiersValid: true },
      { chargeId: 5004, cptHcpcs: '90837', modifiers: '', modifierList: [], units: 1, chargeAmount: 175, notes: '', status: 'Finalized', modifiersValid: false },
      { chargeId: 5005, cptHcpcs: '96130', modifiers: '', modifierList: [], units: 2, chargeAmount: 220, notes: '', status: 'Finalized', modifiersValid: false },
    ],
    primaryPayerPlan: { planId: 201, planName: 'Aetna Select PPO', networkType: 'PPO', requiredModifiers: ['GT'], acceptedModifiers: ['GT', 'GQ', '95'], memberId: 'AET-202' },
    coverageWarning: '',
    activeLock: null,
  },
  1006: {
    encounterId: 1006,
    status: 'ReadyForCoding',
    encounterDate: '2024-11-15T10:00',
    pos: '10',
    documentationUri: 'notes/enc-1006.pdf',
    provider: { providerId: 203, name: 'Dr. Mark Liu', npi: '3456789012', taxonomy: 'Cardiology' },
    attestation: { attestId: 9006, attestText: 'I attest that the documentation accurately reflects the services rendered.', attestDate: '2024-11-15', status: 'Attested' },
    patient: { patientId: 1, name: 'Alice Johnson', mrn: 'PT-A1B2C3D4', gender: 'Female', dob: '1985-03-14' },
    chargeLines: [
      { chargeId: 5009, cptHcpcs: '99214', modifiers: '', modifierList: [], units: 1, chargeAmount: 200, notes: '', status: 'Finalized', modifiersValid: false },
      { chargeId: 5010, cptHcpcs: '90837', modifiers: 'GT', modifierList: ['GT'], units: 1, chargeAmount: 175, notes: '', status: 'Finalized', modifiersValid: true },
      { chargeId: 5011, cptHcpcs: 'G0296', modifiers: '', modifierList: [], units: 1, chargeAmount: 95, notes: '', status: 'Finalized', modifiersValid: false },
      { chargeId: 5012, cptHcpcs: '93000', modifiers: '', modifierList: [], units: 1, chargeAmount: 85, notes: '', status: 'Finalized', modifiersValid: false },
    ],
    primaryPayerPlan: { planId: 101, planName: 'BlueCross PPO Basic', networkType: 'PPO', requiredModifiers: ['GT'], acceptedModifiers: ['GT', 'GQ'], memberId: 'BCB-001' },
    coverageWarning: '',
    activeLock: { codingLockId: 4001, coderName: 'Jane Coder', lockedDate: '2024-11-16T09:00', status: 'Locked' },
  },
  1009: {
    encounterId: 1009,
    status: 'ReadyForCoding',
    encounterDate: '2024-12-05T14:30',
    pos: '02',
    documentationUri: '',
    provider: { providerId: 201, name: 'Dr. Sarah Chen', npi: '1234567890', taxonomy: 'Internal Medicine' },
    attestation: { attestId: 9009, attestText: 'I attest that the documentation accurately reflects the services rendered.', attestDate: '2024-12-05', status: 'Attested' },
    patient: { patientId: 5, name: 'Emily Rodriguez', mrn: 'PT-Q7R8S9T0', gender: 'Female', dob: '1998-06-18' },
    chargeLines: [
      { chargeId: 5016, cptHcpcs: '99214', modifiers: '', modifierList: [], units: 1, chargeAmount: 200, notes: '', status: 'Finalized', modifiersValid: false },
      { chargeId: 5017, cptHcpcs: '93000', modifiers: 'GT', modifierList: ['GT'], units: 1, chargeAmount: 85, notes: '', status: 'Finalized', modifiersValid: true },
    ],
    primaryPayerPlan: { planId: 101, planName: 'BlueCross PPO Basic', networkType: 'PPO', requiredModifiers: ['GT'], acceptedModifiers: ['GT', 'GQ'], memberId: 'BCB-505' },
    coverageWarning: '',
    activeLock: null,
  },
  1010: {
    encounterId: 1010,
    status: 'ReadyForCoding',
    encounterDate: '2024-12-08T09:00',
    pos: '02',
    documentationUri: '',
    provider: { providerId: 202, name: 'Dr. James Patel', npi: '2345678901', taxonomy: 'Psychiatry' },
    attestation: null,
    patient: { patientId: 3, name: 'Carol Nguyen', mrn: 'PT-I9J0K1L2', gender: 'Female', dob: '1990-11-05' },
    chargeLines: [
      { chargeId: 5018, cptHcpcs: '99213', modifiers: '', modifierList: [], units: 1, chargeAmount: 150, notes: '', status: 'Finalized', modifiersValid: false },
      { chargeId: 5019, cptHcpcs: '90837', modifiers: 'GQ', modifierList: ['GQ'], units: 1, chargeAmount: 175, notes: '', status: 'Finalized', modifiersValid: true },
    ],
    primaryPayerPlan: { planId: 202, planName: 'Aetna Choice HMO', networkType: 'HMO', requiredModifiers: ['GQ'], acceptedModifiers: ['GQ', 'GT'], memberId: 'AET-303' },
    coverageWarning: 'Coverage effective date ends 2024-12-31 — verify eligibility.',
    activeLock: null,
  },
  1011: {
    encounterId: 1011,
    status: 'ReadyForCoding',
    encounterDate: '2024-12-10T11:00',
    pos: '02',
    documentationUri: 'notes/enc-1011.pdf',
    provider: { providerId: 201, name: 'Dr. Sarah Chen', npi: '1234567890', taxonomy: 'Internal Medicine' },
    attestation: { attestId: 9011, attestText: 'I attest that the documentation accurately reflects the services rendered.', attestDate: '2024-12-10', status: 'Attested' },
    patient: { patientId: 4, name: 'David Patel', mrn: 'PT-M3N4O5P6', gender: 'Male', dob: '1965-01-30' },
    chargeLines: [{ chargeId: 5020, cptHcpcs: '99215', modifiers: 'GT', modifierList: ['GT'], units: 1, chargeAmount: 250, notes: '', status: 'Finalized', modifiersValid: true }],
    primaryPayerPlan: { planId: 102, planName: 'BCBS HMO Plus', networkType: 'HMO', requiredModifiers: ['GQ'], acceptedModifiers: ['GQ', 'GT'], memberId: 'BCD-001' },
    coverageWarning: '',
    activeLock: { codingLockId: 4002, coderName: 'Jane Coder', lockedDate: '2024-12-11T10:00', status: 'Locked' },
  },
  1012: {
    encounterId: 1012,
    status: 'ReadyForCoding',
    encounterDate: '2024-12-12T15:00',
    pos: '10',
    documentationUri: '',
    provider: { providerId: 203, name: 'Dr. Mark Liu', npi: '3456789012', taxonomy: 'Cardiology' },
    attestation: { attestId: 9012, attestText: 'I attest that the documentation accurately reflects the services rendered.', attestDate: '2024-12-12', status: 'Attested' },
    patient: { patientId: 7, name: 'Grace Kim', mrn: 'PT-Y5Z6A7B8', gender: 'Female', dob: '2001-12-25' },
    chargeLines: [
      { chargeId: 5021, cptHcpcs: '99213', modifiers: '', modifierList: [], units: 1, chargeAmount: 150, notes: '', status: 'Finalized', modifiersValid: false },
      { chargeId: 5022, cptHcpcs: '93000', modifiers: '', modifierList: [], units: 1, chargeAmount: 85, notes: '', status: 'Finalized', modifiersValid: false },
      { chargeId: 5023, cptHcpcs: 'G0296', modifiers: 'GT', modifierList: ['GT'], units: 1, chargeAmount: 95, notes: '', status: 'Finalized', modifiersValid: true },
    ],
    primaryPayerPlan: { planId: 401, planName: 'Cigna HMO', networkType: 'HMO', requiredModifiers: ['GT'], acceptedModifiers: ['GT', '95'], memberId: 'CIG-707' },
    coverageWarning: '',
    activeLock: null,
  },
  1013: {
    encounterId: 1013,
    status: 'ReadyForCoding',
    encounterDate: '2024-12-14T08:30',
    pos: '02',
    documentationUri: '',
    provider: { providerId: 203, name: 'Dr. Mark Liu', npi: '3456789012', taxonomy: 'Cardiology' },
    attestation: { attestId: 9013, attestText: 'I attest that the documentation accurately reflects the services rendered.', attestDate: '2024-12-14', status: 'Attested' },
    patient: { patientId: 6, name: 'Frank Williams', mrn: 'PT-U1V2W3X4', gender: 'Male', dob: '1955-09-09' },
    chargeLines: [
      { chargeId: 5024, cptHcpcs: '99214', modifiers: 'GT', modifierList: ['GT'], units: 1, chargeAmount: 200, notes: '', status: 'Finalized', modifiersValid: true },
      { chargeId: 5025, cptHcpcs: '93000', modifiers: '', modifierList: [], units: 1, chargeAmount: 85, notes: '', status: 'Finalized', modifiersValid: false },
    ],
    primaryPayerPlan: { planId: 301, planName: 'UHC Gold PPO', networkType: 'PPO', requiredModifiers: ['GT', '95'], acceptedModifiers: ['GT', '95', 'GQ'], memberId: 'UHG-404' },
    coverageWarning: '',
    activeLock: null,
  },
}

const INITIAL_DIAGNOSES: Record<number, Diagnosis[]> = {
  1002: [],
  1006: [{ dxId: 601, encounterId: 1006, icd10Code: 'F41.1', description: 'Generalized anxiety disorder', sequence: 2, status: 'Active' }],
  1009: [{ dxId: 602, encounterId: 1009, icd10Code: 'I10', description: 'Essential hypertension', sequence: 1, status: 'Active' }],
  1010: [],
  1011: [
    { dxId: 603, encounterId: 1011, icd10Code: 'I10', description: 'Essential hypertension', sequence: 1, status: 'Active' },
    { dxId: 604, encounterId: 1011, icd10Code: 'E11.9', description: 'Type 2 diabetes w/o complication', sequence: 2, status: 'Active' },
  ],
  1012: [],
  1013: [{ dxId: 605, encounterId: 1013, icd10Code: 'I25.10', description: 'Atherosclerotic heart disease', sequence: 1, status: 'Active' }],
}

type DxFormValues = { icd10Code: string; description: string; sequence: string }

function formatDate(input: string): string {
  const date = new Date(input)
  if (Number.isNaN(date.getTime())) return input
  return date.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })
}

function formatDateTime(input: string): string {
  const date = new Date(input)
  if (Number.isNaN(date.getTime())) return input
  const d = date.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })
  const t = date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false })
  return `${d} ${t}`
}

export default function EncounterCodingView() {
  const { encounterId } = useParams<{ encounterId: string }>()
  const navigate = useNavigate()
  const encounterIdNum = Number(encounterId)

  const card = DUMMY_ENCOUNTER_CARDS[encounterIdNum]

  if (card == null) {
    return (
      <div className="min-h-[60vh] flex flex-col items-center justify-center gap-3">
        <p className="text-gray-500">Encounter not found.</p>
        <button type="button" className="text-blue-600 hover:underline" onClick={() => navigate('/coding/worklist')}>
          ← Back to Worklist
        </button>
      </div>
    )
  }

  const [encounter, setEncounter] = useState<EncounterCard>(card)
  const [diagnoses, setDiagnoses] = useState<Diagnosis[]>(INITIAL_DIAGNOSES[encounterIdNum] ?? [])
  const [lock, setLock] = useState(card.activeLock ?? null)
  const [editingModifiersId, setEditingModifiersId] = useState<number | null>(null)
  const [modifierInput, setModifierInput] = useState('')
  const [editingDxId, setEditingDxId] = useState<number | null>(null)
  const [editDxForm, setEditDxForm] = useState({ icd10Code: '', description: '', sequence: '' })
  const [validation, setValidation] = useState<ValidationResult | null>(null)
  const [showLockDialog, setShowLockDialog] = useState(false)
  const [lockNotes, setLockNotes] = useState('')
  const [showUnlockDialog, setShowUnlockDialog] = useState(false)
  const [unlockReason, setUnlockReason] = useState('')
  const dxForm = useForm<DxFormValues>({ defaultValues: { sequence: '' } })

  const activeDiagnoses = useMemo(() => diagnoses.filter((d) => d.status === 'Active'), [diagnoses])
  const hasPrimaryDx = activeDiagnoses.some((d) => d.sequence === 1)
  const activeDxCount = activeDiagnoses.length

  const runValidation = (): ValidationResult => {
    const errors: string[] = []
    const warnings: string[] = []
    if (!hasPrimaryDx) errors.push('Sequence 1 (principal diagnosis) is required before locking.')
    if (activeDxCount === 0) errors.push('At least one active diagnosis is required.')
    if (!encounter.attestation || encounter.attestation.status !== 'Attested') {
      errors.push('Encounter has not been attested by the provider.')
    }
    const invalidLines = encounter.chargeLines.filter((cl) => !cl.modifiersValid)
    if (invalidLines.length > 0) warnings.push(`${invalidLines.length} charge line(s) have missing or invalid modifiers for this payer plan.`)
    if (encounter.coverageWarning) warnings.push(encounter.coverageWarning)
    return { encounterId: encounterIdNum, canLock: errors.length === 0, errors, warnings }
  }

  const handleSaveModifiers = (chargeId: number) => {
    const modList = modifierInput
      .split(',')
      .map((m) => m.trim().toUpperCase())
      .filter(Boolean)
    const required = encounter.primaryPayerPlan.requiredModifiers
    const accepted = encounter.primaryPayerPlan.acceptedModifiers
    const isValid = required.every((r) => modList.includes(r)) && modList.every((m) => accepted.includes(m))
    setEncounter((prev) => ({
      ...prev,
      chargeLines: prev.chargeLines.map((cl) =>
        cl.chargeId === chargeId ? { ...cl, modifiers: modList.join(','), modifierList: modList, modifiersValid: isValid } : cl,
      ),
    }))
    setEditingModifiersId(null)
    setValidation(null)
  }

  const handleSaveDx = (dxId: number) => {
    setDiagnoses((prev) =>
      prev.map((d) =>
        d.dxId === dxId
          ? { ...d, icd10Code: editDxForm.icd10Code.toUpperCase(), description: editDxForm.description, sequence: Number(editDxForm.sequence) }
          : d,
      ),
    )
    setEditingDxId(null)
    setValidation(null)
  }

  const handleSoftDeleteDx = (dxId: number) => {
    setDiagnoses((prev) => prev.map((d) => (d.dxId === dxId ? { ...d, status: 'Inactive' } : d)))
    setValidation(null)
  }

  const onAddDiagnosis = dxForm.handleSubmit((data) => {
    const newDx: Diagnosis = {
      dxId: Math.floor(Math.random() * 9000) + 700,
      encounterId: encounterIdNum,
      icd10Code: data.icd10Code.toUpperCase(),
      description: data.description,
      sequence: Number(data.sequence),
      status: 'Active',
    }
    setDiagnoses((prev) => [...prev, newDx])
    dxForm.reset()
    setValidation(null)
  })

  return (
    <div className="pb-24">
      <button type="button" className="text-blue-600 hover:underline" onClick={() => navigate('/coding/worklist')}>
        ← Back to Worklist
      </button>

      <div className="flex items-center gap-3 mt-2">
        <h1 className="text-2xl font-bold text-gray-900">Encounter #{encounterIdNum} — Coding View</h1>
        <Badge status={encounter.status} />
      </div>

      <div className="flex gap-5 mt-4">
        <div className="w-[42%] flex-shrink-0 space-y-4">
          <Card title="Patient & Encounter Info">
            <div className="grid grid-cols-2 gap-3 text-sm">
              <p><span className="text-gray-500">Patient:</span> <strong>{encounter.patient.name}</strong></p>
              <p><span className="text-gray-500">MRN:</span> <span className="font-mono">{encounter.patient.mrn}</span></p>
              <p><span className="text-gray-500">DOB:</span> {formatDate(encounter.patient.dob)}</p>
              <p><span className="text-gray-500">Gender:</span> {encounter.patient.gender}</p>
              <p><span className="text-gray-500">Provider:</span> {encounter.provider.name}</p>
              <p><span className="text-gray-500">NPI:</span> {encounter.provider.npi}</p>
              <p><span className="text-gray-500">Specialty:</span> {encounter.provider.taxonomy}</p>
              <p><span className="text-gray-500">Encounter Date:</span> {formatDateTime(encounter.encounterDate)}</p>
              <p className="col-span-2">
                <span className="text-gray-500">POS:</span>{' '}
                {encounter.pos === '02' ? '02 – Telehealth Home' : '10 – Telehealth Non-Home'}
              </p>
            </div>
          </Card>

          <Card title="Attestation" className={encounter.attestation != null ? 'border-l-4 border-l-green-500' : 'border-l-4 border-l-amber-500'}>
            {encounter.attestation != null ? (
              <div>
                <div className="bg-green-50 text-green-700 text-sm font-medium px-3 py-2 rounded-md mb-3">✓ Attested</div>
                <div className="grid grid-cols-2 gap-2 text-sm">
                  <p><span className="text-gray-500">Attestation Date:</span> {formatDate(encounter.attestation.attestDate)}</p>
                </div>
                <p className="text-xs text-gray-500 italic mt-2 line-clamp-2">{encounter.attestation.attestText}</p>
              </div>
            ) : (
              <div className="bg-amber-50 text-amber-700 text-sm font-medium px-3 py-2 rounded-md">
                ⚠ Not attested — provider must attest before this encounter can be locked.
              </div>
            )}
          </Card>

          <Card title="Payer Plan">
            <div className="space-y-2 text-sm">
              <p><span className="text-gray-500">Plan Name:</span> {encounter.primaryPayerPlan.planName}</p>
              <p><span className="text-gray-500">Network Type:</span> {encounter.primaryPayerPlan.networkType}</p>
              <p><span className="text-gray-500">Member ID:</span> {encounter.primaryPayerPlan.memberId}</p>
              <div>
                <p className="text-gray-500 mb-1">Required Modifiers:</p>
                <div className="flex flex-wrap gap-1">
                  {encounter.primaryPayerPlan.requiredModifiers.map((mod) => (
                    <span key={mod} className="bg-blue-100 text-blue-700 text-xs px-2 py-0.5 rounded-full">{mod}</span>
                  ))}
                </div>
              </div>
              <div>
                <p className="text-gray-500 mb-1">Accepted Modifiers:</p>
                <div className="flex flex-wrap gap-1">
                  {encounter.primaryPayerPlan.acceptedModifiers.map((mod) => (
                    <span key={mod} className="bg-gray-100 text-gray-700 text-xs px-2 py-0.5 rounded-full">{mod}</span>
                  ))}
                </div>
              </div>
              {encounter.coverageWarning && (
                <div className="bg-amber-50 border border-amber-200 rounded-lg px-3 py-2 text-amber-700 text-xs mt-2">
                  ⚠ {encounter.coverageWarning}
                </div>
              )}
            </div>
          </Card>

          <Card title="Charge Lines">
            <div className="border rounded-lg overflow-hidden">
              <div className="grid grid-cols-6 gap-2 px-3 py-2 text-xs font-semibold text-gray-500 bg-gray-50 uppercase">
                <span>Line</span><span>CPT</span><span>Modifiers</span><span>Units</span><span>Amount</span><span>Valid</span>
              </div>
              {encounter.chargeLines.map((cl) => (
                <div key={cl.chargeId} className="grid grid-cols-6 gap-2 px-3 py-2 text-sm border-t items-center">
                  <span className="text-gray-500 text-xs">#{cl.chargeId}</span>
                  <span className="font-mono font-medium">{cl.cptHcpcs}</span>
                  <span>
                    {editingModifiersId === cl.chargeId ? (
                      <input
                        value={modifierInput}
                        onChange={(e) => setModifierInput(e.target.value)}
                        onBlur={() => handleSaveModifiers(cl.chargeId)}
                        onKeyDown={(e) => e.key === 'Enter' && handleSaveModifiers(cl.chargeId)}
                        className="w-full border border-blue-400 rounded px-1 py-0.5 text-xs focus:outline-none"
                        autoFocus
                      />
                    ) : (
                      <span className="text-gray-600 text-xs">
                        {cl.modifiers || <span className="text-gray-300 italic">none</span>}
                      </span>
                    )}
                  </span>
                  <span className="text-center text-gray-600">{cl.units}</span>
                  <span>${cl.chargeAmount.toFixed(2)}</span>
                  <span className="flex items-center gap-1">
                    {cl.modifiersValid ? (
                      <span className="text-green-600 text-xs font-medium">✓</span>
                    ) : (
                      <span className="text-red-500 text-xs font-medium" title="Required modifiers missing for this payer plan">✗</span>
                    )}
                    <button
                      onClick={() => {
                        setEditingModifiersId(cl.chargeId)
                        setModifierInput(cl.modifiers)
                      }}
                      className="text-gray-400 hover:text-blue-600 ml-1 text-xs underline"
                      type="button"
                    >
                      edit
                    </button>
                  </span>
                </div>
              ))}
            </div>
            <p className="text-xs text-gray-400 mt-1">
              Required modifiers for {encounter.primaryPayerPlan.planName}: {encounter.primaryPayerPlan.requiredModifiers.join(', ')}
            </p>
          </Card>
        </div>

        <div className="flex-1 space-y-4">
          <Card className="border-l-4 border-l-purple-500">
            <div className="flex justify-between items-center mb-3">
              <div className="flex items-center">
                <h3 className="font-semibold text-gray-800">Diagnoses</h3>
                <span className="text-sm text-gray-500 ml-2">{activeDxCount}/12 active</span>
              </div>
              {activeDxCount === 12 && <span className="text-sm text-gray-400">Max 12 reached</span>}
            </div>

            <div className="border rounded-lg overflow-hidden">
              <div className="grid grid-cols-[3rem_7rem_1fr_6rem_5rem] gap-2 px-3 py-2 bg-gray-50 text-xs font-semibold text-gray-500 uppercase">
                <span>Seq</span><span>ICD-10</span><span>Description</span><span>Status</span><span>Actions</span>
              </div>
              {[...diagnoses].sort((a, b) => a.sequence - b.sequence).map((dx) => (
                <div key={dx.dxId} className={`grid grid-cols-[3rem_7rem_1fr_6rem_5rem] gap-2 px-3 py-2.5 border-t text-sm items-start ${dx.status === 'Inactive' ? 'opacity-40' : ''}`}>
                  <span className={`font-bold text-center text-sm ${dx.sequence === 1 ? 'text-purple-700' : 'text-gray-600'}`}>
                    {dx.sequence}
                    {dx.sequence === 1 ? ' ★' : ''}
                  </span>
                  {editingDxId === dx.dxId ? (
                    <input value={editDxForm.icd10Code} onChange={(e) => setEditDxForm((f) => ({ ...f, icd10Code: e.target.value }))} className="border border-gray-300 rounded px-2 py-1 text-xs w-full" />
                  ) : (
                    <span className="font-mono font-medium">{dx.icd10Code}</span>
                  )}
                  {editingDxId === dx.dxId ? (
                    <input value={editDxForm.description} onChange={(e) => setEditDxForm((f) => ({ ...f, description: e.target.value }))} className="border border-gray-300 rounded px-2 py-1 text-xs w-full" />
                  ) : (
                    <span className="text-gray-700">{dx.description}</span>
                  )}
                  <Badge status={dx.status} />
                  <div className="flex gap-1">
                    {dx.status === 'Active' && editingDxId !== dx.dxId && (
                      <>
                        <button
                          onClick={() => {
                            setEditingDxId(dx.dxId)
                            setEditDxForm({ icd10Code: dx.icd10Code, description: dx.description, sequence: String(dx.sequence) })
                          }}
                          className="text-blue-500 hover:text-blue-700 text-xs underline"
                          type="button"
                        >
                          Edit
                        </button>
                        <button onClick={() => handleSoftDeleteDx(dx.dxId)} className="text-red-400 hover:text-red-600 text-xs underline ml-1" type="button">
                          Remove
                        </button>
                      </>
                    )}
                    {editingDxId === dx.dxId && (
                      <>
                        <button onClick={() => handleSaveDx(dx.dxId)} className="text-green-600 text-xs underline font-medium" type="button">
                          Save
                        </button>
                        <button onClick={() => setEditingDxId(null)} className="text-gray-400 text-xs underline ml-1" type="button">
                          Cancel
                        </button>
                      </>
                    )}
                  </div>
                </div>
              ))}
              {diagnoses.length === 0 && (
                <div className="px-3 py-6 text-center text-gray-400 text-sm italic">
                  No diagnoses added yet. Add the first diagnosis below.
                </div>
              )}
            </div>
          </Card>

          {activeDxCount < 12 && lock == null && (
            <Card title="Add Diagnosis">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">ICD-10 Code *</label>
                  <input
                    {...dxForm.register('icd10Code', {
                      required: 'ICD-10 code is required',
                      pattern: { value: /^[A-Z]\d{2}(\.\d{1,4})?$/, message: 'Invalid ICD-10 format (e.g. I10 or F41.1)' },
                      validate: (v) =>
                        !activeDiagnoses.some((d) => d.icd10Code.toUpperCase() === v.toUpperCase()) ||
                        'This diagnosis code already exists on this encounter',
                    })}
                    placeholder="e.g. I10 or F41.1"
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-purple-500 uppercase"
                    onInput={(e) => {
                      e.currentTarget.value = e.currentTarget.value.toUpperCase()
                    }}
                  />
                  {dxForm.formState.errors.icd10Code && <p className="text-red-500 text-xs mt-1">{dxForm.formState.errors.icd10Code.message}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Sequence * <span className="text-xs text-gray-400 font-normal">(1 = Principal)</span>
                  </label>
                  <input
                    type="number"
                    min="1"
                    max="12"
                    {...dxForm.register('sequence', {
                      required: 'Sequence is required',
                      min: { value: 1, message: 'Sequence must be between 1 and 12' },
                      max: { value: 12, message: 'Sequence must be between 1 and 12' },
                      validate: (v) =>
                        !activeDiagnoses.some((d) => d.sequence === Number(v)) ||
                        `Sequence ${v} is already used by another active diagnosis`,
                    })}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-purple-500"
                  />
                  {dxForm.formState.errors.sequence && <p className="text-red-500 text-xs mt-1">{dxForm.formState.errors.sequence.message}</p>}
                </div>
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Description *</label>
                  <input
                    {...dxForm.register('description', { required: 'Description is required' })}
                    placeholder="e.g. Essential hypertension"
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-purple-500"
                  />
                  {dxForm.formState.errors.description && <p className="text-red-500 text-xs mt-1">{dxForm.formState.errors.description.message}</p>}
                </div>
              </div>
              <div className="flex justify-end mt-3">
                <Button variant="primary" size="sm" onClick={onAddDiagnosis}>
                  Add Diagnosis
                </Button>
              </div>
            </Card>
          )}
        </div>
      </div>

      <div className="fixed bottom-0 left-64 right-0 bg-white border-t border-gray-200 px-6 py-3 z-20 flex items-center justify-between">
        {validation ? (
          <div className="flex-1 mr-6">
            {validation.errors.map((err, i) => (
              <p key={`e-${i}`} className="text-red-600 text-xs flex items-center gap-1">
                <span>✗</span> {err}
              </p>
            ))}
            {validation.warnings.map((w, i) => (
              <p key={`w-${i}`} className="text-amber-600 text-xs flex items-center gap-1">
                <span>⚠</span> {w}
              </p>
            ))}
            {validation.canLock && (
              <p className="text-green-600 text-xs font-medium flex items-center gap-1">
                <span>✓</span> All checks passed — ready to lock.
              </p>
            )}
          </div>
        ) : (
          <div />
        )}

        {lock == null ? (
          <div className="flex gap-3 items-center">
            <Button variant="secondary" size="sm" onClick={() => setValidation(runValidation())}>
              Validate
            </Button>
            <Button
              variant="primary"
              size="sm"
              disabled={!validation?.canLock}
              className={!validation?.canLock ? 'opacity-50 cursor-not-allowed' : ''}
              onClick={() => setShowLockDialog(true)}
            >
              Lock Encounter
            </Button>
          </div>
        ) : (
          <div className="flex items-center gap-4">
            <div className="text-sm">
              <span className="text-gray-500">Locked by </span>
              <span className="font-medium text-gray-800">{lock.coderName}</span>
              <span className="text-gray-400 ml-2 text-xs">on {new Date(lock.lockedDate).toLocaleDateString()}</span>
            </div>
            <Badge status="Locked" />
            <Button variant="danger" size="sm" onClick={() => setShowUnlockDialog(true)}>
              Unlock
            </Button>
          </div>
        )}
      </div>

      <Dialog isOpen={showLockDialog} onClose={() => setShowLockDialog(false)} title="Lock Encounter for Claim Build" maxWidth="md">
        <p className="text-sm text-gray-600 mb-3">
          Locking this encounter will finalize the coding and trigger claim build. This action should only be taken when all diagnoses are complete and validated.
        </p>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Notes (optional)</label>
          <textarea
            value={lockNotes}
            onChange={(e) => setLockNotes(e.target.value)}
            rows={3}
            placeholder="Any coding notes..."
            className="w-full border border-gray-300 rounded-lg p-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div className="flex justify-end gap-2 mt-4">
          <Button
            variant="primary"
            onClick={() => {
              const newLock = {
                codingLockId: Math.floor(Math.random() * 9000) + 4100,
                coderName: 'Current Coder',
                lockedDate: new Date().toISOString(),
                status: 'Locked',
              }
              setLock(newLock)
              setEncounter((prev) => ({ ...prev, status: 'Finalized' }))
              setShowLockDialog(false)
              setLockNotes('')
              setValidation(null)
            }}
          >
            Confirm Lock
          </Button>
          <Button variant="secondary" onClick={() => setShowLockDialog(false)}>
            Cancel
          </Button>
        </div>
      </Dialog>

      <Dialog isOpen={showUnlockDialog} onClose={() => setShowUnlockDialog(false)} title="Unlock Encounter" maxWidth="sm">
        <div className="bg-amber-50 border border-amber-200 rounded-lg px-3 py-2 text-amber-700 text-sm">
          Unlocking will allow diagnosis changes. If a claim has already been built, it may need to be re-scrubbed.
        </div>
        <div className="mt-3">
          <label className="block text-sm font-medium text-gray-700 mb-1">Reason *</label>
          <input
            value={unlockReason}
            onChange={(e) => setUnlockReason(e.target.value)}
            placeholder="Why is this being unlocked?"
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div className="flex justify-end gap-2 mt-4">
          <Button
            variant="danger"
            disabled={!unlockReason.trim()}
            onClick={() => {
              setLock(null)
              setEncounter((prev) => ({ ...prev, status: 'ReadyForCoding' }))
              setShowUnlockDialog(false)
              setUnlockReason('')
              setValidation(null)
            }}
          >
            Confirm Unlock
          </Button>
          <Button variant="secondary" onClick={() => setShowUnlockDialog(false)}>
            Cancel
          </Button>
        </div>
      </Dialog>
    </div>
  )
}
