import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Button } from "../../components/shared/ui/Button";
import { Table } from "../../components/shared/ui/Table";
import { Dialog } from "../../components/shared/ui/Dialog";
import { Badge } from "../../components/shared/ui/Badge";
import apiClient from "../../api/client";
import { Pagination } from "../../components/shared/ui/Pagination";

type Provider = {
  providerId: number;
  npi: string;
  name: string;
  taxonomy: string;
  telehealthEnrolled: number;
  contactInfo: string;
  status: string;
};

type ProviderFormValues = {
  name: string;
  npi: string;
  taxonomy: string;
  telehealthEnrolled: number;
  contactInfo: string;
  status: string;
};

const fieldClassName =
  "w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500";

export default function ProviderManagement() {
  const [providers, setProviders] = useState<Provider[]>([]);
  const [showAddProvider, setShowAddProvider] = useState(false);
  const [editingProvider, setEditingProvider] = useState<Provider | null>(null);
  const [search, setSearch] = useState("");
  const [pageNo, setPageNo] = useState(1);
  const [loader, setLoader] = useState(true);

  const addProviderForm = useForm<ProviderFormValues>();
  const editProviderForm = useForm<ProviderFormValues>();

  async function GetProviders() {
    console.log("requested to backend");
    const response = await apiClient.get(
      "api/v1/MasterData/Provider/GetAllProviders",
      {
        params: {
          search: search,
          page: pageNo,
          limit: 5,
        },
      },
    );
    const data: Provider[] = response.data;
    setProviders(data);
    setLoader(false);
    console.log(data);
  }

  useEffect(() => {
    const CallData = async () => {
      await GetProviders();
    };
    CallData();
  }, [search, pageNo]);

  const onAddProvider = async (data: ProviderFormValues) => {
    const payload = {
      providerName: data.name,
      ProviderNpi: data.npi,
      ProviderTaxonomy: data.taxonomy,
      ProviderEnrolled: data.telehealthEnrolled,
      ProviderContact: data.contactInfo,
      ProviderStatus: data.status,
    };

    try {
      console.log("provider post request sent");
      console.log(payload);
      await apiClient.post(
        "api/v1/MasterData/Provider/CreateProvider",
        payload,
      );

      setShowAddProvider(false);
      await GetProviders();
    } catch (error) {
      console.log("error at provider post ", error);
    }
  };

  const onEditProvider = async (data: ProviderFormValues) => {
    const payload = {
      providerName: data.name,
      ProviderNpi: data.npi,
      ProviderTaxonomy: data.taxonomy,
      ProviderEnrolled: data.telehealthEnrolled,
      ProviderContact: data.contactInfo,
      ProviderStatus: data.status,
    };

    try {
      console.log("provider post request sent");
      console.log(payload, editingProvider?.providerId);
      await apiClient.put(
        `api/v1/MasterData/Provider/UpdateProviderById/${editingProvider?.providerId}`,
        payload,
      );

      setEditingProvider(null);
      await GetProviders();
    } catch (error) {
      console.log("error at provider post ", error);
    }
  };

  const onDeleteProvider = async (pid: number) =>{
    try{
      await apiClient.delete(`api/v1/MasterData/Provider/DeleteProviderById/${pid}`);
      await GetProviders();
      console.log("deletion successfull");
    }
    catch(error){
      console.log("Error deleting Provider ",error);
    }
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Provider Management</h1>
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
          onClick={() => setShowAddProvider(true)}
        >
          Add Provider
        </Button>
      </div>

      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          loading={loader}
          columns={[
            { key: "npi", label: "NPI" },
            { key: "name", label: "Name" },
            { key: "taxonomy", label: "Taxonomy" },
            { key: "telehealth", label: "Telehealth" },
            { key: "contactInfo", label: "Contact" },
            { key: "status", label: "Status" },
          ]}
          data={providers.map((row) => ({
            ...row,
            telehealth: (
              <span
                className={
                  row.telehealthEnrolled
                    ? "text-green-600 font-medium"
                    : "text-gray-500"
                }
              >
                {row.telehealthEnrolled ? "Yes" : "No"}
              </span>
            ),
            status: <Badge status={row.status} />,
          }))}
          showActions={true}
          actions={[
            {
              label: "Edit",
              onClick: (row) => {
                const selected = row as Provider;
                setEditingProvider(selected);
                editProviderForm.reset({
                  name: selected.name,
                  npi: selected.npi,
                  taxonomy: selected.taxonomy,
                  telehealthEnrolled: selected.telehealthEnrolled ?? false,
                  contactInfo: selected.contactInfo,
                  status: selected.status,
                });
              },
            },
            {
              label: "Delete",
              variant: "danger",
              onClick: (row) => {
                onDeleteProvider(row.providerId);
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
        isOpen={showAddProvider}
        onClose={() => {
          setShowAddProvider(false);
          addProviderForm.reset();
        }}
        title="Add Provider"
        maxWidth="lg"
      >
        <form
          className="space-y-4"
          onSubmit={addProviderForm.handleSubmit(onAddProvider)}
        >
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Name *
              </label>
              <input
                {...addProviderForm.register("name", {
                  required: "Name is required",
                })}
                className={fieldClassName}
              />
              {addProviderForm.formState.errors.name && (
                <p className="text-red-500 text-xs mt-1">
                  {addProviderForm.formState.errors.name.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                NPI *
              </label>
              <input
                {...addProviderForm.register("npi", {
                  required: "NPI is required",
                  validate: (value) =>
                    /^\d{10}$/.test(value) || "NPI must be exactly 10 digits",
                })}
                className={fieldClassName}
              />
              {addProviderForm.formState.errors.npi && (
                <p className="text-red-500 text-xs mt-1">
                  {addProviderForm.formState.errors.npi.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Specialty / Taxonomy
              </label>
              <input
                {...addProviderForm.register("taxonomy")}
                className={fieldClassName}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Contact Info
              </label>
              <input
                {...addProviderForm.register("contactInfo")}
                className={fieldClassName}
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                id="add-telehealth"
                type="checkbox"
                {...addProviderForm.register("telehealthEnrolled")}
              />
              <label
                htmlFor="add-telehealth"
                className="text-sm font-medium text-gray-700"
              >
                Telehealth Enrolled
              </label>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                {...addProviderForm.register("status")}
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
                setShowAddProvider(false);
                addProviderForm.reset();
              }}
            >
              Cancel
            </Button>
            <Button type="submit">Save Provider</Button>
          </div>
        </form>
      </Dialog>

      <Dialog
        isOpen={editingProvider !== null}
        onClose={() => setEditingProvider(null)}
        title="Edit Provider"
        maxWidth="lg"
      >
        <form className="space-y-4" onSubmit={editProviderForm.handleSubmit(onEditProvider)}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Name *
              </label>
              <input
                {...editProviderForm.register("name", {
                  required: "Name is required",
                })}
                className={fieldClassName}
              />
              {editProviderForm.formState.errors.name && (
                <p className="text-red-500 text-xs mt-1">
                  {editProviderForm.formState.errors.name.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                NPI *
              </label>
              <input
                {...editProviderForm.register("npi", {
                  required: "NPI is required",
                  validate: (value) =>
                    /^\d{10}$/.test(value) || "NPI must be exactly 10 digits",
                })}
                className={fieldClassName}
              />
              {editProviderForm.formState.errors.npi && (
                <p className="text-red-500 text-xs mt-1">
                  {editProviderForm.formState.errors.npi.message}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Specialty / Taxonomy
              </label>
              <input
                {...editProviderForm.register("taxonomy")}
                className={fieldClassName}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Contact Info
              </label>
              <input
                {...editProviderForm.register("contactInfo")}
                className={fieldClassName}
              />
            </div>
            <div className="flex items-center gap-2">
              <input
                id="edit-telehealth"
                type="checkbox"
                {...editProviderForm.register("telehealthEnrolled")}
              />
              <label
                htmlFor="edit-telehealth"
                className="text-sm font-medium text-gray-700"
              >
                Telehealth Enrolled
              </label>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                {...editProviderForm.register("status")}
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
              onClick={() => setEditingProvider(null)}
            >
              Cancel
            </Button>
            <Button type="submit">Update Provider</Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
