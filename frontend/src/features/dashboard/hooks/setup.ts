import { ALL_TIME } from "@/lib/dates";
import { getPresent } from "../api/attendance";
import { hasInvitations } from "../api/setup";
import { getTithes } from "../api/tithes";
import type { SetupFacts } from "../model/setup";
import { useData, useOnce } from "./internals";
import { useMix } from "./people";

// What the church has done so far, from the figures the cards already ask for (members is the Members card's
// own answer) plus the invitations count. Only the Getting started card uses it, and only an admin sees that.
export function useSetupFacts(): SetupFacts {
  const mix = useMix();
  const present = useData(
    "setup-present",
    ALL_TIME.from,
    ALL_TIME.to,
    getPresent,
  );
  const tithes = useData("setup-tithes", ALL_TIME.from, ALL_TIME.to, getTithes);
  const invited = useOnce("setup-invitations", hasInvitations);
  return {
    members: mix.mix ? mix.mix.total > 0 : undefined,
    attendance: present.data ? present.data.present > 0 : undefined,
    tithes: tithes.data ? tithes.data.total > 0 : undefined,
    invitations: invited.data,
  };
}
