// Shared axios instance — auto-attaches Bearer token to every request via interceptor. Import this instead of plain axios everywhere in the project.

import axios from 'axios'

// TEMP: Paste your JWT token here for testing. Replace with localStorage or cookie retrieval once auth flow is wired up.
const HARDCODED_TOKEN: string = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWUiOiJoZW1hbnRoQGdtYWlsLmNvbSIsInJvbGUiOiJBZG1pbiIsIm5iZiI6MTc3NjA1Mzc3OCwiZXhwIjoxNzc2MTQwMTc4LCJpYXQiOjE3NzYwNTM3Nzh9.5LcCfOcL26EVSF5WCcFHDKX5N2dwe5QorSGl04cGSGrU-iyKikTRzB_Eu1dbVBfEQxkZdAvZT_UcS3zBJk0Zjg"

const apiClient = axios.create({
  baseURL: 'http://localhost:5183',
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use(
  (config) => {
    if (HARDCODED_TOKEN !== "") {
      config.headers.Authorization = `Bearer ${HARDCODED_TOKEN}`
    }
    return config
  },
  (error) => Promise.reject(error),
)

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      console.warn('Unauthorized — token may be expired or invalid')
    }
    if (error.response?.status === 403) {
      console.warn('Forbidden — insufficient role permissions')
    }
    return Promise.reject(error)
  },
)

export default apiClient
