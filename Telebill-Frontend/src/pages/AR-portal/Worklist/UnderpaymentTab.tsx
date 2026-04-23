import { useCallback, useEffect, useMemo, useState } from 'react'
import apiClient from '../../../api/client'
import { Badge } from '../../../components/shared/ui/Badge'
import { Button } from '../../../components/shared/ui/Button'
import { Card } from '../../../components/shared/ui/Card'
import { Dialog } from '../../../components/shared/ui/Dialog'
import { useAuth } from '../../../hooks/useAuth'

// ── Types ─────────────────────────────────────────────────────────────────────

type LineUnderpayment = {
  claimLineId: number
  lineNo: number
  cptHcpcs: string | null
  modifiers: string | null
  chargeAmount: number
  amountPaid: number
  allowedAmount: number | null
  variance: number | null
  isPotentialUnderpayment: boolean
}

type UnderpaymentItem = {
  claimId: number
  patientName: string | null
  payerName: string | null
  planName: string | null
  encounterDateTime: string
  totalCharge: number
  totalPaid: number
  totalAllowed: number
  underpaymentAmount: number
  claimStatus: string | null
  lines: LineUnderpayment[]
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function extractErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const resp = (err as { response?: { data?: { error?: string; message?: string } } }).response
    return resp?.data?.error ?? resp?.data?.message ?? 'An unexpected error occurred.'
  }
  return 'An unexpected error occurred.'
}

function fmt(n: number) {
  return `$${n.toFixed(2)}`
}

function fmtDate(iso: string) {
  if (!iso || iso.startsWith('0001')) return '—'
  return new Date(iso).toLocaleDateString()
}

// ── Row sub-component (avoids key-on-fragment issue) ─────────────────────────

type RowProps = {
  item: UnderpaymentItem
  isExpanded: boolean
  onToggle: () => void
  onFlag: () => void
  flagging: boolean
}

