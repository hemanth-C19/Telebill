import { useEffect, useRef, useState, type ReactNode } from 'react'

export type DropdownMenuItem = {
  label: string
  onClick: () => void
  disabled?: boolean
}

export type DropdownMenuProps = {
  trigger: ReactNode
  items: DropdownMenuItem[]
}

export function DropdownMenu({ trigger, items }: DropdownMenuProps) {
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
        <div className="absolute right-0 z-50 mt-1 w-48 rounded-lg border border-gray-200 bg-white py-1 shadow-lg">
          {items.map((item, index) => (
            <button
              key={index}
              type="button"
              disabled={item.disabled}
              className={`w-full px-4 py-2 text-left text-sm text-gray-700 hover:bg-gray-50 ${
                item.disabled ? 'pointer-events-none cursor-not-allowed opacity-40' : ''
              }`}
              onClick={() => {
                if (item.disabled) return
                item.onClick()
                setIsOpen(false)
              }}
            >
              {item.label}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
