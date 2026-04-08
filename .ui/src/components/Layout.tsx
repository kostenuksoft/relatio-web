import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAppDispatch, useAppSelector } from '../store/hooks'
import { clearCredentials } from '../store/slices/authSlice'

function getUserFromToken(token: string | null) {
  if (!token || token === 'dev-bypass') return { username: 'dev', email: '', role: '' }
  try {
    const b64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    const p = JSON.parse(atob(b64))
    return {
      username: p['unique_name'] ?? p['name'] ?? p.sub ?? 'user',
      email: p['email'] ?? '',
      role: p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] ?? p['role'] ?? '',
    }
  } catch {
    return { username: 'user', email: '', role: '' }
  }
}

function getInitials(username: string): string {
  const parts = username.split(/[._\-\s]+/).filter(Boolean)
  if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase()
  return username.slice(0, 2).toUpperCase()
}

function BuildingIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
      <rect x="3" y="3" width="18" height="18" rx="2" /><path d="M9 22V12h6v10" /><path d="M9 7h1" /><path d="M14 7h1" /><path d="M9 11h1" /><path d="M14 11h1" />
    </svg>
  )
}

function UsersIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
      <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" /><circle cx="9" cy="7" r="4" /><path d="M22 21v-2a4 4 0 0 0-3-3.87" /><path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </svg>
  )
}

function TrendingUpIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
      <polyline points="22 7 13.5 15.5 8.5 10.5 2 17" /><polyline points="16 7 22 7 22 13" />
    </svg>
  )
}

function LogOutIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
      <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" /><polyline points="16 17 21 12 16 7" /><line x1="21" y1="12" x2="9" y2="12" />
    </svg>
  )
}

function RelatioMark() {
  return (
    <svg width="22" height="22" viewBox="0 0 28 28" fill="none">
      <rect width="28" height="28" fill="white" />
      <path d="M7 7h7l4.5 7L14 21H7l4.5-7L7 7z" fill="black" fillOpacity="0.85" />
      <path d="M14 7h7l-2.5 7 2.5 7h-7l4.5-7L14 7z" fill="black" fillOpacity="0.3" />
    </svg>
  )
}

const navItems = [
  { to: '/customers', label: 'Customers', Icon: BuildingIcon },
  { to: '/contacts',  label: 'Contacts',  Icon: UsersIcon },
  { to: '/deals',     label: 'Deals',     Icon: TrendingUpIcon },
]

export function Layout() {
  const dispatch = useAppDispatch()
  const navigate = useNavigate()
  const token = useAppSelector((s) => s.auth.token)
  const { username, email, role } = getUserFromToken(token)
  const initials = getInitials(username)

  function handleLogout() {
    dispatch(clearCredentials())
    navigate('/login', { replace: true })
  }

  return (
    <div className="flex h-screen bg-bg">
      <aside className="w-[240px] bg-surface border-r border-white/7 flex flex-col flex-shrink-0">

        {/* Logo */}
        <div className="px-5 h-14 flex items-center gap-3 border-b border-white/7">
          <RelatioMark />
          <div className="flex flex-col leading-none">
            <span className="font-code font-semibold text-text text-[13px] tracking-tight">relatio</span>
            <span className="text-[9px] font-medium text-muted-foreground uppercase tracking-widest mt-0.5">CRM</span>
          </div>
        </div>

        {/* Nav */}
        <div className="flex-1 overflow-y-auto py-4">
          <div className="px-5 mb-2">
            <span className="text-[9px] font-semibold text-muted-foreground uppercase tracking-[0.12em]">Workspace</span>
          </div>
          <nav className="px-2 space-y-px">
            {navItems.map(({ to, label, Icon }) => (
              <NavLink
                key={to}
                to={to}
                className={({ isActive }) =>
                  `flex items-center gap-2.5 px-3 py-2 text-[13px] font-medium transition-colors cursor-pointer ${
                    isActive
                      ? 'bg-white/[0.07] text-text border-l border-white/40 pl-[11px]'
                      : 'text-muted-foreground hover:bg-white/[0.04] hover:text-text border-l border-transparent'
                  }`
                }
              >
                <Icon />
                {label}
              </NavLink>
            ))}
          </nav>
        </div>

        {/* User profile */}
        <div className="border-t border-white/7">
          <div className="px-4 py-3 flex items-center gap-3">
            <div className="w-8 h-8 bg-white/10 border border-white/15 flex items-center justify-center flex-shrink-0">
              <span className="text-[11px] font-semibold text-text font-code">{initials}</span>
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-[12px] font-medium text-text truncate">{username}</p>
              {(email || role) && (
                <p className="text-[10px] text-muted-foreground truncate">{role || email}</p>
              )}
            </div>
          </div>
          <div className="px-2 pb-3">
            <button
              onClick={handleLogout}
              className="w-full flex items-center gap-2.5 px-3 py-2 text-[12px] font-medium text-muted-foreground hover:text-destructive hover:bg-destructive/5 transition-colors cursor-pointer"
            >
              <LogOutIcon />
              Sign out
            </button>
          </div>
        </div>

      </aside>

      <main className="flex-1 overflow-y-auto bg-bg">
        <Outlet />
      </main>
    </div>
  )
}
