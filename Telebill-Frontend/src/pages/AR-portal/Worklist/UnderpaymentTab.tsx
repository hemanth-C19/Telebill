import { useCallback, useEffect, useMemo, useState } from 'react'
import apiClient from '../../../api/client'
import { Badge } from '../../../components/shared/ui/Badge'
import { Button } from '../../../components/shared/ui/Button'
import { Card } from '../../../components/shared/ui/Card'
import { Dialog } from '../../../components/shared/ui/Dialog'
import { Table } from '../../../components/shared/ui/Table'
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

// ── Constants ─────────────────────────────────────────────────────────────────

const LIST_COLUMNS = [
  { key: 'patient', label: 'Patient' },
  { key: 'claimId', label: 'Claim ID' },
  { key: 'payerPlan', label: 'Payer / Plan' },
  { key: 'encounterDate', label: 'Encounter Date' },
  { key: 'billed', label: 'Billed' },
  { key: 'allowed', label: 'Allowed' },
  { key: 'paid', label: 'Paid' },
  { key: 'underpayment', label: 'Underpayment' },
  { key: 'claimStatus', label: 'Claim Status' },
]

const LINE_COLUMNS = [
  { key: 'lineNo', label: 'Line' },
  { key: 'cptHcpcs', label: 'CPT/HCPCS' },
  { key: 'modifiers', label: 'Modifiers' },
  { key: 'billed', label: 'Billed' },
  { key: 'allowed', label: 'Allowed' },
  { key: 'paid', label: 'Paid' },
  { key: 'variance', label: 'Variance' },
  { key: 'flagged', label: 'Flag?' },
]

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

const inputCls =
  'border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

// ── Component ─────────────────────────────────────────────────────────────────

