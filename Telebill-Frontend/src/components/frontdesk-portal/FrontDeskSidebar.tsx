// FrontDesk sidebar — wraps shared Sidebar with portal-specific links and icons

import Sidebar from '../shared/ui/Sidebar'

const iconClass = 'h-5 w-5'

const DashboardIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <rect x="3" y="4" width="18" height="18" rx="2" />
    <path d="M16 2v4M8 2v4M3 10h18" />
  </svg>
)

const PatientsIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
    <circle cx="12" cy="7" r="4" />
  </svg>
)

const EncountersIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
    <path d="M14 2v6h6M16 13H8M16 17H8M10 9H8" />
  </svg>
)

const BatchesIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M12 2L2 7l10 5 10-5-10-5z" />
    <path d="M2 17l10 5 10-5M2 12l10 5 10-5" />
  </svg>
)

const BalancesIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <line x1="12" y1="1" x2="12" y2="23" />
    <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
  </svg>
)

const NotificationsIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
    <path d="M13.73 21a2 2 0 0 1-3.46 0" />
  </svg>
)

const RejectedIcon = (
  <svg className={iconClass} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
    <circle cx="12" cy="12" r="10" />
    <path d="M15 9l-6 6M9 9l6 6" />
  </svg>
)

const navItems = [
  { label: 'Dashboard', path: '/frontdesk/dashboard', icon: DashboardIcon },
  { label: 'Patients', path: '/frontdesk/patients', icon: PatientsIcon },
  { label: 'Encounters', path: '/frontdesk/encounters', icon: EncountersIcon },
  { label: 'Batch Management', path: '/frontdesk/batches', icon: BatchesIcon },
  { label: 'Rejected Claims', path: '/frontdesk/rejected-claims', icon: RejectedIcon },
  { label: 'Balances & Statements', path: '/frontdesk/balances', icon: BalancesIcon },
  { label: 'Notifications', path: '/frontdesk/notifications', icon: NotificationsIcon },
]

export default function FrontDeskSidebar() {
  return <Sidebar navItems={navItems} />
}
