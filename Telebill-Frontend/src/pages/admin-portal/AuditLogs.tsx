import { useCallback, useEffect, useState } from 'react'
import apiClient from '../../api/client'
import Table, { type Column } from '../../components/shared/ui/Table'

type AuditLog = {
  auditId: number
  userId: number | null
  action: string
  resource: string | null
  timestamp: string | null
  metadata: string | null
}

type AuditRow = Record<string, string | number | null>

const COLUMNS: Column[] = [
  { key: 'auditId', label: 'ID' },
  { key: 'userId', label: 'User ID' },
  { key: 'action', label: 'Action' },
  { key: 'resource', label: 'Resource' },
  { key: 'timestamp', label: 'Timestamp' },
  { key: 'metadata', label: 'Metadata' },
]

function formatTimestamp(ts: string | null): string {
  if (ts == null) return '—'
  const d = new Date(ts)
  if (isNaN(d.getTime())) return ts
  return d.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false,
  })
}

function toRow(log: AuditLog): AuditRow {
  return {
    auditId: log.auditId,
    userId: log.userId ?? null,
    action: log.action,
    resource: log.resource ?? null,
    timestamp: formatTimestamp(log.timestamp),
    metadata: log.metadata ?? null,
  }
}

export default function AuditLogs() {
  const [rows, setRows] = useState<AuditRow[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const fetchLogs = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await apiClient.get<AuditLog[]>('api/v1/IdentityAccess/Audit')
      setRows(res.data.map(toRow))
    } catch {
      setError('Failed to load audit logs. Please try again.')
    } finally {
      setLoading(false)
    }
  }, [])

  const DeleteLogs = async ()=>{
    setError(null);

    try{
      await apiClient.delete('api/v1/IdentityAccess/Audit/DeleteAllAudits')
    } catch{
      setError('Failed to load audit logs. Please try again.')
    } finally{
      fetchLogs();
    }
  }

  useEffect(() => {
    fetchLogs()
  }, [])

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Audit Logs</h1>
          <p className="mt-1 text-sm text-gray-500">
            System-wide activity log ordered by most recent first. Read-only.
          </p>
        </div>
        <button
          type="button"
          onClick={DeleteLogs}
          disabled={loading}
          className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm transition-colors hover:bg-gray-50 disabled:opacity-50"
        >
          {loading ? 'Deleting...' : 'DeleteAll'}
        </button>
      </div>

      {error != null && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error}</p>
      )}

      <div className="rounded-lg border border-gray-200 bg-white shadow-sm">
        <Table columns={COLUMNS} data={rows} loading={loading} showActions={false} />
      </div>

      {!loading && rows.length > 0 && (
        <p className="text-right text-xs text-gray-400">
          {rows.length} record{rows.length !== 1 ? 's' : ''}
        </p>
      )}
    </div>
  )
}
