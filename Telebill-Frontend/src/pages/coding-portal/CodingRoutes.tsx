import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom'
import CodingSidebar from '../../components/coding-portal/CodingSidebar'
import { Loader } from '../../components/shared/ui/Loader'
import Navbar from '../../components/shared/ui/Navbar'
import { useAuth } from '../../hooks/useAuth'

const Worklist = lazy(() => import('./Worklist'))
const EncounterCodingView = lazy(async () => {
  const module = await import('./EncounterCodingView')
  return { default: module.default }
})
const Notifications = lazy(() => import('../shared/Notifications'))

export default function CodingRoutes() {
  const { logout } = useAuth()
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar
        onLogout={() => {
          logout()
          navigate('/sign-in', { replace: true })
        }}
        userName="Coder"
      />

      <div className="fixed top-16 left-0 h-[calc(100vh-4rem)] w-64 z-30 bg-white border-r border-gray-200">
        <CodingSidebar />
      </div>

      <div className="ml-64 pt-16 min-h-screen bg-gray-50 p-6">
        <Suspense fallback={<Loader />}>
          <Routes>
            <Route path="worklist" element={<Worklist />} />
            <Route path="encounter/:encounterId" element={<EncounterCodingView />} />
            <Route path="notifications" element={<Notifications />} />
            <Route index element={<Navigate to="/coding/worklist" replace />} />
          </Routes>
        </Suspense>
      </div>
    </div>
  )
}
