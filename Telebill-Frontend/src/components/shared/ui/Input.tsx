import { forwardRef, type InputHTMLAttributes } from 'react'

export type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  label?: string
  error?: string
}

const inputClass =
  'w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm placeholder:text-gray-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 disabled:bg-gray-50 disabled:text-gray-500'

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { label, error, className = '', id, ...rest },
  ref,
) {
  const inputId =
    id ??
    (label != null && label !== ''
      ? `input-${label.replace(/\s+/g, '-').toLowerCase()}`
      : undefined)

  return (
    <div className="flex flex-col gap-1">
      {label != null && label !== '' && (
        <label htmlFor={inputId} className="text-sm font-medium text-gray-700">
          {label}
        </label>
      )}
      <input
        ref={ref}
        id={inputId}
        className={`${inputClass} ${error != null && error !== '' ? 'border-red-500' : ''} ${className}`.trim()}
        aria-invalid={error != null && error !== ''}
        {...rest}
      />
      {error != null && error !== '' && (
        <p className="text-sm text-red-600" role="alert">
          {error}
        </p>
      )}
    </div>
  )
})

export default Input
