import { useId } from "react";
import { Shell, type Shared } from "./FieldShell";
import { Listbox } from "./Listbox";
import type { ControlProps } from "./selectTypes";
import { SheetSelect } from "../overlays/SheetSelect";
import { useFinePointer } from "../hooks/useFinePointer";

type Props = Shared &
  Pick<
    ControlProps,
    "options" | "value" | "onChange" | "onBlur" | "disabled" | "placeholder"
  >;

// A field whose value comes from a fixed list (design rule S2). Always our own picker, never the browser's:
// a menu under the field with a mouse or trackpad, a bottom sheet on a touch screen (design rule S10).
export function SelectField({
  label,
  hint,
  error,
  aside,
  optional,
  ...rest
}: Props) {
  const id = useId();
  const control = {
    ...rest,
    id,
    label,
    invalid: Boolean(error),
    describedBy: error || hint ? `${id}-msg` : undefined,
  };
  return (
    <Shell
      id={id}
      label={label}
      hint={hint}
      error={error}
      aside={aside}
      optional={optional}
    >
      {useFinePointer() ? (
        <Listbox {...control} />
      ) : (
        <SheetSelect {...control} />
      )}
    </Shell>
  );
}
