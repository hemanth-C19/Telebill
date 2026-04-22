import { useCallback, useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import apiClient from '../../api/client'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import { Dialog } from '../../components/shared/ui/Dialog'
import { getAuthCookie } from '../../utils/cookieToken'

type ProviderInfo = {
  providerId: number
  name: string | null
  npi: string | null
  taxonomy: string | null
}

type AttestationInfo = {
  attestId: number
  attestText: string | null
  attestDate: string | null
  status: string | null
}

type PatientInfo = {
  patientId: number
  name: string | null
  mrn: string | null
  dob: string | null
  gender: string | null
}

type ChargeLineInfo = {
  chargeId: number
  cptHcpcs: string | null
  modifiers: string | null
  modifierList: string[]
  units: number | null
  chargeAmount: number | null
  notes: string | null
  status: string | null
  modifiersValid: boolean
}

type PayerPlanInfo = {
  planId: number
  planName: string | null
  networkType: string | null
  requiredModifiers: string[]
  acceptedModifiers: string[]
  memberID: string | null
}

type LockInfo = {
  codingLockId: number
  coderName: string | null
  lockedDate: string
  status: string | null
}

type EncounterCard = {
  encounterId: number
  status: string
  encounterDateTime: string
  visitType: string | null
  pos: string | null
  documentationUri: string | null
  provider: ProviderInfo | null
  attestation: AttestationInfo | null
  patient: PatientInfo | null
  chargeLines: ChargeLineInfo[]
  primaryPayerPlan: PayerPlanInfo | null
  coverageWarning: boolean
  diagnoses: DiagnosisItem[]
  activeLock: LockInfo | null
}

type DiagnosisItem = {
  dxId: number
  encounterId: number
  icd10Code: string | null
  description: string | null
  sequence: number
  status: string | null
}

type ValidationResult = {
  encounterId: number
  canLock: boolean
  errors: string[]
  warnings: string[]
}

type DxFormValues = { icd10Code: string; description: string; sequence: string }

function formatDate(input: string | null | undefined): string {
  if (!input) return '—'
  if (/^\d{4}-\d{2}-\d{2}$/.test(input)) {
    const [y, m, d] = input.split('-').map(Number)
    return new Date(y, m - 1, d).toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })
  }
  const date = new Date(input)
  if (Number.isNaN(date.getTime())) return input
  return date.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })
}

function formatDateTime(input: string | null | undefined): string {
  if (!input) return '—'
  const date = new Date(input)
  if (Number.isNaN(date.getTime())) return input
  const d = date.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })
  const t = date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false })
  return `${d} ${t}`
}

function extractErrorMessage(err: unknown): string {
  const data = (err as { response?: { data?: { message?: string } } })?.response?.data
  return data?.message ?? 'An unexpected error occurred.'
}

