import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import { Button } from '../../components/shared/ui/Button'
import { Table } from '../../components/shared/ui/Table'
import { Dialog } from '../../components/shared/ui/Dialog'
import { Badge } from '../../components/shared/ui/Badge'

type FeeSchedule = {
  feeId: number
  planId: number
  cptHcpcs: string
  modifierCombo: string
  allowedAmount: number
  effectiveFrom: string
  effectiveTo: string
  status: string
}

type FeeFormValues = {
  cptHcpcs: string
  modifierCombo: string
  allowedAmount: string
  effectiveFrom: string
  effectiveTo: string
  status: string
}

const DUMMY_FEES: Record<number, FeeSchedule[]> = {
  101: [
    { feeId: 1001, planId: 101, cptHcpcs: '99213', modifierCombo: 'GT', allowedAmount: 120.0, effectiveFrom: '2024-01-01', effectiveTo: '2024-12-31', status: 'Active' },
    { feeId: 1002, planId: 101, cptHcpcs: '99214', modifierCombo: 'GT', allowedAmount: 165.0, effectiveFrom: '2024-01-01', effectiveTo: '2024-12-31', status: 'Active' },
    { feeId: 1003, planId: 101, cptHcpcs: '90837', modifierCombo: 'GT', allowedAmount: 150.0, effectiveFrom: '2024-01-01', effectiveTo: '2024-12-31', status: 'Active' },
    { feeId: 1004, planId: 101, cptHcpcs: '93000', modifierCombo: '', allowedAmount: 72.0, effectiveFrom: '2024-01-01', effectiveTo: '2024-12-31', status: 'Active' },
  ],
  102: [
    { feeId: 1005, planId: 102, cptHcpcs: '99213', modifierCombo: 'GQ', allowedAmount: 110.0, effectiveFrom: '2024-01-01', effectiveTo: '2024-12-31', status: 'Active' },
    { feeId: 1006, planId: 102, cptHcpcs: '99214', modifierCombo: 'GQ', allowedAmount: 155.0, effectiveFrom: '2024-01-01', effectiveTo: '2024-12-31', status: 'Active' },
    { feeId: 1007, planId: 102, cptHcpcs: '99215', modifierCombo: 'GQ', allowedAmount: 200.0, effectiveFrom: '2023-01-01', effectiveTo: '2023-12-31', status: 'Inactive' },
  ],
  201: [
    { feeId: 2001, planId: 201, cptHcpcs: '99213', modifierCombo: 'GT', allowedAmount: 125.0, effectiveFrom: '2024-03-01', effectiveTo: '2025-02-28', status: 'Active' },
    { feeId: 2002, planId: 201, cptHcpcs: '99214', modifierCombo: 'GT', allowedAmount: 170.0, effectiveFrom: '2024-03-01', effectiveTo: '2025-02-28', status: 'Active' },
    { feeId: 2003, planId: 201, cptHcpcs: '96130', modifierCombo: 'GT', allowedAmount: 195.0, effectiveFrom: '2024-03-01', effectiveTo: '2025-02-28', status: 'Active' },
  ],
  301: [
    { feeId: 3001, planId: 301, cptHcpcs: '99213', modifierCombo: 'GT,95', allowedAmount: 130.0, effectiveFrom: '2024-01-01', effectiveTo: '2024-12-31', status: 'Active' },
    { feeId: 3002, planId: 301, cptHcpcs: '99214', modifierCombo: 'GT,95', allowedAmount: 178.0, effectiveFrom: '2024-01-01', effectiveTo: '2024-12-31', status: 'Active' },
    { feeId: 3003, planId: 301, cptHcpcs: '90837', modifierCombo: 'GT', allowedAmount: 160.0, effectiveFrom: '2024-01-01', effectiveTo: '2024-12-31', status: 'Active' },
  ],
}

const fieldClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

