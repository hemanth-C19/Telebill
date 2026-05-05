import { useCallback, useEffect, useState } from 'react'
import apiClient from '../../../api/client'
import { Badge } from '../../../components/shared/ui/Badge'
import { Button } from '../../../components/shared/ui/Button'
import { Card } from '../../../components/shared/ui/Card'
import { Dialog } from '../../../components/shared/ui/Dialog'

type StatementLineItem = {
  claimID: number
  serviceDate: string
  cptHcpcs: string
  billed: number
  insurancePaid: number
  adjustment: number
  patientDue: number
}

type Statement = {
  statementID: number
  patientID: number
  patientName: string
  mrn: string
  periodStart: string
  periodEnd: string
  generatedDate: string
  amountDue: number
  lineItems: StatementLineItem[]
  status: string
}

type StatementListResponse = {
  totalCount: number
  statements: Statement[]
}

type GenerateForm = {
  patientID: string
  periodStart: string
  periodEnd: string
}

type BatchForm = {
  periodStart: string
  periodEnd: string
  schedulerKey: string
}

type BatchResult = {
  jobID: string
  startedAt: string
  completedAt: string
  statementsGenerated: number
  patientsProcessed: number
  totalAmountBilled: number
  durationSeconds: number
}

type Filters = {
  patientID: string
  status: string
  dateFrom: string
  dateTo: string
}

function extractErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const resp = (err as { response?: { data?: { error?: string; message?: string } } }).response
    return resp?.data?.error ?? resp?.data?.message ?? 'An unexpected error occurred.'
  }
  return 'An unexpected error occurred.'
}

const inputCls =
  'border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'
const PAGE_SIZE = 25
const EMPTY_FILTERS: Filters = { patientID: '', status: '', dateFrom: '', dateTo: '' }

