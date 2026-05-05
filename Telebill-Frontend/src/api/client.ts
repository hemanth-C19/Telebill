import axios from 'axios'
import { clearAuthCookie, getAuthCookie } from '../utils/cookieToken'

const apiClient = axios.create({
  baseURL: 'http://localhost:5183',
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use(
  (config) => {
    const auth = getAuthCookie()
    if (auth != null) {
      config.headers.Authorization = `Bearer ${auth.token}`
    }
    return config
  },
  (error) => Promise.reject(error),
)

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      if (!window.location.pathname.includes('/sign-in')) {
        clearAuthCookie()
        window.location.href = '/sign-in'
      }
    }
    if (error.response?.status === 403) {
      console.warn('Forbidden — insufficient role permissions')
    }
    return Promise.reject(error)
  },
)

export default apiClient
