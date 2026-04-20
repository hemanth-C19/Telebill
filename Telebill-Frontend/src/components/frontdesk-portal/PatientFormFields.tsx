// PatientFormFields.tsx — Reusable form fields for Patient add/edit dialogs
// Pattern mirrors ProviderFormFields.tsx

import type { FieldErrors, UseFormRegister } from "react-hook-form";
import type { PatientFormValues } from "../../types/frontdesk.types";
export type PatientFormFieldsProps = {
  mode: "add" | "edit";
  register: UseFormRegister<PatientFormValues>;
  errors: FieldErrors<PatientFormValues>;
  fieldClass: string;
};

export function PatientFormFields({
  mode,
  register,
  errors,
  fieldClass,
}: PatientFormFieldsProps) {
  const id = (name: string) => `${mode}-${name}`;

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
      {/* Full Name */}
      <div className="flex flex-col gap-1 md:col-span-2">
        <label
          htmlFor={id("name")}
          className="text-sm font-medium text-gray-700"
        >
          Full Name *
        </label>
        <input
          id={id("name")}
          type="text"
          placeholder="Enter full name"
          className={fieldClass}
          {...register("name", { required: "Full name is required" })}
        />
        {errors.name && (
          <p className="text-sm text-red-600">{errors.name.message}</p>
        )}
      </div>

      {/* Date of Birth — read-only in edit mode */}
      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("dob")}
          className="text-sm font-medium text-gray-700"
        >
          Date of Birth
        </label>
        <input
          id={id("dob")}
          type="date"
          className= {fieldClass}
          {...register("dob", {
            required: mode === "add" ? "Date of birth is required" : false,
          })}
        />
        {errors.dob && (
          <p className="text-sm text-red-600">{errors.dob.message}</p>
        )}
      </div>

      {/* Gender — read-only in edit mode */}
      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("gender")}
          className="text-sm font-medium text-gray-700"
        >
          Gender {mode === "add" ? "*" : ""}
        </label>
        <select
          id={id("gender")}
          className= {fieldClass}
          {...register("gender", {
            required: mode === "add" ? "Gender is required" : false,
          })}
        >
          <option value="">Select…</option>
          <option value="Male">Male</option>
          <option value="Female">Female</option>
          <option value="Other">Other</option>
        </select>
        {errors.gender && (
          <p className="text-sm text-red-600">{errors.gender.message}</p>
        )}
      </div>

      {/* Contact Info */}
      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("contactInfo")}
          className="text-sm font-medium text-gray-700"
        >
          Contact Info
        </label>
        <input
          id={id("contactInfo")}
          type="text"
          placeholder="Enter contact info"
          className={fieldClass}
          {...register("contactInfo")}
        />
      </div>

      {/* Street */}
      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("street")}
          className="text-sm font-medium text-gray-700"
        >
          Street
        </label>
        <input
          id={id("street")}
          type="text"
          placeholder="Enter street"
          className={fieldClass}
          {...register("street")}
        />
      </div>

      {/* Area */}
      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("area")}
          className="text-sm font-medium text-gray-700"
        >
          Area / Neighborhood
        </label>
        <input
          id={id("area")}
          type="text"
          placeholder="Enter area"
          className={fieldClass}
          {...register("area")}
        />
      </div>

      {/* City */}
      <div className="flex flex-col gap-1">
        <label
          htmlFor={id("city")}
          className="text-sm font-medium text-gray-700"
        >
          City
        </label>
        <input
          id={id("city")}
          type="text"
          placeholder="Enter city"
          className={fieldClass}
          {...register("city")}
        />
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor={id("status")} className="text-sm font-medium text-gray-700">
          Status
        </label>
        <select
          id={id("status")}
          className={fieldClass}
          {...register("status")}
        >
          <option value="Active">Active</option>
          <option value="Inactive">Inactive</option>
        </select>
      </div>
    </div>
  );
}
