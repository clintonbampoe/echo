import { formatDate, formatMoney } from "@/lib/format";
import { Button, Notice } from "@/ui";
import type { Tithe } from "../../api/tithes";

type Props = {
  memberName: string;
  // The month in words, e.g. "October 2026".
  period: string;
  // The tithe the member already has for that month.
  tithe: Tithe;
  // Opens that tithe to be updated.
  onUpdate: (tithe: Tithe) => void;
};

// Shown under the member once they already have a tithe for the month. A member has one tithe a month, so this
// is a stop sign, and the way on is right here: update that tithe to the new total and today's date.
export function SameMonthNotice({
  memberName,
  period,
  tithe,
  onUpdate,
}: Props) {
  return (
    <Notice
      tone="warning"
      title={`${memberName} already has a tithe for ${period}`}
      action={
        <Button variant="secondary" onClick={() => onUpdate(tithe)}>
          Update that tithe
        </Button>
      }
    >
      <p>
        {formatMoney(tithe.amount)}, collected{" "}
        {formatDate(tithe.collectionDate)}. A member has one tithe a month, so
        to top up, update it to the new total and today&rsquo;s date.
      </p>
    </Notice>
  );
}
