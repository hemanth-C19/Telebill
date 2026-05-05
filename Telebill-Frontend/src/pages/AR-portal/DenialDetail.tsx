import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import apiClient from '../../api/client'
import { useAuth } from '../../context/AuthContext'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import { Table } from '../../components/shared/ui/Table'
import DenialActions from '../../components/Ar-portal/DenialActions'

// ── Types (exported for DenialActions) ────────────────────────────────────────

export type ClaimLine = {
  claimLineId: number
  lineNo: number
  cptHcpcs: string | null
  modifiers: string | null
  units: number | null
  chargeAmount: number | null
  dxPointers: string | null
}

export type ClaimSummary = {
  claimId: number
  claimStatus: string | null
  totalCharge: number
  patientName: string | null
  payerName: string | null
  planName: string | null
  encounterDateTime: string
  pos: string | null
  lines: ClaimLine[]
}

export type PaymentPost = {
  paymentPostId: number
  claimLineId: number | null
  amountPaid: number
  adjustmentJson: string | null
  postedDate: string
  status: string | null
}

export type Submission = {
  submissionRefId: number
  submitDate: string
  ackType: string | null
  ackStatus: string | null
  ackDate: string | null
  correlationId: string | null
}

export type Attachment = {
  attachId: number
  fileType: string | null
  fileUri: string | null
  notes: string | null
  uploadedDate: string
}

export type DenialDetailData = {
  denialId: number
  denialStatus: string | null
  reasonCode: string | null
  remarkCode: string | null
  denialDate: string
  amountDenied: number
  claim: ClaimSummary | null
  paymentHistory: PaymentPost[]
  submissionHistory: Submission[]
  attachments: Attachment[]
}

// ── Constants ─────────────────────────────────────────────────────────────────

const CARC_DESCRIPTIONS: Record<string, string> = {
  '4': 'Diagnosis inconsistent with procedure',
  '16': 'Claim lacks required information',
  '50': 'Non-covered service',
  '96': 'Non-covered charges — not a member benefit',
  '97': 'Payment included in primary payment',
  '181': 'Procedure code inconsistent with POS',
  UNDERPAYMENT: 'Underpayment dispute',
}

const STATUS_COLORS: Record<string, string> = {
  Open: 'bg-blue-100 text-blue-800',
  Appealed: 'bg-yellow-100 text-yellow-800',
  Resolved: 'bg-green-100 text-green-800',
  WrittenOff: 'bg-red-100 text-red-800',
}

type DetailTab = 'lines' | 'payments' | 'submissions'

const DETAIL_TABS: { key: DetailTab; label: string }[] = [
  { key: 'lines', label: 'Claim Lines' },
  { key: 'payments', label: 'Payment History' },
  { key: 'submissions', label: 'Submission History' },
]

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

function fmtDateTime(s: string | null | undefined): string {
  if (!s) return '—'
  return new Date(s).toLocaleString()
}

// ── Component ─────────────────────────────────────────────────────────────────

