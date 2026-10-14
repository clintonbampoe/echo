import { useIsSignedIn } from "@/features/auth";
import { Icon, LinkButton } from "@/ui";
import styles from "./Cta.module.css";

// The main call to action. Signed-out visitors are asked to register; signed-in people
// get a way straight back into the app.
export function PrimaryCta() {
  const signedIn = useIsSignedIn();
  return (
    <LinkButton
      to={signedIn ? "/dashboard" : "/signup"}
      size="xl"
      pill
      className={styles.cta}
    >
      {signedIn ? "Open Echo" : "Create your church account"}
      <Icon name="arrow-right" size={18} className={styles.arrow} />
    </LinkButton>
  );
}
