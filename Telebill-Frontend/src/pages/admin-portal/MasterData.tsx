import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useLocation, useNavigate } from 'react-router-dom'
import { Button } from '../../components/shared/ui/Button'
import { Table } from '../../components/shared/ui/Table'
import { Dialog } from '../../components/shared/ui/Dialog'
import { Badge } from '../../components/shared/ui/Badge'

type Provider = {
  providerId: number
  npi: string
  name: string
  taxonomy: string
  telehealthEnrolled: boolean
  contactInfo: string
  status: string
}

type Payer = {
  payerId: number
  name: string
  payerCode: string
  clearinghouseCode: string
  contactInfo: string
  status: string
}

type ProviderFormValues = {
  name: string
  npi: string
  taxonomy: string
  telehealthEnrolled: boolean
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

const DUMMY_PROVIDERS: Provider[] = [
  { providerId: 201, npi: '1234567890', name: 'Dr. Sarah Chen', taxonomy: 'Internal Medicine', telehealthEnrolled: true, contactInfo: '555-2001', status: 'Active' },
  { providerId: 202, npi: '2345678901', name: 'Dr. James Patel', taxonomy: 'Psychiatry', telehealthEnrolled: true, contactInfo: '555-2002', status: 'Active' },
  { providerId: 203, npi: '3456789012', name: 'Dr. Mark Liu', taxonomy: 'Cardiology', telehealthEnrolled: true, contactInfo: '555-2003', status: 'Active' },
  { providerId: 204, npi: '4567890123', name: 'Dr. Priya Sharma', taxonomy: 'Endocrinology', telehealthEnrolled: false, contactInfo: '555-2004', status: 'Inactive' },
  { providerId: 205, npi: '5678901234', name: 'Dr. Robert Adams', taxonomy: 'Neurology', telehealthEnrolled: true, contactInfo: '555-2005', status: 'Active' },
]

const DUMMY_PAYERS: Payer[] = [
  { payerId: 10, name: 'BlueCross BlueShield', payerCode: 'BCBS01', clearinghouseCode: 'CHC-BC01', contactInfo: '800-123-4567', status: 'Active' },
  { payerId: 20, name: 'Aetna', payerCode: 'AET001', clearinghouseCode: 'CHC-AE01', contactInfo: '800-234-5678', status: 'Active' },
  { payerId: 30, name: 'United Healthcare', payerCode: 'UHC001', clearinghouseCode: 'CHC-UH01', contactInfo: '800-345-6789', status: 'Active' },
  { payerId: 40, name: 'Cigna', payerCode: 'CGN001', clearinghouseCode: 'CHC-CG01', contactInfo: '800-456-7890', status: 'Inactive' },
]

const fieldClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

export default function MasterData() {
  const location = useLocation()
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState<'providers' | 'payers'>('providers')
  const [providers, setProviders] = useState<Provider[]>(DUMMY_PROVIDERS)
  const [showAddProvider, setShowAddProvider] = useState(false)
  const [editingProvider, setEditingProvider] = useState<Provider | null>(null)
  const [payers, setPayers] = useState<Payer[]>(DUMMY_PAYERS)
  const [showAddPayer, setShowAddPayer] = useState(false)
  const [editingPayer, setEditingPayer] = useState<Payer | null>(null)

  const addProviderForm = useForm<ProviderFormValues>({
    defaultValues: { status: 'Active', telehealthEnrolled: true },
  })
  const editProviderForm = useForm<ProviderFormValues>()
  const addPayerForm = useForm<PayerFormValues>({ defaultValues: { status: 'Active' } })
  const editPayerForm = useForm<PayerFormValues>()

  useEffect(() => {
    const tab = (location.state as { tab?: 'providers' | 'payers' } | undefined)?.tab
    if (tab === 'providers' || tab === 'payers') {
      setActiveTab(tab)
    }
  }, [location.state])

  const onAddProvider = addProviderForm.handleSubmit((data) => {
    const newProvider: Provider = {
      providerId: Math.floor(Math.random() * 9000) + 300,
      npi: data.npi,
      name: data.name,
      taxonomy: data.taxonomy,
      telehealthEnrolled: data.telehealthEnrolled,
      contactInfo: data.contactInfo,
      status: data.status,
    }
    setProviders((prev) => [newProvider, ...prev])
    setShowAddProvider(false)
    addProviderForm.reset()
  })

  const onEditProvider = editProviderForm.handleSubmit((data) => {
    if (editingProvider == null) return
    setProviders((prev) => prev.map((p) => (p.providerId === editingProvider.providerId ? { ...p, ...data } : p)))
    setEditingProvider(null)
  })

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
      <h1 className="text-2xl font-bold text-gray-900">Master Data</h1>

      <div className="flex border-b mb-6">
        {['Providers', 'Payers'].map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab.toLowerCase() as 'providers' | 'payers')}
            className={`px-5 py-2 font-medium text-sm border-b-2 transition-colors ${activeTab === tab.toLowerCase() ? 'border-blue-500 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700'}`}
            type="button"
          >
            {tab}
          </button>
        ))}
      </div>

      {activeTab === 'providers' && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-semibold text-gray-900">Providers</h2>
            <Button variant="primary" size="sm" onClick={() => setShowAddProvider(true)}>
              Add Provider
            </Button>
          </div>

          <Table
            columns={[
              { key: 'npi', label: 'NPI' },
              { key: 'name', label: 'Name' },
              { key: 'taxonomy', label: 'Specialty' },
              { key: 'telehealth', label: 'Telehealth' },
              { key: 'contactInfo', label: 'Contact' },
              { key: 'status', label: 'Status' },
            ]}
            data={providers.map((row) => ({
              ...row,
              telehealth: (
                <span className={row.telehealthEnrolled ? 'text-green-600 font-medium' : 'text-gray-500'}>
                  {row.telehealthEnrolled ? 'Yes' : 'No'}
                </span>
              ),
              status: <Badge status={row.status} />,
            }))}
            showActions={true}
            actions={[
              {
                label: 'Edit',
                onClick: (row) => {
                  const selected = row as Provider
                  setEditingProvider(selected)
                  editProviderForm.reset({
                    name: selected.name,
                    npi: selected.npi,
                    taxonomy: selected.taxonomy,
                    telehealthEnrolled: selected.telehealthEnrolled ?? false,
                    contactInfo: selected.contactInfo,
                    status: selected.status,
                  })
                },
              },
              {
                label: 'Delete',
                variant: 'danger',
                onClick: (row) => {
                  const selected = row as Provider
                  setProviders((prev) => prev.filter((p) => p.providerId !== selected.providerId))
                },
              },
            ]}
          />

          <Dialog
            isOpen={showAddProvider}
            onClose={() => {
              setShowAddProvider(false)
              addProviderForm.reset()
            }}
            title="Add Provider"
            maxWidth="lg"
          >
            <form className="space-y-4" onSubmit={onAddProvider}>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Name *</label>
                  <input {...addProviderForm.register('name', { required: 'Name is required' })} className={fieldClassName} />
                  {addProviderForm.formState.errors.name && (
                    <p className="text-red-500 text-xs mt-1">{addProviderForm.formState.errors.name.message}</p>
                  )}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">NPI *</label>
                  <input
                    {...addProviderForm.register('npi', {
                      required: 'NPI is required',
                      validate: (value) => /^\d{10}$/.test(value) || 'NPI must be exactly 10 digits',
                    })}
                    className={fieldClassName}
                  />
                  {addProviderForm.formState.errors.npi && (
                    <p className="text-red-500 text-xs mt-1">{addProviderForm.formState.errors.npi.message}</p>
                  )}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Specialty / Taxonomy</label>
                  <input {...addProviderForm.register('taxonomy')} className={fieldClassName} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Contact Info</label>
                  <input {...addProviderForm.register('contactInfo')} className={fieldClassName} />
                </div>
                <div className="flex items-center gap-2">
                  <input id="add-telehealth" type="checkbox" {...addProviderForm.register('telehealthEnrolled')} />
                  <label htmlFor="add-telehealth" className="text-sm font-medium text-gray-700">
                    Telehealth Enrolled
                  </label>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                  <select {...addProviderForm.register('status')} className={fieldClassName}>
                    <option value="Active">Active</option>
                    <option value="Inactive">Inactive</option>
                  </select>
                </div>
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <Button
                  variant="secondary"
                  onClick={() => {
                    setShowAddProvider(false)
                    addProviderForm.reset()
                  }}
                >
                  Cancel
                </Button>
                <Button type="submit">Save Provider</Button>
              </div>
            </form>
          </Dialog>

          <Dialog isOpen={editingProvider !== null} onClose={() => setEditingProvider(null)} title="Edit Provider" maxWidth="lg">
            <form className="space-y-4" onSubmit={onEditProvider}>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Name *</label>
                  <input {...editProviderForm.register('name', { required: 'Name is required' })} className={fieldClassName} />
                  {editProviderForm.formState.errors.name && (
                    <p className="text-red-500 text-xs mt-1">{editProviderForm.formState.errors.name.message}</p>
                  )}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">NPI *</label>
                  <input
                    {...editProviderForm.register('npi', {
                      required: 'NPI is required',
                      validate: (value) => /^\d{10}$/.test(value) || 'NPI must be exactly 10 digits',
                    })}
                    className={fieldClassName}
                  />
                  {editProviderForm.formState.errors.npi && (
                    <p className="text-red-500 text-xs mt-1">{editProviderForm.formState.errors.npi.message}</p>
                  )}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Specialty / Taxonomy</label>
                  <input {...editProviderForm.register('taxonomy')} className={fieldClassName} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Contact Info</label>
                  <input {...editProviderForm.register('contactInfo')} className={fieldClassName} />
                </div>
                <div className="flex items-center gap-2">
                  <input id="edit-telehealth" type="checkbox" {...editProviderForm.register('telehealthEnrolled')} />
                  <label htmlFor="edit-telehealth" className="text-sm font-medium text-gray-700">
                    Telehealth Enrolled
                  </label>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                  <select {...editProviderForm.register('status')} className={fieldClassName}>
                    <option value="Active">Active</option>
                    <option value="Inactive">Inactive</option>
                  </select>
                </div>
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <Button variant="secondary" onClick={() => setEditingProvider(null)}>
                  Cancel
                </Button>
                <Button type="submit">Update Provider</Button>
              </div>
            </form>
          </Dialog>
        </div>
      )}

      {activeTab === 'payers' && (
        <div className="space-y-4">
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
      )}
    </div>
  )
}
