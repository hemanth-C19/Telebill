import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import apiClient from '../api/client'
import {
  clearAuthCookie,
  getAuthCookie,
  setAuthCookie,
  type StoredAuth,
} from '../utils/cookieToken'

export type UserRole = 'Admin' | 'FrontDesk' | 'Provider' | 'Coder' | 'AR'

export type AuthUser = {
  userId: number
  email: string
  name: string
  role: UserRole
}

type AuthContextValue = {
  user: AuthUser | null
  isAuthenticated: boolean
  login: (email: string, password: string, role: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function toAuthUser(stored: StoredAuth): AuthUser {
  return {
    userId: stored.userId,
    email: stored.email,
    name: stored.name,
    role: stored.role as UserRole,
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const stored = getAuthCookie()
    return stored != null ? toAuthUser(stored) : null
  })

  const login = useCallback(async (email: string, password: string, role: string) => {
    const res = await apiClient.post('api/Auth-Module/login', {
      Email: email,
      Password: password,
      Role: role,
    })
    console.log(res.data)
    const data = res.data as {
      token: string
      expiresAt: string
      userId: number
      email: string
      name: string
      role: string
    }
    const stored: StoredAuth = {
      token: data.token,
      userId: data.userId,
      email: data.email,
      name: data.name,
      role: data.role,
    }
    setAuthCookie(stored, new Date(data.expiresAt))
    setUser(toAuthUser(stored))
  }, [])

  const logout = useCallback(() => {
    clearAuthCookie()
    setUser(null)
  }, [])

  const value = useMemo(
    () => ({ user, isAuthenticated: user != null, login, logout }),
    [user, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (ctx == null) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
