import { useEffect, useRef, useState } from "react";
import { isApiError } from "@/lib/api";
import { useCooldown } from "@/ui";
import { sendVerificationLink } from "../api/auth";
import {
  markSent,
  sentRecently,
  SLOW_AFTER_MS,
  verifyPhase,
  type SendState,
} from "../model/verifyPhase";
import { useWatchVerified } from "./verified";

const COOLDOWN_SECONDS = 30;

// Everything the verify page needs: which face to show, how long until another link may go, how to send
// one, and whether the account has been verified (in this tab, another tab or another device).
// With autoSend the link goes out as the page opens (the reminder sends people here); otherwise it went out
// moments ago, when the person signed up.
export function useVerifyFlow(
  email: string,
  autoSend: boolean,
  signedIn: boolean,
) {
  const [wait, startWait] = useCooldown(COOLDOWN_SECONDS, true);
  const [sending, setSending] = useState<SendState>(() =>
    autoSend && !sentRecently(email, Date.now()) ? "sending" : "sent",
  );
  const [slow, setSlow] = useState(false);
  const verified = useWatchVerified(signedIn);
  const started = useRef(false);

  // Does the sending. It changes state only once the request has answered, so the page can call it as it opens.
  async function deliver() {
    markSent(email, Date.now());
    try {
      await sendVerificationLink(email);
      setSending("sent");
      startWait();
    } catch (e) {
      // Too many requests means a link went out recently: wait, as if we had sent it.
      if (isApiError(e) && e.status === 429) {
        setSending("sent");
        startWait();
      } else setSending("failed");
    }
  }

  function send() {
    setSending("sending");
    setSlow(false);
    void deliver();
  }

  useEffect(() => {
    if (started.current || sending !== "sending") return;
    started.current = true;
    void deliver();
  });

  useEffect(() => {
    if (sending !== "sent") return;
    const timer = setTimeout(() => setSlow(true), SLOW_AFTER_MS);
    return () => clearTimeout(timer);
  }, [sending]);

  return {
    phase: verifyPhase({ verified, send: sending, slow }),
    wait,
    send,
  };
}
