import axios from 'axios'

const HARDCODED_TOKEN: string = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWUiOiJoZW1hbnRoQGdtYWlsLmNvbSIsInJvbGUiOiJBZG1pbiIsIm5iZiI6MTc3NjE3Mjg3MCwiZXhwIjoxNzc2MjU5MjcwLCJpYXQiOjE3NzYxNzI4NzB9.BIrebSjaFPAMSk4gzrd-o8FG5qGQEhI0P6eEozxmti5XX-Rz8_L0Cn_4sMulLK2iRxu0CA5ItKSekOnsqDznsQ"

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
