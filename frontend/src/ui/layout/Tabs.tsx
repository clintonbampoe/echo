import type { KeyboardEvent } from "react";
import { useFlip } from "../motion/useFlip";
import { useSlidingThumb } from "../motion/useSlidingThumb";
import styles from "./Tabs.module.css";

export type TabItem = {
  id: string;
  label: string;
  count?: number | string | null;
};

type Props = {
  // Names the tab list for screen readers.
  label: string;
  tabs: TabItem[];
  value: string;
  onChange: (id: string) => void;
  // Prefix for the ids that tie each tab to its panel: tab `${idPrefix}-tab-x`, panel `${idPrefix}-panel-x`.
  idPrefix: string;
};

// A row of pill tabs. The picked one is a dark pill that slides to wherever you tap and squishes as it lands.
// Left and right arrows move between them; the caller shows the matching panel, giving it role="tabpanel",
// id `${idPrefix}-panel-${id}` and aria-labelledby `${idPrefix}-tab-${id}`.
export function Tabs({ label, tabs, value, onChange, idPrefix }: Props) {
  const row = useSlidingThumb(value);
  const flip = useFlip(value);

  function onKeyDown(e: KeyboardEvent) {
    const at = tabs.findIndex((t) => t.id === value);
    const next = {
      ArrowRight: at + 1,
      ArrowLeft: at - 1,
      Home: 0,
      End: tabs.length - 1,
    }[e.key];
    if (next === undefined) return;
    e.preventDefault();
    const tab = tabs[(next + tabs.length) % tabs.length];
    if (!tab) return;
    onChange(tab.id);
    document.getElementById(`${idPrefix}-tab-${tab.id}`)?.focus();
  }

  return (
    <div className={styles.scroller}>
      <div
        ref={row}
        role="tablist"
        aria-label={label}
        className={styles.list}
        onKeyDown={onKeyDown}
      >
        <span className={styles.slot} aria-hidden="true">
          <span className={styles.thumb} data-flip={flip} />
        </span>
        {tabs.map((t) => {
          const selected = t.id === value;
          return (
            <button
              key={t.id}
              id={`${idPrefix}-tab-${t.id}`}
              type="button"
              role="tab"
              aria-selected={selected}
              aria-controls={`${idPrefix}-panel-${t.id}`}
              data-selected={selected}
              tabIndex={selected ? 0 : -1}
              className={`${styles.tab} ${selected ? styles.selected : ""}`}
              onClick={() => onChange(t.id)}
            >
              {t.label}
              {t.count != null && (
                <span className={styles.count}>{t.count}</span>
              )}
            </button>
          );
        })}
      </div>
    </div>
  );
}
