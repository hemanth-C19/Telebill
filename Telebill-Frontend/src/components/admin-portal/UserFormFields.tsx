// Shared user form fields for Create / Edit user dialogs — used by UserManagement with react-hook-form register + errors.

import type { FieldErrors, UseFormRegister } from 'react-hook-form'
import type { UserFormData } from '../../types/admin.types'

export type UserFormFieldsProps = {
  mode: 'create' | 'edit'
  register: UseFormRegister<UserFormData>
  errors: FieldErrors<UserFormData>
  roles: readonly string[]
  fieldClass: string
  selectClass: string
}

export function UserFormFields({
  mode,
  register,
  errors,
  roles,
  fieldClass,
  selectClass,
}: UserFormFieldsProps) {
  const id = (name: string) => `${mode}-${name}`

  return (
    <>
      <div className="flex flex-col gap-1">
        <label htmlFor={id('name')} className="text-sm font-medium text-gray-700">
          Full Name
        </label>
        <input
          id={id('name')}
          type="text"
          placeholder="Enter full name"
          className={fieldClass}
          {...register('name', { required: 'Name is required' })}
        />
        {errors.name != null && (
          <p className="text-sm text-red-600">{errors.name.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor={id('email')} className="text-sm font-medium text-gray-700">
          Email Address
        </label>
        <input
          id={id('email')}
          type="email"
          placeholder="Enter email address"
          className={fieldClass}
          {...register('email', { required: 'Email is required' })}
        />
        {errors.email != null && (
          <p className="text-sm text-red-600">{errors.email.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor={id('phone')} className="text-sm font-medium text-gray-700">
          Phone Number
        </label>
        <input
          id={id('phone')}
          type="text"
          placeholder="555-0000"
          className={fieldClass}
          {...register('phone', { required: 'Phone is required' })}
        />
        {errors.phone != null && (
          <p className="text-sm text-red-600">{errors.phone.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor={id('role')} className="text-sm font-medium text-gray-700">
          Role
        </label>
        <select id={id('role')} className={selectClass} {...register('role', { required: 'Role is required' })}>
          <option value="" disabled>
            Select a role
          </option>
          {roles.map((role) => (
            <option key={role} value={role}>
              {role}
            </option>
          ))}
        </select>
        {errors.role != null && (
          <p className="text-sm text-red-600">{errors.role.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor={id('status')} className="text-sm font-medium text-gray-700">
          Status
        </label>
        <select id={id('status')} className={selectClass} {...register('status')}>
          <option value="Active">Active</option>
          <option value="Inactive">Inactive</option>
        </select>
      </div>
    </>
  )
}
