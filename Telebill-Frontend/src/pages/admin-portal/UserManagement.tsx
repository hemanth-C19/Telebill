import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import Table from "../../components/shared/ui/Table";
import Dialog from "../../components/shared/ui/Dialog";
import Button from "../../components/shared/ui/Button";
import apiClient from "../../api/client";
import { Pagination } from "../../components/shared/ui/Pagination";
import { UserFormFields } from "../../components/admin-portal/UserFormFields";
import type { User, UserFormData } from "../../types/admin.types";
import Badge from "../../components/shared/ui/Badge";

const ROLES = ["Admin", "FrontDesk", "Provider", "Coder", "AR"] as const;

const EMPTY_FORM: UserFormData = {
  name: "",
  role: "",
  email: "",
  phone: "",
  status: "Active",
};

const TABLE_COLUMNS = [
  { key: "name", label: "Name" },
  { key: "email", label: "Email" },
  { key: "role", label: "Role" },
  { key: "phone", label: "Phone" },
  { key: "status", label: "Status" },
];

const fieldClass =
  "w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm placeholder:text-gray-400 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20";

const selectClass =
  "w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500";

export default function UserManagement() {
  const [users, setUsers] = useState<User[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [roleFilter, setRoleFilter] = useState("");
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showEditDialog, setShowEditDialog] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);

  async function GetUsers() {
    console.log("Request backend");
    const response = await apiClient.get(
      "api/v1/IdentityAccess/User/GetUsers",
      {
        params: {
          search: searchTerm,
          role: roleFilter,
          page: currentPage,
          limit: 5,
        },
      },
    );
    console.log("got response ", response.data);
    const users: User[] = response.data;
    setUsers(users);
    setIsLoading(false);
  }

  useEffect(() => {
    const CallUsers = async () => {
      await GetUsers();
    };

    CallUsers();
  }, [searchTerm, roleFilter, currentPage]);

  const {
    register: registerCreate,
    handleSubmit: handleSubmitCreate,
    reset: resetCreate,
    formState: { errors: createErrors },
  } = useForm<UserFormData>({ defaultValues: EMPTY_FORM });

  const {
    register: registerEdit,
    handleSubmit: handleSubmitEdit,
    reset: resetEdit,
    formState: { errors: editErrors },
  } = useForm<UserFormData>({ defaultValues: EMPTY_FORM });

  function handleOpenEdit(row: User) {
    setSelectedUser(row);
    resetEdit({
      name: row.name,
      role: row.role,
      email: row.email,
      phone: row.phone,
      status: row.status,
    });
    setShowEditDialog(true);
  }

  function handleCloseAll() {
    setShowCreateDialog(false);
    setShowEditDialog(false);
    setSelectedUser(null);
    resetCreate(EMPTY_FORM);
    resetEdit(EMPTY_FORM);
  }

  async function onCreateSubmit(data: UserFormData) {
    await apiClient.post("api/v1/IdentityAccess/User/AddUser", data);
    resetCreate(EMPTY_FORM);
    setShowCreateDialog(false);
    setIsLoading(true);
    await GetUsers();
  }

  async function onEditSubmit(data: UserFormData) {
    const payload = { ...data, userId: selectedUser?.userId };
    await apiClient.put("api/v1/IdentityAccess/User/UpdateUser", payload);
    console.log("Updation Done");
    setIsLoading(true);
    setShowEditDialog(false);
    await GetUsers();
  }

  async function handleDelete(userId: number) {
    await apiClient.delete(`api/v1/IdentityAccess/User/DeleteUser/${userId}`);
    console.log("Deletion done");
    setIsLoading(true);
    await GetUsers();
  }

  return (
    <div className="w-full">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">User Management</h1>
          <p className="text-sm text-gray-500">
            Manage system users and their role assignments
          </p>
        </div>
        <Button
          variant="primary"
          onClick={() => {
            setShowCreateDialog(true);
          }}
        >
          ＋ Create User
        </Button>
      </div>

      <div className="mb-6 flex items-center gap-3">
        <input
          type="text"
          placeholder="Search by name or email..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-72 rounded-lg border border-gray-300 px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <select
          value={roleFilter}
          onChange={(e) => setRoleFilter(e.target.value)}
          className="rounded-lg border border-gray-300 px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">All Roles</option>
          {ROLES.map((role) => (
            <option key={role} value={role}>
              {role}
            </option>
          ))}
        </select>
      </div>

      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <Table
          columns={TABLE_COLUMNS}
          data={users.map((user) => ({
            ...user,
            status: <Badge status={user.status} />,
          }))}
          loading={isLoading}
          showActions
          actions={[
            {
              label: "Edit",
              onClick: (row) => {
                const u = users.find((x) => x.userId === row.userId);
                if (u != null) handleOpenEdit(u);
              },
              variant: "default",
            },
            {
              label: "Delete",
              onClick: (row) => {
                const u = users.find((x) => x.userId === row.userId);
                if (u != null) handleDelete(u.userId);
              },
              variant: "danger",
            },
          ]}
        />
        <Pagination
          currentPage={currentPage}
          onPageChange={setCurrentPage}
          totalPages={5}
        />
      </div>

      <Dialog
        isOpen={showCreateDialog}
        onClose={handleCloseAll}
        title="Create New User"
        maxWidth="md"
      >
        <form
          onSubmit={handleSubmitCreate(onCreateSubmit)}
          className="flex flex-col gap-4"
          noValidate
        >
          <UserFormFields
            mode="create"
            register={registerCreate}
            errors={createErrors}
            roles={ROLES}
            fieldClass={fieldClass}
            selectClass={selectClass}
          />
          <div className="mt-2 flex justify-end gap-3">
            <Button variant="secondary" type="button" onClick={handleCloseAll}>
              Cancel
            </Button>
            <Button variant="primary" type="submit">
              Create User
            </Button>
          </div>
        </form>
      </Dialog>

      <Dialog
        isOpen={showEditDialog}
        onClose={handleCloseAll}
        title="Edit User"
        maxWidth="md"
      >
        <form
          onSubmit={handleSubmitEdit(onEditSubmit)}
          className="flex flex-col gap-4"
          noValidate
        >
          <UserFormFields
            mode="edit"
            register={registerEdit}
            errors={editErrors}
            roles={ROLES}
            fieldClass={fieldClass}
            selectClass={selectClass}
          />
          <div className="mt-2 flex justify-end gap-3">
            <Button variant="secondary" type="button" onClick={handleCloseAll}>
              Cancel
            </Button>
            <Button variant="primary" type="submit">
              Save Changes
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
