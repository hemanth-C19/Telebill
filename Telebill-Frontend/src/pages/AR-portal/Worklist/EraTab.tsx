import { useCallback, useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import apiClient from '../../../api/client'
import { Badge } from '../../../components/shared/ui/Badge'
import { Button } from '../../../components/shared/ui/Button'
import { Card } from '../../../components/shared/ui/Card'
import { Dialog } from '../../../components/shared/ui/Dialog'
import { Table } from '../../../components/shared/ui/Table'

// ── Types ─────────────────────────────────────────────────────────────────────

type RemitRef = {
  remitID: number
  payerID: number
  payerName: string
  batchID: number | null
  payloadUri: string
  receivedDate: string
  status: string // Loaded | Posted | Failed
}

type RegisterEraForm = {
  payerId: string
  batchId: string
  payloadUri: string
  receivedDate: string
}

type UpdateStatusForm = {
  status: string
}

// ── Constants ─────────────────────────────────────────────────────────────────

const REMIT_COLUMNS = [
  { key: 'remitId', label: 'Remit ID' },
  { key: 'payer', label: 'Payer' },
  { key: 'receivedDate', label: 'Received Date' },
  { key: 'payloadUri', label: 'Payload URI' },
  { key: 'batchId', label: 'Batch' },
  { key: 'status', label: 'Status' },
]

const REMIT_STATUSES = ['Loaded', 'Posted', 'Failed']

const PAGE_SIZE = 20

// ── Helpers ───────────────────────────────────────────────────────────────────

function extractErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const resp = (err as { response?: { data?: { error?: string; message?: string } } }).response
    return resp?.data?.error ?? resp?.data?.message ?? 'An unexpected error occurred.'
  }
  return 'An unexpected error occurred.'
}

function fmtDate(s: string | null | undefined): string {
  if (!s) return '—'
  return new Date(s).toLocaleDateString()
}

const inputCls =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

// ── Component ─────────────────────────────────────────────────────────────────

