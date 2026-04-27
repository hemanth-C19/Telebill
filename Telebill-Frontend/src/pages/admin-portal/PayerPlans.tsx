import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { Button } from "../../components/shared/ui/Button";
import { Table } from "../../components/shared/ui/Table";
import { Dialog } from "../../components/shared/ui/Dialog";
import { Badge } from "../../components/shared/ui/Badge";
import apiClient from "../../api/client";

type PayerPlan = {
  planId: number;
  payerId: number;
  planName: string;
  networkType: string;
  posDefault: string;
  telehealthModifiersJson: string;
  status: string;
};

type PlanFormValues = {
  planName: string;
  networkType: string;
  posDefault: string;
  telehealthModifiersJson: string;
  status: string;
};

const fieldClassName =
  "w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500";

export default function PayerPlans() {
  const { payerId } = useParams<{ payerId: string }>();
  const location = useLocation();
  const navigate = useNavigate();
  const payerIdNum = Number(payerId);
  const payerName =
    (location.state as { payerName?: string })?.payerName ??
    `Payer #${payerId}`;

  const [plans, setPlans] = useState<PayerPlan[]>([]);
  const [showAddPlan, setShowAddPlan] = useState(false);
  const [editingPlan, setEditingPlan] = useState<PayerPlan | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");

  useEffect(() => {
    const id = setTimeout(() => {
      setDebouncedSearch(search.trim());
    }, 700);
    return () => clearTimeout(id);
  }, [search]);

  const FetchPayerPlans = async () => {
    try {
      const response = await apiClient.get(
        `api/v1/MasterData/PayerPlans/GetPlansByPayerId/${payerId}`,
        { params: { search: debouncedSearch } },
      );
      setPlans(response.data);
      setIsLoading(false);
    } catch (error) {
      console.log("Error fetching payer plans ", error);
    }
  };

  useEffect(() => {
    FetchPayerPlans();
  }, [debouncedSearch]);

  const addPlanForm = useForm<PlanFormValues>({
    defaultValues: { networkType: "PPO", posDefault: "02", status: "Active" },
  });
  const editPlanForm = useForm<PlanFormValues>();

  const onAddPlan = async (data: PlanFormValues) => {
    const payload = {
      ...data,
      payerId: payerId,
    };

    try {
      console.log("post requested to backend ", payload);
      await apiClient.post("api/v1/MasterData/PayerPlans/AddPlan", payload);
      setShowAddPlan(false);
      FetchPayerPlans();
    } catch (error) {
      console.log("error adding payer plan ", error);
    }
  };

  const onEditPlan = async (data: PlanFormValues) => {
    const payload = {
      ...data,
      payerId: payerId,
      planId: editingPlan?.planId,
    };

    try {
      console.log("requesting backend for updation");
      await apiClient.put("api/v1/MasterData/PayerPlans/UpdatePlan", payload);
      setEditingPlan(null);
      FetchPayerPlans();
    } catch (error) {
      console.log(error);
    }
  };

  const onDelete = async (planId: number) => {
    try{
      console.log("requesting backend for deletion")
      await apiClient.delete(`api/v1/MasterData/PayerPlans/DeletePlan/${planId}`);
      FetchPayerPlans();
    } catch(error){
      console.log("error deleting payerplan ",error)
    }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Payers Plans</h1>
      <div className="flex items-center gap-1 text-sm text-gray-500">
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
        <span className="text-gray-900 font-medium">{payerName}</span>
      </div>

      <div className="flex items-center justify-between">
        <div>
          <input
            type="text"
            placeholder="Search..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
            }}
            className="w-72 rounded-lg border border-gray-300 px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <Button size="sm" onClick={() => setShowAddPlan(true)}>
          Add Plan
        </Button>
      </div>

      <Table
        columns={[
          { key: "planName", label: "Plan Name" },
          { key: "networkType", label: "Network Type" },
          { key: "posdefault", label: "Default POS" },
          { key: "telehealthModifiersJson", label: "Telehealth Modifiers" },
          { key: "status", label: "Status" },
        ]}
        data={plans.map((row) => ({
          ...row,
          status: <Badge status={row.status} />,
        }))}
        loading={isLoading}
        showActions={true}
        actions={[
          {
            label: "View Fee Schedules",
            onClick: (row) => {
              const selected = row as PayerPlan;
              navigate(
                `/admin/master-data/payers/${payerIdNum}/plans/${selected.planId}/fees`,
                {
                  state: { planName: selected.planName, payerName },
                },
              );
            },
          },
          {
            label: "Edit",
            onClick: (row) => {
              const selected = row as PayerPlan;
              setEditingPlan(selected);
              editPlanForm.reset({
                planName: selected.planName,
                networkType: selected.networkType,
                posDefault: selected.posDefault,
                telehealthModifiersJson: selected.telehealthModifiersJson,
                status: selected.status,
              });
            },
          },
          {
            label: "Delete",
            variant: "danger",
            onClick: (row) => {
              const selected = row as PayerPlan;
              onDelete(selected.planId);
            },
          },
        ]}
      />

      {plans.length === 0 && (
        <div className="text-center py-12 text-gray-500">
          No plans found for this payer. Add the first plan above.
        </div>
      )}

      <Dialog
        isOpen={showAddPlan}
        onClose={() => {
          setShowAddPlan(false);
          addPlanForm.reset();
        }}
        title="Add Payer Plan"
        maxWidth="lg"
      >
        <form
          className="space-y-4"
          onSubmit={addPlanForm.handleSubmit(onAddPlan)}
        >
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Plan Name *
              </label>
              <input
                {...addPlanForm.register("planName", {
                  required: "Plan name is required",
                  setValueAs: (v: string) => v.trim(),
                  minLength: { value: 3, message: "Plan name must be at least 3 characters" },
                })}
                className={fieldClassName}
              />
              {addPlanForm.formState.errors.planName && (
                <p className="text-red-500 text-xs mt-1">
                  {addPlanForm.formState.errors.planName.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Network Type *
              </label>
              <select
                {...addPlanForm.register("networkType", {
                  required: "Network type is required",
                })}
                className={fieldClassName}
              >
                <option value="PPO">PPO</option>
                <option value="HMO">HMO</option>
                <option value="EPO">EPO</option>
                <option value="POS">POS</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Default POS
              </label>
              <select
                {...addPlanForm.register("posDefault")}
                className={fieldClassName}
              >
                <option value="02">02 — Telehealth Patient Home</option>
                <option value="10">10 — Telehealth Non-Patient Home</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Telehealth Modifiers JSON
              </label>
              <input
                {...addPlanForm.register("telehealthModifiersJson", {
                  setValueAs: (v: string) => v.trim(),
                  pattern: { value: /^[A-Z0-9]+(,[A-Z0-9]+)*$/i, message: "Use comma-separated modifier codes e.g. GT or GT,95" },
                })}
                className={fieldClassName}
                placeholder="e.g. GT,GQ"
              />
              {addPlanForm.formState.errors.telehealthModifiersJson && (
                <p className="text-red-500 text-xs mt-1">
                  {addPlanForm.formState.errors.telehealthModifiersJson.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                {...addPlanForm.register("status")}
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
                setShowAddPlan(false);
                addPlanForm.reset();
              }}
            >
              Cancel
            </Button>
            <Button type="submit">Save Plan</Button>
          </div>
        </form>
      </Dialog>

      <Dialog
        isOpen={editingPlan !== null}
        onClose={() => setEditingPlan(null)}
        title="Edit Payer Plan"
        maxWidth="lg"
      >
        <form
          className="space-y-4"
          onSubmit={editPlanForm.handleSubmit(onEditPlan)}
        >
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Plan Name *
              </label>
              <input
                {...editPlanForm.register("planName", {
                  required: "Plan name is required",
                  setValueAs: (v: string) => v.trim(),
                  minLength: { value: 3, message: "Plan name must be at least 3 characters" },
                })}
                className={fieldClassName}
              />
              {editPlanForm.formState.errors.planName && (
                <p className="text-red-500 text-xs mt-1">
                  {editPlanForm.formState.errors.planName.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Network Type *
              </label>
              <select
                {...editPlanForm.register("networkType", {
                  required: "Network type is required",
                })}
                className={fieldClassName}
              >
                <option value="PPO">PPO</option>
                <option value="HMO">HMO</option>
                <option value="EPO">EPO</option>
                <option value="POS">POS</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Default POS
              </label>
              <select
                {...editPlanForm.register("posDefault")}
                className={fieldClassName}
              >
                <option value="02">02 — Telehealth Patient Home</option>
                <option value="10">10 — Telehealth Non-Patient Home</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Telehealth Modifiers JSON
              </label>
              <input
                {...editPlanForm.register("telehealthModifiersJson", {
                  setValueAs: (v: string) => v.trim(),
                  pattern: { value: /^[A-Z0-9]+(,[A-Z0-9]+)*$/i, message: "Use comma-separated modifier codes e.g. GT or GT,95" },
                })}
                className={fieldClassName}
                placeholder="e.g. GT,GQ"
              />
              {editPlanForm.formState.errors.telehealthModifiersJson && (
                <p className="text-red-500 text-xs mt-1">
                  {editPlanForm.formState.errors.telehealthModifiersJson.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                {...editPlanForm.register("status")}
                className={fieldClassName}
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={() => setEditingPlan(null)}>
              Cancel
            </Button>
            <Button type="submit">Update Plan</Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
