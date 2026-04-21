import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import Badge from '../../components/shared/ui/Badge'
import Button from '../../components/shared/ui/Button'
import { Pagination } from '../../components/shared/ui/Pagination'
import Table from '../../components/shared/ui/Table'
import { CreateEncounterDialog } from '../../components/frontdesk-portal/CreateEncounterDialog'
import apiClient from '../../api/client'
import type { CreateEncounterFormValues, Encounter } from '../../types/frontdesk.types'

// ── Types for raw API responses ───────────────────────────────────────

type EncounterIncoming = {
  encounterId: number
  patientId: number
  patientName: string | null
  providerId: number
  providerName: string | null
  encounterDateTime: string
  visitType: string | null
  pos: string | null
  documentationUri: string | null
  status: string
  chargeLineCount: number
}

type ActivePatient = { patientId: number; name: string }
type ActiveProvider = { providerId: number; providerName: string }

// ── Constants ─────────────────────────────────────────────────────────

const POS_OPTIONS = [
  { value: '02', label: '02 — Telehealth (Not Patient Home)' },
  { value: '10', label: '10 — Telehealth (Patient Home)' },
] as const

const PAGE_SIZE = 5

const selectClassName =
  'w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

const textareaClassName =
  'w-full rounded-lg border border-gray-300 p-2 text-sm text-gray-900 shadow-sm placeholder:text-gray-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

// ── Helpers ───────────────────────────────────────────────────────────

