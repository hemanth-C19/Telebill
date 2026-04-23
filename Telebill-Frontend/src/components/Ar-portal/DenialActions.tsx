import { useState } from 'react'
import apiClient from '../../api/client'
import { Button } from '../shared/ui/Button'
import { Card } from '../shared/ui/Card'
import { Dialog } from '../shared/ui/Dialog'
import type { DenialDetailData } from '../../pages/AR-portal/DenialDetail'

// ── Constants ─────────────────────────────────────────────────────────────────

const STATUS_COLORS: Record<string, string> = {
  Open: 'bg-blue-100 text-blue-800',
  Appealed: 'bg-yellow-100 text-yellow-800',
  Resolved: 'bg-green-100 text-green-800',
  WrittenOff: 'bg-red-100 text-red-800',
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

const FILE_TYPES = ['PDF', 'Doc', 'Image', 'Other']
const RESETTABLE_CLAIM_STATUSES = ['Denied', 'Rejected', 'ScrubError']

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

function fmtDate(s: string | null | undefined): string {
  if (!s) return '—'
  return new Date(s).toLocaleDateString()
}

const inputCls =
  'border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 w-full'

// ── Props ─────────────────────────────────────────────────────────────────────

type Props = {
  detail: DenialDetailData
  userId: number
  onSuccess: (msg: string) => void
  onRefresh: () => void
}

// ── Component ─────────────────────────────────────────────────────────────────

export default function DenialActions({ detail, userId, onSuccess, onRefresh }: Props) {
  // Status update dialog
  const [statusOpen, setStatusOpen] = useState(false)
  const [selectedStatus, setSelectedStatus] = useState('')
  const [statusNotes, setStatusNotes] = useState('')

  // Upload appeal doc dialog
  const [attachOpen, setAttachOpen] = useState(false)
  const [attachForm, setAttachForm] = useState({ fileType: 'PDF', fileUri: '', notes: '' })

  // Reset for resubmission dialog
  const [resetOpen, setResetOpen] = useState(false)
  const [resetNotes, setResetNotes] = useState('')

  const [submitting, setSubmitting] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  // ── Derived state ─────────────────────────────────────────────────────────

  const transitions = getAllowedTransitions(detail.denialStatus)
  const isTerminal =
    detail.denialStatus === 'Resolved' || detail.denialStatus === 'WrittenOff'
  const canAttach =
    detail.denialStatus === 'Open' || detail.denialStatus === 'Appealed'
  const canReset =
    !isTerminal &&
    detail.claim?.claimStatus != null &&
    RESETTABLE_CLAIM_STATUSES.includes(detail.claim.claimStatus)

  const { claim } = detail

  // ── Handlers ──────────────────────────────────────────────────────────────

  function openStatusDialog() {
    setSelectedStatus(transitions[0] ?? '')
    setStatusNotes('')
    setActionError(null)
    setStatusOpen(true)
  }

  async function handleUpdateStatus() {
    if (!selectedStatus) return
    setSubmitting(true)
    setActionError(null)
    try {
      await apiClient.patch(`api/v1/ar/denials/${detail.denialId}/status`, {
        newStatus: selectedStatus,
        notes: statusNotes || null,
      })
      setStatusOpen(false)
      onSuccess(`Denial status updated to ${selectedStatus}.`)
      onRefresh()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  function openAttachDialog() {
    setAttachForm({ fileType: 'PDF', fileUri: '', notes: '' })
    setActionError(null)
    setAttachOpen(true)
  }

  async function handleUploadAttachment() {
    if (!attachForm.fileUri.trim()) return
    setSubmitting(true)
    setActionError(null)
    try {
      await apiClient.post(`api/v1/ar/denials/${detail.denialId}/attachments`, {
        denialId: detail.denialId,
        fileType: attachForm.fileType,
        fileUri: attachForm.fileUri.trim(),
        notes: attachForm.notes || null,
        uploadedBy: userId,
      })
      setAttachOpen(false)
      onSuccess(
        detail.denialStatus === 'Open'
          ? 'Document uploaded. Denial status advanced to Appealed.'
          : 'Document uploaded successfully.',
      )
      onRefresh()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  function openResetDialog() {
    setResetNotes('')
    setActionError(null)
    setResetOpen(true)
  }

  async function handleReset() {
    setSubmitting(true)
    setActionError(null)
    try {
      const res = await apiClient.post<{
        claimId: number
        claimStatus: string
        denialId: number
        denialStatus: string
      }>('api/v1/ar/denials/reset-for-resubmission', {
        denialId: detail.denialId,
        notes: resetNotes || null,
      })
      setResetOpen(false)
      onSuccess(`Claim #${res.data.claimId} reset to Draft. Denial marked as Resolved.`)
      onRefresh()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <>
      {/* Actions card */}
      <Card title="Actions">
        <div className="flex flex-col gap-3">
          <div className="flex flex-col gap-1">
            <Button
              variant="primary"
              size="sm"
              disabled={transitions.length === 0}
              onClick={openStatusDialog}
            >
              Update Status
            </Button>
            {isTerminal && (
              <p className="text-xs text-gray-400 text-center">
                Denial is {detail.denialStatus} — no further changes.
              </p>
            )}
          </div>

          <div className="flex flex-col gap-1">
            <Button
              variant="secondary"
              size="sm"
              disabled={!canAttach}
              onClick={openAttachDialog}
            >
              Upload Appeal Document
            </Button>
            {!canAttach && (
              <p className="text-xs text-gray-400 text-center">
                Only available for Open or Appealed denials.
              </p>
            )}
          </div>

          <hr className="border-gray-200" />

          <div className="flex flex-col gap-1">
            <Button
              variant={canReset ? 'danger' : 'secondary'}
              size="sm"
              disabled={!canReset}
              onClick={openResetDialog}
            >
              Reset for Resubmission
            </Button>
            {!canReset && (
              <p className="text-xs text-gray-400 text-center">
                {isTerminal
                  ? 'Denial is already terminal.'
                  : 'Claim must be Denied, Rejected, or ScrubError.'}
              </p>
            )}
          </div>
        </div>
      </Card>

      {/* Attachments card */}
      <Card title={`Attachments (${detail.attachments.length})`}>
        {detail.attachments.length === 0 ? (
          <p className="text-sm text-gray-400 text-center py-4">No documents attached.</p>
        ) : (
          <ul className="space-y-3">
            {detail.attachments.map((a) => (
              <li
                key={a.attachId}
                className="rounded-lg border border-gray-100 bg-gray-50 px-3 py-2.5"
              >
                <div className="flex items-center gap-2 mb-1">
                  <span className="rounded bg-blue-100 px-1.5 py-0.5 text-xs font-medium text-blue-700">
                    {a.fileType ?? 'File'}
                  </span>
                  <span className="text-xs text-gray-400">{fmtDate(a.uploadedDate)}</span>
                </div>
                {a.fileUri && (
                  <p className="text-xs text-blue-600 truncate" title={a.fileUri}>
                    {a.fileUri}
                  </p>
                )}
                {a.notes && <p className="text-xs text-gray-500 mt-0.5">{a.notes}</p>}
              </li>
            ))}
          </ul>
        )}
      </Card>

      {/* ── Update Status Dialog ── */}
      <Dialog
        isOpen={statusOpen}
        onClose={() => setStatusOpen(false)}
        title="Update Denial Status"
      >
        <div className="space-y-4">
          <div className="rounded-lg bg-gray-50 px-4 py-3 text-sm space-y-1.5">
            <div className="flex items-center gap-2">
              <span className="text-gray-500 w-32">Patient</span>
              <strong>{claim?.patientName ?? '—'}</strong>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-gray-500 w-32">Claim</span>
              <strong>#{claim?.claimId}</strong>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-gray-500 w-32">Amount Denied</span>
              <strong className="text-red-600">${detail.amountDenied.toFixed(2)}</strong>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-gray-500 w-32">Current Status</span>
              <span
                className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-semibold ${
                  STATUS_COLORS[detail.denialStatus ?? ''] ?? 'bg-gray-100 text-gray-700'
                }`}
              >
                {detail.denialStatus}
              </span>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Move to</label>
            <div className="flex flex-wrap gap-2">
              {transitions.map((s) => (
                <label
                  key={s}
                  className={`flex items-center gap-2 px-4 py-2 rounded-lg border cursor-pointer text-sm font-medium transition-colors ${
                    selectedStatus === s
                      ? (STATUS_BUTTON_STYLES[s] ?? 'border-blue-500 bg-blue-50 text-blue-700')
                      : 'border-gray-300 text-gray-600 hover:bg-gray-50'
                  }`}
                >
                  <input
                    type="radio"
                    name="newStatus"
                    value={s}
                    checked={selectedStatus === s}
                    onChange={() => setSelectedStatus(s)}
                    className="sr-only"
                  />
                  {STATUS_LABELS[s] ?? s}
                </label>
              ))}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Notes <span className="text-gray-400 font-normal">(optional)</span>
            </label>
            <textarea
              rows={2}
              value={statusNotes}
              onChange={(e) => setStatusNotes(e.target.value)}
              placeholder="Reason for status change..."
              className={inputCls}
            />
          </div>

          {selectedStatus === 'WrittenOff' && (
            <p className="text-xs text-red-600 bg-red-50 rounded-md px-3 py-2">
              Writing off a denial is irreversible — it cannot be changed after this action.
            </p>
          )}

          {actionError && (
            <p className="text-xs text-red-600 bg-red-50 rounded-md px-3 py-2">{actionError}</p>
          )}

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={() => setStatusOpen(false)}>
              Cancel
            </Button>
            <Button
              variant={selectedStatus === 'WrittenOff' ? 'danger' : 'primary'}
              disabled={!selectedStatus || submitting}
              onClick={handleUpdateStatus}
            >
              {submitting ? 'Updating...' : 'Confirm'}
            </Button>
          </div>
        </div>
      </Dialog>

      {/* ── Upload Appeal Document Dialog ── */}
      <Dialog
        isOpen={attachOpen}
        onClose={() => setAttachOpen(false)}
        title="Upload Appeal Document"
      >
        <div className="space-y-4">
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">File Type</label>
            <select
              value={attachForm.fileType}
              onChange={(e) => setAttachForm((f) => ({ ...f, fileType: e.target.value }))}
              className={inputCls}
            >
              {FILE_TYPES.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">Document URI</label>
            <input
              type="text"
              value={attachForm.fileUri}
              onChange={(e) => setAttachForm((f) => ({ ...f, fileUri: e.target.value }))}
              placeholder="https://storage.example.com/appeal-doc.pdf"
              className={inputCls}
            />
            <p className="text-xs text-gray-400">
              Storage URL or file path for the uploaded document.
            </p>
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">
              Notes <span className="text-gray-400 font-normal">(optional)</span>
            </label>
            <textarea
              rows={2}
              value={attachForm.notes}
              onChange={(e) => setAttachForm((f) => ({ ...f, notes: e.target.value }))}
              placeholder="Any notes about this document..."
              className={inputCls}
            />
          </div>

          {detail.denialStatus === 'Open' && (
            <p className="text-xs text-yellow-700 bg-yellow-50 rounded-md px-3 py-2">
              Uploading a document will automatically advance this denial from{' '}
              <strong>Open → Appealed</strong>.
            </p>
          )}

          {actionError && (
            <p className="text-xs text-red-600 bg-red-50 rounded-md px-3 py-2">{actionError}</p>
          )}

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={() => setAttachOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="primary"
              disabled={!attachForm.fileUri.trim() || submitting}
              onClick={handleUploadAttachment}
            >
              {submitting ? 'Uploading...' : 'Upload'}
            </Button>
          </div>
        </div>
      </Dialog>

      {/* ── Reset for Resubmission Dialog ── */}
      <Dialog
        isOpen={resetOpen}
        onClose={() => setResetOpen(false)}
        title="Reset Claim for Resubmission"
      >
        <div className="space-y-4">
          <div className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
            <p className="font-medium mb-1">This action will:</p>
            <ul className="list-disc list-inside space-y-0.5 text-xs">
              <li>
                Reset Claim #{claim?.claimId} from{' '}
                <strong>{claim?.claimStatus}</strong> → <strong>Draft</strong>
              </li>
              <li>
                Mark this denial as <strong>Resolved</strong>
              </li>
              <li>Allow the claim to be scrubbed and re-submitted to the payer</li>
            </ul>
          </div>

          <div className="rounded-lg bg-gray-50 px-4 py-3 text-sm space-y-1.5">
            <div className="flex items-center gap-2">
              <span className="text-gray-500 w-32">Patient</span>
              <strong>{claim?.patientName ?? '—'}</strong>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-gray-500 w-32">Payer</span>
              <strong>{claim?.payerName ?? '—'}</strong>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-gray-500 w-32">Amount Denied</span>
              <strong className="text-red-600">${detail.amountDenied.toFixed(2)}</strong>
            </div>
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">
              Notes <span className="text-gray-400 font-normal">(optional)</span>
            </label>
            <textarea
              rows={2}
              value={resetNotes}
              onChange={(e) => setResetNotes(e.target.value)}
              placeholder="Reason for resubmission..."
              className={inputCls}
            />
          </div>

          {actionError && (
            <p className="text-xs text-red-600 bg-red-50 rounded-md px-3 py-2">{actionError}</p>
          )}

          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={() => setResetOpen(false)}>
              Cancel
            </Button>
            <Button variant="danger" disabled={submitting} onClick={handleReset}>
              {submitting ? 'Resetting...' : 'Confirm Reset'}
            </Button>
          </div>
        </div>
      </Dialog>
    </>
  )
}
