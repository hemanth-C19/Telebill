// Create Encounter dialog — form UI only; Encounters.tsx owns useForm and submit handlers.

import type { UseFormReturn } from 'react-hook-form'
import Button from '../shared/ui/Button'
import Dialog from '../shared/ui/Dialog'
import Input from '../shared/ui/Input'
import type {
  CreateEncounterFormValues,
  EncounterPatientOption,
  EncounterProviderOption,
  PosOption,
} from '../../types/frontdesk.types'

export type CreateEncounterDialogProps = {
  isOpen: boolean
  onClose: () => void
  form: UseFormReturn<CreateEncounterFormValues>
  onSubmit: (values: CreateEncounterFormValues) => void
  patientSearch: string
  onPatientSearchChange: (value: string) => void
  patientOptions: EncounterPatientOption[]
  providerOptions: EncounterProviderOption[]
  posOptions: readonly PosOption[]
  selectClassName: string
  textareaClassName: string
}

export function CreateEncounterDialog({
  isOpen,
  onClose,
  form,
  onSubmit,
  patientSearch,
  onPatientSearchChange,
  patientOptions,
  providerOptions,
  posOptions,
  selectClassName,
  textareaClassName,
}: CreateEncounterDialogProps) {
  const { register, handleSubmit, formState } = form

  return (
    <Dialog isOpen={isOpen} onClose={onClose} title="Create Encounter" maxWidth="lg">
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4" noValidate>
        <Input
          label="Search patient (name or MRN)"
          placeholder="Filter list…"
          value={patientSearch}
          onChange={(e) => onPatientSearchChange(e.target.value)}
        />
        <div className="flex flex-col gap-1">
          <label htmlFor="create-patient" className="text-sm font-medium text-gray-700">
            Patient
          </label>
          <select
            id="create-patient"
            className={selectClassName}
            {...register('patientId', { required: 'Select a patient' })}
          >
            <option value="">Select patient…</option>
            {patientOptions.map((p) => (
              <option key={p.patientId} value={p.patientId}>
                {p.mrn} — {p.name}
              </option>
            ))}
          </select>
          {formState.errors.patientId != null && (
            <p className="text-sm text-red-600">{formState.errors.patientId.message}</p>
          )}
        </div>
        <div className="flex flex-col gap-1">
          <label htmlFor="create-provider" className="text-sm font-medium text-gray-700">
            Provider
          </label>
          <select
            id="create-provider"
            className={selectClassName}
            {...register('providerId', { required: 'Select a provider' })}
          >
            <option value="">Select provider…</option>
            {providerOptions.map((p) => (
              <option key={p.providerId} value={p.providerId}>
                {p.name} ({p.specialty})
              </option>
            ))}
          </select>
          {formState.errors.providerId != null && (
            <p className="text-sm text-red-600">{formState.errors.providerId.message}</p>
          )}
        </div>
        <Input
          label="Encounter Date & Time"
          type="datetime-local"
          {...register('encounterDate', { required: 'Encounter date is required' })}
          error={formState.errors.encounterDate?.message}
        />
        <div className="flex flex-col gap-1">
          <label htmlFor="create-pos" className="text-sm font-medium text-gray-700">
            POS
          </label>
          <select id="create-pos" className={selectClassName} {...register('pos')}>
            {posOptions.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </div>
        <div className="flex flex-col gap-1">
          <label htmlFor="create-notes" className="text-sm font-medium text-gray-700">
            Notes
          </label>
          <textarea
            id="create-notes"
            rows={3}
            className={textareaClassName}
            {...register('notes')}
          />
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" variant="primary">
            Create
          </Button>
        </div>
      </form>
    </Dialog>
  )
}
