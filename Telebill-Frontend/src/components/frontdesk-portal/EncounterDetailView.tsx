// Encounter detail — info, charge lines, attestation, edit encounter & edit charge dialogs. State/handlers stay in Encounters.tsx.

import type { UseFormReturn } from 'react-hook-form'
import Badge from '../shared/ui/Badge'
import Button from '../shared/ui/Button'
import { Card } from '../shared/ui/Card'
import Dialog from '../shared/ui/Dialog'
import Input from '../shared/ui/Input'
import Table from '../shared/ui/Table'
import type { Column } from '../shared/ui/Table'
import type {
  AddChargeFormValues,
  Attestation,
  ChargeLine,
  EditChargeFormValues,
  EditEncounterFormValues,
  Encounter,
  PosOption,
} from '../../types/frontdesk.types'

export type EncounterDetailViewProps = {
  encounter: Encounter
  encounterDateDisplay: string
  posLabel: string
  onBack: () => void
  onOpenEditEncounter: () => void
  onDeleteEncounter: () => void
  canDeleteEncounter: boolean
  chargeColumns: Column[]
  chargeTableData: Record<string, unknown>[]
  hasDraft: boolean
  showAddChargeForm: boolean
  onToggleAddCharge: () => void
  addChargeForm: UseFormReturn<AddChargeFormValues>
  onAddChargeSubmit: (values: AddChargeFormValues) => void
  onCancelAddCharge: () => void
  attest: Attestation | undefined
  showEditEncounterDialog: boolean
  onCloseEditEncounterDialog: () => void
  editEncounterForm: UseFormReturn<EditEncounterFormValues>
  onEditEncounterSubmit: (values: EditEncounterFormValues) => void
  posOptions: readonly PosOption[]
  selectClassName: string
  textareaClassName: string
  editingCharge: ChargeLine | null
  onCloseEditCharge: () => void
  editChargeForm: UseFormReturn<EditChargeFormValues>
  onEditChargeSubmit: (values: EditChargeFormValues) => void
}

