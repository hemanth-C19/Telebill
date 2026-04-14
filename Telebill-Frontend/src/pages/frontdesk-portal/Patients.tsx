// FrontDesk patients — list, register, detail with coverage; dummy data aligned with PatientDto / CoverageDto (see Telebill-Backend Controllers/PatientCoverage)

import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import Badge from '../../components/shared/ui/Badge'
import Button from '../../components/shared/ui/Button'
import { Card } from '../../components/shared/ui/Card'
import Dialog from '../../components/shared/ui/Dialog'
import Input from '../../components/shared/ui/Input'
import { Pagination } from '../../components/shared/ui/Pagination'
import Table from '../../components/shared/ui/Table'

// Backend ref: PatientController GET .../Patient/GetAllPatients — PatientDto: Name, DOB, Gender, ContactInfo, Street, Area, City (list UI adds PatientId, MRN, Status)
const DUMMY_PATIENTS = [
  {
    patientId: 1,
    name: 'Alice Johnson',
    dob: '1985-03-14',
    gender: 'Female',
    contactInfo: '555-1001',
    street: '12 Maple St',
    area: 'Downtown',
    city: 'Austin',
    mrn: 'PT-A1B2C3D4',
    status: 'Active',
  },
  {
    patientId: 2,
    name: 'Bob Martinez',
    dob: '1972-07-22',
    gender: 'Male',
    contactInfo: '555-1002',
    street: '45 Oak Ave',
    area: 'Westside',
    city: 'Austin',
    mrn: 'PT-E5F6G7H8',
    status: 'Active',
  },
  {
    patientId: 3,
    name: 'Carol Nguyen',
    dob: '1990-11-05',
    gender: 'Female',
    contactInfo: '555-1003',
    street: '78 Pine Rd',
    area: 'Eastside',
    city: 'Austin',
    mrn: 'PT-I9J0K1L2',
    status: 'Inactive',
  },
  {
    patientId: 4,
    name: 'David Patel',
    dob: '1965-01-30',
    gender: 'Male',
    contactInfo: '555-1004',
    street: '9 Cedar Blvd',
    area: 'Northgate',
    city: 'Austin',
    mrn: 'PT-M3N4O5P6',
    status: 'Active',
  },
  {
    patientId: 5,
    name: 'Emily Rodriguez',
    dob: '1998-06-18',
    gender: 'Female',
    contactInfo: '555-1005',
    street: '33 Elm Court',
    area: 'Southpark',
    city: 'Austin',
    mrn: 'PT-Q7R8S9T0',
    status: 'Active',
  },
  {
    patientId: 6,
    name: 'Frank Williams',
    dob: '1955-09-09',
    gender: 'Male',
    contactInfo: '555-1006',
    street: '201 Birch Way',
    area: 'Lakewood',
    city: 'Austin',
    mrn: 'PT-U1V2W3X4',
    status: 'Inactive',
  },
  {
    patientId: 7,
    name: 'Grace Kim',
    dob: '2001-12-25',
    gender: 'Female',
    contactInfo: '555-1007',
    street: '57 Walnut St',
    area: 'Riverside',
    city: 'Austin',
    mrn: 'PT-Y5Z6A7B8',
    status: 'Active',
  },
]

type Patient = {
  patientId: number
  name: string
  dob: string
  gender: string
  contactInfo: string
  street: string
  area: string
  city: string
  mrn: string
  status: string
}

type Coverage = {
  coverageId: number
  patientId: number
  planId: number
  planName: string
  memberId: string
  groupNumber: string
  effectiveFrom: string
  effectiveTo: string
  status: string
}

