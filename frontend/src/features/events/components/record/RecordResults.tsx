import { useDebounced } from "@/lib/useDebounced";
import { useMemberSearch, type MemberMatch } from "@/features/members";
import { useAlreadyRecorded } from "../../hooks/queries";
import { Avatar, Button, Chip } from "@/ui";
import styles from "./RecordResults.module.css";
import {
  footnote,
  rowState,
  type Mode,
  type RowState,
} from "../../model/record";

type Props = {
  eventId: string;
  mode: Mode;
  text: string;
  // Registration is closed: people can be found, but not registered.
  closed: boolean;
  // The ones recorded from this panel just now.
  done: readonly string[];
  busy: string | null;
  onRecord: (member: MemberMatch) => void;
};

const NONE: ReadonlySet<string> = new Set();

// Which of the people found were recorded before this panel was opened: one small question about them, not the event's
// whole list.
function useRecordedBefore(
  eventId: string,
  mode: Mode,
  matches: MemberMatch[] | undefined,
): ReadonlySet<string> {
  const ids = matches?.map((m) => m.id) ?? [];
  return useAlreadyRecorded(eventId, mode, ids).data ?? NONE;
}

// What sits under the search box: a hint, "Searching…", a problem, nothing found, or the matches with a button each.
export function RecordResults({
  eventId,
  mode,
  text,
  closed,
  done,
  busy,
  onRecord,
}: Props) {
  const q = useDebounced(text);
  const search = useMemberSearch(q);
  const matches = search.data && q === text ? search.data : undefined;
  const existing = useRecordedBefore(eventId, mode, matches);
  const note = footnote(mode, closed, matches?.length);
  return (
    <div aria-live="polite" className={styles.results}>
      <Message
        text={text}
        settled={q === text}
        loading={search.isPending}
        failed={search.isError}
        count={matches?.length}
      />
      {matches && matches.length > 0 && (
        <ul aria-label="Matching members" className={styles.list}>
          {matches.map((m) => (
            <Row
              key={m.id}
              member={m}
              mode={mode}
              state={rowState(m.id, existing, done)}
              closed={closed}
              busy={busy === m.id}
              onRecord={onRecord}
            />
          ))}
        </ul>
      )}
      {note && <p className={styles.foot}>{note}</p>}
    </div>
  );
}

type RowProps = {
  member: MemberMatch;
  mode: Mode;
  state: RowState;
  closed: boolean;
  busy: boolean;
  onRecord: (member: MemberMatch) => void;
};

function Row({ member, mode, state, closed, busy, onRecord }: RowProps) {
  const register = mode === "register";
  return (
    <li className={`${styles.option} ${state === "already" ? styles.dim : ""}`}>
      <Avatar name={member.name} size={32} />
      <div className={styles.who}>
        <p className={styles.name}>{member.name}</p>
        <p className={styles.phone}>{member.phoneNumber}</p>
        <div className={styles.action}>
          {state === "done" && (
            <Chip tone="positive">
              {register ? "Registered" : "Checked in"}
            </Chip>
          )}
          {state === "already" && (
            <Chip>
              {register ? "Already registered" : "Already checked in"}
            </Chip>
          )}
          {state === "ready" && (
            <Button
              variant="secondary"
              aria-label={`${register ? "Register" : "Check in"} ${member.name}`}
              loading={busy}
              disabled={register && closed}
              onClick={() => onRecord(member)}
            >
              {register ? "Register" : "Check in"}
            </Button>
          )}
        </div>
      </div>
    </li>
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
