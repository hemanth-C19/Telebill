import { useState } from 'react'
import BalancesTab from './Balances/BalancesTab'
import StatementsTab from './Balances/StatementsTab'

type Tab = 'balances' | 'statements'

const TABS: { key: Tab; label: string }[] = [
  { key: 'balances', label: 'Patient Balances' },
  { key: 'statements', label: 'Statements' },
]

export default function BalancesStatements() {
  const [activeTab, setActiveTab] = useState<Tab>('balances')

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Balances &amp; Statements</h1>
        <p className="mt-1 text-sm text-gray-500">
          Manage patient balances, aging buckets, and billing statements.
        </p>
      </div>

      <div className="border-b border-gray-200">
        <nav className="-mb-px flex gap-6">
          {TABS.map((tab) => (
            <button
              key={tab.key}
              type="button"
              onClick={() => setActiveTab(tab.key)}
              className={`pb-3 text-sm font-medium border-b-2 transition-colors ${
                activeTab === tab.key
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </nav>
      </div>

      {activeTab === 'balances' ? <BalancesTab /> : <StatementsTab />}
    </div>
  )
}
