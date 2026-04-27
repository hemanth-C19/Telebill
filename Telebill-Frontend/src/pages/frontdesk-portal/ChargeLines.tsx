import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import Badge from '../../components/shared/ui/Badge'
import Button from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import Dialog from '../../components/shared/ui/Dialog'
import Input from '../../components/shared/ui/Input'
import Table from '../../components/shared/ui/Table'
import apiClient from '../../api/client'

// ── Types ─────────────────────────────────────────────────────────────

type ChargeLineIncoming = {
  chargeId: number
  encounterId: number | null
  cpT_HCPCS: string | null
  modifiers: string | null
  units: number | null
  chargeAmount: number | null
  revenueCode: string | null
  notes: string | null
  status: string | null
}

type ChargeLine = {
  chargeId: number
  encounterId: number
  cptCode: string
  modifiers: string
  units: number
  chargeAmount: number
  revenueCode: string
  notes: string
  status: string
}

type EncounterHeader = {
  encounterId: number
  patientName: string
  providerName: string
  pos: string
  status: string
}

type AddChargeFormValues = {
  cptCode: string
  modifiers: string
  units: string
  chargeAmount: string
  revenueCode: string
  notes: string
}

type EditChargeFormValues = {
  modifiers: string
  units: string
  chargeAmount: string
  notes: string
  status: string
}

// ── Helpers ───────────────────────────────────────────────────────────

function toChargeLine(raw: ChargeLineIncoming): ChargeLine {
  return {
    chargeId: raw.chargeId,
    encounterId: raw.encounterId ?? 0,
    cptCode: raw.cpT_HCPCS ?? '',
    modifiers: raw.modifiers ?? '',
    units: raw.units ?? 1,
    chargeAmount: Number(raw.chargeAmount ?? 0),
    revenueCode: raw.revenueCode ?? '',
    notes: raw.notes ?? '',
    status: raw.status ?? 'Draft',
  }
}

const chargeColumns = [
  { key: 'cptCode', label: 'CPT/HCPCS' },
  { key: 'modifiers', label: 'Modifiers' },
  { key: 'units', label: 'Units' },
  { key: 'chargeAmountDisplay', label: 'Amount' },
  { key: 'revenueCode', label: 'Rev Code' },
  { key: 'notes', label: 'Notes' },
  { key: 'status', label: 'Status' },
]

const CHARGE_STATUS_OPTIONS = ['Draft', 'Finalized', 'Void']

const selectClassName =
  'w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

// ── Component ─────────────────────────────────────────────────────────

