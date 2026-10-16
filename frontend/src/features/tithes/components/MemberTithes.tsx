import { Section } from "@/ui";
import { TitheTable } from "./TitheTable";
import { TitheYearStrip } from "./TitheYearStrip";

// A member's giving: this year month by month, then every tithe. Tithes are recorded on the Tithes page,
// never from here.
export function MemberTithes({
  memberId,
  today,
}: {
  memberId: string;
  today: string;
}) {
  return (
    <Section hint="Tithes this member has given, newest first.">
      <TitheYearStrip memberId={memberId} today={today} />
      <TitheTable memberId={memberId} filters={{}} narrowed={false} />
    </Section>
  );
}
