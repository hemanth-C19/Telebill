import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import Button from '../../components/shared/ui/Button'
import { useAuth, type UserRole } from '../../context/AuthContext'

const ROLES = [
  { id: 'Admin', label: 'Admin' },
  { id: 'FrontDesk', label: 'Front Desk' },
  { id: 'Provider', label: 'Provider' },
  { id: 'Coder', label: 'Coder' },
  { id: 'AR', label: 'AR Analyst' },
] as const

type SignInFormValues = {
  email: string
  selectedRole: string
}

const inputClassName =
  'w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm placeholder:text-gray-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

const ROLE_HOME: Record<UserRole, string> = {
  Admin: '/admin/dashboard',
  FrontDesk: '/frontdesk/dashboard',
  Provider: '/provider/encounters',
  Coder: '/coding/worklist',
  AR: '/ar/dashboard',
}

export default function SignIn() {
  const navigate = useNavigate()
  const { login, user } = useAuth()
  const [loginError, setLoginError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  // Redirect already-authenticated users away from sign-in
  useEffect(() => {
    if (user != null) {
      navigate(ROLE_HOME[user.role], { replace: true })
    }
  }, [user, navigate])

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<SignInFormValues>({
    defaultValues: { email: '', selectedRole: '' },
  })

  const selectedRole = watch('selectedRole')

  async function onSubmit(data: SignInFormValues) {
    setLoginError(null)
    setIsSubmitting(true)
    console.log(data)
    try {
      await login(data.email, data.selectedRole)
      navigate(ROLE_HOME[data.selectedRole as UserRole], { replace: true })
    } catch {
      setLoginError('Invalid credentials. Please check your email and role.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-screen">
      <div className="flex flex-1 flex-col items-center justify-center bg-blue-700 px-16 py-12">
        <div className="w-full max-w-md">
          <h1 className="mb-4 text-5xl font-bold tracking-tight text-white">TeleBill</h1>
          <p className="mb-8 text-lg leading-relaxed text-blue-100">
            A telehealth billing and insurance claim submission portal. Manage patient encounters,
            claims, prior authorizations, and reimbursements — all in one streamlined platform.
          </p>
          <div className="mb-4 flex items-start gap-3">
            <div className="mt-2 h-2 w-2 shrink-0 rounded-full bg-blue-400" />
            <p className="text-sm text-blue-100">End-to-end claim lifecycle management</p>
          </div>
          <div className="mb-4 flex items-start gap-3">
            <div className="mt-2 h-2 w-2 shrink-0 rounded-full bg-blue-400" />
            <p className="text-sm text-blue-100">Role-based access across 5 portals</p>
          </div>
          <div className="mb-4 flex items-start gap-3">
            <div className="mt-2 h-2 w-2 shrink-0 rounded-full bg-blue-400" />
            <p className="text-sm text-blue-100">Prior auth, ERA posting, and AR workflows</p>
          </div>
        </div>
      </div>

      <div className="flex flex-1 flex-col items-center justify-center bg-white px-16 py-12">
        <div className="w-full max-w-md">
          <h2 className="mb-1 text-2xl font-bold text-gray-900">Welcome back</h2>
          <p className="mb-8 text-sm text-gray-500">Sign in to your TeleBill account</p>

          <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5" noValidate>
            <input
              type="hidden"
              {...register('selectedRole', { required: 'Please select a role to continue' })}
            />

            <div className="flex flex-col gap-1">
              <label htmlFor="signin-email" className="text-sm font-medium text-gray-700">
                Email address
              </label>
              <input
                id="signin-email"
                type="email"
                autoComplete="email"
                placeholder="you@example.com"
                className={inputClassName}
                {...register('email', {
                  required: 'Email is required',
                  setValueAs: (v: string) => v.trim(),
                  pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Enter a valid email address' },
                })}
              />
              {errors.email != null && (
                <p className="text-sm text-red-600">{errors.email.message}</p>
              )}
            </div>


            <div className="flex flex-col gap-2">
              <p className="text-sm font-medium text-gray-700">Select your role</p>
              <div className="grid grid-cols-3 gap-2">
                {ROLES.map((role) => (
                  <button
                    key={role.id}
                    type="button"
                    onClick={() =>
                      setValue('selectedRole', role.id, { shouldValidate: true, shouldDirty: true })
                    }
                    className={`w-full cursor-pointer rounded-lg border-2 px-3 py-2.5 text-sm font-medium transition-all ${
                      role.id === selectedRole
                        ? 'border-blue-600 bg-blue-50 text-blue-700'
                        : 'border-gray-200 bg-white text-gray-600 hover:border-blue-300 hover:text-blue-600'
                    }`}
                  >
                    {role.label}
                  </button>
                ))}
              </div>
              {errors.selectedRole != null && (
                <p className="text-sm text-red-600">{errors.selectedRole.message}</p>
              )}
            </div>

            {loginError != null && (
              <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-600">{loginError}</p>
            )}

            <div className="w-full">
              <Button type="submit" variant="primary" className="w-full">
                {isSubmitting ? 'Signing in…' : 'Sign In'}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
