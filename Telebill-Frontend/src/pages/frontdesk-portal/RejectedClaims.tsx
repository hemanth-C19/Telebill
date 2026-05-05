import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import apiClient from '../../api/client'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import { Dialog } from '../../components/shared/ui/Dialog'
import { Table } from '../../components/shared/ui/Table'

// ── Types ─────────────────────────────────────────────────────────────────────

type RejectedClaim = {
  claimID: number
  patientName: string
  planName: string
  payerName: string
  totalCharge: number
}

type ClaimListResponse = {
  totalCount: number
  claims: RejectedClaim[]
}

type SubmissionRef = {
  submitID: number
  batchID: number
  claimID: number
  clearinghouseID: string | null
  correlationID: string | null
  submitDate: string
  ackType: string | null
  ackStatus: string | null
  ackDate: string | null
  notes: string | null
}

// ── Constants ─────────────────────────────────────────────────────────────────

const COLUMNS = [
  { key: 'claimId', label: 'Claim ID' },
  { key: 'patient', label: 'Patient' },
  { key: 'payer', label: 'Payer' },
  { key: 'plan', label: 'Plan' },
  { key: 'totalCharge', label: 'Total Charge' },
  { key: 'status', label: 'Status' },
]

const REF_COLUMNS = [
  { key: 'ackType', label: 'Type' },
  { key: 'ackStatus', label: 'ACK Status' },
  { key: 'ackDate', label: 'ACK Date' },
  { key: 'batchId', label: 'Batch' },
  { key: 'correlationId', label: 'Correlation ID' },
  { key: 'notes', label: 'Notes' },
]

// ── Helpers ───────────────────────────────────────────────────────────────────

function fmtDate(s: string | null | undefined): string {
  if (!s) return '—'
  return new Date(s).toLocaleDateString()
}

// ── Component ─────────────────────────────────────────────────────────────────

