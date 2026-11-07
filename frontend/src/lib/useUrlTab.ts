import { useSearchParams } from "react-router-dom";

// The open tab of a page, kept in the address (?tab=tithes), so the back button, a refresh and a shared link all
// land on the same tab. The first tab is the one shown when the address names none or names one that does not
// exist. Picking a tab changes the address without adding a step to the back button, and keeps the other
// parts of the query (a linked record, say).
export function useUrlTab(
  tabs: readonly { id: string }[],
): [string, (id: string) => void] {
  const [params, setParams] = useSearchParams();
  const asked = params.get("tab");
  const tab = tabs.find((t) => t.id === asked)?.id ?? tabs[0]?.id ?? "";
  const set = (id: string) =>
    setParams(
      (current) => {
        const next = new URLSearchParams(current);
        next.set("tab", id);
        return next;
      },
      { replace: true, preventScrollReset: true },
    );
  return [tab, set];
}
