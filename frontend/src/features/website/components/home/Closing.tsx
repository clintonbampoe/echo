import { Cta } from "../chrome/Cta";
import { Section } from "../chrome/Section";
import { Stop } from "../chrome/Stop";
import styles from "./Closing.module.css";

type Props = { second?: { label: string; to: string } };

// The last words of a page: the title again and two ways in.
export function Closing({
  second = { label: "See pricing", to: "/pricing" },
}: Props) {
  return (
    <Section top={130}>
      <div className={styles.closing}>
        <h2 className={styles.title}>
          Start with your church
          <Stop />
        </h2>
        <div className={styles.actions}>
          <Cta to="/signup">Get started free</Cta>
          <Cta to={second.to} variant="secondary">
            {second.label}
          </Cta>
        </div>
      </div>
    </Section>
  );
}