export default function ChargeLines() {
  const { encounterId: encounterIdParam } = useParams<{ encounterId: string }>()
  const navigate = useNavigate()

  const encounterId = Number(encounterIdParam)

  const [encounterHeader, setEncounterHeader] = useState<EncounterHeader | null>(null)
  const [chargeLines, setChargeLines] = useState<ChargeLine[]>([])
  const [loading, setLoading] = useState(true)
  const [showAddDialog, setShowAddDialog] = useState(false)
  const [editingCharge, setEditingCharge] = useState<ChargeLine | null>(null)

  const addForm = useForm<AddChargeFormValues>({
    defaultValues: {
      cptCode: '',
      modifiers: '',
      units: '1',
      chargeAmount: '',
      revenueCode: '',
      notes: '',
    },
  })

  const editForm = useForm<EditChargeFormValues>({
    defaultValues: { modifiers: '', units: '1', chargeAmount: '', notes: '', status: 'Draft' },
  })

  // ── Data fetching ─────────────────────────────────────────────────

  async function fetchEncounterHeader() {
    const res = await apiClient.get(`api/v1/Encounter/Encounter/GetEncounterById/${encounterId}`)
    const d = res.data
    setEncounterHeader({
      encounterId: d.encounterId,
      patientName: d.patientName ?? '',
      providerName: d.providerName ?? '',
      pos: d.pos ?? '',
      status: d.status ?? '',
    })
  }

  async function fetchChargeLines() {
    const res = await apiClient.get(
      `api/v1/Encounter/ChargeLine/ByEncounter/${encounterId}`,
    )
    setChargeLines((res.data as ChargeLineIncoming[]).map(toChargeLine))
  }

  useEffect(() => {
    Promise.all([fetchEncounterHeader(), fetchChargeLines()]).finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    if (showAddDialog) {
      addForm.reset({ cptCode: '', modifiers: '', units: '1', chargeAmount: '', revenueCode: '', notes: '' })
    }
  }, [showAddDialog, addForm])

  useEffect(() => {
    if (editingCharge != null) {
      editForm.reset({
        modifiers: editingCharge.modifiers,
        units: String(editingCharge.units),
        chargeAmount: String(editingCharge.chargeAmount),
        notes: editingCharge.notes,
        status: editingCharge.status,
      })
    }
  }, [editingCharge, editForm])

  // ── Handlers ─────────────────────────────────────────────────────

  async function onAddCharge(values: AddChargeFormValues) {
    await apiClient.post(`api/v1/Encounter/ChargeLine/AddChargeline/${encounterId}`, {
      CptHcpcs: values.cptCode.trim().toUpperCase(),
      Modifiers: values.modifiers.trim(),
      Units: Number(values.units),
      ChargeAmount: Number(values.chargeAmount),
      RevenueCode: values.revenueCode.trim(),
      Notes: values.notes.trim(),
    })
    await fetchChargeLines()
    setShowAddDialog(false)
  }

  async function onEditCharge(values: EditChargeFormValues) {
    if (editingCharge == null) return
    await apiClient.put(
      `api/v1/Encounter/ChargeLine/UpdateChargeline/${editingCharge.chargeId}`,
      {
        Modifiers: values.modifiers.trim(),
        Units: Number(values.units),
        ChargeAmount: Number(values.chargeAmount),
        Notes: values.notes.trim(),
        Status: values.status,
      },
    )
    await fetchChargeLines()
    setEditingCharge(null)
  }

  async function handleDeleteCharge(chargeId: number) {
    await apiClient.delete(`api/v1/Encounter/ChargeLine/DeleteChargeline/${chargeId}`)
    setChargeLines((prev) => prev.filter((cl) => cl.chargeId !== chargeId))
  }

  // ── Render ────────────────────────────────────────────────────────

  if (loading) {
    return <p className="text-sm text-gray-500">Loading...</p>
  }

  if (encounterHeader == null) {
    return (
      <div className="space-y-4">
        <button
          type="button"
          onClick={() => navigate('/frontdesk/encounters')}
          className="text-sm font-medium text-blue-600 hover:text-blue-800"
        >
          ← Back to Encounters
        </button>
        <p className="text-sm text-red-500">Encounter not found.</p>
      </div>
    )
  }

  const chargeTableData = chargeLines.map((cl) => ({
    ...cl,
    chargeAmountDisplay: `$${cl.chargeAmount.toFixed(2)}`,
    status: <Badge status={cl.status} />,
  }))

  return (
    <div className="space-y-6">
      {/* Back nav */}
      <button
        type="button"
        onClick={() => navigate('/frontdesk/encounters')}
        className="text-sm font-medium text-blue-600 hover:text-blue-800"
      >
        ← Back to Encounters
      </button>

      {/* Encounter header */}
      <Card title="Charge Lines">
        <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">Encounter ID</dt>
            <dd className="text-sm text-gray-900">{encounterHeader.encounterId}</dd>
          </div>
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">Patient</dt>
            <dd className="text-sm text-gray-900">{encounterHeader.patientName}</dd>
          </div>
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">Provider</dt>
            <dd className="text-sm text-gray-900">{encounterHeader.providerName}</dd>
          </div>
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">POS</dt>
            <dd className="text-sm text-gray-900">{encounterHeader.pos}</dd>
          </div>
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">Status</dt>
            <dd className="mt-1"><Badge status={encounterHeader.status} /></dd>
          </div>
        </dl>
      </Card>

      {/* Charge lines table */}
      <Card>
        <div className="mb-4 flex flex-col gap-3 border-b border-gray-200 pb-4 sm:flex-row sm:items-center sm:justify-between">
          <h3 className="text-base font-semibold text-gray-800">
            Charge Lines ({chargeLines.length})
          </h3>
          <Button
            type="button"
            variant="secondary"
            size="sm"
            onClick={() => setShowAddDialog(true)}
          >
            Add Charge Line
          </Button>
        </div>

        {chargeLines.length === 0 ? (
          <p className="py-6 text-center text-sm text-gray-400">
            No charge lines for this encounter.
          </p>
        ) : (
          <Table
            columns={chargeColumns}
            data={chargeTableData}
            showActions
            actions={[
              {
                label: 'Edit',
                onClick: (row) => {
                  const cl = chargeLines.find((c) => c.chargeId === row.chargeId)
                  if (cl != null) setEditingCharge(cl)
                },
              },
              {
                label: 'Delete',
                variant: 'danger',
                onClick: (row) => handleDeleteCharge(row.chargeId as number),
              },
            ]}
          />
        )}
      </Card>

      {/* Add Charge Line dialog */}
      <Dialog
        isOpen={showAddDialog}
        onClose={() => setShowAddDialog(false)}
        title="Add Charge Line"
        maxWidth="md"
      >
        <form
          onSubmit={addForm.handleSubmit(onAddCharge)}
          className="flex flex-col gap-4"
          noValidate
        >
          <Input
            label="CPT/HCPCS"
            placeholder="e.g. 99213"
            {...addForm.register('cptCode', {
              required: 'CPT/HCPCS is required',
              setValueAs: (v: string) => v.trim().toUpperCase(),
              pattern: { value: /^[A-Z0-9]{4,5}$/i, message: 'Enter a valid CPT/HCPCS code (4–5 characters)' },
            })}
            error={addForm.formState.errors.cptCode?.message}
          />
          <Input
            label="Modifiers"
            placeholder="e.g. GT or GT,95"
            {...addForm.register('modifiers', {
              setValueAs: (v: string) => v.trim().toUpperCase(),
              pattern: { value: /^[A-Z0-9]+(,[A-Z0-9]+)*$/i, message: 'Use comma-separated codes e.g. GT or GT,95' },
            })}
            error={addForm.formState.errors.modifiers?.message}
          />
          <Input
            label="Units"
            type="number"
            min={1}
            step={1}
            {...addForm.register('units', {
              required: 'Units is required',
              validate: (v) => Number(v) >= 1 || 'Units must be at least 1',
            })}
            error={addForm.formState.errors.units?.message}
          />
          <Input
            label="Charge Amount"
            type="number"
            min={0}
            step={0.01}
            placeholder="0.00"
            {...addForm.register('chargeAmount', {
              required: 'Charge amount is required',
              validate: (v) => Number(v) > 0 || 'Charge amount must be greater than 0',
            })}
            error={addForm.formState.errors.chargeAmount?.message}
          />
          <Input
            label="Revenue Code"
            placeholder="Optional"
            {...addForm.register('revenueCode', {
              setValueAs: (v: string) => v.trim(),
            })}
          />
          <Input
            label="Notes"
            placeholder="Optional"
            {...addForm.register('notes', {
              setValueAs: (v: string) => v.trim(),
              maxLength: { value: 500, message: 'Notes cannot exceed 500 characters' },
            })}
            error={addForm.formState.errors.notes?.message}
          />
          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={() => setShowAddDialog(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              Add Line
            </Button>
          </div>
        </form>
      </Dialog>

      {/* Edit Charge Line dialog */}
      <Dialog
        isOpen={editingCharge != null}
        onClose={() => setEditingCharge(null)}
        title="Edit Charge Line"
        maxWidth="md"
      >
        {editingCharge != null && (
          <div className="flex flex-col gap-4">
            {/* Read-only context */}
            <div className="rounded-md bg-gray-50 px-3 py-2 text-sm text-gray-600">
              <span className="font-medium">CPT/HCPCS:</span> {editingCharge.cptCode}
              {editingCharge.revenueCode && (
                <span className="ml-4">
                  <span className="font-medium">Rev Code:</span> {editingCharge.revenueCode}
                </span>
              )}
            </div>

            <form
              onSubmit={editForm.handleSubmit(onEditCharge)}
              className="flex flex-col gap-4"
              noValidate
            >
              <Input
                label="Modifiers"
                placeholder="e.g. GT or GT,95"
                {...editForm.register('modifiers', {
                  setValueAs: (v: string) => v.trim().toUpperCase(),
                  pattern: { value: /^[A-Z0-9]+(,[A-Z0-9]+)*$/i, message: 'Use comma-separated codes e.g. GT or GT,95' },
                })}
                error={editForm.formState.errors.modifiers?.message}
              />
              <Input
                label="Units"
                type="number"
                min={1}
                step={1}
                {...editForm.register('units', {
                  required: 'Units is required',
                  validate: (v) => Number(v) >= 1 || 'Units must be at least 1',
                })}
                error={editForm.formState.errors.units?.message}
              />
              <Input
                label="Charge Amount"
                type="number"
                min={0}
                step={0.01}
                {...editForm.register('chargeAmount', {
                  required: 'Charge amount is required',
                  validate: (v) => Number(v) > 0 || 'Charge amount must be greater than 0',
                })}
                error={editForm.formState.errors.chargeAmount?.message}
              />
              <Input
                label="Notes"
                placeholder="Optional"
                {...editForm.register('notes', {
                  setValueAs: (v: string) => v.trim(),
                  maxLength: { value: 500, message: 'Notes cannot exceed 500 characters' },
                })}
                error={editForm.formState.errors.notes?.message}
              />
              <div className="flex flex-col gap-1">
                <label className="text-sm font-medium text-gray-700">Status</label>
                <select className={selectClassName} {...editForm.register('status', { required: 'Required' })}>
                  {CHARGE_STATUS_OPTIONS.map((s) => (
                    <option key={s} value={s}>{s}</option>
                  ))}
                </select>
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <Button type="button" variant="secondary" onClick={() => setEditingCharge(null)}>
                  Cancel
                </Button>
                <Button type="submit" variant="primary">
                  Save Changes
                </Button>
              </div>
            </form>
          </div>
        )}
      </Dialog>
    </div>
  )
}
