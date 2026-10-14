import {
  Controller,
  type Control,
  type FieldValues,
  type Path,
} from "react-hook-form";
import { SelectField } from "./SelectField";
import type { Option } from "./selectTypes";

type Props<T extends FieldValues> = {
  control: Control<T>;
  name: Path<T>;
  label: string;
  options: readonly Option[];
};

// A choice from a fixed list, wired to a react-hook-form field. SelectField is a controlled field, so
// it goes through Controller; every form with a dropdown would otherwise repeat this.
export function ControlledSelect<T extends FieldValues>({
  control,
  name,
  label,
  options,
}: Props<T>) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <SelectField
          label={label}
          options={options}
          error={fieldState.error?.message}
          {...field}
        />
      )}
    />
  );
}