export default function DenialDetail() {
  const { denialId } = useParams<{ denialId: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()

  const [detail, setDetail] = useState<DenialDetailData | null>(null)
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState<string | null>(null)
  const [activeTab, setActiveTab] = useState<DetailTab>('lines')
  const [actionSuccess, setActionSuccess] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!denialId) return
    setLoading(true)
    setPageError(null)
    try {
      const res = await apiClient.get<DenialDetailData>(`api/v1/ar/worklist/${denialId}`)
      setDetail(res.data)
    } catch (err) {
      setPageError(extractErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [denialId])

  useEffect(() => {
    load()
  }, [load])

  // ── Loading / error states ────────────────────────────────────────────────

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin h-8 w-8 rounded-full border-4 border-blue-500 border-t-transparent" />
      </div>
    )
  }

  if (pageError || !detail) {
    return (
      <div className="flex flex-col items-center justify-center h-64 gap-4">
        <p className="text-sm text-red-600">{pageError ?? 'Denial not found.'}</p>
        <Button variant="secondary" onClick={() => navigate('/ar/worklist')}>
          ← Back to Worklist
        </Button>
      </div>
    )
  }

  const { claim } = detail

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <div className="flex flex-col gap-6">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm">
        <button
          type="button"
          onClick={() => navigate('/ar/worklist')}
          className="text-blue-600 hover:underline"
        >
          ← AR Worklist
        </button>
        <span className="text-gray-300">/</span>
        <span className="font-medium text-gray-700">Denial #{detail.denialId}</span>
      </div>

      {/* Page header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-gray-900">Denial #{detail.denialId}</h1>
          <span
            className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${
              STATUS_COLORS[detail.denialStatus ?? ''] ?? 'bg-gray-100 text-gray-700'
            }`}
          >
            {detail.denialStatus ?? 'Unknown'}
          </span>
        </div>
        {claim && (
          <span className="text-sm text-gray-500">
            Claim #{claim.claimId} · {claim.patientName ?? '—'}
          </span>
        )}
      </div>

      {/* Success banner */}
      {actionSuccess && (
        <div className="flex items-center justify-between rounded-md border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
          {actionSuccess}
          <button
            type="button"
            onClick={() => setActionSuccess(null)}
            className="ml-4 text-green-500 hover:text-green-700"
          >
            ✕
          </button>
        </div>
      )}

      {/* Main grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* ── Left column (2/3) ── */}
        <div className="lg:col-span-2 flex flex-col gap-6">
          {/* Denial info */}
          <Card title="Denial Information">
            <div className="grid grid-cols-2 md:grid-cols-3 gap-x-6 gap-y-4">
              <InfoRow label="Denial Date" value={fmtDate(detail.denialDate)} />
              <InfoRow
                label="Amount Denied"
                value={`$${detail.amountDenied.toFixed(2)}`}
                valueClass="text-red-600 font-semibold"
              />
              <InfoRow
                label="CARC Code"
                value={
                  detail.reasonCode
                    ? `${detail.reasonCode} — ${CARC_DESCRIPTIONS[detail.reasonCode] ?? 'Unknown'}`
                    : '—'
                }
              />
              <InfoRow label="Remark Code" value={detail.remarkCode ?? '—'} />
              <InfoRow label="Status" value={detail.denialStatus ?? '—'} />
            </div>
          </Card>

          {/* Claim summary */}
          {claim && (
            <Card title="Claim Summary">
              <div className="grid grid-cols-2 md:grid-cols-3 gap-x-6 gap-y-4">
                <InfoRow label="Patient" value={claim.patientName ?? '—'} />
                <InfoRow label="Payer" value={claim.payerName ?? '—'} />
                <InfoRow label="Plan" value={claim.planName ?? '—'} />
                <InfoRow label="Encounter Date" value={fmtDateTime(claim.encounterDateTime)} />
                <InfoRow label="Place of Service" value={claim.pos ?? '—'} />
                <InfoRow label="Total Charge" value={`$${claim.totalCharge.toFixed(2)}`} />
                <div className="flex flex-col gap-1">
                  <span className="text-xs text-gray-500">Claim Status</span>
                  <Badge status={claim.claimStatus ?? ''} />
                </div>
              </div>
            </Card>
          )}

          {/* Detail tabs */}
          <div>
            <div className="border-b border-gray-200 mb-4">
              <nav className="-mb-px flex gap-6">
                {DETAIL_TABS.map((tab) => {
                  const count =
                    tab.key === 'payments'
                      ? detail.paymentHistory.length
                      : tab.key === 'submissions'
                        ? detail.submissionHistory.length
                        : null
                  return (
                    <button
                      key={tab.key}
                      type="button"
                      onClick={() => setActiveTab(tab.key)}
                      className={`pb-3 text-sm font-medium border-b-2 transition-colors ${
                        activeTab === tab.key
                          ? 'border-blue-600 text-blue-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700'
                      }`}
                    >
                      {tab.label}
                      {count != null && count > 0 && (
                        <span className="ml-1.5 rounded-full bg-gray-200 px-1.5 py-0.5 text-xs text-gray-600">
                          {count}
                        </span>
                      )}
                    </button>
                  )
                })}
              </nav>
            </div>

            {activeTab === 'lines' && <ClaimLinesTable lines={claim?.lines ?? []} />}
            {activeTab === 'payments' && (
              <PaymentHistoryTable payments={detail.paymentHistory} />
            )}
            {activeTab === 'submissions' && (
              <SubmissionHistoryTable submissions={detail.submissionHistory} />
            )}
          </div>
        </div>

        {/* ── Right column (1/3) — delegated to DenialActions ── */}
        <div className="flex flex-col gap-6">
          <DenialActions
            detail={detail}
            userId={user?.userId ?? 0}
            onSuccess={setActionSuccess}
            onRefresh={load}
          />
        </div>
      </div>
    </div>
  )
}

