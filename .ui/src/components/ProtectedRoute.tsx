import { Navigate, Outlet } from 'react-router-dom'
import { useAppSelector } from '../store/hooks'

export function ProtectedRoute() {
  const token = useAppSelector((state) => state.auth.token)
  return token ? <Outlet /> : <Navigate to="/login" replace />
}
