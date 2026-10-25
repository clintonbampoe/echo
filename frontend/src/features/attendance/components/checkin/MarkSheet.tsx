import { Button, Sheet } from "@/ui";
import type { CheckIn } from "../../api/checkIns";
import type { Session } from "../../api/sessions";
import { sessionTitle } from "../../model/day";
import { CheckInPanel } from "./CheckInPanel";

type Props = { session: Session; rows: CheckIn[]; onClose: () => void };

// Marking people present, in a panel over the session: search, tap, and each one is saved at once. On a phone this is
// where it happens; on a wide screen the same panel is also on the page. Done only closes it.
export function MarkSheet({ session, rows, onClose }: Props) {
  return (
    <Sheet
      open
      onClose={onClose}
      title="Mark present"
      subtitle={sessionTitle(session)}
      footer={<Button onClick={onClose}>Done</Button>}
    >
      <CheckInPanel session={session} rows={rows} flat />
    </Sheet>
  );
}
