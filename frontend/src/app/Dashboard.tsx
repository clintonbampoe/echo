import { useNavigate } from "react-router-dom";
import { useSignOut } from "@/features/auth";
import { Button, Logo } from "@/ui";
import styles from "./Dashboard.module.css";

// Placeholder until the app shell exists. It proves the signed-in flow works end to end.
export function Dashboard() {
  const signOut = useSignOut();
  const navigate = useNavigate();
  async function onSignOut() {
    await signOut.mutateAsync();
    navigate("/", { replace: true });
  }
  return (
    <main className={styles.page}>
      <div className={`${styles.card} anim-rise`}>
        <Logo />
        <h1 className={styles.title}>You are signed in</h1>
        <p className={styles.text}>The dashboard is coming next.</p>
        <Button
          variant="secondary"
          size="lg"
          block
          loading={signOut.isPending}
          onClick={onSignOut}
        >
          Sign out
        </Button>
      </div>
    </main>
  );
}
