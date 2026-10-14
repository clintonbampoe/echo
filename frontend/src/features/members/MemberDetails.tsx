import { ageOn } from "@/lib/dates";
import { formatDate } from "@/lib/format";
import { regionLabel } from "@/lib/regions";
import { DetailCard, DetailGroup } from "@/ui";
import type { Member } from "./api";

// Everything we know about a member, in four groups. On a wide screen it sits under their name;
// on a tablet or phone it is the first tab.
export function MemberDetails({ m, today }: { m: Member; today: string }) {
  return (
    <DetailCard label="Details">
      <DetailGroup
        title="Contact"
        rows={[
          ["Email", m.emailAddress],
          ["Phone", m.phoneNumber],
        ]}
      />
      <DetailGroup
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
      <DetailGroup
        title="Address"
        rows={[
          ["Residence", m.residentialAddress],
          ["City", m.city],
          ["Region", regionLabel(m.region)],
          ["Digital address", m.gpsAddress],
        ]}
      />
      <DetailGroup
        title="Emergency"
        rows={[
          ["Next of kin", m.nextOfKin],
          ["Contact", m.emergencyContactName],
          ["Phone", m.emergencyContactPhoneNumber],
        ]}
      />
    </DetailCard>
  );
}