function UnderpaymentRow({ item, isExpanded, onToggle, onFlag, flagging }: RowProps) {
  const variance = item.totalAllowed > 0 ? item.totalAllowed - item.totalPaid : null

  return (
    <>
      <tr
        className="border-b border-gray-100 hover:bg-amber-50 cursor-pointer transition-colors"
        onClick={onToggle}
      >
        {/* Patient */}
        <td className="px-4 py-3 font-medium text-gray-800 whitespace-nowrap">
          {item.patientName ?? '—'}
        </td>

        {/* Claim ID */}
        <td className="px-4 py-3 font-mono text-gray-600">#{item.claimId}</td>

        {/* Payer / Plan */}
        <td className="px-4 py-3">
          <div className="font-medium text-gray-800">{item.payerName ?? '—'}</div>
          {item.planName && (
            <div className="text-xs text-gray-400">{item.planName}</div>
          )}
        </td>

        {/* Encounter Date */}
        <td className="px-4 py-3 text-gray-500 whitespace-nowrap">
          {fmtDate(item.encounterDateTime)}
        </td>

        {/* Billed */}
        <td className="px-4 py-3 text-gray-700">{fmt(item.totalCharge)}</td>

        {/* Allowed */}
        <td className="px-4 py-3 text-blue-700 font-medium">
          {item.totalAllowed > 0 ? fmt(item.totalAllowed) : '—'}
        </td>

        {/* Paid */}
        <td className="px-4 py-3 text-green-700">{fmt(item.totalPaid)}</td>

        {/* Underpayment (variance) */}
        <td className="px-4 py-3 font-semibold text-red-600 whitespace-nowrap">
          {variance != null && variance > 0.01 ? fmt(variance) : '—'}
        </td>

        {/* Claim Status */}
        <td className="px-4 py-3 whitespace-nowrap">
          <Badge status={item.claimStatus ?? ''} />
        </td>

        {/* Actions — stop propagation */}
        <td
          className="px-4 py-3 whitespace-nowrap"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="flex items-center gap-3">
            <button
              type="button"
              className="text-xs font-semibold text-blue-600 hover:underline"
              onClick={onToggle}
            >
              {isExpanded ? 'Hide Lines' : 'View Lines'}
            </button>
            <button
              type="button"
              disabled={flagging}
              className="text-xs font-semibold text-amber-700 hover:underline disabled:opacity-50"
              onClick={onFlag}
            >
              {flagging ? 'Flagging...' : 'Flag'}
            </button>
          </div>
        </td>
      </tr>

      {isExpanded && (
        <tr>
          <td colSpan={10} className="bg-amber-50 px-6 py-4 border-b border-amber-100">
            <p className="text-xs font-semibold text-amber-800 uppercase tracking-wide mb-3">
              Line-level Underpayment Breakdown
            </p>
            <table className="w-full text-sm border border-gray-200 rounded-lg overflow-hidden">
              <thead>
                <tr className="bg-white border-b border-gray-200">
                  {[
                    'Line',
                    'CPT / HCPCS',
                    'Modifiers',
                    'Billed',
                    'Allowed',
                    'Paid',
                    'Variance',
                    'Flag?',
                  ].map((h) => (
                    <th
                      key={h}
                      className="px-3 py-2 text-xs font-semibold text-gray-500 text-left whitespace-nowrap"
                    >
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {item.lines.map((line) => (
                  <tr
                    key={line.claimLineId}
                    className={`border-b border-gray-100 ${
                      line.isPotentialUnderpayment ? 'bg-red-50' : 'bg-white'
                    }`}
                  >
                    <td className="px-3 py-2 text-gray-500">{line.lineNo}</td>
                    <td className="px-3 py-2 font-medium">{line.cptHcpcs ?? '—'}</td>
                    <td className="px-3 py-2 text-gray-500 text-xs">{line.modifiers ?? '—'}</td>
                    <td className="px-3 py-2">{fmt(line.chargeAmount)}</td>
                    <td className="px-3 py-2 text-blue-700">
                      {line.allowedAmount != null ? fmt(line.allowedAmount) : '—'}
                    </td>
                    <td className="px-3 py-2 text-green-700">{fmt(line.amountPaid)}</td>
                    <td className="px-3 py-2 font-semibold text-red-600">
                      {line.variance != null && line.variance > 0.01
                        ? fmt(line.variance)
                        : '—'}
                    </td>
                    <td className="px-3 py-2">
                      {line.isPotentialUnderpayment ? (
                        <span className="inline-flex items-center rounded-full bg-red-100 text-red-700 px-2 py-0.5 text-xs font-semibold">
                          Yes
                        </span>
                      ) : (
                        <span className="text-gray-400 text-xs">No</span>
                      )}
                    </td>
                  </tr>
                ))}

                {/* Totals row */}
                <tr className="bg-gray-50 border-t-2 border-gray-200">
                  <td
                    colSpan={3}
                    className="px-3 py-2 text-right text-xs font-semibold text-gray-500 uppercase"
                  >
                    Totals
                  </td>
                  <td className="px-3 py-2 font-semibold">
                    {fmt(item.lines.reduce((a, l) => a + l.chargeAmount, 0))}
                  </td>
                  <td className="px-3 py-2 font-semibold text-blue-700">
                    {item.totalAllowed > 0 ? fmt(item.totalAllowed) : '—'}
                  </td>
                  <td className="px-3 py-2 font-semibold text-green-700">
                    {fmt(item.totalPaid)}
                  </td>
                  <td className="px-3 py-2 font-bold text-red-600">
                    {item.underpaymentAmount > 0.01 ? fmt(item.underpaymentAmount) : '—'}
                  </td>
                  <td />
                </tr>
              </tbody>
            </table>
          </td>
        </tr>
      )}
    </>
  )
}

// ── Main Component ────────────────────────────────────────────────────────────

export default function UnderpaymentTab() {
  const { user } = useAuth()

  const [items, setItems] = useState<UnderpaymentItem[]>([])
  const [loading, setLoading] = useState(true)
  const [actionError, setActionError] = useState<string | null>(null)
  const [actionSuccess, setActionSuccess] = useState<string | null>(null)

  // Expanded row tracking
  const [expandedClaimId, setExpandedClaimId] = useState<number | null>(null)

  // Flag underpayment dialog
  const [flagTarget, setFlagTarget] = useState<UnderpaymentItem | null>(null)
  const [flagNotes, setFlagNotes] = useState('')
  const [flaggingId, setFlaggingId] = useState<number | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setActionError(null)
    try {
      const res = await apiClient.get<UnderpaymentItem[]>('api/v1/ar/underpayments')
      setItems(res.data)
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  // Summary stats
  const totalUnderpaymentAmount = useMemo(
    () => items.reduce((s, i) => s + i.underpaymentAmount, 0),
    [items],
  )

  function toggleRow(claimId: number) {
    setExpandedClaimId((prev) => (prev === claimId ? null : claimId))
  }

  function openFlagDialog(item: UnderpaymentItem) {
    setFlagTarget(item)
    setFlagNotes('')
    setActionError(null)
    setActionSuccess(null)
  }

  async function handleFlag() {
    if (!flagTarget) return
    setFlaggingId(flagTarget.claimId)
    setActionError(null)
    try {
      await apiClient.post(
        `api/v1/ar/underpayments/flag?userId=${user?.userId ?? 0}`,
        { claimId: flagTarget.claimId, notes: flagNotes || null },
      )
      setFlagTarget(null)
      setActionSuccess(
        `Underpayment flagged for Claim #${flagTarget.claimId}. A denial record has been created in the AR Worklist.`,
      )
      await load()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setFlaggingId(null)
    }
  }

  const inputCls =
    'border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

  return (
    <div className="flex flex-col gap-5">
      {actionError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{actionError}</p>
      )}
      {actionSuccess && (
        <p className="rounded-md bg-green-50 px-4 py-3 text-sm text-green-700">{actionSuccess}</p>
      )}

      {/* Summary strip */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
        {[
          {
            label: 'Claims with Underpayment',
            value: loading ? '—' : String(items.length),
            color: 'text-amber-700',
          },
          {
            label: 'Total Underpayment Amount',
            value: loading ? '—' : fmt(totalUnderpaymentAmount),
            color: 'text-red-600',
          },
          {
            label: 'All PartiallyPaid',
            value: 'PartiallyPaid',
            color: 'text-gray-500',
            note: true,
          },
        ].map((card) => (
          <div
            key={card.label}
            className="rounded-xl border border-gray-200 bg-white shadow-sm p-4"
          >
            <p className="text-xs text-gray-500 mb-1">{card.label}</p>
            {'note' in card ? (
              <Badge status="PartiallyPaid" />
            ) : (
              <p className={`text-2xl font-bold ${card.color}`}>{card.value}</p>
            )}
          </div>
        ))}
      </div>

      <Card title="Underpayment Worklist">
        <p className="text-xs text-gray-500 mb-4">
          Claims where the payer paid less than the contracted fee schedule amount. Click a row or
          "View Lines" to see the per-line breakdown. Use "Flag" to open a formal dispute.
        </p>

        <div className="w-full overflow-x-auto">
          <table className="w-full border-collapse text-left text-sm">
            <thead>
              <tr className="border-b border-gray-200 bg-gray-50">
                {[
                  'Patient',
                  'Claim ID',
                  'Payer / Plan',
                  'Encounter Date',
                  'Billed',
                  'Allowed',
                  'Paid',
                  'Underpayment',
                  'Claim Status',
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
                Array.from({ length: 5 }).map((_, i) => (
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
                  <td colSpan={10} className="px-4 py-10 text-center text-sm text-gray-400">
                    No underpayment claims found. All PartiallyPaid claims are paid at or above
                    the contracted fee schedule.
                  </td>
                </tr>
              )}

              {!loading &&
                items.map((item) => (
                  <UnderpaymentRow
                    key={item.claimId}
                    item={item}
                    isExpanded={expandedClaimId === item.claimId}
                    onToggle={() => toggleRow(item.claimId)}
                    onFlag={() => openFlagDialog(item)}
                    flagging={flaggingId === item.claimId}
                  />
                ))}
            </tbody>
          </table>
        </div>

        {!loading && items.length > 0 && (
          <p className="mt-3 text-xs text-gray-400 text-right">
            {items.length} claim{items.length !== 1 ? 's' : ''} with underpayment — ordered by
            highest underpayment first
          </p>
        )}
      </Card>

      {/* Flag Underpayment Confirm Dialog */}
      <Dialog
        isOpen={flagTarget !== null}
        onClose={() => setFlagTarget(null)}
        title="Flag Underpayment"
      >
        {flagTarget && (
          <div className="space-y-4">
            <div className="rounded-lg bg-amber-50 border border-amber-200 px-4 py-3 text-sm space-y-1.5">
              <p className="font-semibold text-amber-800 mb-2">
                This will open a formal underpayment dispute for this claim.
              </p>
              <div className="flex gap-2">
                <span className="text-gray-500 w-32">Patient</span>
                <strong>{flagTarget.patientName ?? '—'}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-32">Claim ID</span>
                <strong>#{flagTarget.claimId}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-32">Payer</span>
                <strong>{flagTarget.payerName ?? '—'}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-32">Total Allowed</span>
                <strong className="text-blue-700">{fmt(flagTarget.totalAllowed)}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-32">Total Paid</span>
                <strong className="text-green-700">{fmt(flagTarget.totalPaid)}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-32">Underpayment</span>
                <strong className="text-red-600">{fmt(flagTarget.underpaymentAmount)}</strong>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Notes (optional)
              </label>
              <textarea
                rows={2}
                value={flagNotes}
                onChange={(e) => setFlagNotes(e.target.value)}
                placeholder="e.g. Fee schedule rate mismatch on CPT 99213"
                className={`w-full resize-none ${inputCls}`}
              />
            </div>

            <div className="flex justify-end gap-2 pt-1">
              <Button variant="secondary" onClick={() => setFlagTarget(null)}>
                Cancel
              </Button>
              <Button
                variant="primary"
                disabled={flaggingId !== null}
                onClick={handleFlag}
              >
                {flaggingId !== null ? 'Flagging...' : 'Flag Underpayment'}
              </Button>
            </div>
          </div>
        )}
      </Dialog>
    </div>
  )
}
