import { useEffect, useState } from "react";
import { useMe, useVerificationStatus } from "@/features/account";
import { useSendVerification } from "@/features/auth";
import { isApiError } from "@/lib/api";
import { useCooldown } from "@/ui";
import { reminderPhase } from "./reminderView";

const COOLDOWN_SECONDS = 30;
// How long "Email confirmed" stays before the reminder leaves.
const CONFIRMED_MS = 4000;

// What the verify-email reminder should be doing right now, and how to send the link. It is shown while the
// address is unverified; once a link has gone out it asks, whenever the person returns to the tab, whether
// they opened it, and says so for a few seconds before it goes. "hidden" means draw nothing.
export function useVerifyReminder() {
  const me = useMe().data;
  const send = useSendVerification();
  const [sent, setSent] = useState(false);
  const [failed, setFailed] = useState(false);
  const [wait, startWait] = useCooldown(COOLDOWN_SECONDS);
  const [sawUnverified, setSawUnverified] = useState(false);
  const [gone, setGone] = useState(false);
  const status = useVerificationStatus(sent);

  // Null means "not verified". Anything else (including an account older than the field) is quiet.
  const unverified = me?.verifiedAt === null && status.data?.verified !== true;
  if (unverified && !sawUnverified) setSawUnverified(true);

  const phase = reminderPhase({
    loaded: me !== undefined,
    unverified,
    sawUnverified,
    gone,
    sent,
    failed,
  });

  useEffect(() => {
    if (phase !== "confirmed") return;
    const timer = setTimeout(() => setGone(true), CONFIRMED_MS);
    return () => clearTimeout(timer);
  }, [phase]);

  async function onSend() {
    if (!me) return;
    setFailed(false);
    try {
      await send.mutateAsync(me.emailAddress);
      setSent(true);
      startWait();
    } catch (e) {
      // Too many requests means a link went out recently: wait, as the resend button does.
      if (isApiError(e) && e.status === 429) {
        setSent(true);
        startWait();
      } else setFailed(true);
    }
  }

  return {
    phase,
    email: me?.emailAddress ?? "",
    wait,
    sending: send.isPending,
    onSend,
  };
}
