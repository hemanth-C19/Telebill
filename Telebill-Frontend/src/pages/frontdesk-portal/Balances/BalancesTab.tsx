import { useCallback, useEffect, useState } from 'react'
import apiClient from '../../../api/client'
import { Badge } from '../../../components/shared/ui/Badge'
import { Button } from '../../../components/shared/ui/Button'
import { Card } from '../../../components/shared/ui/Card'
import { Dialog } from '../../../components/shared/ui/Dialog'
import { Table } from '../../../components/shared/ui/Table'

type PatientBalance = {
  balanceID: number
  patientID: number
  patientName: string
  mrn: string
  claimID: number
  amountDue: number
  agingBucket: string
  lastStatementDate: string | null
  status: string
}

type PatientBalanceListResponse = {
  totalCount: number
  totalAmountDue: number
  balances: PatientBalance[]
}

type AgingSummary = {
  bucket0To30: number
  bucket31To60: number
  bucket61To90: number
  bucket90Plus: number
  totalOutstanding: number
  openBalanceCount: number
}

type AgingJobResult = {
  jobID: string
  runAt: string
  balancesUpdated: number
  durationSeconds: number
}

type Filters = {
  patientID: string
  agingBucket: string
  status: string
  minAmount: string
  maxAmount: string
}

function extractErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const resp = (err as { response?: { data?: { error?: string; message?: string } } }).response
    return resp?.data?.error ?? resp?.data?.message ?? 'An unexpected error occurred.'
  }
  return 'An unexpected error occurred.'
}

const inputCls =
  'border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'
const PAGE_SIZE = 25

const EMPTY_FILTERS: Filters = {
  patientID: '',
  agingBucket: '',
  status: 'Open',
  minAmount: '',
  maxAmount: '',
}

