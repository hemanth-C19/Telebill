// Provider portal — my encounters, read-only charges/diagnoses, attestation, mark ready for coding (dummy data only)

import { useState } from 'react'
import Badge from '../../components/shared/ui/Badge'
import Button from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import Dialog from '../../components/shared/ui/Dialog'
import { Pagination } from '../../components/shared/ui/Pagination'
import Table from '../../components/shared/ui/Table'
import type { Encounter, ChargeLine, Diagnosis, Attestation } from '../../types/provider.types'

// Simulates logged-in provider — replace with useAuth().userId in production
const CURRENT_PROVIDER = { providerId: 201, name: 'Dr. Sarah Chen', specialty: 'Internal Medicine' }

// Backend ref: GET .../Encounter/GetAllEncounters — filtered to CURRENT_PROVIDER
const DUMMY_ENCOUNTERS: Encounter[] = [
  {
    encounterId: 1001,
    patientId: 1,
    patientName: 'Alice Johnson',
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-11-05T09:30',
    pos: '02',
    notes: 'Follow-up visit',
    status: 'Open',
  },
  {
    encounterId: 1003,
    patientId: 4,
    patientName: 'David Patel',
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-11-10T11:15',
    pos: '02',
    notes: 'Chronic care mgmt',
    status: 'Finalized',
  },
  {
    encounterId: 1007,
    patientId: 2,
    patientName: 'Bob Martinez',
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-11-18T15:30',
    pos: '02',
    notes: 'Post-op follow-up',
    status: 'Finalized',
  },
  {
    encounterId: 1008,
    patientId: 7,
    patientName: 'Grace Kim',
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-12-02T10:00',
    pos: '10',
    notes: 'Annual wellness',
    status: 'Open',
  },
  {
    encounterId: 1009,
    patientId: 5,
    patientName: 'Emily Rodriguez',
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-12-05T14:30',
    pos: '02',
    notes: 'Hypertension mgmt',
    status: 'ReadyForCoding',
  },
]

const DUMMY_CHARGE_LINES: Record<number, ChargeLine[]> = {
  1001: [
    {
      chargeId: 5001,
      encounterId: 1001,
      lineNo: 1,
      cptCode: '99213',
      modifiers: '',
      units: 1,
      chargeAmount: 150.0,
      dxPointers: '1',
      status: 'Draft',
    },
    {
      chargeId: 5002,
      encounterId: 1001,
      lineNo: 2,
      cptCode: '93000',
      modifiers: '',
      units: 1,
      chargeAmount: 85.0,
      dxPointers: '1,2',
      status: 'Finalized',
    },
  ],
  1003: [
    {
      chargeId: 5006,
      encounterId: 1003,
      lineNo: 1,
      cptCode: '99215',
      modifiers: 'GQ',
      units: 1,
      chargeAmount: 250.0,
      dxPointers: '1',
      status: 'Finalized',
    },
  ],
  1007: [
    {
      chargeId: 5013,
      encounterId: 1007,
      lineNo: 1,
      cptCode: '99213',
      modifiers: 'GQ,GT',
      units: 1,
      chargeAmount: 150.0,
      dxPointers: '1',
      status: 'Finalized',
    },
    {
      chargeId: 5014,
      encounterId: 1007,
      lineNo: 2,
      cptCode: '93000',
      modifiers: '',
      units: 1,
      chargeAmount: 85.0,
      dxPointers: '1,2',
      status: 'Finalized',
    },
    {
      chargeId: 5015,
      encounterId: 1007,
      lineNo: 3,
      cptCode: '90837',
      modifiers: 'GT',
      units: 2,
      chargeAmount: 350.0,
      dxPointers: '2',
      status: 'Finalized',
    },
  ],
  1008: [],
  1009: [
    {
      chargeId: 5016,
      encounterId: 1009,
      lineNo: 1,
      cptCode: '99214',
      modifiers: '',
      units: 1,
      chargeAmount: 200.0,
      dxPointers: '1',
      status: 'Finalized',
    },
    {
      chargeId: 5017,
      encounterId: 1009,
      lineNo: 2,
      cptCode: '93000',
      modifiers: 'GT',
      units: 1,
      chargeAmount: 85.0,
      dxPointers: '1',
      status: 'Finalized',
    },
  ],
}

