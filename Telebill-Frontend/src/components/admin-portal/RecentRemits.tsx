import { useEffect, useState } from 'react'
import apiClient from '../../api/client'

type RemitRow = {
  remitId: number
  payerName: string
  batchId: number
  receivedDate: string
  remitStatus: string
  totalPosted: number
}

const STATUS_CLS: Record<string, string> = {
  Loaded: 'text-blue-600',
  Posted: 'text-green-600',
  Failed: 'text-red-600',
}

function fmt(n: number) {
  return '$' + n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

function fmtDate(s: string) {
  return new Date(s).toLocaleDateString()
}

export default function RecentRemits() {
  const [rows, setRows] = useState<RemitRow[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function load() {
      try {
        const res = await apiClient.get<RemitRow[]>('api/v1/reports/export/remits')
        setRows(res.data.slice(0, 10))
      } catch {
        setError('Failed to load remit data.')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  return (
    <div className="border border-gray-200 rounded-lg bg-white">
      <div className="px-4 py-3 border-b border-gray-100">
        <h2 className="text-sm font-semibold text-gray-700">Recent Remittances</h2>
        <p className="text-xs text-gray-400 mt-0.5">Latest 10 ERA / remit records</p>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-100 text-left">
              <th className="px-4 py-2 text-xs font-medium text-gray-500">Remit</th>
              <th className="px-4 py-2 text-xs font-medium text-gray-500">Payer</th>
              <th className="px-4 py-2 text-xs font-medium text-gray-500">Batch</th>
              <th className="px-4 py-2 text-xs font-medium text-gray-500">Received</th>
              <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">
                Total Posted
              </th>
              <th className="px-4 py-2 text-xs font-medium text-gray-500">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {loading &&
              [...Array(5)].map((_, i) => (
                <tr key={i} className="animate-pulse">
                  {[...Array(6)].map((_, j) => (
                    <td key={j} className="px-4 py-2.5">
                      <div className="h-3 bg-gray-200 rounded w-full" />
                    </td>
                  ))}
                </tr>
              ))}

            {error && (
              <tr>
                <td colSpan={6} className="px-4 py-3 text-xs text-red-500">
                  {error}
                </td>
              </tr>
            )}

            {!loading && !error && rows.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-3 text-xs text-gray-400">
                  No remit data.
                </td>
              </tr>
            )}

            {!loading &&
              !error &&
              rows.map((r) => (
                <tr key={r.remitId}>
                  <td className="px-4 py-2.5 text-gray-500 font-mono text-xs">#{r.remitId}</td>
                  <td className="px-4 py-2.5 text-gray-700 text-sm">{r.payerName}</td>
                  <td className="px-4 py-2.5 text-gray-500 font-mono text-xs">#{r.batchId}</td>
                  <td className="px-4 py-2.5 text-gray-500 text-xs">{fmtDate(r.receivedDate)}</td>
                  <td className="px-4 py-2.5 text-right text-gray-600 font-mono text-xs">
                    {fmt(r.totalPosted)}
                  </td>
                  <td className="px-4 py-2.5">
                    <span
                      className={`text-xs font-medium ${STATUS_CLS[r.remitStatus] ?? 'text-gray-500'}`}
                    >
                      {r.remitStatus}
                    </span>
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
