import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import RequireAuth from './components/shared/RequireAuth'
import AdminRoutes from './pages/admin-portal/AdminRoutes'
import SignIn from './pages/auth/SignIn'
import CodingRoutes from './pages/coding-portal/CodingRoutes'
import FrontDeskRoutes from './pages/frontdesk-portal/FrontDeskRoutes'
import ProviderRoutes from './pages/provider-portal/ProviderRoutes'

function AppRoutes() {
  const { user } = useAuth()

  return (
    <Routes>
      <Route path="/sign-in" element={<SignIn />} />

      <Route element={<RequireAuth allowedRole="Admin" />}>
        <Route path="/admin/*" element={<AdminRoutes />} />
      </Route>

      <Route element={<RequireAuth allowedRole="FrontDesk" />}>
        <Route path="/frontdesk/*" element={<FrontDeskRoutes />} />
      </Route>

      <Route element={<RequireAuth allowedRole="Provider" />}>
        <Route path="/provider/*" element={<ProviderRoutes />} />
      </Route>

      <Route element={<RequireAuth allowedRole="Coder" />}>
        <Route path="/coding/*" element={<CodingRoutes />} />
      </Route>

      <Route
        path="/"
        element={
          <Navigate
            to={
              user?.role === 'Admin' ? '/admin/dashboard'
              : user?.role === 'FrontDesk' ? '/frontdesk/dashboard'
              : user?.role === 'Provider' ? '/provider/encounters'
              : user?.role === 'Coder' ? '/coding/worklist'
              : '/sign-in'
            }
            replace
          />
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  )
}
