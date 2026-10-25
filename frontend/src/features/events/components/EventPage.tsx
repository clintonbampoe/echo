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
  type Arrival,
  type Guest,
  type Phase,
} from "../roster";
import { rosterStats, waitingStats } from "../rosterStat";
import { CheckInsTab } from "./CheckInsTab";
import { DeleteEventDialog } from "./DeleteEventDialog";
import { EditEventSheet } from "./EditEventSheet";
import { EventHeader } from "./EventHeader";
import styles from "./EventPage.module.css";
import { RegistrationsTab } from "./RegistrationsTab";
import { EventDetails } from "./EventDetails";
import { RecordSheet } from "./RecordSheet";
import type { Mode } from "../record";

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

type BodyProps = {
  tab: string;
  event: EventItem;
  guests: Guest[];
  arrivals: Arrival[];
  phase: Phase;
  state: { loading: boolean; failed: boolean; retry: () => void };
};

// The tab that is open: who signed up, who came, or what the event is.
function TabBody({ tab, event, guests, arrivals, phase, state }: BodyProps) {
  if (tab === "details") return <EventDetails event={event} />;
  if (tab === "check-ins")
    return (
      <CheckInsTab
        arrivals={arrivals}
        eventName={event.name}
        phase={phase}
        state={state}
      />
    );
  return (
    <RegistrationsTab
      guests={guests}
      eventName={event.name}
      phase={phase}
      state={state}
    />
  );
}

function Detail({ event: e }: { event: EventItem }) {
  const navigate = useNavigate();
  const [tab, setTab] = useState("registrations");
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [recording, setRecording] = useState<Mode | null>(null);
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
    ? rosterStats({ event: e, arrivals, phase })
    : waitingStats(e, state.failed);
  const tabs = [
    { id: "registrations", label: "Registrations" },
    { id: "check-ins", label: "Check-ins" },
    { id: "details", label: "Details" },
  ];

  return (
    <div className="anim-rise">
      <BackLink to={paths.events}>Events</BackLink>
      <EventHeader
        event={e}
        onCheckIn={() => setRecording("check-in")}
        onRegister={() => setRecording("register")}
        onEdit={() => setEditing(true)}
        onDelete={() => setDeleting(true)}
      />
      <div>
        <StatStrip label="Event numbers" stats={stats} busy={state.loading} />
      </div>
      <div>
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
        <TabBody
          tab={tab}
          event={e}
          guests={guests}
          arrivals={arrivals}
          phase={phase}
          state={state}
        />
      </div>
      {recording && (
        <RecordSheet
          event={e}
          mode={recording}
          registered={new Set(regs.rows.map((r) => r.memberId))}
          checkedIn={new Set(checks.rows.map((c) => c.memberId))}
          onClose={() => setRecording(null)}
        />
      )}
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
