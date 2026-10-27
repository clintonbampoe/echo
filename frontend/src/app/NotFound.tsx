import { LinkButton, Logo, usePublicSurface } from "@/ui";
import styles from "./NotFound.module.css";

export function NotFound() {
  usePublicSurface();
  return (
    <main className={styles.page}>
      <div className={`${styles.card} anim-rise`}>
        <Logo />
        <h1 className={styles.title}>We could not find that page</h1>
        <p className={styles.text}>
          The link may be old, or the address may have a typo.
        </p>
        <LinkButton to="/" size="lg" block>
          Go to the home page
        </LinkButton>
      </div>
    </main>
  );
}
