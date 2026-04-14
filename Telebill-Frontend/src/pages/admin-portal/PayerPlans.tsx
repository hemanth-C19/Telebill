import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import { Button } from '../../components/shared/ui/Button'
import { Table } from '../../components/shared/ui/Table'
import { Dialog } from '../../components/shared/ui/Dialog'
import { Badge } from '../../components/shared/ui/Badge'

type PayerPlan = {
  planId: number
  payerId: number
  planName: string
  networkType: string
  posDefault: string
  telehealthModifiersJson: string
  status: string
}

type PlanFormValues = {
  planName: string
  networkType: string
  posDefault: string
  telehealthModifiersJson: string
  status: string
}

const DUMMY_PLANS: Record<number, PayerPlan[]> = {
  10: [
    { planId: 101, payerId: 10, planName: 'BCBS PPO Basic', networkType: 'PPO', posDefault: '02', telehealthModifiersJson: 'GT,GQ', status: 'Active' },
    { planId: 102, payerId: 10, planName: 'BCBS HMO Plus', networkType: 'HMO', posDefault: '10', telehealthModifiersJson: 'GT', status: 'Active' },
    { planId: 103, payerId: 10, planName: 'BCBS Dental Rider', networkType: 'PPO', posDefault: '02', telehealthModifiersJson: '', status: 'Inactive' },
  ],
  20: [
    { planId: 201, payerId: 20, planName: 'Aetna Select PPO', networkType: 'PPO', posDefault: '02', telehealthModifiersJson: 'GT', status: 'Active' },
    { planId: 202, payerId: 20, planName: 'Aetna Choice HMO', networkType: 'HMO', posDefault: '10', telehealthModifiersJson: 'GQ', status: 'Active' },
  ],
  30: [
    { planId: 301, payerId: 30, planName: 'UHC Gold PPO', networkType: 'PPO', posDefault: '02', telehealthModifiersJson: 'GT,95', status: 'Active' },
    { planId: 302, payerId: 30, planName: 'UHC Silver HMO', networkType: 'HMO', posDefault: '10', telehealthModifiersJson: 'GT', status: 'Active' },
    { planId: 303, payerId: 30, planName: 'UHC Bronze EPO', networkType: 'EPO', posDefault: '02', telehealthModifiersJson: '', status: 'Inactive' },
  ],
  40: [{ planId: 401, payerId: 40, planName: 'Cigna Connect HMO', networkType: 'HMO', posDefault: '02', telehealthModifiersJson: 'GT', status: 'Inactive' }],
}

const fieldClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

