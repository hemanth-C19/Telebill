import { useState } from 'react'
import DenialsTab from './Worklist/DenialsTab'
import UnderpaymentTab from './Worklist/UnderpaymentTab'

type Tab = 'denials' | 'underpayments'

const TABS: { key: Tab; label: string }[] = [
  { key: 'denials', label: 'Denial Worklist' },
  { key: 'underpayments', label: 'Underpayment Worklist' },
]

export default function ArWorklist() {
  const [activeTab, setActiveTab] = useState<Tab>('denials')

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">AR Worklist</h1>
        <p className="mt-1 text-sm text-gray-500">
          Triage open denials and identify underpaid claims.
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

      {activeTab === 'denials' ? <DenialsTab /> : <UnderpaymentTab />}
    </div>
  )
}
