import { Link } from "react-router-dom";
import { Icon } from "@/ui";
import styles from "./BackToSite.module.css";

export function BackToSite({ className = "" }: { className?: string }) {
  return (
    <Link to="/" className={`${styles.link} ${className}`}>
      <Icon name="arrow-right" size={16} className={styles.flip} />
      Back to site
    </Link>
  );
}
