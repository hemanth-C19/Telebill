import axios from 'axios'

const HARDCODED_TOKEN: string = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWUiOiJoZW1hbnRoQGdtYWlsLmNvbSIsInJvbGUiOiJBZG1pbiIsIm5iZiI6MTc3NjY2NjAwNywiZXhwIjoxNzc2NzUyNDA3LCJpYXQiOjE3NzY2NjYwMDd9.hWdKlIpRB6WO3w0UFxcdvX_UIH6yJ1Z4du14KUogtGDFuzDpHF1hnBWND9ikqp2fqPgJTPbwVSApwU1uX3LZZA"

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
    else{
      console.warn('Error occured ', error);``
    }
    return Promise.reject(error)
  },
)

export default apiClient
