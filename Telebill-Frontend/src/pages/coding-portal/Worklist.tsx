import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import { Pagination } from '../../components/shared/ui/Pagination'
import { Table } from '../../components/shared/ui/Table'

type WorklistItem = {
  encounterId: number
  patientName: string
  mrn: string
  providerName: string
  encounterDate: string
  pos: string
  planName: string
  diagnosisCount: number
  chargeLineCount: number
  totalCharge: number
  hasPrimaryDiagnosis: boolean
  status: 'ReadyForCoding'
  lockStatus: 'Unlocked' | 'Locked'
  lockedBy: string
}

const DUMMY_WORKLIST: WorklistItem[] = [
  {
    encounterId: 1002,
    patientName: 'Bob Martinez',
    mrn: 'PT-E5F6G7H8',
    providerName: 'Dr. James Patel',
    encounterDate: '2024-11-08T14:00',
    pos: '10',
    planName: 'Aetna Select PPO',
    diagnosisCount: 0,
    chargeLineCount: 3,
    totalCharge: 595.0,
    hasPrimaryDiagnosis: false,
    status: 'ReadyForCoding',
    lockStatus: 'Unlocked',
    lockedBy: '',
  },
  {
    encounterId: 1006,
    patientName: 'Alice Johnson',
    mrn: 'PT-A1B2C3D4',
    providerName: 'Dr. Mark Liu',
    encounterDate: '2024-11-15T10:00',
    pos: '10',
    planName: 'BlueCross PPO Basic',
    diagnosisCount: 1,
    chargeLineCount: 4,
    totalCharge: 555.0,
    hasPrimaryDiagnosis: true,
    status: 'ReadyForCoding',
    lockStatus: 'Locked',
    lockedBy: 'Jane Coder',
  },
  {
    encounterId: 1009,
    patientName: 'Emily Rodriguez',
    mrn: 'PT-Q7R8S9T0',
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-12-05T14:30',
    pos: '02',
    planName: 'BlueCross PPO Basic',
    diagnosisCount: 1,
    chargeLineCount: 2,
    totalCharge: 285.0,
    hasPrimaryDiagnosis: true,
    status: 'ReadyForCoding',
    lockStatus: 'Unlocked',
    lockedBy: '',
  },
  {
    encounterId: 1010,
    patientName: 'Carol Nguyen',
    mrn: 'PT-I9J0K1L2',
    providerName: 'Dr. James Patel',
    encounterDate: '2024-12-08T09:00',
    pos: '02',
    planName: 'Aetna Choice HMO',
    diagnosisCount: 0,
    chargeLineCount: 2,
    totalCharge: 325.0,
    hasPrimaryDiagnosis: false,
    status: 'ReadyForCoding',
    lockStatus: 'Unlocked',
    lockedBy: '',
  },
  {
    encounterId: 1011,
    patientName: 'David Patel',
    mrn: 'PT-M3N4O5P6',
    providerName: 'Dr. Sarah Chen',
    encounterDate: '2024-12-10T11:00',
    pos: '02',
    planName: 'BCBS HMO Plus',
    diagnosisCount: 2,
    chargeLineCount: 1,
    totalCharge: 250.0,
    hasPrimaryDiagnosis: true,
    status: 'ReadyForCoding',
    lockStatus: 'Locked',
    lockedBy: 'Jane Coder',
  },
  {
    encounterId: 1012,
    patientName: 'Grace Kim',
    mrn: 'PT-Y5Z6A7B8',
    providerName: 'Dr. Mark Liu',
    encounterDate: '2024-12-12T15:00',
    pos: '10',
    planName: 'Cigna HMO',
    diagnosisCount: 0,
    chargeLineCount: 3,
    totalCharge: 430.0,
    hasPrimaryDiagnosis: false,
    status: 'ReadyForCoding',
    lockStatus: 'Unlocked',
    lockedBy: '',
  },
  {
    encounterId: 1013,
    patientName: 'Frank Williams',
    mrn: 'PT-U1V2W3X4',
    providerName: 'Dr. Mark Liu',
    encounterDate: '2024-12-14T08:30',
    pos: '02',
    planName: 'UHC Gold PPO',
    diagnosisCount: 1,
    chargeLineCount: 2,
    totalCharge: 315.0,
    hasPrimaryDiagnosis: true,
    status: 'ReadyForCoding',
    lockStatus: 'Unlocked',
    lockedBy: '',
  },
]

const PROVIDER_OPTIONS = [...new Set(DUMMY_WORKLIST.map((w) => w.providerName))]
const PLAN_OPTIONS = [...new Set(DUMMY_WORKLIST.map((w) => w.planName))]

const inputClassName =
  'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

function formatEncounterDate(input: string): string {
  const date = new Date(input)
  if (Number.isNaN(date.getTime())) return input
  const datePart = date.toLocaleDateString('en-US', {
    month: 'short',
    day: '2-digit',
    year: 'numeric',
  })
  const timePart = date.toLocaleTimeString('en-US', {
    hour12: false,
    hour: '2-digit',
    minute: '2-digit',
  })
  return `${datePart} ${timePart}`
}