// Backend ref: CoverageController GET .../Coverage/GetCoverageById/{patientId} — CoverageDto: PatientID, PlanID, MemberID, GroupNumber, EffectiveFrom, EffectiveTo (UI adds CoverageId, PlanName, Status)
const DUMMY_COVERAGES: Record<number, Coverage[]> = {
  1: [
    {
      coverageId: 101,
      patientId: 1,
      planId: 10,
      planName: 'BlueCross Basic',
      memberId: 'BCB-001',
      groupNumber: 'GRP-100',
      effectiveFrom: '2023-01-01',
      effectiveTo: '2024-12-31',
      status: 'Active',
    },
    {
      coverageId: 102,
      patientId: 1,
      planId: 11,
      planName: 'BlueCross Dental',
      memberId: 'BCD-001',
      groupNumber: 'GRP-101',
      effectiveFrom: '2023-01-01',
      effectiveTo: '2023-12-31',
      status: 'Inactive',
    },
  ],
  2: [
    {
      coverageId: 103,
      patientId: 2,
      planId: 20,
      planName: 'Aetna PPO',
      memberId: 'AET-202',
      groupNumber: 'GRP-200',
      effectiveFrom: '2024-03-01',
      effectiveTo: '2025-02-28',
      status: 'Active',
    },
  ],
  3: [],
  4: [
    {
      coverageId: 104,
      patientId: 4,
      planId: 30,
      planName: 'United Gold',
      memberId: 'UHG-404',
      groupNumber: 'GRP-300',
      effectiveFrom: '2022-06-01',
      effectiveTo: '2024-05-31',
      status: 'Inactive',
    },
  ],
  5: [
    {
      coverageId: 105,
      patientId: 5,
      planId: 10,
      planName: 'BlueCross Basic',
      memberId: 'BCB-505',
      groupNumber: 'GRP-100',
      effectiveFrom: '2024-01-01',
      effectiveTo: '2025-12-31',
      status: 'Active',
    },
  ],
  6: [],
  7: [
    {
      coverageId: 106,
      patientId: 7,
      planId: 40,
      planName: 'Cigna HMO',
      memberId: 'CIG-707',
      groupNumber: 'GRP-400',
      effectiveFrom: '2023-07-01',
      effectiveTo: '2025-06-30',
      status: 'Active',
    },
  ],
}

const PAYER_PLANS = [
  { planId: 10, planName: 'BlueCross Basic' },
  { planId: 11, planName: 'BlueCross Dental' },
  { planId: 20, planName: 'Aetna PPO' },
  { planId: 30, planName: 'United Gold' },
  { planId: 40, planName: 'Cigna HMO' },
  { planId: 50, planName: 'Medicare Part B' },
] as const

const PAGE_SIZE = 5

const selectClassName =
  'w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20'

function cloneCoverages(): Record<number, Coverage[]> {
  const out: Record<number, Coverage[]> = {}
  for (const k of Object.keys(DUMMY_COVERAGES)) {
    const id = Number(k)
    out[id] = DUMMY_COVERAGES[id].map((c) => ({ ...c }))
  }
  return out
}

function generateMrn(): string {
  const hex = crypto.randomUUID().replace(/-/g, '').slice(0, 8).toUpperCase()
  return `PT-${hex}`
}

function nextPatientId(patients: Patient[]): number {
  return patients.reduce((m, p) => Math.max(m, p.patientId), 0) + 1
}

function nextCoverageId(map: Record<number, Coverage[]>): number {
  let max = 0
  for (const list of Object.values(map)) {
    for (const c of list) {
      max = Math.max(max, c.coverageId)
    }
  }
  return max + 1
}

type RegisterFormValues = {
  name: string
  dob: string
  gender: string
  contactInfo: string
  street: string
  area: string
  city: string
}

type CoverageFormValues = {
  planId: string
  memberId: string
  groupNumber: string
  effectiveFrom: string
  effectiveTo: string
}

type PatientEditFormValues = {
  name: string
  contactInfo: string
}

