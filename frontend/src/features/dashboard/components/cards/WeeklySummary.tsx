import { useEffect, useId, useState } from "react";
import { createPortal } from "react-dom";
import { formatDayMonth, formatNumber } from "@/lib/format";
import { Button, FilterSelect, Icon } from "@/ui";
import { useWeek } from "../../hooks/money";
import {
  weekDates,
  weekFigures,
  weekMessage,
  weekOptions,
  signed,
  type WeekFigures,
  type Row,
  type WeekKey,
} from "../../model/weekly";
import { weekImage } from "../../model/shareImage";
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
  const [week, setWeek] = useState<WeekKey>("this");
  const { from, to } = weekDates(week, today);
  const data = useWeek(from, to);
  const titleId = useId();
  const figures =
    data.categories && data.tithes && data.gifts !== undefined
      ? weekFigures(data.categories, data.tithes, data.gifts)
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
              setWeek(v === "last" ? "last" : "this");
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
        <h2 id={titleId} className={styles.hint} aria-busy={!figures}>
          {data.failed
            ? "We could not load this week. Try again in a moment."
            : figures
              ? ""
              : "Adding up the week…"}
        </h2>
        {figures && <Money figures={figures} />}
        <Actions
          message={message}
          make={() =>
            figures
              ? weekImage(
                  week === "this" ? "This week" : "Last week",
                  `${formatDayMonth(from)} to ${formatDayMonth(to)}`,
                  figures,
                )
              : Promise.resolve(null)
          }
        />
      </div>
    </div>,
    document.body,
  );
}

function Lines({
  title,
  total,
  rows,
}: {
  title: string;
  total: number;
  rows: Row[];
}) {
  return (
    <section className={styles.group}>
      <div className={styles.groupHead}>
        <h3>{title}</h3>
        <span>{formatNumber(total)}</span>
      </div>
      <ul className={styles.lines}>
        {rows.map((r) => (
          <li key={r.name}>
            <span className={styles.name}>
              {r.name}
              {r.note && <em>{r.note}</em>}
            </span>
            <span>{formatNumber(r.amount)}</span>
          </li>
        ))}
      </ul>
    </section>
  );
}

// Net first, as the one figure that matters, then where it came in and where it went.
function Money({ figures }: { figures: WeekFigures }) {
  return (
    <>
      <div className={styles.hero}>
        <p className={styles.heroLabel}>Net</p>
        <p className={styles.heroValue} data-negative={figures.net < 0}>
          <small>GH₵</small>
          {signed(figures.net)}
        </p>
      </div>
      <Lines title="Came in" total={figures.came} rows={figures.cameRows} />
      <Lines title="Went out" total={figures.spent} rows={figures.spentRows} />
    </>
  );
}

// Share sends a clean picture where the phone can share files, the words where it can only share text; Copy message
// always works. The message changes with the week, so the "Copied" mark is the one for the message on show.
function Actions({
  message,
  make,
}: {
  message: string;
  make: () => Promise<Blob | null>;
}) {
  const [copied, setCopied] = useState("");
  const canShare = typeof navigator !== "undefined" && "share" in navigator;

  async function share() {
    try {
      const blob = await make();
      const file =
        blob && new File([blob], "weekly-summary.png", { type: "image/png" });
      if (file && navigator.canShare?.({ files: [file] }))
        await navigator.share({ files: [file], text: message });
      else await navigator.share({ text: message });
    } catch {
      // The share sheet was closed, or nothing could be shared: nothing to do.
    }
  }

  return (
    <div className={styles.actions}>
      {canShare && (
        <Button block disabled={!message} onClick={() => void share()}>
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
