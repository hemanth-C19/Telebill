import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import AdminRoutes from './pages/admin-portal/AdminRoutes'
import SignIn from './pages/auth/SignIn'
import FrontDeskRoutes from './pages/frontdesk-portal/FrontDeskRoutes'
import ProviderRoutes from './pages/provider-portal/ProviderRoutes'

function AppRoutes() {
  const { role } = useAuth()

  return (
    <Routes>
      <Route path="/sign-in" element={<SignIn />} />
      <Route
        path="/admin/*"
        element={<AdminRoutes />}
      />
      <Route
        path="/frontdesk/*"
        element={
          <FrontDeskRoutes />
        }
      />
      <Route
        path="/provider/*"
        element={
          <ProviderRoutes />
        }
      />
      <Route
        path="/"
        element={
          <Navigate
            to={
              role === 'Admin'
                ? '/admin/dashboard'
                : role === 'FrontDesk'
                  ? '/frontdesk/dashboard'
                  : role === 'Provider'
                    ? '/provider/dashboard'
                    : '/sign-in'
            }
            replace
          />
        }
      />
    </Routes>
  )
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
