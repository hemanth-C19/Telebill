import { useCallback, useEffect, useState } from 'react'
import apiClient from '../../api/client'
import { Button } from '../shared/ui/Button'

type KpiDetail = {
  reportId: number
  scope: string
  generatedDate: string
  ccr: number | null
  fpar: number | null
  dso: number | null
  denialRate: number | null
  tat: number | null
}

type GenerateResponse = {
  success: boolean
  error: string | null
}

function pct(v: number | null | undefined): string {
  if (v == null) return '—'
  return `${v.toFixed(1)}%`
}

function days(v: number | null | undefined): string {
  if (v == null) return '—'
  return `${v.toFixed(1)} days`
}

export default function KpiCards() {
  const [data, setData] = useState<KpiDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [generating, setGenerating] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const generateKpi = useCallback(async () => {
    setGenerating(true)
    const now = new Date()
    const from = new Date(now)
    from.setDate(now.getDate() - 90)
    try {
      await apiClient.post<GenerateResponse>('api/v1/reports/kpi/generate', {
        scope: 'Period',
        periodStart: from.toISOString(),
        periodEnd: now.toISOString(),
      })
      const res = await apiClient.get<KpiDetail>('api/v1/reports/kpi/latest?scope=Period')
      setData(res.data)
    } catch {
      setError('Failed to generate KPI report.')
    } finally {
      setGenerating(false)
      setLoading(false)
    }
  }, [])

  const fetchLatest = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await apiClient.get<KpiDetail>('api/v1/reports/kpi/latest?scope=Period')
      setData(res.data)
      setLoading(false)
    } catch (err: unknown) {
      const e = err as { response?: { status?: number } }
      if (e?.response?.status === 404) {
        // No report yet — auto-generate on first load
        await generateKpi()
      } else {
        setError('Failed to load KPI data.')
        setLoading(false)
      }
    }
  }, [generateKpi])

  useEffect(() => {
    fetchLatest()
  }, [fetchLatest])

  async function handleGenerate() {
    setError(null)
    await generateKpi()
  }

  const metrics = [
    {
      label: 'Clean Claim Rate',
      value: pct(data?.ccr),
      desc: '% claims with no scrub errors',
    },
    {
      label: 'First Pass Acceptance',
      value: pct(data?.fpar),
      desc: '% accepted on first submission',
    },
    {
      label: 'Denial Rate',
      value: pct(data?.denialRate),
      desc: '% of submitted claims denied',
    },
    {
      label: 'Days Sales Outstanding',
      value: days(data?.dso),
      desc: 'Avg days from encounter to payment',
    },
    {
      label: 'Payer Turnaround',
      value: days(data?.tat),
      desc: 'Avg days submission to 277CA ack',
    },
  ]

  return (
    <div>
      <div className="flex items-center justify-between mb-3">
        <div>
          <h2 className="text-sm font-semibold text-gray-700">Key Performance Indicators</h2>
          {data && (
            <p className="text-xs text-gray-400 mt-0.5">
              Last computed: {new Date(data.generatedDate).toLocaleDateString()} · Scope:{' '}
              {data.scope}
            </p>
          )}
        </div>
        <Button
          variant="secondary"
          size="sm"
          onClick={handleGenerate}
          disabled={generating || loading}
        >
          {generating ? 'Computing…' : 'Refresh KPIs (last 90 days)'}
        </Button>
      </div>

      {error && <p className="text-xs text-red-500 mb-3">{error}</p>}

      <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
        {metrics.map((m) => (
          <div key={m.label} className="border border-gray-200 rounded-lg bg-white p-4">
            <p className="text-xs text-gray-500 leading-snug">{m.label}</p>
            <p className="text-2xl font-bold text-gray-900 mt-1">
              {loading || generating ? <span className="text-gray-300">—</span> : m.value}
            </p>
            <p className="text-xs text-gray-400 mt-1 leading-snug">{m.desc}</p>
          </div>
        ))}
      </div>

      {!loading && !data && !error && (
        <p className="text-xs text-gray-400 mt-2">
          No KPI report found. Click "Refresh KPIs" to generate one.
        </p>
      )}
    </div>
  )
}
