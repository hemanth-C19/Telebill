import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import Badge from "../../components/shared/ui/Badge";
import Button from "../../components/shared/ui/Button";
import { Card } from "../../components/shared/ui/Card";
import Dialog from "../../components/shared/ui/Dialog";
import Input from "../../components/shared/ui/Input";
import Table from "../../components/shared/ui/Table";
import apiClient from "../../api/client";
import type { Coverage } from "../../types/frontdesk.types";

type CoverageFormValues = {
  payerId: string;
  planId: string;
  memberId: string;
  groupNumber: string;
  effectiveFrom: string;
  effectiveTo: string;
};

type PayerOption = { payerId: number; payerName: string; payerCode: string };
type PlanOption = { planId: number; planName: string };

const selectClassName =
  "w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20";

export default function PatientCoverage() {
  const { patientId: patientIdParam } = useParams<{ patientId: string }>();
  const navigate = useNavigate();

  const patientId = Number(patientIdParam);

  const [coverages, setCoverages] = useState<Coverage[]>([]);
  const [patient, setPatient] = useState<{ name: string; mrn: string } | null>(
    null,
  );
  const [loading, setLoading] = useState(true);
  const [payerOptions, setPayerOptions] = useState<PayerOption[]>([]);
  const [planOptions, setPlanOptions] = useState<PlanOption[]>([]);
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [verifyingId, setVerifyingId] = useState<number | null>(null);
  const [verifyResult, setVerifyResult] = useState<{
    coverageId: number;
    result: string;
  } | null>(null);

  const coverageForm = useForm<CoverageFormValues>({
    defaultValues: {
      payerId: "",
      planId: "",
      memberId: "",
      groupNumber: "",
      effectiveFrom: "",
      effectiveTo: "",
    },
  });

  const watchedPayerId = coverageForm.watch("payerId");

  async function fetchCoverages() {
    const res = await apiClient.get(
      `api/v1/PatientCoverage/Coverage/GetCoverageById/${patientId}`,
    );
    setCoverages(res.data);
  }

  async function fetchPatient() {
    const res = await apiClient.get(
      `api/v1/PatientCoverage/Patient/GetPatientById/${patientId}`,
    );
    setPatient({ name: res.data.name, mrn: res.data.mrn });
  }

  async function fetchPayers() {
    const res = await apiClient.get(
      "api/v1/MasterData/Payers/GetAllPayersNames",
    );
    setPayerOptions(res.data);
  }

  useEffect(() => {
    Promise.all([fetchPatient(), fetchCoverages(), fetchPayers()]).finally(() =>
      setLoading(false),
    );
  }, []);

  useEffect(() => {
    if (!watchedPayerId) {
      setPlanOptions([]);
      coverageForm.setValue("planId", "");
      return;
    }
    apiClient
      .get("api/v1/MasterData/PayerPlans/GetPlanNamesByPayerId", {
        params: { payerId: watchedPayerId },
      })
      .then((res) => {
        setPlanOptions(res.data);
        coverageForm.setValue(
          "planId",
          res.data.length > 0 ? String(res.data[0].planId) : "",
        );
      });
  }, [watchedPayerId]);

  useEffect(() => {
    if (showAddDialog) {
      coverageForm.reset({
        payerId: "",
        planId: "",
        memberId: "",
        groupNumber: "",
        effectiveFrom: "",
        effectiveTo: "",
      });
      setPlanOptions([]);
    }
  }, [showAddDialog]);

  async function onAddCoverage(values: CoverageFormValues) {
    await apiClient.post("api/v1/PatientCoverage/Coverage/AddCoverage", {
      PatientID: patientId,
      PlanID: Number(values.planId),
      MemberID: values.memberId.trim(),
      GroupNumber: values.groupNumber.trim(),
      EffectiveFrom: values.effectiveFrom,
      EffectiveTo: values.effectiveTo,
    });
    await fetchCoverages();
    setShowAddDialog(false);
  }

  async function handleRemove(coverageId: number) {
    await apiClient.delete(
      `api/v1/PatientCoverage/Coverage/DeleteCoverage/${patientId}/${coverageId}`,
    );
    await fetchCoverages();
  }

  async function handleVerify(coverageId: number) {
    setVerifyingId(coverageId);
    setVerifyResult(null);
    try {
      const res = await apiClient.post(
        `api/v1/PatientCoverage/Coverage/VerifyInsurance/${coverageId}`,
      );
      setVerifyResult({
        coverageId,
        result: res.data.result ?? "Eligible",
      });
    } finally {
      setVerifyingId(null);
    }
  }

  // ── Table config ─────────────────────────────────────────────────────

  const coverageColumns = [
    { key: "planId", label: "Plan ID" },
    { key: "memberId", label: "Member ID" },
    { key: "groupNumber", label: "Group Number" },
    { key: "effectiveFrom", label: "Effective From" },
    { key: "effectiveTo", label: "Effective To" },
    { key: "status", label: "Status" },
  ];

  const coverageTableData = coverages.map((row) => ({
    ...row,
    status: <Badge status={row.status ?? "Active"} />,
  }));

  if (loading) {
    return <p className="text-sm text-gray-500">Loading...</p>;
  }

  if (patient == null) {
    return (
      <div className="space-y-4">
        <button
          type="button"
          onClick={() => navigate("/frontdesk/patients")}
          className="text-sm font-medium text-blue-600 hover:text-blue-800"
        >
          ← Back to Patients
        </button>
        <p className="text-sm text-red-500">Patient not found.</p>
      </div>
    );
  }

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

      {/* Eligibility verify result banner */}
      {verifyResult && (
        <div className="rounded-md border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-800">
          Coverage #{verifyResult.coverageId} — {verifyResult.result}
        </div>
      )}

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
                label: verifyingId !== null ? "Validating…" : "Validate",
                onClick: (row) => handleVerify(row.coverageId as number),
              },
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
            <label className="text-sm font-medium text-gray-700">Payer</label>
            <select
              className={selectClassName}
              {...coverageForm.register("payerId", {
                required: "Select a payer",
              })}
            >
              <option value="">— Select Payer —</option>
              {payerOptions.map((p) => (
                <option key={p.payerId} value={p.payerId}>
                  {p.payerName}
                </option>
              ))}
            </select>
            {coverageForm.formState.errors.payerId && (
              <p className="text-xs text-red-500">
                {coverageForm.formState.errors.payerId.message}
              </p>
            )}
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-gray-700">
              Payer Plan
            </label>
            <select
              className={selectClassName}
              disabled={!watchedPayerId || planOptions.length === 0}
              {...coverageForm.register("planId", {
                required: "Select a plan",
              })}
            >
              <option value="">
                {watchedPayerId && planOptions.length === 0
                  ? "No plans available"
                  : "— Select Plan —"}
              </option>
              {planOptions.map((p) => (
                <option key={p.planId} value={p.planId}>
                  {p.planName}
                </option>
              ))}
            </select>
            {coverageForm.formState.errors.planId && (
              <p className="text-xs text-red-500">
                {coverageForm.formState.errors.planId.message}
              </p>
            )}
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
