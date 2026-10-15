import { useState } from "react";
import { todayText } from "@/lib/dates";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { Button, Icon, ListBody, PageHeader } from "@/ui";
import type { Tithe } from "../api";
import {
  chooseForMonth,
  collectedOptions,
  titheFilters,
  toApiFilters,
} from "../filters";
import { useTithes, useTitheSummary } from "../hooks";
import { DeleteTitheDialog } from "./DeleteTitheDialog";
import { EditTitheSheet } from "./EditTitheSheet";
import { TitheSheet } from "./TitheSheet";
import { TitheChart } from "./TitheChart";
import { TitheList } from "./TitheList";
import { TitheFilterBar } from "./TitheFilterBar";
import { TitheStats } from "./TitheStats";

// The Tithes page: the numbers and the monthly chart, the filters, and every tithe. The page only decides what a row is.
export function TithesPage() {
  const today = todayText();
  const { filters, update, clear, resetKey } = useUrlFilters(titheFilters);
  const [recording, setRecording] = useState(false);
  const [editing, setEditing] = useState<{
    tithe: Tithe;
    topUp: boolean;
  } | null>(null);
  const [deleting, setDeleting] = useState<Tithe | null>(null);
  const query = toApiFilters(filters, today);
  const list = useTithes(query);
  const summary = useTitheSummary(query);
  const collected =
    collectedOptions.find((o) => o.value === filters.window)?.label ?? "";

  return (
    <div className="anim-rise">
      <PageHeader
        title="Tithes"
        text="What members give, and the month each tithe counts toward."
        action={
          <Button onClick={() => setRecording(true)}>
            <Icon name="plus" size={16} />
            Record tithe
          </Button>
        }
      />
      <TitheStats
        summary={summary.data}
        busy={summary.isPending}
        collected={collected}
      />
      <TitheChart
        filters={filters}
        today={today}
        onPick={(forMonth) => update(chooseForMonth(filters, forMonth))}
      />
      <TitheFilterBar
        filters={filters}
        onChange={update}
        resetKey={resetKey}
        onClear={clear}
        today={today}
      />
      <ListBody
        list={list}
        icon="tithes"
        noun={["tithe", "tithes"]}
        emptyTitle={`No tithes ${collected.toLowerCase()}`}
        emptyText="Record a tithe, or choose a wider range."
        filtered={titheFilters.isFiltered(filters)}
        onClear={clear}
      >
        {(tithes) => (
          <TitheList
            tithes={tithes}
            onEdit={(tithe) => setEditing({ tithe, topUp: false })}
            onDelete={setDeleting}
          />
        )}
      </ListBody>
      <TitheSheet
        open={recording}
        today={today}
        onClose={() => setRecording(false)}
        onUpdate={(tithe) => {
          setRecording(false);
          setEditing({ tithe, topUp: true });
        }}
      />
      {editing && (
        <EditTitheSheet
          tithe={editing.tithe}
          topUp={editing.topUp}
          today={today}
          onClose={() => setEditing(null)}
        />
      )}
      {deleting && (
        <DeleteTitheDialog tithe={deleting} onClose={() => setDeleting(null)} />
      )}
    </div>
  );
}