export default function EraTab() {
  const [items, setItems] = useState<RemitRef[]>([])
  const [loading, setLoading] = useState(true)
  const [totalCount, setTotalCount] = useState(0)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const [statusFilter, setStatusFilter] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [page, setPage] = useState(1)

  const [registerOpen, setRegisterOpen] = useState(false)
  const [registering, setRegistering] = useState(false)

  const [statusTarget, setStatusTarget] = useState<RemitRef | null>(null)
  const [updatingStatus, setUpdatingStatus] = useState(false)

  const registerForm = useForm<RegisterEraForm>({
    defaultValues: { payerId: '', batchId: '', payloadUri: '', receivedDate: '' },
  })

  const statusForm = useForm<UpdateStatusForm>({
    defaultValues: { status: '' },
  })

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE) })
      if (statusFilter) params.append('status', statusFilter)
      if (dateFrom) params.append('dateFrom', dateFrom)
      if (dateTo) params.append('dateTo', dateTo)
      const res = await apiClient.get<{ totalCount: number; remitRefs: RemitRef[] }>(
        `api/v1/posting/remits?${params}`,
      )
      setItems(res.data.remitRefs)
      setTotalCount(res.data.totalCount)
    } catch (err) {
      setError(extractErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [page, statusFilter, dateFrom, dateTo])

  useEffect(() => {
    load()
  }, [load])

  async function handleRegister(data: RegisterEraForm) {
    setRegistering(true)
    setError(null)
    try {
      await apiClient.post('api/v1/posting/remits', {
        payerId: Number(data.payerId),
        batchId: data.batchId ? Number(data.batchId) : null,
        payloadUri: data.payloadUri,
        receivedDate: data.receivedDate,
      })
      setRegisterOpen(false)
      registerForm.reset()
      setSuccess('ERA registered successfully.')
      await load()
    } catch (err) {
      setError(extractErrorMessage(err))
    } finally {
      setRegistering(false)
    }
  }

  async function handleUpdateStatus(data: UpdateStatusForm) {
    if (!statusTarget) return
    setUpdatingStatus(true)
    setError(null)
    try {
      await apiClient.patch(`api/v1/posting/remits/${statusTarget.remitID}/status`, {
        status: data.status,
      })
      setSuccess(`Remit #${statusTarget.remitID} updated to "${data.status}".`)
      setStatusTarget(null)
      statusForm.reset()
      await load()
    } catch (err) {
      setError(extractErrorMessage(err))
    } finally {
      setUpdatingStatus(false)
    }
  }

  function openStatusDialog(item: RemitRef) {
    setStatusTarget(item)
    statusForm.setValue('status', item.status)
    setError(null)
    setSuccess(null)
  }

  const totalPages = Math.ceil(totalCount / PAGE_SIZE)

  return (
    <div className="flex flex-col gap-5">
      {error && <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error}</p>}
      {success && (
        <p className="rounded-md bg-green-50 px-4 py-3 text-sm text-green-700">{success}</p>
      )}

      {/* Summary strip */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {[
          { label: 'Total ERA Remits', value: loading ? '—' : String(totalCount), color: 'text-gray-800' },
          { label: 'Loaded (awaiting posting)', value: loading ? '—' : String(items.filter((i) => i.status === 'Loaded').length), color: 'text-blue-600' },
          { label: 'Failed', value: loading ? '—' : String(items.filter((i) => i.status === 'Failed').length), color: 'text-red-600' },
        ].map((card) => (
          <div key={card.label} className="rounded-xl border border-gray-200 bg-white shadow-sm p-4">
            <p className="text-xs text-gray-500 mb-1">{card.label}</p>
            <p className={`text-2xl font-bold ${card.color}`}>{card.value}</p>
          </div>
        ))}
      </div>

      <Card title="ERA / Remit Register">
        {/* Filters + action bar */}
        <div className="flex flex-wrap gap-3 mb-4 items-center">
          <select
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1) }}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Statuses</option>
            {REMIT_STATUSES.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => { setDateFrom(e.target.value); setPage(1) }}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <input
            type="date"
            value={dateTo}
            onChange={(e) => { setDateTo(e.target.value); setPage(1) }}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <div className="ml-auto">
            <Button
              variant="primary"
              onClick={() => {
                setRegisterOpen(true)
                setError(null)
                setSuccess(null)
                registerForm.reset()
              }}
            >
              + Register ERA
            </Button>
          </div>
        </div>

        <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
          <Table
            columns={REMIT_COLUMNS}
            data={items.map((item) => ({
              remitId: `#${item.remitID}`,
              payer: item.payerName,
              receivedDate: fmtDate(item.receivedDate),
              payloadUri: item.payloadUri,
              batchId: item.batchID ? `#${item.batchID}` : null,
              status: <Badge status={item.status} />,
              _remitID: item.remitID,
            }))}
            loading={loading}
            showActions
            actions={[
              {
                label: 'Update Status',
                onClick: (row) => {
                  const item = items.find((i) => i.remitID === (row._remitID as number))
                  if (item) openStatusDialog(item)
                },
              },
            ]}
          />
        </div>

        {totalPages > 1 && (
          <div className="flex items-center justify-between mt-3">
            <p className="text-xs text-gray-400">{totalCount} total remits</p>
            <div className="flex gap-2 items-center">
              <Button variant="secondary" size="sm" disabled={page === 1} onClick={() => setPage((p) => p - 1)}>
                Prev
              </Button>
              <span className="px-3 text-sm text-gray-600">{page} / {totalPages}</span>
              <Button variant="secondary" size="sm" disabled={page === totalPages} onClick={() => setPage((p) => p + 1)}>
                Next
              </Button>
            </div>
          </div>
        )}
      </Card>

      {/* ── Register ERA Dialog ── */}
      <Dialog
        isOpen={registerOpen}
        onClose={() => setRegisterOpen(false)}
        title="Register ERA / Remit"
      >
        <form onSubmit={registerForm.handleSubmit(handleRegister)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Payer ID <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                min={1}
                {...registerForm.register('payerId', { required: 'Payer ID is required' })}
                className={inputCls}
                placeholder="e.g. 1"
              />
              {registerForm.formState.errors.payerId && (
                <p className="mt-1 text-xs text-red-500">
                  {registerForm.formState.errors.payerId.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Batch ID <span className="text-gray-400 font-normal">(optional)</span>
              </label>
              <input
                type="number"
                min={1}
                {...registerForm.register('batchId')}
                className={inputCls}
                placeholder="e.g. 10"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Payload URI <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              {...registerForm.register('payloadUri', { required: 'Payload URI is required' })}
              className={inputCls}
              placeholder="e.g. /era/837/2024-01-15.x12"
            />
            {registerForm.formState.errors.payloadUri && (
              <p className="mt-1 text-xs text-red-500">
                {registerForm.formState.errors.payloadUri.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Received Date <span className="text-red-500">*</span>
            </label>
            <input
              type="date"
              {...registerForm.register('receivedDate', { required: 'Received date is required' })}
              className={inputCls}
            />
            {registerForm.formState.errors.receivedDate && (
              <p className="mt-1 text-xs text-red-500">
                {registerForm.formState.errors.receivedDate.message}
              </p>
            )}
          </div>

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={() => setRegisterOpen(false)}>Cancel</Button>
            <Button variant="primary" type="submit" disabled={registering}>
              {registering ? 'Registering...' : 'Register ERA'}
            </Button>
          </div>
        </form>
      </Dialog>

      {/* ── Update Status Dialog ── */}
      <Dialog
        isOpen={statusTarget !== null}
        onClose={() => setStatusTarget(null)}
        title={`Update Status — Remit #${statusTarget?.remitID}`}
      >
        {statusTarget && (
          <form onSubmit={statusForm.handleSubmit(handleUpdateStatus)} className="space-y-4">
            <div className="rounded-lg bg-gray-50 px-4 py-3 text-sm space-y-1.5">
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Payer</span>
                <strong>{statusTarget.payerName}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Received</span>
                <strong>{fmtDate(statusTarget.receivedDate)}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Current</span>
                <Badge status={statusTarget.status} />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                New Status <span className="text-red-500">*</span>
              </label>
              <select
                {...statusForm.register('status', { required: 'Status is required' })}
                className={inputCls}
              >
                <option value="">Select status…</option>
                {REMIT_STATUSES.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
              {statusForm.formState.errors.status && (
                <p className="mt-1 text-xs text-red-500">
                  {statusForm.formState.errors.status.message}
                </p>
              )}
            </div>

            <div className="flex justify-end gap-2 pt-1">
              <Button variant="secondary" onClick={() => setStatusTarget(null)}>Cancel</Button>
              <Button variant="primary" type="submit" disabled={updatingStatus}>
                {updatingStatus ? 'Updating…' : 'Update Status'}
              </Button>
            </div>
          </form>
        )}
      </Dialog>
    </div>
  )
}
