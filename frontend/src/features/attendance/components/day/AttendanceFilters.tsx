import { FilterSelect } from "@/ui";
import type { SessionLabel } from "../../api/sessionLabels";
import { periodOptions, type Period } from "../../model/display";
import styles from "./AttendanceFilters.module.css";

type Props = {
  labels: SessionLabel[];
  type: string;
  period: Period;
  onType: (type: string) => void;
  onPeriod: (period: Period) => void;
};

// Two pills above the list: which kind of session, and how far back.
export function AttendanceFilters({
  labels,
  type,
  period,
  onType,
  onPeriod,
}: Props) {
  const types = [
    { value: "", label: "All types" },
    ...labels.map((l) => ({ value: String(l.id), label: l.name })),
  ];
  return (
    <div className={styles.filters}>
      <FilterSelect
        label="Type"
        options={types}
        value={type}
        onChange={onType}
      />
      <FilterSelect
        label="Period"
        options={periodOptions}
        value={period}
        onChange={(v) => onPeriod(v as Period)}
        quiet
      />
    </div>
  );
}