export default function EncounterCodingView() {
  const { encounterId } = useParams<{ encounterId: string }>()
  const navigate = useNavigate()
  const encounterIdNum = Number(encounterId)

  const [encounter, setEncounter] = useState<EncounterCard | null>(null)
  const [diagnoses, setDiagnoses] = useState<DiagnosisItem[]>([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [editingModifiersId, setEditingModifiersId] = useState<number | null>(null)
  const [modifierInput, setModifierInput] = useState('')
  const [editingDxId, setEditingDxId] = useState<number | null>(null)
  const [editDxForm, setEditDxForm] = useState({ icd10Code: '', description: '', sequence: '' })

  const [validation, setValidation] = useState<ValidationResult | null>(null)
  const [validating, setValidating] = useState(false)

  const [showLockDialog, setShowLockDialog] = useState(false)
  const [lockNotes, setLockNotes] = useState('')
  const [locking, setLocking] = useState(false)

  const [showUnlockDialog, setShowUnlockDialog] = useState(false)
  const [unlockReason, setUnlockReason] = useState('')
  const [unlocking, setUnlocking] = useState(false)

  const [apiError, setApiError] = useState<string | null>(null)

  const dxForm = useForm<DxFormValues>({ defaultValues: { sequence: '' } })

  const userId = getAuthCookie()?.userId

  const loadData = useCallback(async () => {
    setLoading(true)
    setLoadError(null)
    try {
      const [cardRes, dxRes] = await Promise.all([
        apiClient.get<EncounterCard>(`api/v1/coding/worklist/${encounterIdNum}`),
        apiClient.get<DiagnosisItem[]>(`api/v1/coding/diagnoses/by-encounter/${encounterIdNum}`),
      ])
      setEncounter(cardRes.data)
      setDiagnoses(dxRes.data)
    } catch {
      setLoadError('Failed to load encounter data.')
    } finally {
      setLoading(false)
    }
  }, [encounterIdNum])

  useEffect(() => {
    loadData()
  }, [loadData])

  const activeDiagnoses = useMemo(() => diagnoses.filter((d) => d.status === 'Active'), [diagnoses])
  const activeDxCount = activeDiagnoses.length

  const handleValidate = async () => {
    setValidating(true)
    setApiError(null)
    try {
      const res = await apiClient.get<ValidationResult>(`api/v1/coding/lock/validate/${encounterIdNum}`)
      setValidation(res.data)
    } catch {
      setApiError('Validation request failed.')
    } finally {
      setValidating(false)
    }
  }

  const handleSaveModifiers = async (chargeId: number) => {
    const modList = modifierInput
      .split(',')
      .map((m) => m.trim().toUpperCase())
      .filter(Boolean)
    setEditingModifiersId(null)
    setApiError(null)
    try {
      const res = await apiClient.patch<ChargeLineInfo>(
        `api/v1/coding/worklist/${encounterIdNum}/charge-lines/${chargeId}/modifiers?userId=${userId}`,
        { modifiers: modList },
      )
      setEncounter((prev) =>
        prev == null
          ? null
          : { ...prev, chargeLines: prev.chargeLines.map((cl) => (cl.chargeId === chargeId ? res.data : cl)) },
      )
      setValidation(null)
    } catch (err) {
      setApiError(extractErrorMessage(err))
    }
  }

  const handleSaveDx = async (dxId: number) => {
    setApiError(null)
    try {
      const res = await apiClient.patch<DiagnosisItem>(
        `api/v1/coding/diagnoses/${dxId}?userId=${userId}`,
        {
          icd10Code: editDxForm.icd10Code.toUpperCase(),
          description: editDxForm.description,
          sequence: Number(editDxForm.sequence),
        },
      )
      setDiagnoses((prev) => prev.map((d) => (d.dxId === dxId ? res.data : d)))
      setEditingDxId(null)
      setValidation(null)
    } catch (err) {
      setApiError(extractErrorMessage(err))
    }
  }

  const handleRemoveDx = async (dxId: number) => {
    setApiError(null)
    try {
      await apiClient.delete(`api/v1/coding/diagnoses/${dxId}?userId=${userId}`)
      setDiagnoses((prev) => prev.map((d) => (d.dxId === dxId ? { ...d, status: 'Inactive' } : d)))
      setValidation(null)
    } catch (err) {
      setApiError(extractErrorMessage(err))
    }
  }

  const onAddDiagnosis = dxForm.handleSubmit(async (data) => {
    setApiError(null)
    try {
      const res = await apiClient.post<DiagnosisItem>(
        `api/v1/coding/diagnoses?userId=${userId}`,
        {
          encounterId: encounterIdNum,
          icd10Code: data.icd10Code.toUpperCase(),
          description: data.description,
          sequence: Number(data.sequence),
        },
      )
      setDiagnoses((prev) => [...prev, res.data])
      dxForm.reset()
      setValidation(null)
    } catch (err) {
      setApiError(extractErrorMessage(err))
    }
  })

  const handleLockConfirm = async () => {
    setLocking(true)
    setApiError(null)
    try {
      const res = await apiClient.post<{
        codingLock: LockInfo | null
        encounterStatus: string
        claimBuildTriggered: boolean
        validationErrors: string[]
      }>(`api/v1/coding/lock/apply?userId=${userId}`, {
        encounterId: encounterIdNum,
        notes: lockNotes || null,
      })
      if (res.data.validationErrors.length > 0) {
        setApiError(res.data.validationErrors.join(' | '))
        return
      }
      setEncounter((prev) =>
        prev == null ? null : { ...prev, status: res.data.encounterStatus, activeLock: res.data.codingLock },
      )
      setShowLockDialog(false)
      setLockNotes('')
      setValidation(null)
    } catch (err) {
      setApiError(extractErrorMessage(err))
    } finally {
      setLocking(false)
    }
  }

  const handleUnlockConfirm = async () => {
    setUnlocking(true)
    setApiError(null)
    try {
      const res = await apiClient.post<{ encounterId: number; encounterStatus: string; previousLockId: number }>(
        `api/v1/coding/lock/unlock?userId=${userId}`,
        { encounterId: encounterIdNum, reason: unlockReason },
      )
      setEncounter((prev) =>
        prev == null ? null : { ...prev, status: res.data.encounterStatus, activeLock: null },
      )
      setShowUnlockDialog(false)
      setUnlockReason('')
      setValidation(null)
    } catch (err) {
      setApiError(extractErrorMessage(err))
    } finally {
      setUnlocking(false)
    }
  }

  if (loading) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center">
        <p className="text-gray-400 text-sm">Loading encounter...</p>
      </div>
    )
  }

  if (loadError != null || encounter == null) {
    return (
      <div className="min-h-[60vh] flex flex-col items-center justify-center gap-3">
        <p className="text-red-500">{loadError ?? 'Encounter not found.'}</p>
        <button type="button" className="text-blue-600 hover:underline" onClick={() => navigate('/coding/worklist')}>
          ← Back to Worklist
        </button>
      </div>
    )
  }

  const lock = encounter.activeLock

  return (
    <div className="pb-24">
      <button type="button" className="text-blue-600 hover:underline" onClick={() => navigate('/coding/worklist')}>
        ← Back to Worklist
      </button>

      <div className="flex items-center gap-3 mt-2">
        <h1 className="text-2xl font-bold text-gray-900">Encounter #{encounterIdNum} — Coding View</h1>
        <Badge status={encounter.status} />
      </div>

      {apiError != null && (
        <div className="mt-3 rounded-md bg-red-50 border border-red-200 px-4 py-2 text-sm text-red-700">
          {apiError}
        </div>
      )}

      <div className="flex gap-5 mt-4">
        <div className="w-[42%] flex-shrink-0 space-y-4">
          <Card title="Patient & Encounter Info">
            <div className="grid grid-cols-2 gap-3 text-sm">
              <p><span className="text-gray-500">Patient:</span> <strong>{encounter.patient?.name ?? '—'}</strong></p>
              <p><span className="text-gray-500">MRN:</span> <span className="font-mono">{encounter.patient?.mrn ?? '—'}</span></p>
              <p><span className="text-gray-500">DOB:</span> {formatDate(encounter.patient?.dob)}</p>
              <p><span className="text-gray-500">Gender:</span> {encounter.patient?.gender ?? '—'}</p>
              <p><span className="text-gray-500">Provider:</span> {encounter.provider?.name ?? '—'}</p>
              <p><span className="text-gray-500">NPI:</span> {encounter.provider?.npi ?? '—'}</p>
              <p><span className="text-gray-500">Specialty:</span> {encounter.provider?.taxonomy ?? '—'}</p>
              <p><span className="text-gray-500">Encounter Date:</span> {formatDateTime(encounter.encounterDateTime)}</p>
              <p className="col-span-2">
                <span className="text-gray-500">POS:</span>{' '}
                {encounter.pos === '02' ? '02 – Telehealth Home' : encounter.pos === '10' ? '10 – Telehealth Non-Home' : encounter.pos ?? '—'}
              </p>
            </div>
          </Card>

          <Card
            title="Attestation"
            className={encounter.attestation != null ? 'border-l-4 border-l-green-500' : 'border-l-4 border-l-amber-500'}
          >
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
            {encounter.primaryPayerPlan != null ? (
              <div className="space-y-2 text-sm">
                <p><span className="text-gray-500">Plan Name:</span> {encounter.primaryPayerPlan.planName ?? '—'}</p>
                <p><span className="text-gray-500">Network Type:</span> {encounter.primaryPayerPlan.networkType ?? '—'}</p>
                <p><span className="text-gray-500">Member ID:</span> {encounter.primaryPayerPlan.memberID ?? '—'}</p>
                <div>
                  <p className="text-gray-500 mb-1">Required Modifiers:</p>
                  <div className="flex flex-wrap gap-1">
                    {encounter.primaryPayerPlan.requiredModifiers.length > 0 ? (
                      encounter.primaryPayerPlan.requiredModifiers.map((mod) => (
                        <span key={mod} className="bg-blue-100 text-blue-700 text-xs px-2 py-0.5 rounded-full">{mod}</span>
                      ))
                    ) : (
                      <span className="text-gray-400 text-xs italic">None</span>
                    )}
                  </div>
                </div>
                <div>
                  <p className="text-gray-500 mb-1">Accepted Modifiers:</p>
                  <div className="flex flex-wrap gap-1">
                    {encounter.primaryPayerPlan.acceptedModifiers.length > 0 ? (
                      encounter.primaryPayerPlan.acceptedModifiers.map((mod) => (
                        <span key={mod} className="bg-gray-100 text-gray-700 text-xs px-2 py-0.5 rounded-full">{mod}</span>
                      ))
                    ) : (
                      <span className="text-gray-400 text-xs italic">None</span>
                    )}
                  </div>
                </div>
                {encounter.coverageWarning && (
                  <div className="bg-amber-50 border border-amber-200 rounded-lg px-3 py-2 text-amber-700 text-xs mt-2">
                    ⚠ No active coverage found within the encounter date window. Claim may fail scrub.
                  </div>
                )}
              </div>
            ) : (
              <div className="bg-amber-50 border border-amber-200 rounded-lg px-3 py-2 text-amber-700 text-xs">
                ⚠ No payer plan found for this patient.
              </div>
            )}
          </Card>

          <Card title="Charge Lines">
            <div className="border rounded-lg overflow-hidden">
              <div className="grid grid-cols-6 gap-2 px-3 py-2 text-xs font-semibold text-gray-500 bg-gray-50 uppercase">
                <span>Line</span><span>CPT</span><span>Modifiers</span><span>Units</span><span>Amount</span><span>Valid</span>
              </div>
              {encounter.chargeLines.length === 0 && (
                <div className="px-3 py-6 text-center text-gray-400 text-sm italic">No charge lines.</div>
              )}
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
                        {cl.modifierList.length > 0
                          ? cl.modifierList.join(',')
                          : <span className="text-gray-300 italic">none</span>}
                      </span>
                    )}
                  </span>
                  <span className="text-center text-gray-600">{cl.units ?? '—'}</span>
                  <span>{cl.chargeAmount != null ? `$${cl.chargeAmount.toFixed(2)}` : '—'}</span>
                  <span className="flex items-center gap-1">
                    {cl.modifiersValid ? (
                      <span className="text-green-600 text-xs font-medium">✓</span>
                    ) : (
                      <span className="text-red-500 text-xs font-medium" title="Required modifiers missing for this payer plan">✗</span>
                    )}
                    {lock == null && (
                      <button
                        type="button"
                        onClick={() => {
                          setEditingModifiersId(cl.chargeId)
                          setModifierInput(cl.modifierList.join(','))
                        }}
                        className="text-gray-400 hover:text-blue-600 ml-1 text-xs underline"
                      >
                        edit
                      </button>
                    )}
                  </span>
                </div>
              ))}
            </div>
            {encounter.primaryPayerPlan != null && encounter.primaryPayerPlan.requiredModifiers.length > 0 && (
              <p className="text-xs text-gray-400 mt-1">
                Required modifiers for {encounter.primaryPayerPlan.planName}:{' '}
                {encounter.primaryPayerPlan.requiredModifiers.join(', ')}
              </p>
            )}
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
                <div
                  key={dx.dxId}
                  className={`grid grid-cols-[3rem_7rem_1fr_6rem_5rem] gap-2 px-3 py-2.5 border-t text-sm items-start ${dx.status === 'Inactive' ? 'opacity-40' : ''}`}
                >
                  <span className={`font-bold text-center text-sm ${dx.sequence === 1 ? 'text-purple-700' : 'text-gray-600'}`}>
                    {dx.sequence}{dx.sequence === 1 ? ' ★' : ''}
                  </span>
                  {editingDxId === dx.dxId ? (
                    <input
                      value={editDxForm.icd10Code}
                      onChange={(e) => setEditDxForm((f) => ({ ...f, icd10Code: e.target.value }))}
                      className="border border-gray-300 rounded px-2 py-1 text-xs w-full"
                    />
                  ) : (
                    <span className="font-mono font-medium">{dx.icd10Code ?? '—'}</span>
                  )}
                  {editingDxId === dx.dxId ? (
                    <input
                      value={editDxForm.description}
                      onChange={(e) => setEditDxForm((f) => ({ ...f, description: e.target.value }))}
                      className="border border-gray-300 rounded px-2 py-1 text-xs w-full"
                    />
                  ) : (
                    <span className="text-gray-700">{dx.description ?? '—'}</span>
                  )}
                  <Badge status={dx.status ?? ''} />
                  <div className="flex gap-1">
                    {dx.status === 'Active' && editingDxId !== dx.dxId && lock == null && (
                      <>
                        <button
                          type="button"
                          onClick={() => {
                            setEditingDxId(dx.dxId)
                            setEditDxForm({
                              icd10Code: dx.icd10Code ?? '',
                              description: dx.description ?? '',
                              sequence: String(dx.sequence),
                            })
                          }}
                          className="text-blue-500 hover:text-blue-700 text-xs underline"
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          onClick={() => handleRemoveDx(dx.dxId)}
                          className="text-red-400 hover:text-red-600 text-xs underline ml-1"
                        >
                          Remove
                        </button>
                      </>
                    )}
                    {editingDxId === dx.dxId && (
                      <>
                        <button
                          type="button"
                          onClick={() => handleSaveDx(dx.dxId)}
                          className="text-green-600 text-xs underline font-medium"
                        >
                          Save
                        </button>
                        <button
                          type="button"
                          onClick={() => setEditingDxId(null)}
                          className="text-gray-400 text-xs underline ml-1"
                        >
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
                        !activeDiagnoses.some((d) => d.icd10Code?.toUpperCase() === v.toUpperCase()) ||
                        'This diagnosis code already exists on this encounter',
                    })}
                    placeholder="e.g. I10 or F41.1"
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-purple-500 uppercase"
                    onInput={(e) => {
                      e.currentTarget.value = e.currentTarget.value.toUpperCase()
                    }}
                  />
                  {dxForm.formState.errors.icd10Code && (
                    <p className="text-red-500 text-xs mt-1">{dxForm.formState.errors.icd10Code.message}</p>
                  )}
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
                  {dxForm.formState.errors.sequence && (
                    <p className="text-red-500 text-xs mt-1">{dxForm.formState.errors.sequence.message}</p>
                  )}
                </div>
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Description *</label>
                  <input
                    {...dxForm.register('description', { required: 'Description is required' })}
                    placeholder="e.g. Essential hypertension"
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-purple-500"
                  />
                  {dxForm.formState.errors.description && (
                    <p className="text-red-500 text-xs mt-1">{dxForm.formState.errors.description.message}</p>
                  )}
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
        {validation != null ? (
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
            <Button variant="secondary" size="sm" onClick={handleValidate} disabled={validating}>
              {validating ? 'Validating...' : 'Validate'}
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
              <span className="font-medium text-gray-800">{lock.coderName ?? 'Coder'}</span>
              <span className="text-gray-400 ml-2 text-xs">on {formatDate(lock.lockedDate)}</span>
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
          Locking this encounter will finalize the coding and trigger claim build. This action should only be taken when all
          diagnoses are complete and validated.
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
          <Button variant="primary" onClick={handleLockConfirm} disabled={locking}>
            {locking ? 'Locking...' : 'Confirm Lock'}
          </Button>
          <Button variant="secondary" onClick={() => setShowLockDialog(false)} disabled={locking}>
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
          <Button variant="danger" disabled={!unlockReason.trim() || unlocking} onClick={handleUnlockConfirm}>
            {unlocking ? 'Unlocking...' : 'Confirm Unlock'}
          </Button>
          <Button variant="secondary" onClick={() => setShowUnlockDialog(false)} disabled={unlocking}>
            Cancel
          </Button>
        </div>
      </Dialog>
    </div>
  )
}
