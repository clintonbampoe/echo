import { ageOn } from "@/lib/dates";
import { formatDate } from "@/lib/format";
import { regionLabel } from "@/lib/regions";
import type { Member } from "./api";
import styles from "./MemberDetails.module.css";

type Row = [label: string, value: string | null | undefined];

// Everything we know about a member, in four groups. On a wide screen it sits under their name;
// on a tablet or phone it is the first tab.
export function MemberDetails({ m, today }: { m: Member; today: string }) {
  return (
    <section className={styles.card} aria-label="Details">
      <Group
        title="Contact"
        rows={[
          ["Email", m.emailAddress],
          ["Phone", m.phoneNumber],
        ]}
      />
      <Group
        title="Personal"
        rows={[
          ["Other names", m.otherNames],
          [
            "Date of birth",
            `${formatDate(m.dateOfBirth)} (${ageOn(m.dateOfBirth, today)})`,
          ],
          ["Gender", m.gender],
          ["Marital status", m.maritalStatus],
          ["Hometown", m.hometown],
          ["Joined", m.joinedDate ? formatDate(m.joinedDate) : null],
        ]}
      />
      <Group
        title="Address"
        rows={[
          ["Residence", m.residentialAddress],
          ["City", m.city],
          ["Region", regionLabel(m.region)],
          ["Digital address", m.gpsAddress],
        ]}
      />
      <Group
        title="Emergency"
        rows={[
          ["Next of kin", m.nextOfKin],
          ["Contact", m.emergencyContactName],
          ["Phone", m.emergencyContactPhoneNumber],
        ]}
      />
    </section>
  );
}

// A titled block of label and value lines. A missing value shows a dash so the rows never jump.
function Group({ title, rows }: { title: string; rows: Row[] }) {
  return (
    <section className={styles.group} aria-label={title}>
      <h3 className={styles.title}>{title}</h3>
      <dl className={styles.list}>
        {rows.map(([label, value]) => (
          <div key={label} className={styles.row}>
            <dt>{label}</dt>
            <dd>{value || "–"}</dd>
          </div>
        ))}
      </dl>
    </section>
  );
}
