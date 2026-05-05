import { useCallback, useEffect, useState, type FormEvent } from 'react'
import apiClient from '../../api/client'
import Table, { type Column } from '../../components/shared/ui/Table'

type ScrubRule = {
  ruleID: number
  name: string
  expressionJSON: string
  severity: string
  status: string
}

type FormValues = {
  name: string
  expressionJSON: string
  severity: string
  status: string
}

const COLUMNS: Column[] = [
  { key: 'ruleID', label: 'ID' },
  { key: 'name', label: 'Name' },
  { key: 'severity', label: 'Severity' },
  { key: 'status', label: 'Status' },
  { key: 'expression', label: 'Expression JSON' },
]

const inputCls =
  'rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

function emptyForm(): FormValues {
  return { name: '', expressionJSON: '', severity: 'Error', status: 'Active' }
}

function truncate(s: string, max = 60): string {
  return s.length > max ? s.slice(0, max) + '…' : s
}

export default function ScrubRules() {
  const [rules, setRules] = useState<ScrubRule[]>([])
  const [loading, setLoading] = useState(false)
  const [pageError, setPageError] = useState<string | null>(null)

  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingRule, setEditingRule] = useState<ScrubRule | null>(null)
  const [form, setForm] = useState<FormValues>(emptyForm())
  const [submitting, setSubmitting] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)

  const fetchRules = useCallback(async () => {
    setLoading(true)
    setPageError(null)
    try {
      const res = await apiClient.get<ScrubRule[]>('api/scrub-rules')
      setRules(res.data)
    } catch {
      setPageError('Failed to load scrub rules.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchRules()
  }, [fetchRules])

  function openCreate() {
    setEditingRule(null)
    setForm(emptyForm())
    setFormError(null)
    setDialogOpen(true)
  }

  function openEdit(row: Record<string, unknown>) {
    const rule = rules.find(r => r.ruleID === row.ruleID)
    if (rule == null) return
    setEditingRule(rule)
    setForm({
      name: rule.name,
      expressionJSON: rule.expressionJSON,
      severity: rule.severity,
      status: rule.status,
    })
    setFormError(null)
    setDialogOpen(true)
  }

  async function handleDelete(row: Record<string, unknown>) {
    setPageError(null)
    try {
      await apiClient.delete(`api/scrub-rules/${row.ruleID}`)
      await fetchRules()
    } catch {
      setPageError('Failed to delete the rule.')
    }
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setFormError(null)

    const trimmedName = form.name.trim()
    if (trimmedName.length < 4) {
      setFormError('Name must be at least 4 characters.')
      return
    }

    const trimmedExpr = form.expressionJSON.trim()
    try {
      JSON.parse(trimmedExpr)
    } catch {
      setFormError('Expression JSON is not valid JSON.')
      return
    }

    setSubmitting(true)
    try {
      if (editingRule != null) {
        await apiClient.patch(`api/scrub-rules/${editingRule.ruleID}`, {
          Name: trimmedName,
          ExpressionJSON: trimmedExpr,
          Severity: form.severity,
          Status: form.status,
        })
      } else {
        await apiClient.post('api/scrub-rules', {
          Name: trimmedName,
          ExpressionJSON: trimmedExpr,
          Severity: form.severity,
        })
      }
      setDialogOpen(false)
      await fetchRules()
    } catch {
      setFormError('Failed to save the rule. Check your inputs and try again.')
    } finally {
      setSubmitting(false)
    }
  }

  const tableData = rules.map(r => ({
    ruleID: r.ruleID,
    name: r.name,
    severity: r.severity,
    status: r.status,
    expression: truncate(r.expressionJSON),
  }))

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Scrub Rules</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage claim validation rules applied during the scrubbing process.
          </p>
        </div>
        <button
          type="button"
          onClick={openCreate}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700"
        >
          + Create Scrub Rule
        </button>
      </div>

      {pageError != null && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{pageError}</p>
      )}

      <div className="rounded-lg border border-gray-200 bg-white shadow-sm">
        <Table
          columns={COLUMNS}
          data={tableData}
          loading={loading}
          showActions={true}
          actions={[
            { label: 'Edit', onClick: openEdit },
            { label: 'Delete', onClick: handleDelete, variant: 'danger' },
          ]}
        />
      </div>

      {dialogOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="w-full max-w-lg rounded-xl bg-white p-6 shadow-xl">
            <h2 className="mb-5 text-lg font-semibold text-gray-900">
              {editingRule != null ? 'Edit Scrub Rule' : 'Create Scrub Rule'}
            </h2>

            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
              <div className="flex flex-col gap-1">
                <label className="text-sm font-medium text-gray-700">Name</label>
                <input
                  type="text"
                  value={form.name}
                  onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                  required
                  className={inputCls}
                  placeholder="e.g. NPI Required"
                />
              </div>

              <div className="flex gap-4">
                <div className="flex flex-1 flex-col gap-1">
                  <label className="text-sm font-medium text-gray-700">Severity</label>
                  <select
                    value={form.severity}
                    onChange={e => setForm(f => ({ ...f, severity: e.target.value }))}
                    className={inputCls}
                  >
                    <option value="Error">Error</option>
                    <option value="Warning">Warning</option>
                  </select>
                </div>

                {editingRule != null && (
                  <div className="flex flex-1 flex-col gap-1">
                    <label className="text-sm font-medium text-gray-700">Status</label>
                    <select
                      value={form.status}
                      onChange={e => setForm(f => ({ ...f, status: e.target.value }))}
                      className={inputCls}
                    >
                      <option value="Active">Active</option>
                      <option value="Inactive">Inactive</option>
                    </select>
                  </div>
                )}
              </div>

              <div className="flex flex-col gap-1">
                <label className="text-sm font-medium text-gray-700">Expression JSON</label>
                <textarea
                  value={form.expressionJSON}
                  onChange={e => setForm(f => ({ ...f, expressionJSON: e.target.value }))}
                  required
                  rows={4}
                  className={`${inputCls} resize-y font-mono`}
                  placeholder='{"type":"not_null","field":"ClaimId"}'
                />
                <p className="text-xs text-gray-400">
                  JSON object describing the rule type and parameters evaluated during scrub.
                </p>
              </div>

              {formError != null && (
                <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-600">{formError}</p>
              )}

              <div className="flex justify-end gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setDialogOpen(false)}
                  className="rounded-md border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={submitting}
                  className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                >
                  {submitting ? 'Saving…' : editingRule != null ? 'Save Changes' : 'Create Rule'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
