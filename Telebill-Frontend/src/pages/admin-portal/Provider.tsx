import { useState } from 'react'
import { useForm } from 'react-hook-form'
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

type ProviderFormValues = {
  name: string
  npi: string
  taxonomy: string
  telehealthEnrolled: boolean
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

const fieldClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

export default function ProviderManagement() {
  const [providers, setProviders] = useState<Provider[]>(DUMMY_PROVIDERS)
  const [showAddProvider, setShowAddProvider] = useState(false)
  const [editingProvider, setEditingProvider] = useState<Provider | null>(null)

  const addProviderForm = useForm<ProviderFormValues>({
    defaultValues: { status: 'Active', telehealthEnrolled: true },
  })
  const editProviderForm = useForm<ProviderFormValues>()

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

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Providers</h1>
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
  )
}