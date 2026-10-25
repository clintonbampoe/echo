import { useState } from "react";
import { todayText } from "@/lib/dates";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { ListBody, PageHeader } from "@/ui";
import type { Tithe } from "../api/tithes";
import {
  chooseForMonth,
  collectedOptions,
  titheFilters,
  toApiFilters,
} from "../model/filters";
import { useTithes, useTitheSummary } from "../hooks/queries";
import { LinkedTithe } from "../components/profile/LinkedTithe";
import { TitheChart } from "../components/list/TitheChart";
import {
  TitheDialogs,
  type TitheStart,
} from "../components/dialogs/TitheDialogs";
import { TitheFilterBar } from "../components/list/TitheFilterBar";
import { TitheList } from "../components/list/TitheList";
import { TitheSheet } from "../components/form/TitheSheet";
import { TitheStats } from "../components/list/TitheStats";
import { useUrlFlag } from "@/lib/useUrlFlag";

// The Tithes page: the numbers and the monthly chart, the filters, and every tithe. The page only decides what a row is.
export function TithesPage() {
  const today = todayText();
  const { filters, update, clear, resetKey } = useUrlFilters(titheFilters);
  const [recording, setRecording] = useUrlFlag("new");
  const [opened, setOpened] = useState<{
    tithe: Tithe;
    start: TitheStart;
  } | null>(null);
  const query = toApiFilters(filters, today);
  const list = useTithes(query);
  const summary = useTitheSummary(query);
  const collected =
    collectedOptions.find((o) => o.value === filters.window)?.label ?? "";

  return (
    <div className="anim-rise">
      <PageHeader
        title="Tithes"
        add={{ label: "Record tithe", onClick: () => setRecording(true) }}
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
        footerInside
      >
        {(tithes, footer) => (
          <TitheList
            footer={footer}
            tithes={tithes}
            onOpen={(tithe, start) => setOpened({ tithe, start })}
          />
        )}
      </ListBody>
      <TitheSheet
        open={recording}
        today={today}
        onClose={() => setRecording(false)}
        onUpdate={(tithe) => {
          setRecording(false);
          setOpened({ tithe, start: "top-up" });
        }}
      />
      {opened && (
        <TitheDialogs
          tithe={opened.tithe}
          start={opened.start}
          today={today}
          onClose={() => setOpened(null)}
        />
      )}
      <LinkedTithe today={today} />
    </div>
  );
}
