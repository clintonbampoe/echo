import { LinkButton } from "@/ui";
import { Ripples } from "../chrome/Ripples";
import { ProductPreview } from "./ProductPreview";
import styles from "./Hero.module.css";

export function Hero() {
  return (
    <div className={styles.hero}>
      <Ripples className={styles.ripples} />
      <span className={styles.eyebrow}>
        <span className={styles.dot} aria-hidden="true" />
        Free and open source, for every church
      </span>
      <h1 className={styles.title}>
        <span>Your church,</span>
        <span>answered.</span>
      </h1>
      <p className={styles.lead}>
        Members, attendance, giving and events in one calm place, and the answer
        to every question about them one tap away.
      </p>
      <div className={styles.actions}>
        <LinkButton to="/signup" size="xl" pill className={styles.primary}>
          Get started free
        </LinkButton>
        <LinkButton to="/features" size="xl" variant="secondary" pill>
          See how it works
        </LinkButton>
      </div>
      <ProductPreview />
    </div>
  );
}
