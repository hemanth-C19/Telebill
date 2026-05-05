import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import Badge from "../../components/shared/ui/Badge";
import Button from "../../components/shared/ui/Button";
import Dialog from "../../components/shared/ui/Dialog";
import { Pagination } from "../../components/shared/ui/Pagination";
import Table from "../../components/shared/ui/Table";
import type {
  Patient,
  PatientFormValues,
  PatientIncomming,
} from "../../types/frontdesk.types";
import { PatientFormFields } from "../../components/frontdesk-portal/PatientFormFields";
import apiClient from "../../api/client";

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
  status: "Active",
};

export default function Patients() {
  const navigate = useNavigate();

  const [patients, setPatients] = useState<Patient[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [editingPatient, setEditingPatient] = useState<Patient | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const id = setTimeout(() => {
      setDebouncedSearch(searchQuery.trim());
      setCurrentPage(1);
    }, 700);
    return () => clearTimeout(id);
  }, [searchQuery]);

  async function GetPatients() {
    const result = await apiClient.get(
      "api/v1/PatientCoverage/Patient/GetAllPatients",
      {
        params: {
          search: debouncedSearch,
          page: currentPage,
          limit: 5,
        },
      },
    );
    const finalData: Patient[] = result.data.map((data: PatientIncomming) => ({
      ...data,
      area: JSON.parse(data.addressJson).Area,
      city: JSON.parse(data.addressJson).City,
      street: JSON.parse(data.addressJson).Street,
    }));
    setPatients(finalData);
    setIsLoading(false);
  }

  useEffect(() => {
    GetPatients();
  }, [debouncedSearch, currentPage]);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PatientFormValues>({ defaultValues: EMPTY_FORM });

  function handleOpenAdd() {
    reset(EMPTY_FORM);
    setShowAddDialog(true);
  }

  function handleOpenEdit(row: Patient) {
    setEditingPatient(row);
    console.log(row);
    reset({
      name: row.name,
      dob: row.dob,
      gender: row.gender,
      contactInfo: row.contactInfo,
      //@ts-ignore
      status: row.status.props.status,
      area: row.area,
      city: row.city,
      street: row.street,
    });
  }

  function handleClose() {
    setShowAddDialog(false);
    setEditingPatient(null);
    reset(EMPTY_FORM);
  }

  async function onSubmit(values: PatientFormValues) {
    if (editingPatient != null) {
      await apiClient.put(
        `api/v1/PatientCoverage/Patient/UpdatePatientById/${editingPatient.patientId}`,
        values,
      );
      console.log(values);
    } else {
      await apiClient.post(
        "api/v1/PatientCoverage/Patient/RegisterPatient",
        values,
      );
    }
    await GetPatients();
    handleClose();
  }

  async function handleDelete(patientId: number) {
    await apiClient.delete(
      `api/v1/PatientCoverage/Patient/DeletePatientByID/${patientId}`,
    );
    setIsLoading(true);
    GetPatients();
  }

  const listColumns = [
    { key: "mrn", label: "MRN" },
    { key: "name", label: "Name" },
    { key: "dob", label: "Date of Birth" },
    { key: "gender", label: "Gender" },
    { key: "contactInfo", label: "Contact" },
    { key: "address", label: "Address" },
    { key: "status", label: "Status" },
  ];

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

      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          columns={listColumns}
          data={patients.map((pat) => ({
            ...pat,
            address: (
              <div className="flex flex-col">
                <span className="text-xs">Street: {pat.street}</span>
                <span className="text-xs">Area: {pat.area}</span>
                <span className="text-xs">City: {pat.city}</span>
              </div>
            ),
            status: <Badge status={pat.status} />,
          }))}
          loading={isLoading}
          showActions
          actions={[
            {
              label: "Edit",
              onClick: (row) => {
                handleOpenEdit(row as Patient);
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
          totalPages={5}
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
