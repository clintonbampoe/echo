import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import {
  Button,
  EmptyState,
  Icon,
  ListBody,
  PageHeader,
  StatStrip,
  Tabs,
} from "@/ui";
import { eventFilters, toApiFilters, type EventTab } from "../model/filters";
import { useEvents, useEventSummary, useNextEvent } from "../hooks/queries";
import { summaryStats } from "../model/figures";
import type { EventItem } from "../api/events";
import { DeleteEventDialog } from "../components/dialogs/DeleteEventDialog";
import { EditEventSheet } from "../components/form/EditEventSheet";
import { EventList } from "../components/list/EventList";
import { EventSheet } from "../components/form/EventSheet";
import { EventsFilterBar } from "../components/list/EventsFilterBar";
import { useUrlFlag } from "@/lib/useUrlFlag";
import styles from "./EventsPage.module.css";

const text = {
  upcoming: {
    noun: ["upcoming event", "upcoming events"],
    title: "No upcoming events",
    empty: "Events you plan will appear here.",
    order: "Soonest first.",
  },
  past: {
    noun: ["past event", "past events"],
    title: "No past events",
    empty: "Events that have taken place will appear here.",
    order: "Newest first.",
  },
} as const;

// The Events page: the numbers, the two tabs and the list of the open one. Each tab is its own address.
export function EventsPage({ tab }: { tab: EventTab }) {
  const navigate = useNavigate();
  const [adding, setAdding] = useUrlFlag("new");
  const [editing, setEditing] = useState<EventItem | null>(null);
  const [deleting, setDeleting] = useState<EventItem | null>(null);
  const { filters, update, clear, resetKey } = useUrlFilters(eventFilters);
  const today = todayText();
  const list = useEvents(toApiFilters(filters, tab, today));
  const summary = useEventSummary(filters.organization || undefined);
  const next = useNextEvent(filters.organization || undefined, today);
  const filtered = eventFilters.isFiltered(filters);
  const none = !filtered && summary.data?.totalEvents === 0;
  const t = text[tab];
  const add = (
    <Button onClick={() => setAdding(true)}>
      <Icon name="plus" size={16} />
      New event
    </Button>
  );
  const tabs = [
    { id: "upcoming", label: "Upcoming" },
    { id: "past", label: "Past" },
  ];

  return (
    <div className="anim-rise">
      <PageHeader
        title="Events"
        add={{ label: "New event", onClick: () => setAdding(true) }}
      />
      {none ? (
        <EmptyState icon="calendar" title="Plan your first event" action={add}>
          Events are one-off gatherings like a convention or concert. Pick the
          group hosting it and who is in charge, then register members and check
          them in on the day.
        </EmptyState>
      ) : (
        <>
          <Tabs
            label="Events"
            idPrefix="events"
            tabs={tabs}
            value={tab}
            onChange={(id) =>
              navigate(id === "past" ? paths.eventsPast : paths.events)
            }
          />
          <StatStrip
            label="Event summary"
            stats={summaryStats(summary.data, next.data, tab)}
            busy={summary.isPending}
          />
          <div
            role="tabpanel"
            id={`events-panel-${tab}`}
            aria-labelledby={`events-tab-${tab}`}
            className={styles.panel}
          >
            <EventsFilterBar
              filters={filters}
              onChange={update}
              resetKey={resetKey}
              onClear={clear}
            />
            <ListBody
              list={list}
              icon="calendar"
              noun={[...t.noun]}
              emptyTitle={t.title}
              emptyText={t.empty}
              filtered={filtered}
              onClear={clear}
              order={t.order}
              footerInside
            >
              {(events, footer) => (
                <EventList
                  events={events}
                  past={tab === "past"}
                  footer={footer}
                  onEdit={setEditing}
                  onDelete={setDeleting}
                />
              )}
            </ListBody>
          </div>
        </>
      )}
      <EventSheet open={adding} onClose={() => setAdding(false)} />
      {editing && (
        <EditEventSheet event={editing} onClose={() => setEditing(null)} />
      )}
      {deleting && (
        <DeleteEventDialog event={deleting} onClose={() => setDeleting(null)} />
      )}
    </div>
  );
}
