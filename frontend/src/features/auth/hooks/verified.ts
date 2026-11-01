import { useEffect, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getVerified } from "../api/auth";
import { authKeys } from "./keys";

const POLL_MS = 4000;
const CHANNEL = "echo-account";

const hasChannels = () => typeof BroadcastChannel !== "undefined";

const ACK_MS = 500;

// Called by the page the email link opens. A verify page waiting in another tab hears it at once, answers,
// and takes the person on from there. Resolves true if one answered, false if this is the only tab.
export function announceVerified(): Promise<boolean> {
  if (!hasChannels()) return Promise.resolve(false);
  return new Promise((resolve) => {
    const channel = new BroadcastChannel(CHANNEL);
    const finish = (answered: boolean) => {
      channel.close();
      resolve(answered);
    };
    channel.onmessage = (e: MessageEvent) => {
      if (e.data === "ack") finish(true);
    };
    channel.postMessage("verified");
    setTimeout(() => finish(false), ACK_MS);
  });
}

// For the page the email link opens: undefined while it asks, then whether another tab took over.
export function useOtherTab(enabled: boolean): boolean | undefined {
  const [other, setOther] = useState<boolean>();
  useEffect(() => {
    if (!enabled) return;
    void announceVerified().then(setOther);
  }, [enabled]);
  return other;
}

// True once the account is verified. It asks every few seconds while the tab is visible and again whenever
// the person returns to it, and it hears at once from another tab in this browser that opened the link.
// Without a session the server cannot say, so then only the other tab can tell us.
export function useWatchVerified(signedIn: boolean): boolean {
  const queryClient = useQueryClient();
  const [heard, setHeard] = useState(false);
  const status = useQuery({
    queryKey: authKeys.verified,
    queryFn: ({ signal }) => getVerified(signal),
    enabled: signedIn,
    refetchInterval: (q) => (q.state.data?.verified ? false : POLL_MS),
    refetchOnWindowFocus: "always",
  });

  useEffect(() => {
    if (!hasChannels()) return;
    const channel = new BroadcastChannel(CHANNEL);
    channel.onmessage = (e: MessageEvent) => {
      if (e.data !== "verified") return;
      setHeard(true);
      channel.postMessage("ack");
    };
    return () => channel.close();
  }, []);

  const verified = heard || status.data?.verified === true;
  // The rest of the app (the reminder, the Account page) reads the profile again.
  useEffect(() => {
    if (verified) void queryClient.invalidateQueries({ queryKey: ["account"] });
  }, [verified, queryClient]);
  return verified;
}
