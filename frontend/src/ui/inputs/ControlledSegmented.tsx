import {
  Controller,
  type Control,
  type FieldValues,
  type Path,
} from "react-hook-form";
import { SegmentedField } from "./SegmentedField";
import type { Option } from "./selectTypes";

type Props<T extends FieldValues> = {
  control: Control<T>;
  name: Path<T>;
  label: string;
  options: readonly Option[];
};

// A short choice shown as pills, wired to a react-hook-form field. Fills the width of its row, and says what is
// wrong underneath when the form is submitted with nothing picked.
export function ControlledSegmented<T extends FieldValues>({
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
        <SegmentedField
          label={label}
          options={options}
          value={field.value}
          onChange={field.onChange}
          error={fieldState.error?.message}
          stretch
        />
      )}
    />
  );
}
