import { useEffect, useState } from 'react'
import apiClient from '../../api/client'

// ── Types ─────────────────────────────────────────────────────────────────────

type AgingBucket = {
  bucket: string
  count: number
  amount: number
}

type PayerSummary = {
  payerId: number
  payerName: string | null
  denialCount: number
  totalAmountDenied: number
  denialRate: number
}

type ReasonSummary = {
  reasonCode: string | null
  description: string | null
  count: number
  totalAmount: number
}

type DashboardData = {
  totalOpenDenials: number
  totalAmountAtRisk: number
  totalAppealedDenials: number
  totalUnderpayments: number
  agingBreakdown: AgingBucket[]
  byPayer: PayerSummary[]
  byReasonCode: ReasonSummary[]
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function fmt(n: number) {
  return '$' + n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const AGING_COLOR: Record<string, string> = {
  '0-30': 'text-green-700 bg-green-50',
  '31-60': 'text-yellow-700 bg-yellow-50',
  '61-90': 'text-orange-700 bg-orange-50',
  '90+': 'text-red-700 bg-red-50',
}

// ── Component ─────────────────────────────────────────────────────────────────

export default function ArDashboard() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    apiClient
      .get<DashboardData>('api/v1/ar/dashboard')
      .then((res) => setData(res.data))
      .catch((err: unknown) => {
        const e = err as { response?: { data?: { message?: string } } }
        setError(e?.response?.data?.message ?? 'Failed to load AR dashboard data.')
      })
      .finally(() => setLoading(false))
  }, [])

  const topCards = [
    { label: 'Open Denials', value: data?.totalOpenDenials ?? 0, money: false, alert: true },
    { label: 'Amount at Risk', value: data ? fmt(data.totalAmountAtRisk) : '$0.00', money: true, alert: false },
    { label: 'Appealed Denials', value: data?.totalAppealedDenials ?? 0, money: false, alert: false },
    { label: 'Underpayments', value: data?.totalUnderpayments ?? 0, money: false, alert: false },
  ]

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-xl font-semibold text-gray-900">AR Dashboard</h1>
        <p className="text-sm text-gray-400 mt-0.5">Denial and recovery financial summary</p>
      </div>

      {error && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error}</p>
      )}

      {/* ── Top stat cards ── */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        {topCards.map((c) => (
          <div
            key={c.label}
            className={`border rounded-lg bg-white p-4 ${c.alert && (data?.totalOpenDenials ?? 0) > 0 ? 'border-red-200' : 'border-gray-200'}`}
          >
            <p className="text-xs text-gray-500">{c.label}</p>
            <p className={`text-2xl font-bold mt-1 ${loading ? 'text-gray-300' : c.alert && (data?.totalOpenDenials ?? 0) > 0 ? 'text-red-600' : 'text-gray-900'}`}>
              {loading ? '—' : c.value}
            </p>
          </div>
        ))}
      </div>

      {/* ── Middle row: Aging + By Payer ── */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

        {/* Aging breakdown */}
        <div className="border border-gray-200 rounded-lg bg-white">
          <div className="px-4 py-3 border-b border-gray-100">
            <h2 className="text-sm font-semibold text-gray-700">AR Aging Breakdown</h2>
            <p className="text-xs text-gray-400 mt-0.5">Open denials grouped by days outstanding</p>
          </div>
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100 text-left">
                <th className="px-4 py-2 text-xs font-medium text-gray-500">Bucket</th>
                <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">Denials</th>
                <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">Amount</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {loading && [...Array(4)].map((_, i) => (
                <tr key={i} className="animate-pulse">
                  <td className="px-4 py-2.5"><div className="h-3 w-16 bg-gray-200 rounded" /></td>
                  <td className="px-4 py-2.5 text-right"><div className="h-3 w-8 bg-gray-100 rounded ml-auto" /></td>
                  <td className="px-4 py-2.5 text-right"><div className="h-3 w-20 bg-gray-100 rounded ml-auto" /></td>
                </tr>
              ))}
              {!loading && (data?.agingBreakdown.length ?? 0) === 0 && (
                <tr><td colSpan={3} className="px-4 py-3 text-xs text-gray-400">No aging data.</td></tr>
              )}
              {!loading && data?.agingBreakdown.map((b) => (
                <tr key={b.bucket}>
                  <td className="px-4 py-2.5">
                    <span className={`inline-block rounded px-2 py-0.5 text-xs font-medium ${AGING_COLOR[b.bucket] ?? 'text-gray-600 bg-gray-50'}`}>
                      {b.bucket} days
                    </span>
                  </td>
                  <td className="px-4 py-2.5 text-right text-gray-700 font-medium">{b.count}</td>
                  <td className="px-4 py-2.5 text-right text-gray-600 font-mono text-xs">{fmt(b.amount)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* By payer */}
        <div className="border border-gray-200 rounded-lg bg-white">
          <div className="px-4 py-3 border-b border-gray-100">
            <h2 className="text-sm font-semibold text-gray-700">Denials by Payer</h2>
            <p className="text-xs text-gray-400 mt-0.5">Denial count, amount, and rate per payer</p>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100 text-left">
                  <th className="px-4 py-2 text-xs font-medium text-gray-500">Payer</th>
                  <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">Denials</th>
                  <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">Amount</th>
                  <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">Rate</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {loading && [...Array(4)].map((_, i) => (
                  <tr key={i} className="animate-pulse">
                    {[...Array(4)].map((_, j) => (
                      <td key={j} className="px-4 py-2.5"><div className="h-3 bg-gray-200 rounded w-full" /></td>
                    ))}
                  </tr>
                ))}
                {!loading && (data?.byPayer.length ?? 0) === 0 && (
                  <tr><td colSpan={4} className="px-4 py-3 text-xs text-gray-400">No payer data.</td></tr>
                )}
                {!loading && data?.byPayer.map((p) => (
                  <tr key={p.payerId}>
                    <td className="px-4 py-2.5 text-gray-700 text-sm">{p.payerName ?? '—'}</td>
                    <td className="px-4 py-2.5 text-right text-gray-700 font-medium">{p.denialCount}</td>
                    <td className="px-4 py-2.5 text-right text-gray-600 font-mono text-xs">{fmt(p.totalAmountDenied)}</td>
                    <td className="px-4 py-2.5 text-right text-xs text-gray-500">{p.denialRate.toFixed(1)}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* ── Denial reasons ── */}
      <div className="border border-gray-200 rounded-lg bg-white">
        <div className="px-4 py-3 border-b border-gray-100">
          <h2 className="text-sm font-semibold text-gray-700">Denials by Reason Code</h2>
          <p className="text-xs text-gray-400 mt-0.5">Top denial reason codes across all open denials</p>
        </div>
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-100 text-left">
              <th className="px-4 py-2 text-xs font-medium text-gray-500">Code</th>
              <th className="px-4 py-2 text-xs font-medium text-gray-500">Description</th>
              <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">Count</th>
              <th className="px-4 py-2 text-xs font-medium text-gray-500 text-right">Total Amount</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {loading && [...Array(5)].map((_, i) => (
              <tr key={i} className="animate-pulse">
                {[...Array(4)].map((_, j) => (
                  <td key={j} className="px-4 py-2.5"><div className="h-3 bg-gray-200 rounded w-full" /></td>
                ))}
              </tr>
            ))}
            {!loading && (data?.byReasonCode.length ?? 0) === 0 && (
              <tr><td colSpan={4} className="px-4 py-3 text-xs text-gray-400">No denial reason data.</td></tr>
            )}
            {!loading && data?.byReasonCode.map((r, i) => (
              <tr key={i}>
                <td className="px-4 py-2.5 font-mono text-xs text-gray-600">{r.reasonCode ?? '—'}</td>
                <td className="px-4 py-2.5 text-gray-700">{r.description ?? '—'}</td>
                <td className="px-4 py-2.5 text-right text-gray-700 font-medium">{r.count}</td>
                <td className="px-4 py-2.5 text-right font-mono text-xs text-gray-600">{fmt(r.totalAmount)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
