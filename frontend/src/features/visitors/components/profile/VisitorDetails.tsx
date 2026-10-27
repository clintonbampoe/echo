import { ageOn } from "@/lib/dates";
import { formatDate } from "@/lib/format";
import { DetailBlock, DetailCard, DetailGroup } from "@/ui";
import type { Visitor } from "../../api/visitors";
import styles from "./VisitorDetails.module.css";

// What we know about a visitor. On a wide screen it sits under their name; on a tablet or phone it follows the numbers.
export function VisitorDetails({ v, today }: { v: Visitor; today: string }) {
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
          [
            "Date of birth",
            v.dateOfBirth
              ? `${formatDate(v.dateOfBirth)} (${ageOn(v.dateOfBirth, today)})`
              : null,
          ],
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
