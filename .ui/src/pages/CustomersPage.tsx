import { type FormEvent, useState } from 'react'
import { useGetCustomersQuery, useCreateCustomerMutation, useDeleteCustomerMutation } from '../store/api/customersApi'
import { Modal } from '../components/Modal'
import { Field } from '@/components/ui/field'
import { parseApiError } from '@/lib/errors'

const fi = 'w-full bg-transparent text-sm text-text py-1 focus:outline-none placeholder:text-white/20'

function StatusDot({ status }: { status: string }) {
  const map: Record<string, [string, string]> = {
    Prospect: ['bg-amber-400',   'text-amber-400'],
    Active:   ['bg-emerald-400', 'text-emerald-300'],
    Inactive: ['bg-zinc-500',    'text-zinc-400'],
  }
  const colors = map[status]
  const dot  = colors?.[0] ?? 'bg-zinc-500'
  const text = colors?.[1] ?? 'text-zinc-400'
  return (
    <span className="inline-flex items-center gap-1.5">
      <span className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${dot}`} />
      <span className={`text-xs font-medium ${text}`}>{status}</span>
    </span>
  )
}

function TrashIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
      <polyline points="3 6 5 6 21 6" />
      <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
      <path d="M10 11v6" /><path d="M14 11v6" />
      <path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
    </svg>
  )
}

export function CustomersPage() {
  const [page, setPage] = useState(1)
  const [showModal, setShowModal] = useState(false)
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [industry, setIndustry] = useState('')

  const { data, isLoading, isFetching } = useGetCustomersQuery({ page, pageSize: 20 })
  const [createCustomer, { isLoading: isCreating, error: createError }] = useCreateCustomerMutation()
  const [deleteCustomer] = useDeleteCustomerMutation()

  const { general: createGeneral, fields: createFields } = parseApiError(createError)

  function resetForm() {
    setName(''); setEmail(''); setPhone(''); setIndustry('')
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    try {
      await createCustomer({
        name, email,
        ...(phone ? { phone } : {}),
        ...(industry ? { industry } : {}),
      }).unwrap()
      setShowModal(false)
      resetForm()
    } catch { /* shown via createError */ }
  }

  async function handleDelete(id: string, customerName: string) {
    if (!window.confirm(`Delete "${customerName}"? This cannot be undone.`)) return
    await deleteCustomer(id)
  }

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="font-code font-bold text-2xl text-text">Customers</h1>
          {data && <p className="text-sm text-muted-foreground mt-0.5">{data.totalCount} total</p>}
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="bg-white hover:bg-white/90 text-black text-sm font-medium px-4 py-2 transition-colors cursor-pointer"
        >
          + New Customer
        </button>
      </div>

      <div className="bg-surface border border-white/7 overflow-hidden">
        {isLoading ? (
          <div className="p-12 text-center text-muted-foreground">Loading…</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-raised">
              <tr>
                {['Name', 'Email', 'Phone', 'Industry', 'Status', 'Created', 'Actions'].map((h) => (
                  <th key={h} className="text-left px-4 py-3 text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {data?.items.map((c) => (
                <tr key={c.id} className="border-t border-white/5 hover:bg-white/[0.03] transition-colors">
                  <td className="px-4 py-3 font-medium text-text">{c.name}</td>
                  <td className="px-4 py-3 text-muted-foreground">{c.email}</td>
                  <td className="px-4 py-3 text-muted-foreground">{c.phone ?? '—'}</td>
                  <td className="px-4 py-3 text-muted-foreground">{c.industry ?? '—'}</td>
                  <td className="px-4 py-3"><StatusDot status={c.status} /></td>
                  <td className="px-4 py-3 font-code text-xs text-muted-foreground/60">
                    {new Date(c.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => handleDelete(c.id, c.name)}
                      className="text-muted-foreground hover:text-destructive transition-colors cursor-pointer"
                      title="Delete customer"
                    >
                      <TrashIcon />
                    </button>
                  </td>
                </tr>
              ))}
              {data?.items.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">No customers yet</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between mt-4">
          <button disabled={!data.hasPreviousPage || isFetching} onClick={() => setPage((p) => p - 1)}
            className="text-sm px-3 py-1.5 border border-white/10 text-muted-foreground hover:bg-white/5 hover:text-text disabled:opacity-30 cursor-pointer transition-colors">
            ← Previous
          </button>
          <span className="text-sm text-muted-foreground">Page {data.page} of {data.totalPages}</span>
          <button disabled={!data.hasNextPage || isFetching} onClick={() => setPage((p) => p + 1)}
            className="text-sm px-3 py-1.5 border border-white/10 text-muted-foreground hover:bg-white/5 hover:text-text disabled:opacity-30 cursor-pointer transition-colors">
            Next →
          </button>
        </div>
      )}

      {showModal && (
        <Modal title="New Customer" onClose={() => { setShowModal(false); resetForm() }}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <Field label="Name *" error={createFields['name']}>
              <input value={name} onChange={(e) => setName(e.target.value)} required className={fi} placeholder="Acme Corp" />
            </Field>
            <Field label="Email *" error={createFields['email']}>
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required className={fi} placeholder="contact@acme.com" />
            </Field>
            <div className="grid grid-cols-2 gap-3">
              <Field label="Phone" error={createFields['phone'] ?? createFields['phonenumber']}>
                <input value={phone} onChange={(e) => setPhone(e.target.value)} className={fi} placeholder="+380..." />
              </Field>
              <Field label="Industry" error={createFields['industry']}>
                <input value={industry} onChange={(e) => setIndustry(e.target.value)} className={fi} placeholder="Technology" />
              </Field>
            </div>
            {createGeneral && <p className="text-xs text-destructive">{createGeneral}</p>}
            <div className="flex justify-end gap-3 pt-1">
              <button type="button" onClick={() => { setShowModal(false); resetForm() }}
                className="text-sm px-4 py-2 border border-white/10 text-muted-foreground hover:text-text hover:bg-white/5 cursor-pointer transition-colors">
                Cancel
              </button>
              <button type="submit" disabled={isCreating}
                className="text-sm px-4 py-2 bg-white hover:bg-white/90 disabled:opacity-40 text-black cursor-pointer transition-colors">
                {isCreating ? 'Creating…' : 'Create'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  )
}
