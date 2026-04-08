import { type FormEvent, useState } from 'react'
import { useGetDealsQuery, useCreateDealMutation, useDeleteDealMutation } from '../store/api/dealsApi'
import { useGetCustomersQuery } from '../store/api/customersApi'
import { Modal } from '../components/Modal'
import { Field } from '@/components/ui/field'
import { parseApiError } from '@/lib/errors'

const fi = 'w-full bg-transparent text-sm text-text py-1 focus:outline-none placeholder:text-white/20'

const CURRENCIES = ['USD', 'EUR', 'UAH', 'GBP']

function formatAmount(amount: number, currency: string) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount)
}

function StageDot({ stage }: { stage: string }) {
  const map: Record<string, [string, string]> = {
    Prospect:    ['bg-sky-400',     'text-sky-400'],
    Qualified:   ['bg-amber-400',   'text-amber-400'],
    Proposal:    ['bg-violet-400',  'text-violet-400'],
    Negotiation: ['bg-orange-400',  'text-orange-400'],
    Won:         ['bg-emerald-400', 'text-emerald-400'],
    Lost:        ['bg-red-400',     'text-red-400'],
  }
  const colors = map[stage]
  const dot  = colors?.[0] ?? 'bg-zinc-500'
  const text = colors?.[1] ?? 'text-zinc-400'
  return (
    <span className="inline-flex items-center gap-1.5">
      <span className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${dot}`} />
      <span className={`text-xs font-medium ${text}`}>{stage}</span>
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
  const [deleteDeal] = useDeleteDealMutation()

  const { general: createGeneral, fields: createFields } = parseApiError(createError)

  function resetForm() {
    setTitle(''); setAmount(''); setCurrency('USD')
    setCustomerId(''); setExpectedCloseDate(''); setNotes('')
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    try {
      await createDeal({
        title, amount: parseFloat(amount), currency, customerId,
        ...(expectedCloseDate ? { expectedCloseDate: new Date(expectedCloseDate).toISOString() } : {}),
        ...(notes ? { notes } : {}),
      }).unwrap()
      setShowModal(false)
      resetForm()
    } catch { /* shown via createError */ }
  }

  async function handleDelete(id: string, dealTitle: string) {
    if (!window.confirm(`Delete "${dealTitle}"? This cannot be undone.`)) return
    await deleteDeal(id)
  }

  return (
    <div className="p-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="font-code font-bold text-2xl text-text">Deals</h1>
          {data && <p className="text-sm text-muted-foreground mt-0.5">{data.totalCount} total</p>}
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="bg-white hover:bg-white/90 text-black text-sm font-medium px-4 py-2 transition-colors cursor-pointer"
        >
          + New Deal
        </button>
      </div>

      <div className="bg-surface border border-white/7 overflow-hidden">
        {isLoading ? (
          <div className="p-12 text-center text-muted-foreground">Loading…</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-raised">
              <tr>
                {['Title', 'Amount', 'Stage', 'Customer', 'Close Date', 'Created', 'Actions'].map((h) => (
                  <th key={h} className="text-left px-4 py-3 text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {data?.items.map((d) => (
                <tr key={d.id} className="border-t border-white/5 hover:bg-white/[0.03] transition-colors">
                  <td className="px-4 py-3 font-medium text-text">{d.title}</td>
                  <td className="px-4 py-3 font-code font-medium text-accent">
                    {formatAmount(d.amount, d.currency)}
                  </td>
                  <td className="px-4 py-3"><StageDot stage={d.stage} /></td>
                  <td className="px-4 py-3 font-code text-xs text-muted-foreground/60">{d.customerId.slice(0, 8)}…</td>
                  <td className="px-4 py-3 font-code text-xs text-muted-foreground/60">
                    {d.expectedCloseDate ? new Date(d.expectedCloseDate).toLocaleDateString() : '—'}
                  </td>
                  <td className="px-4 py-3 font-code text-xs text-muted-foreground/60">
                    {new Date(d.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => handleDelete(d.id, d.title)}
                      className="text-muted-foreground hover:text-destructive transition-colors cursor-pointer"
                      title="Delete deal"
                    >
                      <TrashIcon />
                    </button>
                  </td>
                </tr>
              ))}
              {data?.items.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">No deals yet</td>
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
        <Modal title="New Deal" onClose={() => { setShowModal(false); resetForm() }}>
          <form onSubmit={handleSubmit} className="space-y-4">
            <Field label="Title *" error={createFields['title']}>
              <input value={title} onChange={(e) => setTitle(e.target.value)} required className={fi} placeholder="Enterprise License Q2" />
            </Field>
            <div className="grid grid-cols-2 gap-3">
              <Field label="Amount *" error={createFields['amount']}>
                <input type="number" min="0" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} required className={fi} placeholder="10000" />
              </Field>
              <Field label="Currency" error={createFields['currency']}>
                <select value={currency} onChange={(e) => setCurrency(e.target.value)} className={fi + ' cursor-pointer'} style={{ backgroundColor: 'transparent' }}>
                  {CURRENCIES.map((c) => <option key={c} style={{ backgroundColor: 'var(--color-raised)' }}>{c}</option>)}
                </select>
              </Field>
            </div>
            <Field label="Customer *" error={createFields['customerid']}>
              <select value={customerId} onChange={(e) => setCustomerId(e.target.value)} required className={fi + ' cursor-pointer'} style={{ backgroundColor: 'transparent' }}>
                <option value="" style={{ backgroundColor: 'var(--color-raised)' }}>Select customer…</option>
                {customers?.items.map((c) => (
                  <option key={c.id} value={c.id} style={{ backgroundColor: 'var(--color-raised)' }}>{c.name}</option>
                ))}
              </select>
            </Field>
            <Field label="Expected close date" error={createFields['expectedclosedate']}>
              <input type="date" value={expectedCloseDate} onChange={(e) => setExpectedCloseDate(e.target.value)} className={fi} />
            </Field>
            <Field label="Notes" error={createFields['notes']}>
              <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} className={fi + ' resize-none'} />
            </Field>
            {createGeneral && <p className="text-xs text-destructive">{createGeneral}</p>}
            <div className="flex justify-end gap-3 pt-1">
              <button type="button" onClick={() => { setShowModal(false); resetForm() }}
                className="text-sm px-4 py-2 border border-white/10 text-muted-foreground hover:text-text hover:bg-white/5 cursor-pointer transition-colors">
                Cancel
              </button>
              <button type="submit" disabled={isCreating}
                className="text-sm px-4 py-2 bg-white hover:bg-white/90 disabled:opacity-40 text-black cursor-pointer transition-colors">
                {isCreating ? 'Creating…' : 'Create Deal'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  )
}