const DUMMY_DIAGNOSES: Record<number, Diagnosis[]> = {
  1001: [
    {
      diagnosisId: 301,
      encounterId: 1001,
      icdCode: 'I10',
      description: 'Essential hypertension',
      sequence: 1,
    },
    {
      diagnosisId: 302,
      encounterId: 1001,
      icdCode: 'E11.9',
      description: 'Type 2 diabetes w/o complication',
      sequence: 2,
    },
  ],
  1003: [
    {
      diagnosisId: 303,
      encounterId: 1003,
      icdCode: 'I10',
      description: 'Essential hypertension',
      sequence: 1,
    },
  ],
  1007: [
    {
      diagnosisId: 304,
      encounterId: 1007,
      icdCode: 'M54.5',
      description: 'Low back pain',
      sequence: 1,
    },
    {
      diagnosisId: 305,
      encounterId: 1007,
      icdCode: 'F32.9',
      description: 'Major depressive episode, unspecified',
      sequence: 2,
    },
  ],
  1008: [],
  1009: [
    {
      diagnosisId: 306,
      encounterId: 1009,
      icdCode: 'I10',
      description: 'Essential hypertension',
      sequence: 1,
    },
  ],
}

const INITIAL_ATTESTATIONS: Record<number, Attestation> = {
  1001: {
    attestId: 0,
    encounterId: 1001,
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    attestedDate: '',
    signatureNote: '',
  },
  1003: {
    attestId: 9003,
    encounterId: 1003,
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    attestedDate: '2024-11-10',
    signatureNote: 'I attest that the documentation accurately reflects the services rendered.',
  },
  1007: {
    attestId: 9007,
    encounterId: 1007,
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    attestedDate: '2024-11-18',
    signatureNote: 'I attest that the documentation accurately reflects the services rendered.',
  },
  1008: {
    attestId: 0,
    encounterId: 1008,
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    attestedDate: '',
    signatureNote: '',
  },
  1009: {
    attestId: 9009,
    encounterId: 1009,
    providerId: 201,
    providerName: 'Dr. Sarah Chen',
    attestedDate: '2024-12-05',
    signatureNote: 'I attest that the documentation accurately reflects the services rendered.',
  },
}

const textareaClassName =
  'mt-1 w-full resize-none rounded-lg border border-gray-300 p-2 text-sm text-gray-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

function formatEncounterDate(iso: string): string {
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return iso
  return d.toLocaleDateString('en-US', {
    month: 'short',
    day: '2-digit',
    year: 'numeric',
  })
}

function posFullLabel(code: string): string {
  if (code === '02') return `${code} — Telehealth – Patient Home`
  if (code === '10') return `${code} — Telehealth – Non-Patient Home`
  return code
}

function nextAttestId(map: Record<number, Attestation>): number {
  let max = 0
  for (const a of Object.values(map)) {
    if (a.attestId > max) max = a.attestId
  }
  return max + 1
}

