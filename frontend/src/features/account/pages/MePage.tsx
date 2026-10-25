import { useNavigate } from "react-router-dom";
import { useSignOut } from "@/features/auth";
import { Avatar, Icon, PageHeader } from "@/ui";
import { roleLabel } from "../labels";
import { useCongregation, useMe } from "../hooks";
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
        <Line
          title="Role"
          sub={me ? roleLabel(me.role) : ""}
          value={me?.role}
        />
        <Line title="Congregation" sub={church?.name ?? ""} />
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