export default function BalancesTab() {
  const [aging, setAging] = useState<AgingSummary | null>(null)
  const [balances, setBalances] = useState<PatientBalance[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [actionError, setActionError] = useState<string | null>(null)
  const [actionSuccess, setActionSuccess] = useState<string | null>(null)
  const [page, setPage] = useState(1)

  // Form inputs (uncommitted)
  const [form, setForm] = useState<Filters>(EMPTY_FILTERS)
  // Committed filters (what drives the query)
  const [applied, setApplied] = useState<Filters>(EMPTY_FILTERS)

  // Update status dialog
  const [updateTarget, setUpdateTarget] = useState<PatientBalance | null>(null)
  const [updateStatus, setUpdateStatus] = useState<'Paid' | 'WrittenOff'>('Paid')
  const [updateReason, setUpdateReason] = useState('')
  const [updating, setUpdating] = useState(false)

  // Run aging job dialog
  const [showAgingJob, setShowAgingJob] = useState(false)
  const [agingKey, setAgingKey] = useState('')
  const [runningJob, setRunningJob] = useState(false)
  const [jobResult, setJobResult] = useState<AgingJobResult | null>(null)

  const loadData = useCallback(async () => {
    setLoading(true)
    setActionError(null)
    try {
      const params = new URLSearchParams()
      if (applied.patientID) params.set('patientID', applied.patientID)
      if (applied.agingBucket) params.set('agingBucket', applied.agingBucket)
      if (applied.status) params.set('status', applied.status)
      if (applied.minAmount) params.set('minAmount', applied.minAmount)
      if (applied.maxAmount) params.set('maxAmount', applied.maxAmount)
      params.set('page', String(page))
      params.set('pageSize', String(PAGE_SIZE))

      const [agingRes, listRes] = await Promise.all([
        apiClient.get<AgingSummary>('api/v1/posting/balances/aging-summary'),
        apiClient.get<PatientBalanceListResponse>(`api/v1/posting/balances?${params}`),
      ])
      setAging(agingRes.data)
      setBalances(listRes.data.balances)
      setTotalCount(listRes.data.totalCount)
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [applied, page])

  useEffect(() => {
    loadData()
  }, [loadData])

  function handleApply() {
    setApplied(form)
    setPage(1)
  }

  function handleReset() {
    setForm(EMPTY_FILTERS)
    setApplied(EMPTY_FILTERS)
    setPage(1)
  }

  function openUpdateDialog(balance: PatientBalance, status: 'Paid' | 'WrittenOff') {
    setUpdateTarget(balance)
    setUpdateStatus(status)
    setUpdateReason('')
    setActionError(null)
    setActionSuccess(null)
  }

  async function handleUpdateStatus() {
    if (!updateTarget) return
    setUpdating(true)
    setActionError(null)
    try {
      await apiClient.patch(`api/v1/posting/balances/${updateTarget.balanceID}/status`, {
        status: updateStatus,
        reason: updateReason || null,
      })
      setUpdateTarget(null)
      setActionSuccess(`Balance for ${updateTarget.patientName} marked as ${updateStatus}.`)
      await loadData()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setUpdating(false)
    }
  }

  async function handleRunAgingJob() {
    setRunningJob(true)
    setActionError(null)
    try {
      const res = await apiClient.post<AgingJobResult>(
        `api/v1/posting/balances/run-aging-job${agingKey ? `?schedulerKey=${encodeURIComponent(agingKey)}` : ''}`,
      )
      setJobResult(res.data)
      await loadData()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setRunningJob(false)
    }
  }

  const totalPages = Math.ceil(totalCount / PAGE_SIZE)

  const agingBuckets = aging
    ? [
        { label: '0–30 days', value: aging.bucket0To30, color: 'text-green-700' },
        { label: '31–60 days', value: aging.bucket31To60, color: 'text-yellow-700' },
        { label: '61–90 days', value: aging.bucket61To90, color: 'text-orange-600' },
        { label: '90+ days', value: aging.bucket90Plus, color: 'text-red-700' },
        { label: 'Total Outstanding', value: aging.totalOutstanding, color: 'text-blue-700', highlight: true },
      ]
    : []

  return (
    <div className="flex flex-col gap-6">
      {actionError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{actionError}</p>
      )}
      {actionSuccess && (
        <p className="rounded-md bg-green-50 px-4 py-3 text-sm text-green-700">{actionSuccess}</p>
      )}

      {/* Aging Summary Bar */}
      <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
        {aging == null
          ? Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="rounded-xl border border-gray-200 bg-white shadow-sm p-6 animate-pulse">
                <div className="h-3 w-16 bg-gray-200 rounded mb-3" />
                <div className="h-6 w-24 bg-gray-200 rounded" />
              </div>
            ))
          : agingBuckets.map((b) => (
              <div
                key={b.label}
                className={`rounded-xl border shadow-sm p-4 ${
                  b.highlight ? 'border-blue-200 bg-blue-50' : 'border-gray-200 bg-white'
                }`}
              >
                <p className="text-xs text-gray-500 mb-1">{b.label}</p>
                <p className={`text-xl font-bold ${b.color}`}>${b.value.toFixed(2)}</p>
                {b.highlight && aging && (
                  <p className="text-xs text-blue-500 mt-1">{aging.openBalanceCount} open</p>
                )}
              </div>
            ))}
      </div>

      {/* Filters + Table */}
      <Card title="Patient Balances">
        <div className="flex flex-wrap items-end gap-3 mb-5">
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Patient ID</label>
            <input
              type="number"
              placeholder="e.g. 42"
              value={form.patientID}
              onChange={(e) => setForm((f) => ({ ...f, patientID: e.target.value }))}
              className={`w-28 ${inputCls}`}
            />
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Aging Bucket</label>
            <select
              value={form.agingBucket}
              onChange={(e) => setForm((f) => ({ ...f, agingBucket: e.target.value }))}
              className={inputCls}
            >
              <option value="">All</option>
              <option value="0-30">0–30 days</option>
              <option value="31-60">31–60 days</option>
              <option value="61-90">61–90 days</option>
              <option value="90+">90+ days</option>
            </select>
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Status</label>
            <select
              value={form.status}
              onChange={(e) => setForm((f) => ({ ...f, status: e.target.value }))}
              className={inputCls}
            >
              <option value="">All</option>
              <option value="Open">Open</option>
              <option value="Paid">Paid</option>
              <option value="WrittenOff">Written Off</option>
            </select>
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Min $</label>
            <input
              type="number"
              placeholder="0"
              value={form.minAmount}
              onChange={(e) => setForm((f) => ({ ...f, minAmount: e.target.value }))}
              className={`w-24 ${inputCls}`}
            />
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Max $</label>
            <input
              type="number"
              placeholder="9999"
              value={form.maxAmount}
              onChange={(e) => setForm((f) => ({ ...f, maxAmount: e.target.value }))}
              className={`w-24 ${inputCls}`}
            />
          </div>
          <Button variant="primary" size="sm" onClick={handleApply}>
            Apply
          </Button>
          <Button variant="secondary" size="sm" onClick={handleReset}>
            Reset
          </Button>
          <div className="ml-auto">
            <Button
              variant="secondary"
              size="sm"
              onClick={() => {
                setShowAgingJob(true)
                setJobResult(null)
                setAgingKey('')
              }}
            >
              Run Aging Job
            </Button>
          </div>
        </div>

        <Table
          columns={[
            { key: 'patientName', label: 'Patient Name' },
            { key: 'mrnText', label: 'MRN' },
            { key: 'claimIDText', label: 'Claim ID' },
            { key: 'amountDueText', label: 'Amount Due' },
            { key: 'agingBucket', label: 'Aging Bucket' },
            { key: 'lastStatementDateText', label: 'Last Statement' },
            { key: 'statusBadge', label: 'Status' },
          ]}
          data={balances.map((b) => ({
            ...b,
            mrnText: b.mrn || '—',
            claimIDText: `#${b.claimID}`,
            amountDueText: `$${b.amountDue.toFixed(2)}`,
            lastStatementDateText: b.lastStatementDate ?? '—',
            statusBadge: <Badge status={b.status} />,
          }))}
          loading={loading}
          showActions={true}
          actions={[
            {
              label: 'Mark Paid',
              onClick: (row) => openUpdateDialog(row as PatientBalance, 'Paid'),
            },
            {
              label: 'Write Off',
              variant: 'danger',
              onClick: (row) => openUpdateDialog(row as PatientBalance, 'WrittenOff'),
            },
          ]}
        />

        {totalPages > 1 && (
          <div className="flex items-center justify-between mt-4 text-sm text-gray-500">
            <span>
              Page {page} of {totalPages} &mdash; {totalCount} records
            </span>
            <div className="flex gap-2">
              <Button variant="secondary" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                ← Prev
              </Button>
              <Button
                variant="secondary"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                Next →
              </Button>
            </div>
          </div>
        )}
      </Card>

      {/* Update Status Dialog */}
      <Dialog
        isOpen={updateTarget !== null}
        onClose={() => setUpdateTarget(null)}
        title={`Update Balance — ${updateTarget?.patientName ?? ''}`}
      >
        <div className="space-y-4">
          <div>
            <p className="text-sm text-gray-500 mb-3">
              Balance ID #{updateTarget?.balanceID} &nbsp;|&nbsp; Amount Due:{' '}
              <strong>${updateTarget?.amountDue.toFixed(2)}</strong>
            </p>
            <label className="block text-sm font-medium text-gray-700 mb-2">New Status</label>
            <div className="flex gap-3">
              {(['Paid', 'WrittenOff'] as const).map((s) => (
                <label
                  key={s}
                  className={`flex items-center gap-2 px-4 py-2 rounded-lg border cursor-pointer text-sm font-medium ${
                    updateStatus === s
                      ? s === 'Paid'
                        ? 'bg-green-50 border-green-500 text-green-700'
                        : 'bg-red-50 border-red-500 text-red-700'
                      : 'border-gray-300 text-gray-600'
                  }`}
                >
                  <input
                    type="radio"
                    name="balanceStatus"
                    value={s}
                    checked={updateStatus === s}
                    onChange={() => setUpdateStatus(s)}
                    className="sr-only"
                  />
                  {s === 'Paid' ? '✓ Paid' : '✗ Write Off'}
                </label>
              ))}
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Reason (optional)</label>
            <input
              value={updateReason}
              onChange={(e) => setUpdateReason(e.target.value)}
              placeholder="e.g. Patient paid in cash"
              className={`w-full ${inputCls}`}
            />
          </div>
          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={() => setUpdateTarget(null)}>
              Cancel
            </Button>
            <Button variant="primary" disabled={updating} onClick={handleUpdateStatus}>
              {updating ? 'Updating...' : 'Confirm'}
            </Button>
          </div>
        </div>
      </Dialog>

      {/* Run Aging Job Dialog */}
      <Dialog
        isOpen={showAgingJob}
        onClose={() => setShowAgingJob(false)}
        title="Run Aging Bucket Job"
      >
        <div className="space-y-4">
          {jobResult == null ? (
            <>
              <p className="text-sm text-gray-500">
                Recalculates aging buckets (0–30, 31–60, 61–90, 90+) for all open patient balances.
                Requires a scheduler key.
              </p>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Scheduler Key *</label>
                <input
                  value={agingKey}
                  onChange={(e) => setAgingKey(e.target.value)}
                  placeholder="Enter scheduler key"
                  className={`w-full ${inputCls}`}
                />
              </div>
              <div className="flex justify-end gap-2 pt-1">
                <Button variant="secondary" onClick={() => setShowAgingJob(false)}>
                  Cancel
                </Button>
                <Button variant="primary" disabled={!agingKey || runningJob} onClick={handleRunAgingJob}>
                  {runningJob ? 'Running...' : 'Run Job'}
                </Button>
              </div>
            </>
          ) : (
            <>
              <div className="rounded-lg bg-green-50 border border-green-200 p-4 space-y-2 text-sm">
                <p className="font-semibold text-green-700">Aging job completed</p>
                <p className="text-gray-600">
                  Balances updated: <strong>{jobResult.balancesUpdated}</strong>
                </p>
                <p className="text-gray-600">
                  Duration: <strong>{jobResult.durationSeconds.toFixed(2)}s</strong>
                </p>
                <p className="text-gray-600">
                  Run at: <strong>{new Date(jobResult.runAt).toLocaleString()}</strong>
                </p>
              </div>
              <div className="flex justify-end">
                <Button variant="primary" onClick={() => setShowAgingJob(false)}>
                  Done
                </Button>
              </div>
            </>
          )}
        </div>
      </Dialog>
    </div>
  )
}
