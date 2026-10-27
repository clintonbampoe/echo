import { Link, useNavigate } from "react-router-dom";
import { FilterSelect, useMedia } from "@/ui";
import { quickActions } from "../../model/quickActions";
import styles from "./QuickActions.module.css";

const SHOWN = 3;
const SHOWN_ON_PHONE = 2;

// The shortcuts to the pages that own each record, as pills. The first is the main one, drawn dark. The rest
// wait under "More"; a phone shows one fewer, so the pills and More stay on one line.
export function QuickActions() {
  const navigate = useNavigate();
  const phone = useMedia("(max-width: 700px)");
  const shown = phone ? SHOWN_ON_PHONE : SHOWN;
  const more = quickActions.slice(shown);
  return (
    <nav aria-label="Quick actions" className={styles.row}>
      {quickActions.slice(0, shown).map((a) => (
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