export function EncounterDetailView({
  encounter,
  encounterDateDisplay,
  posLabel,
  onBack,
  onOpenEditEncounter,
  onDeleteEncounter,
  canDeleteEncounter,
  chargeColumns,
  chargeTableData,
  hasDraft,
  showAddChargeForm,
  onToggleAddCharge,
  addChargeForm,
  onAddChargeSubmit,
  onCancelAddCharge,
  attest,
  showEditEncounterDialog,
  onCloseEditEncounterDialog,
  editEncounterForm,
  onEditEncounterSubmit,
  posOptions,
  selectClassName,
  textareaClassName,
  editingCharge,
  onCloseEditCharge,
  editChargeForm,
  onEditChargeSubmit,
}: EncounterDetailViewProps) {
  return (
    <div className="space-y-6">
      <button
        type="button"
        onClick={onBack}
        className="text-sm font-medium text-blue-600 hover:text-blue-800"
      >
        ← Back to Encounters
      </button>

      <Card title="Encounter Information">
        <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">Patient</dt>
            <dd className="text-sm text-gray-900">{encounter.patientName}</dd>
          </div>
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">Provider</dt>
            <dd className="text-sm text-gray-900">{encounter.providerName}</dd>
          </div>
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">Date</dt>
            <dd className="text-sm text-gray-900">{encounterDateDisplay}</dd>
          </div>
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">POS</dt>
            <dd className="text-sm text-gray-900">{posLabel}</dd>
          </div>
          <div className="sm:col-span-2">
            <dt className="text-xs font-semibold uppercase text-gray-500">Notes</dt>
            <dd className="text-sm text-gray-900">
              {encounter.notes.trim() === '' ? '—' : encounter.notes}
            </dd>
          </div>
          <div>
            <dt className="text-xs font-semibold uppercase text-gray-500">Status</dt>
            <dd className="mt-1">
              <Badge status={encounter.status} />
            </dd>
          </div>
        </dl>

        <div className="mt-6 flex flex-wrap gap-2">
          <Button type="button" variant="secondary" size="sm" onClick={onOpenEditEncounter}>
            Edit
          </Button>
          {canDeleteEncounter && (
            <Button type="button" variant="danger" size="sm" onClick={onDeleteEncounter}>
              Delete Encounter
            </Button>
          )}
        </div>
      </Card>

      <Card>
        <div className="mb-4 flex flex-col gap-3 border-b border-gray-200 pb-4 sm:flex-row sm:items-center sm:justify-between">
          <h3 className="text-base font-semibold text-gray-800">Charge Lines</h3>
          <Button type="button" variant="secondary" size="sm" onClick={onToggleAddCharge}>
            Add Charge Line
          </Button>
        </div>

        {hasDraft && (
          <div className="mb-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-900">
            ⚠ All charge lines must be Finalized before the Provider can mark this encounter as
            Ready for Coding.
          </div>
        )}

        {showAddChargeForm && (
          <form
            onSubmit={addChargeForm.handleSubmit(onAddChargeSubmit)}
            className="mb-4 grid grid-cols-1 gap-4 rounded-lg border border-gray-200 bg-gray-50 p-4 sm:grid-cols-2"
            noValidate
          >
            <Input
              label="CPT/HCPCS"
              {...addChargeForm.register('cptCode', { required: 'CPT/HCPCS is required' })}
              error={addChargeForm.formState.errors.cptCode?.message}
            />
            <Input
              label="Units"
              type="number"
              min={1}
              step={1}
              {...addChargeForm.register('units', { required: 'Required' })}
              error={addChargeForm.formState.errors.units?.message}
            />
            <Input
              label="Charge Amount"
              type="number"
              min={0}
              step={0.01}
              {...addChargeForm.register('chargeAmount', { required: 'Required' })}
              error={addChargeForm.formState.errors.chargeAmount?.message}
            />
            <Input label="Modifiers" {...addChargeForm.register('modifiers')} />
            <div className="sm:col-span-2">
              <Input label="Dx Pointers" {...addChargeForm.register('dxPointers')} />
            </div>
            <div className="flex flex-wrap gap-2 sm:col-span-2">
              <Button type="submit" variant="primary" size="sm">
                Add Line
              </Button>
              <Button type="button" variant="secondary" size="sm" onClick={onCancelAddCharge}>
                Cancel
              </Button>
            </div>
          </form>
        )}

        <Table columns={chargeColumns} data={chargeTableData} />
      </Card>

      <div className="flex items-start gap-3 rounded-lg border border-blue-200 bg-blue-50 px-4 py-3">
        <svg
          className="mt-0.5 h-5 w-5 shrink-0 text-blue-600"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          aria-hidden
        >
          <circle cx="12" cy="12" r="10" />
          <path d="M12 16v-4M12 8h.01" />
        </svg>
        <div className="min-w-0 flex-1">
          <p className="text-sm text-gray-800">
            <span className="font-bold text-gray-900">Attestation:</span>{' '}
            {attest?.status === 'Attested' ? (
              <>
                <span className="font-medium text-green-700">✓ Attested</span>
                <span className="text-gray-600">
                  {' '}
                  by {attest.attestedBy}
                  {attest.attestedDate !== '' ? ` on ${attest.attestedDate}` : ''}
                </span>
              </>
            ) : (
              <span className="font-medium text-amber-700">⏳ Not yet attested by provider</span>
            )}
          </p>
          <p className="mt-2 text-xs italic text-gray-500">
            FrontDesk view only — attestation is managed by the Provider.
          </p>
        </div>
      </div>

      <Dialog
        isOpen={showEditEncounterDialog}
        onClose={onCloseEditEncounterDialog}
        title="Edit Encounter"
        maxWidth="md"
      >
        <form
          onSubmit={editEncounterForm.handleSubmit(onEditEncounterSubmit)}
          className="flex flex-col gap-4"
          noValidate
        >
          <Input
            label="Encounter Date"
            type="datetime-local"
            {...editEncounterForm.register('encounterDate', { required: 'Required' })}
            error={editEncounterForm.formState.errors.encounterDate?.message}
          />
          <div className="flex flex-col gap-1">
            <label htmlFor="edit-enc-pos" className="text-sm font-medium text-gray-700">
              POS
            </label>
            <select id="edit-enc-pos" className={selectClassName} {...editEncounterForm.register('pos')}>
              {posOptions.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </div>
          <div className="flex flex-col gap-1">
            <label htmlFor="edit-enc-notes" className="text-sm font-medium text-gray-700">
              Notes
            </label>
            <textarea
              id="edit-enc-notes"
              rows={3}
              className={textareaClassName}
              {...editEncounterForm.register('notes')}
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={onCloseEditEncounterDialog}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              Save
            </Button>
          </div>
        </form>
      </Dialog>

      <Dialog
        isOpen={editingCharge != null}
        onClose={onCloseEditCharge}
        title="Edit Charge Line"
        maxWidth="md"
      >
        <form
          onSubmit={editChargeForm.handleSubmit(onEditChargeSubmit)}
          className="flex flex-col gap-4"
          noValidate
        >
          <Input
            label="CPT/HCPCS"
            {...editChargeForm.register('cptCode', { required: 'Required' })}
            error={editChargeForm.formState.errors.cptCode?.message}
          />
          <Input label="Modifiers" {...editChargeForm.register('modifiers')} />
          <Input
            label="Units"
            type="number"
            min={1}
            step={1}
            {...editChargeForm.register('units', { required: 'Required' })}
            error={editChargeForm.formState.errors.units?.message}
          />
          <Input
            label="Charge Amount"
            type="number"
            min={0}
            step={0.01}
            {...editChargeForm.register('chargeAmount', { required: 'Required' })}
            error={editChargeForm.formState.errors.chargeAmount?.message}
          />
          <Input label="Dx Pointers" {...editChargeForm.register('dxPointers')} />
          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={onCloseEditCharge}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              Save
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  )
}
