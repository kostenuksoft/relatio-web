import { type FormEvent, useState } from 'react'
import { useGetContactsQuery, useCreateContactMutation } from '../store/api/contactsApi'
import { useGetCustomersQuery } from '../store/api/customersApi'
import { Modal } from '../components/Modal'

const inputCls =
  'w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500'

export function ContactsPage() {
  const [page, setPage] = useState(1)
  const [showModal, setShowModal] = useState(false)
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [position, setPosition] = useState('')
  const [customerId, setCustomerId] = useState('')

  const { data, isLoading, isFetching } = useGetContactsQuery({ page, pageSize: 20 })
  const { data: customers } = useGetCustomersQuery({ pageSize: 100 })
  const [createContact, { isLoading: isCreating, error: createError }] = useCreateContactMutation()

  function resetForm() {
    setFirstName(''); setLastName(''); setEmail(''); setPhone('')
    setPosition(''); setCustomerId('')
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    try {
      await createContact({
        firstName,
        lastName,
        customerId,
        ...(email ? { email } : {}),
        ...(phone ? { phone } : {}),
        ...(position ? { position } : {}),
      }).unwrap()
      setShowModal(false)
      resetForm()
    } catch { /* shown via createError */ }
  }

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Contacts</h1>
          {data && <p className="text-sm text-gray-500 mt-0.5">{data.totalCount} total</p>}
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          + New Contact
        </button>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        {isLoading ? (
          <div className="p-12 text-center text-gray-400">Loading…</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                {['Full Name', 'Email', 'Phone', 'Position', 'Customer ID', 'Created'].map((h) => (
                  <th key={h} className="text-left px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wide">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {data?.items.map((c) => (
                <tr key={c.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-900">{c.fullName}</td>
                  <td className="px-4 py-3 text-gray-600">{c.email ?? '—'}</td>
                  <td className="px-4 py-3 text-gray-500">{c.phone ?? '—'}</td>
                  <td className="px-4 py-3 text-gray-500">{c.position ?? '—'}</td>
                  <td className="px-4 py-3 text-gray-400 font-mono text-xs">{c.customerId.slice(0, 8)}…</td>
                  <td className="px-4 py-3 text-gray-400">{new Date(c.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}
              {data?.items.length === 0 && (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-gray-400">No contacts yet</td></tr>
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
        <Modal title="New Contact" onClose={() => { setShowModal(false); resetForm() }}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">First Name *</label>
                <input value={firstName} onChange={(e) => setFirstName(e.target.value)} required className={inputCls} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Last Name *</label>
                <input value={lastName} onChange={(e) => setLastName(e.target.value)} required className={inputCls} />
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
              <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} className={inputCls} />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
                <input value={phone} onChange={(e) => setPhone(e.target.value)} className={inputCls} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Position</label>
                <input value={position} onChange={(e) => setPosition(e.target.value)} className={inputCls} placeholder="CEO" />
              </div>
            </div>
            {createError && <p className="text-sm text-red-600">Failed to create contact</p>}
            <div className="flex justify-end gap-3 pt-2">
              <button type="button" onClick={() => { setShowModal(false); resetForm() }}
                className="text-sm px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50">
                Cancel
              </button>
              <button type="submit" disabled={isCreating}
                className="text-sm px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-lg">
                {isCreating ? 'Creating…' : 'Create'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  )
}
