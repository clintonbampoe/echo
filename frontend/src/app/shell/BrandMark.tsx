import { Link } from "react-router-dom";
import { Logo } from "@/ui";
import styles from "./BrandMark.module.css";

// The logo and the word "Echo", linking to the dashboard (the signed-in app's own home).
export function BrandMark() {
  return (
    <Link to="/dashboard" className={styles.brand} aria-label="Echo dashboard">
      <Logo size={24} />
      <span>Echo</span>
    </Link>
  );
}
