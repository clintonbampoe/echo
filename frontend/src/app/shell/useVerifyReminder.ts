import { useEffect, useState } from "react";
import { useMe, useVerificationStatus } from "@/features/account";
import { reminderPhase } from "./reminderView";

// How long "Account verified" stays before the reminder leaves.
const CONFIRMED_MS = 4000;

// What the reminder should be doing right now. It shows while the account is unverified, and while it is
// there it asks, whenever the person returns to the tab, whether the link was opened (on any device). It then
// says so for a few seconds before it goes. "hidden" means draw nothing.
export function useVerifyReminder() {
  const me = useMe().data;
  const [sawUnverified, setSawUnverified] = useState(false);
  const [gone, setGone] = useState(false);
  // Null means "not verified". Anything else (including an account older than the field) is quiet.
  const notVerified = me?.verifiedAt === null;
  const status = useVerificationStatus(notVerified);
  const unverified = notVerified && status.data?.verified !== true;
  if (unverified && !sawUnverified) setSawUnverified(true);

  const phase = reminderPhase({
    loaded: me !== undefined,
    unverified,
    sawUnverified,
    gone,
  });

  useEffect(() => {
    if (phase !== "confirmed") return;
    const timer = setTimeout(() => setGone(true), CONFIRMED_MS);
    return () => clearTimeout(timer);
  }, [phase]);

  return { phase, email: me?.emailAddress ?? "" };
}
