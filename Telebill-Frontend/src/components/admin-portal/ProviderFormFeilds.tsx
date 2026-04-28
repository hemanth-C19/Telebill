import type { FieldErrors, UseFormRegister } from "react-hook-form";
import type { ProviderFormValues } from "../../types/admin.types";

export type ProviderFormFieldsProps = {
  mode: "add" | "edit";
  register: UseFormRegister<ProviderFormValues>;
  errors: FieldErrors<ProviderFormValues>;
  fieldClass: string;
};

export function ProviderFormFields({
  mode,
  register,
  errors,
  fieldClass,
}: ProviderFormFieldsProps) {
  const id = (name: string) => `${mode}-${name}`;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

      <div className="flex flex-col gap-1">
        <label htmlFor={id("name")} className="text-sm font-medium text-gray-700">
          Name *
        </label>
        <input
          id={id("name")}
          type="text"
          placeholder="Enter provider name"
          className={fieldClass}
          {...register("name", {
            required: "Name is required",
            setValueAs: (v: string) => v.trim(),
            minLength: { value: 4, message: "Name must be at least 4 characters" },
            pattern: {
              value: /^[A-Za-z\s'-]+$/,
              message: "Name must not contain numbers or special characters",
            },
          })}
        />
        {errors.name && (
          <p className="text-sm text-red-600">{errors.name.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor={id("npi")} className="text-sm font-medium text-gray-700">
          NPI *
        </label>
        <input
          id={id("npi")}
          type="text"
          placeholder="10-digit NPI"
          className={fieldClass}
          {...register("npi", {
            required: "NPI is required",
            setValueAs: (v: string) => v.trim(),
            pattern: { value: /^\d{10}$/, message: "NPI must be exactly 10 digits" },
          })}
        />
        {errors.npi && (
          <p className="text-sm text-red-600">{errors.npi.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor={id("taxonomy")} className="text-sm font-medium text-gray-700">
          Specialty / Taxonomy
        </label>
        <input
          id={id("taxonomy")}
          type="text"
          placeholder="Enter taxonomy"
          className={fieldClass}
          {...register("taxonomy", {
            setValueAs: (v: string) => v.trim(),
            minLength: { value: 5, message: "Taxonomy must be at least 5 characters" },
          })}
        />
        {errors.taxonomy && (
          <p className="text-sm text-red-600">{errors.taxonomy.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor={id("contactInfo")} className="text-sm font-medium text-gray-700">
          Contact Info
        </label>
        <input
          id={id("contactInfo")}
          type="text"
          placeholder="Enter contact email"
          className={fieldClass}
          {...register("contactInfo", {
            setValueAs: (v: string) => v.trim(),
            pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: "Enter a valid email address" },
          })}
        />
        {errors.contactInfo && (
          <p className="text-sm text-red-600">{errors.contactInfo.message}</p>
        )}
      </div>

      <div className="flex items-center gap-2">
        <input
          id={id("telehealthEnrolled")}
          type="checkbox"
          {...register("telehealthEnrolled")}
        />
        <label
          htmlFor={id("telehealthEnrolled")}
          className="text-sm font-medium text-gray-700"
        >
          Telehealth Enrolled
        </label>
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