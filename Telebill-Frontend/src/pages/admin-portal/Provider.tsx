import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Button } from "../../components/shared/ui/Button";
import { Table } from "../../components/shared/ui/Table";
import { Dialog } from "../../components/shared/ui/Dialog";
import { Badge } from "../../components/shared/ui/Badge";
import { Pagination } from "../../components/shared/ui/Pagination";
import apiClient from "../../api/client";
import { ProviderFormFields } from "../../components/admin-portal/ProviderFormFeilds";
import type { Provider, ProviderFormValues } from "../../types/admin.types";

const fieldClassName =
  "w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500";

const EMPTY_FORM: ProviderFormValues = {
  name: "",
  npi: "",
  taxonomy: "",
  telehealthEnrolled: 0,
  contactInfo: "",
  status: "Active",
};

export default function ProviderManagement() {
  const [providers, setProviders] = useState<Provider[]>([]);
  const [editingProvider, setEditingProvider] = useState<Provider | null>(null);
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [pageNo, setPageNo] = useState(1);
  const [loader, setLoader] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ProviderFormValues>({ defaultValues: EMPTY_FORM });

  useEffect(() => {
    const id = setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPageNo(1);
    }, 700);
    return () => clearTimeout(id);
  }, [search]);

  async function GetProviders() {
    const response = await apiClient.get(
      "api/v1/MasterData/Provider/GetAllProviders",
      { params: { search: debouncedSearch, page: pageNo, limit: 5 } },
    );
    setProviders(response.data);
    setLoader(false);
  }

  useEffect(() => {
    const load = async () => await GetProviders();
    load();
  }, [debouncedSearch, pageNo]);

  function handleOpenAdd() {
    reset(EMPTY_FORM);
    setShowAddDialog(true);
  }

  function handleOpenEdit(row: Provider) {
    setEditingProvider(row);
    reset({
      name: row.name,
      npi: row.npi,
      taxonomy: row.taxonomy,
      telehealthEnrolled: row.telehealthEnrolled,
      contactInfo: row.contactInfo,
      status: row.status,
    });
  }

  function handleClose() {
    setShowAddDialog(false);
    setEditingProvider(null);
    reset(EMPTY_FORM);
  }

  const onSubmit = async (data: ProviderFormValues) => {
    const payload = {
      providerName: data.name,
      ProviderNpi: data.npi,
      ProviderTaxonomy: data.taxonomy,
      ProviderEnrolled: data.telehealthEnrolled,
      ProviderContact: data.contactInfo,
      ProviderStatus: data.status,
    };

    try {
      if (editingProvider) {
        await apiClient.put(
          `api/v1/MasterData/Provider/UpdateProviderById/${editingProvider.providerId}`,
          payload,
        );
      } else {
        await apiClient.post(
          "api/v1/MasterData/Provider/CreateProvider",
          payload,
        );
      }

      handleClose();
      await GetProviders();
    } catch (error) {
      console.log("Error saving provider:", error);
    }
  };

  const onDeleteProvider = async (pid: number) => {
    try {
      await apiClient.delete(
        `api/v1/MasterData/Provider/DeleteProviderById/${pid}`,
      );
      await GetProviders();
    } catch (error) {
      console.log("Error deleting provider:", error);
    }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Provider Management</h1>

      <div className="flex items-center justify-between">
        <input
          type="text"
          placeholder="Search..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-72 rounded-lg border border-gray-300 px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <Button variant="primary" size="sm" onClick={handleOpenAdd}>
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
              onClick: (row) => handleOpenEdit(row as Provider),
            },
            {
              label: "Delete",
              variant: "danger",
              onClick: (row) => onDeleteProvider(row.providerId),
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
        isOpen={showAddDialog || editingProvider !== null}
        onClose={handleClose}
        title={editingProvider ? "Edit Provider" : "Add Provider"}
        maxWidth="lg"
      >
        <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
          <ProviderFormFields
            mode={editingProvider ? "edit" : "add"}
            register={register}
            errors={errors}
            fieldClass={fieldClassName}
          />

          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={handleClose}>
              Cancel
            </Button>
            <Button type="submit">
              {editingProvider ? "Update Provider" : "Save Provider"}
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