export default function RejectedClaims() {
  const navigate = useNavigate()
  const [claims, setClaims] = useState<RejectedClaim[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Submission history dialog
  const [historyTarget, setHistoryTarget] = useState<RejectedClaim | null>(null)
  const [refs, setRefs] = useState<SubmissionRef[]>([])
  const [loadingRefs, setLoadingRefs] = useState(false)

  // Mark as Ready
  const [readyTarget, setReadyTarget] = useState<RejectedClaim | null>(null)
  const [markingReady, setMarkingReady] = useState(false)
  const [readyError, setReadyError] = useState<string | null>(null)

  async function handleMarkReady() {
    if (!readyTarget) return
    setMarkingReady(true)
    setReadyError(null)
    try {
      await apiClient.patch(`api/claims/${readyTarget.claimID}/status`, { newStatus: 'Ready' })
      setReadyTarget(null)
      load()
    } catch {
      setReadyError('Failed to reset claim status. Please try again.')
    } finally {
      setMarkingReady(false)
    }
  }

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await apiClient.get<ClaimListResponse>(
        'api/claims?claimStatus=Rejected&pageSize=200',
      )
      setClaims(res.data.claims)
      setTotalCount(res.data.totalCount)
    } catch {
      setError('Failed to load rejected claims.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  async function openHistory(claim: RejectedClaim) {
    setHistoryTarget(claim)
    setRefs([])
    setLoadingRefs(true)
    try {
      const res = await apiClient.get<SubmissionRef[]>(
        `api/v1/batch/submission-refs/by-claim/${claim.claimID}`,
      )
      setRefs(res.data)
    } catch {
      setRefs([])
    } finally {
      setLoadingRefs(false)
    }
  }

  const rejection277 = refs.filter(
    (r) => r.ackType === '277CA' && r.ackStatus?.toLowerCase() === 'rejected',
  )

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Rejected Claims</h1>
        <p className="mt-1 text-sm text-gray-500">
          Claims rejected by the payer at the 277CA stage. Review the rejection reason, correct the
          issue, and resubmit by adding the claim to a new batch.
        </p>
      </div>

      {error && <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error}</p>}

      <Card title={`Rejected Claims${!loading ? ` (${totalCount})` : ''}`}>
        <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
          <Table
            columns={COLUMNS}
            loading={loading}
            data={claims.map((c) => ({
              claimId: `#${c.claimID}`,
              patient: c.patientName,
              payer: c.payerName,
              plan: c.planName,
              totalCharge: `$${c.totalCharge.toFixed(2)}`,
              status: <Badge status="Rejected" />,
              _claimID: c.claimID,
            }))}
            showActions
            actions={[
              {
                label: 'View History',
                onClick: (row) => {
                  const claim = claims.find((c) => c.claimID === (row._claimID as number))
                  if (claim) openHistory(claim)
                },
              },
              {
                label: 'Mark as Ready',
                onClick: (row) => {
                  const claim = claims.find((c) => c.claimID === (row._claimID as number))
                  if (claim) { setReadyTarget(claim); setReadyError(null) }
                },
              },
            ]}
          />
        </div>

        {!loading && claims.length === 0 && (
          <p className="mt-4 text-center text-sm text-green-600 font-medium">
            No rejected claims — all clear.
          </p>
        )}
      </Card>

      {/* ── Mark as Ready Dialog ── */}
      <Dialog
        isOpen={readyTarget !== null}
        onClose={() => setReadyTarget(null)}
        title="Reset Claim to Ready"
        maxWidth="sm"
      >
        <div className="space-y-4">
          <p className="text-sm text-gray-700">
            This will reset <strong>Claim #{readyTarget?.claimID}</strong> ({readyTarget?.patientName}) back
            to <strong>Ready</strong> status so it can be added to a new batch.
          </p>
          {readyError && <p className="text-sm text-red-600">{readyError}</p>}
          <div className="flex justify-end gap-2">
            <Button variant="secondary" onClick={() => setReadyTarget(null)} disabled={markingReady}>
              Cancel
            </Button>
            <Button variant="primary" onClick={handleMarkReady} disabled={markingReady}>
              {markingReady ? 'Resetting...' : 'Confirm'}
            </Button>
          </div>
        </div>
      </Dialog>

      {/* ── Submission History Dialog ── */}
      <Dialog
        isOpen={historyTarget !== null}
        onClose={() => setHistoryTarget(null)}
        title={`Submission History — Claim #${historyTarget?.claimID}`}
        maxWidth="xl"
      >
        {historyTarget && (
          <div className="space-y-4">
            {/* Claim summary */}
            <div className="grid grid-cols-2 gap-x-6 gap-y-2 rounded-lg bg-gray-50 px-4 py-3 text-sm">
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Patient</span>
                <strong>{historyTarget.patientName}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Payer</span>
                <strong>{historyTarget.payerName}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Plan</span>
                <strong>{historyTarget.planName}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-24">Total Charge</span>
                <strong>${historyTarget.totalCharge.toFixed(2)}</strong>
              </div>
            </div>

            {/* 277CA rejection reason callout */}
            {rejection277.length > 0 && (
              <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm space-y-1">
                <p className="font-semibold text-red-700">277CA Rejection Reason(s)</p>
                {rejection277.map((r) => (
                  <div key={r.submitID} className="text-red-600">
                    <span className="font-medium">Batch #{r.batchID}</span>
                    {r.notes ? ` — ${r.notes}` : ' — No reason recorded.'}
                    {r.ackDate && (
                      <span className="ml-2 text-xs text-red-400">({fmtDate(r.ackDate)})</span>
                    )}
                  </div>
                ))}
              </div>
            )}

            {/* Full submission ref table */}
            {loadingRefs ? (
              <div className="flex justify-center py-6">
                <div className="animate-spin h-7 w-7 rounded-full border-4 border-blue-500 border-t-transparent" />
              </div>
            ) : (
              <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
                <Table
                  columns={REF_COLUMNS}
                  data={refs.map((r) => ({
                    ackType: r.ackType ?? 'Membership',
                    ackStatus: r.ackStatus ? <Badge status={r.ackStatus} /> : '—',
                    ackDate: fmtDate(r.ackDate),
                    batchId: `#${r.batchID}`,
                    correlationId: r.correlationID,
                    notes: r.notes,
                  }))}
                />
              </div>
            )}

            <div className="flex justify-between items-center pt-1">
              <Button
                variant="primary"
                onClick={() => {
                  setHistoryTarget(null)
                  navigate('/frontdesk/batches')
                }}
              >
                Go to Batch Management →
              </Button>
              <Button variant="secondary" onClick={() => setHistoryTarget(null)}>
                Close
              </Button>
            </div>
          </div>
        )}
      </Dialog>
    </div>
  )
}
