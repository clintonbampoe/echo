import { useEffect, useId, useState } from "react";
import { createPortal } from "react-dom";
import { formatNumber } from "@/lib/format";
import { Button, FilterSelect, Icon } from "@/ui";
import { useWeek } from "../../hooks/money";
import {
  weekDates,
  weekFigures,
  weekMessage,
  weekOptions,
  type WeekFigures,
  type WeekKey,
} from "../../model/weekly";
import styles from "./WeeklySummary.module.css";

// The "Summary" pill on the Money card, and the dialog it opens.
export function SummaryButton({ today }: { today: string }) {
  const [open, setOpen] = useState(false);
  return (
    <>
      <button
        type="button"
        className={styles.pill}
        onClick={() => setOpen(true)}
      >
        <Icon name="reports" size={14} />
        Summary
      </button>
      {open && <WeeklySummary today={today} onClose={() => setOpen(false)} />}
    </>
  );
}

type Props = { today: string; onClose: () => void };

// "Summary" on the Money card: a short message about the week, in words that can be sent to the congregation, with
// the three numbers under it. Share hands it to the phone's share sheet where there is one; Copy message always works.
export function WeeklySummary({ today, onClose }: Props) {
  const [week, setWeek] = useState<WeekKey>("last");
  const { from, to } = weekDates(week, today);
  const data = useWeek(from, to);
  const titleId = useId();
  const figures = data.categories
    ? weekFigures(data.categories, data.present ?? 0)
    : undefined;
  const message = figures ? weekMessage(week, figures) : "";

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [onClose]);

  return createPortal(
    <div className={styles.root}>
      <div
        className={`${styles.scrim} glass-scrim`}
        onClick={onClose}
        aria-hidden="true"
      />
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className={`${styles.panel} glass-panel glass-in-centre`}
      >
        <header className={styles.head}>
          <FilterSelect
            label="Week"
            options={weekOptions}
            value={week}
            quiet
            onChange={(v) => {
              setWeek(v === "this" ? "this" : "last");
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
        {figures && <Numbers figures={figures} />}
        <Actions message={message} />
      </div>
    </div>,
    document.body,
  );
}

function Numbers({ figures }: { figures: WeekFigures }) {
  const numbers = [
    { label: "Came in", value: figures.received },
    { label: "Went out", value: figures.spent },
    { label: "Left over", value: figures.received - figures.spent },
  ];
  return (
    <dl className={styles.numbers}>
      {numbers.map((n) => (
        <div key={n.label}>
          <dt>{n.label}</dt>
          <dd>{formatNumber(n.value)}</dd>
        </div>
      ))}
    </dl>
  );
}

// Share where the device has a share sheet; Copy message everywhere. The message changes with the week, so the
// "Copied" mark is the one for the message on show.
function Actions({ message }: { message: string }) {
  const [copied, setCopied] = useState("");
  const canShare = typeof navigator !== "undefined" && "share" in navigator;
  return (
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
        onClick={() => {
          void navigator.clipboard?.writeText(message);
          setCopied(message);
        }}
      >
        {copied === message ? "Copied" : "Copy message"}
      </Button>
    </div>
  );
}
