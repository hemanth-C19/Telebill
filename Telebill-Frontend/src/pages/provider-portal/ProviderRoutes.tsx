// Provider portal routes — Navbar + sidebar layout matching Admin / FrontDesk

import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom'
import ProviderSidebar from '../../components/provider-portal/ProviderSidebar'
import Navbar from '../../components/shared/ui/Navbar'
import { Loader } from '../../components/shared/ui/Loader'
import { useAuth } from '../../hooks/useAuth'

const Dashboard = lazy(() => import('./Dashboard.tsx'))
const ProviderDetails = lazy(() => import('./ProviderDetails.tsx'))
const Notifications = lazy(() => import('../shared/Notifications.tsx'))

export default function ProviderRoutes() {
  const { logout } = useAuth()
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar
        onLogout={() => {
          logout()
          navigate('/sign-in', { replace: true })
        }}
        userName="Provider"
      />

      <div className="flex pt-16">
        <div className="fixed left-0 top-16 z-30 h-[calc(100vh-4rem)] w-64 border-r border-gray-200 bg-white">
          <ProviderSidebar />
        </div>

        <div className="ml-64 min-h-[calc(100vh-4rem)] flex-1 overflow-y-auto bg-gray-50 p-6">
          <Suspense fallback={<Loader />}>
            <Routes>
              <Route path="dashboard" element={<Dashboard />} />
              <Route path="encounters" element={<ProviderDetails />} />
              <Route path="notifications" element={<Notifications />} />
              <Route path="*" element={<Navigate to="dashboard" replace />} />
            </Routes>
          </Suspense>
        </div>
      </div>
    </div>
  )
}
