import { ALL_TIME } from "@/lib/dates";
import { getPresent } from "../api/attendance";
import { getMembers } from "../api/members";
import { getNet } from "../api/money";
import { getTithes } from "../api/tithes";
import type { Amount, Numbers } from "../model/cards";
import { previousSpan, type Span } from "../model/periods";
import { useAmount, useOnce } from "./internals";

// Members who joined in a range. The server counts a member with no joined date in every range, so this
// asks for a single day long ago (which only those members match) and takes that many off.
export function useUndatedMembers() {
  return useOnce("members-undated", (signal) =>
    getMembers(ALL_TIME.from, ALL_TIME.from, signal),
  );
}

// People who joined in a range, without the members who have no joined date.
export function useNewMembers(span: Span): Amount {
  const joined = useAmount("members", span.from, span.to, getMembers);
  const undated = useUndatedMembers();
  const gone = undated.data;
  return {
    value:
      joined.value !== undefined && gone !== undefined
        ? Math.max(0, joined.value - gone)
        : undefined,
    failed: joined.failed || undated.failed,
    retry: () => {
      joined.retry();
      undated.retry();
    },
  };
}

// Everything the four cards need: the period, and the stretch of the same length before it.
// Members is the whole church as of today, so it does not change with the period.
export function useNumbers(span: Span, today: string): Numbers {
  const before = previousSpan(span);
  return {
    members: useAmount("members", ALL_TIME.from, today, getMembers),
    newMembers: useNewMembers(span),
    present: useAmount("present", span.from, span.to, getPresent),
    presentBefore: useAmount("present", before.from, before.to, getPresent),
    tithes: useAmount("tithes", span.from, span.to, getTithes),
    tithesBefore: useAmount("tithes", before.from, before.to, getTithes),
    net: useAmount("net", span.from, span.to, getNet),
    netBefore: useAmount("net", before.from, before.to, getNet),
  };
}
