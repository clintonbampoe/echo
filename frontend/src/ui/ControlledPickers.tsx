import {
  Controller,
  type Control,
  type FieldValues,
  type Path,
} from "react-hook-form";
import { DateField } from "./DateField";
import type { Shared } from "./FieldShell";
import { TimeField } from "./TimeField";

type Base<T extends FieldValues> = Pick<Shared, "hint" | "optional"> & {
  control: Control<T>;
  name: Path<T>;
  label: string;
};

type DateProps<T extends FieldValues> = Base<T> & {
  min?: string;
  max?: string;
  kind?: "birth";
  // Told the day each time one is picked, for a form that fills in another field from it.
  onPick?: (day: string) => void;
};

// A day, wired to a react-hook-form field. Like ControlledSelect: the picker is a controlled field.
export function ControlledDate<T extends FieldValues>({
  control,
  name,
  label,
  onPick,
  ...rest
}: DateProps<T>) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <DateField
          {...rest}
          label={label}
          value={field.value ?? ""}
          onChange={(day) => {
            field.onChange(day);
            onPick?.(day);
          }}
          onBlur={field.onBlur}
          error={fieldState.error?.message}
        />
      )}
    />
  );
}

// A time of day, wired to a react-hook-form field.
export function ControlledTime<T extends FieldValues>({
  control,
  name,
  label,
  ...rest
}: Base<T>) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <TimeField
          {...rest}
          label={label}
          value={field.value ?? ""}
          onChange={field.onChange}
          onBlur={field.onBlur}
          error={fieldState.error?.message}
        />
      )}
    />
  );
}