export default function FeeSchedules() {
  const { planId } = useParams<{ planId: string }>()
  const { payerId } = useParams<{ payerId: string }>()
  const location = useLocation()
  const navigate = useNavigate()
  const planIdNum = Number(planId)
  const state = (location.state as { planName?: string; payerName?: string }) ?? {}
  const planName = state.planName ?? `Plan #${planId}`
  const payerName = state.payerName ?? 'Payer'

  const [fees, setFees] = useState<FeeSchedule[]>(DUMMY_FEES[planIdNum] ?? [])
  const [showAddFee, setShowAddFee] = useState(false)
  const [editingFee, setEditingFee] = useState<FeeSchedule | null>(null)

  const addFeeForm = useForm<FeeFormValues>({ defaultValues: { status: 'Active' } })
  const editFeeForm = useForm<FeeFormValues>()

  const onAddFee = addFeeForm.handleSubmit((data) => {
    const newFee: FeeSchedule = {
      feeId: Math.floor(Math.random() * 9000) + 5000,
      planId: planIdNum,
      cptHcpcs: data.cptHcpcs.toUpperCase(),
      modifierCombo: data.modifierCombo,
      allowedAmount: Number(data.allowedAmount),
      effectiveFrom: data.effectiveFrom,
      effectiveTo: data.effectiveTo,
      status: data.status,
    }
    setFees((prev) => [newFee, ...prev])
    setShowAddFee(false)
    addFeeForm.reset()
  })

  const onEditFee = editFeeForm.handleSubmit((data) => {
    if (editingFee == null) return
    setFees((prev) =>
      prev.map((f) =>
        f.feeId === editingFee.feeId
          ? {
              ...f,
              cptHcpcs: data.cptHcpcs.toUpperCase(),
              modifierCombo: data.modifierCombo,
              allowedAmount: Number(data.allowedAmount),
              effectiveFrom: data.effectiveFrom,
              effectiveTo: data.effectiveTo,
              status: data.status,
            }
          : f,
      ),
    )
    setEditingFee(null)
  })

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-1 text-sm text-gray-500">
        <button type="button" className="hover:text-gray-700" onClick={() => navigate('/admin/master-data')}>
          Master Data
        </button>
        <span>{'>'}</span>
        <button type="button" className="hover:text-gray-700" onClick={() => navigate('/admin/master-data', { state: { tab: 'payers' } })}>
          Payers
        </button>
        <span>{'>'}</span>
        <button
          type="button"
          className="hover:text-gray-700"
          onClick={() => navigate(`/admin/master-data/payers/${payerId}/plans`, { state: { payerName } })}
        >
          {payerName}
        </button>
        <span>{'>'}</span>
        <span className="font-medium text-gray-900">{planName}</span>
      </div>

      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{planName} — Fee Schedules</h1>
        <Button size="sm" onClick={() => setShowAddFee(true)}>
          Add Fee Schedule
        </Button>
      </div>

      <Table
        columns={[
          { key: 'cptHcpcs', label: 'CPT/HCPCS' },
          { key: 'modifierCombo', label: 'Modifier Combo' },
          { key: 'allowedAmountText', label: 'Allowed Amount' },
          { key: 'effectiveFrom', label: 'Effective From' },
          { key: 'effectiveTo', label: 'Effective To' },
          { key: 'status', label: 'Status' },
        ]}
        data={fees.map((row) => ({
          ...row,
          allowedAmountText: `$${row.allowedAmount.toFixed(2)}`,
          status: <Badge status={row.status} />,
        }))}
        showActions={true}
        actions={[
          {
            label: 'Edit',
            onClick: (row) => {
              const selected = row as FeeSchedule
              setEditingFee(selected)
              editFeeForm.reset({
                cptHcpcs: selected.cptHcpcs,
                modifierCombo: selected.modifierCombo,
                allowedAmount: String(selected.allowedAmount),
                effectiveFrom: selected.effectiveFrom,
                effectiveTo: selected.effectiveTo,
                status: selected.status,
              })
            },
          },
          {
            label: 'Delete',
            variant: 'danger',
            onClick: (row) => {
              const selected = row as FeeSchedule
              setFees((prev) => prev.filter((f) => f.feeId !== selected.feeId))
            },
          },
        ]}
      />

      {fees.length === 0 && (
        <div className="text-center py-12 text-gray-500">No fee schedules for this plan yet. Add the first fee schedule above.</div>
      )}

      <Dialog
        isOpen={showAddFee}
        onClose={() => {
          setShowAddFee(false)
          addFeeForm.reset()
        }}
        title="Add Fee Schedule"
        maxWidth="lg"
      >
        <form className="space-y-4" onSubmit={onAddFee}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">CPT/HCPCS Code *</label>
              <input
                {...addFeeForm.register('cptHcpcs', { required: 'CPT/HCPCS is required' })}
                className={fieldClassName}
                placeholder="e.g. 99213"
              />
              {addFeeForm.formState.errors.cptHcpcs && (
                <p className="text-red-500 text-xs mt-1">{addFeeForm.formState.errors.cptHcpcs.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Modifier Combo</label>
              <input {...addFeeForm.register('modifierCombo')} className={fieldClassName} placeholder="e.g. GT or GT,95" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Allowed Amount *</label>
              <input
                type="number"
                step="0.01"
                min="0"
                {...addFeeForm.register('allowedAmount', {
                  required: 'Allowed amount is required',
                  validate: (v) => Number(v) > 0 || 'Must be > 0',
                })}
                className={fieldClassName}
              />
              {addFeeForm.formState.errors.allowedAmount && (
                <p className="text-red-500 text-xs mt-1">{addFeeForm.formState.errors.allowedAmount.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Effective From *</label>
              <input type="date" {...addFeeForm.register('effectiveFrom', { required: 'Effective from is required' })} className={fieldClassName} />
              {addFeeForm.formState.errors.effectiveFrom && (
                <p className="text-red-500 text-xs mt-1">{addFeeForm.formState.errors.effectiveFrom.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Effective To *</label>
              <input type="date" {...addFeeForm.register('effectiveTo', { required: 'Effective to is required' })} className={fieldClassName} />
              {addFeeForm.formState.errors.effectiveTo && (
                <p className="text-red-500 text-xs mt-1">{addFeeForm.formState.errors.effectiveTo.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select {...addFeeForm.register('status')} className={fieldClassName}>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button
              variant="secondary"
              onClick={() => {
                setShowAddFee(false)
                addFeeForm.reset()
              }}
            >
              Cancel
            </Button>
            <Button type="submit">Save Fee</Button>
          </div>
        </form>
      </Dialog>

      <Dialog isOpen={editingFee !== null} onClose={() => setEditingFee(null)} title="Edit Fee Schedule" maxWidth="lg">
        <form className="space-y-4" onSubmit={onEditFee}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">CPT/HCPCS Code *</label>
              <input
                {...editFeeForm.register('cptHcpcs', { required: 'CPT/HCPCS is required' })}
                className={fieldClassName}
                placeholder="e.g. 99213"
              />
              {editFeeForm.formState.errors.cptHcpcs && (
                <p className="text-red-500 text-xs mt-1">{editFeeForm.formState.errors.cptHcpcs.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Modifier Combo</label>
              <input {...editFeeForm.register('modifierCombo')} className={fieldClassName} placeholder="e.g. GT or GT,95" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Allowed Amount *</label>
              <input
                type="number"
                step="0.01"
                min="0"
                {...editFeeForm.register('allowedAmount', {
                  required: 'Allowed amount is required',
                  validate: (v) => Number(v) > 0 || 'Must be > 0',
                })}
                className={fieldClassName}
              />
              {editFeeForm.formState.errors.allowedAmount && (
                <p className="text-red-500 text-xs mt-1">{editFeeForm.formState.errors.allowedAmount.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Effective From *</label>
              <input type="date" {...editFeeForm.register('effectiveFrom', { required: 'Effective from is required' })} className={fieldClassName} />
              {editFeeForm.formState.errors.effectiveFrom && (
                <p className="text-red-500 text-xs mt-1">{editFeeForm.formState.errors.effectiveFrom.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Effective To *</label>
              <input type="date" {...editFeeForm.register('effectiveTo', { required: 'Effective to is required' })} className={fieldClassName} />
              {editFeeForm.formState.errors.effectiveTo && (
                <p className="text-red-500 text-xs mt-1">{editFeeForm.formState.errors.effectiveTo.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select {...editFeeForm.register('status')} className={fieldClassName}>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={() => setEditingFee(null)}>
              Cancel
            </Button>
            <Button type="submit">Update Fee</Button>
          </div>
        </form>
      </Dialog>
    </div>
  )
}
