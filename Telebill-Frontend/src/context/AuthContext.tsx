import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'

export type UserRole = 'Admin' | 'FrontDesk' | 'Provider' | 'Coder' | 'AR'

type AuthContextValue = {
  role: UserRole | null
  login: (role: UserRole) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [role, setRole] = useState<UserRole | null>(null)

  const login = useCallback((next: UserRole) => {
    setRole(next)
  }, [])

  const logout = useCallback(() => {
    setRole(null)
  }, [])

  const value = useMemo(
    () => ({
      role,
      login,
      logout,
    }),
    [role, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (ctx == null) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return ctx
}
