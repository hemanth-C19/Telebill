// Generic reusable table — accepts columns, data, loading state, pagination info, and optional action card per row with configurable action buttons.

import { isValidElement, type ReactNode } from 'react'
import { ActionCard } from './ActionCard'

export interface Column {
  key: string
  label: string
}

export interface RowAction {
  label: string
  onClick: (row: Record<string, any>) => void
  variant?: 'default' | 'danger'
}

export type TableProps = {
  columns: Column[]
  data: Record<string, any>[]
  loading?: boolean
  showActions?: boolean
  actions?: RowAction[]
}

export function Table({
  columns,
  data,
  loading = false,
  showActions = false,
  actions = [],
}: TableProps) {
  const colSpan = columns.length + (showActions ? 1 : 0)
  const skeletonCols = columns.length + (showActions ? 1 : 0)

  return (
    <div className="w-full overflow-x-auto min-h-50">
      <table className="w-full border-collapse text-left text-sm">
        <thead>
          <tr className="border-b border-gray-200 bg-gray-50">
            {columns.map((col) => (
              <th
                key={col.key}
                className="whitespace-nowrap px-4 py-3 text-xs font-semibold uppercase tracking-wider text-gray-500"
              >
                {col.label}
              </th>
            ))}
            {showActions && (
              <th className="w-12 whitespace-nowrap px-4 py-3 text-xs font-semibold uppercase tracking-wider text-gray-500" />
            )}
          </tr>
        </thead>
        <tbody>
          {loading &&
            Array.from({ length: 5 }, (_, rowIdx) => (
              <tr key={`skeleton-${rowIdx}`} className="animate-pulse">
                {Array.from({ length: skeletonCols }, (_, colIdx) => (
                  <td key={colIdx} className="px-4 py-3">
                    <div className="h-4 w-full rounded bg-gray-200" />
                  </td>
                ))}
              </tr>
            ))}

          {!loading && data.length === 0 && (
            <tr>
              <td
                colSpan={colSpan}
                className="px-4 py-8 text-center text-sm text-gray-400"
              >
                No records found
              </td>
            </tr>
          )}

          {!loading &&
            data.length > 0 &&
            data.map((row, rowIdx) => (
              <tr
                key={
                  row.chargeId != null
                    ? String(row.chargeId)
                    : row.encounterId != null
                      ? String(row.encounterId)
                      : row.userId != null
                        ? String(row.userId)
                        : row.patientId != null
                          ? String(row.patientId)
                          : rowIdx
                }
                className="border-b border-gray-100 transition-colors hover:bg-gray-50"
              >
                {columns.map((col) => (
                  <td
                    key={col.key}
                    className="whitespace-nowrap px-4 py-3 text-gray-700"
                  >
                    {renderCell(row[col.key])}
                  </td>
                ))}
                {showActions && (
                  <td className="whitespace-nowrap px-4 py-3 text-right">
                    <ActionCard
                      trigger={
                        <button
                          type="button"
                          className="rounded p-1 text-lg font-bold leading-none text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600"
                          aria-label="Row actions"
                        >
                          •••
                        </button>
                      }
                      items={actions.map((action) => ({
                        label: action.label,
                        onClick: () => action.onClick(row),
                        variant: action.variant,
                      }))}
                    />
                  </td>
                )}
              </tr>
            ))}
        </tbody>
      </table>
    </div>
  )
}

function renderCell(value: unknown): ReactNode {
  if (value == null) return '—'
  if (isValidElement(value)) return value
  if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
    return String(value)
  }
  return '—'
}

export default Table
