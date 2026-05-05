import { useEffect, useState } from 'react'
import apiClient from '../../api/client'

type AgingRow = {
  amountDenied: number
  agingBucket: string
}

type BucketGroup = {
  bucket: string
  count: number
  totalAmount: number
}

const BUCKET_ORDER = ['0-30', '31-60', '61-90', '90+']

const BUCKET_STYLE: Record<string, string> = {
  '0-30': 'text-green-700 bg-green-50',
  '31-60': 'text-yellow-700 bg-yellow-50',
  '61-90': 'text-orange-700 bg-orange-50',
  '90+': 'text-red-700 bg-red-50',
}

function fmt(n: number) {
  return '$' + n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

export default function ArAgingTable() {
  const [buckets, setBuckets] = useState<BucketGroup[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function load() {
      try {
        const res = await apiClient.get<AgingRow[]>('api/v1/reports/export/ar-aging')
        const map: Record<string, BucketGroup> = {}
        for (const r of res.data) {
          const b = r.agingBucket
          if (!map[b]) map[b] = { bucket: b, count: 0, totalAmount: 0 }
          map[b].count++
          map[b].totalAmount += r.amountDenied
        }
        setBuckets(BUCKET_ORDER.filter((b) => map[b]).map((b) => map[b]))
      } catch {
        setError('Failed to load AR aging data.')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  return (
    <div className="border border-gray-200 rounded-lg bg-white">
      <div className="px-4 py-3 border-b border-gray-100">
        <h2 className="text-sm font-semibold text-gray-700">AR Aging — Denied Claims</h2>
        <p className="text-xs text-gray-400 mt-0.5">Grouped by days since denial</p>
      </div>

      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-gray-100 text-left">
            <th className="px-4 py-2 text-xs font-medium text-gray-500">Bucket</th>
            <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">Denials</th>
            <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">
              Amount Denied
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-50">
          {loading &&
            [...Array(4)].map((_, i) => (
              <tr key={i} className="animate-pulse">
                <td className="px-4 py-2.5">
                  <div className="h-3 w-16 bg-gray-200 rounded" />
                </td>
                <td className="px-4 py-2.5 text-right">
                  <div className="h-3 w-8 bg-gray-100 rounded ml-auto" />
                </td>
                <td className="px-4 py-2.5 text-right">
                  <div className="h-3 w-20 bg-gray-100 rounded ml-auto" />
                </td>
              </tr>
            ))}

          {error && (
            <tr>
              <td colSpan={3} className="px-4 py-3 text-xs text-red-500">
                {error}
              </td>
            </tr>
          )}

          {!loading && !error && buckets.length === 0 && (
            <tr>
              <td colSpan={3} className="px-4 py-3 text-xs text-gray-400">
                No aged denials.
              </td>
            </tr>
          )}

          {!loading &&
            !error &&
            buckets.map((b) => (
              <tr key={b.bucket}>
                <td className="px-4 py-2.5">
                  <span
                    className={`inline-block rounded px-2 py-0.5 text-xs font-medium ${BUCKET_STYLE[b.bucket] ?? 'text-gray-600 bg-gray-50'}`}
                  >
                    {b.bucket} days
                  </span>
                </td>
                <td className="px-4 py-2.5 text-right text-gray-700 font-medium">{b.count}</td>
                <td className="px-4 py-2.5 text-right text-gray-600 font-mono text-xs">
                  {fmt(b.totalAmount)}
                </td>
              </tr>
            ))}
        </tbody>
      </table>
    </div>
  )
}
