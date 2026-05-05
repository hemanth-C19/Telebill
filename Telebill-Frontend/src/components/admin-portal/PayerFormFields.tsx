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
          {...register("Name", {
            required: "Payer name is required",
            setValueAs: (v: string) => v.trim(),
            minLength: { value: 3, message: "Payer name must be at least 3 characters" },
            pattern: {
              value: /^[A-Za-z\s'-]+$/,
              message: "Payer name must not contain numbers or special characters",
            },
          })}
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
          {...register("PayerCode", {
            required: "Payer code is required",
            setValueAs: (v: string) => v.trim(),
            minLength: { value: 2, message: "Payer code must be at least 2 characters" },
          })}
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
          {...register("ClearinghouseCode", {
            setValueAs: (v: string) => v.trim(),
          })}
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
          placeholder="Enter contact email"
          className={fieldClass}
          {...register("ContactInfo", {
            setValueAs: (v: string) => v.trim(),
            pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: "Enter a valid email address" },
          })}
        />
        {errors.ContactInfo && (
          <p className="text-red-500 text-xs mt-1">{errors.ContactInfo.message}</p>
        )}
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
