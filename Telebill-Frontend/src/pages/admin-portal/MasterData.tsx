import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { Button } from '../../components/shared/ui/Button'
import { Table } from '../../components/shared/ui/Table'
import { Dialog } from '../../components/shared/ui/Dialog'
import { Badge } from '../../components/shared/ui/Badge'

type Payer = {
  payerId: number
  name: string
  payerCode: string
  clearinghouseCode: string
  contactInfo: string
  status: string
}

type PayerFormValues = {
  name: string
  payerCode: string
  clearinghouseCode: string
  contactInfo: string
  status: string
}

const DUMMY_PAYERS: Payer[] = [
  { payerId: 10, name: 'BlueCross BlueShield', payerCode: 'BCBS01', clearinghouseCode: 'CHC-BC01', contactInfo: '800-123-4567', status: 'Active' },
  { payerId: 20, name: 'Aetna', payerCode: 'AET001', clearinghouseCode: 'CHC-AE01', contactInfo: '800-234-5678', status: 'Active' },
  { payerId: 30, name: 'United Healthcare', payerCode: 'UHC001', clearinghouseCode: 'CHC-UH01', contactInfo: '800-345-6789', status: 'Active' },
  { payerId: 40, name: 'Cigna', payerCode: 'CGN001', clearinghouseCode: 'CHC-CG01', contactInfo: '800-456-7890', status: 'Inactive' },
]

const fieldClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

export default function MasterData() {
  const navigate = useNavigate()
  const [payers, setPayers] = useState<Payer[]>(DUMMY_PAYERS)
  const [showAddPayer, setShowAddPayer] = useState(false)
  const [editingPayer, setEditingPayer] = useState<Payer | null>(null)

  const addPayerForm = useForm<PayerFormValues>({ defaultValues: { status: 'Active' } })
  const editPayerForm = useForm<PayerFormValues>()

  const onAddPayer = addPayerForm.handleSubmit((data) => {
    const newPayer: Payer = { payerId: Math.floor(Math.random() * 900) + 100, ...data }
    setPayers((prev) => [newPayer, ...prev])
    setShowAddPayer(false)
    addPayerForm.reset()
  })

  const onEditPayer = editPayerForm.handleSubmit((data) => {
    if (editingPayer == null) return
    setPayers((prev) => prev.map((p) => (p.payerId === editingPayer.payerId ? { ...p, ...data } : p)))
    setEditingPayer(null)
  })

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Payers</h1>
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-gray-900">Payers</h2>
        <Button variant="primary" size="sm" onClick={() => setShowAddPayer(true)}>
          Add Payer
        </Button>
      </div>
      <Table
        columns={[
          { key: 'name', label: 'Payer Name' },
          { key: 'payerCode', label: 'Payer Code (EDI)' },
          { key: 'clearinghouseCode', label: 'Clearinghouse Code' },
          { key: 'contactInfo', label: 'Contact Info' },
          { key: 'status', label: 'Status' },
          { key: 'plans', label: 'Plans' },
        ]}
        data={payers.map((row) => ({
          ...row,
          status: <Badge status={row.status} />,
          plans: (
            <button
              type="button"
              className="text-blue-600 text-sm hover:underline cursor-pointer"
              onClick={() =>
                navigate(`/admin/master-data/payers/${row.payerId}/plans`, {
                  state: { payerName: row.name },
                })
              }
            >
              View Plans →
            </button>
          ),
        }))}
        showActions={true}
        actions={[
          {
            label: 'View Plans',
            onClick: (row) => {
              const selected = row as Payer
              navigate(`/admin/master-data/payers/${selected.payerId}/plans`, { state: { payerName: selected.name } })
            },
          },
          {
            label: 'Edit',
            onClick: (row) => {
              const selected = row as Payer
              setEditingPayer(selected)
              editPayerForm.reset({
                name: selected.name,
                payerCode: selected.payerCode,
                clearinghouseCode: selected.clearinghouseCode,
                contactInfo: selected.contactInfo,
                status: selected.status,
              })
            },
          },
          {
            label: 'Delete',
            variant: 'danger',
            onClick: (row) => {
              const selected = row as Payer
              setPayers((prev) => prev.filter((p) => p.payerId !== selected.payerId))
            },
          },
        ]}
      />

      <Dialog
        isOpen={showAddPayer}
        onClose={() => {
          setShowAddPayer(false)
          addPayerForm.reset()
        }}
        title="Add Payer"
        maxWidth="lg"
      >
        <form className="space-y-4" onSubmit={onAddPayer}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Payer Name *</label>
              <input {...addPayerForm.register('name', { required: 'Payer name is required' })} className={fieldClassName} />
              {addPayerForm.formState.errors.name && (
                <p className="text-red-500 text-xs mt-1">{addPayerForm.formState.errors.name.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Payer Code / EDI ID *</label>
              <input {...addPayerForm.register('payerCode', { required: 'Payer code is required' })} className={fieldClassName} />
              {addPayerForm.formState.errors.payerCode && (
                <p className="text-red-500 text-xs mt-1">{addPayerForm.formState.errors.payerCode.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Clearinghouse Code</label>
              <input {...addPayerForm.register('clearinghouseCode')} className={fieldClassName} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Contact Info</label>
              <input {...addPayerForm.register('contactInfo')} className={fieldClassName} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select {...addPayerForm.register('status')} className={fieldClassName}>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button
              variant="secondary"
              onClick={() => {
                setShowAddPayer(false)
                addPayerForm.reset()
              }}
            >
              Cancel
            </Button>
            <Button type="submit">Save Payer</Button>
          </div>
        </form>
      </Dialog>

      <Dialog isOpen={editingPayer !== null} onClose={() => setEditingPayer(null)} title="Edit Payer" maxWidth="lg">
        <form className="space-y-4" onSubmit={onEditPayer}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Payer Name *</label>
              <input {...editPayerForm.register('name', { required: 'Payer name is required' })} className={fieldClassName} />
              {editPayerForm.formState.errors.name && (
                <p className="text-red-500 text-xs mt-1">{editPayerForm.formState.errors.name.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Payer Code / EDI ID *</label>
              <input {...editPayerForm.register('payerCode', { required: 'Payer code is required' })} className={fieldClassName} />
              {editPayerForm.formState.errors.payerCode && (
                <p className="text-red-500 text-xs mt-1">{editPayerForm.formState.errors.payerCode.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Clearinghouse Code</label>
              <input {...editPayerForm.register('clearinghouseCode')} className={fieldClassName} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Contact Info</label>
              <input {...editPayerForm.register('contactInfo')} className={fieldClassName} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select {...editPayerForm.register('status')} className={fieldClassName}>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={() => setEditingPayer(null)}>
              Cancel
            </Button>
            <Button type="submit">Update Payer</Button>
          </div>
        </form>
      </Dialog>
    </div>
  )
}