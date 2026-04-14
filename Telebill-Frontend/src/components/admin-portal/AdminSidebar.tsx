// Admin portal sidebar — wraps the shared Sidebar component with admin-specific navigation links

import Sidebar from '../shared/ui/Sidebar'

const navItems = [
  { label: 'Dashboard', path: '/admin/dashboard' },
  { label: 'User Management', path: '/admin/users' },
  { label: 'Provider', path: '/admin/provider' },
  { label: 'Master Data', path: '/admin/master-data' },
  { label: 'Audit Logs', path: '/admin/audit' },
  { label: 'Notifications', path: '/admin/notifications' },
]

export default function AdminSidebar() {
  return <Sidebar navItems={navItems} />
}
