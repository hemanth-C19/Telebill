import { useState } from 'react'
import EraTab from './Worklist/EraTab'
import PostingTab from './Worklist/PostingTab'

type Tab = 'era' | 'posting'

const TABS: { key: Tab; label: string }[] = [
  { key: 'era', label: 'ERA / Remit' },
  { key: 'posting', label: 'Payment Posting' },
]

export default function EraPayments() {
  const [activeTab, setActiveTab] = useState<Tab>('era')

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">ERA &amp; Payment Posting</h1>
        <p className="mt-1 text-sm text-gray-500">
          Register remittance advice and post payments against claims.
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

      {activeTab === 'era' ? <EraTab /> : <PostingTab />}
    </div>
  )
}
