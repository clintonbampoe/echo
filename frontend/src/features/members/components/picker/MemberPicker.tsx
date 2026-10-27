import { useState } from "react";
import { useDebounced } from "@/lib/useDebounced";
import { Avatar, Button, Lookup } from "@/ui";
import type { MemberMatch } from "../../api/members";
import { useMemberSearch, useRecentMembers } from "../../hooks";
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
    <Finder
      text={text}
      onText={setText}
      onPick={onChange}
      taken={taken}
      takenText={takenText}
      error={error}
    />
  );
}

type FinderProps = {
  text: string;
  onText: (text: string) => void;
  onPick: (m: MemberMatch) => void;
  taken?: ReadonlySet<string>;
  takenText: string;
  error?: string;
};

// The search box and what it finds. It asks once typing has paused for 250 ms, and remembers who was picked last.
function Finder({
  text,
  onText,
  onPick,
  taken,
  takenText,
  error,
}: FinderProps) {
  const q = useDebounced(text.trim(), 250);
  const search = useMemberSearch(q);
  const { recent, remember } = useRecentMembers();
  const settled = q === text.trim();
  const item = (m: MemberMatch) => ({
    id: m.id,
    label: m.name,
    sub: m.phoneNumber,
    disabled: taken?.has(m.id),
    note: taken?.has(m.id) ? takenText : undefined,
    member: m,
  });
  const status =
    !settled || search.isPending
      ? "searching"
      : search.isError
        ? "error"
        : "ready";
  return (
    <Lookup
      label="Search members"
      placeholder="Search by name or phone"
      text={text}
      onText={onText}
      status={status}
      items={(settled ? (search.data ?? []) : []).map(item)}
      recent={recent.map(item)}
      noMatch="No member found. Check the spelling, or search by phone number."
      failed="We could not search right now. Try again in a moment."
      lead={(i) => <Avatar name={i.label} size={32} />}
      onPick={(i) => {
        remember(i.member);
        onPick(i.member);
      }}
      error={error}
    />
  );
}
