import { useUrlFilters } from "@/lib/useUrlFilters";
import { windowOptions } from "@/lib/windows";
import { ListBody, PageHeader } from "@/ui";
import { FilterBar } from "./FilterBar";
import { toApiFilters, toSummaryFilters, visitorFilters } from "./filters";
import { useVisitors, useVisitorSummary } from "./hooks";
import { SummaryStrip } from "./SummaryStrip";
import { VisitorList } from "./VisitorList";
import { VisitorSheet } from "./VisitorSheet";
import { useUrlFlag } from "@/lib/useUrlFlag";

// The visitors list: header, numbers, filters and the rows. The page only decides what a row is.
export function VisitorsPage() {
  const { filters, update, clear, resetKey } = useUrlFilters(visitorFilters);
  const [adding, setAdding] = useUrlFlag("new");
  const list = useVisitors(toApiFilters(filters));
  const summary = useVisitorSummary(toSummaryFilters(filters));
  const windowLabel =
    windowOptions.find((o) => o.value === filters.window)?.label ?? "This year";

  return (
    <div className="anim-rise">
      <PageHeader
        title="Visitors"
        add={{ label: "Add visitor", onClick: () => setAdding(true) }}
      />
      <SummaryStrip summary={summary.data} windowLabel={windowLabel} />
      <FilterBar
        filters={filters}
        onChange={update}
        resetKey={resetKey}
        onClear={clear}
      />
      <ListBody
        list={list}
        icon="visitor"
        noun={["visitor", "visitors"]}
        emptyText="Visitors you add will appear here."
        filtered={visitorFilters.isFiltered(filters)}
        onClear={clear}
        footerInside
      >
        {(visitors, footer) => (
          <VisitorList visitors={visitors} footer={footer} />
        )}
      </ListBody>
      <VisitorSheet open={adding} onClose={() => setAdding(false)} />
    </div>
  );
}
