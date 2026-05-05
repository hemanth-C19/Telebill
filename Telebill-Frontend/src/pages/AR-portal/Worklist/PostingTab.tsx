import { useState } from 'react'
import { useFieldArray, useForm } from 'react-hook-form'
import apiClient from '../../../api/client'
import { Badge } from '../../../components/shared/ui/Badge'
import { Button } from '../../../components/shared/ui/Button'
import { Card } from '../../../components/shared/ui/Card'
import { Dialog } from '../../../components/shared/ui/Dialog'
import { Table } from '../../../components/shared/ui/Table'

// ── Types ─────────────────────────────────────────────────────────────────────

type AdjustmentEntry = {
  group: string
  carc: string
  amount: number
}

type PaymentPostItem = {
  paymentID: number
  claimID: number
  claimLineID: number | null
  lineNo: number | null
  cptHcpcs: string | null
  chargeAmount: number
  amountPaid: number
  adjustments: AdjustmentEntry[]
  totalAdjusted: number
  patientResponsibility: number
  postedDate: string
  postedByName: string
  status: string // Active | Voided
}

type ClaimPaymentSummary = {
  claimID: number
  claimStatus: string
  totalCharge: number
  totalPaid: number
  totalContractualAdjustment: number
  totalPatientResponsibility: number
  paymentPosts: PaymentPostItem[]
}

type PostingResult = {
  claimID: number
  previousClaimStatus: string
  newClaimStatus: string
  totalPaid: number
  totalCharge: number
  totalPatientResponsibility: number
  patientBalance: {
    balanceID: number
    amountDue: number
    agingBucket: string
    status: string
  }
  denialCreated: boolean
}

type PostPaymentForm = {
  claimLineId: string
  amountPaid: string
  adjustments: { group: string; carc: string; amount: string }[]
}

type VoidForm = {
  reason: string
}

// ── Constants ─────────────────────────────────────────────────────────────────

const POST_COLUMNS = [
  { key: 'paymentId', label: '#' },
  { key: 'scope', label: 'Scope' },
  { key: 'charged', label: 'Charged' },
  { key: 'paid', label: 'Paid' },
  { key: 'adjusted', label: 'CO Adj.' },
  { key: 'patientResp', label: 'Pt. Resp.' },
  { key: 'postedDate', label: 'Posted Date' },
  { key: 'postedBy', label: 'Posted By' },
  { key: 'status', label: 'Status' },
]

const ADJ_GROUPS = ['CO', 'PR', 'OA', 'PI']

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

function fmtDate(s: string | null | undefined): string {
  if (!s) return '—'
  return new Date(s).toLocaleDateString()
}

const inputCls =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

const fieldCls =
  'border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

// ── Component ─────────────────────────────────────────────────────────────────

