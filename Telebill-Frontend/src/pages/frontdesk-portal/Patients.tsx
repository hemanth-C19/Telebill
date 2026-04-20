// Patients.tsx — FrontDesk patient list, register, edit
// Backend ref: PatientController GET .../Patient/GetAllPatients

import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import Badge from "../../components/shared/ui/Badge";
import Button from "../../components/shared/ui/Button";
import Dialog from "../../components/shared/ui/Dialog";
import { Pagination } from "../../components/shared/ui/Pagination";
import Table from "../../components/shared/ui/Table";
import type { Patient, PatientFormValues } from "../../types/frontdesk.types";
import { PatientFormFields } from "../../components/frontdesk-portal/PatientFormFields";


// ── Dummy data ────────────────────────────────────────────────────────
const DUMMY_PATIENTS = [
  {
    patientId: 1,
    name: "Alice Johnson",
    dob: "1985-03-14",
    gender: "Female",
    contactInfo: "555-1001",
    street: "12 Maple St",
    area: "Downtown",
    city: "Austin",
    mrn: "PT-A1B2C3D4",
    status: "Active",
  },
  {
    patientId: 2,
    name: "Bob Martinez",
    dob: "1972-07-22",
    gender: "Male",
    contactInfo: "555-1002",
    street: "45 Oak Ave",
    area: "Westside",
    city: "Austin",
    mrn: "PT-E5F6G7H8",
    status: "Active",
  },
  {
    patientId: 3,
    name: "Carol Nguyen",
    dob: "1990-11-05",
    gender: "Female",
    contactInfo: "555-1003",
    street: "78 Pine Rd",
    area: "Eastside",
    city: "Austin",
    mrn: "PT-I9J0K1L2",
    status: "Inactive",
  },
  {
    patientId: 4,
    name: "David Patel",
    dob: "1965-01-30",
    gender: "Male",
    contactInfo: "555-1004",
    street: "9 Cedar Blvd",
    area: "Northgate",
    city: "Austin",
    mrn: "PT-M3N4O5P6",
    status: "Active",
  },
  {
    patientId: 5,
    name: "Emily Rodriguez",
    dob: "1998-06-18",
    gender: "Female",
    contactInfo: "555-1005",
    street: "33 Elm Court",
    area: "Southpark",
    city: "Austin",
    mrn: "PT-Q7R8S9T0",
    status: "Active",
  },
  {
    patientId: 6,
    name: "Frank Williams",
    dob: "1955-09-09",
    gender: "Male",
    contactInfo: "555-1006",
    street: "201 Birch Way",
    area: "Lakewood",
    city: "Austin",
    mrn: "PT-U1V2W3X4",
    status: "Inactive",
  },
  {
    patientId: 7,
    name: "Grace Kim",
    dob: "2001-12-25",
    gender: "Female",
    contactInfo: "555-1007",
    street: "57 Walnut St",
    area: "Riverside",
    city: "Austin",
    mrn: "PT-Y5Z6A7B8",
    status: "Active",
  },
];

const PAGE_SIZE = 5;

const fieldClassName =
  "w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20";

const EMPTY_FORM: PatientFormValues = {
  name: "",
  dob: "",
  gender: "",
  contactInfo: "",
  street: "",
  area: "",
  city: "",
  status: ""
};

function generateMrn(): string {
  const hex = crypto.randomUUID().replace(/-/g, "").slice(0, 8).toUpperCase();
  return `PT-${hex}`;
}

function nextPatientId(patients: Patient[]): number {
  return patients.reduce((m, p) => Math.max(m, p.patientId), 0) + 1;
}

// ── Component ─────────────────────────────────────────────────────────

