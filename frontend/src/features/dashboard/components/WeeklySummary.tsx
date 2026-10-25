import { useEffect, useId, useState } from "react";
import { createPortal } from "react-dom";
import { formatNumber } from "@/lib/format";
import { Button, FilterSelect, Icon } from "@/ui";
import { useWeek } from "../hooks";
import {
  weekDates,
  weekFigures,
  weekMessage,
  weekOptions,
  type WeekKey,
} from "../weekly";
import styles from "./WeeklySummary.module.css";

type Props = { today: string; onClose: () => void };

// "Summary" on the Money card: a short message about the week, in words that can be sent to the congregation, with
// the three numbers under it. Share hands it to the phone's share sheet where there is one; Copy message always works.
export function WeeklySummary({ today, onClose }: Props) {
  const [week, setWeek] = useState<WeekKey>("last");
  const [copied, setCopied] = useState(false);
  const { from, to } = weekDates(week, today);
  const data = useWeek(from, to);
  const titleId = useId();
  const figures = data.categories
    ? weekFigures(data.categories, data.present ?? 0)
    : undefined;
  const message = figures ? weekMessage(week, figures) : "";
  const canShare = typeof navigator !== "undefined" && "share" in navigator;

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [onClose]);

  async function copy() {
    await navigator.clipboard?.writeText(message);
    setCopied(true);
  }

  const numbers = figures && [
    { label: "Came in", value: figures.received },
    { label: "Went out", value: figures.spent },
    { label: "Left over", value: figures.received - figures.spent },
  ];
  return createPortal(
    <div className={styles.root}>
      <div className={styles.scrim} onClick={onClose} aria-hidden="true" />
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className={styles.panel}
      >
        <header className={styles.head}>
          <FilterSelect
            label="Week"
            options={weekOptions}
            value={week}
            quiet
            onChange={(v) => {
              setWeek(v === "this" ? "this" : "last");
              setCopied(false);
            }}
          />
          <button
            type="button"
            className={styles.close}
            aria-label="Close"
            onClick={onClose}
          >
            <Icon name="x" size={16} />
          </button>
        </header>
        <h2 id={titleId} className={styles.message} aria-busy={!figures}>
          {data.failed
            ? "We could not load this week. Try again in a moment."
            : message || "Adding up the week…"}
        </h2>
        {numbers && (
          <dl className={styles.numbers}>
            {numbers.map((n) => (
              <div key={n.label}>
                <dt>{n.label}</dt>
                <dd>{formatNumber(n.value)}</dd>
              </div>
            ))}
          </dl>
        )}
        <div className={styles.actions}>
          {canShare && (
            <Button
              block
              disabled={!message}
              onClick={() => void navigator.share({ text: message })}
            >
              Share
            </Button>
          )}
          <Button
            block
            variant={canShare ? "ghost" : "primary"}
            disabled={!message}
            onClick={() => void copy()}
          >
            {copied ? "Copied" : "Copy message"}
          </Button>
        </div>
      </div>
    </div>,
    document.body,
  );
}
