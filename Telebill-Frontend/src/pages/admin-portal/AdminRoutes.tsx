// Admin portal route definitions — persistent layout with Navbar and AdminSidebar wrapping all admin pages

import React from 'react'
import { Routes, Route, Navigate, useNavigate } from 'react-router-dom'
import AdminSidebar from '../../components/admin-portal/AdminSidebar'
import Navbar from '../../components/shared/ui/Navbar'
import { useAuth } from '../../hooks/useAuth'
import Dashboard from './Dashboard'
import UserManagement from './UserManagement'
import MasterData from './MasterData'
import AuditLogs from './AuditLogs'
import Notifications from '../shared/Notifications'

const PayerPlans = React.lazy(() => import('../admin-portal/PayerPlans'))
const FeeSchedules = React.lazy(() => import('../admin-portal/FeeSchedules'))

export default function AdminRoutes() {
  const { logout } = useAuth()
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar
        onLogout={() => {
          logout()
          navigate('/sign-in', { replace: true })
        }}
        userName="Admin User"
      />

      <div className="flex pt-16">
        <div className="fixed left-0 top-16 z-30 h-[calc(100vh-4rem)] w-64">
          <AdminSidebar />
        </div>

        <div className="ml-64 min-h-[calc(100vh-4rem)] flex-1 overflow-y-auto p-8">
          <Routes>
            <Route path="dashboard" element={<Dashboard />} />
            <Route path="users" element={<UserManagement />} />
            <Route path="master-data" element={<MasterData />} />
            <Route path="master-data/payers/:payerId/plans" element={<PayerPlans />} />
            <Route path="master-data/payers/:payerId/plans/:planId/fees" element={<FeeSchedules />} />
            <Route path="audit" element={<AuditLogs />} />
            <Route path="notifications" element={<Notifications />} />
            <Route path="*" element={<Navigate to="dashboard" replace />} />
          </Routes>
        </div>
      </div>
    </div>
  )
}
