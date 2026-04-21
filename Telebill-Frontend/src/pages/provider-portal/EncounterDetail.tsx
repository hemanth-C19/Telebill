import { useEffect, useMemo, useState } from 'react'
import Badge from '../../components/shared/ui/Badge'
import Button from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import Dialog from '../../components/shared/ui/Dialog'
import { Pagination } from '../../components/shared/ui/Pagination'
import Table from '../../components/shared/ui/Table'
import apiClient from '../../api/client'
import { useAuth } from '../../hooks/useAuth'

// ── Types ──────────────────────────────────────────────────────────────

type EncounterSummary = {
  encounterId: number
  patientId: number | null
  patientName: string | null
  encounterDateTime: string
  pos: string | null
  documentationUri: string | null
  status: string
  hasAttestation: boolean
  allChargesFinalized: boolean
  readyToHandOff: boolean
  chargeLineCount: number
  totalCharge: number
}

type ChargeLineIncoming = {
  chargeId: number
  encounterId: number | null
  cpT_HCPCS: string | null
  modifiers: string | null
  units: number | null
  chargeAmount: number | null
  revenueCode: string | null
  notes: string | null
  status: string | null
}

type ChargeLine = {
  chargeId: number
  cptCode: string
  modifiers: string
  units: number
  chargeAmount: number
  revenueCode: string
  status: string
}

type AttestationData = {
  attestId: number
  attestText: string | null
  attestDate: string | null
  status: string | null
}

// ── Constants ──────────────────────────────────────────────────────────

const PAGE_SIZE = 10
const DEFAULT_ATTEST_TEXT =
  'I attest that the documentation accurately reflects the services rendered.'

const listColumns = [
  { key: 'encounterId', label: 'Encounter ID' },
  { key: 'patientName', label: 'Patient Name' },
  { key: 'encounterDateDisplay', label: 'Encounter Date' },
  { key: 'pos', label: 'POS' },
  { key: 'status', label: 'Status' },
  { key: 'attestCol', label: 'Attestation' },
  { key: 'readyCol', label: 'Ready?' },
]

const chargeColumns = [
  { key: 'cptCode', label: 'CPT/HCPCS' },
  { key: 'modifiers', label: 'Modifiers' },
  { key: 'units', label: 'Units' },
  { key: 'chargeAmountDisplay', label: 'Amount' },
  { key: 'revenueCode', label: 'Rev Code' },
  { key: 'statusBadge', label: 'Status' },
]

const textareaClassName =
  'mt-1 w-full resize-none rounded-lg border border-gray-300 p-2 text-sm text-gray-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

// ── Helpers ────────────────────────────────────────────────────────────

function toChargeLine(raw: ChargeLineIncoming): ChargeLine {
  return {
    chargeId: raw.chargeId,
    cptCode: raw.cpT_HCPCS ?? '',
    modifiers: raw.modifiers ?? '',
    units: raw.units ?? 1,
    chargeAmount: Number(raw.chargeAmount ?? 0),
    revenueCode: raw.revenueCode ?? '',
    status: raw.status ?? 'Draft',
  }
}

function formatDate(iso: string): string {
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return iso
  return d.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })
}

function posFullLabel(code: string): string {
  if (code === '02') return `${code} — Telehealth – Patient Home`
  if (code === '10') return `${code} — Telehealth – Non-Patient Home`
  return code
}

// ── ChecklistIcon ──────────────────────────────────────────────────────

