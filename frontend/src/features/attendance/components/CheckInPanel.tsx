import { useRef, useState } from "react";
import { SearchField, SegmentedField } from "@/ui";
import type { CheckIn, PersonKind, Session } from "../api";
import { kindOptions, standing, type Match } from "../matches";
import { useFound } from "../useFound";
import { useMarking, type NewName } from "../useMarking";
import { CameLastTime } from "./CameLastTime";
import styles from "./CheckInPanel.module.css";
import { MatchList } from "./MatchList";
import { NewVisitor } from "./NewVisitor";

type Props = {
  session: Session;
  rows: CheckIn[];
  // Inside a sheet that already has the title and the edge.
  flat?: boolean;
};

// Where people are marked present: pick who is arriving, search, tap. Each person is saved as you go.
export function CheckInPanel({ session, rows, flat }: Props) {
  const [kind, setKind] = useState<PersonKind>("Member");
  const [text, setText] = useState("");
  const input = useRef<HTMLInputElement>(null);
  const marking = useMarking(session.id, () => {
    setText("");
    input.current?.focus();
  });
  const { matches, settled, failed } = useFound(kind, text);

  const now = new Set(marking.marked.map((m) => m.personId));
  const already = new Set([...rows.map((c) => c.personId), ...marking.known]);
  const last = marking.marked[marking.marked.length - 1];

  // Enter marks the first match who is not already here, so a roll can be taken without the mouse.
  function onEnter() {
    const first = matches.find((m) => standing(m.id, now, already) === "none");
    if (first) void marking.mark(first);
  }

  return (
    <aside
      aria-label="Mark present"
      className={flat ? styles.flat : styles.panel}
    >
      <div className={styles.head}>
        {!flat && <h2 className={styles.title}>Mark present</h2>}
        <SegmentedField
          label="Who is arriving"
          hideLabel
          joined
          compact
          options={kindOptions}
          value={kind}
          onChange={(next) => {
            setKind(next);
            setText("");
          }}
        />
      </div>
      <SearchField
        ref={input}
        label={`Search ${kind === "Member" ? "members" : "visitors"}`}
        placeholder={`Find a ${kind === "Member" ? "member" : "visitor"}`}
        className={styles.search}
        value={text}
        onChange={(e) => setText(e.target.value)}
        onKeyDown={(e) => e.key === "Enter" && (e.preventDefault(), onEnter())}
      />
      <div aria-live="polite">
        <Results
          text={text.trim()}
          kind={kind}
          settled={settled}
          failed={failed}
          matches={matches}
          now={now}
          already={already}
          busy={marking.busy}
          onMark={(m) => void marking.mark(m)}
          onAdd={(n) => void marking.addVisitor(n)}
        />
      </div>
      {marking.failed && (
        <p role="alert" className={styles.problem}>
          We could not save that. Try again.
        </p>
      )}
      {text.trim() === "" && (
        <CameLastTime
          session={session}
          kind={kind}
          present={new Set([...already, ...now])}
          busy={marking.busy}
          onMark={(m) => void marking.mark(m)}
        />
      )}
      <p role="status" className={styles.tail}>
        {last
          ? `${marking.marked.length} marked present just now`
          : "No one marked just now"}
        {last && (
          <button
            type="button"
            className={styles.undo}
            disabled={marking.undoing}
            onClick={() => void marking.undo()}
          >
            Undo {last.name}
          </button>
        )}
      </p>
    </aside>
  );
}

type ResultsProps = {
  text: string;
  kind: PersonKind;
  settled: boolean;
  failed: boolean;
  matches: Match[];
  now: ReadonlySet<string>;
  already: ReadonlySet<string>;
  busy: boolean;
  onMark: (match: Match) => void;
  onAdd: (name: NewName) => void;
};

// What sits under the search box: a hint, "Searching…", a problem, nothing found, or the matches.
function Results({
  text,
  kind,
  settled,
  failed,
  matches,
  now,
  already,
  busy,
  onMark,
  onAdd,
}: ResultsProps) {
  if (text.length < 2)
    return <p className={styles.hint}>Start typing a name to find someone.</p>;
  if (failed)
    return (
      <p className={styles.hint}>
        We could not search right now. Try again in a moment.
      </p>
    );
  if (!settled) return <p className={styles.hint}>Searching…</p>;
  if (matches.length > 0)
    return (
      <MatchList
        matches={matches}
        now={now}
        already={already}
        busy={busy}
        onMark={onMark}
      />
    );
  return kind === "Visitor" ? (
    <NewVisitor text={text} busy={busy} onAdd={onAdd} />
  ) : (
    <p className={styles.hint}>
      No member found. Check the spelling, or search by phone number.
    </p>
  );
}
