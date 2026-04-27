import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import apiClient from '../../api/client'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import { Dialog } from '../../components/shared/ui/Dialog'
import { Table } from '../../components/shared/ui/Table'

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

type BatchClaimLine = {
  claimID: number
  patientName: string
  planName: string
  payerName: string
  totalCharge: number
  claimStatus: string
  payloadUri: string | null
  submissionRefs: SubmissionRef[]
}

type BatchDetail = {
  batchID: number
  batchDate: string
  itemCount: number
  totalCharge: number
  status: string
  claims: BatchClaimLine[]
}

type EligibleClaim = {
  claimID: number
  patientName: string
  planName: string
  payerName: string
  totalCharge: number
  claimStatus?: string
}

type ClaimListResponse = {
  totalCount: number
  claims: EligibleClaim[]
}

const inputClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

function extractErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const resp = (err as { response?: { data?: { message?: string } } }).response
    if (resp?.data?.message) return resp.data.message
  }
  return 'An unexpected error occurred.'
}

export default function BatchDetail() {
  const { batchId } = useParams<{ batchId: string }>()
  const navigate = useNavigate()
  const batchIdNum = Number(batchId ?? '0')

  const [detail, setDetail] = useState<BatchDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)

  const [showAddClaims, setShowAddClaims] = useState(false)
  const [eligibleClaims, setEligibleClaims] = useState<EligibleClaim[]>([])
  const [loadingEligible, setLoadingEligible] = useState(false)
  const [selectedClaimIDs, setSelectedClaimIDs] = useState<number[]>([])

  const [adding, setAdding] = useState(false)
  const [generating, setGenerating] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [recording999, setRecording999] = useState(false)
  const [recording277, setRecording277] = useState(false)

  const [submitForm, setSubmitForm] = useState({ clearinghouseID: '', submitDate: '' })
  const [ack999Form, setAck999Form] = useState({
    ackStatus: 'Accepted',
    clearinghouseID: '',
    correlationID: '',
    ackDate: '',
    notes: '',
  })
  const [active277ClaimID, setActive277ClaimID] = useState<number | null>(null)
  const [ack277Form, setAck277Form] = useState({
    ackStatus: 'Accepted',
    ackDate: '',
    correlationID: '',
    notes: '',
  })

  const loadDetail = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await apiClient.get<BatchDetail>(`api/v1/batch/${batchIdNum}`)
      setDetail(res.data)
    } catch (err) {
      setError(extractErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [batchIdNum])

  const refreshDetail = useCallback(async () => {
    try {
      const res = await apiClient.get<BatchDetail>(`api/v1/batch/${batchIdNum}`)
      setDetail(res.data)
    } catch (err) {
      setActionError(extractErrorMessage(err))
    }
  }, [batchIdNum])

  useEffect(() => {
    loadDetail()
  }, [loadDetail])

  // Pre-fill 999 form clearinghouse/correlation from submit ref when status is Submitted
  useEffect(() => {
    if (!detail || detail.status !== 'Submitted') return
    const allRefs = detail.claims.flatMap((c) => c.submissionRefs)
    const submitRef = allRefs.find((r) => r.ackType == null)
    if (submitRef) {
      setAck999Form((f) => ({
        ...f,
        clearinghouseID: submitRef.clearinghouseID ?? '',
        correlationID: submitRef.correlationID ?? '',
      }))
    }
  }, [detail?.status])

  const claims = detail?.claims ?? []

  const allRefs = useMemo(() => claims.flatMap((c) => c.submissionRefs), [claims])
  const submitRef = useMemo(() => allRefs.find((r) => r.ackType == null) ?? null, [allRefs])
  const ack999Ref = useMemo(() => allRefs.find((r) => r.ackType === '999') ?? null, [allRefs])
  const get277Ref = useCallback(
    (claimID: number) => allRefs.find((r) => r.ackType === '277CA' && r.claimID === claimID) ?? null,
    [allRefs],
  )
  const totalCharge = useMemo(() => claims.reduce((s, c) => s + c.totalCharge, 0), [claims])
  const total277 = claims.length
  const recorded277 = useMemo(
    () => claims.filter((c) => get277Ref(c.claimID) != null).length,
    [claims, get277Ref],
  )
  const alreadyAdded = useMemo(() => new Set(claims.map((c) => c.claimID)), [claims])

  async function openAddClaims() {
    setSelectedClaimIDs([])
    setShowAddClaims(true)
    setLoadingEligible(true)
    try {
      const [readyRes, rejectedRes] = await Promise.all([
        apiClient.get<ClaimListResponse>('api/claims?claimStatus=Ready&pageSize=100'),
        apiClient.get<ClaimListResponse>('api/claims?claimStatus=Rejected&pageSize=100'),
      ])
      const combined = [
        ...readyRes.data.claims.map((c) => ({ ...c, claimStatus: 'Ready' })),
        ...rejectedRes.data.claims.map((c) => ({ ...c, claimStatus: 'Rejected' })),
      ].filter((c) => !alreadyAdded.has(c.claimID))
      setEligibleClaims(combined)
    } catch {
      setEligibleClaims([])
    } finally {
      setLoadingEligible(false)
    }
  }

  async function handleAddClaims() {
    if (selectedClaimIDs.length === 0) return
    setAdding(true)
    setActionError(null)
    try {
      const res = await apiClient.post<{
        claimsAdded: number
        failedClaimIDs: number[]
        failureReasons: string[]
      }>(`api/v1/batch/${batchIdNum}/claims`, { claimIDs: selectedClaimIDs })

      const { claimsAdded, failedClaimIDs, failureReasons } = res.data
      if (claimsAdded > 0) {
        setShowAddClaims(false)
        setSelectedClaimIDs([])
        await refreshDetail()
      }
      if (failedClaimIDs.length > 0) {
        setActionError(`${failedClaimIDs.length} claim(s) could not be added: ${failureReasons[0]}`)
      }
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setAdding(false)
    }
  }

  async function handleRemoveClaim(claimID: number) {
    setActionError(null)
    try {
      await apiClient.delete(`api/v1/batch/${batchIdNum}/claims/${claimID}`)
      await refreshDetail()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    }
  }

  async function handleGenerate() {
    setGenerating(true)
    setActionError(null)
    try {
      await apiClient.post(`api/v1/batch/${batchIdNum}/generate`)
      await refreshDetail()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setGenerating(false)
    }
  }

  async function handleSubmit() {
    if (!submitForm.clearinghouseID || !submitForm.submitDate) return
    setSubmitting(true)
    setActionError(null)
    try {
      await apiClient.post(`api/v1/batch/${batchIdNum}/submit`, {
        clearinghouseID: submitForm.clearinghouseID,
        submitDate: submitForm.submitDate,
      })
      await refreshDetail()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  async function handleRecord999() {
    if (!ack999Form.ackDate) return
    setRecording999(true)
    setActionError(null)
    try {
      await apiClient.post(`api/v1/batch/${batchIdNum}/ack/999`, {
        clearinghouseID: ack999Form.clearinghouseID,
        correlationID: ack999Form.correlationID,
        ackStatus: ack999Form.ackStatus,
        ackDate: ack999Form.ackDate,
        notes: ack999Form.notes || null,
      })
      await refreshDetail()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setRecording999(false)
    }
  }

  async function handleRecord277(claimID: number) {
    if (!ack277Form.ackDate) return
    setRecording277(true)
    setActionError(null)
    try {
      await apiClient.post(`api/v1/batch/${batchIdNum}/ack/277ca/${claimID}`, {
        claimID,
        ackStatus: ack277Form.ackStatus,
        ackDate: ack277Form.ackDate,
        correlationID: ack277Form.correlationID || null,
        notes: ack277Form.notes || null,
      })
      setActive277ClaimID(null)
      await refreshDetail()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setRecording277(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600" />
      </div>
    )
  }

  if (error || !detail) {
    return (
      <div className="space-y-4">
        <button
          type="button"
          className="text-blue-600 hover:underline cursor-pointer"
          onClick={() => navigate('/frontdesk/batches')}
        >
          ← Back to Batches
        </button>
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error ?? 'Batch not found.'}</p>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-6">
      <button
        type="button"
        className="text-blue-600 hover:underline cursor-pointer text-left"
        onClick={() => navigate('/frontdesk/batches')}
      >
        ← Back to Batches
      </button>

      {actionError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{actionError}</p>
      )}

      {/* Section A: Batch Info Header */}
      <Card title="Batch Information">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 text-sm">
          <div>
            <p className="text-gray-500">Batch ID</p>
            <p className="font-semibold">#{detail.batchID}</p>
          </div>
          <div>
            <p className="text-gray-500">Batch Date</p>
            <p className="font-semibold">{detail.batchDate || '—'}</p>
          </div>
          <div>
            <p className="text-gray-500">Total Claims</p>
            <p className="font-semibold">{claims.length}</p>
          </div>
          <div>
            <p className="text-gray-500">Total Charge</p>
            <p className="font-semibold">${totalCharge.toFixed(2)}</p>
          </div>
          <div className="md:col-span-4">
            <p className="text-gray-500 mb-1">Status</p>
            <Badge status={detail.status} />
          </div>
        </div>
      </Card>

      {/* Failed banner */}
      {detail.status === 'Failed' && (
        <div className="bg-red-50 border border-red-300 rounded-lg px-4 py-3 text-red-700 text-sm">
          ⚠ 999 ACK rejected by clearinghouse. All claims in this batch have been set to <strong>Rejected</strong> status.
          Create a new batch, correct the issues, and re-add them via "Add Claims".
        </div>
      )}

      {/* Section B: Claims in Batch (Open) */}
      {detail.status === 'Open' && (
        <Card title="Claims in Batch">
          <div className="flex justify-end mb-4">
            <Button variant="secondary" size="sm" onClick={openAddClaims}>
              Add Claims
            </Button>
          </div>

          <Table
            columns={[
              { key: 'claimIDText', label: 'Claim ID' },
              { key: 'patientName', label: 'Patient Name' },
              { key: 'payerName', label: 'Payer' },
              { key: 'planName', label: 'Plan' },
              { key: 'totalChargeText', label: 'Total Charge' },
              { key: 'statusBadge', label: 'Status' },
              { key: 'readyIcon', label: '837P Ready' },
            ]}
            data={claims.map((row) => ({
              ...row,
              claimIDText: `#${row.claimID}`,
              totalChargeText: `$${row.totalCharge.toFixed(2)}`,
              statusBadge: <Badge status={row.claimStatus} />,
              readyIcon: row.payloadUri ? (
                <span className="text-green-600 font-medium">✓ Ready</span>
              ) : (
                <span className="text-red-600 font-medium">✗ Missing</span>
              ),
            }))}
            showActions={true}
            actions={[
              {
                label: 'Remove',
                variant: 'danger',
                onClick: (row) => handleRemoveClaim((row as BatchClaimLine).claimID),
              },
            ]}
          />

          {claims.length === 0 && (
            <p className="text-gray-500 italic mt-4">
              No claims added yet. Click 'Add Claims' to add eligible claims.
            </p>
          )}

          <div className="flex justify-end mt-4">
            <Button
              variant="primary"
              disabled={claims.length === 0 || generating}
              onClick={handleGenerate}
            >
              {generating ? 'Generating...' : 'Generate Batch'}
            </Button>
          </div>
          {claims.length === 0 && (
            <p className="text-xs text-gray-500 text-right mt-2">
              Add at least one claim to generate the batch.
            </p>
          )}
        </Card>
      )}

      {/* Section C: Submit (Generated) */}
      {detail.status === 'Generated' && (
        <Card title="Submit Batch">
          <div className="bg-gray-50 rounded-lg px-4 py-3 flex flex-wrap gap-6 mb-4 text-sm">
            <span>Total Claims: {claims.length}</span>
            <span>Total Charge: ${totalCharge.toFixed(2)}</span>
            <span className="flex items-center gap-2">
              Status: <Badge status="Generated" />
            </span>
          </div>
          <p className="text-sm text-gray-500 mb-4 italic">
            The actual EDI file transmission happens outside TeleBill. Record the submission details here once
            the file has been sent to the clearinghouse.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Clearinghouse ID *</label>
              <input
                value={submitForm.clearinghouseID}
                onChange={(e) => setSubmitForm((f) => ({ ...f, clearinghouseID: e.target.value }))}
                placeholder="e.g. CHC-BC01"
                className={inputClassName}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Submit Date *</label>
              <input
                type="date"
                value={submitForm.submitDate}
                onChange={(e) => setSubmitForm((f) => ({ ...f, submitDate: e.target.value }))}
                className={inputClassName}
              />
            </div>
          </div>
          <Button
            variant="primary"
            onClick={handleSubmit}
            disabled={!submitForm.clearinghouseID || !submitForm.submitDate || submitting}
          >
            {submitting ? 'Submitting...' : 'Mark as Submitted'}
          </Button>
        </Card>
      )}

      {/* Section D: 999 ACK (Submitted) */}
      {detail.status === 'Submitted' && (
        <Card title="999 Functional Acknowledgement">
          <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 text-sm mb-4">
            The 999 ACK is the batch-level acknowledgement from the clearinghouse confirming whether your EDI
            submission was structurally valid.
          </div>
          {submitRef && (
            <div className="bg-gray-50 rounded-lg px-4 py-3 mb-4 text-sm grid grid-cols-1 md:grid-cols-3 gap-3">
              <div>
                <span className="text-gray-500">Clearinghouse ID:</span>{' '}
                <strong>{submitRef.clearinghouseID}</strong>
              </div>
              <div>
                <span className="text-gray-500">Correlation ID:</span>{' '}
                <strong>{submitRef.correlationID}</strong>
              </div>
              <div>
                <span className="text-gray-500">Submit Date:</span>{' '}
                <strong>{submitRef.submitDate}</strong>
              </div>
            </div>
          )}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">ACK Status *</label>
              <div className="flex gap-3">
                {['Accepted', 'Rejected'].map((opt) => (
                  <label
                    key={opt}
                    className={`flex items-center gap-2 px-4 py-2 rounded-lg border cursor-pointer text-sm font-medium ${
                      ack999Form.ackStatus === opt
                        ? opt === 'Accepted'
                          ? 'bg-green-50 border-green-500 text-green-700'
                          : 'bg-red-50 border-red-500 text-red-700'
                        : 'border-gray-300 text-gray-600'
                    }`}
                  >
                    <input
                      type="radio"
                      name="ack999Status"
                      value={opt}
                      checked={ack999Form.ackStatus === opt}
                      onChange={() => setAck999Form((f) => ({ ...f, ackStatus: opt }))}
                      className="sr-only"
                    />
                    {opt === 'Accepted' ? '✓' : '✗'} {opt}
                  </label>
                ))}
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">ACK Date *</label>
              <input
                type="date"
                value={ack999Form.ackDate}
                onChange={(e) => setAck999Form((f) => ({ ...f, ackDate: e.target.value }))}
                className={inputClassName}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Clearinghouse ID</label>
              <input
                value={ack999Form.clearinghouseID}
                onChange={(e) => setAck999Form((f) => ({ ...f, clearinghouseID: e.target.value }))}
                className={inputClassName}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Correlation ID</label>
              <input
                value={ack999Form.correlationID}
                onChange={(e) => setAck999Form((f) => ({ ...f, correlationID: e.target.value }))}
                className={inputClassName}
              />
            </div>
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
              <textarea
                rows={2}
                value={ack999Form.notes}
                onChange={(e) => setAck999Form((f) => ({ ...f, notes: e.target.value }))}
                className={inputClassName}
              />
            </div>
          </div>
          <div className="mt-4">
            <Button
              variant="primary"
              onClick={handleRecord999}
              disabled={!ack999Form.ackDate || recording999}
            >
              {recording999 ? 'Recording...' : 'Record 999 ACK'}
            </Button>
          </div>
        </Card>
      )}

      {/* Section E: 277CA per-claim ACK (Acked) */}
      {detail.status === 'Acked' && (
        <Card title="277CA Claim-Level Acknowledgements">
          <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 text-sm mb-4">
            Record the individual claim-level 277CA responses. Each claim must be marked Accepted or Rejected
            separately.
          </div>
          <div className="mb-4">
            <p className="text-sm text-gray-600 mb-2">
              {recorded277} of {total277} claims acknowledged
            </p>
            <div className="w-full bg-gray-100 rounded-full h-2">
              <div
                style={{ width: `${total277 === 0 ? 0 : (recorded277 / total277) * 100}%` }}
                className="bg-blue-500 h-2 rounded-full transition-all"
              />
            </div>
          </div>

          <div className="border rounded-lg overflow-hidden">
            <div className="grid grid-cols-6 gap-3 bg-gray-50 px-4 py-2 text-xs font-semibold text-gray-500 uppercase tracking-wide">
              <span>Claim ID</span>
              <span>Patient</span>
              <span>Payer</span>
              <span>Charge</span>
              <span>277CA Status</span>
              <span>Actions</span>
            </div>
            {claims.map((claim) => {
              const ref277 = get277Ref(claim.claimID)
              const isActive = active277ClaimID === claim.claimID
              return (
                <div key={claim.claimID} className="border-t">
                  <div className="grid grid-cols-6 gap-3 px-4 py-3 items-center text-sm">
                    <span className="font-mono text-gray-600">#{claim.claimID}</span>
                    <span>{claim.patientName}</span>
                    <span className="text-gray-500">{claim.payerName}</span>
                    <span>${claim.totalCharge.toFixed(2)}</span>
                    <span>
                      {ref277 ? (
                        <Badge status={ref277.ackStatus ?? ''} />
                      ) : (
                        <span className="text-amber-600 text-xs font-medium">Pending</span>
                      )}
                    </span>
                    <span>
                      {!ref277 && !isActive && (
                        <button
                          type="button"
                          onClick={() => {
                            setActive277ClaimID(claim.claimID)
                            setAck277Form({
                              ackStatus: 'Accepted',
                              ackDate: '',
                              correlationID: ack999Ref?.correlationID ?? '',
                              notes: '',
                            })
                          }}
                          className="text-blue-600 text-sm hover:underline font-medium"
                        >
                          Record ACK
                        </button>
                      )}
                      {ref277 && <span className="text-green-600 text-xs">✓ Recorded</span>}
                    </span>
                  </div>

                  {isActive && (
                    <div className="bg-blue-50 border-t px-4 py-4">
                      <p className="text-xs font-semibold text-blue-700 mb-3 uppercase">
                        Record 277CA for {claim.patientName}
                      </p>
                      <div className="grid grid-cols-2 gap-3 mb-3">
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">ACK Status *</label>
                          <div className="flex gap-2">
                            {['Accepted', 'Rejected'].map((opt) => (
                              <label
                                key={opt}
                                className={`flex items-center gap-1 px-3 py-1.5 rounded-lg border cursor-pointer text-xs font-medium ${
                                  ack277Form.ackStatus === opt
                                    ? opt === 'Accepted'
                                      ? 'bg-green-50 border-green-500 text-green-700'
                                      : 'bg-red-50 border-red-500 text-red-700'
                                    : 'border-gray-300 text-gray-600'
                                }`}
                              >
                                <input
                                  type="radio"
                                  name="ack277Status"
                                  value={opt}
                                  checked={ack277Form.ackStatus === opt}
                                  onChange={() => setAck277Form((f) => ({ ...f, ackStatus: opt }))}
                                  className="sr-only"
                                />
                                {opt === 'Accepted' ? '✓' : '✗'} {opt}
                              </label>
                            ))}
                          </div>
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">ACK Date *</label>
                          <input
                            type="date"
                            value={ack277Form.ackDate}
                            onChange={(e) => setAck277Form((f) => ({ ...f, ackDate: e.target.value }))}
                            className="w-full border border-gray-300 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">Correlation ID</label>
                          <input
                            value={ack277Form.correlationID}
                            onChange={(e) => setAck277Form((f) => ({ ...f, correlationID: e.target.value }))}
                            className="w-full border border-gray-300 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">Notes</label>
                          <input
                            value={ack277Form.notes}
                            onChange={(e) => setAck277Form((f) => ({ ...f, notes: e.target.value }))}
                            placeholder="Optional"
                            className="w-full border border-gray-300 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
                          />
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <Button
                          variant="primary"
                          size="sm"
                          onClick={() => handleRecord277(claim.claimID)}
                          disabled={!ack277Form.ackDate || recording277}
                        >
                          {recording277 ? 'Recording...' : 'Record 277CA'}
                        </Button>
                        <Button
                          variant="secondary"
                          size="sm"
                          onClick={() => setActive277ClaimID(null)}
                        >
                          Cancel
                        </Button>
                      </div>
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        </Card>
      )}

      {/* Add Claims Dialog */}
      <Dialog
        isOpen={showAddClaims}
        onClose={() => {
          setShowAddClaims(false)
          setSelectedClaimIDs([])
        }}
        title="Add Claims to Batch"
        maxWidth="xl"
      >
        <p className="text-sm text-gray-500 mb-3">
          Showing <strong>Ready</strong> and <strong>Rejected</strong> claims not already in this batch.
          Rejected claims were previously submitted but refused by the payer — correct the issue before resubmitting.
        </p>
        {loadingEligible ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
          </div>
        ) : (
          <div className="max-h-96 overflow-y-auto">
            {eligibleClaims.map((claim) => (
              <div
                key={claim.claimID}
                onClick={() =>
                  setSelectedClaimIDs((prev) =>
                    prev.includes(claim.claimID)
                      ? prev.filter((id) => id !== claim.claimID)
                      : [...prev, claim.claimID],
                  )
                }
                className={`flex items-center gap-3 px-3 py-2 rounded-lg cursor-pointer border mb-2 ${
                  selectedClaimIDs.includes(claim.claimID)
                    ? 'bg-blue-50 border-blue-400'
                    : 'border-gray-200 hover:bg-gray-50'
                }`}
              >
                <input
                  type="checkbox"
                  readOnly
                  checked={selectedClaimIDs.includes(claim.claimID)}
                  className="h-4 w-4"
                />
                <span className="flex-1 text-sm font-medium">{claim.patientName}</span>
                <span className="text-sm text-gray-500">{claim.payerName}</span>
                <span className="text-sm text-gray-500">{claim.planName}</span>
                <span className="text-sm font-medium">${claim.totalCharge.toFixed(2)}</span>
                <Badge status={claim.claimStatus ?? 'Ready'} />
              </div>
            ))}
            {eligibleClaims.length === 0 && (
              <p className="text-sm text-gray-500">
                No eligible claims available. All ready claims have been added.
              </p>
            )}
          </div>
        )}
        <div className="mt-4 flex items-center justify-between">
          <span className="text-sm text-gray-500">{selectedClaimIDs.length} selected</span>
          <div className="flex gap-2">
            <Button variant="secondary" onClick={() => setShowAddClaims(false)}>
              Cancel
            </Button>
            <Button
              variant="primary"
              disabled={selectedClaimIDs.length === 0 || adding}
              onClick={handleAddClaims}
            >
              {adding ? 'Adding...' : 'Add Selected'}
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  )
}
