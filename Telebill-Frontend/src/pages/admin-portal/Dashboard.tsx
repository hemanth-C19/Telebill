import ArAgingTable from '../../components/admin-portal/ArAgingTable'
import ClaimsSummary from '../../components/admin-portal/ClaimsSummary'
import KpiCards from '../../components/admin-portal/KpiCards'
import RecentRemits from '../../components/admin-portal/RecentRemits'

export default function Dashboard() {
  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-xl font-semibold text-gray-900">Admin Dashboard</h1>
        <p className="text-sm text-gray-400 mt-0.5">
          Billing performance overview and system health
        </p>
      </div>

      <KpiCards />

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <ClaimsSummary />
        <ArAgingTable />
      </div>

      <RecentRemits />
    </div>
  )
}
