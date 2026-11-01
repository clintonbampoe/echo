import { ALL_TIME } from "@/lib/dates";
import { getPresent } from "../api/attendance";
import { hasInvitations } from "../api/setup";
import { getTithes } from "../api/tithes";
import type { SetupFacts } from "../model/setup";
import { useData, useOnce } from "./internals";
import { useMix } from "./people";

// What the church has done so far, from the figures the cards already ask for (members is the Members card's
// own answer) plus the invitations count. Asks nothing unless `enabled`, so only an admin ever pays for it.
export function useSetupFacts(enabled: boolean): SetupFacts {
  const mix = useMix();
  const { from, to } = ALL_TIME;
  const present = useData("setup-present", from, to, getPresent, enabled);
  const tithes = useData("setup-tithes", from, to, getTithes, enabled);
  const invited = useOnce("setup-invitations", hasInvitations, enabled);
  return {
    members: mix.mix ? mix.mix.total > 0 : undefined,
    attendance: present.data ? present.data.present > 0 : undefined,
    tithes: tithes.data ? tithes.data.total > 0 : undefined,
    invitations: invited.data,
  };
}
