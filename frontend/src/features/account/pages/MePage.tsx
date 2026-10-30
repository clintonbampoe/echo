import { useNavigate } from "react-router-dom";
import { useSignOut } from "@/features/auth";
import { formatDate } from "@/lib/format";
import { Avatar, Icon, PageHeader } from "@/ui";
import { roleLabel } from "../model/labels";
import type { Congregation, Me } from "../api/account";
import { useCongregation, useMe } from "../hooks/queries";
import styles from "./MePage.module.css";

// Who is signed in, and the way out.
export function MePage() {
  const me = useMe().data;
  const church = useCongregation().data;
  const signOut = useSignOut();
  const navigate = useNavigate();

  // Go to sign-in first, then end the session, so the guard around the app does not redirect twice.
  function leave() {
    navigate("/signin", { replace: true });
    signOut.mutate();
  }

  return (
    <div className={`${styles.page} anim-rise`}>
      <PageHeader title="Me" />
      <section className={styles.card}>
        <div className={styles.identity}>
          <Avatar name={me?.name ?? ""} size={64} />
          <div className={styles.who}>
            <h2 className={styles.name}>{me?.name ?? "Loading…"}</h2>
            <p className={styles.sub}>{me?.emailAddress}</p>
          </div>
        </div>
        <AccountLines me={me} church={church} />
      </section>
      <section className={styles.card}>
        <button
          type="button"
          className={styles.line}
          disabled={signOut.isPending}
          onClick={leave}
        >
          <span className={styles.title}>Sign out</span>
          <Icon name="chevron-right" size={18} className={styles.chevron} />
        </button>
      </section>
    </div>
  );
}

function Line({
  title,
  sub,
  value,
}: {
  title: string;
  sub: string;
  value?: string;
}) {
  return (
    <div className={styles.line}>
      <div>
        <p className={styles.title}>{title}</p>
        <p className={styles.sub}>{sub}</p>
      </div>
      {value && <span className={styles.value}>{value}</span>}
    </div>
  );
}

// Role, email, password and church, one line each. Email and password only appear once they can say something.
function AccountLines({
  me,
  church,
}: {
  me: Me | undefined;
  church: Congregation | undefined;
}) {
  return (
    <>
      <Line title="Role" sub={me ? roleLabel(me.role) : ""} value={me?.role} />
      <Line
        title="Email"
        sub={me?.emailAddress ?? ""}
        value={me?.emailVerifiedAt ? "Verified" : "Not verified"}
      />
      <PasswordLine changedAt={me?.passwordChangedAt} />
      <Line title="Congregation" sub={church?.name ?? ""} />
    </>
  );
}

// Only shown once the account has a change date to report.
function PasswordLine({ changedAt }: { changedAt?: string | null }) {
  if (!changedAt) return null;
  return (
    <Line
      title="Password"
      sub={`Changed ${formatDate(changedAt.slice(0, 10))}`}
    />
  );
}
