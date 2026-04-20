// PatientCoverage.tsx — Coverage management for a specific patient
// Route: /patients/:patientId/coverage
// Backend ref: CoverageController GET .../Coverage/GetCoverageById/{patientId}

import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import Badge from "../../components/shared/ui/Badge";
import Button from "../../components/shared/ui/Button";
import { Card } from "../../components/shared/ui/Card";
import Dialog from "../../components/shared/ui/Dialog";
import Input from "../../components/shared/ui/Input";
import Table from "../../components/shared/ui/Table";

// ── Dummy data ────────────────────────────────────────────────────────

const DUMMY_PATIENTS: Record<number, { name: string; mrn: string }> = {
  1: { name: "Alice Johnson", mrn: "PT-A1B2C3D4" },
  2: { name: "Bob Martinez", mrn: "PT-E5F6G7H8" },
  3: { name: "Carol Nguyen", mrn: "PT-I9J0K1L2" },
  4: { name: "David Patel", mrn: "PT-M3N4O5P6" },
  5: { name: "Emily Rodriguez", mrn: "PT-Q7R8S9T0" },
  6: { name: "Frank Williams", mrn: "PT-U1V2W3X4" },
  7: { name: "Grace Kim", mrn: "PT-Y5Z6A7B8" },
};

type Coverage = {
  coverageId: number;
  patientId: number;
  planId: number;
  planName: string;
  memberId: string;
  groupNumber: string;
  effectiveFrom: string;
  effectiveTo: string;
  status: string;
};

// Backend ref: CoverageController GET .../Coverage/GetCoverageById/{patientId}
const DUMMY_COVERAGES: Record<number, Coverage[]> = {
  1: [
    {
      coverageId: 101,
      patientId: 1,
      planId: 10,
      planName: "BlueCross Basic",
      memberId: "BCB-001",
      groupNumber: "GRP-100",
      effectiveFrom: "2023-01-01",
      effectiveTo: "2024-12-31",
      status: "Active",
    },
    {
      coverageId: 102,
      patientId: 1,
      planId: 11,
      planName: "BlueCross Dental",
      memberId: "BCD-001",
      groupNumber: "GRP-101",
      effectiveFrom: "2023-01-01",
      effectiveTo: "2023-12-31",
      status: "Inactive",
    },
  ],
  2: [
    {
      coverageId: 103,
      patientId: 2,
      planId: 20,
      planName: "Aetna PPO",
      memberId: "AET-202",
      groupNumber: "GRP-200",
      effectiveFrom: "2024-03-01",
      effectiveTo: "2025-02-28",
      status: "Active",
    },
  ],
  3: [],
  4: [
    {
      coverageId: 104,
      patientId: 4,
      planId: 30,
      planName: "United Gold",
      memberId: "UHG-404",
      groupNumber: "GRP-300",
      effectiveFrom: "2022-06-01",
      effectiveTo: "2024-05-31",
      status: "Inactive",
    },
  ],
  5: [
    {
      coverageId: 105,
      patientId: 5,
      planId: 10,
      planName: "BlueCross Basic",
      memberId: "BCB-505",
      groupNumber: "GRP-100",
      effectiveFrom: "2024-01-01",
      effectiveTo: "2025-12-31",
      status: "Active",
    },
  ],
  6: [],
  7: [
    {
      coverageId: 106,
      patientId: 7,
      planId: 40,
      planName: "Cigna HMO",
      memberId: "CIG-707",
      groupNumber: "GRP-400",
      effectiveFrom: "2023-07-01",
      effectiveTo: "2025-06-30",
      status: "Active",
    },
  ],
};

const PAYER_PLANS = [
  { planId: 10, planName: "BlueCross Basic" },
  { planId: 11, planName: "BlueCross Dental" },
  { planId: 20, planName: "Aetna PPO" },
  { planId: 30, planName: "United Gold" },
  { planId: 40, planName: "Cigna HMO" },
  { planId: 50, planName: "Medicare Part B" },
] as const;

// ── Helpers ───────────────────────────────────────────────────────────

function nextCoverageId(list: Coverage[]): number {
  return list.reduce((m, c) => Math.max(m, c.coverageId), 0) + 1;
}

const selectClassName =
  "w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20";

// ── Types ─────────────────────────────────────────────────────────────

type CoverageFormValues = {
  planId: string;
  memberId: string;
  groupNumber: string;
  effectiveFrom: string;
  effectiveTo: string;
};

// ── Component ─────────────────────────────────────────────────────────

