import { Navigate, Outlet } from 'react-router-dom'
import { useAuth, type UserRole } from '../../context/AuthContext'

const ROLE_HOME: Record<UserRole, string> = {
  Admin: '/admin/dashboard',
  FrontDesk: '/frontdesk/dashboard',
  Provider: '/provider/encounters',
  Coder: '/coding/worklist',
  AR: '/sign-in',
}

export default function RequireAuth({ allowedRole }: { allowedRole: UserRole }) {
  const { user } = useAuth()

  if (user == null) {
    return <Navigate to="/sign-in" replace />
  }

  if (user.role !== allowedRole) {
    return <Navigate to={ROLE_HOME[user.role]} replace />
  }

  return <Outlet />
}
