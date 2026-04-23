import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import apiClient from '../../api/client'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Dialog } from '../../components/shared/ui/Dialog'
import { Pagination } from '../../components/shared/ui/Pagination'
import { Table } from '../../components/shared/ui/Table'

type BatchSummary = {
  batchID: number
  batchDate: string
  itemCount: number
  totalCharge: number
  status: string
}

const inputClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

const PAGE_SIZE = 10

export default function BatchList() {
  const navigate = useNavigate()
  const [batches, setBatches] = useState<BatchSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [statusFilter, setStatusFilter] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [showCreateDialog, setShowCreateDialog] = useState(false)
  const [newBatchDate, setNewBatchDate] = useState('')
  const [creating, setCreating] = useState(false)
  const [createError, setCreateError] = useState<string | null>(null)

  const fetchBatches = useCallback(async (status: string, from: string, to: string) => {
    setLoading(true)
    setError(null)
    try {
      const params = new URLSearchParams({ pageSize: '200' })
      if (status) params.set('status', status)
      if (from) params.set('dateFrom', from)
      if (to) params.set('dateTo', to)
      const res = await apiClient.get<{ totalCount: number; batches: BatchSummary[] }>(
        `api/v1/batch?${params.toString()}`,
      )
      setBatches(res.data.batches)
    } catch {
      setError('Failed to load batches.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchBatches('', '', '')
  }, [fetchBatches])

  function handleStatusChange(e: React.ChangeEvent<HTMLSelectElement>) {
    const s = e.target.value
    setStatusFilter(s)
    setCurrentPage(1)
    fetchBatches(s, dateFrom, dateTo)
  }

  function handleDateFromChange(e: React.ChangeEvent<HTMLInputElement>) {
    const d = e.target.value
    setDateFrom(d)
    setCurrentPage(1)
    fetchBatches(statusFilter, d, dateTo)
  }

  function handleDateToChange(e: React.ChangeEvent<HTMLInputElement>) {
    const d = e.target.value
    setDateTo(d)
    setCurrentPage(1)
    fetchBatches(statusFilter, dateFrom, d)
  }

  function handleClear() {
    setStatusFilter('')
    setDateFrom('')
    setDateTo('')
    setCurrentPage(1)
    fetchBatches('', '', '')
  }

  async function handleCreateBatch() {
    if (!newBatchDate) return
    setCreating(true)
    setCreateError(null)
    try {
      await apiClient.post('api/v1/batch', { batchDate: newBatchDate })
      setShowCreateDialog(false)
      setNewBatchDate('')
      setCurrentPage(1)
      fetchBatches(statusFilter, dateFrom, dateTo)
    } catch {
      setCreateError('Failed to create batch. Please try again.')
    } finally {
      setCreating(false)
    }
  }

  const totalPages = Math.max(1, Math.ceil(batches.length / PAGE_SIZE))
  const safeCurrentPage = Math.min(currentPage, totalPages)
  const paginated = batches.slice((safeCurrentPage - 1) * PAGE_SIZE, safeCurrentPage * PAGE_SIZE)

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Batch Management</h1>
        <Button variant="primary" onClick={() => setShowCreateDialog(true)}>
          Create Batch
        </Button>
      </div>

      {error != null && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error}</p>
      )}

      <div className="flex flex-wrap items-end gap-3 mb-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
          <select value={statusFilter} onChange={handleStatusChange} className={inputClassName}>
            <option value="">All</option>
            <option value="Open">Open</option>
            <option value="Generated">Generated</option>
            <option value="Submitted">Submitted</option>
            <option value="Acked">Acked</option>
            <option value="Failed">Failed</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Date From</label>
          <input type="date" value={dateFrom} onChange={handleDateFromChange} className={inputClassName} />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Date To</label>
          <input type="date" value={dateTo} onChange={handleDateToChange} className={inputClassName} />
        </div>
        <Button variant="secondary" size="sm" onClick={handleClear}>
          Clear
        </Button>
      </div>

      <Table
        columns={[
          { key: 'batchIdText', label: 'Batch ID' },
          { key: 'batchDate', label: 'Batch Date' },
          { key: 'itemCount', label: 'Claims' },
          { key: 'totalChargeText', label: 'Total Charge' },
          { key: 'statusBadge', label: 'Status' },
          { key: 'view', label: 'View' },
        ]}
        loading={loading}
        data={paginated.map((row) => ({
          ...row,
          batchIdText: `#${row.batchID}`,
          totalChargeText: `$${row.totalCharge.toFixed(2)}`,
          statusBadge: <Badge status={row.status} />,
          view: (
            <button
              type="button"
              className="text-blue-600 text-sm hover:underline cursor-pointer"
              onClick={() => navigate(`/frontdesk/batch-detail/${row.batchID}`)}
            >
              View
            </button>
          ),
        }))}
        showActions={true}
        actions={[
          {
            label: 'Open Detail',
            onClick: (row) => navigate(`/frontdesk/batch-detail/${(row as BatchSummary).batchID}`),
          },
        ]}
      />

      {!loading && batches.length === 0 && error == null && (
        <p className="text-center text-sm text-gray-400 py-8">No batches found.</p>
      )}

      <Pagination currentPage={safeCurrentPage} totalPages={totalPages} onPageChange={setCurrentPage} />

      <Dialog
        isOpen={showCreateDialog}
        onClose={() => {
          setShowCreateDialog(false)
          setCreateError(null)
          setNewBatchDate('')
        }}
        title="Create New Batch"
        maxWidth="sm"
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Batch Date *</label>
            <input
              type="date"
              value={newBatchDate}
              onChange={(e) => setNewBatchDate(e.target.value)}
              className={inputClassName}
            />
          </div>
          {createError != null && <p className="text-sm text-red-600">{createError}</p>}
          <div className="flex justify-end gap-2">
            <Button
              variant="secondary"
              onClick={() => {
                setShowCreateDialog(false)
                setCreateError(null)
                setNewBatchDate('')
              }}
              disabled={creating}
            >
              Cancel
            </Button>
            <Button variant="primary" onClick={handleCreateBatch} disabled={!newBatchDate || creating}>
              {creating ? 'Creating...' : 'Create'}
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  )
}
