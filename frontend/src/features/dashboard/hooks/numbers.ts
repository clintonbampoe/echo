import { getPresent } from "../api/attendance";
import { getJoined } from "../api/members";
import { getNet } from "../api/money";
import { getTithes } from "../api/tithes";
import type { Amount, Numbers } from "../model/cards";
import type { Span } from "../model/periods";
import { useData } from "./internals";
import { useMix } from "./people";

// People who joined in a range, and in the stretch before it. The server leaves out members with no joined date.
export function useJoined(span: Span) {
  return useData("joined", span.from, span.to, getJoined);
}

// Everything the four cards need: the period, and the stretch of the same length before it (the server sends both).
// Members is the whole church as of today, so it does not change with the period.
export function useNumbers(span: Span): Numbers {
  const mix = useMix();
  const net = useData("net", span.from, span.to, getNet);
  const joined = useJoined(span);
  const present = useData("present", span.from, span.to, getPresent);
  const tithes = useData("tithes", span.from, span.to, getTithes);
  const of = (
    value: number | undefined,
    source: { failed: boolean; retry: () => void },
  ): Amount => ({ value, failed: source.failed, retry: source.retry });
  return {
    members: of(mix.mix?.total, mix),
    newMembers: of(joined.data?.joined, joined),
    present: of(present.data?.present, present),
    presentBefore: of(present.data?.before, present),
    tithes: of(tithes.data?.total, tithes),
    tithesBefore: of(tithes.data?.before, tithes),
    net: of(net.data?.net, net),
    netBefore: of(net.data?.before, net),
  };
}
