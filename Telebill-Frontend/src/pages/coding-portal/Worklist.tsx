import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import apiClient from '../../api/client'
import { Badge } from '../../components/shared/ui/Badge'
import { Button } from '../../components/shared/ui/Button'
import { Pagination } from '../../components/shared/ui/Pagination'
import { Table } from '../../components/shared/ui/Table'

type WorklistItem = {
  encounterId: number
  patientName: string
  providerId: number | null
  providerName: string
  encounterDateTime: string
  visitType: string
  planId: number | null
  planName: string
  chargeLineCount: number
  totalCharge: number
  diagnosisCount: number
  hasPrimaryDiagnosis: boolean
  status: string
}

type FilterOption = { id: number; name: string }

type WorklistFilters = {
  providers: FilterOption[]
  plans: FilterOption[]
}

const PAGE_SIZE = 10

const selectCls =
  'border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

function formatDate(input: string): string {
  const d = new Date(input)
  if (Number.isNaN(d.getTime())) return input
  return d.toLocaleString('en-US', {
    month: 'short',
    day: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
}

export default function Worklist() {
  const navigate = useNavigate()
  const [worklist, setWorklist] = useState<WorklistItem[]>([])
  const [filters, setFilters] = useState<WorklistFilters>({ providers: [], plans: [] })
  const [selectedProviderId, setSelectedProviderId] = useState<number | null>(null)
  const [selectedPlanId, setSelectedPlanId] = useState<number | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [currentPage, setCurrentPage] = useState(1)

  const fetchWorklist = useCallback(async (providerId: number | null, planId: number | null) => {
    setLoading(true)
    setError(null)
    try {
      const params = new URLSearchParams()
      if (providerId != null) params.set('providerId', String(providerId))
      if (planId != null) params.set('planId', String(planId))
      const url = params.size > 0
        ? `api/v1/coding/worklist?${params.toString()}`
        : 'api/v1/coding/worklist'
      const res = await apiClient.get<WorklistItem[]>(url)
      console.log(res.data)
      setWorklist(res.data)
    } catch {
      setError('Failed to load coding worklist.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    apiClient
      .get<WorklistFilters>('api/v1/coding/worklist/filters')
      .then(res => setFilters(res.data))
      .catch(() => {})

    fetchWorklist(null, null)
  }, [fetchWorklist])

  function handleProviderChange(e: React.ChangeEvent<HTMLSelectElement>) {
    const id = e.target.value === '' ? null : Number(e.target.value)
    setSelectedProviderId(id)
    setCurrentPage(1)
    fetchWorklist(id, selectedPlanId)
  }

  function handlePlanChange(e: React.ChangeEvent<HTMLSelectElement>) {
    const id = e.target.value === '' ? null : Number(e.target.value)
    setSelectedPlanId(id)
    setCurrentPage(1)
    fetchWorklist(selectedProviderId, id)
  }

  function clearFilters() {
    setSelectedProviderId(null)
    setSelectedPlanId(null)
    setCurrentPage(1)
    fetchWorklist(null, null)
  }

  const totalPages = Math.max(1, Math.ceil(worklist.length / PAGE_SIZE))
  const safeCurrentPage = Math.min(currentPage, totalPages)
  const paginated = worklist.slice((safeCurrentPage - 1) * PAGE_SIZE, safeCurrentPage * PAGE_SIZE)

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold text-gray-900">Coding Worklist</h1>

      {error != null && (
        <p className="mb-4 rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error}</p>
      )}

      <div className="mb-4 flex flex-wrap items-center gap-3">
        <select
          value={selectedProviderId ?? ''}
          onChange={handleProviderChange}
          className={selectCls}
        >
          <option value="">All Providers</option>
          {filters.providers.map(p => (
            <option key={p.id} value={p.id}>
              {p.name}
            </option>
          ))}
        </select>

        <select
          value={selectedPlanId ?? ''}
          onChange={handlePlanChange}
          className={selectCls}
        >
          <option value="">All Plans</option>
          {filters.plans.map(p => (
            <option key={p.id} value={p.id}>
              {p.name}
            </option>
          ))}
        </select>

        {(selectedProviderId != null || selectedPlanId != null) && (
          <Button variant="secondary" size="sm" onClick={clearFilters}>
            Clear
          </Button>
        )}
      </div>

      {!loading && worklist.length === 0 ? (
        <div className="py-16 text-center">
          <p className="text-sm text-gray-400">No ReadyForCoding encounters found.</p>
        </div>
      ) : (
        <>
          <Table
            columns={[
              { key: 'patientName', label: 'Patient Name' },
              { key: 'providerName', label: 'Provider' },
              { key: 'encounterDateText', label: 'Encounter Date' },
              { key: 'visitType', label: 'Visit Type' },
              { key: 'planName', label: 'Plan' },
              { key: 'diagnoses', label: 'Diagnoses' },
              { key: 'chargeLines', label: 'Charge Lines' },
              { key: 'statusBadge', label: 'Status' },
            ]}
            loading={loading}
            data={paginated.map(row => ({
              encounterId: row.encounterId,
              providerName: row.providerName ?? '—',
              planName: row.planName ?? '—',
              visitType: row.visitType ?? '—',
              patientName: (
                <button
                  onClick={() => navigate(`/coding/encounter/${row.encounterId}`)}
                  className="text-left font-medium text-blue-600 hover:underline"
                  type="button"
                >
                  {row.patientName}
                </button>
              ),
              encounterDateText: formatDate(row.encounterDateTime),
              diagnoses:
                row.diagnosisCount === 0 ? (
                  <span className="text-gray-400">—</span>
                ) : (
                  <span>
                    {row.diagnosisCount}
                    {!row.hasPrimaryDiagnosis && (
                      <span title="No primary diagnosis set" className="ml-1 text-amber-500">
                        ⚠
                      </span>
                    )}
                  </span>
                ),
              chargeLines: (
                <div>
                  <p>{row.chargeLineCount} lines</p>
                  <p className="text-xs text-gray-500">${row.totalCharge.toFixed(2)}</p>
                </div>
              ),
              statusBadge: <Badge status={row.status} />,
            }))}
            showActions={true}
            actions={[
              {
                label: 'Open Coding View',
                onClick: row => navigate(`/coding/encounter/${row.encounterId as number}`),
              },
            ]}
          />
          <Pagination
            currentPage={safeCurrentPage}
            totalPages={totalPages}
            onPageChange={setCurrentPage}
          />
        </>
      )}
    </div>
  )
}
