import { Link, useNavigate } from "react-router-dom";
import { FilterSelect } from "@/ui";
import { quickActions } from "../quickActions";
import styles from "./QuickActions.module.css";

const SHOWN = 3;

// The shortcuts to the pages that own each record, as pills. The first is the main one, drawn dark. The rest
// wait under "More".
export function QuickActions() {
  const navigate = useNavigate();
  const more = quickActions.slice(SHOWN);
  return (
    <nav aria-label="Quick actions" className={styles.row}>
      {quickActions.slice(0, SHOWN).map((a) => (
        <Link
          key={a.key}
          to={a.to}
          className={`${styles.pill} ${a.primary ? styles.first : ""}`}
        >
          {a.label}
        </Link>
      ))}
      <FilterSelect
        quiet
        label="More"
        value=""
        options={[
          { value: "", label: "More" },
          ...more.map((a) => ({ value: a.to, label: a.label })),
        ]}
        onChange={(to) => to && navigate(to)}
      />
    </nav>
  );
}
