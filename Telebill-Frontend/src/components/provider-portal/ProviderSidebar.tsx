// Provider portal sidebar — wraps shared Sidebar with provider-specific navigation

import Sidebar from '../shared/ui/Sidebar'

const iconClass = 'h-5 w-5'

const HomeIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
    <path d="M9 22V12h6v10" />
  </svg>
)

const CalendarIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <rect x="3" y="4" width="18" height="18" rx="2" />
    <path d="M16 2v4M8 2v4M3 10h18" />
  </svg>
)

const BellIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
    <path d="M13.73 21a2 2 0 0 1-3.46 0" />
  </svg>
)

const navItems = [
  { label: 'Dashboard', path: '/provider/dashboard', icon: HomeIcon },
  { label: 'Provider Details', path: '/provider/encounters', icon: CalendarIcon },
  { label: 'Notifications', path: '/provider/notifications', icon: BellIcon },
]

export default function ProviderSidebar() {
  return <Sidebar navItems={navItems} />
}
