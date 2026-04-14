import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { Button } from "../../components/shared/ui/Button";
import { Table } from "../../components/shared/ui/Table";
import { Dialog } from "../../components/shared/ui/Dialog";
import { Badge } from "../../components/shared/ui/Badge";
import apiClient from "../../api/client";
import { Pagination } from "../../components/shared/ui/Pagination";

type Payer = {
  payerId: number;
  Name: string;
  PayerCode: string;
  ClearinghouseCode: string;
  ContactInfo: string;
  Status: string;
};

type PayerFormValues = {
  Name: string;
  PayerCode: string;
  ClearinghouseCode: string;
  ContactInfo: string;
  Status: string;
};

// const DUMMY_PAYERS: Payer[] = [
//   { PayerId: 10, Name: 'BlueCross BlueShield', PayerCode: 'BCBS01', ClearinghouseCode: 'CHC-BC01', ContactInfo: '800-123-4567', Status: 'Active' },
//   { PayerId: 20, Name: 'Aetna', PayerCode: 'AET001', ClearinghouseCode: 'CHC-AE01', ContactInfo: '800-234-5678', Status: 'Active' },
//   { PayerId: 30, Name: 'United Healthcare', PayerCode: 'UHC001', ClearinghouseCode: 'CHC-UH01', ContactInfo: '800-345-6789', Status: 'Active' },
//   { PayerId: 40, Name: 'Cigna', PayerCode: 'CGN001', ClearinghouseCode: 'CHC-CG01', ContactInfo: '800-456-7890', Status: 'Inactive' },
// ]

const fieldClassName =
  "w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500";

export default function MasterData() {
  const navigate = useNavigate();
  const [payers, setPayers] = useState<Payer[]>([]);
  const [showAddPayer, setShowAddPayer] = useState(false);
  const [editingPayer, setEditingPayer] = useState<Payer | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [pageNo, setPageNo] = useState(1);

  const addPayerForm = useForm<PayerFormValues>({
    defaultValues: { Status: "Active" },
  });
  const editPayerForm = useForm<PayerFormValues>();

  const FetchPayers = async () => {
    try {
      console.log("request sent to backend");
      const response = await apiClient.get(
        "api/v1/MasterData/Payers/GetAllPayers",
        {
          params: {
            search: search,
            page: pageNo,
            limit: 5,
          },
        },
      );
      console.log(response.data)
      setPayers(response.data);
      setIsLoading(false);
    } catch (error) {
      console.log("error fetching payers: ", error);
    }
  };

  useEffect(() => {
    FetchPayers();
  }, [search, pageNo]);

  const onAddPayer = async (data: PayerFormValues) => {
    try {
      console.log("post request to backend");
      await apiClient.post("api/v1/MasterData/Payers/AddPayer", data);
      setShowAddPayer(false);
      FetchPayers();
    } catch (error) {
      console.log("error adding payer ", error);
    }
  };

  const onEditPayer = async (data: PayerFormValues) => {
    const payload = {
      ...data,
      PayerId: editingPayer?.payerId,
    };

    try {
      await apiClient.put("api/v1/MasterData/Payers/UpdatePayer", payload);
      setEditingPayer(null);
      FetchPayers();
    } catch (error) {
      console.log("error editing payer ", error);
    }
  };

  const onDeletePayer = async (Pid: number) => {
    try {
      await apiClient.delete(`api/v1/MasterData/Payers/DeletePayer/${Pid}`);
      FetchPayers();
    } catch (error) {
      console.log("error deleting Payer ", error);
    }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Payers</h1>
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
        <Button
          variant="primary"
          size="sm"
          onClick={() => setShowAddPayer(true)}
        >
          Add Payer
        </Button>
      </div>

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
                    state: { payerName: row.Name },
                  })
                }
              >
                View Plans →
              </button>
            ),
            //@ts-ignore
            status: (<Badge status= {row.status}/>)
          }))}
          showActions={true}
          actions={[
            {
              label: "Edit",
              onClick: (row) => {
                const selected = row as Payer;
                setEditingPayer(selected);
                editPayerForm.reset({
                  Name: row.name,
                  PayerCode: row.payerCode,
                  ClearinghouseCode: row.clearinghouseCode,
                  ContactInfo: row.contactInfo,
                  Status: row.status,
                });
              },
            },
            {
              label: "Delete",
              variant: "danger",
              onClick: (row) => {
                onDeletePayer(row.payerId);
              },
            },
          ]}
        />
        <Pagination
          currentPage={pageNo}
          onPageChange={setPageNo}
          totalPages={5}
        />
      </div>

      <Dialog
        isOpen={showAddPayer}
        onClose={() => {
          setShowAddPayer(false);
          addPayerForm.reset();
        }}
        title="Add Payer"
        maxWidth="lg"
      >
        <form
          className="space-y-4"
          onSubmit={addPayerForm.handleSubmit(onAddPayer)}
        >
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Payer Name *
              </label>
              <input
                {...addPayerForm.register("Name", {
                  required: "Payer name is required",
                })}
                className={fieldClassName}
              />
              {addPayerForm.formState.errors.Name && (
                <p className="text-red-500 text-xs mt-1">
                  {addPayerForm.formState.errors.Name.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Payer Code / EDI ID *
              </label>
              <input
                {...addPayerForm.register("PayerCode", {
                  required: "Payer code is required",
                })}
                className={fieldClassName}
              />
              {addPayerForm.formState.errors.PayerCode && (
                <p className="text-red-500 text-xs mt-1">
                  {addPayerForm.formState.errors.PayerCode.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Clearinghouse Code
              </label>
              <input
                {...addPayerForm.register("ClearinghouseCode")}
                className={fieldClassName}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Contact Info
              </label>
              <input
                {...addPayerForm.register("ContactInfo")}
                className={fieldClassName}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                {...addPayerForm.register("Status")}
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
                setShowAddPayer(false);
                addPayerForm.reset();
              }}
            >
              Cancel
            </Button>
            <Button type="submit">Save Payer</Button>
          </div>
        </form>
      </Dialog>

      <Dialog
        isOpen={editingPayer !== null}
        onClose={() => setEditingPayer(null)}
        title="Edit Payer"
        maxWidth="lg"
      >
        <form
          className="space-y-4"
          onSubmit={editPayerForm.handleSubmit(onEditPayer)}
        >
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Payer Name *
              </label>
              <input
                {...editPayerForm.register("Name", {
                  required: "Payer name is required",
                })}
                className={fieldClassName}
              />
              {editPayerForm.formState.errors.Name && (
                <p className="text-red-500 text-xs mt-1">
                  {editPayerForm.formState.errors.Name.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Payer Code / EDI ID *
              </label>
              <input
                {...editPayerForm.register("PayerCode", {
                  required: "Payer code is required",
                })}
                className={fieldClassName}
              />
              {editPayerForm.formState.errors.PayerCode && (
                <p className="text-red-500 text-xs mt-1">
                  {editPayerForm.formState.errors.PayerCode.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Clearinghouse Code
              </label>
              <input
                {...editPayerForm.register("ClearinghouseCode")}
                className={fieldClassName}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Contact Info
              </label>
              <input
                {...editPayerForm.register("ContactInfo")}
                className={fieldClassName}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                {...editPayerForm.register("Status")}
                className={fieldClassName}
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={() => setEditingPayer(null)}>
              Cancel
            </Button>
            <Button type="submit">Update Payer</Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
