const COOKIE_NAME = 'telebill_auth'

export type StoredAuth = {
  token: string
  userId: number
  email: string
  name: string
  role: string
}

export function setAuthCookie(auth: StoredAuth, expiresAt: Date): void {
  document.cookie = [
    `${COOKIE_NAME}=${encodeURIComponent(JSON.stringify(auth))}`,
    `expires=${expiresAt.toUTCString()}`,
    'path=/',
    'SameSite=Strict',
  ].join(';')
}

export function getAuthCookie(): StoredAuth | null {
  const match = document.cookie.match(new RegExp(`(?:^|;\\s*)${COOKIE_NAME}=([^;]*)`))
  if (match == null) return null
  try {
    return JSON.parse(decodeURIComponent(match[1])) as StoredAuth
  } catch {
    return null
  }
}

export function clearAuthCookie(): void {
  document.cookie = `${COOKIE_NAME}=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/;SameSite=Strict`
}
