import { useState, type CSSProperties } from "react";
import type { UserRole } from "@/features/account";
import { Donut, ListHeader } from "@/ui";
import { formatNumber } from "@/lib/format";
import type { StaffSummary } from "../api/staff";
import { ringCentre, shareOf } from "../model/figures";
import { roleParts } from "../model/roles";
import styles from "./RoleOverview.module.css";

// Roles in the ring, and the key beside it. Hover or pick a role and the middle shows its head count instead of the
// total; pick it again (or press the middle) to go back. The key is the real content, so the ring is hidden from screen readers.
export function RoleOverview({
  summary,
}: {
  summary: StaffSummary | undefined;
}) {
  const [hot, setHot] = useState<string | null>(null);
  const [picked, setPicked] = useState<string | null>(null);
  const togglePick = (key: string) =>
    setPicked((p) => (p === key ? null : key));

  return (
    <section className={styles.card} aria-label="Roles">
      <ListHeader
        title="Roles"
        summary={summary ? `${formatNumber(summary.total)} people` : ""}
      />
      {summary ? (
        <Ring
          summary={summary}
          hot={hot}
          picked={picked}
          onHot={setHot}
          onPick={togglePick}
          onClear={() => setPicked(null)}
        />
      ) : (
        <div className={styles.body} aria-hidden="true">
          <span className={`skeleton ${styles.ghostRing}`} />
          <span className={`skeleton ${styles.ghostKey}`} />
        </div>
      )}
    </section>
  );
}

type RingProps = {
  summary: StaffSummary;
  hot: string | null;
  picked: string | null;
  onHot: (key: string | null) => void;
  onPick: (key: string) => void;
  onClear: () => void;
};

function Ring({ summary, hot, picked, onHot, onPick, onClear }: RingProps) {
  const parts = roleParts(summary.roles);
  const drawn = parts.filter((p) => p.value > 0).map((p) => p.key);
  const active = (hot ?? picked) as UserRole | null;
  const centre = ringCentre(summary, active);

  if (drawn.length === 0)
    return <p className={styles.empty}>Nobody has joined yet.</p>;

  return (
    <div className={styles.body}>
      <Donut
        parts={parts}
        hot={hot}
        picked={picked}
        onHot={onHot}
        onPick={onPick}
      >
        <CentreFigure
          centre={centre}
          onClear={picked && !hot ? onClear : undefined}
        />
      </Donut>
      <ul className={styles.key} aria-label="People by role">
        {parts.map((p) => {
          const colour = drawn.indexOf(p.key);
          return (
            <li key={p.key}>
              <button
                type="button"
                className={styles.row}
                aria-pressed={p.key === picked}
                data-on={p.key === (hot ?? picked)}
                style={
                  {
                    "--dot":
                      colour < 0
                        ? "var(--line-2)"
                        : `var(--ring-${colour % 4})`,
                  } as CSSProperties
                }
                onPointerEnter={() => onHot(p.key)}
                onPointerLeave={() => onHot(null)}
                onFocus={(e) =>
                  e.target.matches(":focus-visible") && onHot(p.key)
                }
                onBlur={() => onHot(null)}
                onClick={() => onPick(p.key)}
              >
                <i className={styles.dot} />
                <span className={styles.name}>{p.label}</span>
                <span className={styles.count}>
                  {p.text}
                  <span className={styles.word}>
                    {" "}
                    {p.value === 1 ? "person" : "people"}
                  </span>
                </span>
                <span className={styles.share}>
                  {shareOf(p.value, summary.total)}
                </span>
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );
}

type CentreProps = {
  centre: ReturnType<typeof ringCentre>;
  onClear?: () => void;
};

// The number in the hole. While a role is picked and the pointer is elsewhere, pressing it shows everyone again.
function CentreFigure({ centre, onClear }: CentreProps) {
  const inner = (
    <>
      {centre.label && (
        <span className={styles.centreLabel}>{centre.label}</span>
      )}
      <span className={styles.centreValue}>{centre.value}</span>
      <span className={styles.centreNote}>{centre.note}</span>
    </>
  );
  return onClear ? (
    <button
      type="button"
      className={styles.centre}
      aria-label="Show everyone"
      onClick={onClear}
    >
      {inner}
    </button>
  ) : (
    <div className={styles.centre} role="status">
      {inner}
    </div>
  );
}
