import { useEffect, useState } from 'react'
import apiClient from '../../api/client'

type ClaimsRow = {
  claimId: number
  claimStatus: string
  totalCharge: number
}

type StatusGroup = {
  status: string
  count: number
  totalCharge: number
}

function fmt(n: number) {
  return '$' + n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const STATUS_COLOR: Record<string, string> = {
  Accepted: 'text-green-600',
  Paid: 'text-green-700',
  Submitted: 'text-blue-600',
  Denied: 'text-red-600',
  Rejected: 'text-red-700',
  Draft: 'text-gray-500',
  Batched: 'text-yellow-600',
  PartiallyPaid: 'text-yellow-700',
}

export default function ClaimsSummary() {
  const [groups, setGroups] = useState<StatusGroup[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function load() {
      try {
        const res = await apiClient.get<ClaimsRow[]>('api/v1/reports/export/claims')
        const rows = res.data
        setTotal(rows.length)
        const map: Record<string, StatusGroup> = {}
        for (const r of rows) {
          if (!map[r.claimStatus]) {
            map[r.claimStatus] = { status: r.claimStatus, count: 0, totalCharge: 0 }
          }
          map[r.claimStatus].count++
          map[r.claimStatus].totalCharge += r.totalCharge
        }
        setGroups(Object.values(map).sort((a, b) => b.count - a.count))
      } catch {
        setError('Failed to load claims data.')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  return (
    <div className="border border-gray-200 rounded-lg bg-white">
      <div className="px-4 py-3 border-b border-gray-100">
        <h2 className="text-sm font-semibold text-gray-700">Claims by Status</h2>
        {!loading && (
          <p className="text-xs text-gray-400 mt-0.5">{total} total claims</p>
        )}
      </div>

      <div className="divide-y divide-gray-100">
        {loading &&
          [...Array(5)].map((_, i) => (
            <div key={i} className="px-4 py-2.5 flex justify-between animate-pulse">
              <div className="h-3 w-24 bg-gray-200 rounded" />
              <div className="h-3 w-16 bg-gray-100 rounded" />
            </div>
          ))}

        {error && (
          <p className="px-4 py-3 text-xs text-red-500">{error}</p>
        )}

        {!loading && !error && groups.length === 0 && (
          <p className="px-4 py-3 text-xs text-gray-400">No claims data.</p>
        )}

        {!loading &&
          !error &&
          groups.map((g) => (
            <div key={g.status} className="px-4 py-2.5 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <span
                  className={`text-sm font-medium ${STATUS_COLOR[g.status] ?? 'text-gray-700'}`}
                >
                  {g.status}
                </span>
                <span className="text-xs text-gray-400">{g.count}</span>
              </div>
              <span className="text-xs text-gray-600 font-mono">{fmt(g.totalCharge)}</span>
            </div>
          ))}
      </div>
    </div>
  )
}
