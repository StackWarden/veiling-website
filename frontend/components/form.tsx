"use client";

import React from "react";

/* ---------- Types ---------- */

export type FormFieldType =
  | "text"
  | "textarea"
  | "datetime-local"
  | "date"
  | "time"
  | "custom";

type ValuesShape = Record<string, unknown>;

export type FormField<
  TValues extends ValuesShape,
  TName extends keyof TValues = keyof TValues
> = {
  name: TName;
  label?: string;
  type: FormFieldType;

  placeholder?: string;
  required?: boolean;

  colSpan?: 1 | 2;

  minHeightClassName?: string;

  render?: (args: {
    value: TValues[TName];
    values: TValues;
    setValue: <K extends keyof TValues>(name: K, value: TValues[K]) => void;
  }) => React.ReactNode;

  formatValue?: (value: TValues[TName], values: TValues) => string;
  parseValue?: (raw: string, values: TValues) => TValues[TName];
};

type FormProps<TValues extends ValuesShape> = {
  title?: string;

  values: TValues;
  setValues: React.Dispatch<React.SetStateAction<TValues>>;

  fields: Array<FormField<TValues>>;

  onSubmit: (values: TValues) => Promise<void> | void;

  submitting?: boolean;
  submitLabel?: string;

  error?: string | null;

  columns?: 1 | 2;
  className?: string;
};

export default function Form<TValues extends ValuesShape>({
  title,
  values,
  setValues,
  fields,
  onSubmit,
  submitting = false,
  submitLabel = "Submit",
  error = null,
  columns = 2,
  className = "",
}: FormProps<TValues>) {
  const setValue = <K extends keyof TValues>(name: K, value: TValues[K]) => {
    setValues((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    await onSubmit(values);
  };

  const gridColsClass =
    columns === 2 ? "grid-cols-1 md:grid-cols-2" : "grid-cols-1";

  return (
    <div className={`flex justify-center items-start px-6 pt-8 ${className}`}>
      <form onSubmit={handleSubmit} className="w-full max-w-4xl space-y-8">
        {title ? (
          <h1 className="text-3xl font-bold text-center">{title}</h1>
        ) : null}

        <div className={`grid ${gridColsClass} gap-8`}>
          {fields.map((field) => {
            const colSpan = field.colSpan ?? 1;
            const spanClass =
              columns === 2
                ? colSpan === 2
                  ? "md:col-span-2"
                  : ""
                : "";

            const rawValue = values[field.name];
            const displayValue =
              field.formatValue?.(
                rawValue as never,
                values
              ) ?? (rawValue ?? "");

            if (field.type === "custom") {
              if (!field.render) {
                throw new Error(
                  `Field "${String(field.name)}" is type "custom" but has no render()`
                );
              }

              return (
                <div key={String(field.name)} className={`space-y-2 ${spanClass}`}>
                  {field.label ? (
                    <label className="font-semibold">{field.label}</label>
                  ) : null}
                  {field.render({
                    value: rawValue as never,
                    values,
                    setValue,
                  })}
                </div>
              );
            }

            if (field.type === "textarea") {
              return (
                <div key={String(field.name)} className={`flex flex-col ${spanClass}`}>
                  {field.label ? (
                    <label className="font-semibold">{field.label}</label>
                  ) : null}
                  <textarea
                    value={displayValue == null ? "" : String(displayValue)}
                    placeholder={field.placeholder}
                    required={field.required}
                    onChange={(e) => {
                      const parsed = field.parseValue
                        ? field.parseValue(e.target.value, values)
                        : (e.target.value as unknown as typeof rawValue);
                      setValue(field.name, parsed as never);
                    }}
                    className={`mt-1 w-full border rounded-lg p-3 ${
                      field.minHeightClassName ?? "min-h-[100px]"
                    }`}
                  />
                </div>
              );
            }

            return (
              <div key={String(field.name)} className={`flex flex-col ${spanClass}`}>
                {field.label ? (
                  <label className="font-semibold">{field.label}</label>
                ) : null}
                <input
                  type={field.type}
                  value={displayValue == null ? "" : String(displayValue)}
                  placeholder={field.placeholder}
                  required={field.required}
                  onChange={(e) => {
                    const parsed = field.parseValue
                      ? field.parseValue(e.target.value, values)
                      : (e.target.value as unknown as typeof rawValue);
                    setValue(field.name, parsed as never);
                  }}
                  className="mt-1 w-full border rounded-lg p-2"
                />
              </div>
            );
          })}
        </div>

        <div className="flex justify-center">
          <button
            type="submit"
            disabled={submitting}
            className="w-48 bg-[#162218] text-white py-3 rounded-lg font-semibold hover:bg-[#0f1c14]"
          >
            {submitting ? "Posting..." : submitLabel}
          </button>
        </div>

        {error ? (
          <p className="text-red-600 text-center font-semibold">{error}</p>
        ) : null}
      </form>
    </div>
  );
}
