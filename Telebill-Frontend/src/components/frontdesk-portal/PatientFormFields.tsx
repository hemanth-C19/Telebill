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
  const today = new Date().toISOString().split("T")[0];

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
          {...register("name", {
            required: "Full name is required",
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
          max={today}
          {...register("dob", {
            required: mode === "add" ? "Date of birth is required" : false,
            validate: (v) => !v || v <= today || "Date of birth cannot be in the future",
          })}
        />
        {errors.dob && (
          <p className="text-sm text-red-600">{errors.dob.message}</p>
        )}
      </div>

      {/* Gender */}
      <div className="flex flex-col gap-2">
        <span className="text-sm font-medium text-gray-700">
          Gender {mode === "add" ? "*" : ""}
        </span>
        <div className="flex gap-6">
          {["Male", "Female"].map((g) => (
            <label key={g} className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                value={g}
                className="accent-blue-600"
                {...register("gender", {
                  required: mode === "add" ? "Gender is required" : false,
                })}
              />
              <span className="text-sm text-gray-700">{g}</span>
            </label>
          ))}
        </div>
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
          {...register("contactInfo", {
            setValueAs: (v: string) => v.trim(),
            pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: "Enter a valid email address" },
          })}
        />
        {errors.contactInfo && (
          <p className="text-sm text-red-600">{errors.contactInfo.message}</p>
        )}
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
