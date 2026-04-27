import { useEffect, useState } from 'react'
import apiClient from '../../api/client'

type Summary = {
  totalPatients: number
  todayEncounters: number
  unbilledEncounters: number
  rejectedClaims: number
  pendingBatches: number
  outstandingStatements: number
  outstandingAmount: number
}

function fmt(n: number) {
  return '$' + n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

type CardProps = {
  label: string
  value: string | number
  sub?: string
  alert?: boolean
  loading: boolean
}

function StatCard({ label, value, sub, alert, loading }: CardProps) {
  return (
    <div className={`border rounded-lg bg-white p-4 ${alert ? 'border-red-200' : 'border-gray-200'}`}>
      <p className="text-xs text-gray-500">{label}</p>
      <p className={`text-2xl font-bold mt-1 ${loading ? 'text-gray-300' : alert ? 'text-red-600' : 'text-gray-900'}`}>
        {loading ? '—' : value}
      </p>
      {sub && <p className="text-xs text-gray-400 mt-1">{sub}</p>}
    </div>
  )
}

export default function Dashboard() {
  const [data, setData] = useState<Summary | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    apiClient
      .get<Summary>('api/v1/reports/frontdesk/summary')
      .then((res) => setData(res.data))
      .catch(() => setError('Failed to load dashboard data.'))
      .finally(() => setLoading(false))
  }, [])

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-xl font-semibold text-gray-900">Dashboard</h1>
        <p className="text-sm text-gray-400 mt-0.5">Front desk overview</p>
      </div>

      {error && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error}</p>
      )}

      <div>
        <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-3">
          Patients &amp; Encounters
        </p>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <StatCard
            label="Total Patients"
            value={data?.totalPatients ?? 0}
            sub="All registered patients"
            loading={loading}
          />
          <StatCard
            label="Today's Encounters"
            value={data?.todayEncounters ?? 0}
            sub="Scheduled for today"
            loading={loading}
          />
          <StatCard
            label="Unbilled Encounters"
            value={data?.unbilledEncounters ?? 0}
            sub="No claim created yet"
            alert={(data?.unbilledEncounters ?? 0) > 0}
            loading={loading}
          />
        </div>
      </div>

      <div>
        <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-3">
          Claims &amp; Batches
        </p>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <StatCard
            label="Rejected Claims"
            value={data?.rejectedClaims ?? 0}
            sub="Need resubmission"
            alert={(data?.rejectedClaims ?? 0) > 0}
            loading={loading}
          />
          <StatCard
            label="Pending Batches"
            value={data?.pendingBatches ?? 0}
            sub="Ready or submitted, awaiting ack"
            loading={loading}
          />
        </div>
      </div>

      <div>
        <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-3">
          Patient Statements
        </p>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <StatCard
            label="Outstanding Statements"
            value={data?.outstandingStatements ?? 0}
            sub="Generated or sent, not yet paid"
            loading={loading}
          />
          <StatCard
            label="Total Amount Due"
            value={data ? fmt(data.outstandingAmount) : '$0.00'}
            sub="Across all outstanding statements"
            loading={loading}
          />
        </div>
      </div>
    </div>
  )
}