// ── Sub-components ────────────────────────────────────────────────────────────

function InfoRow({
  label,
  value,
  valueClass = 'text-gray-800',
}: {
  label: string
  value: string
  valueClass?: string
}) {
  return (
    <div className="flex flex-col gap-0.5">
      <span className="text-xs text-gray-500">{label}</span>
      <span className={`text-sm font-medium ${valueClass}`}>{value}</span>
    </div>
  )
}

const CLAIM_LINE_COLUMNS = [
  { key: 'lineNo', label: '#' },
  { key: 'cptHcpcs', label: 'CPT/HCPCS' },
  { key: 'modifiers', label: 'Modifiers' },
  { key: 'units', label: 'Units' },
  { key: 'chargeAmount', label: 'Charge' },
  { key: 'dxPointers', label: 'Dx Pointers' },
]

const PAYMENT_COLUMNS = [
  { key: 'postedDate', label: 'Posted Date' },
  { key: 'claimLineId', label: 'Line ID' },
  { key: 'amountPaid', label: 'Amount Paid' },
  { key: 'adjustmentJson', label: 'Adjustment' },
  { key: 'status', label: 'Status' },
]

const SUBMISSION_COLUMNS = [
  { key: 'submitDate', label: 'Submit Date' },
  { key: 'ackType', label: 'Ack Type' },
  { key: 'ackStatus', label: 'Ack Status' },
  { key: 'ackDate', label: 'Ack Date' },
  { key: 'correlationId', label: 'Correlation ID' },
]

function ClaimLinesTable({ lines }: { lines: ClaimLine[] }) {
  const data = lines.map((l) => ({
    lineNo: l.lineNo,
    cptHcpcs: l.cptHcpcs,
    modifiers: l.modifiers,
    units: l.units,
    chargeAmount: l.chargeAmount != null ? `$${l.chargeAmount.toFixed(2)}` : null,
    dxPointers: l.dxPointers,
  }))

  return (
    <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
      <Table columns={CLAIM_LINE_COLUMNS} data={data} />
    </div>
  )
}

function PaymentHistoryTable({ payments }: { payments: PaymentPost[] }) {
  const data = payments.map((p) => ({
    postedDate: fmtDate(p.postedDate),
    claimLineId: p.claimLineId != null ? `#${p.claimLineId}` : null,
    amountPaid: `$${p.amountPaid.toFixed(2)}`,
    adjustmentJson: p.adjustmentJson,
    status: <Badge status={p.status ?? ''} />,
  }))

  return (
    <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
      <Table columns={PAYMENT_COLUMNS} data={data} />
    </div>
  )
}

function SubmissionHistoryTable({ submissions }: { submissions: Submission[] }) {
  const data = submissions.map((s) => ({
    submitDate: fmtDate(s.submitDate),
    ackType: s.ackType,
    ackStatus: s.ackStatus ? <Badge status={s.ackStatus} /> : null,
    ackDate: fmtDate(s.ackDate),
    correlationId: s.correlationId,
  }))

  return (
    <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
      <Table columns={SUBMISSION_COLUMNS} data={data} />
    </div>
  )
}
