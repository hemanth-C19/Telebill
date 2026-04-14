import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Dialog } from '../../components/shared/ui/Dialog'
import { Pagination } from '../../components/shared/ui/Pagination'
import { Table } from '../../components/shared/ui/Table'

type BatchStatus = 'Open' | 'Generated' | 'Submitted' | 'Acked' | 'Failed'

type Batch = {
  batchId: number
  batchDate: string
  itemCount: number
  totalCharge: number
  status: BatchStatus
}

const DUMMY_BATCHES: Batch[] = [
  { batchId: 3001, batchDate: '2024-11-10', itemCount: 0, totalCharge: 0, status: 'Open' },
  { batchId: 3002, batchDate: '2024-11-12', itemCount: 3, totalCharge: 595.0, status: 'Open' },
  { batchId: 3003, batchDate: '2024-11-15', itemCount: 2, totalCharge: 335.0, status: 'Generated' },
  { batchId: 3004, batchDate: '2024-11-18', itemCount: 4, totalCharge: 820.0, status: 'Submitted' },
  { batchId: 3005, batchDate: '2024-11-20', itemCount: 3, totalCharge: 505.0, status: 'Acked' },
  { batchId: 3006, batchDate: '2024-11-22', itemCount: 2, totalCharge: 280.0, status: 'Failed' },
]

const inputClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

export default function BatchList() {
  const [batches, setBatches] = useState<Batch[]>(DUMMY_BATCHES)
  const [statusFilter, setStatusFilter] = useState('All')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [showCreateDialog, setShowCreateDialog] = useState(false)
  const [batchDate, setBatchDate] = useState('')
  const navigate = useNavigate()
  const PAGE_SIZE = 10

  const filtered = useMemo(
    () =>
      batches
        .filter((b) => statusFilter === 'All' || b.status === statusFilter)
        .filter((b) => !dateFrom || b.batchDate >= dateFrom)
        .filter((b) => !dateTo || b.batchDate <= dateTo),
    [batches, statusFilter, dateFrom, dateTo],
  )

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
  const safeCurrentPage = Math.min(currentPage, totalPages)
  const paginated = filtered.slice((safeCurrentPage - 1) * PAGE_SIZE, safeCurrentPage * PAGE_SIZE)

  const handleCreateBatch = () => {
    if (!batchDate) return
    const newBatch: Batch = {
      batchId: Math.floor(Math.random() * 9000) + 4000,
      batchDate,
      itemCount: 0,
      totalCharge: 0,
      status: 'Open',
    }
    setBatches((prev) => [newBatch, ...prev])
    setShowCreateDialog(false)
    setBatchDate('')
    setCurrentPage(1)
  }

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Batch Management</h1>
        <Button variant="primary" onClick={() => setShowCreateDialog(true)}>
          Create Batch
        </Button>
      </div>

      <div className="flex flex-wrap items-end gap-3 mb-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value)
              setCurrentPage(1)
            }}
            className={inputClassName}
          >
            <option value="All">All</option>
            <option value="Open">Open</option>
            <option value="Generated">Generated</option>
            <option value="Submitted">Submitted</option>
            <option value="Acked">Acked</option>
            <option value="Failed">Failed</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Date From</label>
          <input type="date" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} className={inputClassName} />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Date To</label>
          <input type="date" value={dateTo} onChange={(e) => setDateTo(e.target.value)} className={inputClassName} />
        </div>
        <Button
          variant="secondary"
          size="sm"
          onClick={() => {
            setStatusFilter('All')
            setDateFrom('')
            setDateTo('')
            setCurrentPage(1)
          }}
        >
          Clear
        </Button>
      </div>

      <Table
        columns={[
          { key: 'batchIdText', label: 'Batch ID' },
          { key: 'batchDate', label: 'Batch Date' },
          { key: 'itemCount', label: 'Claims' },
          { key: 'totalChargeText', label: 'Total Charge' },
          { key: 'status', label: 'Status' },
          { key: 'view', label: 'View' },
        ]}
        data={paginated.map((row) => ({
          ...row,
          batchIdText: `#${row.batchId}`,
          totalChargeText: `$${row.totalCharge.toFixed(2)}`,
          status: <Badge status={row.status} />,
          view: (
            <button
              type="button"
              className="text-blue-600 text-sm hover:underline cursor-pointer"
              onClick={() => navigate(`/frontdesk/batch-detail/${row.batchId}`)}
            >
              View
            </button>
          ),
        }))}
        showActions={true}
        actions={[
          {
            label: 'Open Detail',
            onClick: (row) => navigate(`/frontdesk/batch-detail/${(row as Batch).batchId}`),
          },
        ]}
      />

      <Pagination currentPage={safeCurrentPage} totalPages={totalPages} onPageChange={setCurrentPage} />

      <Dialog isOpen={showCreateDialog} onClose={() => setShowCreateDialog(false)} title="Create New Batch" maxWidth="sm">
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Batch Date *</label>
            <input type="date" value={batchDate} onChange={(e) => setBatchDate(e.target.value)} className={inputClassName} />
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="secondary" onClick={() => setShowCreateDialog(false)}>
              Cancel
            </Button>
            <Button variant="primary" onClick={handleCreateBatch}>
              Create
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  )
}
