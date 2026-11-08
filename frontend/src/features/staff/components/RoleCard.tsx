import { Link } from "react-router-dom";
import { type UserRole } from "@/features/account";
import { Alert, Button, PersonListSkeleton } from "@/ui";
import type { StaffQuery, StaffUser } from "../api/staff";
import { useRoleList } from "../hooks";
import type { RowAction } from "../model/actions";
import { roleSlug, rolePlural } from "../model/roles";
import styles from "./RoleCard.module.css";
import { StaffRoleList } from "./StaffRoleList";

type Props = {
  role: UserRole;
  // What the page is narrowed by, so the card shows the same people the search and filter would.
  query: StaffQuery;
  // The address search of that narrowing, so "See all" opens the role's page with it still applied.
  search: string;
  onAct: (action: RowAction, user: StaffUser) => void;
};

// A few people show here; the role's own page has all of them, with a search of its own.
const PREVIEW = 5;

// One role and the first few people who hold it. Each card loads its own list, so a big role never holds up the others.
export function RoleCard({ role, query, search, onAct }: Props) {
  const list = useRoleList(role, { ...query, size: PREVIEW });

  if (list.loading) return <PersonListSkeleton rows={3} />;
  if (list.failed)
    return (
      <Alert
        title={`We could not load the ${rolePlural(role).toLowerCase()}`}
        action={
          <Button variant="secondary" size="sm" onClick={list.retry}>
            Try again
          </Button>
        }
      >
        Nothing was changed. Check your connection and try again.
      </Alert>
    );

  const narrowed = Boolean(query.q || query.status);
  const footer =
    list.rows.length === 0 ? (
      <p className={styles.empty}>
        {narrowed ? "Nobody matches." : "Nobody has this role yet."}
      </p>
    ) : (
      <div className={styles.footer}>
        <Link
          to={{ pathname: `/staff/${roleSlug(role)}`, search }}
          className={styles.see}
        >
          See all {rolePlural(role).toLowerCase()}
        </Link>
      </div>
    );

  return (
    <StaffRoleList
      role={role}
      rows={list.rows.slice(0, PREVIEW)}
      footer={footer}
      onAct={onAct}
    />
  );
}