export default function Patients() {
  const navigate = useNavigate();

  const [patients, setPatients] = useState<Patient[]>(() => [
    ...DUMMY_PATIENTS,
  ]);
  const [searchQuery, setSearchQuery] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [editingPatient, setEditingPatient] = useState<Patient | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PatientFormValues>({ defaultValues: EMPTY_FORM });

  // ── Dialog open/close ─────────────────────────────────────────────────

  function handleOpenAdd() {
    reset(EMPTY_FORM);
    setShowAddDialog(true);
  }

  function handleOpenEdit(row: Patient) {
    setEditingPatient(row);
    reset({
      name: row.name,
      dob: row.dob,
      gender: row.gender,
      contactInfo: row.contactInfo,
      street: row.street,
      area: row.area,
      city: row.city,
    });
  }

  function handleClose() {
    setShowAddDialog(false);
    setEditingPatient(null);
    reset(EMPTY_FORM);
  }

  // ── Single submit handler for both add & edit ─────────────────────────

  function onSubmit(values: PatientFormValues) {
    if (editingPatient != null) {
      // Edit — DOB & gender are disabled in edit mode, keep originals
      setPatients((prev) =>
        prev.map((p) =>
          p.patientId === editingPatient.patientId
            ? {
                ...p,
                name: values.name.trim(),
                contactInfo: values.contactInfo.trim(),
                street: values.street.trim(),
                area: values.area.trim(),
                city: values.city.trim(),
              }
            : p,
        ),
      );
    } else {
      // Add
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
        status: "Active",
      };
      setPatients((prev) => [...prev, newPatient]);
    }
    handleClose();
  }

  function handleDelete(patientId: number) {
    setPatients((prev) => prev.filter((p) => p.patientId !== patientId));
  }

  // ── Filtering & pagination ────────────────────────────────────────────

  const filteredPatients = useMemo(() => {
    const q = searchQuery.trim().toLowerCase();
    if (q === "") return patients;
    return patients.filter(
      (p) =>
        p.name.toLowerCase().includes(q) || p.mrn.toLowerCase().includes(q),
    );
  }, [patients, searchQuery]);

  const totalPages = Math.max(
    1,
    Math.ceil(filteredPatients.length / PAGE_SIZE),
  );

  useEffect(() => {
    setCurrentPage((p) => Math.min(p, totalPages));
  }, [totalPages]);

  const paginatedPatients = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return filteredPatients.slice(start, start + PAGE_SIZE);
  }, [filteredPatients, currentPage]);

  // ── Table config ──────────────────────────────────────────────────────

  const listColumns = [
    { key: "mrn", label: "MRN" },
    { key: "name", label: "Name" },
    { key: "dob", label: "Date of Birth" },
    { key: "gender", label: "Gender" },
    { key: "contactInfo", label: "Contact" },
    { key: "status", label: "Status" },
  ];

  const listTableData = paginatedPatients.map((p) => ({
    ...p,
    status: <Badge status={p.status} />,
  }));

  // ── Render ────────────────────────────────────────────────────────────

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Patients</h1>

      <div className="flex items-center justify-between gap-4">
        <input
          type="text"
          placeholder="Search by name or MRN..."
          value={searchQuery}
          onChange={(e) => {
            setSearchQuery(e.target.value);
            setCurrentPage(1);
          }}
          className="w-72 rounded-lg border border-gray-300 px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <Button
          type="button"
          variant="primary"
          size="sm"
          onClick={handleOpenAdd}
        >
          Register New Patient
        </Button>
      </div>

      {/* Patients Table */}
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          columns={listColumns}
          data={listTableData}
          showActions
          actions={[
            {
              label: "Edit",
              onClick: (row) => {
                const p = patients.find((x) => x.patientId === row.patientId);
                if (p != null) handleOpenEdit(p);
              },
            },
            {
              label: "Add Coverage",
              onClick: (row) => navigate(`${row.patientId}/coverage`),
            },
            {
              label: "Delete",
              variant: "danger",
              onClick: (row) => handleDelete(row.patientId as number),
            },
          ]}
        />
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setCurrentPage}
        />
      </div>

      {/* ── Single Add/Edit Dialog ── */}
      <Dialog
        isOpen={showAddDialog || editingPatient != null}
        onClose={handleClose}
        title={editingPatient != null ? "Edit Patient" : "Register New Patient"}
        maxWidth="lg"
      >
        <form
          className="space-y-4"
          onSubmit={handleSubmit(onSubmit)}
          noValidate
        >
          {/* MRN read-only info in edit mode */}
          {editingPatient != null && (
            <p className="text-sm text-gray-500">
              <span className="font-medium text-gray-700">MRN:</span>{" "}
              {editingPatient.mrn}
            </p>
          )}

          <PatientFormFields
            mode={editingPatient != null ? "edit" : "add"}
            register={register}
            errors={errors}
            fieldClass={fieldClassName}
          />

          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={handleClose}>
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              {editingPatient != null ? "Save Changes" : "Register"}
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
