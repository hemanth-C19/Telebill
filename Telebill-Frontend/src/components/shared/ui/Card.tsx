import type { ReactNode } from 'react'

export type CardProps = {
  title?: string
  children: ReactNode
  className?: string
}

export function Card({ title, children, className = '' }: CardProps) {
  return (
    <div
      className={`rounded-xl border border-gray-200 bg-white shadow-sm ${className}`.trim()}
    >
      {title != null && title !== '' && (
        <div className="border-b border-gray-200 px-6 py-4">
          <h3 className="font-semibold text-gray-800">{title}</h3>
        </div>
      )}
      <div className="p-6">{children}</div>
    </div>
  )
}
