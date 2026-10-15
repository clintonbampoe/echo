import { useState } from "react";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { windowOptions } from "@/lib/windows";
import { Button, Icon, ListBody, PageHeader } from "@/ui";
import type { Visitor } from "./api";
import { ConvertSheet } from "./ConvertSheet";
import { DeleteVisitorDialog } from "./DeleteVisitorDialog";
import { EditVisitorSheet } from "./EditVisitorSheet";
import { FilterBar } from "./FilterBar";
import { toApiFilters, toSummaryFilters, visitorFilters } from "./filters";
import { useVisitors, useVisitorSummary } from "./hooks";
import { SummaryStrip } from "./SummaryStrip";
import { VisitorList } from "./VisitorList";
import { VisitorSheet } from "./VisitorSheet";

// The visitors list: header, numbers, filters and the rows. The page only decides what a row is.
export function VisitorsPage() {
  const { filters, update, clear, resetKey } = useUrlFilters(visitorFilters);
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<Visitor | null>(null);
  const [converting, setConverting] = useState<Visitor | null>(null);
  const [deleting, setDeleting] = useState<Visitor | null>(null);
  const list = useVisitors(toApiFilters(filters));
  const summary = useVisitorSummary(toSummaryFilters(filters));
  const windowLabel =
    windowOptions.find((o) => o.value === filters.window)?.label ?? "This year";

  return (
    <div className="anim-rise">
      <PageHeader
        title="Visitors"
        text="First-time guests, and the people you are following up with."
        action={
          <Button onClick={() => setAdding(true)}>
            <Icon name="plus" size={16} />
            New visitor
          </Button>
        }
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
      >
        {(visitors) => (
          <VisitorList
            visitors={visitors}
            converted={filters.status === "became"}
            onConvert={setConverting}
            onEdit={setEditing}
            onDelete={setDeleting}
          />
        )}
      </ListBody>
      <VisitorSheet open={adding} onClose={() => setAdding(false)} />
      {editing && (
        <EditVisitorSheet visitor={editing} onClose={() => setEditing(null)} />
      )}
      {converting && (
        <ConvertSheet
          visitor={converting}
          onClose={() => setConverting(null)}
        />
      )}
      {deleting && (
        <DeleteVisitorDialog
          visitor={deleting}
          onClose={() => setDeleting(null)}
        />
      )}
    </div>
  );
}
