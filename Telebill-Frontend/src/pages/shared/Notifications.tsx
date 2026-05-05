import { useCallback, useEffect, useState } from 'react'
import apiClient from '../../api/client'
import { useAuth } from '../../hooks/useAuth'
import { Button } from '../../components/shared/ui/Button'

// ── Types ─────────────────────────────────────────────────────────────────────

type NotificationItem = {
  notificationId: number
  userId: number
  message: string | null
  category: string | null
  status: string | null
  createdDate: string
}

type PagedResult = {
  items: NotificationItem[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

// ── Constants ─────────────────────────────────────────────────────────────────

const STATUSES = ['Unread', 'Read', 'Dismissed']
const CATEGORIES = ['Scrub', 'Submission', 'Ack', 'Remit', 'Denial', 'Statement']
const PAGE_SIZE = 20

const CATEGORY_STYLE: Record<string, string> = {
  Scrub: 'bg-yellow-50 text-yellow-700',
  Submission: 'bg-blue-50 text-blue-700',
  Ack: 'bg-green-50 text-green-700',
  Remit: 'bg-purple-50 text-purple-700',
  Denial: 'bg-red-50 text-red-700',
  Statement: 'bg-gray-100 text-gray-600',
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime()
  const mins = Math.floor(diff / 60000)
  if (mins < 1) return 'just now'
  if (mins < 60) return `${mins}m ago`
  const hrs = Math.floor(mins / 60)
  if (hrs < 24) return `${hrs}h ago`
  const days = Math.floor(hrs / 24)
  if (days < 7) return `${days}d ago`
  return new Date(dateStr).toLocaleDateString()
}

// ── Component ─────────────────────────────────────────────────────────────────

export default function Notifications() {
  const { user } = useAuth()
  const userId = user?.userId ?? 0

  const [items, setItems] = useState<NotificationItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(1)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [unreadCount, setUnreadCount] = useState(0)
  const [error, setError] = useState<string | null>(null)

  const [statusFilter, setStatusFilter] = useState('')
  const [categoryFilter, setCategoryFilter] = useState('')

  const [updatingId, setUpdatingId] = useState<number | null>(null)
  const [markingAll, setMarkingAll] = useState(false)

  const fetchUnread = useCallback(async () => {
    try {
      const res = await apiClient.get<{ unreadCount: number }>(
        `api/v1/notifications/unread-count?userId=${userId}`,
      )
      setUnreadCount(res.data.unreadCount)
    } catch {
      // unread count is non-critical
    }
  }, [userId])

  const load = useCallback(async () => {
    if (!userId) return
    setLoading(true)
    setError(null)
    try {
      const params = new URLSearchParams({
        userId: String(userId),
        page: String(page),
        pageSize: String(PAGE_SIZE),
      })
      if (statusFilter) params.set('status', statusFilter)
      if (categoryFilter) params.set('category', categoryFilter)

      const res = await apiClient.get<PagedResult>(`api/v1/notifications?${params}`)
      setItems(res.data.items)
      setTotalCount(res.data.totalCount)
      setTotalPages(res.data.totalPages)
    } catch {
      setError('Failed to load notifications.')
    } finally {
      setLoading(false)
    }
  }, [userId, page, statusFilter, categoryFilter])

  useEffect(() => {
    load()
    fetchUnread()
  }, [load, fetchUnread])

  async function handleUpdateStatus(item: NotificationItem, newStatus: 'Read' | 'Dismissed') {
    setUpdatingId(item.notificationId)
    try {
      await apiClient.patch(
        `api/v1/notifications/${item.notificationId}/status?userId=${userId}`,
        { newStatus },
      )
      await load()
      await fetchUnread()
    } catch {
      // silently ignore per-item errors
    } finally {
      setUpdatingId(null)
    }
  }

  async function handleMarkAllRead() {
    setMarkingAll(true)
    try {
      await apiClient.patch(`api/v1/notifications/mark-all-read?userId=${userId}`)
      await load()
      await fetchUnread()
    } catch {
      setError('Failed to mark all as read.')
    } finally {
      setMarkingAll(false)
    }
  }

  return (
    <div className="flex flex-col gap-5">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-gray-900">
            Notifications
            {unreadCount > 0 && (
              <span className="ml-2 inline-flex items-center rounded-full bg-blue-600 px-2 py-0.5 text-xs font-medium text-white">
                {unreadCount} unread
              </span>
            )}
          </h1>
          {!loading && (
            <p className="text-sm text-gray-400 mt-0.5">{totalCount} total</p>
          )}
        </div>
        {unreadCount > 0 && (
          <Button
            variant="secondary"
            size="sm"
            onClick={handleMarkAllRead}
            disabled={markingAll}
          >
            {markingAll ? 'Marking…' : 'Mark all read'}
          </Button>
        )}
      </div>

      {error && (
        <p className="rounded-md bg-red-50 px-4 py-3 text-sm text-red-600">{error}</p>
      )}

      {/* Filters */}
      <div className="flex flex-wrap gap-2">
        <select
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setPage(1) }}
          className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">All Statuses</option>
          {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
        <select
          value={categoryFilter}
          onChange={(e) => { setCategoryFilter(e.target.value); setPage(1) }}
          className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">All Categories</option>
          {CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
        </select>
      </div>

      {/* List */}
      <div className="border border-gray-200 rounded-lg bg-white divide-y divide-gray-100">
        {loading && (
          [...Array(5)].map((_, i) => (
            <div key={i} className="px-4 py-4 animate-pulse flex gap-3">
              <div className="h-3 w-16 bg-gray-200 rounded mt-1" />
              <div className="flex-1 space-y-2">
                <div className="h-3 w-3/4 bg-gray-200 rounded" />
                <div className="h-3 w-1/4 bg-gray-100 rounded" />
              </div>
            </div>
          ))
        )}

        {!loading && items.length === 0 && (
          <p className="px-4 py-8 text-sm text-gray-400 text-center">
            No notifications found.
          </p>
        )}

        {!loading && items.map((item) => {
          const isUnread = item.status === 'Unread'
          const isDismissed = item.status === 'Dismissed'
          const busy = updatingId === item.notificationId

          return (
            <div
              key={item.notificationId}
              className={`px-4 py-3.5 flex items-start gap-4 ${isUnread ? 'bg-blue-50/40' : ''}`}
            >
              {/* Category badge */}
              <span
                className={`mt-0.5 shrink-0 rounded px-2 py-0.5 text-xs font-medium ${
                  CATEGORY_STYLE[item.category ?? ''] ?? 'bg-gray-100 text-gray-500'
                }`}
              >
                {item.category ?? '—'}
              </span>

              {/* Message + meta */}
              <div className="flex-1 min-w-0">
                <p
                  className={`text-sm leading-snug ${
                    isDismissed
                      ? 'text-gray-400 line-through'
                      : isUnread
                      ? 'text-gray-900 font-medium'
                      : 'text-gray-700'
                  }`}
                >
                  {item.message ?? '—'}
                </p>
                <p className="text-xs text-gray-400 mt-0.5">{timeAgo(item.createdDate)}</p>
              </div>

              {/* Actions */}
              {!isDismissed && (
                <div className="flex gap-2 shrink-0">
                  {isUnread && (
                    <button
                      disabled={busy}
                      onClick={() => handleUpdateStatus(item, 'Read')}
                      className="text-xs text-blue-600 hover:underline disabled:opacity-40"
                    >
                      {busy ? '…' : 'Mark read'}
                    </button>
                  )}
                  <button
                    disabled={busy}
                    onClick={() => handleUpdateStatus(item, 'Dismissed')}
                    className="text-xs text-gray-400 hover:text-gray-600 hover:underline disabled:opacity-40"
                  >
                    Dismiss
                  </button>
                </div>
              )}
            </div>
          )
        })}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-xs text-gray-400">{totalCount} notifications</p>
          <div className="flex gap-2 items-center">
            <Button
              variant="secondary"
              size="sm"
              disabled={page === 1}
              onClick={() => setPage((p) => p - 1)}
            >
              Prev
            </Button>
            <span className="text-sm text-gray-600 px-2">{page} / {totalPages}</span>
            <Button
              variant="secondary"
              size="sm"
              disabled={page === totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
