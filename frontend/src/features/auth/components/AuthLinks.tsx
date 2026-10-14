import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import styles from "./AuthLinks.module.css";

// "Already have an account? Sign in" style line under a form.
export function AuthPrompt({
  children,
  to,
  action,
}: {
  children: ReactNode;
  to: string;
  action: string;
}) {
  return (
    <p className={styles.prompt}>
      {children}{" "}
      <Link to={to} className={styles.strong}>
        {action}
      </Link>
    </p>
  );
}

// The quiet "Back to sign in" link at the foot of a screen.
export function BackLink({
  to = "/signin",
  children = "Back to sign in",
}: {
  to?: string;
  children?: ReactNode;
}) {
  return (
    <p className={styles.back}>
      <Link to={to} className={styles.muted}>
        {children}
      </Link>
    </p>
  );
}
