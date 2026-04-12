// FrontDesk portal routes — Navbar + sidebar layout matching Admin portal structure

import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom'
import FrontDeskSidebar from '../../components/frontdesk-portal/FrontDeskSidebar'
import Navbar from '../../components/shared/ui/Navbar'
import { Loader } from '../../components/shared/ui/Loader'
import { useAuth } from '../../hooks/useAuth'

const Dashboard = lazy(() => import('./Dashboard'))
const Patients = lazy(() => import('./Patients'))
const Encounters = lazy(() => import('./Encounters'))
const BatchList = lazy(() => import('./BatchList'))
const BatchDetail = lazy(() => import('./BatchDetail'))
const BalancesStatements = lazy(() => import('./BalancesStatements'))
const Notifications = lazy(() => import('../shared/Notifications'))

export default function FrontDeskRoutes() {
  const { logout } = useAuth()
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar
        onLogout={() => {
          logout()
          navigate('/sign-in', { replace: true })
        }}
        userName="Front Desk"
      />

      <div className="flex pt-16">
        <div className="fixed left-0 top-16 z-30 h-[calc(100vh-4rem)] w-64 border-r border-gray-200 bg-white">
          <FrontDeskSidebar />
        </div>

        <div className="ml-64 min-h-[calc(100vh-4rem)] flex-1 overflow-y-auto bg-gray-50 p-6">
          <Suspense fallback={<Loader />}>
            <Routes>
              <Route path="dashboard" element={<Dashboard />} />
              <Route path="patients" element={<Patients />} />
              <Route path="encounters" element={<Encounters />} />
              <Route path="batches" element={<BatchList />} />
              <Route path="batch-detail" element={<BatchDetail />} />
              <Route path="balances" element={<BalancesStatements />} />
              <Route path="notifications" element={<Notifications />} />
              <Route path="*" element={<Navigate to="dashboard" replace />} />
            </Routes>
          </Suspense>
        </div>
      </div>
    </div>
  )
}
