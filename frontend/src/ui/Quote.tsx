import styles from "./Quote.module.css";

type Props = {
  text: string;
  reference: string;
  size?: "md" | "lg";
  align?: "start" | "center";
};

// Rule S1: the area is a fixed height sized for the longest verse, so swapping verses
// never moves anything around it. Remounting on a new reference replays the fade.
export function Quote({
  text,
  reference,
  size = "md",
  align = "start",
}: Props) {
  return (
    <figure className={`${styles.quote} ${styles[size]} ${styles[align]}`}>
      <div key={reference} className="anim-fade">
        <blockquote className={styles.text}>{`“${text}”`}</blockquote>
        <figcaption className={styles.reference}>{reference}</figcaption>
      </div>
    </figure>
  );
}
