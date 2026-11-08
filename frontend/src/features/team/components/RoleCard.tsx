import { useState } from "react";
import { roleLabel, useMe, type UserRole } from "@/features/account";
import {
  Alert,
  Button,
  Chip,
  PersonList,
  PersonListSkeleton,
  type MenuItem,
} from "@/ui";
import type { TeamStatus, TeamUser } from "../api/team";
import { useRoleList } from "../hooks";
import { flag, isDisabled } from "../model/display";
import styles from "./RoleCard.module.css";

export type RowAction = "view" | "role" | "disable" | "enable" | "remove";

type Props = {
  role: UserRole;
  // How many people hold the role (from the summary), so the card can say "5 of 16" before the list loads.
  count: number | undefined;
  share: string;
  status: TeamStatus | undefined;
  onAct: (action: RowAction, user: TeamUser) => void;
};

// Five people show first; the rest open in place, a page at a time.
const PREVIEW = 5;

// One role and the people who hold it. Each card loads its own list, so a big role never holds up the others.
export function RoleCard({ role, count, share, status, onAct }: Props) {
  const list = useRoleList(role, status);
  const me = useMe();
  const [open, setOpen] = useState(false);
  const shown = open ? list.rows : list.rows.slice(0, PREVIEW);
  const total = count ?? list.rows.length;

  if (list.loading) return <PersonListSkeleton rows={3} />;
  if (list.failed)
    return (
      <Alert
        title={`We could not load the ${roleLabel(role).toLowerCase()}s`}
        action={<button onClick={list.retry}>Try again</button>}
      >
        Nothing was changed. Check your connection and try again.
      </Alert>
    );

  const items = (u: TeamUser): MenuItem[] =>
    u.id === me.data?.id
      ? [{ label: "View details", onSelect: () => onAct("view", u) }]
      : [
          { label: "View details", onSelect: () => onAct("view", u) },
          { label: "Change role", onSelect: () => onAct("role", u) },
          isDisabled(u)
            ? { label: "Enable account", onSelect: () => onAct("enable", u) }
            : { label: "Disable account", onSelect: () => onAct("disable", u) },
          {
            label: "Remove account",
            danger: true,
            separated: true,
            onSelect: () => onAct("remove", u),
          },
        ];

  return (
    <PersonList
      label={roleLabel(role)}
      title={roleLabel(role)}
      aside={`${total} · ${share} of the team`}
      rows={shown}
      rowKey={(u) => u.id}
      person={(u) => ({
        name: u.name,
        detail: u.emailAddress,
        onOpen: () => onAct("view", u),
      })}
      columns={[
        {
          header: "Status",
          width: "96px",
          chip: true,
          cell: (u) => {
            const f = flag(u, u.id === me.data?.id);
            return f ? <Chip tone={f.tone}>{f.label}</Chip> : null;
          },
        },
      ]}
      actions={items}
      footer={
        <CardFooter
          shown={shown.length}
          total={total}
          open={open}
          hasMore={list.hasMore}
          loadingMore={list.loadingMore}
          onToggle={() => setOpen((o) => !o)}
          onMore={list.more}
        />
      }
    />
  );
}

type FooterProps = {
  shown: number;
  total: number;
  open: boolean;
  hasMore: boolean;
  loadingMore: boolean;
  onToggle: () => void;
  onMore: () => void;
};

function CardFooter(p: FooterProps) {
  if (p.total <= PREVIEW && !p.hasMore) return null;
  return (
    <div className={styles.footer}>
      <span>
        {p.shown} of {p.total}
      </span>
      <span className={styles.links}>
        {p.open && p.hasMore && (
          <Button
            variant="ghost"
            size="sm"
            loading={p.loadingMore}
            onClick={p.onMore}
          >
            Show more
          </Button>
        )}
        <Button variant="ghost" size="sm" onClick={p.onToggle}>
          {p.open ? "Show fewer" : `See all ${p.total}`}
        </Button>
      </span>
    </div>
  );
}
