import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { Alert, BackLink, Button, StatStrip, Tabs } from "@/ui";
import type { EventItem } from "../api";
import { useEvent, useEventCheckIns, useEventRegistrations } from "../hooks";
import {
  arrivals as joinArrivals,
  eventPhase,
  guests as joinGuests,
} from "../roster";
import { rosterStats, waitingStats } from "../rosterStat";
import { CheckInsTab } from "./CheckInsTab";
import { DeleteEventDialog } from "./DeleteEventDialog";
import { EditEventSheet } from "./EditEventSheet";
import { EventHeader } from "./EventHeader";
import styles from "./EventPage.module.css";
import { RegistrationsTab } from "./RegistrationsTab";
import { RecordPanel } from "../RecordPanel";

// One event: what it is, who signed up and who came.
export function EventPage() {
  const { id = "" } = useParams();
  const event = useEvent(id);

  if (event.isPending)
    return (
      <p role="status" className={styles.note}>
        Loading event…
      </p>
    );
  if (event.isError) {
    const missing = isApiError(event.error) && event.error.status === 404;
    return (
      <div className={styles.note}>
        <BackLink to={paths.events}>Events</BackLink>
        <Alert
          title={
            missing
              ? "We could not find this event"
              : "We could not load this event"
          }
        >
          {missing
            ? "It may have been deleted."
            : "Check your connection and try again."}
        </Alert>
        {!missing && (
          <Button variant="secondary" onClick={() => void event.refetch()}>
            Try again
          </Button>
        )}
      </div>
    );
  }
  return <Detail event={event.data} />;
}

function Detail({ event: e }: { event: EventItem }) {
  const navigate = useNavigate();
  const [tab, setTab] = useState("registrations");
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const regs = useEventRegistrations(e.id);
  const checks = useEventCheckIns(e.id);
  const phase = eventPhase(e, todayText());
  const state = {
    loading: regs.loading || checks.loading,
    failed: regs.failed || checks.failed,
    retry: () => (regs.retry(), checks.retry()),
  };
  const guests = joinGuests(regs.rows, checks.rows);
  const arrivals = joinArrivals(checks.rows, regs.rows);
  const ready = !state.loading && !state.failed;
  const stats = ready
    ? rosterStats({ event: e, guests, arrivals, phase })
    : waitingStats(e, state.failed);
  const tabs = [
    { id: "registrations", label: "Registrations", count: e.registrationCount },
    {
      id: "check-ins",
      label: "Check-ins",
      count: ready ? arrivals.length : null,
    },
  ];

  return (
    <div className="anim-rise">
      <BackLink to={paths.events}>Events</BackLink>
      <EventHeader
        event={e}
        onEdit={() => setEditing(true)}
        onDelete={() => setDeleting(true)}
      />
      <div className={styles.panel}>
        <RecordPanel
          event={e}
          registered={new Set(regs.rows.map((r) => r.memberId))}
          checkedIn={new Set(checks.rows.map((c) => c.memberId))}
        />
      </div>
      <div className={styles.stats}>
        <StatStrip label="Event numbers" stats={stats} busy={state.loading} />
      </div>
      <div className={styles.tabs}>
        <Tabs
          label="Event"
          tabs={tabs}
          value={tab}
          onChange={setTab}
          idPrefix="event"
        />
      </div>
      <div
        role="tabpanel"
        id={`event-panel-${tab}`}
        aria-labelledby={`event-tab-${tab}`}
      >
        {tab === "registrations" ? (
          <RegistrationsTab
            guests={guests}
            eventName={e.name}
            phase={phase}
            state={state}
          />
        ) : (
          <CheckInsTab
            arrivals={arrivals}
            eventName={e.name}
            phase={phase}
            state={state}
          />
        )}
      </div>
      {editing && (
        <EditEventSheet event={e} onClose={() => setEditing(false)} />
      )}
      {deleting && (
        <DeleteEventDialog
          event={e}
          checkIns={ready ? arrivals.length : undefined}
          onClose={() => setDeleting(false)}
          onDeleted={() => navigate(paths.events, { replace: true })}
        />
      )}
    </div>
  );
}
