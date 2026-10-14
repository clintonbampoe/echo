import { formatDate } from "@/lib/format";
import { paths } from "@/lib/paths";
import { LinkButton } from "@/ui";
import type { Visitor } from "./api";
import { memberIdOf } from "./view";
import styles from "./ConvertedBanner.module.css";

// Shown on the page of a visitor who has joined: where they went, with a way to follow them there.
export function ConvertedBanner({ v }: { v: Visitor }) {
  const memberId = memberIdOf(v);
  if (!memberId) return null;
  return (
    <div className={styles.banner} role="status">
      <p className={styles.text}>
        {v.firstName} became a member
        {v.convertedAt ? ` on ${formatDate(v.convertedAt)}` : ""}. Their record
        now lives under Members.
      </p>
      <LinkButton to={paths.member(memberId)} variant="secondary" size="sm">
        View member
      </LinkButton>
    </div>
  );
}
