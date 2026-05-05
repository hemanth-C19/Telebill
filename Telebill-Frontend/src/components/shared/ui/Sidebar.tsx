// Sidebar navigation — shared across all portals. Shows Telebill brand header and accepts nav links as props. Uses NavLink for active state highlighting.

import type { ReactNode } from 'react'
import { NavLink } from 'react-router-dom'

export type SidebarNavItem = {
  label: string
  path: string
  icon?: ReactNode
}

export type SidebarProps = {
  navItems: SidebarNavItem[]
}

export function Sidebar({ navItems }: SidebarProps) {
  return (
    <aside className="flex h-full w-full flex-col border-r border-gray-200 bg-white">
      <nav className="flex-1 overflow-y-auto px-3 py-4">
        {navItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              [
                'mb-1 flex items-center rounded-r-lg border-l-4 py-2.5 pl-2 pr-3 text-sm font-medium transition-colors',
                isActive
                  ? 'border-blue-600 bg-blue-50 text-blue-700'
                  : 'border-transparent text-gray-600 hover:bg-gray-100 hover:text-gray-900',
              ].join(' ')
            }
          >
            {item.icon != null && (
              <span className="mr-3 flex h-5 w-5 shrink-0 items-center justify-center text-current">
                {item.icon}
              </span>
            )}
            {item.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}

export default Sidebar
