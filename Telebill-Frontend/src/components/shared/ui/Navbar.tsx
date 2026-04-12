// Top navbar — shared across all portals. Shows notification bell and user profile on the right. Clicking profile opens an ActionCard with a logout option.

import { ActionCard } from './ActionCard'

export type NavbarProps = {
  onLogout: () => void
  userName?: string
}

export function Navbar({ onLogout, userName = 'User' }: NavbarProps) {
  const initial = userName.trim().charAt(0).toUpperCase() || 'U'

  return (
    <nav className="fixed left-0 right-0 top-0 z-40 h-16 border-b border-gray-200 bg-white shadow-sm">
      <div className="flex h-full items-center justify-end gap-4 px-6">
        <button
          type="button"
          className="relative rounded-full p-2 text-gray-500 transition-colors hover:bg-gray-100"
          aria-label="Notifications"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="22"
            height="22"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
            <path d="M13.73 21a2 2 0 0 1-3.46 0" />
          </svg>
        </button>

        <ActionCard
          trigger={
            <div className="flex h-9 w-9 cursor-pointer items-center justify-center rounded-full bg-blue-600 text-sm font-semibold text-white transition-colors hover:bg-blue-700">
              {initial}
            </div>
          }
          items={[
            {
              label: 'Logout',
              onClick: onLogout,
              variant: 'danger',
            },
          ]}
        />
      </div>
    </nav>
  )
}

export default Navbar
