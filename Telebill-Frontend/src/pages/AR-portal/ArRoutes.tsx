import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom'
import ArSidebar from '../../components/Ar-portal/ArSidebar'
import Navbar from '../../components/shared/ui/Navbar'
import { Loader } from '../../components/shared/ui/Loader'
import { useAuth } from '../../hooks/useAuth'

const ArDashboard = lazy(() => import('./ArDashboard'))
const ArWorklist = lazy(() => import('./ArWorklist'))
const DenialDetail = lazy(() => import('./DenialDetail'))
const EraPayments = lazy(() => import('./EraPayments'))
const ArBalances = lazy(() => import('./ArBalances'))
const Notifications = lazy(() => import('../shared/Notifications'))

export default function ArRoutes() {
  const { logout, user } = useAuth()
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar
        onLogout={() => {
          logout()
          navigate('/sign-in', { replace: true })
        }}
        userName={user?.name ?? 'AR Specialist'}
      />

      <div className="flex pt-16">
        <div className="fixed left-0 top-16 z-30 h-[calc(100vh-4rem)] w-64 border-r border-gray-200 bg-white">
          <ArSidebar />
        </div>

        <div className="ml-64 min-h-[calc(100vh-4rem)] flex-1 overflow-y-auto bg-gray-50 p-6">
          <Suspense fallback={<Loader />}>
            <Routes>
              <Route path="dashboard" element={<ArDashboard />} />
              <Route path="worklist" element={<ArWorklist />} />
              <Route path="denials/:denialId" element={<DenialDetail />} />
              <Route path="era-payments" element={<EraPayments />} />
              <Route path="balances" element={<ArBalances />} />
              <Route path="notifications" element={<Notifications />} />
              <Route path="*" element={<Navigate to="dashboard" replace />} />
            </Routes>
          </Suspense>
        </div>
      </div>
    </div>
  )
}
