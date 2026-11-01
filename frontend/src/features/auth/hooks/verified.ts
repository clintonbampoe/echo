import { useEffect, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getVerified } from "../api/auth";
import { authKeys } from "./keys";

const POLL_MS = 4000;
const CHANNEL = "echo-account";

const hasChannels = () => typeof BroadcastChannel !== "undefined";

// Called by the page the email link opens, so a verify page waiting in another tab knows at once.
export function announceVerified() {
  if (!hasChannels()) return;
  const channel = new BroadcastChannel(CHANNEL);
  channel.postMessage("verified");
  channel.close();
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
    channel.onmessage = () => setHeard(true);
    return () => channel.close();
  }, []);

  const verified = heard || status.data?.verified === true;
  // The rest of the app (the reminder, the Account page) reads the profile again.
  useEffect(() => {
    if (verified) void queryClient.invalidateQueries({ queryKey: ["account"] });
  }, [verified, queryClient]);
  return verified;
}
