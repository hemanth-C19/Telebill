import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import apiClient from '../../../api/client'
import { Badge } from '../../../components/shared/ui/Badge'
import { Button } from '../../../components/shared/ui/Button'
import { Card } from '../../../components/shared/ui/Card'
import { Dialog } from '../../../components/shared/ui/Dialog'

// ── Types ─────────────────────────────────────────────────────────────────────

type ArWorklistItem = {
  denialId: number
  claimId: number
  patientName: string | null
  payerName: string | null
  planName: string | null
  encounterDateTime: string
  totalCharge: number
  amountDenied: number
  reasonCode: string | null
  remarkCode: string | null
  denialStatus: string | null
  denialDate: string
  daysSinceDenial: number
  agingBucket: string | null
  claimStatus: string | null
  submissionCount: number
}

type Filters = {
  denialStatus: string
  reasonCode: string
  agingBucket: string
  denialDateFrom: string
  denialDateTo: string
}

// ── Constants ─────────────────────────────────────────────────────────────────

const EMPTY_FILTERS: Filters = {
  denialStatus: '',
  reasonCode: '',
  agingBucket: '',
  denialDateFrom: '',
  denialDateTo: '',
}

const CARC_OPTIONS = [
  { value: '4', label: '4 — Diagnosis inconsistent with procedure' },
  { value: '16', label: '16 — Claim lacks required information' },
  { value: '50', label: '50 — Non-covered service' },
  { value: '96', label: '96 — Non-covered charges' },
  { value: '97', label: '97 — Payment included in primary payment' },
  { value: '181', label: '181 — Procedure code inconsistent with POS' },
  { value: 'UNDERPAYMENT', label: 'UNDERPAYMENT — Underpayment dispute' },
]

const AGING_STYLES: Record<string, string> = {
  '0-30': 'text-green-700 bg-green-50',
  '31-60': 'text-yellow-700 bg-yellow-50',
  '61-90': 'text-orange-600 bg-orange-50',
  '90+': 'text-red-700 bg-red-50',
}

const STATUS_BUTTON_STYLES: Record<string, string> = {
  Appealed: 'border-yellow-400 bg-yellow-50 text-yellow-700',
  Resolved: 'border-green-500 bg-green-50 text-green-700',
  WrittenOff: 'border-red-500 bg-red-50 text-red-700',
}

const STATUS_LABELS: Record<string, string> = {
  Appealed: 'Mark Appealed',
  Resolved: 'Mark Resolved',
  WrittenOff: 'Write Off',
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function extractErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const resp = (err as { response?: { data?: { error?: string; message?: string } } }).response
    return resp?.data?.error ?? resp?.data?.message ?? 'An unexpected error occurred.'
  }
  return 'An unexpected error occurred.'
}

function getAllowedTransitions(status: string | null): string[] {
  if (status === 'Open') return ['Appealed', 'Resolved', 'WrittenOff']
  if (status === 'Appealed') return ['Resolved', 'WrittenOff']
  return []
}

const inputCls =
  'border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

// ── Component ─────────────────────────────────────────────────────────────────