function ChecklistIcon({ ok }: { ok: boolean }) {
  if (ok) {
    return (
      <svg
        className="h-6 w-6 shrink-0 text-green-600"
        viewBox="0 0 24 24"
        fill="none"
        aria-hidden
      >
        <circle cx="12" cy="12" r="10" className="fill-green-100" stroke="currentColor" strokeWidth="2" />
        <path d="M8 12l2.5 2.5L16 9" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    )
  }
  return (
    <svg
      className="h-6 w-6 shrink-0 text-red-500"
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden
    >
      <circle cx="12" cy="12" r="10" className="fill-red-50" stroke="currentColor" strokeWidth="2" />
      <path d="M9 9l6 6M15 9l-6 6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  )
}

// ── Component ──────────────────────────────────────────────────────────

export default function EncounterDetails() {
  const { user } = useAuth()

  const [encounters, setEncounters] = useState<EncounterSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [currentPage, setCurrentPage] = useState(1)

  const [selectedEncounter, setSelectedEncounter] = useState<EncounterSummary | null>(null)
  const [detailChargeLines, setDetailChargeLines] = useState<ChargeLine[]>([])
  const [detailAttestation, setDetailAttestation] = useState<AttestationData | null>(null)
  const [detailLoading, setDetailLoading] = useState(false)

  const [showAttestDialog, setShowAttestDialog] = useState(false)
  const [signatureNote, setSignatureNote] = useState(DEFAULT_ATTEST_TEXT)
  const [showReadyDialog, setShowReadyDialog] = useState(false)

  // ── Data fetching ────────────────────────────────────────────────────

  async function fetchEncounters(): Promise<EncounterSummary[]> {
    const res = await apiClient.get('api/v1/coding/provider/encounters', {
      params: { providerId: user?.userId, status: 'Open' },
    })
    const data = res.data as EncounterSummary[]
    setEncounters(data)
    return data
  }

  useEffect(() => {
    fetchEncounters().finally(() => setLoading(false))
  }, [])

  async function openDetail(enc: EncounterSummary) {
    setSelectedEncounter(enc)
    setDetailChargeLines([])
    setDetailAttestation(null)
    setDetailLoading(true)

    const [chargesResult, attestResult] = await Promise.allSettled([
      apiClient.get(`api/v1/Encounter/ChargeLine/ByEncounter/${enc.encounterId}`),
      apiClient.get('api/v1/Encounter/Attestation/get-by-encounterId', {
        params: { encounterId: enc.encounterId },
      }),
    ])

    setDetailChargeLines(
      chargesResult.status === 'fulfilled'
        ? (chargesResult.value.data as ChargeLineIncoming[]).map(toChargeLine)
        : [],
    )
    setDetailAttestation(
      attestResult.status === 'fulfilled' && attestResult.value.data
        ? (attestResult.value.data as AttestationData)
        : null,
    )
    setDetailLoading(false)
  }

  // ── Handlers ─────────────────────────────────────────────────────────

  async function confirmAttestation() {
    if (selectedEncounter == null) return
    const encId = selectedEncounter.encounterId

    // 1. Create draft attestation
    const addRes = await apiClient.post(
      `api/v1/Encounter/Attestation/Add/encounter/${encId}`,
      { ProviderID: user?.userId, AttestText: signatureNote.trim() },
    )
    const attestId = (addRes.data as { attestId: number }).attestId

    // 2. Finalize with status="Attested" so MarkReadyForCoding can find it
    await apiClient.put(`api/v1/Encounter/Attestation/${attestId}/finalize`, null, {
      params: { status: 'Attested' },
    })

    // 3. Refresh encounter list (hasAttestation becomes true), sync selected
    const fresh = await fetchEncounters()
    const freshSelected = fresh.find((e) => e.encounterId === encId)
    if (freshSelected != null) setSelectedEncounter(freshSelected)

    // 4. Refresh attestation details in the detail panel
    const attestRes = await apiClient.get('api/v1/Encounter/Attestation/get-by-encounterId', {
      params: { encounterId: encId },
    })
    setDetailAttestation(attestRes.data ?? null)

    setShowAttestDialog(false)
    setSignatureNote(DEFAULT_ATTEST_TEXT)
  }

  async function confirmMarkReady() {
    if (selectedEncounter == null) return
    await apiClient.post(
      `api/v1/coding/provider/encounters/${selectedEncounter.encounterId}/ready-for-coding`,
      null,
      { params: { providerId: user?.userId } },
    )
    setShowReadyDialog(false)
    setSelectedEncounter(null)
    await fetchEncounters()
  }

  // ── Pagination ────────────────────────────────────────────────────────

  const totalPages = Math.max(1, Math.ceil(encounters.length / PAGE_SIZE))

  const paginated = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE
    return encounters.slice(start, start + PAGE_SIZE)
  }, [encounters, currentPage])

  // ── Detail view ────────────────────────────────────────────────────────

  if (selectedEncounter != null) {
    const attested = selectedEncounter.hasAttestation
    const allLinesFinal =
      detailChargeLines.length > 0 &&
      detailChargeLines.every((cl) => cl.status === 'Finalized')
    const canMarkReady = selectedEncounter.readyToHandOff

    const chargeTableData = detailChargeLines.map((cl) => ({
      ...cl,
      chargeAmountDisplay: `$${cl.chargeAmount.toFixed(2)}`,
      statusBadge: <Badge status={cl.status} />,
    }))

    return (
      <div className="space-y-6">
        <button
          type="button"
          onClick={() => setSelectedEncounter(null)}
          className="text-sm font-medium text-blue-600 hover:text-blue-800"
        >
          ← Back to My Encounters
        </button>

        <Card title="Encounter Information">
          <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Patient</dt>
              <dd className="text-sm text-gray-900">{selectedEncounter.patientName ?? '—'}</dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Encounter Date</dt>
              <dd className="text-sm text-gray-900">
                {formatDate(selectedEncounter.encounterDateTime)}
              </dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-xs font-semibold uppercase text-gray-500">Place of Service</dt>
              <dd className="text-sm text-gray-900">{posFullLabel(selectedEncounter.pos ?? '')}</dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-xs font-semibold uppercase text-gray-500">Notes</dt>
              <dd className="text-sm text-gray-900">
                {(selectedEncounter.documentationUri ?? '').trim() === ''
                  ? '—'
                  : selectedEncounter.documentationUri}
              </dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Status</dt>
              <dd className="mt-1">
                <Badge status={selectedEncounter.status} />
              </dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Charge Lines</dt>
              <dd className="text-sm text-gray-900">
                {selectedEncounter.chargeLineCount} lines · $
                {selectedEncounter.totalCharge.toFixed(2)} total
              </dd>
            </div>
          </dl>
        </Card>

        <Card title="Charge Lines">
          {detailLoading ? (
            <p className="text-sm text-gray-500">Loading charge lines…</p>
          ) : detailChargeLines.length === 0 ? (
            <p className="text-sm italic text-gray-500">
              No charge lines recorded yet. FrontDesk will add and finalize charge lines.
            </p>
          ) : (
            <Table columns={chargeColumns} data={chargeTableData} />
          )}
          <p className="mt-2 text-sm text-gray-500">
            Charge lines are managed by FrontDesk. All lines must be Finalized before you can mark
            this encounter Ready for Coding.
          </p>
        </Card>

        <Card title="Attestation" className="border-l-4 border-blue-500">
          {!attested ? (
            <>
              <div className="mb-4 rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900">
                ⚠ This encounter has not been attested yet. You must attest before it can be marked
                Ready for Coding.
              </div>
              {selectedEncounter.status === 'Open' && (
                <Button
                  type="button"
                  variant="primary"
                  onClick={() => setShowAttestDialog(true)}
                >
                  Attest This Encounter
                </Button>
              )}
            </>
          ) : (
            <>
              <div className="mb-4 rounded-lg border border-green-200 bg-green-50 p-3 text-sm font-medium text-green-800">
                ✓ Attested
              </div>
              {detailAttestation != null && (
                <dl className="space-y-2 text-sm">
                  <div>
                    <dt className="text-xs font-semibold uppercase text-gray-500">Date</dt>
                    <dd className="text-gray-900">
                      {detailAttestation.attestDate
                        ? formatDate(detailAttestation.attestDate)
                        : '—'}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-xs font-semibold uppercase text-gray-500">
                      Signature Note
                    </dt>
                    <dd className="italic text-gray-600">
                      {detailAttestation.attestText ?? '—'}
                    </dd>
                  </div>
                </dl>
              )}
            </>
          )}
        </Card>

        {/* Ready for coding checklist + action */}
        <div className="mt-4 flex flex-col gap-4 rounded-xl border-2 border-blue-200 bg-white px-5 py-5 shadow-md ring-2 ring-blue-100 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex flex-col gap-2">
            <p className="text-xs font-bold uppercase tracking-wide text-blue-800">
              Ready for coding checklist
            </p>
            <div className="flex items-start gap-2">
              <ChecklistIcon ok={allLinesFinal} />
              <span className="text-sm text-gray-800">All charge lines finalized</span>
            </div>
            <div className="flex items-start gap-2">
              <ChecklistIcon ok={attested} />
              <span className="text-sm text-gray-800">Encounter attested</span>
            </div>
            <div className="flex items-start gap-2">
              <ChecklistIcon ok={selectedEncounter.status === 'Open'} />
              <span className="text-sm text-gray-800">Status is Open</span>
            </div>
          </div>
          <div className="shrink-0">
            <Button
              type="button"
              variant="primary"
              size="lg"
              disabled={!canMarkReady}
              onClick={() => setShowReadyDialog(true)}
            >
              Mark Ready for Coding
            </Button>
          </div>
        </div>

        {/* Attest dialog */}
        <Dialog
          isOpen={showAttestDialog}
          onClose={() => setShowAttestDialog(false)}
          title="Attest Encounter"
          maxWidth="md"
        >
          <p className="mb-3 text-sm text-gray-600">
            By attesting, you confirm that the documentation accurately reflects the services
            rendered. This action cannot be undone.
          </p>
          <textarea
            rows={4}
            className={textareaClassName}
            value={signatureNote}
            onChange={(ev) => setSignatureNote(ev.target.value)}
          />
          <div className="mt-4 flex justify-end gap-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => setShowAttestDialog(false)}
            >
              Cancel
            </Button>
            <Button type="button" variant="primary" onClick={confirmAttestation}>
              Confirm Attestation
            </Button>
          </div>
        </Dialog>

        {/* Mark Ready dialog */}
        <Dialog
          isOpen={showReadyDialog}
          onClose={() => setShowReadyDialog(false)}
          title="Confirm Handoff to Coding"
          maxWidth="sm"
        >
          <p className="text-sm text-gray-700">
            Are you sure you want to mark Encounter #{selectedEncounter.encounterId} as Ready for
            Coding? The Coding team will be able to pick this up. You will no longer be able to
            attest or modify this encounter.
          </p>
          <div className="mt-4 flex justify-end gap-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => setShowReadyDialog(false)}
            >
              Cancel
            </Button>
            <Button type="button" variant="primary" onClick={confirmMarkReady}>
              Yes, Mark Ready
            </Button>
          </div>
        </Dialog>
      </div>
    )
  }

  // ── List view ──────────────────────────────────────────────────────────

  const listTableData = paginated.map((enc) => ({
    ...enc,
    encounterDateDisplay: formatDate(enc.encounterDateTime),
    status: <Badge status={enc.status} />,
    attestCol: <ChecklistIcon ok={enc.hasAttestation} />,
    readyCol: <ChecklistIcon ok={enc.readyToHandOff} />,
  }))

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Encounters to be Attested</h1>

      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          columns={listColumns}
          data={listTableData}
          loading={loading}
          showActions
          actions={[
            {
              label: 'View Detail',
              onClick: (row) => {
                const enc = encounters.find((x) => x.encounterId === row.encounterId)
                if (enc != null) void openDetail(enc)
              },
            },
          ]}
        />
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setCurrentPage}
        />
      </div>
    </div>
  )
}
