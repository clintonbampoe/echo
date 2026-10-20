import { SegmentedField, useMedia } from "@/ui";

// One range a chart card can show: the words for a wide screen, the short ones where room is short.
export type RangeChoice<V extends string> = {
  value: V;
  label: string;
  short: string;
};

type Props<V extends string> = {
  // What the switch picks, for a screen reader: "Range".
  label: string;
  value: V;
  options: readonly RangeChoice<V>[];
  onChange: (value: V) => void;
};

// The range switch every chart card carries, as the boards draw it on the Attendance card: the ranges in
// one joined bar with the picked one filled. The cards share it, so a range looks and behaves the same
// wherever it is changed.
//
// A tight row gets the short words ("8 wks"), and a phone keeps the two nearest ranges, as the phone
// board does: three would push the card's title onto a second line. The range picked is always shown,
// so shrinking a window never hides the one in use.
export function RangeTabs<V extends string>({
  label,
  value,
  options,
  onChange,
}: Props<V>) {
  const tight = useMedia("(max-width: 1180px)");
  const phone = useMedia("(max-width: 700px)");
  const shown =
    phone && options.length > 2
      ? options.filter((o, i) => i < 2 || o.value === value)
      : options;
  return (
    <SegmentedField
      joined
      hideLabel
      compact={phone}
      label={label}
      value={value}
      onChange={onChange}
      options={shown.map((o) => ({
        value: o.value,
        label: tight ? o.short : o.label,
      }))}
    />
  );
}
