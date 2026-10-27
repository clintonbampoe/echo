import {
  useId,
  useState,
  type CSSProperties,
  type FocusEvent,
  type ReactNode,
} from "react";
import { SearchField } from "./SearchField";
import styles from "./Lookup.module.css";

// What a lookup lists: a name, a line under it, and a mark at the right when it cannot be picked.
export type LookupItem = {
  id: string;
  label: string;
  sub?: string;
  disabled?: boolean;
  note?: string;
};

type Props<T extends LookupItem> = {
  // Names the box for screen readers, e.g. "Search members".
  label: string;
  placeholder: string;
  text: string;
  onText: (text: string) => void;
  // idle: nothing asked yet. searching: asked, waiting. ready: answered (maybe with nothing). error: could not ask.
  status: "idle" | "searching" | "ready" | "error";
  items: T[];
  // Shown when the box is focused and empty.
  recent?: T[];
  // Fewer letters than this are not searched.
  minChars?: number;
  // Said when the answer was empty and when the search failed.
  noMatch: string;
  failed: string;
  // What stands at the left of an item (an avatar).
  lead: (item: T) => ReactNode;
  onPick: (item: T) => void;
  // A problem with the choice itself (nothing chosen yet), shown under the box.
  error?: string;
};

// A box that finds one thing among many as you type. Its answers open as a glass popover under the box, which knows
// its moments: focused and empty ("Recent"), searching, results, no match, error. The caller does the asking
// (debounced) and passes the state in, so the same box serves members, groups and anything else with a name.
export function Lookup<T extends LookupItem>({
  label,
  placeholder,
  text,
  onText,
  status,
  items,
  recent = [],
  minChars,
  noMatch,
  failed,
  lead,
  onPick,
  error,
}: Props<T>) {
  const [focused, setFocused] = useState(false);
  const id = useId();
  const typed = text.trim();

  function onBlur(e: FocusEvent<HTMLDivElement>) {
    if (!(
      e.relatedTarget instanceof Node &&
      e.currentTarget.contains(e.relatedTarget)
    ))
      setFocused(false);
  }

  return (
    <div
      className={styles.wrap}
      onFocus={() => setFocused(true)}
      onBlur={onBlur}
      onKeyDown={(e) => e.key === "Escape" && focused && setFocused(false)}
    >
      <SearchField
        label={label}
        placeholder={placeholder}
        value={text}
        aria-describedby={error ? `${id}-error` : undefined}
        onChange={(e) => onText(e.target.value)}
      />
      {error && (
        <p id={`${id}-error`} role="alert" className={styles.error}>
          {error}
        </p>
      )}
      {focused && (
        <Panel
          typed={typed}
          minChars={minChars}
          status={status}
          items={items}
          recent={recent}
          noMatch={noMatch}
          failed={failed}
          lead={lead}
          onPick={onPick}
        />
      )}
    </div>
  );
}

type PanelProps<T extends LookupItem> = Pick<
  Props<T>,
  "status" | "items" | "noMatch" | "failed" | "lead" | "onPick" | "minChars"
> & { typed: string; recent: T[] };

// The glass popover under the box: the line that says where the search is, and the items under it.
function Panel<T extends LookupItem>({
  typed,
  minChars = 2,
  status,
  items,
  recent,
  noMatch,
  failed,
  lead,
  onPick,
}: PanelProps<T>) {
  const short = typed.length < minChars;
  const shown = typed === "" ? recent : short ? [] : items;
  const listed =
    shown.length > 0 && (status === "ready" || typed === "" || short);
  return (
    // The mouse press is kept off the box's blur, so focus stays in the box while an item is pressed.
    <div
      className={`${styles.panel} glass-pop glass-in-pop`}
      aria-live="polite"
      onMouseDown={(e) => e.preventDefault()}
    >
      <Body
        typed={typed}
        short={short}
        minChars={minChars}
        status={status}
        noMatch={noMatch}
        failed={failed}
        hasRecent={recent.length > 0}
        count={items.length}
      />
      {listed && (
        <ul
          aria-label={typed === "" ? "Recent" : "Matches"}
          className={styles.list}
        >
          {shown.map((item, i) => (
            <li key={item.id}>
              <button
                type="button"
                className={`${styles.option} glass-opt`}
                style={{ "--i": Math.min(i, 8) } as CSSProperties}
                disabled={item.disabled}
                onClick={() => onPick(item)}
              >
                {lead(item)}
                <span className={styles.who}>
                  <span className={styles.name}>{item.label}</span>
                  {item.sub && <span className={styles.sub}>{item.sub}</span>}
                </span>
                {item.note && <span className={styles.taken}>{item.note}</span>}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

type BodyProps = {
  typed: string;
  short: boolean;
  minChars: number;
  status: Props<LookupItem>["status"];
  noMatch: string;
  failed: string;
  hasRecent: boolean;
  count: number;
};

// The line above the answers: a heading, a hint, "Searching…" with its dots, or why there is nothing to list.
function Body({
  typed,
  short,
  minChars,
  status,
  noMatch,
  failed,
  hasRecent,
  count,
}: BodyProps) {
  if (typed === "")
    return hasRecent ? (
      <p className={styles.heading}>Recent</p>
    ) : (
      <p className={styles.note}>
        Type at least {words[minChars] ?? minChars} letters.
      </p>
    );
  if (short)
    return (
      <p className={styles.note}>
        Type at least {words[minChars] ?? minChars} letters.
      </p>
    );
  if (status === "searching" || status === "idle")
    return (
      <p className={styles.note}>
        <span className="liquid-load" aria-hidden="true">
          <i />
          <i />
          <i />
        </span>{" "}
        Searching…
      </p>
    );
  if (status === "error") return <p className={styles.note}>{failed}</p>;
  // Said only when nothing came back; the matches themselves are the list under it.
  return count === 0 ? <p className={styles.note}>{noMatch}</p> : null;
}

const words: Record<number, string> = { 2: "two", 3: "three" };