export default function PatientCoverage() {
  const { patientId: patientIdParam } = useParams<{ patientId: string }>();
  const navigate = useNavigate();

  const patientId = Number(patientIdParam);

  // Seed local coverage state from dummy data for this patient
  const [coverages, setCoverages] = useState<Coverage[]>(() => {
    return (DUMMY_COVERAGES[patientId] ?? []).map((c) => ({ ...c }));
  });

  const [showAddDialog, setShowAddDialog] = useState(false);

  const coverageForm = useForm<CoverageFormValues>({
    defaultValues: {
      planId: String(PAYER_PLANS[0].planId),
      memberId: "",
      groupNumber: "",
      effectiveFrom: "",
      effectiveTo: "",
    },
  });

  // Reset form when dialog opens
  useEffect(() => {
    if (showAddDialog) {
      coverageForm.reset({
        planId: String(PAYER_PLANS[0].planId),
        memberId: "",
        groupNumber: "",
        effectiveFrom: "",
        effectiveTo: "",
      });
    }
  }, [showAddDialog, coverageForm]);

  const patient = DUMMY_PATIENTS[patientId];

  // ── Handlers ────────────────────────────────────────────────────────

  function onAddCoverage(values: CoverageFormValues) {
    const planId = Number(values.planId);
    const plan = PAYER_PLANS.find((p) => p.planId === planId);
    const newCov: Coverage = {
      coverageId: nextCoverageId(coverages),
      patientId,
      planId,
      planName: plan?.planName ?? "Unknown Plan",
      memberId: values.memberId.trim(),
      groupNumber: values.groupNumber.trim(),
      effectiveFrom: values.effectiveFrom,
      effectiveTo: values.effectiveTo,
      status: "Active",
    };
    setCoverages((prev) => [...prev, newCov]);
    setShowAddDialog(false);
  }

  function handleRemove(coverageId: number) {
    setCoverages((prev) => prev.filter((c) => c.coverageId !== coverageId));
  }

  // ── Table config ─────────────────────────────────────────────────────

  const coverageColumns = [
    { key: "planName", label: "Plan Name" },
    { key: "memberId", label: "Member ID" },
    { key: "groupNumber", label: "Group Number" },
    { key: "effectiveFrom", label: "Effective From" },
    { key: "effectiveTo", label: "Effective To" },
    { key: "status", label: "Status" },
  ];

  const coverageTableData = coverages.map((row) => ({
    ...row,
    status: <Badge status={row.status} />,
  }));

  // ── Guard: unknown patient ────────────────────────────────────────────

  if (patient == null) {
    return (
      <div className="space-y-4">
        <button
          type="button"
          onClick={() => navigate("/frontdesk/dashboard")}
          className="text-sm font-medium text-blue-600 hover:text-blue-800"
        >
          ← Back to Patients
        </button>
        <p className="text-sm text-red-500">Patient not found.</p>
      </div>
    );
  }

  // ── Render ────────────────────────────────────────────────────────────

  return (
    <div className="space-y-6">
      {/* Back nav */}
      <button
        type="button"
        onClick={() => navigate("/frontdesk/patients")}
        className="text-sm font-medium text-blue-600 hover:text-blue-800"
      >
        ← Back to Patients
      </button>

      {/* Patient info header */}
      <Card title="Patient Coverage">
        <div className="flex flex-wrap gap-6 text-sm text-gray-600">
          <p>
            <span className="font-medium text-gray-800">Name:</span>{" "}
            {patient.name}
          </p>
          <p>
            <span className="font-medium text-gray-800">MRN:</span>{" "}
            {patient.mrn}
          </p>
          <p>
            <span className="font-medium text-gray-800">Patient ID:</span>{" "}
            {patientId}
          </p>
        </div>
      </Card>

      {/* Coverage table */}
      <Card>
        <div className="mb-4 flex flex-col gap-3 border-b border-gray-200 pb-4 sm:flex-row sm:items-center sm:justify-between">
          <h3 className="text-base font-semibold text-gray-800">
            Insurance Coverage
          </h3>
          <Button
            type="button"
            variant="secondary"
            size="sm"
            onClick={() => setShowAddDialog(true)}
          >
            Add Coverage
          </Button>
        </div>

        {coverages.length === 0 ? (
          <p className="py-6 text-center text-sm text-gray-400">
            No coverage records found for this patient.
          </p>
        ) : (
          <Table
            columns={coverageColumns}
            data={coverageTableData}
            showActions
            actions={[
              {
                label: "Remove",
                onClick: (row) => handleRemove(row.coverageId as number),
                variant: "danger",
              },
            ]}
          />
        )}
      </Card>

      {/* Add Coverage Dialog */}
      <Dialog
        isOpen={showAddDialog}
        onClose={() => setShowAddDialog(false)}
        title="Add Coverage"
        maxWidth="md"
      >
        <form
          onSubmit={coverageForm.handleSubmit(onAddCoverage)}
          className="flex flex-col gap-4"
          noValidate
        >
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">
              Payer Plan
            </label>
            <select
              className={selectClassName}
              {...coverageForm.register("planId", { required: true })}
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
            {...coverageForm.register("memberId", {
              required: "Member ID is required",
            })}
            error={coverageForm.formState.errors.memberId?.message}
          />
          <Input
            label="Group Number"
            {...coverageForm.register("groupNumber", {
              required: "Group number is required",
            })}
            error={coverageForm.formState.errors.groupNumber?.message}
          />
          <Input
            label="Effective From"
            type="date"
            {...coverageForm.register("effectiveFrom", {
              required: "Required",
            })}
            error={coverageForm.formState.errors.effectiveFrom?.message}
          />
          <Input
            label="Effective To"
            type="date"
            {...coverageForm.register("effectiveTo", { required: "Required" })}
            error={coverageForm.formState.errors.effectiveTo?.message}
          />
          <div className="flex justify-end gap-2 pt-2">
            <Button
              type="button"
              variant="secondary"
              onClick={() => setShowAddDialog(false)}
            >
              Cancel
            </Button>
            <Button type="submit" variant="primary">
              Add Coverage
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