export default function PayerPlans() {
  const { payerId } = useParams<{ payerId: string }>()
  const location = useLocation()
  const navigate = useNavigate()
  const payerIdNum = Number(payerId)
  const payerName = (location.state as { payerName?: string })?.payerName ?? `Payer #${payerId}`

  const [plans, setPlans] = useState<PayerPlan[]>(DUMMY_PLANS[payerIdNum] ?? [])
  const [showAddPlan, setShowAddPlan] = useState(false)
  const [editingPlan, setEditingPlan] = useState<PayerPlan | null>(null)

  const addPlanForm = useForm<PlanFormValues>({
    defaultValues: { networkType: 'PPO', posDefault: '02', status: 'Active' },
  })
  const editPlanForm = useForm<PlanFormValues>()

  const onAddPlan = addPlanForm.handleSubmit((data) => {
    const newPlan: PayerPlan = { planId: Math.floor(Math.random() * 9000) + 500, payerId: payerIdNum, ...data }
    setPlans((prev) => [newPlan, ...prev])
    setShowAddPlan(false)
    addPlanForm.reset()
  })

  const onEditPlan = editPlanForm.handleSubmit((data) => {
    if (editingPlan == null) return
    setPlans((prev) => prev.map((p) => (p.planId === editingPlan.planId ? { ...p, ...data } : p)))
    setEditingPlan(null)
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
        <span className="text-gray-900 font-medium">{payerName}</span>
      </div>

      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{payerName} — Payer Plans</h1>
        <Button size="sm" onClick={() => setShowAddPlan(true)}>
          Add Plan
        </Button>
      </div>

      <Table
        columns={[
          { key: 'planName', label: 'Plan Name' },
          { key: 'networkType', label: 'Network Type' },
          { key: 'posDefault', label: 'Default POS' },
          { key: 'telehealthModifiersJson', label: 'Telehealth Modifiers' },
          { key: 'status', label: 'Status' },
        ]}
        data={plans.map((row) => ({ ...row, status: <Badge status={row.status} /> }))}
        showActions={true}
        actions={[
          {
            label: 'View Fee Schedules',
            onClick: (row) => {
              const selected = row as PayerPlan
              navigate(`/admin/master-data/payers/${payerIdNum}/plans/${selected.planId}/fees`, {
                state: { planName: selected.planName, payerName },
              })
            },
          },
          {
            label: 'Edit',
            onClick: (row) => {
              const selected = row as PayerPlan
              setEditingPlan(selected)
              editPlanForm.reset({
                planName: selected.planName,
                networkType: selected.networkType,
                posDefault: selected.posDefault,
                telehealthModifiersJson: selected.telehealthModifiersJson,
                status: selected.status,
              })
            },
          },
          {
            label: 'Delete',
            variant: 'danger',
            onClick: (row) => {
              const selected = row as PayerPlan
              setPlans((prev) => prev.filter((p) => p.planId !== selected.planId))
            },
          },
        ]}
      />

      {plans.length === 0 && (
        <div className="text-center py-12 text-gray-500">No plans found for this payer. Add the first plan above.</div>
      )}

      <Dialog
        isOpen={showAddPlan}
        onClose={() => {
          setShowAddPlan(false)
          addPlanForm.reset()
        }}
        title="Add Payer Plan"
        maxWidth="lg"
      >
        <form className="space-y-4" onSubmit={onAddPlan}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Plan Name *</label>
              <input {...addPlanForm.register('planName', { required: 'Plan name is required' })} className={fieldClassName} />
              {addPlanForm.formState.errors.planName && (
                <p className="text-red-500 text-xs mt-1">{addPlanForm.formState.errors.planName.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Network Type *</label>
              <select {...addPlanForm.register('networkType', { required: 'Network type is required' })} className={fieldClassName}>
                <option value="PPO">PPO</option>
                <option value="HMO">HMO</option>
                <option value="EPO">EPO</option>
                <option value="POS">POS</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Default POS</label>
              <select {...addPlanForm.register('posDefault')} className={fieldClassName}>
                <option value="02">02 — Telehealth Patient Home</option>
                <option value="10">10 — Telehealth Non-Patient Home</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Telehealth Modifiers JSON</label>
              <input {...addPlanForm.register('telehealthModifiersJson')} className={fieldClassName} placeholder="e.g. GT,GQ" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select {...addPlanForm.register('status')} className={fieldClassName}>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button
              variant="secondary"
              onClick={() => {
                setShowAddPlan(false)
                addPlanForm.reset()
              }}
            >
              Cancel
            </Button>
            <Button type="submit">Save Plan</Button>
          </div>
        </form>
      </Dialog>

      <Dialog isOpen={editingPlan !== null} onClose={() => setEditingPlan(null)} title="Edit Payer Plan" maxWidth="lg">
        <form className="space-y-4" onSubmit={onEditPlan}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Plan Name *</label>
              <input {...editPlanForm.register('planName', { required: 'Plan name is required' })} className={fieldClassName} />
              {editPlanForm.formState.errors.planName && (
                <p className="text-red-500 text-xs mt-1">{editPlanForm.formState.errors.planName.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Network Type *</label>
              <select {...editPlanForm.register('networkType', { required: 'Network type is required' })} className={fieldClassName}>
                <option value="PPO">PPO</option>
                <option value="HMO">HMO</option>
                <option value="EPO">EPO</option>
                <option value="POS">POS</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Default POS</label>
              <select {...editPlanForm.register('posDefault')} className={fieldClassName}>
                <option value="02">02 — Telehealth Patient Home</option>
                <option value="10">10 — Telehealth Non-Patient Home</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Telehealth Modifiers JSON</label>
              <input {...editPlanForm.register('telehealthModifiersJson')} className={fieldClassName} placeholder="e.g. GT,GQ" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select {...editPlanForm.register('status')} className={fieldClassName}>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={() => setEditingPlan(null)}>
              Cancel
            </Button>
            <Button type="submit">Update Plan</Button>
          </div>
        </form>
      </Dialog>
    </div>
  )
}