export default function Worklist() {
  const [worklist] = useState<WorklistItem[]>(DUMMY_WORKLIST)
  const [providerFilter, setProviderFilter] = useState('All')
  const [planFilter, setPlanFilter] = useState('All')
  const [lockFilter, setLockFilter] = useState('All')
  const [search, setSearch] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const PAGE_SIZE = 10
  const navigate = useNavigate()

  useEffect(() => {
    setCurrentPage(1)
  }, [providerFilter, planFilter, lockFilter, search])

  const totalItems = worklist.length
  const lockedCount = worklist.filter((w) => w.lockStatus === 'Locked').length
  const unlockedCount = worklist.filter((w) => w.lockStatus === 'Unlocked').length

  const filtered = useMemo(
    () =>
      worklist
        .filter((w) => providerFilter === 'All' || w.providerName === providerFilter)
        .filter((w) => planFilter === 'All' || w.planName === planFilter)
        .filter((w) => lockFilter === 'All' || w.lockStatus === lockFilter)
        .filter(
          (w) =>
            !search ||
            w.patientName.toLowerCase().includes(search.toLowerCase()) ||
            w.mrn.toLowerCase().includes(search.toLowerCase()),
        ),
    [worklist, providerFilter, planFilter, lockFilter, search],
  )

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
  const safeCurrentPage = Math.min(currentPage, totalPages)
  const paginated = filtered.slice((safeCurrentPage - 1) * PAGE_SIZE, safeCurrentPage * PAGE_SIZE)

  const clearFilters = () => {
    setSearch('')
    setProviderFilter('All')
    setPlanFilter('All')
    setLockFilter('All')
    setCurrentPage(1)
  }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Coding Worklist</h1>

      <div className="flex gap-4 mb-6">
        <Card className="border-l-4 border-blue-500 flex-1">
          <p className="text-xs uppercase text-gray-500">Total Encounters</p>
          <p className="text-3xl font-bold text-gray-900 mt-1">{totalItems}</p>
        </Card>
        <Card className="border-l-4 border-green-500 flex-1">
          <p className="text-xs uppercase text-gray-500">Unlocked</p>
          <p className="text-3xl font-bold text-green-600 mt-1">{unlockedCount}</p>
        </Card>
        <Card className="border-l-4 border-amber-500 flex-1">
          <p className="text-xs uppercase text-gray-500">Locked by Coder</p>
          <p className="text-3xl font-bold text-amber-600 mt-1">{lockedCount}</p>
        </Card>
      </div>

      <div className="flex gap-3 mb-4 flex-wrap">
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search by patient name or MRN..."
          className={`${inputClassName} min-w-64`}
        />
        <select
          value={providerFilter}
          onChange={(e) => setProviderFilter(e.target.value)}
          className={inputClassName}
        >
          <option value="All">All Providers</option>
          {PROVIDER_OPTIONS.map((provider) => (
            <option key={provider} value={provider}>
              {provider}
            </option>
          ))}
        </select>
        <select value={planFilter} onChange={(e) => setPlanFilter(e.target.value)} className={inputClassName}>
          <option value="All">All Plans</option>
          {PLAN_OPTIONS.map((plan) => (
            <option key={plan} value={plan}>
              {plan}
            </option>
          ))}
        </select>
        <select value={lockFilter} onChange={(e) => setLockFilter(e.target.value)} className={inputClassName}>
          <option value="All">All Locks</option>
          <option value="Unlocked">Unlocked</option>
          <option value="Locked">Locked</option>
        </select>
        <Button variant="secondary" size="sm" onClick={clearFilters}>
          Clear
        </Button>
      </div>

      {filtered.length === 0 ? (
        <div className="text-center py-16">
          <p className="text-gray-400 text-sm">No encounters match your filters.</p>
          <button onClick={clearFilters} className="text-blue-600 text-sm hover:underline mt-2" type="button">
            Clear all filters
          </button>
        </div>
      ) : (
        <>
          <Table
            columns={[
              { key: 'patientName', label: 'Patient Name' },
              { key: 'mrn', label: 'MRN' },
              { key: 'providerName', label: 'Provider' },
              { key: 'encounterDateText', label: 'Encounter Date' },
              { key: 'posText', label: 'POS' },
              { key: 'diagnoses', label: 'Diagnoses' },
              { key: 'chargeLines', label: 'Charge Lines' },
              { key: 'lockStatusBadge', label: 'Lock Status' },
            ]}
            data={paginated.map((row) => ({
              ...row,
              patientName: (
                <button
                  onClick={() => navigate(`/coding/encounter/${row.encounterId}`)}
                  className="text-blue-600 hover:underline font-medium text-left"
                  type="button"
                >
                  {row.patientName}
                </button>
              ),
              mrn: <span className="font-mono text-sm text-gray-600">{row.mrn}</span>,
              encounterDateText: formatEncounterDate(row.encounterDate),
              posText: row.pos === '02' ? '02 – Home' : '10 – Non-Home',
              diagnoses:
                row.diagnosisCount === 0 ? (
                  <span className="text-gray-400">—</span>
                ) : (
                  <span>
                    {row.diagnosisCount}{' '}
                    {!row.hasPrimaryDiagnosis ? (
                      <span title="No primary dx set" className="text-amber-500 ml-1">
                        ⚠
                      </span>
                    ) : null}
                  </span>
                ),
              chargeLines: (
                <div>
                  <p>{row.chargeLineCount} lines</p>
                  <p className="text-xs text-gray-500">${row.totalCharge.toFixed(2)}</p>
                </div>
              ),
              lockStatusBadge:
                row.lockStatus === 'Locked' ? (
                  <div>
                    <Badge status="Locked" />
                    <p className="text-xs text-gray-400 mt-0.5">{row.lockedBy}</p>
                  </div>
                ) : (
                  <Badge status="Unlocked" />
                ),
            }))}
            showActions={true}
            actions={[
              {
                label: 'Open Coding View',
                onClick: (row) => navigate(`/coding/encounter/${(row as WorklistItem).encounterId}`),
              },
            ]}
          />

          <Pagination currentPage={safeCurrentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
        </>
      )}
    </div>
  )
}
