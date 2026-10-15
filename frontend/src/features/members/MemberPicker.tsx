import { useState } from "react";
import { useDebounced } from "@/lib/useDebounced";
import { Avatar, Button, SearchField } from "@/ui";
import type { MemberMatch } from "./api";
import { useMemberSearch } from "./hooks";
import styles from "./MemberPicker.module.css";

type Props = {
  // The person picked so far, if any.
  value: MemberMatch | null;
  onChange: (member: MemberMatch | null) => void;
  // Ids that cannot be picked, with the reason shown beside the name (e.g. already in the group).
  taken?: ReadonlySet<string>;
  takenText?: string;
  error?: string;
};

// Find a member by name or phone and pick one. Once picked, the search gives way to the person with a Change button.
export function MemberPicker({
  value,
  onChange,
  taken,
  takenText = "Already added",
  error,
}: Props) {
  const [text, setText] = useState("");
  if (value) {
    return (
      <div className={styles.picked}>
        <Avatar name={value.name} size={36} />
        <div className={styles.who}>
          <p className={styles.name}>{value.name}</p>
          <p className={styles.phone}>{value.phoneNumber}</p>
        </div>
        <Button variant="secondary" onClick={() => onChange(null)}>
          Change
        </Button>
      </div>
    );
  }
  return (
    <div>
      <SearchField
        label="Search members"
        placeholder="Search by name or phone"
        value={text}
        onChange={(e) => setText(e.target.value)}
      />
      {error && (
        <p role="alert" className={styles.error}>
          {error}
        </p>
      )}
      <Results
        text={text.trim()}
        onPick={onChange}
        taken={taken}
        takenText={takenText}
      />
    </div>
  );
}

type ResultsProps = {
  text: string;
  onPick: (m: MemberMatch) => void;
  taken?: ReadonlySet<string>;
  takenText: string;
};

// What sits under the search box: a hint, "Searching…", a problem, nothing found, or the matches.
function Results({ text, onPick, taken, takenText }: ResultsProps) {
  const q = useDebounced(text);
  const search = useMemberSearch(q);
  return (
    <div aria-live="polite" className={styles.results}>
      <Message
        text={text}
        settled={q === text}
        loading={search.isPending}
        failed={search.isError}
        count={search.data?.length}
      />
      {search.data && q === text && search.data.length > 0 && (
        <ul aria-label="Matching members" className={styles.list}>
          {search.data.map((m) => (
            <li key={m.id}>
              <button
                type="button"
                className={styles.option}
                disabled={taken?.has(m.id)}
                onClick={() => onPick(m)}
              >
                <Avatar name={m.name} size={32} />
                <span className={styles.who}>
                  <span className={styles.name}>{m.name}</span>
                  <span className={styles.phone}>{m.phoneNumber}</span>
                </span>
                {taken?.has(m.id) && (
                  <span className={styles.taken}>{takenText}</span>
                )}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

type MessageProps = {
  text: string;
  settled: boolean;
  loading: boolean;
  failed: boolean;
  count?: number;
};

function Message({ text, settled, loading, failed, count }: MessageProps) {
  if (text.length < 2)
    return <p className={styles.note}>Type at least two letters.</p>;
  if (!settled || loading) return <p className={styles.note}>Searching…</p>;
  if (failed)
    return (
      <p className={styles.note}>
        We could not search right now. Try again in a moment.
      </p>
    );
  if (count === 0)
    return (
      <p className={styles.note}>
        No member found. Check the spelling, or search by phone number.
      </p>
    );
  return null;
}
