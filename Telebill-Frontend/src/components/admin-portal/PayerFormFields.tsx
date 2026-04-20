// Shared payer form fields for Add / Edit payer dialogs
// used by MasterData with react-hook-form register + errors.

import type { FieldErrors, UseFormRegister } from "react-hook-form";
import type { PayerFormValues } from "../../types/admin.types";

export type PayerFormFieldsProps = {
  mode: "add" | "edit";
  register: UseFormRegister<PayerFormValues>;
  errors: FieldErrors<PayerFormValues>;
  fieldClass: string;
};

export function PayerFormFields({
  mode,
  register,
  errors,
  fieldClass,
}: PayerFormFieldsProps) {
  const id = (name: string) => `${mode}-${name}`;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("Name")}
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          Payer Name *
        </label>
        <input
          id={id("Name")}
          placeholder="Enter payer name"
          className={fieldClass}
          {...register("Name", { required: "Payer name is required" })}
        />
        {errors.Name && (
          <p className="text-red-500 text-xs mt-1">{errors.Name.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("PayerCode")}
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          Payer Code / EDI ID *
        </label>
        <input
          id={id("PayerCode")}
          placeholder="Enter payer code"
          className={fieldClass}
          {...register("PayerCode", { required: "Payer code is required" })}
        />
        {errors.PayerCode && (
          <p className="text-red-500 text-xs mt-1">
            {errors.PayerCode.message}
          </p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("ClearinghouseCode")}
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          Clearinghouse Code
        </label>
        <input
          id={id("ClearinghouseCode")}
          placeholder="Enter clearinghouse code"
          className={fieldClass}
          {...register("ClearinghouseCode")}
        />
      </div>

      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("ContactInfo")}
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          Contact Info
        </label>
        <input
          id={id("ContactInfo")}
          placeholder="Enter contact info"
          className={fieldClass}
          {...register("ContactInfo")}
        />
      </div>

      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("Status")}
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          Status
        </label>
        <select
          id={id("Status")}
          className={fieldClass}
          {...register("Status")}
        >
          <option value="Active">Active</option>
          <option value="Inactive">Inactive</option>
        </select>
      </div>
    </div>
  );
}
