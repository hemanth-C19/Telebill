import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { Button } from "../../components/shared/ui/Button";
import { Table } from "../../components/shared/ui/Table";
import { Dialog } from "../../components/shared/ui/Dialog";
import { Badge } from "../../components/shared/ui/Badge";
import apiClient from "../../api/client";

type FeeSchedule = {
  feeId: number;
  planId: number;
  cptHcpcs: string;
  modifierCombo: string;
  allowedAmount: number;
  effectiveFrom: string;
  effectiveTo: string;
  status: string;
};

type FeeFormValues = {
  cptHcpcs: string;
  modifierCombo: string;
  allowedAmount: string;
  effectiveFrom: string;
  effectiveTo: string;
  status: string;
};

const fieldClassName =
  "w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500";

export default function FeeSchedules() {
  const { planId } = useParams<{ planId: string }>();
  const { payerId } = useParams<{ payerId: string }>();
  const location = useLocation();
  const navigate = useNavigate();
  const state =
    (location.state as { planName?: string; payerName?: string }) ?? {};
  const planName = state.planName ?? `Plan #${planId}`;
  const payerName = state.payerName ?? "Payer";

  const [fees, setFees] = useState<FeeSchedule[]>([]);
  const [showAddFee, setShowAddFee] = useState(false);
  const [editingFee, setEditingFee] = useState<FeeSchedule | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const FetchFeeSchedules = async () => {
    try {
      console.log("Fetching Fee Schedules");
      const response = await apiClient.get(
        `api/v1/MasterData/FeeSchedules/GetFeesByPlanId/${planId}`,
      );
      console.log(response.data);
      setFees(response.data);
      setIsLoading(false);
    } catch (error) {
      console.log("Error fetching fee schedules ", error);
    }
  };

  useEffect(() => {
    FetchFeeSchedules();
  }, []);

  const addFeeForm = useForm<FeeFormValues>({
    defaultValues: { status: "Active" },
  });
  const editFeeForm = useForm<FeeFormValues>();

  const onAddFee = async (data: FeeFormValues) => {
    const payload = {
      ...data,
      planId: planId,
    };

    try {
      console.log("requested for adding fee");
      await apiClient.post("api/v1/MasterData/FeeSchedules/AddFee", payload);
      setShowAddFee(false);
      FetchFeeSchedules();
    } catch (error) {
      console.log("Error posting fee data ", error);
    }
  };

  const onEditFee = async (data: FeeFormValues) => {

    const payload = {
      ...data,
      feeId: editingFee?.feeId,
      planId: planId
    }

    try {
      console.log("Requested for fee updation");
      await apiClient.put('api/v1/MasterData/FeeSchedules/UpdateFee', payload);
      setEditingFee(null);
      FetchFeeSchedules();
    } catch (error) {
      console.log("Error editing fee data ", error);
    }
  };

  const onDelete = async (feeId: number)=>{
    try{
      console.log("requested for Fee Deletion")
      await apiClient.delete(`api/v1/MasterData/FeeSchedules/DeleteFee/${feeId}`)
      FetchFeeSchedules();
    }
    catch(error){
      console.log("error deleting fee schedules: ", error);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-1 text-sm text-gray-500">
        <span>{">"}</span>
        <button
          type="button"
          className="hover:text-gray-700"
          onClick={() =>
            navigate("/admin/master-data", { state: { tab: "payers" } })
          }
        >
          Payers
        </button>
        <span>{">"}</span>
        <button
          type="button"
          className="hover:text-gray-700"
          onClick={() =>
            navigate(`/admin/master-data/payers/${payerId}/plans`, {
              state: { payerName },
            })
          }
        >
          {payerName}
        </button>
        <span>{">"}</span>
        <span className="font-medium text-gray-900">{planName}</span>
      </div>

      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">
          {planName} — Fee Schedules
        </h1>
        <Button size="sm" onClick={() => setShowAddFee(true)}>
          Add Fee Schedule
        </Button>
      </div>

      <Table
        columns={[
          { key: "cptHcpcs", label: "CPT/HCPCS" },
          { key: "modifierCombo", label: "Modifier Combo" },
          { key: "allowedAmountText", label: "Allowed Amount" },
          { key: "effectiveFrom", label: "Effective From" },
          { key: "effectiveTo", label: "Effective To" },
          { key: "status", label: "Status" },
        ]}
        data={fees.map((row) => ({
          ...row,
          allowedAmountText: `$${row.allowedAmount.toFixed(2)}`,
          status: <Badge status={row.status} />,
        }))}
        loading={isLoading}
        showActions={true}
        actions={[
          {
            label: "Edit",
            onClick: (row) => {
              const selected = row as FeeSchedule;
              setEditingFee(selected);
              editFeeForm.reset({
                cptHcpcs: selected.cptHcpcs,
                modifierCombo: selected.modifierCombo,
                allowedAmount: String(selected.allowedAmount),
                effectiveFrom: selected.effectiveFrom,
                effectiveTo: selected.effectiveTo,
                status: selected.status,
              });
            },
          },
          {
            label: "Delete",
            variant: "danger",
            onClick: (row) => {
              const selected = row as FeeSchedule;
              onDelete(selected.feeId)
            },
          },
        ]}
      />

      {fees.length === 0 && (
        <div className="text-center py-12 text-gray-500">
          No fee schedules for this plan yet. Add the first fee schedule above.
        </div>
      )}

      <Dialog
        isOpen={showAddFee}
        onClose={() => {
          setShowAddFee(false);
          addFeeForm.reset();
        }}
        title="Add Fee Schedule"
        maxWidth="lg"
      >
        <form
          className="space-y-4"
          onSubmit={addFeeForm.handleSubmit(onAddFee)}
        >
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                CPT/HCPCS Code *
              </label>
              <input
                {...addFeeForm.register("cptHcpcs", {
                  required: "CPT/HCPCS is required",
                  setValueAs: (v: string) => v.trim().toUpperCase(),
                  pattern: { value: /^[A-Z0-9]{4,5}$/i, message: "Enter a valid CPT/HCPCS code (4–5 characters)" },
                })}
                className={fieldClassName}
                placeholder="e.g. 99213"
              />
              {addFeeForm.formState.errors.cptHcpcs && (
                <p className="text-red-500 text-xs mt-1">
                  {addFeeForm.formState.errors.cptHcpcs.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Modifier Combo
              </label>
              <input
                {...addFeeForm.register("modifierCombo", {
                  setValueAs: (v: string) => v.trim().toUpperCase(),
                  pattern: { value: /^[A-Z0-9]+(,[A-Z0-9]+)*$/i, message: "Use comma-separated codes e.g. GT or GT,95" },
                })}
                className={fieldClassName}
                placeholder="e.g. GT or GT,95"
              />
              {addFeeForm.formState.errors.modifierCombo && (
                <p className="text-red-500 text-xs mt-1">{addFeeForm.formState.errors.modifierCombo.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Allowed Amount *
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                {...addFeeForm.register("allowedAmount", {
                  required: "Allowed amount is required",
                  validate: (v) => Number(v) > 0 || "Must be > 0",
                })}
                className={fieldClassName}
              />
              {addFeeForm.formState.errors.allowedAmount && (
                <p className="text-red-500 text-xs mt-1">
                  {addFeeForm.formState.errors.allowedAmount.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Effective From *
              </label>
              <input
                type="date"
                {...addFeeForm.register("effectiveFrom", {
                  required: "Effective from is required",
                })}
                className={fieldClassName}
              />
              {addFeeForm.formState.errors.effectiveFrom && (
                <p className="text-red-500 text-xs mt-1">
                  {addFeeForm.formState.errors.effectiveFrom.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Effective To *
              </label>
              <input
                type="date"
                {...addFeeForm.register("effectiveTo", {
                  required: "Effective to is required",
                  validate: (v) =>
                    v > addFeeForm.getValues("effectiveFrom") || "Must be after Effective From",
                })}
                className={fieldClassName}
              />
              {addFeeForm.formState.errors.effectiveTo && (
                <p className="text-red-500 text-xs mt-1">
                  {addFeeForm.formState.errors.effectiveTo.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                {...addFeeForm.register("status")}
                className={fieldClassName}
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button
              variant="secondary"
              onClick={() => {
                setShowAddFee(false);
                addFeeForm.reset();
              }}
            >
              Cancel
            </Button>
            <Button type="submit">Save Fee</Button>
          </div>
        </form>
      </Dialog>

      <Dialog
        isOpen={editingFee !== null}
        onClose={() => setEditingFee(null)}
        title="Edit Fee Schedule"
        maxWidth="lg"
      >
        <form className="space-y-4" onSubmit={editFeeForm.handleSubmit(onEditFee)}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                CPT/HCPCS Code *
              </label>
              <input
                {...editFeeForm.register("cptHcpcs", {
                  required: "CPT/HCPCS is required",
                  setValueAs: (v: string) => v.trim().toUpperCase(),
                  pattern: { value: /^[A-Z0-9]{4,5}$/i, message: "Enter a valid CPT/HCPCS code (4–5 characters)" },
                })}
                className={fieldClassName}
                placeholder="e.g. 99213"
              />
              {editFeeForm.formState.errors.cptHcpcs && (
                <p className="text-red-500 text-xs mt-1">
                  {editFeeForm.formState.errors.cptHcpcs.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Modifier Combo
              </label>
              <input
                {...editFeeForm.register("modifierCombo", {
                  setValueAs: (v: string) => v.trim().toUpperCase(),
                  pattern: { value: /^[A-Z0-9]+(,[A-Z0-9]+)*$/i, message: "Use comma-separated codes e.g. GT or GT,95" },
                })}
                className={fieldClassName}
                placeholder="e.g. GT or GT,95"
              />
              {editFeeForm.formState.errors.modifierCombo && (
                <p className="text-red-500 text-xs mt-1">{editFeeForm.formState.errors.modifierCombo.message}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Allowed Amount *
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                {...editFeeForm.register("allowedAmount", {
                  required: "Allowed amount is required",
                  validate: (v) => Number(v) > 0 || "Must be > 0",
                })}
                className={fieldClassName}
              />
              {editFeeForm.formState.errors.allowedAmount && (
                <p className="text-red-500 text-xs mt-1">
                  {editFeeForm.formState.errors.allowedAmount.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Effective From *
              </label>
              <input
                type="date"
                {...editFeeForm.register("effectiveFrom", {
                  required: "Effective from is required",
                })}
                className={fieldClassName}
              />
              {editFeeForm.formState.errors.effectiveFrom && (
                <p className="text-red-500 text-xs mt-1">
                  {editFeeForm.formState.errors.effectiveFrom.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Effective To *
              </label>
              <input
                type="date"
                {...editFeeForm.register("effectiveTo", {
                  required: "Effective to is required",
                  validate: (v) =>
                    v > editFeeForm.getValues("effectiveFrom") || "Must be after Effective From",
                })}
                className={fieldClassName}
              />
              {editFeeForm.formState.errors.effectiveTo && (
                <p className="text-red-500 text-xs mt-1">
                  {editFeeForm.formState.errors.effectiveTo.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                {...editFeeForm.register("status")}
                className={fieldClassName}
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={() => setEditingFee(null)}>
              Cancel
            </Button>
            <Button type="submit">Update Fee</Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
