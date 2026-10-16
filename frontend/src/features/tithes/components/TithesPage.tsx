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
import { LinkedTithe } from "./LinkedTithe";
import { TitheChart } from "./TitheChart";
import { TitheDialogs, type TitheStart } from "./TitheDialogs";
import { TitheFilterBar } from "./TitheFilterBar";
import { TitheList } from "./TitheList";
import { TitheSheet } from "./TitheSheet";
import { TitheStats } from "./TitheStats";

// The Tithes page: the numbers and the monthly chart, the filters, and every tithe. The page only decides what a row is.
export function TithesPage() {
  const today = todayText();
  const { filters, update, clear, resetKey } = useUrlFilters(titheFilters);
  const [recording, setRecording] = useState(false);
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
        text="What members have given, and for which month."
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
        order="Newest first."
      >
        {(tithes) => (
          <TitheList
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
