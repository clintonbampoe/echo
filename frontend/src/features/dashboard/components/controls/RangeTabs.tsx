import { FilterSelect } from "@/ui";

// One range a chart card can show.
export type RangeChoice<V extends string> = { value: V; label: string };

type Props<V extends string> = {
  // What the pill picks, for a screen reader: "Range".
  label: string;
  value: V;
  options: readonly RangeChoice<V>[];
  onChange: (value: V) => void;
};

// The range every chart card carries, as the boards draw it: a dashed pill that opens the choices.
export function RangeTabs<V extends string>({
  label,
  value,
  options,
  onChange,
}: Props<V>) {
  return (
    <FilterSelect
      quiet
      label={label}
      value={value}
      options={options}
      onChange={(v) => {
        const next = options.find((o) => o.value === v);
        if (next) onChange(next.value);
      }}
    />
  );
}