export default function PostingTab() {
  const [claimIdInput, setClaimIdInput] = useState('')
  const [summary, setSummary] = useState<ClaimPaymentSummary | null>(null)
  const [loadingClaim, setLoadingClaim] = useState(false)
  const [claimError, setClaimError] = useState<string | null>(null)
  const [actionSuccess, setActionSuccess] = useState<string | null>(null)
  const [lastResult, setLastResult] = useState<PostingResult | null>(null)

  const [postOpen, setPostOpen] = useState(false)
  const [posting, setPosting] = useState(false)
  const [postError, setPostError] = useState<string | null>(null)

  const [voidTarget, setVoidTarget] = useState<PaymentPostItem | null>(null)
  const [voiding, setVoiding] = useState(false)
  const [voidError, setVoidError] = useState<string | null>(null)

  const postForm = useForm<PostPaymentForm>({
    defaultValues: { claimLineId: '', amountPaid: '', adjustments: [] },
  })

  const { fields, append, remove } = useFieldArray({
    control: postForm.control,
    name: 'adjustments',
  })

  const voidForm = useForm<VoidForm>({ defaultValues: { reason: '' } })

  async function loadClaim() {
    const id = parseInt(claimIdInput)
    if (!id || id <= 0) {
      setClaimError('Enter a valid Claim ID.')
      return
    }
    setLoadingClaim(true)
    setClaimError(null)
    setSummary(null)
    setLastResult(null)
    setActionSuccess(null)
    try {
      const res = await apiClient.get<ClaimPaymentSummary>(
        `api/v1/posting/payments/by-claim/${id}`,
      )
      setSummary(res.data)
    } catch (err) {
      setClaimError(extractErrorMessage(err))
    } finally {
      setLoadingClaim(false)
    }
  }

  async function refreshSummary(claimID: number) {
    const res = await apiClient.get<ClaimPaymentSummary>(
      `api/v1/posting/payments/by-claim/${claimID}`,
    )
    setSummary(res.data)
  }

  async function handlePostPayment(data: PostPaymentForm) {
    if (!summary) return
    setPosting(true)
    setPostError(null)
    try {
      const res = await apiClient.post<PostingResult>('api/v1/posting/payments', {
        claimID: summary.claimID,
        claimLineID: data.claimLineId ? Number(data.claimLineId) : null,
        amountPaid: Number(data.amountPaid),
        adjustments: data.adjustments.map((a) => ({
          group: a.group,
          carc: a.carc,
          amount: Number(a.amount),
        })),
      })
      setLastResult(res.data)
      setPostOpen(false)
      postForm.reset({ claimLineId: '', amountPaid: '', adjustments: [] })
      setActionSuccess(
        `Payment posted. Claim status: ${res.data.previousClaimStatus} → ${res.data.newClaimStatus}.${
          res.data.denialCreated ? ' A denial record was automatically created.' : ''
        }`,
      )
      await refreshSummary(summary.claimID)
    } catch (err) {
      setPostError(extractErrorMessage(err))
    } finally {
      setPosting(false)
    }
  }

  async function handleVoid(data: VoidForm) {
    if (!voidTarget || !summary) return
    setVoiding(true)
    setVoidError(null)
    try {
      const res = await apiClient.post<PostingResult>(
        `api/v1/posting/payments/${voidTarget.paymentID}/void`,
        { reason: data.reason || null },
      )
      setActionSuccess(
        `Payment #${voidTarget.paymentID} voided. Claim status: ${res.data.newClaimStatus}.`,
      )
      setVoidTarget(null)
      voidForm.reset()
      await refreshSummary(summary.claimID)
    } catch (err) {
      setVoidError(extractErrorMessage(err))
    } finally {
      setVoiding(false)
    }
  }

  const watchedAdj = postForm.watch('adjustments')
  const watchedPaid = parseFloat(postForm.watch('amountPaid')) || 0
  const adjTotal = watchedAdj.reduce((s, a) => s + (parseFloat(a.amount) || 0), 0)
  const chargeForBalance = summary?.totalCharge ?? 0
  const balanceDiff = chargeForBalance - (watchedPaid + adjTotal)
  const isBalanced = Math.abs(balanceDiff) <= 0.01

  return (
    <div className="flex flex-col gap-5">
      {/* Claim lookup */}
      <Card title="Claim Lookup">
        <p className="text-xs text-gray-500 mb-3">
          Enter a Claim ID to load its payment summary and post or void payments.
        </p>
        <div className="flex gap-3 items-end">
          <div className="max-w-xs flex-1">
            <label className="block text-xs font-medium text-gray-600 mb-1">Claim ID</label>
            <input
              type="number"
              min={1}
              value={claimIdInput}
              onChange={(e) => setClaimIdInput(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && loadClaim()}
              className={inputCls}
              placeholder="e.g. 42"
            />
          </div>
          <Button variant="primary" onClick={loadClaim} disabled={loadingClaim}>
            {loadingClaim ? 'Loading…' : 'Load Claim'}
          </Button>
        </div>
        {claimError && <p className="mt-3 text-sm text-red-600">{claimError}</p>}
      </Card>

      {actionSuccess && (
        <p className="rounded-md bg-green-50 px-4 py-3 text-sm text-green-700">{actionSuccess}</p>
      )}

      {/* Last posting result banner */}
      {lastResult && (
        <div className="rounded-xl border border-blue-200 bg-blue-50 px-5 py-4 text-sm space-y-2">
          <p className="font-semibold text-blue-800">
            Last Posting Result — Claim #{lastResult.claimID}
          </p>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            <span>
              Billed: <strong>{fmt(lastResult.totalCharge)}</strong>
            </span>
            <span>
              Paid: <strong className="text-green-700">{fmt(lastResult.totalPaid)}</strong>
            </span>
            <span>
              Pt. Resp.:{' '}
              <strong className="text-amber-700">{fmt(lastResult.totalPatientResponsibility)}</strong>
            </span>
            <span>
              Balance Due:{' '}
              <strong className="text-red-600">{fmt(lastResult.patientBalance.amountDue)}</strong>
            </span>
          </div>
          {lastResult.denialCreated && (
            <p className="text-amber-700 font-medium text-xs">
              A denial record was automatically created and added to the AR Worklist.
            </p>
          )}
        </div>
      )}

      {/* Claim summary + payment history */}
      {summary && (
        <Card title={`Claim #${summary.claimID} — Payment Summary`}>
          {/* Claim-level totals */}
          <div className="grid grid-cols-2 md:grid-cols-3 gap-x-6 gap-y-4 text-sm mb-5">
            <div className="flex flex-col gap-0.5">
              <span className="text-xs text-gray-500">Claim Status</span>
              <Badge status={summary.claimStatus} />
            </div>
            <div className="flex flex-col gap-0.5">
              <span className="text-xs text-gray-500">Total Charged</span>
              <span className="font-semibold text-gray-800">{fmt(summary.totalCharge)}</span>
            </div>
            <div className="flex flex-col gap-0.5">
              <span className="text-xs text-gray-500">Total Paid</span>
              <span className="font-semibold text-green-700">{fmt(summary.totalPaid)}</span>
            </div>
            <div className="flex flex-col gap-0.5">
              <span className="text-xs text-gray-500">Contractual Adj. (CO)</span>
              <span className="font-semibold text-blue-700">
                {fmt(summary.totalContractualAdjustment)}
              </span>
            </div>
            <div className="flex flex-col gap-0.5">
              <span className="text-xs text-gray-500">Patient Responsibility (PR)</span>
              <span className="font-semibold text-amber-700">
                {fmt(summary.totalPatientResponsibility)}
              </span>
            </div>
          </div>

          <div className="flex justify-end mb-4">
            <Button
              variant="primary"
              onClick={() => {
                setPostOpen(true)
                setPostError(null)
                postForm.reset({ claimLineId: '', amountPaid: '', adjustments: [] })
              }}
            >
              + Post Payment
            </Button>
          </div>

          <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
            <Table
              columns={POST_COLUMNS}
              data={summary.paymentPosts.map((p) => ({
                paymentId: `#${p.paymentID}`,
                scope: p.claimLineID
                  ? `Line ${p.lineNo ?? p.claimLineID}${p.cptHcpcs ? ` (${p.cptHcpcs})` : ''}`
                  : 'Claim Level',
                charged: fmt(p.chargeAmount),
                paid: fmt(p.amountPaid),
                adjusted: fmt(p.totalAdjusted),
                patientResp: fmt(p.patientResponsibility),
                postedDate: fmtDate(p.postedDate),
                postedBy: p.postedByName,
                status: <Badge status={p.status} />,
                _paymentID: p.paymentID,
                _isActive: p.status === 'Active',
              }))}
              showActions
              actions={[
                {
                  label: 'Void',
                  variant: 'danger',
                  onClick: (row) => {
                    if (!row._isActive) return
                    const item = summary.paymentPosts.find(
                      (p) => p.paymentID === (row._paymentID as number),
                    )
                    if (item) {
                      setVoidTarget(item)
                      setVoidError(null)
                      voidForm.reset()
                    }
                  },
                },
              ]}
            />
          </div>

          {summary.paymentPosts.length > 0 && (
            <p className="mt-3 text-xs text-gray-400 text-right">
              {summary.paymentPosts.length} payment post{summary.paymentPosts.length !== 1 ? 's' : ''}
            </p>
          )}
        </Card>
      )}

      {/* ── Post Payment Dialog ── */}
      <Dialog
        isOpen={postOpen}
        onClose={() => setPostOpen(false)}
        title={`Post Payment — Claim #${summary?.claimID}`}
        maxWidth="lg"
      >
        {summary && (
          <form onSubmit={postForm.handleSubmit(handlePostPayment)} className="space-y-5">
            {postError && (
              <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{postError}</p>
            )}

            {/* Billing equation explainer */}
            <div className="rounded-lg border border-blue-100 bg-blue-50 px-4 py-2.5 text-xs text-blue-700">
              <strong>Rule:</strong> Paid + Adjustments must equal the charge amount ({fmt(summary.totalCharge)}).
              If the payer paid less, record the difference as{' '}
              <strong>CO (contractual write-off)</strong> or <strong>PR (patient responsibility)</strong>.
              Use the <em>Quick-fill CO</em> button below to auto-fill the gap.
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Claim Line ID{' '}
                  <span className="text-gray-400 font-normal">(blank = claim level)</span>
                </label>
                <input
                  type="number"
                  min={1}
                  {...postForm.register('claimLineId')}
                  className={inputCls}
                  placeholder="Leave blank for claim-level"
                />
                <p className="mt-1 text-xs text-gray-400">
                  Leave blank to post against the entire claim.
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Amount Paid ($) <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  step="0.01"
                  min={0}
                  {...postForm.register('amountPaid', {
                    required: 'Amount paid is required',
                    min: { value: 0, message: 'Must be ≥ 0' },
                  })}
                  className={inputCls}
                  placeholder="0.00"
                />
                {postForm.formState.errors.amountPaid && (
                  <p className="mt-1 text-xs text-red-500">
                    {postForm.formState.errors.amountPaid.message}
                  </p>
                )}
              </div>
            </div>

            {/* Adjustments */}
            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-sm font-medium text-gray-700">
                  Adjustments{' '}
                  <span className="text-gray-400 font-normal text-xs">CO / PR / OA / PI</span>
                </label>
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => append({ group: 'CO', carc: '', amount: '' })}
                >
                  + Add Row
                </Button>
              </div>

              {/* Column headers */}
              {fields.length > 0 && (
                <div className="grid grid-cols-[80px_100px_1fr_32px] gap-2 mb-1 px-1">
                  <span className="text-xs text-gray-500 font-medium">Group</span>
                  <span className="text-xs text-gray-500 font-medium">CARC Code</span>
                  <span className="text-xs text-gray-500 font-medium">Amount ($)</span>
                  <span />
                </div>
              )}

              {fields.length === 0 && (
                <p className="text-xs text-gray-400 italic py-2">
                  No adjustments — add a row to record CO/PR write-offs.
                </p>
              )}

              <div className="space-y-2">
                {fields.map((field, idx) => (
                  <div key={field.id} className="grid grid-cols-[80px_100px_1fr_32px] gap-2 items-start">
                    <select
                      {...postForm.register(`adjustments.${idx}.group`)}
                      className={fieldCls}
                    >
                      {ADJ_GROUPS.map((g) => (
                        <option key={g} value={g}>{g}</option>
                      ))}
                    </select>
                    <input
                      type="text"
                      placeholder="e.g. 45"
                      {...postForm.register(`adjustments.${idx}.carc`, { required: true })}
                      className={fieldCls}
                    />
                    <input
                      type="number"
                      step="0.01"
                      min={0}
                      placeholder="0.00"
                      {...postForm.register(`adjustments.${idx}.amount`, {
                        required: true,
                        min: 0,
                      })}
                      className={fieldCls}
                    />
                    <button
                      type="button"
                      onClick={() => remove(idx)}
                      className="text-red-400 hover:text-red-600 py-2 text-sm font-medium"
                    >
                      ✕
                    </button>
                  </div>
                ))}
              </div>

              {/* Balance check strip */}
              <div
                className={`mt-3 rounded-lg px-4 py-3 text-xs space-y-1.5 ${
                  isBalanced ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'
                }`}
              >
                <div className="flex flex-wrap gap-4 items-center">
                  <span>
                    Paid: <strong>{fmt(watchedPaid)}</strong>
                  </span>
                  <span>
                    + Adjustments: <strong>{fmt(adjTotal)}</strong>
                  </span>
                  <span>
                    = Total: <strong>{fmt(watchedPaid + adjTotal)}</strong>
                  </span>
                  <span className="ml-auto">
                    Claim Charge: <strong>{fmt(summary.totalCharge)}</strong>
                  </span>
                </div>
                {!isBalanced && (
                  <div className="flex items-center justify-between pt-0.5">
                    <span className="font-semibold">
                      {fmt(Math.abs(balanceDiff))} unaccounted — add adjustments to balance.
                    </span>
                    <button
                      type="button"
                      className="underline font-semibold hover:opacity-80"
                      onClick={() =>
                        append({ group: 'CO', carc: '45', amount: balanceDiff.toFixed(2) })
                      }
                    >
                      Quick-fill CO (write-off)
                    </button>
                  </div>
                )}
                {isBalanced && (
                  <span className="font-semibold">Balanced ✓</span>
                )}
              </div>
            </div>

            <div className="flex justify-end gap-2 pt-1">
              <Button variant="secondary" onClick={() => setPostOpen(false)}>
                Cancel
              </Button>
              <Button variant="primary" type="submit" disabled={posting}>
                {posting ? 'Posting…' : 'Post Payment'}
              </Button>
            </div>
          </form>
        )}
      </Dialog>

      {/* ── Void Payment Dialog ── */}
      <Dialog
        isOpen={voidTarget !== null}
        onClose={() => setVoidTarget(null)}
        title={`Void Payment #${voidTarget?.paymentID}`}
      >
        {voidTarget && (
          <form onSubmit={voidForm.handleSubmit(handleVoid)} className="space-y-4">
            {voidError && (
              <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{voidError}</p>
            )}

            <div className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm space-y-1.5">
              <p className="font-semibold text-amber-800 mb-1">
                Voiding this payment will recalculate the claim status and patient balance.
              </p>
              <div className="flex gap-2">
                <span className="text-gray-500 w-28">Scope</span>
                <strong>
                  {voidTarget.claimLineID
                    ? `Line ${voidTarget.lineNo ?? voidTarget.claimLineID}`
                    : 'Claim Level'}
                </strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-28">Amount Paid</span>
                <strong className="text-green-700">{fmt(voidTarget.amountPaid)}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-28">Posted Date</span>
                <strong>{fmtDate(voidTarget.postedDate)}</strong>
              </div>
              <div className="flex gap-2">
                <span className="text-gray-500 w-28">Posted By</span>
                <strong>{voidTarget.postedByName}</strong>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Reason <span className="text-gray-400 font-normal">(optional)</span>
              </label>
              <textarea
                rows={2}
                {...voidForm.register('reason')}
                placeholder="e.g. Duplicate posting, incorrect line assignment"
                className="w-full resize-none border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div className="flex justify-end gap-2 pt-1">
              <Button variant="secondary" onClick={() => setVoidTarget(null)}>
                Cancel
              </Button>
              <Button variant="danger" type="submit" disabled={voiding}>
                {voiding ? 'Voiding…' : 'Void Payment'}
              </Button>
            </div>
          </form>
        )}
      </Dialog>
    </div>
  )
}