export default function StatementsTab() {
  const [statements, setStatements] = useState<Statement[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [actionError, setActionError] = useState<string | null>(null)
  const [actionSuccess, setActionSuccess] = useState<string | null>(null)
  const [page, setPage] = useState(1)

  // Filters
  const [form, setForm] = useState<Filters>(EMPTY_FILTERS)
  const [applied, setApplied] = useState<Filters>(EMPTY_FILTERS)

  // Expanded row (lazy-loaded line items)
  const [expandedID, setExpandedID] = useState<number | null>(null)
  const [expandedStatement, setExpandedStatement] = useState<Statement | null>(null)
  const [loadingDetail, setLoadingDetail] = useState(false)

  // Generate single statement dialog
  const [showGenerate, setShowGenerate] = useState(false)
  const [generateForm, setGenerateForm] = useState<GenerateForm>({
    patientID: '',
    periodStart: '',
    periodEnd: '',
  })
  const [generating, setGenerating] = useState(false)

  // Batch generate dialog
  const [showBatch, setShowBatch] = useState(false)
  const [batchForm, setBatchForm] = useState<BatchForm>({
    periodStart: '',
    periodEnd: '',
    schedulerKey: '',
  })
  const [batchRunning, setBatchRunning] = useState(false)
  const [batchResult, setBatchResult] = useState<BatchResult | null>(null)

  const loadStatements = useCallback(async () => {
    setLoading(true)
    setActionError(null)
    try {
      const params = new URLSearchParams()
      if (applied.patientID) params.set('patientID', applied.patientID)
      if (applied.status) params.set('status', applied.status)
      if (applied.dateFrom) params.set('dateFrom', applied.dateFrom)
      if (applied.dateTo) params.set('dateTo', applied.dateTo)
      params.set('page', String(page))
      params.set('pageSize', String(PAGE_SIZE))

      const res = await apiClient.get<StatementListResponse>(`api/v1/posting/statements?${params}`)
      setStatements(res.data.statements)
      setTotalCount(res.data.totalCount)
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [applied, page])

  useEffect(() => {
    loadStatements()
  }, [loadStatements])

  function handleApply() {
    setApplied(form)
    setPage(1)
  }

  function handleReset() {
    setForm(EMPTY_FILTERS)
    setApplied(EMPTY_FILTERS)
    setPage(1)
  }

  async function handleExpandRow(statementID: number) {
    if (expandedID === statementID) {
      setExpandedID(null)
      setExpandedStatement(null)
      return
    }
    setExpandedID(statementID)
    setLoadingDetail(true)
    try {
      const res = await apiClient.get<Statement>(`api/v1/posting/statements/${statementID}`)
      setExpandedStatement(res.data)
    } catch {
      setExpandedStatement(null)
    } finally {
      setLoadingDetail(false)
    }
  }

  async function handleGenerate() {
    if (!generateForm.patientID || !generateForm.periodStart || !generateForm.periodEnd) return
    setGenerating(true)
    setActionError(null)
    try {
      await apiClient.post('api/v1/posting/statements/generate', {
        patientID: Number(generateForm.patientID),
        periodStart: generateForm.periodStart,
        periodEnd: generateForm.periodEnd,
      })
      setShowGenerate(false)
      setGenerateForm({ patientID: '', periodStart: '', periodEnd: '' })
      setActionSuccess('Statement generated successfully.')
      await loadStatements()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setGenerating(false)
    }
  }

  async function handleBatchGenerate() {
    if (!batchForm.periodStart || !batchForm.periodEnd || !batchForm.schedulerKey) return
    setBatchRunning(true)
    setActionError(null)
    try {
      const res = await apiClient.post<BatchResult>('api/v1/posting/statements/generate-batch', {
        periodStart: batchForm.periodStart,
        periodEnd: batchForm.periodEnd,
        schedulerKey: batchForm.schedulerKey,
      })
      setBatchResult(res.data)
      await loadStatements()
    } catch (err) {
      setActionError(extractErrorMessage(err))
    } finally {
      setBatchRunning(false)
    }
  }

  const totalPages = Math.ceil(totalCount / PAGE_SIZE)

  return (
    <div className="flex flex-col gap-6">
      {actionError && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{actionError}</p>
      )}
      {actionSuccess && (
        <p className="rounded-md bg-green-50 px-4 py-3 text-sm text-green-700">{actionSuccess}</p>
      )}

      <Card title="Statements">
        {/* Filters */}
        <div className="flex flex-wrap items-end gap-3 mb-5">
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Patient ID</label>
            <input
              type="number"
              placeholder="e.g. 42"
              value={form.patientID}
              onChange={(e) => setForm((f) => ({ ...f, patientID: e.target.value }))}
              className={`w-28 ${inputCls}`}
            />
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Status</label>
            <select
              value={form.status}
              onChange={(e) => setForm((f) => ({ ...f, status: e.target.value }))}
              className={inputCls}
            >
              <option value="">All</option>
              <option value="Open">Open</option>
              <option value="Closed">Closed</option>
            </select>
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Date From</label>
            <input
              type="date"
              value={form.dateFrom}
              onChange={(e) => setForm((f) => ({ ...f, dateFrom: e.target.value }))}
              className={inputCls}
            />
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs text-gray-500">Date To</label>
            <input
              type="date"
              value={form.dateTo}
              onChange={(e) => setForm((f) => ({ ...f, dateTo: e.target.value }))}
              className={inputCls}
            />
          </div>
          <Button variant="primary" size="sm" onClick={handleApply}>
            Apply
          </Button>
          <Button variant="secondary" size="sm" onClick={handleReset}>
            Reset
          </Button>
          <div className="ml-auto flex gap-2">
            <Button
              variant="secondary"
              size="sm"
              onClick={() => {
                setShowBatch(true)
                setBatchResult(null)
                setBatchForm({ periodStart: '', periodEnd: '', schedulerKey: '' })
              }}
            >
              Generate Batch
            </Button>
            <Button
              variant="primary"
              size="sm"
              onClick={() => {
                setShowGenerate(true)
                setGenerateForm({ patientID: '', periodStart: '', periodEnd: '' })
              }}
            >
              Generate Statement
            </Button>
          </div>
        </div>

        {/* Statements Table with expandable rows */}
        <div className="w-full overflow-x-auto">
          <table className="w-full border-collapse text-left text-sm">
            <thead>
              <tr className="border-b border-gray-200 bg-gray-50">
                {[
                  'Statement ID',
                  'Patient Name',
                  'MRN',
                  'Period',
                  'Amount Due',
                  'Generated Date',
                  'Status',
                  '',
                ].map((h) => (
                  <th
                    key={h}
                    className="whitespace-nowrap px-4 py-3 text-xs font-semibold uppercase tracking-wider text-gray-500"
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {loading &&
                Array.from({ length: 5 }).map((_, i) => (
                  <tr key={`sk-${i}`} className="animate-pulse">
                    {Array.from({ length: 8 }).map((__, j) => (
                      <td key={j} className="px-4 py-3">
                        <div className="h-4 w-full rounded bg-gray-200" />
                      </td>
                    ))}
                  </tr>
                ))}

              {!loading && statements.length === 0 && (
                <tr>
                  <td colSpan={8} className="px-4 py-8 text-center text-sm text-gray-400">
                    No statements found
                  </td>
                </tr>
              )}

              {!loading &&
                statements.map((s) => (
                  <StatementRow
                    key={s.statementID}
                    statement={s}
                    isExpanded={expandedID === s.statementID}
                    expandedData={expandedID === s.statementID ? expandedStatement : null}
                    loadingDetail={expandedID === s.statementID && loadingDetail}
                    onToggle={() => handleExpandRow(s.statementID)}
                  />
                ))}
            </tbody>
          </table>
        </div>

        {totalPages > 1 && (
          <div className="flex items-center justify-between mt-4 text-sm text-gray-500">
            <span>
              Page {page} of {totalPages} &mdash; {totalCount} records
            </span>
            <div className="flex gap-2">
              <Button variant="secondary" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                ← Prev
              </Button>
              <Button
                variant="secondary"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                Next →
              </Button>
            </div>
          </div>
        )}
      </Card>

      {/* Generate Statement Dialog */}
      <Dialog isOpen={showGenerate} onClose={() => setShowGenerate(false)} title="Generate Statement">
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Patient ID *</label>
            <input
              type="number"
              value={generateForm.patientID}
              onChange={(e) => setGenerateForm((f) => ({ ...f, patientID: e.target.value }))}
              placeholder="Enter patient ID"
              className={`w-full ${inputCls}`}
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Period Start *</label>
              <input
                type="date"
                value={generateForm.periodStart}
                onChange={(e) => setGenerateForm((f) => ({ ...f, periodStart: e.target.value }))}
                className={`w-full ${inputCls}`}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Period End *</label>
              <input
                type="date"
                value={generateForm.periodEnd}
                onChange={(e) => setGenerateForm((f) => ({ ...f, periodEnd: e.target.value }))}
                className={`w-full ${inputCls}`}
              />
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-1">
            <Button variant="secondary" onClick={() => setShowGenerate(false)}>
              Cancel
            </Button>
            <Button
              variant="primary"
              disabled={
                !generateForm.patientID ||
                !generateForm.periodStart ||
                !generateForm.periodEnd ||
                generating
              }
              onClick={handleGenerate}
            >
              {generating ? 'Generating...' : 'Generate'}
            </Button>
          </div>
        </div>
      </Dialog>

      {/* Generate Batch Statements Dialog */}
      <Dialog
        isOpen={showBatch}
        onClose={() => setShowBatch(false)}
        title="Generate Batch Statements"
      >
        <div className="space-y-4">
          {batchResult == null ? (
            <>
              <p className="text-sm text-gray-500">
                Generates statements for all patients with open balances in the selected billing period.
              </p>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Period Start *</label>
                  <input
                    type="date"
                    value={batchForm.periodStart}
                    onChange={(e) => setBatchForm((f) => ({ ...f, periodStart: e.target.value }))}
                    className={`w-full ${inputCls}`}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Period End *</label>
                  <input
                    type="date"
                    value={batchForm.periodEnd}
                    onChange={(e) => setBatchForm((f) => ({ ...f, periodEnd: e.target.value }))}
                    className={`w-full ${inputCls}`}
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Scheduler Key *</label>
                <input
                  value={batchForm.schedulerKey}
                  onChange={(e) => setBatchForm((f) => ({ ...f, schedulerKey: e.target.value }))}
                  placeholder="Enter scheduler key"
                  className={`w-full ${inputCls}`}
                />
              </div>
              <div className="flex justify-end gap-2 pt-1">
                <Button variant="secondary" onClick={() => setShowBatch(false)}>
                  Cancel
                </Button>
                <Button
                  variant="primary"
                  disabled={
                    !batchForm.periodStart ||
                    !batchForm.periodEnd ||
                    !batchForm.schedulerKey ||
                    batchRunning
                  }
                  onClick={handleBatchGenerate}
                >
                  {batchRunning ? 'Running...' : 'Generate Batch'}
                </Button>
              </div>
            </>
          ) : (
            <>
              <div className="rounded-lg bg-green-50 border border-green-200 p-4 space-y-2 text-sm">
                <p className="font-semibold text-green-700">Batch completed successfully</p>
                <p className="text-gray-600">
                  Statements generated: <strong>{batchResult.statementsGenerated}</strong>
                </p>
                <p className="text-gray-600">
                  Patients processed: <strong>{batchResult.patientsProcessed}</strong>
                </p>
                <p className="text-gray-600">
                  Total amount billed: <strong>${batchResult.totalAmountBilled.toFixed(2)}</strong>
                </p>
                <p className="text-gray-600">
                  Duration: <strong>{batchResult.durationSeconds.toFixed(2)}s</strong>
                </p>
              </div>
              <div className="flex justify-end">
                <Button variant="primary" onClick={() => setShowBatch(false)}>
                  Done
                </Button>
              </div>
            </>
          )}
        </div>
      </Dialog>
    </div>
  )
}

// ── Extracted row component to keep JSX map clean (avoids key-on-fragment issue) ──

type StatementRowProps = {
  statement: Statement
  isExpanded: boolean
  expandedData: Statement | null
  loadingDetail: boolean
  onToggle: () => void
}

function StatementRow({
  statement: s,
  isExpanded,
  expandedData,
  loadingDetail,
  onToggle,
}: StatementRowProps) {
  return (
    <>
      <tr
        className="border-b border-gray-100 hover:bg-gray-50 cursor-pointer"
        onClick={onToggle}
      >
        <td className="px-4 py-3 font-mono text-gray-600">#{s.statementID}</td>
        <td className="px-4 py-3 font-medium text-gray-800">{s.patientName}</td>
        <td className="px-4 py-3 text-gray-500">{s.mrn || '—'}</td>
        <td className="px-4 py-3 text-gray-500 whitespace-nowrap">
          {s.periodStart} → {s.periodEnd}
        </td>
        <td className="px-4 py-3 font-semibold text-gray-800">${s.amountDue.toFixed(2)}</td>
        <td className="px-4 py-3 text-gray-500">
          {new Date(s.generatedDate).toLocaleDateString()}
        </td>
        <td className="px-4 py-3">
          <Badge status={s.status} />
        </td>
        <td className="px-4 py-3 text-blue-600 text-xs font-medium whitespace-nowrap">
          {isExpanded ? '▲ Hide' : '▼ Detail'}
        </td>
      </tr>

      {isExpanded && (
        <tr>
          <td colSpan={8} className="bg-blue-50 px-6 py-4 border-b border-blue-100">
            {loadingDetail ? (
              <div className="flex justify-center py-4">
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600" />
              </div>
            ) : expandedData != null && expandedData.lineItems.length > 0 ? (
              <div>
                <p className="text-xs font-semibold text-blue-700 uppercase tracking-wide mb-3">
                  Statement Line Items
                </p>
                <table className="w-full text-sm border border-gray-200 rounded-lg overflow-hidden">
                  <thead>
                    <tr className="bg-white border-b border-gray-200">
                      {[
                        'Claim ID',
                        'Service Date',
                        'CPT / HCPCS',
                        'Billed',
                        'Insurance Paid',
                        'Adjustment',
                        'Patient Due',
                      ].map((h) => (
                        <th
                          key={h}
                          className="px-3 py-2 text-xs font-semibold text-gray-500 text-left whitespace-nowrap"
                        >
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {expandedData.lineItems.map((line, i) => (
                      <tr key={i} className="border-b border-gray-100 bg-white">
                        <td className="px-3 py-2 font-mono text-gray-600">#{line.claimID}</td>
                        <td className="px-3 py-2 text-gray-600">{line.serviceDate}</td>
                        <td className="px-3 py-2 font-medium">{line.cptHcpcs}</td>
                        <td className="px-3 py-2">${line.billed.toFixed(2)}</td>
                        <td className="px-3 py-2 text-green-700">${line.insurancePaid.toFixed(2)}</td>
                        <td className="px-3 py-2 text-amber-600">${line.adjustment.toFixed(2)}</td>
                        <td className="px-3 py-2 font-semibold text-blue-700">
                          ${line.patientDue.toFixed(2)}
                        </td>
                      </tr>
                    ))}
                    <tr className="bg-gray-50 border-t-2 border-gray-200">
                      <td
                        colSpan={3}
                        className="px-3 py-2 text-right text-xs font-semibold text-gray-500 uppercase"
                      >
                        Totals
                      </td>
                      <td className="px-3 py-2 font-semibold">
                        ${expandedData.lineItems.reduce((a, l) => a + l.billed, 0).toFixed(2)}
                      </td>
                      <td className="px-3 py-2 font-semibold text-green-700">
                        ${expandedData.lineItems.reduce((a, l) => a + l.insurancePaid, 0).toFixed(2)}
                      </td>
                      <td className="px-3 py-2 font-semibold text-amber-600">
                        ${expandedData.lineItems.reduce((a, l) => a + l.adjustment, 0).toFixed(2)}
                      </td>
                      <td className="px-3 py-2 font-bold text-blue-700">
                        ${expandedData.lineItems.reduce((a, l) => a + l.patientDue, 0).toFixed(2)}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-sm text-gray-400 italic">No line items available for this statement.</p>
            )}
          </td>
        </tr>
      )}
    </>
  )
}
