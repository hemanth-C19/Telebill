import { useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import { Dialog } from '../../components/shared/ui/Dialog'
import { Table } from '../../components/shared/ui/Table'

type BatchStatus = 'Open' | 'Generated' | 'Submitted' | 'Acked' | 'Failed'
type ClaimStatus = 'Draft' | 'ScrubError' | 'Ready' | 'Batched' | 'Submitted' | 'Accepted' | 'Rejected'

type BatchClaimLine = {
  claimId: number
  patientName: string
  planName: string
  payerName: string
  totalCharge: number
  claimStatus: ClaimStatus
  payloadUri: string
}

type SubmissionRef = {
  submitId: number
  batchId: number
  claimId: number
  clearinghouseId: string
  correlationId: string
  submitDate: string
  ackType: '999' | '277CA'
  ackStatus: string
  ackDate: string
  notes: string
}

type Batch = {
  batchId: number
  batchDate: string
  itemCount: number
  totalCharge: number
  status: BatchStatus
}

type EligibleClaim = {
  claimId: number
  patientName: string
  payerName: string
  planName: string
  totalCharge: number
  payloadUri: string
}

const DUMMY_BATCH_DETAILS: Record<number, { batch: Batch; claims: BatchClaimLine[]; refs: SubmissionRef[] }> = {
  3001: { batch: { batchId: 3001, batchDate: '2024-11-10', itemCount: 0, totalCharge: 0, status: 'Open' }, claims: [], refs: [] },
  3002: {
    batch: { batchId: 3002, batchDate: '2024-11-12', itemCount: 3, totalCharge: 595.0, status: 'Open' },
    claims: [
      { claimId: 8001, patientName: 'Alice Johnson', planName: 'BCBS PPO Basic', payerName: 'BlueCross BlueShield', totalCharge: 235.0, claimStatus: 'Batched', payloadUri: '837p-8001.edi' },
      { claimId: 8002, patientName: 'Bob Martinez', planName: 'Aetna Select PPO', payerName: 'Aetna', totalCharge: 200.0, claimStatus: 'Batched', payloadUri: '837p-8002.edi' },
      { claimId: 8003, patientName: 'Emily Rodriguez', planName: 'BCBS PPO Basic', payerName: 'BlueCross BlueShield', totalCharge: 160.0, claimStatus: 'Batched', payloadUri: '837p-8003.edi' },
    ],
    refs: [],
  },
  3003: {
    batch: { batchId: 3003, batchDate: '2024-11-15', itemCount: 2, totalCharge: 335.0, status: 'Generated' },
    claims: [
      { claimId: 8004, patientName: 'David Patel', planName: 'UHC Gold PPO', payerName: 'United Healthcare', totalCharge: 175.0, claimStatus: 'Batched', payloadUri: '837p-8004.edi' },
      { claimId: 8005, patientName: 'Grace Kim', planName: 'Cigna HMO', payerName: 'Cigna', totalCharge: 160.0, claimStatus: 'Batched', payloadUri: '837p-8005.edi' },
    ],
    refs: [],
  },
  3004: {
    batch: { batchId: 3004, batchDate: '2024-11-18', itemCount: 4, totalCharge: 820.0, status: 'Submitted' },
    claims: [
      { claimId: 8006, patientName: 'Alice Johnson', planName: 'BCBS PPO Basic', payerName: 'BlueCross BlueShield', totalCharge: 235.0, claimStatus: 'Submitted', payloadUri: '837p-8006.edi' },
      { claimId: 8007, patientName: 'Bob Martinez', planName: 'Aetna Select PPO', payerName: 'Aetna', totalCharge: 175.0, claimStatus: 'Submitted', payloadUri: '837p-8007.edi' },
      { claimId: 8008, patientName: 'Carol Nguyen', planName: 'BCBS HMO Plus', payerName: 'BlueCross BlueShield', totalCharge: 220.0, claimStatus: 'Submitted', payloadUri: '837p-8008.edi' },
      { claimId: 8009, patientName: 'David Patel', planName: 'UHC Gold PPO', payerName: 'United Healthcare', totalCharge: 190.0, claimStatus: 'Submitted', payloadUri: '837p-8009.edi' },
    ],
    refs: [{ submitId: 7001, batchId: 3004, claimId: 0, clearinghouseId: 'CHC-BC01', correlationId: 'ISA-20241118-001', submitDate: '2024-11-18', ackType: '999', ackStatus: '', ackDate: '', notes: '' }],
  },
  3005: {
    batch: { batchId: 3005, batchDate: '2024-11-20', itemCount: 3, totalCharge: 505.0, status: 'Acked' },
    claims: [
      { claimId: 8010, patientName: 'Emily Rodriguez', planName: 'BCBS PPO Basic', payerName: 'BlueCross BlueShield', totalCharge: 200.0, claimStatus: 'Submitted', payloadUri: '837p-8010.edi' },
      { claimId: 8011, patientName: 'Grace Kim', planName: 'Cigna HMO', payerName: 'Cigna', totalCharge: 160.0, claimStatus: 'Accepted', payloadUri: '837p-8011.edi' },
      { claimId: 8012, patientName: 'Frank Williams', planName: 'UHC Silver HMO', payerName: 'United Healthcare', totalCharge: 145.0, claimStatus: 'Submitted', payloadUri: '837p-8012.edi' },
    ],
    refs: [
      { submitId: 7002, batchId: 3005, claimId: 0, clearinghouseId: 'CHC-UH01', correlationId: 'ISA-20241120-002', submitDate: '2024-11-20', ackType: '999', ackStatus: 'Accepted', ackDate: '2024-11-21', notes: '' },
      { submitId: 7003, batchId: 3005, claimId: 8011, clearinghouseId: 'CHC-UH01', correlationId: 'ISA-20241120-002', submitDate: '2024-11-20', ackType: '277CA', ackStatus: 'Accepted', ackDate: '2024-11-22', notes: '' },
    ],
  },
  3006: {
    batch: { batchId: 3006, batchDate: '2024-11-22', itemCount: 2, totalCharge: 280.0, status: 'Failed' },
    claims: [
      { claimId: 8013, patientName: 'Alice Johnson', planName: 'BCBS PPO Basic', payerName: 'BlueCross BlueShield', totalCharge: 150.0, claimStatus: 'Draft', payloadUri: '837p-8013.edi' },
      { claimId: 8014, patientName: 'Bob Martinez', planName: 'Aetna Select PPO', payerName: 'Aetna', totalCharge: 130.0, claimStatus: 'Draft', payloadUri: '837p-8014.edi' },
    ],
    refs: [{ submitId: 7004, batchId: 3006, claimId: 0, clearinghouseId: 'CHC-AE01', correlationId: 'ISA-20241122-003', submitDate: '2024-11-22', ackType: '999', ackStatus: 'Rejected', ackDate: '2024-11-23', notes: 'Schema validation failed' }],
  },
}

const DUMMY_ELIGIBLE_CLAIMS: EligibleClaim[] = [
  { claimId: 8020, patientName: 'Alice Johnson', payerName: 'BlueCross BlueShield', planName: 'BCBS PPO Basic', totalCharge: 150.0, payloadUri: '837p-8020.edi' },
  { claimId: 8021, patientName: 'David Patel', payerName: 'United Healthcare', planName: 'UHC Gold PPO', totalCharge: 200.0, payloadUri: '837p-8021.edi' },
  { claimId: 8022, patientName: 'Grace Kim', payerName: 'Cigna', planName: 'Cigna HMO', totalCharge: 160.0, payloadUri: '837p-8022.edi' },
  { claimId: 8023, patientName: 'Emily Rodriguez', payerName: 'Aetna', planName: 'Aetna Select PPO', totalCharge: 175.0, payloadUri: '837p-8023.edi' },
  { claimId: 8024, patientName: 'Frank Williams', payerName: 'BlueCross BlueShield', planName: 'BCBS HMO Plus', totalCharge: 110.0, payloadUri: '837p-8024.edi' },
]

const inputClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

export default function BatchDetail() {
  const { batchId } = useParams<{ batchId: string }>()
  const navigate = useNavigate()
  const batchIdNum = Number(batchId ?? '3001')

  const detail = DUMMY_BATCH_DETAILS[batchIdNum]
  const [batch, setBatch] = useState<Batch>(detail?.batch ?? { batchId: batchIdNum, batchDate: '', itemCount: 0, totalCharge: 0, status: 'Open' })
  const [claims, setClaims] = useState<BatchClaimLine[]>(detail?.claims ?? [])
  const [submissionRefs, setSubmissionRefs] = useState<SubmissionRef[]>(detail?.refs ?? [])
  const [showAddClaims, setShowAddClaims] = useState(false)
  const [selectedClaimIds, setSelectedClaimIds] = useState<number[]>([])
  const [submitForm, setSubmitForm] = useState({ clearinghouseId: '', submitDate: '' })
  const [ack999Form, setAck999Form] = useState({
    ackStatus: 'Accepted',
    clearinghouseId: '',
    correlationId: '',
    ackDate: '',
    notes: '',
  })
  const [active277ClaimId, setActive277ClaimId] = useState<number | null>(null)
  const [ack277Form, setAck277Form] = useState({ ackStatus: 'Accepted', ackDate: '', correlationId: '', notes: '' })

  const ref999 = submissionRefs.find((r) => r.ackType === '999')
  const get277Ref = (claimId: number) => submissionRefs.find((r) => r.ackType === '277CA' && r.claimId === claimId)
  const totalCharge = claims.reduce((sum, c) => sum + c.totalCharge, 0)

  const alreadyAdded = useMemo(() => new Set(claims.map((c) => c.claimId)), [claims])
  const eligible = DUMMY_ELIGIBLE_CLAIMS.filter((c) => !alreadyAdded.has(c.claimId))

  const handleGenerateBatch = () => {
    setBatch((prev) => ({ ...prev, status: 'Generated' }))
  }

  const handleRemoveClaim = (claimId: number) => {
    const removedCharge = claims.find((c) => c.claimId === claimId)?.totalCharge ?? 0
    setClaims((prev) => prev.filter((c) => c.claimId !== claimId))
    setBatch((prev) => ({
      ...prev,
      itemCount: Math.max(0, prev.itemCount - 1),
      totalCharge: Math.max(0, prev.totalCharge - removedCharge),
    }))
  }

  const handleAddSelectedClaims = () => {
    const toAdd = DUMMY_ELIGIBLE_CLAIMS.filter((c) => selectedClaimIds.includes(c.claimId))
    const newLines: BatchClaimLine[] = toAdd.map((c) => ({ ...c, claimStatus: 'Batched' }))
    setClaims((prev) => [...prev, ...newLines])
    setBatch((prev) => ({
      ...prev,
      itemCount: prev.itemCount + newLines.length,
      totalCharge: prev.totalCharge + newLines.reduce((s, c) => s + c.totalCharge, 0),
    }))
    setShowAddClaims(false)
    setSelectedClaimIds([])
  }

  const handleMarkSubmitted = () => {
    if (!submitForm.clearinghouseId || !submitForm.submitDate) return
    const newRef: SubmissionRef = {
      submitId: Math.floor(Math.random() * 9000) + 7100,
      batchId: batchIdNum,
      claimId: 0,
      clearinghouseId: submitForm.clearinghouseId,
      correlationId: `ISA-${submitForm.submitDate.replace(/-/g, '')}-${batchIdNum}`,
      submitDate: submitForm.submitDate,
      ackType: '999',
      ackStatus: '',
      ackDate: '',
      notes: '',
    }
    setSubmissionRefs((prev) => [...prev, newRef])
    setAck999Form((f) => ({ ...f, clearinghouseId: submitForm.clearinghouseId, correlationId: newRef.correlationId }))
    setBatch((prev) => ({ ...prev, status: 'Submitted' }))
    setClaims((prev) => prev.map((c) => ({ ...c, claimStatus: 'Submitted' })))
  }

  const handleRecord999 = () => {
    if (!ack999Form.ackDate) return
    setSubmissionRefs((prev) =>
      prev.map((r) =>
        r.ackType === '999' && r.claimId === 0
          ? {
              ...r,
              ackStatus: ack999Form.ackStatus,
              ackDate: ack999Form.ackDate,
              correlationId: ack999Form.correlationId,
              notes: ack999Form.notes,
            }
          : r,
      ),
    )
    if (ack999Form.ackStatus === 'Accepted') {
      setBatch((prev) => ({ ...prev, status: 'Acked' }))
    } else {
      setBatch((prev) => ({ ...prev, status: 'Failed' }))
      setClaims((prev) => prev.map((c) => ({ ...c, claimStatus: 'Draft' })))
    }
  }

  const handleRecord277 = (claimId: number) => {
    if (!ack277Form.ackDate) return
    const newRef: SubmissionRef = {
      submitId: Math.floor(Math.random() * 9000) + 7200,
      batchId: batchIdNum,
      claimId,
      clearinghouseId: ref999?.clearinghouseId ?? '',
      correlationId: ack277Form.correlationId,
      submitDate: ref999?.submitDate ?? '',
      ackType: '277CA',
      ackStatus: ack277Form.ackStatus,
      ackDate: ack277Form.ackDate,
      notes: ack277Form.notes,
    }
    setSubmissionRefs((prev) => [...prev, newRef])
    setClaims((prev) =>
      prev.map((c) => (c.claimId === claimId ? { ...c, claimStatus: ack277Form.ackStatus === 'Accepted' ? 'Accepted' : 'Draft' } : c)),
    )
    setActive277ClaimId(null)
    setAck277Form({ ackStatus: 'Accepted', ackDate: '', correlationId: ref999?.correlationId ?? '', notes: '' })
  }

  const total277 = claims.length
  const recorded277 = claims.filter((c) => get277Ref(c.claimId) != null).length

  return (
    <div className="flex flex-col gap-6">
      <button type="button" className="text-blue-600 hover:underline cursor-pointer text-left" onClick={() => navigate('/frontdesk/batches')}>
        ← Back to Batches
      </button>

      <Card title="Batch Information">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 text-sm">
          <div>
            <p className="text-gray-500">Batch ID</p>
            <p className="font-semibold">#{batch.batchId}</p>
          </div>
          <div>
            <p className="text-gray-500">Batch Date</p>
            <p className="font-semibold">{batch.batchDate || '—'}</p>
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
            <Badge status={batch.status} />
          </div>
        </div>
      </Card>

      {batch.status === 'Failed' && (
        <div className="bg-red-50 border border-red-300 rounded-lg px-4 py-3 text-red-700 text-sm">
          ⚠ 999 ACK rejected by clearinghouse. All claims in this batch have been reset to Draft status for rework.
        </div>
      )}

      {batch.status === 'Open' && (
        <Card title="Claims in Batch">
          <div className="flex justify-end mb-4">
            <Button variant="secondary" size="sm" onClick={() => setShowAddClaims(true)}>
              Add Claims
            </Button>
          </div>

          <Table
            columns={[
              { key: 'claimIdText', label: 'Claim ID' },
              { key: 'patientName', label: 'Patient Name' },
              { key: 'payerName', label: 'Payer' },
              { key: 'planName', label: 'Plan' },
              { key: 'totalChargeText', label: 'Total Charge' },
              { key: 'status', label: 'Status' },
              { key: 'ready', label: '837P Ready' },
            ]}
            data={claims.map((row) => ({
              ...row,
              claimIdText: `#${row.claimId}`,
              totalChargeText: `$${row.totalCharge.toFixed(2)}`,
              status: <Badge status={row.claimStatus} />,
              ready: row.payloadUri ? <span className="text-green-600 font-medium">✓ Ready</span> : <span className="text-red-600 font-medium">✗ Missing</span>,
            }))}
            showActions={true}
            actions={[
              {
                label: 'Remove',
                variant: 'danger',
                onClick: (row) => handleRemoveClaim((row as BatchClaimLine).claimId),
              },
            ]}
          />

          {claims.length === 0 && (
            <p className="text-gray-500 italic mt-4">No claims added yet. Click 'Add Claims' to add eligible claims.</p>
          )}

          <div className="flex justify-end mt-4">
            <Button
              variant="primary"
              disabled={claims.length === 0}
              onClick={handleGenerateBatch}
              className={claims.length === 0 ? 'opacity-50 cursor-not-allowed' : ''}
            >
              Generate Batch
            </Button>
          </div>
          {claims.length === 0 && <p className="text-xs text-gray-500 text-right mt-2">Add at least one claim to generate the batch.</p>}
        </Card>
      )}

      {batch.status === 'Generated' && (
        <Card title="Submit Batch">
          <div className="bg-gray-50 rounded-lg px-4 py-3 flex flex-wrap gap-6 mb-4 text-sm">
            <span>Total Claims: {claims.length}</span>
            <span>Total Charge: ${totalCharge.toFixed(2)}</span>
            <span className="flex items-center gap-2">
              Status: <Badge status="Generated" />
            </span>
          </div>
          <p className="text-sm text-gray-500 mb-4 italic">
            The actual EDI file transmission happens outside TeleBill. Record the submission details here once the file has been sent to the clearinghouse.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Clearinghouse ID *</label>
              <input
                value={submitForm.clearinghouseId}
                onChange={(e) => setSubmitForm((f) => ({ ...f, clearinghouseId: e.target.value }))}
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
          <Button variant="primary" onClick={handleMarkSubmitted} disabled={!submitForm.clearinghouseId || !submitForm.submitDate}>
            Mark as Submitted
          </Button>
        </Card>
      )}

      {batch.status === 'Submitted' && (
        <Card title="999 Functional Acknowledgement">
          <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 text-sm mb-4">
            The 999 ACK is the batch-level acknowledgement from the clearinghouse confirming whether your EDI submission was structurally valid.
          </div>
          {ref999 && (
            <div className="bg-gray-50 rounded-lg px-4 py-3 mb-4 text-sm grid grid-cols-1 md:grid-cols-3 gap-3">
              <div>
                <span className="text-gray-500">Clearinghouse ID:</span> <strong>{ref999.clearinghouseId}</strong>
              </div>
              <div>
                <span className="text-gray-500">Correlation ID:</span> <strong>{ref999.correlationId}</strong>
              </div>
              <div>
                <span className="text-gray-500">Submit Date:</span> <strong>{ref999.submitDate}</strong>
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
                    <input type="radio" name="ack999Status" value={opt} checked={ack999Form.ackStatus === opt} onChange={() => setAck999Form((f) => ({ ...f, ackStatus: opt }))} className="sr-only" />
                    {opt === 'Accepted' ? '✓' : '✗'} {opt}
                  </label>
                ))}
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Correlation ID</label>
              <input value={ack999Form.correlationId} onChange={(e) => setAck999Form((f) => ({ ...f, correlationId: e.target.value }))} className={inputClassName} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">ACK Date *</label>
              <input type="date" value={ack999Form.ackDate} onChange={(e) => setAck999Form((f) => ({ ...f, ackDate: e.target.value }))} className={inputClassName} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Clearinghouse ID</label>
              <input value={ack999Form.clearinghouseId} onChange={(e) => setAck999Form((f) => ({ ...f, clearinghouseId: e.target.value }))} className={inputClassName} />
            </div>
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
              <textarea rows={2} value={ack999Form.notes} onChange={(e) => setAck999Form((f) => ({ ...f, notes: e.target.value }))} className={inputClassName} />
            </div>
          </div>
          <div className="mt-4">
            <Button variant="primary" onClick={handleRecord999} disabled={!ack999Form.ackDate}>
              Record 999 ACK
            </Button>
          </div>
        </Card>
      )}

      {batch.status === 'Acked' && (
        <Card title="277CA Claim-Level Acknowledgements">
          <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 text-sm mb-4">
            Record the individual claim-level 277CA responses. Each claim must be marked Accepted or Rejected separately.
          </div>
          <div className="mb-4">
            <p className="text-sm text-gray-600 mb-2">
              {recorded277} of {total277} claims acknowledged
            </p>
            <div className="w-full bg-gray-100 rounded-full h-2">
              <div style={{ width: `${total277 === 0 ? 0 : (recorded277 / total277) * 100}%` }} className="bg-blue-500 h-2 rounded-full transition-all" />
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
              const ref277 = get277Ref(claim.claimId)
              const isActive = active277ClaimId === claim.claimId
              return (
                <div key={claim.claimId} className="border-t">
                  <div className="grid grid-cols-6 gap-3 px-4 py-3 items-center text-sm">
                    <span className="font-mono text-gray-600">#{claim.claimId}</span>
                    <span>{claim.patientName}</span>
                    <span className="text-gray-500">{claim.payerName}</span>
                    <span>${claim.totalCharge.toFixed(2)}</span>
                    <span>{ref277 ? <Badge status={ref277.ackStatus} /> : <span className="text-amber-600 text-xs font-medium">Pending</span>}</span>
                    <span>
                      {!ref277 && !isActive && (
                        <button
                          type="button"
                          onClick={() => {
                            setActive277ClaimId(claim.claimId)
                            setAck277Form({ ackStatus: 'Accepted', ackDate: '', correlationId: ref999?.correlationId ?? '', notes: '' })
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
                      <p className="text-xs font-semibold text-blue-700 mb-3 uppercase">Record 277CA for {claim.patientName}</p>
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
                                <input type="radio" name="ack277Status" value={opt} checked={ack277Form.ackStatus === opt} onChange={() => setAck277Form((f) => ({ ...f, ackStatus: opt }))} className="sr-only" />
                                {opt === 'Accepted' ? '✓' : '✗'} {opt}
                              </label>
                            ))}
                          </div>
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">ACK Date *</label>
                          <input type="date" value={ack277Form.ackDate} onChange={(e) => setAck277Form((f) => ({ ...f, ackDate: e.target.value }))} className="w-full border border-gray-300 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">Correlation ID</label>
                          <input value={ack277Form.correlationId} onChange={(e) => setAck277Form((f) => ({ ...f, correlationId: e.target.value }))} className="w-full border border-gray-300 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">Notes</label>
                          <input value={ack277Form.notes} onChange={(e) => setAck277Form((f) => ({ ...f, notes: e.target.value }))} placeholder="Optional" className="w-full border border-gray-300 rounded-md px-2 py-1.5 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <Button variant="primary" size="sm" onClick={() => handleRecord277(claim.claimId)} disabled={!ack277Form.ackDate}>
                          Record 277CA
                        </Button>
                        <Button variant="secondary" size="sm" onClick={() => setActive277ClaimId(null)}>
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

      <Dialog
        isOpen={showAddClaims}
        onClose={() => {
          setShowAddClaims(false)
          setSelectedClaimIds([])
        }}
        title="Add Claims to Batch"
        maxWidth="xl"
      >
        <p className="text-sm text-gray-500 mb-3">Showing claims with status 'Ready' that are not already in this batch.</p>
        <div className="max-h-96 overflow-y-auto">
          {eligible.map((claim) => (
            <div
              key={claim.claimId}
              onClick={() =>
                setSelectedClaimIds((prev) =>
                  prev.includes(claim.claimId) ? prev.filter((id) => id !== claim.claimId) : [...prev, claim.claimId],
                )
              }
              className={`flex items-center gap-3 px-3 py-2 rounded-lg cursor-pointer border mb-2 ${
                selectedClaimIds.includes(claim.claimId) ? 'bg-blue-50 border-blue-400' : 'border-gray-200 hover:bg-gray-50'
              }`}
            >
              <input type="checkbox" readOnly checked={selectedClaimIds.includes(claim.claimId)} className="h-4 w-4" />
              <span className="flex-1 text-sm font-medium">{claim.patientName}</span>
              <span className="text-sm text-gray-500">{claim.payerName}</span>
              <span className="text-sm text-gray-500">{claim.planName}</span>
              <span className="text-sm font-medium">${claim.totalCharge.toFixed(2)}</span>
            </div>
          ))}
          {eligible.length === 0 && <p className="text-sm text-gray-500">No eligible claims available. All ready claims have been added.</p>}
        </div>
        <div className="mt-4 flex items-center justify-between">
          <span className="text-sm text-gray-500">{selectedClaimIds.length} selected</span>
          <div className="flex gap-2">
            <Button variant="secondary" onClick={() => setShowAddClaims(false)}>
              Cancel
            </Button>
            <Button variant="primary" disabled={selectedClaimIds.length === 0} onClick={handleAddSelectedClaims}>
              Add Selected
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  )
}
