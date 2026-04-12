// FrontDesk encounters — list/detail views, charge lines, attestation (dummy data; see Telebill-Backend Controllers/Encounter)

import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { ActionCard } from '../../components/shared/ui/ActionCard'
import Badge from '../../components/shared/ui/Badge'
import Button from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import Dialog from '../../components/shared/ui/Dialog'
import Input from '../../components/shared/ui/Input'
import { Pagination } from '../../components/shared/ui/Pagination'
import Table from '../../components/shared/ui/Table'

type EncounterStatus = 'Open' | 'ReadyForCoding' | 'Finalized'
type ChargeStatus = 'Draft' | 'Finalized'

type Encounter = {
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

type ChargeLine = {
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

type Attestation = {
  attestId: number
  encounterId: number
  attestedBy: string
  attestedDate: string
  status: 'Attested' | 'NotAttested'
}

// Backend ref: GET /api/v1/Encounter/Encounter/GetAllEncounters — AddEncounterDTO: PatientId, ProviderId, EncounterDate, POS, Notes
const DUMMY_ENCOUNTERS: Encounter[] = [
  {
    encounterId: 1001,
    patientId: 1,
    patientName: 'Alice Johnson',
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-11-05T09:30',
    pos: '02',
    notes: 'Follow-up visit',
    status: 'Open',
    chargeLineCount: 2,
  },
  {
    encounterId: 1002,
    patientId: 2,
    patientName: 'Bob Martinez',
    providerId: 202,
    providerName: 'Dr. James Patel',
    encounterDate: '2024-11-08T14:00',
    pos: '10',
    notes: 'Initial consult',
    status: 'ReadyForCoding',
    chargeLineCount: 3,
  },
  {
    encounterId: 1003,
    patientId: 4,
    patientName: 'David Patel',
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-11-10T11:15',
    pos: '02',
    notes: 'Chronic care mgmt',
    status: 'Finalized',
    chargeLineCount: 1,
  },
  {
    encounterId: 1004,
    patientId: 5,
    patientName: 'Emily Rodriguez',
    providerId: 203,
    providerName: 'Dr. Mark Liu',
    encounterDate: '2024-11-12T08:45',
    pos: '10',
    notes: '',
    status: 'Open',
    chargeLineCount: 0,
  },
  {
    encounterId: 1005,
    patientId: 7,
    patientName: 'Grace Kim',
    providerId: 202,
    providerName: 'Dr. James Patel',
    encounterDate: '2024-11-14T13:00',
    pos: '02',
    notes: 'Mental health check',
    status: 'Open',
    chargeLineCount: 2,
  },
  {
    encounterId: 1006,
    patientId: 1,
    patientName: 'Alice Johnson',
    providerId: 203,
    providerName: 'Dr. Mark Liu',
    encounterDate: '2024-11-15T10:00',
    pos: '10',
    notes: 'Medication review',
    status: 'ReadyForCoding',
    chargeLineCount: 4,
  },
  {
    encounterId: 1007,
    patientId: 2,
    patientName: 'Bob Martinez',
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-11-18T15:30',
    pos: '02',
    notes: 'Post-op follow-up',
    status: 'Finalized',
    chargeLineCount: 3,
  },
]

// Backend ref: GET /api/v1/Encounter/ChargeLine/ByEncounter/{encounterId}
const DUMMY_CHARGE_LINES: Record<number, ChargeLine[]> = {
  1001: [
    {
      chargeId: 5001,
      encounterId: 1001,
      lineNo: 1,
      cptCode: '99213',
      modifiers: '',
      units: 1,
      chargeAmount: 150.0,
      dxPointers: '1',
      status: 'Draft',
    },
    {
      chargeId: 5002,
      encounterId: 1001,
      lineNo: 2,
      cptCode: '93000',
      modifiers: '',
      units: 1,
      chargeAmount: 85.0,
      dxPointers: '1,2',
      status: 'Finalized',
    },
  ],
  1002: [
    {
      chargeId: 5003,
      encounterId: 1002,
      lineNo: 1,
      cptCode: '99214',
      modifiers: '25',
      units: 1,
      chargeAmount: 200.0,
      dxPointers: '1',
      status: 'Finalized',
    },
    {
      chargeId: 5004,
      encounterId: 1002,
      lineNo: 2,
      cptCode: '90837',
      modifiers: '',
      units: 1,
      chargeAmount: 175.0,
      dxPointers: '2',
      status: 'Finalized',
    },
    {
      chargeId: 5005,
      encounterId: 1002,
      lineNo: 3,
      cptCode: '96130',
      modifiers: '',
      units: 2,
      chargeAmount: 220.0,
      dxPointers: '1,3',
      status: 'Finalized',
    },
  ],
  1003: [
    {
      chargeId: 5006,
      encounterId: 1003,
      lineNo: 1,
      cptCode: '99215',
      modifiers: 'GQ',
      units: 1,
      chargeAmount: 250.0,
      dxPointers: '1',
      status: 'Finalized',
    },
  ],
  1004: [],
  1005: [
    {
      chargeId: 5007,
      encounterId: 1005,
      lineNo: 1,
      cptCode: '99213',
      modifiers: '',
      units: 1,
      chargeAmount: 150.0,
      dxPointers: '1',
      status: 'Draft',
    },
    {
      chargeId: 5008,
      encounterId: 1005,
      lineNo: 2,
      cptCode: '96130',
      modifiers: 'GT',
      units: 1,
      chargeAmount: 180.0,
      dxPointers: '2',
      status: 'Draft',
    },
  ],
  1006: [
    {
      chargeId: 5009,
      encounterId: 1006,
      lineNo: 1,
      cptCode: '99214',
      modifiers: '',
      units: 1,
      chargeAmount: 200.0,
      dxPointers: '1',
      status: 'Finalized',
    },
    {
      chargeId: 5010,
      encounterId: 1006,
      lineNo: 2,
      cptCode: '90837',
      modifiers: 'GT',
      units: 1,
      chargeAmount: 175.0,
      dxPointers: '1',
      status: 'Finalized',
    },
    {
      chargeId: 5011,
      encounterId: 1006,
      lineNo: 3,
      cptCode: 'G0296',
      modifiers: '',
      units: 1,
      chargeAmount: 95.0,
      dxPointers: '2',
      status: 'Finalized',
    },
    {
      chargeId: 5012,
      encounterId: 1006,
      lineNo: 4,
      cptCode: '93000',
      modifiers: '',
      units: 1,
      chargeAmount: 85.0,
      dxPointers: '1',
      status: 'Draft',
    },
  ],
  1007: [
    {
      chargeId: 5013,
      encounterId: 1007,
      lineNo: 1,
      cptCode: '99213',
      modifiers: 'GQ,GT',
      units: 1,
      chargeAmount: 150.0,
      dxPointers: '1',
      status: 'Finalized',
    },
    {
      chargeId: 5014,
      encounterId: 1007,
      lineNo: 2,
      cptCode: '93000',
      modifiers: '',
      units: 1,
      chargeAmount: 85.0,
      dxPointers: '1,2',
      status: 'Finalized',
    },
    {
      chargeId: 5015,
      encounterId: 1007,
      lineNo: 3,
      cptCode: '90837',
      modifiers: 'GT',
      units: 2,
      chargeAmount: 350.0,
      dxPointers: '2',
      status: 'Finalized',
    },
  ],
}

// Backend ref: GET /api/v1/Encounter/Attestation/get-by-encounterId — route binding bug noted in script; dummy only.
const DUMMY_ATTESTATIONS: Record<number, Attestation> = {
  1001: {
    attestId: 9001,
    encounterId: 1001,
    attestedBy: 'Dr. Sarah Chen',
    attestedDate: '',
    status: 'NotAttested',
  },
  1002: {
    attestId: 9002,
    encounterId: 1002,
    attestedBy: 'Dr. James Patel',
    attestedDate: '2024-11-08',
    status: 'Attested',
  },
  1003: {
    attestId: 9003,
    encounterId: 1003,
    attestedBy: 'Dr. Sarah Chen',
    attestedDate: '2024-11-10',
    status: 'Attested',
  },
  1004: {
    attestId: 9004,
    encounterId: 1004,
    attestedBy: 'Dr. Mark Liu',
    attestedDate: '',
    status: 'NotAttested',
  },
  1005: {
    attestId: 9005,
    encounterId: 1005,
    attestedBy: 'Dr. James Patel',
    attestedDate: '',
    status: 'NotAttested',
  },
  1006: {
    attestId: 9006,
    encounterId: 1006,
    attestedBy: 'Dr. Mark Liu',
    attestedDate: '2024-11-15',
    status: 'Attested',
  },
  1007: {
    attestId: 9007,
    encounterId: 1007,
    attestedBy: 'Dr. Sarah Chen',
    attestedDate: '2024-11-18',
    status: 'Attested',
  },
}

const DUMMY_PATIENTS_DROPDOWN = [
  { patientId: 1, name: 'Alice Johnson', mrn: 'PT-A1B2C3D4' },
  { patientId: 2, name: 'Bob Martinez', mrn: 'PT-E5F6G7H8' },
  { patientId: 3, name: 'Carol Nguyen', mrn: 'PT-I9J0K1L2' },
  { patientId: 4, name: 'David Patel', mrn: 'PT-M3N4O5P6' },
  { patientId: 5, name: 'Emily Rodriguez', mrn: 'PT-Q7R8S9T0' },
  { patientId: 6, name: 'Frank Williams', mrn: 'PT-U1V2W3X4' },
  { patientId: 7, name: 'Grace Kim', mrn: 'PT-Y5Z6A7B8' },
]

const DUMMY_PROVIDERS_DROPDOWN = [
  { providerId: 201, name: 'Dr. Sarah Chen', specialty: 'Internal Medicine' },
  { providerId: 202, name: 'Dr. James Patel', specialty: 'Psychiatry' },
  { providerId: 203, name: 'Dr. Mark Liu', specialty: 'Cardiology' },
]

const POS_OPTIONS = [
  { value: '02', label: '02 — Telehealth (Patient Home)' },
  { value: '10', label: '10 — Telehealth (Non-Patient Home)' },
]

const PAGE_SIZE = 5

const selectClassName =
  'w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

const textareaClassName =
  'w-full rounded-lg border border-gray-300 p-2 text-sm text-gray-900 shadow-sm placeholder:text-gray-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

function cloneChargeLines(): Record<number, ChargeLine[]> {
  const out: Record<number, ChargeLine[]> = {}
  for (const k of Object.keys(DUMMY_CHARGE_LINES)) {
    const id = Number(k)
    out[id] = DUMMY_CHARGE_LINES[id].map((c) => ({ ...c }))
  }
  return out
}

function formatEncounterDate(iso: string): string {
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return iso
  return d.toLocaleString('en-US', {
    month: 'short',
    day: '2-digit',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  })
}

function posDescription(code: string): string {
  return POS_OPTIONS.find((o) => o.value === code)?.label ?? code
}

function nextEncounterId(list: Encounter[]): number {
  return list.reduce((m, e) => Math.max(m, e.encounterId), 0) + 1
}

function nextChargeId(map: Record<number, ChargeLine[]>): number {
  let max = 0
  for (const lines of Object.values(map)) {
    for (const c of lines) {
      max = Math.max(max, c.chargeId)
    }
  }
  return max + 1
}

type CreateEncounterFormValues = {
  patientId: string
  providerId: string
  encounterDate: string
  pos: string
  notes: string
}

type EditEncounterFormValues = {
  encounterDate: string
  pos: string
  notes: string
}

type AddChargeFormValues = {
  cptCode: string
  modifiers: string
  units: string
  chargeAmount: string
  dxPointers: string
}

type EditChargeFormValues = AddChargeFormValues

export default function Encounters() {
  const [encounters, setEncounters] = useState<Encounter[]>(() => [...DUMMY_ENCOUNTERS])
  const [chargeLines, setChargeLines] = useState<Record<number, ChargeLine[]>>(cloneChargeLines)
  const [attestations] = useState<Record<number, Attestation>>(() => ({ ...DUMMY_ATTESTATIONS }))

  const [selectedEncounter, setSelectedEncounter] = useState<Encounter | null>(null)

  const [statusFilter, setStatusFilter] = useState('All')
  const [providerFilter, setProviderFilter] = useState('All')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [currentPage, setCurrentPage] = useState(1)

  const [showCreateDialog, setShowCreateDialog] = useState(false)
  const [patientSearch, setPatientSearch] = useState('')

  const [showEditDialog, setShowEditDialog] = useState(false)

  const [showAddChargeForm, setShowAddChargeForm] = useState(false)
  const [editingCharge, setEditingCharge] = useState<ChargeLine | null>(null)

  const createForm = useForm<CreateEncounterFormValues>({
    defaultValues: {
      patientId: '',
      providerId: '',
      encounterDate: '',
      pos: '02',
      notes: '',
    },
  })
  const { reset: resetCreateForm } = createForm

  const editEncounterForm = useForm<EditEncounterFormValues>({
    defaultValues: { encounterDate: '', pos: '02', notes: '' },
  })
  const { reset: resetEditEncounterForm } = editEncounterForm

  const addChargeForm = useForm<AddChargeFormValues>({
    defaultValues: {
      cptCode: '',
      modifiers: '',
      units: '1',
      chargeAmount: '',
      dxPointers: '',
    },
  })
  const { reset: resetAddChargeForm } = addChargeForm

  const editChargeForm = useForm<EditChargeFormValues>({
    defaultValues: {
      cptCode: '',
      modifiers: '',
      units: '1',
      chargeAmount: '',
      dxPointers: '',
    },
  })
  const { reset: resetEditChargeForm } = editChargeForm

  useEffect(() => {
    if (showCreateDialog) {
      resetCreateForm({
        patientId: '',
        providerId: '',
        encounterDate: '',
        pos: '02',
        notes: '',
      })
      setPatientSearch('')
    }
  }, [showCreateDialog, resetCreateForm])

  useEffect(() => {
    if (showEditDialog && selectedEncounter != null) {
      resetEditEncounterForm({
        encounterDate: selectedEncounter.encounterDate,
        pos: selectedEncounter.pos,
        notes: selectedEncounter.notes,
      })
    }
  }, [showEditDialog, selectedEncounter, resetEditEncounterForm])

  useEffect(() => {
    if (editingCharge != null) {
      resetEditChargeForm({
        cptCode: editingCharge.cptCode,
        modifiers: editingCharge.modifiers,
        units: String(editingCharge.units),
        chargeAmount: String(editingCharge.chargeAmount),
        dxPointers: editingCharge.dxPointers,
      })
    }
  }, [editingCharge, resetEditChargeForm])

  const filteredPatientsDropdown = useMemo(() => {
    const q = patientSearch.trim().toLowerCase()
    if (q === '') return DUMMY_PATIENTS_DROPDOWN
    return DUMMY_PATIENTS_DROPDOWN.filter(
      (p) => p.name.toLowerCase().includes(q) || p.mrn.toLowerCase().includes(q),
    )
  }, [patientSearch])

  const providerNamesDistinct = useMemo(() => {
    const names = [...new Set(encounters.map((e) => e.providerName))]
    return names.sort()
  }, [encounters])

  const filtered = useMemo(
    () =>
      encounters
        .filter((e) => statusFilter === 'All' || e.status === statusFilter)
        .filter((e) => providerFilter === 'All' || e.providerName === providerFilter)
        .filter((e) => dateFrom === '' || e.encounterDate >= dateFrom)
        .filter((e) => dateTo === '' || e.encounterDate.slice(0, 10) <= dateTo),
    [encounters, statusFilter, providerFilter, dateFrom, dateTo],
  )

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))

  useEffect(() => {
    setCurrentPage((p) => Math.min(p, totalPages))
  }, [totalPages])

  const paginated = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE
    return filtered.slice(start, start + PAGE_SIZE)
  }, [filtered, currentPage])

  function clearFilters() {
    setStatusFilter('All')
    setProviderFilter('All')
    setDateFrom('')
    setDateTo('')
    setCurrentPage(1)
  }

  function onFilterChange() {
    setCurrentPage(1)
  }

  function onCreateEncounterSubmit(values: CreateEncounterFormValues) {
    const patient = DUMMY_PATIENTS_DROPDOWN.find(
      (p) => p.patientId === Number(values.patientId),
    )
    const provider = DUMMY_PROVIDERS_DROPDOWN.find(
      (p) => p.providerId === Number(values.providerId),
    )
    if (patient == null || provider == null) return

    const next: Encounter = {
      encounterId: nextEncounterId(encounters),
      patientId: patient.patientId,
      patientName: patient.name,
      providerId: provider.providerId,
      providerName: provider.name,
      encounterDate: values.encounterDate,
      pos: values.pos,
      notes: values.notes,
      status: 'Open',
      chargeLineCount: 0,
    }
    setEncounters((prev) => [next, ...prev])
    setChargeLines((prev) => ({ ...prev, [next.encounterId]: [] }))
    setShowCreateDialog(false)
  }

  function onEditEncounterSubmit(values: EditEncounterFormValues) {
    if (selectedEncounter == null) return
    const id = selectedEncounter.encounterId
    setEncounters((prev) =>
      prev.map((e) =>
        e.encounterId === id
          ? { ...e, encounterDate: values.encounterDate, pos: values.pos, notes: values.notes }
          : e,
      ),
    )
    setSelectedEncounter((prev) =>
      prev != null && prev.encounterId === id
        ? {
            ...prev,
            encounterDate: values.encounterDate,
            pos: values.pos,
            notes: values.notes,
          }
        : prev,
    )
    setShowEditDialog(false)
  }

  function deleteEncounter() {
    if (selectedEncounter == null || selectedEncounter.status !== 'Open') return
    const id = selectedEncounter.encounterId
    setEncounters((prev) => prev.filter((e) => e.encounterId !== id))
    setChargeLines((prev) => {
      const next = { ...prev }
      delete next[id]
      return next
    })
    setSelectedEncounter(null)
  }

  function onAddChargeSubmit(values: AddChargeFormValues) {
    if (selectedEncounter == null) return
    const eid = selectedEncounter.encounterId
    const lines = chargeLines[eid] ?? []
    const newLine: ChargeLine = {
      chargeId: nextChargeId(chargeLines),
      encounterId: eid,
      lineNo: lines.length + 1,
      cptCode: values.cptCode.trim().toUpperCase(),
      modifiers: values.modifiers.trim(),
      units: Number(values.units),
      chargeAmount: Number(values.chargeAmount),
      dxPointers: values.dxPointers.trim(),
      status: 'Draft',
    }
    setChargeLines((prev) => ({ ...prev, [eid]: [...(prev[eid] ?? []), newLine] }))
    setEncounters((prev) =>
      prev.map((e) =>
        e.encounterId === eid ? { ...e, chargeLineCount: e.chargeLineCount + 1 } : e,
      ),
    )
    setSelectedEncounter((prev) =>
      prev != null && prev.encounterId === eid
        ? { ...prev, chargeLineCount: prev.chargeLineCount + 1 }
        : prev,
    )
    resetAddChargeForm({
      cptCode: '',
      modifiers: '',
      units: '1',
      chargeAmount: '',
      dxPointers: '',
    })
    setShowAddChargeForm(false)
  }

  function handleFinalizeCharge(chargeId: number) {
    if (selectedEncounter == null) return
    const eid = selectedEncounter.encounterId
    setChargeLines((prev) => ({
      ...prev,
      [eid]: (prev[eid] ?? []).map((cl) =>
        cl.chargeId === chargeId ? { ...cl, status: 'Finalized' } : cl,
      ),
    }))
  }

  function handleDeleteCharge(chargeId: number) {
    if (selectedEncounter == null) return
    const eid = selectedEncounter.encounterId
    const remaining = (chargeLines[eid] ?? []).filter((cl) => cl.chargeId !== chargeId)
    const renumbered = remaining.map((cl, i) => ({ ...cl, lineNo: i + 1 }))
    setChargeLines((prev) => ({ ...prev, [eid]: renumbered }))
    setEncounters((prev) =>
      prev.map((e) =>
        e.encounterId === eid ? { ...e, chargeLineCount: Math.max(0, e.chargeLineCount - 1) } : e,
      ),
    )
    setSelectedEncounter((prev) =>
      prev != null && prev.encounterId === eid
        ? { ...prev, chargeLineCount: Math.max(0, prev.chargeLineCount - 1) }
        : prev,
    )
  }

  function onEditChargeSubmit(values: EditChargeFormValues) {
    if (selectedEncounter == null || editingCharge == null) return
    const eid = selectedEncounter.encounterId
    const cid = editingCharge.chargeId
    setChargeLines((prev) => ({
      ...prev,
      [eid]: (prev[eid] ?? []).map((cl) =>
        cl.chargeId === cid
          ? {
              ...cl,
              cptCode: values.cptCode.trim().toUpperCase(),
              modifiers: values.modifiers.trim(),
              units: Number(values.units),
              chargeAmount: Number(values.chargeAmount),
              dxPointers: values.dxPointers.trim(),
            }
          : cl,
      ),
    }))
    setEditingCharge(null)
  }

  const listColumns = [
    { key: 'encounterId', label: 'Encounter ID' },
    { key: 'patientName', label: 'Patient Name' },
    { key: 'providerName', label: 'Provider Name' },
    { key: 'encounterDateDisplay', label: 'Encounter Date' },
    { key: 'pos', label: 'POS' },
    { key: 'status', label: 'Status' },
    { key: 'chargeLineCountDisplay', label: 'Charge Lines' },
  ]

  const listTableData = paginated.map((e) => ({
    ...e,
    encounterDateDisplay: formatEncounterDate(e.encounterDate),
    status: <Badge status={e.status} />,
    chargeLineCountDisplay: `${e.chargeLineCount} lines`,
  }))

  const chargeColumns = [
    { key: 'lineNo', label: 'Line No' },
    { key: 'cptCode', label: 'CPT/HCPCS' },
    { key: 'modifiers', label: 'Modifiers' },
    { key: 'units', label: 'Units' },
    { key: 'chargeAmountDisplay', label: 'Charge Amount' },
    { key: 'dxPointers', label: 'Dx Pointers' },
    { key: 'status', label: 'Status' },
    { key: 'rowActions', label: '' },
  ]

  if (selectedEncounter != null) {
    const lines = chargeLines[selectedEncounter.encounterId] ?? []
    const hasDraft = lines.some((cl) => cl.status === 'Draft')
    const attest = attestations[selectedEncounter.encounterId]

    const chargeTableData = lines.map((cl) => ({
      ...cl,
      chargeAmountDisplay: `$${cl.chargeAmount.toFixed(2)}`,
      status: <Badge status={cl.status} />,
      rowActions:
        cl.status === 'Draft' ? (
          <ActionCard
            trigger={
              <button
                type="button"
                className="rounded p-1 text-lg font-bold leading-none text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600"
                aria-label="Charge line actions"
              >
                •••
              </button>
            }
            items={[
              {
                label: 'Edit',
                onClick: () => setEditingCharge(cl),
              },
              {
                label: 'Finalize',
                onClick: () => handleFinalizeCharge(cl.chargeId),
              },
              {
                label: 'Delete',
                onClick: () => handleDeleteCharge(cl.chargeId),
                variant: 'danger',
              },
            ]}
          />
        ) : (
          <span className="text-xs text-gray-400">Finalized — no actions</span>
        ),
    }))

    return (
      <div className="space-y-6">
        <button
          type="button"
          onClick={() => setSelectedEncounter(null)}
          className="text-sm font-medium text-blue-600 hover:text-blue-800"
        >
          ← Back to Encounters
        </button>

        <Card title="Encounter Information">
          <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Patient</dt>
              <dd className="text-sm text-gray-900">{selectedEncounter.patientName}</dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Provider</dt>
              <dd className="text-sm text-gray-900">{selectedEncounter.providerName}</dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Date</dt>
              <dd className="text-sm text-gray-900">
                {formatEncounterDate(selectedEncounter.encounterDate)}
              </dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">POS</dt>
              <dd className="text-sm text-gray-900">{posDescription(selectedEncounter.pos)}</dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-xs font-semibold uppercase text-gray-500">Notes</dt>
              <dd className="text-sm text-gray-900">
                {selectedEncounter.notes.trim() === '' ? '—' : selectedEncounter.notes}
              </dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Status</dt>
              <dd className="mt-1">
                <Badge status={selectedEncounter.status} />
              </dd>
            </div>
          </dl>

          <div className="mt-6 flex flex-wrap gap-2">
            <Button
              type="button"
              variant="secondary"
              size="sm"
              onClick={() => setShowEditDialog(true)}
            >
              Edit
            </Button>
            {selectedEncounter.status === 'Open' && (
              <Button type="button" variant="danger" size="sm" onClick={deleteEncounter}>
                Delete Encounter
              </Button>
            )}
          </div>
        </Card>

        <Card>
          <div className="mb-4 flex flex-col gap-3 border-b border-gray-200 pb-4 sm:flex-row sm:items-center sm:justify-between">
            <h3 className="text-base font-semibold text-gray-800">Charge Lines</h3>
            <Button
              type="button"
              variant="secondary"
              size="sm"
              onClick={() => setShowAddChargeForm((v) => !v)}
            >
              Add Charge Line
            </Button>
          </div>

          {hasDraft && (
            <div className="mb-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-900">
              ⚠ All charge lines must be Finalized before the Provider can mark this encounter as
              Ready for Coding.
            </div>
          )}

          {showAddChargeForm && (
            <form
              onSubmit={addChargeForm.handleSubmit(onAddChargeSubmit)}
              className="mb-4 grid grid-cols-1 gap-4 rounded-lg border border-gray-200 bg-gray-50 p-4 sm:grid-cols-2"
              noValidate
            >
              <Input
                label="CPT/HCPCS"
                {...addChargeForm.register('cptCode', { required: 'CPT/HCPCS is required' })}
                error={addChargeForm.formState.errors.cptCode?.message}
              />
              <Input
                label="Units"
                type="number"
                min={1}
                step={1}
                {...addChargeForm.register('units', { required: 'Required' })}
                error={addChargeForm.formState.errors.units?.message}
              />
              <Input
                label="Charge Amount"
                type="number"
                min={0}
                step={0.01}
                {...addChargeForm.register('chargeAmount', { required: 'Required' })}
                error={addChargeForm.formState.errors.chargeAmount?.message}
              />
              <Input label="Modifiers" {...addChargeForm.register('modifiers')} />
              <div className="sm:col-span-2">
                <Input label="Dx Pointers" {...addChargeForm.register('dxPointers')} />
              </div>
              <div className="flex flex-wrap gap-2 sm:col-span-2">
                <Button type="submit" variant="primary" size="sm">
                  Add Line
                </Button>
                <Button
                  type="button"
                  variant="secondary"
                  size="sm"
                  onClick={() => {
                    setShowAddChargeForm(false)
                    resetAddChargeForm({
                      cptCode: '',
                      modifiers: '',
                      units: '1',
                      chargeAmount: '',
                      dxPointers: '',
                    })
                  }}
                >
                  Cancel
                </Button>
              </div>
            </form>
          )}

          <Table columns={chargeColumns} data={chargeTableData} />
        </Card>

        <div className="flex items-start gap-3 rounded-lg border border-blue-200 bg-blue-50 px-4 py-3">
          <svg
            className="mt-0.5 h-5 w-5 shrink-0 text-blue-600"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            aria-hidden
          >
            <circle cx="12" cy="12" r="10" />
            <path d="M12 16v-4M12 8h.01" />
          </svg>
          <div className="min-w-0 flex-1">
            <p className="text-sm text-gray-800">
              <span className="font-bold text-gray-900">Attestation:</span>{' '}
              {attest?.status === 'Attested' ? (
                <>
                  <span className="font-medium text-green-700">✓ Attested</span>
                  <span className="text-gray-600">
                    {' '}
                    by {attest.attestedBy}
                    {attest.attestedDate !== '' ? ` on ${attest.attestedDate}` : ''}
                  </span>
                </>
              ) : (
                <span className="font-medium text-amber-700">
                  ⏳ Not yet attested by provider
                </span>
              )}
            </p>
            <p className="mt-2 text-xs italic text-gray-500">
              FrontDesk view only — attestation is managed by the Provider.
            </p>
          </div>
        </div>

        <Dialog
          isOpen={showEditDialog}
          onClose={() => setShowEditDialog(false)}
          title="Edit Encounter"
          maxWidth="md"
        >
          <form
            onSubmit={editEncounterForm.handleSubmit(onEditEncounterSubmit)}
            className="flex flex-col gap-4"
            noValidate
          >
            <Input
              label="Encounter Date"
              type="datetime-local"
              {...editEncounterForm.register('encounterDate', { required: 'Required' })}
              error={editEncounterForm.formState.errors.encounterDate?.message}
            />
            <div className="flex flex-col gap-1">
              <label htmlFor="edit-enc-pos" className="text-sm font-medium text-gray-700">
                POS
              </label>
              <select
                id="edit-enc-pos"
                className={selectClassName}
                {...editEncounterForm.register('pos')}
              >
                {POS_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex flex-col gap-1">
              <label htmlFor="edit-enc-notes" className="text-sm font-medium text-gray-700">
                Notes
              </label>
              <textarea
                id="edit-enc-notes"
                rows={3}
                className={textareaClassName}
                {...editEncounterForm.register('notes')}
              />
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <Button type="button" variant="secondary" onClick={() => setShowEditDialog(false)}>
                Cancel
              </Button>
              <Button type="submit" variant="primary">
                Save
              </Button>
            </div>
          </form>
        </Dialog>

        <Dialog
          isOpen={editingCharge != null}
          onClose={() => setEditingCharge(null)}
          title="Edit Charge Line"
          maxWidth="md"
        >
          <form
            onSubmit={editChargeForm.handleSubmit(onEditChargeSubmit)}
            className="flex flex-col gap-4"
            noValidate
          >
            <Input
              label="CPT/HCPCS"
              {...editChargeForm.register('cptCode', { required: 'Required' })}
              error={editChargeForm.formState.errors.cptCode?.message}
            />
            <Input label="Modifiers" {...editChargeForm.register('modifiers')} />
            <Input
              label="Units"
              type="number"
              min={1}
              step={1}
              {...editChargeForm.register('units', { required: 'Required' })}
              error={editChargeForm.formState.errors.units?.message}
            />
            <Input
              label="Charge Amount"
              type="number"
              min={0}
              step={0.01}
              {...editChargeForm.register('chargeAmount', { required: 'Required' })}
              error={editChargeForm.formState.errors.chargeAmount?.message}
            />
            <Input label="Dx Pointers" {...editChargeForm.register('dxPointers')} />
            <div className="flex justify-end gap-2 pt-2">
              <Button type="button" variant="secondary" onClick={() => setEditingCharge(null)}>
                Cancel
              </Button>
              <Button type="submit" variant="primary">
                Save
              </Button>
            </div>
          </form>
        </Dialog>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Encounters</h1>
        <Button type="button" variant="primary" onClick={() => setShowCreateDialog(true)}>
          Create Encounter
        </Button>
      </div>

      <div className="mb-4 flex flex-wrap items-end gap-3">
        <div className="flex min-w-[140px] flex-col gap-1">
          <label htmlFor="flt-status" className="text-xs font-medium text-gray-600">
            Status
          </label>
          <select
            id="flt-status"
            className={selectClassName}
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value)
              onFilterChange()
            }}
          >
            <option value="All">All</option>
            <option value="Open">Open</option>
            <option value="ReadyForCoding">ReadyForCoding</option>
            <option value="Finalized">Finalized</option>
          </select>
        </div>
        <div className="flex min-w-[180px] flex-col gap-1">
          <label htmlFor="flt-provider" className="text-xs font-medium text-gray-600">
            Provider
          </label>
          <select
            id="flt-provider"
            className={selectClassName}
            value={providerFilter}
            onChange={(e) => {
              setProviderFilter(e.target.value)
              onFilterChange()
            }}
          >
            <option value="All">All</option>
            {providerNamesDistinct.map((name) => (
              <option key={name} value={name}>
                {name}
              </option>
            ))}
          </select>
        </div>
        <div className="flex min-w-[140px] flex-col gap-1">
          <label htmlFor="flt-from" className="text-xs font-medium text-gray-600">
            Date From
          </label>
          <input
            id="flt-from"
            type="date"
            className={selectClassName}
            value={dateFrom}
            onChange={(e) => {
              setDateFrom(e.target.value)
              onFilterChange()
            }}
          />
        </div>
        <div className="flex min-w-[140px] flex-col gap-1">
          <label htmlFor="flt-to" className="text-xs font-medium text-gray-600">
            Date To
          </label>
          <input
            id="flt-to"
            type="date"
            className={selectClassName}
            value={dateTo}
            onChange={(e) => {
              setDateTo(e.target.value)
              onFilterChange()
            }}
          />
        </div>
        <Button type="button" variant="secondary" size="sm" onClick={clearFilters}>
          Clear
        </Button>
      </div>

      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          columns={listColumns}
          data={listTableData}
          showActions
          actions={[
            {
              label: 'View Detail',
              onClick: (row) => {
                const enc = encounters.find((e) => e.encounterId === row.encounterId)
                if (enc != null) setSelectedEncounter(enc)
              },
            },
          ]}
        />
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setCurrentPage}
        />
      </div>

      <Dialog
        isOpen={showCreateDialog}
        onClose={() => setShowCreateDialog(false)}
        title="Create Encounter"
        maxWidth="lg"
      >
        <form
          onSubmit={createForm.handleSubmit(onCreateEncounterSubmit)}
          className="flex flex-col gap-4"
          noValidate
        >
          <Input
            label="Search patient (name or MRN)"
            placeholder="Filter list…"
            value={patientSearch}
            onChange={(e) => setPatientSearch(e.target.value)}
          />
          <div className="flex flex-col gap-1">
            <label htmlFor="create-patient" className="text-sm font-medium text-gray-700">
              Patient
            </label>
            <select
              id="create-patient"
              className={selectClassName}
              {...createForm.register('patientId', { required: 'Select a patient' })}
            >
              <option value="">Select patient…</option>
              {filteredPatientsDropdown.map((p) => (
                <option key={p.patientId} value={p.patientId}>
                  {p.mrn} — {p.name}
                </option>
              ))}
            </select>
            {createForm.formState.errors.patientId != null && (
              <p className="text-sm text-red-600">{createForm.formState.errors.patientId.message}</p>
            )}
          </div>
          <div className="flex flex-col gap-1">
            <label htmlFor="create-provider" className="text-sm font-medium text-gray-700">
              Provider
            </label>
            <select
              id="create-provider"
              className={selectClassName}
              {...createForm.register('providerId', { required: 'Select a provider' })}
            >
              <option value="">Select provider…</option>
              {DUMMY_PROVIDERS_DROPDOWN.map((p) => (
                <option key={p.providerId} value={p.providerId}>
                  {p.name} ({p.specialty})
                </option>
              ))}
            </select>
            {createForm.formState.errors.providerId != null && (
              <p className="text-sm text-red-600">{createForm.formState.errors.providerId.message}</p>
            )}
          </div>
          <Input
            label="Encounter Date & Time"
            type="datetime-local"
            {...createForm.register('encounterDate', { required: 'Encounter date is required' })}
            error={createForm.formState.errors.encounterDate?.message}
          />
          <div className="flex flex-col gap-1">
            <label htmlFor="create-pos" className="text-sm font-medium text-gray-700">
              POS
            </label>
            <select id="create-pos" className={selectClassName} {...createForm.register('pos')}>
              {POS_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </div>
          <div className="flex flex-col gap-1">
            <label htmlFor="create-notes" className="text-sm font-medium text-gray-700">
              Notes
            </label>
            <textarea
              id="create-notes"
              rows={3}
              className={textareaClassName}
              {...createForm.register('notes')}
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={() => setShowCreateDialog(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              Create
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  )
}
