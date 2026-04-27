import { useEffect } from 'react'

export type ToastProps = {
  message: string
  type: 'success' | 'error' | 'info' | 'warning'
  onClose: () => void
  duration?: number
}

const typeStyles: Record<ToastProps['type'], string> = {
  success: 'border border-green-200 bg-green-50 text-green-800',
  error: 'border border-red-200 bg-red-50 text-red-800',
  info: 'border border-blue-200 bg-blue-50 text-blue-800',
  warning: 'border border-yellow-200 bg-yellow-50 text-yellow-800',
}

const dotColors: Record<ToastProps['type'], string> = {
  success: 'bg-green-500',
  error: 'bg-red-500',
  info: 'bg-blue-500',
  warning: 'bg-yellow-500',
}

export function Toast({
  message,
  type,
  onClose,
  duration = 3000,
}: ToastProps) {
  useEffect(() => {
    const id = window.setTimeout(() => {
      onClose()
    }, duration)
    return () => window.clearTimeout(id)
  }, [duration, onClose])

  return (
    <div
      className={`fixed top-4 right-4 z-50 flex min-w-64 max-w-sm items-center gap-3 rounded-lg border px-4 py-3 text-sm font-medium shadow-lg ${typeStyles[type]}`}
      role="status"
    >
      <span className={`h-2 w-2 shrink-0 rounded-full ${dotColors[type]}`} />
      <span className="flex-1">{message}</span>
      <button
        type="button"
        className="shrink-0 text-current opacity-60 hover:opacity-100"
        onClick={onClose}
        aria-label="Dismiss notification"
      >
        ✕
      </button>
    </div>
  )
}