export default function UnderpaymentTab() {
  const { user } = useAuth()

  const [items, setItems] = useState<UnderpaymentItem[]>([])
  const [loading, setLoading] = useState(true)
  const [actionError, setActionError] = useState<string | null>(null)
  const [actionSuccess, setActionSuccess] = useState<string | null>(null)

  // Lines detail dialog
  const [linesTarget, setLinesTarget] = useState<UnderpaymentItem | null>(null)

  // Flag underpayment dialog
  const [flagTarget, setFlagTarget] = useState<UnderpaymentItem | null>(null)
  const [flagNotes, setFlagNotes] = useState('')
  const [flagging, setFlagging] = useState(false)

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

  const totalUnderpaymentAmount = useMemo(
    () => items.reduce((s, i) => s + i.underpaymentAmount, 0),
    [items],
  )

  function openFlagDialog(item: UnderpaymentItem) {
    setFlagTarget(item)
    setFlagNotes('')
    setActionError(null)
    setActionSuccess(null)
  }

  async function handleFlag() {
    if (!flagTarget) return
    setFlagging(true)
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
      setFlagging(false)
    }
  }

  // ── Render ────────────────────────────────────────────────────────────────

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

      {/* Worklist Table */}
      <Card title="Underpayment Worklist">
        <p className="text-xs text-gray-500 mb-4">
          Claims where the payer paid less than the contracted fee schedule amount. Use "View Lines"
          to see the per-line breakdown, or "Flag" to open a formal dispute.
        </p>

        <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
          <Table
            columns={LIST_COLUMNS}
            data={items.map((item) => {
              const variance = item.totalAllowed > 0 ? item.totalAllowed - item.totalPaid : null
              return {
                patient: item.patientName ?? '—',
                claimId: `#${item.claimId}`,
                payerPlan: (
                  <div className="flex flex-col">
                    <span className="font-medium text-gray-800">{item.payerName ?? '—'}</span>
                    {item.planName && (
                      <span className="text-xs text-gray-400">{item.planName}</span>
                    )}
                  </div>
                ),
                encounterDate: fmtDate(item.encounterDateTime),
                billed: fmt(item.totalCharge),
                allowed: item.totalAllowed > 0 ? fmt(item.totalAllowed) : null,
                paid: fmt(item.totalPaid),
                underpayment:
                  variance != null && variance > 0.01 ? fmt(variance) : null,
                claimStatus: <Badge status={item.claimStatus ?? ''} />,
                // hidden — used by action handlers only
                _claimId: item.claimId,
              }
            })}
            loading={loading}
            showActions
            actions={[
              {
                label: 'View Lines',
                onClick: (row) => {
                  const item = items.find((i) => i.claimId === (row._claimId as number))
                  if (item) setLinesTarget(item)
                },
              },
              {
                label: 'Flag Underpayment',
                variant: 'danger',
                onClick: (row) => {
                  const item = items.find((i) => i.claimId === (row._claimId as number))
                  if (item) openFlagDialog(item)
                },
              },
            ]}
          />
        </div>

        {!loading && items.length > 0 && (
          <p className="mt-3 text-xs text-gray-400 text-right">
            {items.length} claim{items.length !== 1 ? 's' : ''} with underpayment
          </p>
        )}
      </Card>

      {/* ── Line Detail Dialog ── */}
      <Dialog
        isOpen={linesTarget !== null}
        onClose={() => setLinesTarget(null)}
        title={`Claim Lines — #${linesTarget?.claimId}`}
        maxWidth="lg"
      >
        {linesTarget && (
          <div className="space-y-4">
            {/* Claim summary */}
            <div className="grid grid-cols-2 gap-x-6 gap-y-2 rounded-lg bg-gray-50 px-4 py-3 text-sm">
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Patient</span>
                <strong>{linesTarget.patientName ?? '—'}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Payer</span>
                <strong>{linesTarget.payerName ?? '—'}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Total Billed</span>
                <strong>{fmt(linesTarget.totalCharge)}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Total Allowed</span>
                <strong className="text-blue-700">{fmt(linesTarget.totalAllowed)}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Total Paid</span>
                <strong className="text-green-700">{fmt(linesTarget.totalPaid)}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Underpayment</span>
                <strong className="text-red-600">{fmt(linesTarget.underpaymentAmount)}</strong>
              </div>
            </div>

            {/* Lines table */}
            <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
              <Table
                columns={LINE_COLUMNS}
                data={linesTarget.lines.map((line) => ({
                  lineNo: line.lineNo,
                  cptHcpcs: line.cptHcpcs,
                  modifiers: line.modifiers,
                  billed: fmt(line.chargeAmount),
                  allowed: line.allowedAmount != null ? fmt(line.allowedAmount) : null,
                  paid: fmt(line.amountPaid),
                  variance:
                    line.variance != null && line.variance > 0.01
                      ? fmt(line.variance)
                      : null,
                  flagged: line.isPotentialUnderpayment ? (
                    <span className="inline-flex items-center rounded-full bg-red-100 px-2 py-0.5 text-xs font-semibold text-red-700">
                      Yes
                    </span>
                  ) : (
                    <span className="text-xs text-gray-400">No</span>
                  ),
                }))}
              />
            </div>

            {/* Totals row */}
            <div className="flex justify-end gap-6 rounded-lg border border-gray-200 bg-gray-50 px-4 py-2.5 text-sm">
              <span className="text-gray-500 font-medium">Totals</span>
              <span>
                Billed:{' '}
                <strong>
                  {fmt(linesTarget.lines.reduce((a, l) => a + l.chargeAmount, 0))}
                </strong>
              </span>
              <span>
                Allowed: <strong className="text-blue-700">{fmt(linesTarget.totalAllowed)}</strong>
              </span>
              <span>
                Paid: <strong className="text-green-700">{fmt(linesTarget.totalPaid)}</strong>
              </span>
              <span>
                Underpayment:{' '}
                <strong className="text-red-600">{fmt(linesTarget.underpaymentAmount)}</strong>
              </span>
            </div>

            <div className="flex justify-end gap-2 pt-1">
              <Button variant="secondary" onClick={() => setLinesTarget(null)}>
                Close
              </Button>
              <Button
                variant="danger"
                onClick={() => {
                  openFlagDialog(linesTarget)
                  setLinesTarget(null)
                }}
              >
                Flag Underpayment
              </Button>
            </div>
          </div>
        )}
      </Dialog>

      {/* ── Flag Underpayment Dialog ── */}
      <Dialog
        isOpen={flagTarget !== null}
        onClose={() => setFlagTarget(null)}
        title="Flag Underpayment"
      >
        {flagTarget && (
          <div className="space-y-4">
            <div className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm space-y-1.5">
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
                Notes <span className="text-gray-400 font-normal">(optional)</span>
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
              <Button variant="primary" disabled={flagging} onClick={handleFlag}>
                {flagging ? 'Flagging...' : 'Flag Underpayment'}
              </Button>
            </div>
          </div>
        )}
      </Dialog>
    </div>
  )
}
