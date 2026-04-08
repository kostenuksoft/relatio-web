import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAppDispatch } from '../store/hooks'
import { clearCredentials } from '../store/slices/authSlice'

const navItems = [
  { to: '/customers', label: 'Customers', icon: '🏢' },
  { to: '/contacts', label: 'Contacts', icon: '👥' },
  { to: '/deals', label: 'Deals', icon: '💼' },
]

export function Layout() {
  const dispatch = useAppDispatch()
  const navigate = useNavigate()

  function handleLogout() {
    dispatch(clearCredentials())
    navigate('/login', { replace: true })
  }

  return (
    <div className="flex h-screen bg-gray-50">
      <aside className="w-56 bg-gray-900 flex flex-col">
        <div className="px-5 py-5 border-b border-gray-700">
          <span className="text-white font-bold text-lg tracking-tight">Relatio CRM</span>
        </div>
        <nav className="flex-1 px-3 py-4 space-y-1">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-blue-600 text-white'
                    : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                }`
              }
            >
              <span>{item.icon}</span>
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="px-3 py-4 border-t border-gray-700">
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium text-gray-400 hover:bg-gray-800 hover:text-white transition-colors"
          >
            <span>🚪</span>
            Logout
          </button>
        </div>
      </aside>
      <main className="flex-1 overflow-y-auto">
        <Outlet />
      </main>
    </div>
  )
}