function ChecklistIcon({ ok }: { ok: boolean }) {
  if (ok) {
    return (
      <svg
        className="h-6 w-6 shrink-0 text-green-600"
        viewBox="0 0 24 24"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden
      >
        <circle cx="12" cy="12" r="10" className="fill-green-100" stroke="currentColor" strokeWidth="2" />
        <path
          d="M8 12l2.5 2.5L16 9"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </svg>
    )
  }
  return (
    <svg
      className="h-6 w-6 shrink-0 text-red-500"
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden
    >
      <circle cx="12" cy="12" r="10" className="fill-red-50" stroke="currentColor" strokeWidth="2" />
      <path d="M9 9l6 6M15 9l-6 6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  )
}

export default function EncounterDetails() {
  const chargeLines = DUMMY_CHARGE_LINES
  const diagnoses = DUMMY_DIAGNOSES

  const [encounters, setEncounters] = useState<Encounter[]>(() => [...DUMMY_ENCOUNTERS])
  const [attestations, setAttestations] = useState<Record<number, Attestation>>(() => ({
    ...INITIAL_ATTESTATIONS,
  }))

  const [selectedEncounter, setSelectedEncounter] = useState<Encounter | null>(null)
  const [currentPage, setCurrentPage] = useState(1)

  const [showAttestDialog, setShowAttestDialog] = useState(false)
  const [signatureNote, setSignatureNote] = useState(
    'I attest that the documentation accurately reflects the services rendered.',
  )
  const [showReadyDialog, setShowReadyDialog] = useState(false)

  const isAttested = (id: number) => (attestations[id]?.attestId ?? 0) !== 0

  const canMarkReady = (enc: Encounter): boolean => {
    const lines = chargeLines[enc.encounterId] ?? []
    return (
      enc.status === 'Open' &&
      isAttested(enc.encounterId) &&
      lines.length > 0 &&
      lines.every((cl) => cl.status === 'Finalized')
    )
  }

  const listColumns = [
    { key: 'encounterId', label: 'Encounter ID' },
    { key: 'patientName', label: 'Patient Name' },
    { key: 'encounterDateDisplay', label: 'Encounter Date' },
    { key: 'pos', label: 'POS' },
    { key: 'status', label: 'Status' },
    { key: 'attestCol', label: 'Attestation' },
    { key: 'readyCol', label: 'Ready?' },
  ]

  function confirmAttestation() {
    if (selectedEncounter == null) return
    const eid = selectedEncounter.encounterId
    const newAttest: Attestation = {
      attestId: nextAttestId(attestations),
      encounterId: eid,
      providerId: CURRENT_PROVIDER.providerId,
      providerName: CURRENT_PROVIDER.name,
      attestedDate: new Date().toISOString().slice(0, 10),
      signatureNote: signatureNote.trim(),
    }
    setAttestations((prev) => ({ ...prev, [eid]: newAttest }))
    setShowAttestDialog(false)
    setSignatureNote('I attest that the documentation accurately reflects the services rendered.')
  }

  function confirmMarkReady() {
    if (selectedEncounter == null) return
    const eid = selectedEncounter.encounterId
    setEncounters((prev) =>
      prev.map((e) => (e.encounterId === eid ? { ...e, status: 'ReadyForCoding' as const } : e)),
    )
    setSelectedEncounter((prev) =>
      prev != null && prev.encounterId === eid
        ? { ...prev, status: 'ReadyForCoding' as const }
        : prev,
    )
    setShowReadyDialog(false)
  }

  if (selectedEncounter != null) {
    const eid = selectedEncounter.encounterId
    const lines = chargeLines[eid] ?? []
    const dxList = diagnoses[eid] ?? []
    const att = attestations[eid]
    const attested = isAttested(eid)
    const ready = canMarkReady(selectedEncounter)
    const allLinesFinal =
      lines.length > 0 && lines.every((cl) => cl.status === 'Finalized')

    const chargeColumns = [
      { key: 'lineNo', label: 'Line No' },
      { key: 'cptCode', label: 'CPT/HCPCS' },
      { key: 'modifiers', label: 'Modifiers' },
      { key: 'units', label: 'Units' },
      { key: 'chargeAmountDisplay', label: 'Charge Amount' },
      { key: 'dxPointers', label: 'Dx Pointers' },
      { key: 'status', label: 'Status' },
    ]

    const chargeTableData = lines.map((cl) => ({
      ...cl,
      chargeAmountDisplay: `$${cl.chargeAmount.toFixed(2)}`,
      status: <Badge status={cl.status} />,
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
              <dd className="text-sm text-gray-900">{selectedEncounter.patientName}</dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Encounter Date</dt>
              <dd className="text-sm text-gray-900">
                {formatEncounterDate(selectedEncounter.encounterDate)}
              </dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-xs font-semibold uppercase text-gray-500">Place of Service</dt>
              <dd className="text-sm text-gray-900">{posFullLabel(selectedEncounter.pos)}</dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-xs font-semibold uppercase text-gray-500">Notes</dt>
              <dd className="text-sm text-gray-900">
                {selectedEncounter.notes.trim() === '' ? '—' : selectedEncounter.notes}
              </dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Status</dt>
              <dd className="mt-1">
                <Badge status={selectedEncounter.status} />
              </dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase text-gray-500">Diagnoses Recorded</dt>
              <dd className="text-sm text-gray-900">{dxList.length} ICD-10 codes</dd>
            </div>
          </dl>
        </Card>

        <Card title="Charge Lines">
          {lines.length === 0 ? (
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
              {(selectedEncounter.status === 'Open' ||
                selectedEncounter.status === 'ReadyForCoding') && (
                <Button type="button" variant="primary" onClick={() => setShowAttestDialog(true)}>
                  Attest This Encounter
                </Button>
              )}
            </>
          ) : (
            <>
              <div className="mb-4 rounded-lg border border-green-200 bg-green-50 p-3 text-sm font-medium text-green-800">
                ✓ Attested
              </div>
              <dl className="space-y-2 text-sm">
                <div>
                  <dt className="text-xs font-semibold uppercase text-gray-500">Attested By</dt>
                  <dd className="text-gray-900">{att?.providerName}</dd>
                </div>
                <div>
                  <dt className="text-xs font-semibold uppercase text-gray-500">Date</dt>
                  <dd className="text-gray-900">{att?.attestedDate}</dd>
                </div>
                <div>
                  <dt className="text-xs font-semibold uppercase text-gray-500">Signature Note</dt>
                  <dd className="italic text-gray-600">{att?.signatureNote}</dd>
                </div>
              </dl>
            </>
          )}
        </Card>

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
              <ChecklistIcon ok={isAttested(eid)} />
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
              disabled={!ready}
              onClick={() => setShowReadyDialog(true)}
            >
              Mark Ready for Coding
            </Button>
          </div>
        </div>

        <Dialog
          isOpen={showAttestDialog}
          onClose={() => setShowAttestDialog(false)}
          title="Attest Encounter"
          maxWidth="md"
        >
          <p className="mb-3 text-sm text-gray-600">
            By attesting, you confirm that the documentation accurately reflects the services rendered.
            This action cannot be undone.
          </p>
          <textarea
            rows={4}
            className={textareaClassName}
            value={signatureNote}
            onChange={(ev) => setSignatureNote(ev.target.value)}
          />
          <div className="mt-4 flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setShowAttestDialog(false)}>
              Cancel
            </Button>
            <Button type="button" variant="primary" onClick={confirmAttestation}>
              Confirm Attestation
            </Button>
          </div>
        </Dialog>

        <Dialog
          isOpen={showReadyDialog}
          onClose={() => setShowReadyDialog(false)}
          title="Confirm Handoff to Coding"
          maxWidth="sm"
        >
          <p className="text-sm text-gray-700">
            Are you sure you want to mark Encounter #{selectedEncounter.encounterId} as Ready for
            Coding? The Coding team will be able to pick this up. You will no longer be able to attest
            or modify this encounter.
          </p>
          <div className="mt-4 flex justify-end gap-2">
            <Button type="button" variant="secondary" onClick={() => setShowReadyDialog(false)}>
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

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Encounters to be Attested</h1>

      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          columns={listColumns}
          data={DUMMY_ENCOUNTERS}
          showActions
          actions={[
            {
              label: 'View Detail',
              onClick: (row) => {
                const enc = encounters.find((x) => x.encounterId === row.encounterId)
                if (enc != null) setSelectedEncounter(enc)
              },
            },
          ]}
        />
        <Pagination
          currentPage={currentPage}
          totalPages={5}
          onPageChange={setCurrentPage}
        />
      </div>
    </div>
  )
}