function toEncounter(raw: EncounterIncoming): Encounter {
  return {
    encounterId: raw.encounterId,
    patientId: raw.patientId,
    patientName: raw.patientName ?? '',
    providerId: raw.providerId,
    providerName: raw.providerName ?? '',
    encounterDate: raw.encounterDateTime,
    pos: raw.pos ?? '',
    notes: raw.documentationUri ?? '',
    status: (raw.status ?? 'Open') as Encounter['status'],
    chargeLineCount: raw.chargeLineCount ?? 0,
  }
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

// ── Component ─────────────────────────────────────────────────────────

export default function Encounters() {
  const navigate = useNavigate()

  const [encounters, setEncounters] = useState<Encounter[]>([])
  const [loading, setLoading] = useState(true)
  const [patientOptions, setPatientOptions] = useState<ActivePatient[]>([])
  const [providerOptions, setProviderOptions] = useState<ActiveProvider[]>([])

  const [statusFilter, setStatusFilter] = useState('All')
  const [providerFilter, setProviderFilter] = useState('All')
  const [currentPage, setCurrentPage] = useState(1)
  const [showCreateDialog, setShowCreateDialog] = useState(false)
  const [editingEncounter, setEditingEncounter] = useState<Encounter | null>(null)

  const createForm = useForm<CreateEncounterFormValues>({
    defaultValues: { patientId: '', providerId: '', encounterDate: '', pos: '02', notes: '' },
  })
  const editForm = useForm<CreateEncounterFormValues>({
    defaultValues: { patientId: '', providerId: '', encounterDate: '', pos: '02', notes: '' },
  })

  // ── Data fetching ──────────────────────────────────────────────────

  async function fetchEncounters() {
    const res = await apiClient.get('api/v1/Encounter/Encounter/GetAllEncounters')
    setEncounters((res.data as EncounterIncoming[]).map(toEncounter))
  }

  async function fetchDropdowns() {
    const [patientsRes, providersRes] = await Promise.all([
      apiClient.get('api/v1/PatientCoverage/Patient/GetActivePatients'),
      apiClient.get('api/v1/MasterData/Provider/GetActiveProviders'),
    ])
    setPatientOptions(patientsRes.data)
    setProviderOptions(providersRes.data)
  }

  useEffect(() => {
    Promise.all([fetchEncounters(), fetchDropdowns()]).finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    if (showCreateDialog) {
      createForm.reset({ patientId: '', providerId: '', encounterDate: '', pos: '02', notes: '' })
    }
  }, [showCreateDialog, createForm])

  useEffect(() => {
    if (editingEncounter != null) {
      editForm.reset({
        patientId: String(editingEncounter.patientId),
        providerId: String(editingEncounter.providerId),
        encounterDate: editingEncounter.encounterDate,
        pos: editingEncounter.pos,
        notes: editingEncounter.notes,
      })
    }
  }, [editingEncounter, editForm])

  // ── Filters & pagination ───────────────────────────────────────────

  const providerNamesDistinct = useMemo(
    () => [...new Set(encounters.map((e) => e.providerName))].sort(),
    [encounters],
  )

  const filtered = useMemo(
    () =>
      encounters
        .filter((e) => statusFilter === 'All' || e.status === statusFilter)
        .filter((e) => providerFilter === 'All' || e.providerName === providerFilter),
    [encounters, statusFilter, providerFilter],
  )

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))

  useEffect(() => {
    setCurrentPage((p) => Math.min(p, totalPages))
  }, [totalPages])

  const paginated = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE
    return filtered.slice(start, start + PAGE_SIZE)
  }, [filtered, currentPage])

  // ── Handlers ──────────────────────────────────────────────────────

  async function onCreateSubmit(values: CreateEncounterFormValues) {
    await apiClient.post('api/v1/Encounter/Encounter/AddEncounter', {
      PatientId: Number(values.patientId),
      ProviderId: Number(values.providerId),
      EncounterDateTime: values.encounterDate,
      Pos: values.pos,
      DocumentationUri: values.notes,
      Status: 'Open',
    })
    await fetchEncounters()
    setShowCreateDialog(false)
  }

  async function onEditSubmit(values: CreateEncounterFormValues) {
    if (editingEncounter == null) return
    await apiClient.put(
      `api/v1/Encounter/Encounter/UpdateEncounterById/${editingEncounter.encounterId}`,
      {
        EncounterDateTime: values.encounterDate,
        Pos: values.pos,
        DocumentationUri: values.notes,
        Status: editingEncounter.status,
        VisitType: null,
      },
    )
    await fetchEncounters()
    setEditingEncounter(null)
  }

  async function handleDelete(encounterId: number) {
    await apiClient.delete(`api/v1/Encounter/Encounter/DeleteEncounter/${encounterId}`)
    setEncounters((prev) => prev.filter((e) => e.encounterId !== encounterId))
  }

  // ── Table ─────────────────────────────────────────────────────────

  const listColumns = [
    { key: 'encounterId', label: 'ID' },
    { key: 'patientName', label: 'Patient' },
    { key: 'providerName', label: 'Provider' },
    { key: 'encounterDateDisplay', label: 'Date' },
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

  // ── Render ────────────────────────────────────────────────────────

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Encounters</h1>
        <Button type="button" variant="primary" onClick={() => setShowCreateDialog(true)}>
          Create Encounter
        </Button>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-end gap-3">
        <div className="flex min-w-35 flex-col gap-1">
          <label htmlFor="flt-status" className="text-xs font-medium text-gray-600">
            Status
          </label>
          <select
            id="flt-status"
            className={selectClassName}
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setCurrentPage(1) }}
          >
            <option value="All">All</option>
            <option value="Open">Open</option>
            <option value="ReadyForCoding">ReadyForCoding</option>
            <option value="Finalized">Finalized</option>
          </select>
        </div>
        <div className="flex min-w-45 flex-col gap-1">
          <label htmlFor="flt-provider" className="text-xs font-medium text-gray-600">
            Provider
          </label>
          <select
            id="flt-provider"
            className={selectClassName}
            value={providerFilter}
            onChange={(e) => { setProviderFilter(e.target.value); setCurrentPage(1) }}
          >
            <option value="All">All</option>
            {providerNamesDistinct.map((name) => (
              <option key={name} value={name}>{name}</option>
            ))}
          </select>
        </div>
        <Button
          type="button"
          variant="secondary"
          size="sm"
          onClick={() => { setStatusFilter('All'); setProviderFilter('All'); setCurrentPage(1) }}
        >
          Clear
        </Button>
      </div>

      {/* Encounter table */}
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          columns={listColumns}
          data={listTableData}
          loading={loading}
          showActions
          actions={[
            {
              label: 'Edit',
              onClick: (row) => {
                const enc = encounters.find((e) => e.encounterId === row.encounterId)
                if (enc != null) setEditingEncounter(enc)
              },
            },
            {
              label: 'Add ChargeLine',
              onClick: (row) =>
                navigate(`/frontdesk/encounters/chargeLines/${row.encounterId}`),
            },
            {
              label: 'Delete',
              variant: 'danger',
              onClick: (row) => handleDelete(row.encounterId as number),
            },
          ]}
        />
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setCurrentPage}
        />
      </div>

      {/* Create dialog */}
      <CreateEncounterDialog
        mode="create"
        isOpen={showCreateDialog}
        onClose={() => setShowCreateDialog(false)}
        form={createForm}
        onSubmit={onCreateSubmit}
        patientOptions={patientOptions.map((p) => ({ patientId: p.patientId, name: p.name, mrn: '' }))}
        providerOptions={providerOptions.map((p) => ({ providerId: p.providerId, name: p.providerName, specialty: '' }))}
        posOptions={POS_OPTIONS}
        selectClassName={selectClassName}
        textareaClassName={textareaClassName}
      />

      {/* Edit dialog */}
      <CreateEncounterDialog
        mode="edit"
        isOpen={editingEncounter != null}
        onClose={() => setEditingEncounter(null)}
        form={editForm}
        onSubmit={onEditSubmit}
        editingEncounter={editingEncounter}
        posOptions={POS_OPTIONS}
        selectClassName={selectClassName}
        textareaClassName={textareaClassName}
      />
    </div>
  )
}
