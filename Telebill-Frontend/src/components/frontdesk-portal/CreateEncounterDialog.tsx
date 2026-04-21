import type { UseFormReturn } from 'react-hook-form'
import Button from '../shared/ui/Button'
import Dialog from '../shared/ui/Dialog'
import Input from '../shared/ui/Input'
import type {
  CreateEncounterFormValues,
  Encounter,
  EncounterPatientOption,
  EncounterProviderOption,
  PosOption,
} from '../../types/frontdesk.types'

export type CreateEncounterDialogProps = {
  mode: 'create' | 'edit'
  isOpen: boolean
  onClose: () => void
  form: UseFormReturn<CreateEncounterFormValues>
  onSubmit: (values: CreateEncounterFormValues) => void
  editingEncounter?: Encounter | null
  patientOptions?: EncounterPatientOption[]
  providerOptions?: EncounterProviderOption[]
  posOptions: readonly PosOption[]
  selectClassName: string
  textareaClassName: string
}

export function CreateEncounterDialog({
  mode,
  isOpen,
  onClose,
  form,
  onSubmit,
  editingEncounter,
  patientOptions = [],
  providerOptions = [],
  posOptions,
  selectClassName,
  textareaClassName,
}: CreateEncounterDialogProps) {
  const { register, handleSubmit, formState } = form
  const isEdit = mode === 'edit'

  return (
    <Dialog
      isOpen={isOpen}
      onClose={onClose}
      title={isEdit ? 'Edit Encounter' : 'Create Encounter'}
      maxWidth="lg"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4" noValidate>
        {/* Patient — read-only in edit, dropdown in create */}
        {isEdit ? (
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">Patient</label>
            <p className="rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-700">
              {editingEncounter?.patientName ?? '—'}
            </p>
          </div>
        ) : (
          <div className="flex flex-col gap-1">
            <label htmlFor="enc-patient" className="text-sm font-medium text-gray-700">
              Patient
            </label>
            <select
              id="enc-patient"
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
        )}

        {/* Provider — read-only in edit, dropdown in create */}
        {isEdit ? (
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">Provider</label>
            <p className="rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-700">
              {editingEncounter?.providerName ?? '—'}
            </p>
          </div>
        ) : (
          <div className="flex flex-col gap-1">
            <label htmlFor="enc-provider" className="text-sm font-medium text-gray-700">
              Provider
            </label>
            <select
              id="enc-provider"
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
        )}

        <Input
          label="Encounter Date & Time"
          type="datetime-local"
          {...register('encounterDate', { required: 'Encounter date is required' })}
          error={formState.errors.encounterDate?.message}
        />

        <div className="flex flex-col gap-1">
          <label htmlFor="enc-pos" className="text-sm font-medium text-gray-700">
            POS
          </label>
          <select id="enc-pos" className={selectClassName} {...register('pos')}>
            {posOptions.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="enc-notes" className="text-sm font-medium text-gray-700">
            Notes
          </label>
          <textarea
            id="enc-notes"
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
            {isEdit ? 'Save Changes' : 'Create'}
          </Button>
        </div>
      </form>
    </Dialog>
  )
}
