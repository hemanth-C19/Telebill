// Popover-style action card — triggered by any element, opens a small card with action buttons inline. Used in table rows and navbar profile.

import { useEffect, useRef, useState, type ReactNode } from 'react'

export type ActionCardItem = {
  label: string
  onClick: () => void
  variant?: 'default' | 'danger'
}

export type ActionCardProps = {
  trigger: ReactNode
  items: ActionCardItem[]
}

export function ActionCard({ trigger, items }: ActionCardProps) {
  const [isOpen, setIsOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleMouseDown(event: MouseEvent) {
      const el = containerRef.current
      if (el && !el.contains(event.target as Node)) {
        setIsOpen(false)
      }
    }

    document.addEventListener('mousedown', handleMouseDown)
    return () => document.removeEventListener('mousedown', handleMouseDown)
  }, [])

  return (
    <div ref={containerRef} className="relative inline-block">
      <div onClick={() => setIsOpen((open) => !open)}>{trigger}</div>
      {isOpen && (
        <div className="absolute right-0 top-full z-50 mt-1 min-w-36 rounded-lg border border-gray-200 bg-white py-1 shadow-lg">
          {items.map((item, index) => {
            const variant = item.variant ?? 'default'
            const style =
              variant === 'danger'
                ? 'text-red-600 hover:bg-red-50'
                : 'text-gray-700 hover:bg-gray-50'
            return (
              <button
                key={index}
                type="button"
                className={`w-full px-4 py-2 text-left text-sm transition-colors ${style}`}
                onClick={() => {
                  item.onClick()
                  setIsOpen(false)
                }}
              >
                {item.label}
              </button>
            )
          })}
        </div>
      )}
    </div>
  )
}
