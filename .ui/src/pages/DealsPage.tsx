import { type FormEvent, useState } from 'react'
import { useGetDealsQuery, useCreateDealMutation } from '../store/api/dealsApi'
import { useGetCustomersQuery } from '../store/api/customersApi'
import { Modal } from '../components/Modal'

const STAGE_COLORS: Record<string, string> = {
  Prospect: 'bg-blue-100 text-blue-800',
  Qualified: 'bg-yellow-100 text-yellow-800',
  Proposal: 'bg-purple-100 text-purple-800',
  Negotiation: 'bg-orange-100 text-orange-800',
  Won: 'bg-green-100 text-green-800',
  Lost: 'bg-red-100 text-red-800',
}

const CURRENCIES = ['USD', 'EUR', 'UAH', 'GBP']

const inputCls =
  'w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

function formatAmount(amount: number, currency: string) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount)
}

export function DealsPage() {
  const [page, setPage] = useState(1)
  const [showModal, setShowModal] = useState(false)
  const [title, setTitle] = useState('')
  const [amount, setAmount] = useState('')
  const [currency, setCurrency] = useState('USD')
  const [customerId, setCustomerId] = useState('')
  const [expectedCloseDate, setExpectedCloseDate] = useState('')
  const [notes, setNotes] = useState('')

  const { data, isLoading, isFetching } = useGetDealsQuery({ page, pageSize: 20 })
  const { data: customers } = useGetCustomersQuery({ pageSize: 100 })
  const [createDeal, { isLoading: isCreating, error: createError }] = useCreateDealMutation()

  function resetForm() {
    setTitle(''); setAmount(''); setCurrency('USD')
    setCustomerId(''); setExpectedCloseDate(''); setNotes('')
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    try {
      await createDeal({
        title,
        amount: parseFloat(amount),
        currency,
        customerId,
        ...(expectedCloseDate ? { expectedCloseDate: new Date(expectedCloseDate).toISOString() } : {}),
        ...(notes ? { notes } : {}),
      }).unwrap()
      setShowModal(false)
      resetForm()
    } catch { /* shown via createError */ }
  }

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Deals</h1>
          {data && <p className="text-sm text-gray-500 mt-0.5">{data.totalCount} total</p>}
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          + New Deal
        </button>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        {isLoading ? (
          <div className="p-12 text-center text-gray-400">Loading…</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                {['Title', 'Amount', 'Stage', 'Customer ID', 'Close Date', 'Created'].map((h) => (
                  <th key={h} className="text-left px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {data?.items.map((d) => (
                <tr key={d.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-900">{d.title}</td>
                  <td className="px-4 py-3 text-gray-700 font-mono">{formatAmount(d.amount, d.currency)}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${STAGE_COLORS[d.stage] ?? 'bg-gray-100 text-gray-600'}`}>
                      {d.stage}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-400 font-mono text-xs">{d.customerId.slice(0, 8)}…</td>
                  <td className="px-4 py-3 text-gray-500">
                    {d.expectedCloseDate ? new Date(d.expectedCloseDate).toLocaleDateString() : '—'}
                  </td>
                  <td className="px-4 py-3 text-gray-400">{new Date(d.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}
              {data?.items.length === 0 && (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-gray-400">No deals yet</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between mt-4">
          <button disabled={!data.hasPreviousPage || isFetching} onClick={() => setPage((p) => p - 1)}
            className="text-sm px-3 py-1.5 border border-gray-300 rounded-lg disabled:opacity-40 hover:bg-gray-50">
            ← Previous
          </button>
          <span className="text-sm text-gray-500">Page {data.page} of {data.totalPages}</span>
          <button disabled={!data.hasNextPage || isFetching} onClick={() => setPage((p) => p + 1)}
            className="text-sm px-3 py-1.5 border border-gray-300 rounded-lg disabled:opacity-40 hover:bg-gray-50">
            Next →
          </button>
        </div>
      )}

      {showModal && (
        <Modal title="New Deal" onClose={() => { setShowModal(false); resetForm() }}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Title *</label>
              <input value={title} onChange={(e) => setTitle(e.target.value)} required className={inputCls} placeholder="Enterprise License Q2" />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Amount *</label>
                <input type="number" min="0" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} required className={inputCls} placeholder="10000" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Currency</label>
                <select value={currency} onChange={(e) => setCurrency(e.target.value)} className={inputCls}>
                  {CURRENCIES.map((c) => <option key={c}>{c}</option>)}
                </select>
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Customer *</label>
              <select value={customerId} onChange={(e) => setCustomerId(e.target.value)} required className={inputCls}>
                <option value="">Select customer…</option>
                {customers?.items.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Expected Close Date</label>
              <input type="date" value={expectedCloseDate} onChange={(e) => setExpectedCloseDate(e.target.value)} className={inputCls} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
              <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} className={inputCls} />
            </div>
            {createError && <p className="text-sm text-red-600">Failed to create deal</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={() => { setShowModal(false); resetForm() }}
                className="text-sm px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50">
                Cancel
              </button>
              <button type="submit" disabled={isCreating}
                className="text-sm px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg">
                {isCreating ? 'Creating…' : 'Create Deal'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  )
}
