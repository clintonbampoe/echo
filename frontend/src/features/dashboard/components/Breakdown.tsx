import { useState } from "react";
import { Donut } from "./Donut";
import styles from "./Breakdown.module.css";

// One slice of the ring: its size, and how it reads in the list.
export type BreakdownSlice = {
  key: string;
  name: string;
  value: number;
  // The amount as shown: "GH₵ 21,600".
  text: string;
  percent: number;
  color: string;
};

type Props = {
  // Names the ring for screen readers.
  label: string;
  // The whole, as shown, and what it is called: "Total income".
  total: string;
  totalLabel: string;
  // A narrow card puts the total above the ring with this line under it. Without it, nothing goes above.
  note?: string;
  slices: readonly BreakdownSlice[];
  ready: boolean;
  // What the list says when there is nothing to show.
  empty: string;
  // A tall card with the legend under a larger ring, as the desktop Projects card is drawn.
  stacked?: boolean;
};

const findSlice = (
  slices: readonly BreakdownSlice[],
  key: string | null,
): BreakdownSlice | null =>
  key ? (slices.find((s) => s.key === key) ?? null) : null;

const toggle = (current: string | null, key: string) =>
  current === key ? null : key;

// The line under the total: what a picked slice is, or what a hovered one would add.
const lineFor = (
  chosen: BreakdownSlice | null,
  read: BreakdownSlice | null,
  note: string | undefined,
  totalLabel: string,
) => {
  if (chosen) return `${chosen.percent}% of ${totalLabel.toLowerCase()}`;
  if (read) return `${read.name} · ${read.text}`;
  return note;
};

function Figure({
  total,
  text,
  line,
}: {
  total: string;
  text: string | null;
  line?: string;
}) {
  return (
    <div className={styles.figure}>
      <p className={styles.big}>{text ?? total}</p>
      {line && <p className={styles.note}>{line}</p>}
    </div>
  );
}

type LegendProps = {
  total: string;
  totalLabel: string;
  chosen: BreakdownSlice | null;
  read: BreakdownSlice | null;
  picked: string | null;
  slices: readonly BreakdownSlice[];
  empty: string;
  showEmpty: boolean;
  onHover: (key: string | null) => void;
  onPick: (key: string) => void;
};

type RingProps = {
  label: string;
  slices: readonly BreakdownSlice[];
  read: BreakdownSlice | null;
  big: boolean;
  onHover: (key: string | null) => void;
  onPick: (key: string) => void;
};

function Ring({ label, slices, read, big, onHover, onPick }: RingProps) {
  return (
    <div className={styles.ring}>
      <Donut
        label={label}
        slices={slices.map((s) => ({
          key: s.key,
          value: s.value,
          color: s.color,
        }))}
        active={read?.key ?? null}
        onHover={onHover}
        onPick={onPick}
        big={big}
      />
      {read && (
        <span className={styles.center} aria-hidden="true">
          {read.percent}%
        </span>
      )}
    </div>
  );
}

function Legend({
  total,
  totalLabel,
  chosen,
  read,
  picked,
  slices,
  empty,
  showEmpty,
  onHover,
  onPick,
}: LegendProps) {
  return (
    <div className={styles.legend}>
      <p className={styles.tot}>
        <span>{chosen?.name ?? totalLabel}</span>
        <strong>{chosen?.text ?? total}</strong>
      </p>
      {showEmpty ? (
        <p className={styles.empty}>{empty}</p>
      ) : (
        <ul className={styles.rows}>
          {slices.map((s) => (
            <li key={s.key}>
              <button
                type="button"
                className={`${styles.item} ${read?.key === s.key ? styles.on : ""}`}
                aria-pressed={picked === s.key}
                onClick={() => onPick(s.key)}
                onPointerEnter={() => onHover(s.key)}
                onPointerLeave={() => onHover(null)}
                onFocus={() => onHover(s.key)}
                onBlur={() => onHover(null)}
              >
                <i
                  className={styles.dot}
                  style={{ background: s.color }}
                  aria-hidden="true"
                />
                <span className={styles.name} title={s.name}>
                  {s.name}
                </span>
                <span className={styles.amount}>{s.text}</span>
                <span className={styles.percent}>{s.percent}%</span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

// A ring and the list that explains it. A narrow card shows each name with its percent; a wide one adds the
// amount, and puts the total at the top of the list. A slice can be read and picked: hovering one says what
// it is, and picking one makes the card speak about that slice, the way picking a bar does.
export function Breakdown({
  label,
  total,
  totalLabel,
  note,
  slices,
  ready,
  empty,
  stacked = false,
}: Props) {
  const [picked, setPicked] = useState<string | null>(null);
  const [hover, setHover] = useState<string | null>(null);
  const chosen = findSlice(slices, picked);
  const read = findSlice(slices, hover ?? picked);
  const line = lineFor(chosen, read, note, totalLabel);
  const pick = (key: string) => setPicked((current) => toggle(current, key));

  return (
    <div
      className={`${styles.content} ${stacked ? styles.stacked : ""}`}
      aria-busy={!ready}
    >
      {(stacked || note) && (
        <Figure total={total} text={chosen?.text ?? null} line={line} />
      )}
      <div className={styles.row}>
        <Ring
          label={label}
          slices={slices}
          read={read}
          big={stacked}
          onHover={setHover}
          onPick={pick}
        />
        <Legend
          total={total}
          totalLabel={totalLabel}
          chosen={chosen}
          read={read}
          picked={picked}
          slices={slices}
          empty={empty}
          showEmpty={ready && slices.length === 0}
          onHover={setHover}
          onPick={pick}
        />
      </div>
    </div>
  );
}