export default function Patients() {
  const [patients, setPatients] = useState<Patient[]>(() => [...DUMMY_PATIENTS])
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null)
  const [coverageMap, setCoverageMap] = useState<Record<number, Coverage[]>>(cloneCoverages)
  const [searchQuery, setSearchQuery] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [showRegisterDialog, setShowRegisterDialog] = useState(false)
  const [showCoverageDialog, setShowCoverageDialog] = useState(false)
  const [eligibilityResults, setEligibilityResults] = useState<Record<number, true>>({})

  const registerForm = useForm<RegisterFormValues>({
    defaultValues: {
      name: '',
      dob: '',
      gender: '',
      contactInfo: '',
      street: '',
      area: '',
      city: '',
    },
  })

  const coverageForm = useForm<CoverageFormValues>({
    defaultValues: {
      planId: '',
      memberId: '',
      groupNumber: '',
      effectiveFrom: '',
      effectiveTo: '',
    },
  })

  const patientEditForm = useForm<PatientEditFormValues>({
    defaultValues: { name: '', contactInfo: '' },
  })
  const { reset: resetPatientEdit } = patientEditForm

  useEffect(() => {
    if (selectedPatient == null) return
    resetPatientEdit({
      name: selectedPatient.name,
      contactInfo: selectedPatient.contactInfo,
    })
  }, [selectedPatient, resetPatientEdit])

  const { reset: resetRegister } = registerForm
  const { reset: resetCoverage } = coverageForm

  useEffect(() => {
    if (showRegisterDialog) {
      resetRegister({
        name: '',
        dob: '',
        gender: '',
        contactInfo: '',
        street: '',
        area: '',
        city: '',
      })
    }
  }, [showRegisterDialog, resetRegister])

  useEffect(() => {
    if (showCoverageDialog) {
      resetCoverage({
        planId: PAYER_PLANS[0] ? String(PAYER_PLANS[0].planId) : '',
        memberId: '',
        groupNumber: '',
        effectiveFrom: '',
        effectiveTo: '',
      })
    }
  }, [showCoverageDialog, resetCoverage])

  const filteredPatients = useMemo(() => {
    const q = searchQuery.trim().toLowerCase()
    if (q === '') return patients
    return patients.filter(
      (p) => p.name.toLowerCase().includes(q) || p.mrn.toLowerCase().includes(q),
    )
  }, [patients, searchQuery])

  const totalPages = Math.max(1, Math.ceil(filteredPatients.length / PAGE_SIZE))

  useEffect(() => {
    setCurrentPage((p) => Math.min(p, totalPages))
  }, [totalPages])

  const paginatedPatients = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE
    return filteredPatients.slice(start, start + PAGE_SIZE)
  }, [filteredPatients, currentPage])

  const coverages =
    selectedPatient != null ? (coverageMap[selectedPatient.patientId] ?? []) : []

  function handleDelete(patientId: number) {
    setPatients((prev) => prev.filter((p) => p.patientId !== patientId))
    setSelectedPatient((cur) => (cur?.patientId === patientId ? null : cur))
    setCoverageMap((prev) => {
      const next = { ...prev }
      delete next[patientId]
      return next
    })
  }

  function onRegisterSubmit(values: RegisterFormValues) {
    const newPatient: Patient = {
      patientId: nextPatientId(patients),
      name: values.name.trim(),
      dob: values.dob,
      gender: values.gender,
      contactInfo: values.contactInfo.trim(),
      street: values.street.trim(),
      area: values.area.trim(),
      city: values.city.trim(),
      mrn: generateMrn(),
      status: 'Active',
    }
    setPatients((prev) => [...prev, newPatient])
    setCoverageMap((prev) => ({ ...prev, [newPatient.patientId]: [] }))
    setShowRegisterDialog(false)
  }

  function onSavePatient(values: PatientEditFormValues) {
    if (selectedPatient == null) return
    setPatients((prev) =>
      prev.map((p) =>
        p.patientId === selectedPatient.patientId
          ? { ...p, name: values.name.trim(), contactInfo: values.contactInfo.trim() }
          : p,
      ),
    )
    setSelectedPatient((cur) =>
      cur != null && cur.patientId === selectedPatient.patientId
        ? {
            ...cur,
            name: values.name.trim(),
            contactInfo: values.contactInfo.trim(),
          }
        : cur,
    )
  }

  function onAddCoverage(values: CoverageFormValues) {
    if (selectedPatient == null) return
    const planId = Number(values.planId)
    const plan = PAYER_PLANS.find((p) => p.planId === planId)
    const planName = plan?.planName ?? 'Unknown Plan'
    const newCov: Coverage = {
      coverageId: nextCoverageId(coverageMap),
      patientId: selectedPatient.patientId,
      planId,
      planName,
      memberId: values.memberId.trim(),
      groupNumber: values.groupNumber.trim(),
      effectiveFrom: values.effectiveFrom,
      effectiveTo: values.effectiveTo,
      status: 'Active',
    }
    setCoverageMap((prev) => ({
      ...prev,
      [selectedPatient.patientId]: [...(prev[selectedPatient.patientId] ?? []), newCov],
    }))
    setShowCoverageDialog(false)
  }

  function handleRemoveCoverage(coverageId: number) {
    if (selectedPatient == null) return
    const pid = selectedPatient.patientId
    setCoverageMap((prev) => ({
      ...prev,
      [pid]: (prev[pid] ?? []).filter((c) => c.coverageId !== coverageId),
    }))
    setEligibilityResults((prev) => {
      const next = { ...prev }
      delete next[coverageId]
      return next
    })
  }

  function handleVerify(coverageId: number) {
    setEligibilityResults((prev) => ({ ...prev, [coverageId]: true }))
  }

  function deleteCurrentPatient() {
    if (selectedPatient == null) return
    handleDelete(selectedPatient.patientId)
  }

  const listColumns = [
    { key: 'mrn', label: 'MRN' },
    { key: 'name', label: 'Name' },
    { key: 'dob', label: 'Date of Birth' },
    { key: 'gender', label: 'Gender' },
    { key: 'contactInfo', label: 'Contact' },
    { key: 'status', label: 'Status' },
  ]

  const listTableData = paginatedPatients.map((p) => ({
    ...p,
    status: <Badge status={p.status} />,
  }))

  const coverageColumns = [
    { key: 'planName', label: 'Plan Name' },
    { key: 'memberId', label: 'Member ID' },
    { key: 'groupNumber', label: 'Group Number' },
    { key: 'effectiveFrom', label: 'Effective From' },
    { key: 'effectiveTo', label: 'Effective To' },
    { key: 'status', label: 'Status' },
  ]

  const coverageTableData = coverages.map((row) => ({
    ...row,
    status: (
      <span className="flex flex-wrap items-center gap-2">
        <Badge status={row.status} />
        {eligibilityResults[row.coverageId] === true && (
          <span className="text-sm font-medium text-green-600">✓ Eligible</span>
        )}
      </span>
    ),
  }))

  if (selectedPatient != null) {
    return (
      <div className="space-y-6">
        <button
          type="button"
          onClick={() => setSelectedPatient(null)}
          className="text-sm font-medium text-blue-600 hover:text-blue-800"
        >
          ← Back to Patients
        </button>

        <Card title="Patient Information">
          <div className="space-y-4">
            <p className="text-sm text-gray-600">
              <span className="font-medium text-gray-800">MRN:</span> {selectedPatient.mrn}
            </p>
            <p className="text-sm text-gray-600">
              <span className="font-medium text-gray-800">Date of Birth:</span>{' '}
              {selectedPatient.dob}
            </p>
            <p className="text-sm text-gray-600">
              <span className="font-medium text-gray-800">Gender:</span> {selectedPatient.gender}
            </p>

            <form
              onSubmit={patientEditForm.handleSubmit(onSavePatient)}
              className="grid gap-4 sm:grid-cols-2"
              noValidate
            >
              <Input
                label="Full Name"
                {...patientEditForm.register('name', { required: 'Full name is required' })}
                error={patientEditForm.formState.errors.name?.message}
              />
              <Input
                label="Contact Info"
                {...patientEditForm.register('contactInfo')}
              />
              <div className="sm:col-span-2">
                <Button type="submit" variant="primary" size="sm">
                  Save Changes
                </Button>
              </div>
            </form>

            <div className="flex justify-end border-t border-gray-100 pt-4">
              <Button type="button" variant="danger" size="sm" onClick={deleteCurrentPatient}>
                Delete Patient
              </Button>
            </div>
          </div>
        </Card>

        <Card>
          <div className="mb-6 flex flex-col gap-3 border-b border-gray-200 pb-4 sm:flex-row sm:items-center sm:justify-between">
            <h3 className="text-base font-semibold text-gray-800">Insurance Coverage</h3>
            <Button
              type="button"
              variant="secondary"
              size="sm"
              onClick={() => setShowCoverageDialog(true)}
            >
              Add Coverage
            </Button>
          </div>
          <Table
            columns={coverageColumns}
            data={coverageTableData}
            showActions
            actions={[
              {
                label: 'Verify Eligibility',
                onClick: (row) => handleVerify(row.coverageId as number),
              },
              {
                label: 'Remove',
                onClick: (row) => handleRemoveCoverage(row.coverageId as number),
                variant: 'danger',
              },
            ]}
          />
        </Card>

        <Dialog
          isOpen={showCoverageDialog}
          onClose={() => setShowCoverageDialog(false)}
          title="Add Coverage"
          maxWidth="md"
        >
          <form
            onSubmit={coverageForm.handleSubmit(onAddCoverage)}
            className="flex flex-col gap-4"
            noValidate
          >
            <div className="flex flex-col gap-1">
              <label htmlFor="coverage-plan" className="text-sm font-medium text-gray-700">
                Payer Plan
              </label>
              <select
                id="coverage-plan"
                className={selectClassName}
                {...coverageForm.register('planId', { required: true })}
              >
                {PAYER_PLANS.map((p) => (
                  <option key={p.planId} value={p.planId}>
                    {p.planName}
                  </option>
                ))}
              </select>
            </div>
            <Input
              label="Member ID"
              {...coverageForm.register('memberId', { required: 'Member ID is required' })}
              error={coverageForm.formState.errors.memberId?.message}
            />
            <Input
              label="Group Number"
              {...coverageForm.register('groupNumber', { required: 'Group number is required' })}
              error={coverageForm.formState.errors.groupNumber?.message}
            />
            <Input
              label="Effective From"
              type="date"
              {...coverageForm.register('effectiveFrom', { required: 'Required' })}
              error={coverageForm.formState.errors.effectiveFrom?.message}
            />
            <Input
              label="Effective To"
              type="date"
              {...coverageForm.register('effectiveTo', { required: 'Required' })}
              error={coverageForm.formState.errors.effectiveTo?.message}
            />
            <div className="flex justify-end gap-2 pt-2">
              <Button type="button" variant="secondary" onClick={() => setShowCoverageDialog(false)}>
                Cancel
              </Button>
              <Button type="submit" variant="primary">
                Add Coverage
              </Button>
            </div>
          </form>
        </Dialog>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Patients</h1>
        <Button type="button" variant="primary" onClick={() => setShowRegisterDialog(true)}>
          Register New Patient
        </Button>
      </div>

      <Input
        label="Search by name or MRN"
        placeholder="Search..."
        value={searchQuery}
        onChange={(e) => {
          setSearchQuery(e.target.value)
          setCurrentPage(1)
        }}
      />

      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          columns={listColumns}
          data={listTableData}
          showActions
          actions={[
            {
              label: 'View / Edit',
              onClick: (row) => {
                const p = patients.find((x) => x.patientId === row.patientId)
                if (p != null) setSelectedPatient(p)
              },
            },
            {
              label: 'Delete',
              onClick: (row) => handleDelete(row.patientId as number),
              variant: 'danger',
            },
          ]}
        />
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setCurrentPage}
        />
      </div>

      <Dialog
        isOpen={showRegisterDialog}
        onClose={() => setShowRegisterDialog(false)}
        title="Register New Patient"
        maxWidth="lg"
      >
        <form
          onSubmit={registerForm.handleSubmit(onRegisterSubmit)}
          className="grid gap-4 sm:grid-cols-2"
          noValidate
        >
          <div className="sm:col-span-2">
            <Input
              label="Full Name"
              {...registerForm.register('name', { required: 'Full name is required' })}
              error={registerForm.formState.errors.name?.message}
            />
          </div>
          <Input
            label="Date of Birth"
            type="date"
            {...registerForm.register('dob', { required: 'Date of birth is required' })}
            error={registerForm.formState.errors.dob?.message}
          />
          <div className="flex flex-col gap-1">
            <label htmlFor="register-gender" className="text-sm font-medium text-gray-700">
              Gender
            </label>
            <select
              id="register-gender"
              className={selectClassName}
              {...registerForm.register('gender', { required: 'Gender is required' })}
            >
              <option value="">Select…</option>
              <option value="Male">Male</option>
              <option value="Female">Female</option>
              <option value="Other">Other</option>
            </select>
            {registerForm.formState.errors.gender != null && (
              <p className="text-sm text-red-600">{registerForm.formState.errors.gender.message}</p>
            )}
          </div>
          <Input label="Contact" {...registerForm.register('contactInfo')} />
          <Input label="Street" {...registerForm.register('street')} />
          <Input label="Area / Neighborhood" {...registerForm.register('area')} />
          <Input label="City" {...registerForm.register('city')} />
          <div className="flex justify-end gap-2 sm:col-span-2">
            <Button type="button" variant="secondary" onClick={() => setShowRegisterDialog(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              Register
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  )
}
