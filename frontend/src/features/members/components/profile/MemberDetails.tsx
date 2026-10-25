import { ageOn } from "@/lib/dates";
import { formatDate } from "@/lib/format";
import { regionLabel } from "@/lib/regions";
import { DetailCard, DetailGroup } from "@/ui";
import type { Member } from "../../api/members";
import styles from "./MemberDetails.module.css";

// Everything we know about a member, in cards: who they are, their place in the church, and who to call.
export function MemberDetails({ m, today }: { m: Member; today: string }) {
  return (
    <div className={styles.cards}>
      <DetailCard label="Personal">
        <DetailGroup
          title="Personal"
          rows={[
            ["Other names", m.otherNames],
            ["Gender", m.gender],
            [
              "Date of birth",
              `${formatDate(m.dateOfBirth)} (${ageOn(m.dateOfBirth, today)})`,
            ],
            ["Marital status", m.maritalStatus],
            ["Phone", m.phoneNumber],
            ["Email", m.emailAddress],
            [
              "Address",
              [m.residentialAddress, m.city].filter(Boolean).join(", "),
            ],
            ["Digital address", m.gpsAddress],
          ]}
        />
      </DetailCard>
      <div className={styles.column}>
        <DetailCard label="Church">
          <DetailGroup
            title="Church"
            rows={[
              ["Status", m.status],
              ["Joined", m.joinedDate ? formatDate(m.joinedDate) : null],
              ["Hometown", m.hometown],
              ["Region", regionLabel(m.region)],
            ]}
          />
        </DetailCard>
        <DetailCard label="Emergency contact">
          <DetailGroup
            title="Emergency contact"
            rows={[
              ["Next of kin", m.nextOfKin],
              ["Contact", m.emergencyContactName],
              ["Phone", m.emergencyContactPhoneNumber],
            ]}
          />
        </DetailCard>
      </div>
    </div>
  );
}