export default function DenialsTab() {
  const navigate = useNavigate()

  const [items, setItems] = useState<ArWorklistItem[]>([])
  const [loading, setLoading] = useState(true)
  const [actionError, setActionError] = useState<string | null>(null)

  // Filters: form (input state) vs applied (committed on Apply)
  const [form, setForm] = useState<Filters>(EMPTY_FILTERS)
  const [applied, setApplied] = useState<Filters>(EMPTY_FILTERS)

  // Quick status update dialog
  const [updateTarget, setUpdateTarget] = useState<ArWorklistItem | null>(null)
  const [updateStatus, setUpdateStatus] = useState('')
  const [updating, setUpdating] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    setActionError(null)
    try {
      const params = new URLSearchParams()
      if (applied.denialStatus) params.set('denialStatus', applied.denialStatus)
      if (applied.reasonCode) params.set('reasonCode', applied.reasonCode)
      if (applied.agingBucket) params.set('agingBucket', applied.agingBucket)
      if (applied.denialDateFrom) params.set('denialDateFrom', applied.denialDateFrom)
      if (applied.denialDateTo) params.set('denialDateTo', applied.denialDateTo)

      const res = await apiClient.get<ArWorklistItem[]>(`api/v1/ar/worklist?${params}`)
      setItems(res.data)
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [applied])

  useEffect(() => {
    load()
  }, [load])

  // Summary stats computed from loaded items
  const openItems = useMemo(() => items.filter((i) => i.denialStatus === 'Open'), [items])
  const appealedItems = useMemo(() => items.filter((i) => i.denialStatus === 'Appealed'), [items])
  const totalAtRisk = useMemo(
    () => openItems.reduce((s, i) => s + i.amountDenied, 0),
    [openItems],
  )

  function handleApply() {
    setApplied(form)
  }

  function handleReset() {
    setForm(EMPTY_FILTERS)
    setApplied(EMPTY_FILTERS)
  }

  function openUpdateDialog(item: ArWorklistItem) {
    const transitions = getAllowedTransitions(item.denialStatus)
    setUpdateTarget(item)
    setUpdateStatus(transitions[0] ?? '')
  }

  async function handleUpdateStatus() {
    if (!updateTarget || !updateStatus) return
    setUpdating(true)
    setActionError(null)
    try {
      await apiClient.patch(`api/v1/ar/denials/${updateTarget.denialId}/status`, {
        newStatus: updateStatus,
      })
      setUpdateTarget(null)
      await load()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setUpdating(false)
    }
  }

  return (
    <div className="flex flex-col gap-5">
      {actionError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{actionError}</p>
      )}

      {/* Summary strip */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {[
          {
            label: 'Open Denials',
            value: loading ? '—' : String(openItems.length),
            color: 'text-blue-700',
            sub: null,
          },
          {
            label: 'Amount at Risk',
            value: loading ? '—' : `$${totalAtRisk.toFixed(2)}`,
            color: 'text-red-600',
            sub: 'open only',
          },
          {
            label: 'Appealed',
            value: loading ? '—' : String(appealedItems.length),
            color: 'text-yellow-700',
            sub: null,
          },
          {
            label: 'Total in View',
            value: loading ? '—' : String(items.length),
            color: 'text-gray-700',
            sub: 'all statuses in filter',
          },
        ].map((card) => (
          <div
            key={card.label}
            className="rounded-xl border border-gray-200 bg-white shadow-sm p-4"
          >
            <p className="text-xs text-gray-500 mb-1">{card.label}</p>
            <p className={`text-2xl font-bold ${card.color}`}>{card.value}</p>
            {card.sub && <p className="text-xs text-gray-400 mt-1">{card.sub}</p>}
          </div>
        ))}
      </div>

      {/* Filters + Table */}
      <Card title="Denials">
        <div className="flex flex-wrap items-end gap-3 mb-5">
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Status</label>
            <select
              value={form.denialStatus}
              onChange={(e) => setForm((f) => ({ ...f, denialStatus: e.target.value }))}
              className={inputCls}
            >
              <option value="">All Statuses</option>
              <option value="Open">Open</option>
              <option value="Appealed">Appealed</option>
              <option value="Resolved">Resolved</option>
              <option value="WrittenOff">Written Off</option>
            </select>
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">CARC Code</label>
            <select
              value={form.reasonCode}
              onChange={(e) => setForm((f) => ({ ...f, reasonCode: e.target.value }))}
              className={inputCls}
            >
              <option value="">All Codes</option>
              {CARC_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
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
            <label className="text-xs text-gray-500">Denial Date From</label>
            <input
              type="date"
              value={form.denialDateFrom}
              onChange={(e) => setForm((f) => ({ ...f, denialDateFrom: e.target.value }))}
              className={inputCls}
            />
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Denial Date To</label>
            <input
              type="date"
              value={form.denialDateTo}
              onChange={(e) => setForm((f) => ({ ...f, denialDateTo: e.target.value }))}
              className={inputCls}
            />
          </div>

          <Button variant="primary" size="sm" onClick={handleApply}>
            Apply
          </Button>
          <Button variant="secondary" size="sm" onClick={handleReset}>
            Reset
          </Button>
        </div>

        {/* Worklist Table */}
        <div className="w-full overflow-x-auto">
          <table className="w-full border-collapse text-left text-sm">
            <thead>
              <tr className="border-b border-gray-200 bg-gray-50">
                {[
                  'Patient',
                  'Claim ID',
                  'Payer / Plan',
                  'CARC',
                  'Amount Denied',
                  'Days',
                  'Aging',
                  'Claim Status',
                  'Denial Status',
                  '',
                ].map((h) => (
                  <th
                    key={h}
                    className="whitespace-nowrap px-4 py-3 text-xs font-semibold uppercase tracking-wider text-gray-500"
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {loading &&
                Array.from({ length: 6 }).map((_, i) => (
                  <tr key={i} className="animate-pulse">
                    {Array.from({ length: 10 }).map((__, j) => (
                      <td key={j} className="px-4 py-3">
                        <div className="h-4 w-full rounded bg-gray-200" />
                      </td>
                    ))}
                  </tr>
                ))}

              {!loading && items.length === 0 && (
                <tr>
                  <td
                    colSpan={10}
                    className="px-4 py-10 text-center text-sm text-gray-400"
                  >
                    No denials found with the current filters.
                  </td>
                </tr>
              )}

              {!loading &&
                items.map((item) => {
                  const transitions = getAllowedTransitions(item.denialStatus)
                  const canUpdate = transitions.length > 0

                  return (
                    <tr
                      key={item.denialId}
                      className="border-b border-gray-100 hover:bg-blue-50 cursor-pointer transition-colors"
                      onClick={() => navigate(`/ar/denials/${item.denialId}`)}
                    >
                      {/* Patient */}
                      <td className="px-4 py-3 font-medium text-gray-800 whitespace-nowrap">
                        {item.patientName ?? '—'}
                      </td>

                      {/* Claim ID */}
                      <td className="px-4 py-3 font-mono text-gray-600">
                        #{item.claimId}
                      </td>

                      {/* Payer / Plan */}
                      <td className="px-4 py-3">
                        <div className="font-medium text-gray-800">
                          {item.payerName ?? '—'}
                        </div>
                        {item.planName && (
                          <div className="text-xs text-gray-400">{item.planName}</div>
                        )}
                      </td>

                      {/* CARC + Remark */}
                      <td className="px-4 py-3">
                        {item.reasonCode ? (
                          <span className="inline-flex items-center rounded-md bg-gray-100 px-2 py-0.5 text-xs font-semibold text-gray-700">
                            {item.reasonCode}
                          </span>
                        ) : (
                          '—'
                        )}
                        {item.remarkCode && (
                          <div className="text-xs text-gray-400 mt-0.5">
                            {item.remarkCode}
                          </div>
                        )}
                      </td>

                      {/* Amount Denied */}
                      <td className="px-4 py-3 font-semibold text-red-600 whitespace-nowrap">
                        ${item.amountDenied.toFixed(2)}
                      </td>

                      {/* Days since denial */}
                      <td className="px-4 py-3 text-gray-700 whitespace-nowrap">
                        {item.daysSinceDenial}d
                      </td>

                      {/* Aging bucket */}
                      <td className="px-4 py-3 whitespace-nowrap">
                        <span
                          className={`inline-flex items-center rounded-md px-2 py-0.5 text-xs font-semibold ${
                            AGING_STYLES[item.agingBucket ?? ''] ?? 'text-gray-700 bg-gray-50'
                          }`}
                        >
                          {item.agingBucket ?? '—'}
                        </span>
                      </td>

                      {/* Claim Status */}
                      <td className="px-4 py-3 whitespace-nowrap">
                        <Badge status={item.claimStatus ?? ''} />
                      </td>

                      {/* Denial Status */}
                      <td className="px-4 py-3 whitespace-nowrap">
                        <Badge status={item.denialStatus ?? ''} />
                      </td>

                      {/* Action — stop propagation so row click doesn't navigate */}
                      <td
                        className="px-4 py-3 whitespace-nowrap"
                        onClick={(e) => e.stopPropagation()}
                      >
                        {canUpdate && (
                          <button
                            type="button"
                            onClick={() => openUpdateDialog(item)}
                            className="text-xs font-semibold text-blue-600 hover:underline"
                          >
                            Update Status
                          </button>
                        )}
                      </td>
                    </tr>
                  )
                })}
            </tbody>
          </table>
        </div>

        {!loading && items.length > 0 && (
          <p className="mt-3 text-xs text-gray-400 text-right">
            {items.length} denial{items.length !== 1 ? 's' : ''} — click a row to view full detail
          </p>
        )}
      </Card>

      {/* Quick Status Update Dialog */}
      <Dialog
        isOpen={updateTarget !== null}
        onClose={() => setUpdateTarget(null)}
        title="Update Denial Status"
      >
        {updateTarget && (
          <div className="space-y-4">
            {/* Summary of the denial being updated */}
            <div className="rounded-lg bg-gray-50 px-4 py-3 text-sm space-y-1.5">
              <div className="flex items-center gap-2">
                <span className="text-gray-500 w-28">Patient</span>
                <strong>{updateTarget.patientName ?? '—'}</strong>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-gray-500 w-28">Claim</span>
                <strong>#{updateTarget.claimId}</strong>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-gray-500 w-28">CARC Code</span>
                <span className="inline-flex items-center rounded-md bg-gray-200 px-2 py-0.5 text-xs font-semibold text-gray-700">
                  {updateTarget.reasonCode ?? '—'}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-gray-500 w-28">Amount Denied</span>
                <strong className="text-red-600">
                  ${updateTarget.amountDenied.toFixed(2)}
                </strong>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-gray-500 w-28">Current Status</span>
                <Badge status={updateTarget.denialStatus ?? ''} />
              </div>
            </div>

            {/* Status selector */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Move to
              </label>
              <div className="flex flex-wrap gap-2">
                {getAllowedTransitions(updateTarget.denialStatus).map((s) => (
                  <label
                    key={s}
                    className={`flex items-center gap-2 px-4 py-2 rounded-lg border cursor-pointer text-sm font-medium transition-colors ${
                      updateStatus === s
                        ? (STATUS_BUTTON_STYLES[s] ??
                          'border-blue-500 bg-blue-50 text-blue-700')
                        : 'border-gray-300 text-gray-600 hover:bg-gray-50'
                    }`}
                  >
                    <input
                      type="radio"
                      name="denialStatus"
                      value={s}
                      checked={updateStatus === s}
                      onChange={() => setUpdateStatus(s)}
                      className="sr-only"
                    />
                    {STATUS_LABELS[s] ?? s}
                  </label>
                ))}
              </div>
            </div>

            {updateStatus === 'WrittenOff' && (
              <p className="text-xs text-red-600 bg-red-50 rounded-md px-3 py-2">
                Writing off a denial is irreversible. The denial cannot be changed after this action.
              </p>
            )}

            <div className="flex justify-end gap-2 pt-1">
              <Button variant="secondary" onClick={() => setUpdateTarget(null)}>
                Cancel
              </Button>
              <Button
                variant={updateStatus === 'WrittenOff' ? 'danger' : 'primary'}
                disabled={!updateStatus || updating}
                onClick={handleUpdateStatus}
              >
                {updating ? 'Updating...' : 'Confirm'}
              </Button>
            </div>
          </div>
        )}
      </Dialog>
    </div>
  )
}
