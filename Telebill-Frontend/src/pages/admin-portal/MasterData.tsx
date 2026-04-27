import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { Button } from "../../components/shared/ui/Button";
import { Table } from "../../components/shared/ui/Table";
import { Dialog } from "../../components/shared/ui/Dialog";
import { Badge } from "../../components/shared/ui/Badge";
import { Pagination } from "../../components/shared/ui/Pagination";
import { PayerFormFields } from "../../components/admin-portal/PayerFormFields";
import apiClient from "../../api/client";
import type { Payer, PayerFormValues } from "../../types/admin.types";

// ─── Constants ───────────────────────────────────────────

const fieldClassName =
  "w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500";

const EMPTY_FORM: PayerFormValues = {
  Name: "",
  PayerCode: "",
  ClearinghouseCode: "",
  ContactInfo: "",
  Status: "Active",
};

// ─── Component ───────────────────────────────────────────

export default function MasterData() {
  const navigate = useNavigate();

  const [payers, setPayers] = useState<Payer[]>([]);
  const [editingPayer, setEditingPayer] = useState<Payer | null>(null);
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [pageNo, setPageNo] = useState(1);

  // ✅ ONE form instance — used for both Add and Edit
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PayerFormValues>({ defaultValues: EMPTY_FORM });

  // ─── Fetch Payers ─────────────────────────────────────

  useEffect(() => {
    const id = setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPageNo(1);
    }, 700);
    return () => clearTimeout(id);
  }, [search]);

  const FetchPayers = async () => {
    try {
      const response = await apiClient.get(
        "api/v1/MasterData/Payers/GetAllPayers",
        { params: { search: debouncedSearch, page: pageNo, limit: 5 } },
      );
      console.log(response.data);
      setPayers(response.data);
      setIsLoading(false);
    } catch (error) {
      console.log("Error fetching payers:", error);
    }
  };

  useEffect(() => {
    FetchPayers();
  }, [debouncedSearch, pageNo]);

  // ─── Open Handlers ────────────────────────────────────

  function handleOpenAdd() {
    reset(EMPTY_FORM); // ✅ clear form before opening Add
    setShowAddDialog(true);
  }

  function handleOpenEdit(row: Payer) {
    console.log(row)
    reset({
      Name: row.name,
      PayerCode: row.payerCode,
      ClearinghouseCode: row.clearinghouseCode,
      ContactInfo: row.contactInfo,
      Status: row.status.props.status,
    });
    setEditingPayer(row);
  }

  // ─── Close Handler ────────────────────────────────────

  function handleClose() {
    setShowAddDialog(false);
    setEditingPayer(null);
    reset(EMPTY_FORM);
  }

  // ─── Submit — handles both Add and Edit ───────────────

  const onSubmit = async (data: PayerFormValues) => {
    try {
      if (editingPayer) {
        // Use correct property name for payerId
        const payload = { ...data, payerId: editingPayer.payerId };
        await apiClient.put("api/v1/MasterData/Payers/UpdatePayer", payload);
      } else {
        await apiClient.post("api/v1/MasterData/Payers/AddPayer", data);
      }

      handleClose();
      await FetchPayers();
    } catch (error) {
      console.log("Error saving payer:", error);
    }
  };

  // ─── Delete ───────────────────────────────────────────

  const onDeletePayer = async (Pid: number) => {
    try {
      await apiClient.delete(`api/v1/MasterData/Payers/DeletePayer/${Pid}`);
      await FetchPayers();
    } catch (error) {
      console.log("Error deleting payer:", error);
    }
  };

  // ─── Render ───────────────────────────────────────────

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Payers</h1>

      {/* Search + Add Button */}
      <div className="flex items-center justify-between">
        <input
          type="text"
          placeholder="Search..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-72 rounded-lg border border-gray-300 px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <Button variant="primary" size="sm" onClick={handleOpenAdd}>
          Add Payer
        </Button>
      </div>

      {/* Table */}
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          loading={isLoading}
          columns={[
            { key: "name", label: "Payer Name" },
            { key: "payerCode", label: "Payer Code (EDI)" },
            { key: "clearinghouseCode", label: "Clearinghouse Code" },
            { key: "contactInfo", label: "Contact Info" },
            { key: "status", label: "Status" },
            { key: "plans", label: "Plans" },
          ]}
          data={payers.map((row) => ({
            ...row,
            plans: (
              <button
                type="button"
                className="text-blue-600 text-sm hover:underline cursor-pointer"
                onClick={() =>
                  navigate(`/admin/master-data/payers/${row.payerId}/plans`, {
                    state: { payerName: row.name },
                  })
                }
              >
                View Plans →
              </button>
            ),
            // @ts-ignore
            status: <Badge status={row.status} />,
          }))}
          showActions={true}
          actions={[
            {
              label: "Edit",
              onClick: (row) => handleOpenEdit(row as Payer),
            },
            {
              label: "Delete",
              variant: "danger",
              onClick: (row) => onDeletePayer(row.payerId),
            },
          ]}
        />
        <Pagination
          currentPage={pageNo}
          onPageChange={setPageNo}
          totalPages={5}
        />
      </div>

      {/* ✅ ONE Dialog — used for both Add and Edit */}
      <Dialog
        isOpen={showAddDialog || editingPayer !== null}
        onClose={handleClose}
        title={editingPayer ? "Edit Payer" : "Add Payer"}
        maxWidth="lg"
      >
        <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
          {/* ✅ Reusable form fields component */}
          <PayerFormFields
            mode={editingPayer ? "edit" : "add"}
            register={register}
            errors={errors}
            fieldClass={fieldClassName}
          />

          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={handleClose}>
              Cancel
            </Button>
            <Button type="submit">
              {editingPayer ? "Update Payer" : "Save Payer"}
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
