import { formatDate } from "@/lib/format";
import { genderLabel } from "@/features/members";
import { DetailBlock, DetailCard, DetailGroup } from "@/ui";
import type { Visitor } from "../../api/visitors";
import styles from "./VisitorDetails.module.css";

// What we know about a visitor. On a wide screen it sits under their name; on a tablet or phone it follows the numbers.
export function VisitorDetails({ v }: { v: Visitor }) {
  return (
    <DetailCard label="Details">
      <DetailGroup
        title="Contact"
        rows={[
          ["Phone", v.phoneNumber],
          ["Email", v.emailAddress],
        ]}
      />
      <DetailGroup
        title="Personal"
        rows={[
          ["Gender", v.gender ? genderLabel(v.gender) : null],
          ["First added", formatDate(v.createdAt)],
        ]}
      />
      <DetailBlock title="Notes">
        {v.notes ? (
          <p className={styles.notes}>{v.notes}</p>
        ) : (
          <p className={styles.empty}>No notes yet.</p>
        )}
        <p className={styles.hint}>Notes are only visible to church staff.</p>
      </DetailBlock>
    </DetailCard>
  );
}
