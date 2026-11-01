import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { Alert, BackLink, Button, StatStrip, Tabs } from "@/ui";
import type { EventItem } from "../api/events";
import { useEvent, useEventRoster } from "../hooks/queries";
import { eventPhase, type Phase } from "../model/roster";
import { rosterStats, waitingStats } from "../model/rosterFigures";
import { CheckInsTab } from "../components/event/CheckInsTab";
import { DeleteEventDialog } from "../components/dialogs/DeleteEventDialog";
import { EditEventSheet } from "../components/form/EditEventSheet";
import { EventHeader } from "../components/event/EventHeader";
import styles from "./EventPage.module.css";
import { RegistrationsTab } from "../components/event/RegistrationsTab";
import { EventDetails } from "../components/event/EventDetails";
import { RecordSheet } from "../components/record/RecordSheet";
import type { Mode } from "../model/record";

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

type BodyProps = { tab: string; event: EventItem; phase: Phase };

// The tab that is open: who signed up, who came, or what the event is. Only the open tab reads its list.
function TabBody({ tab, event, phase }: BodyProps) {
  if (tab === "details") return <EventDetails event={event} />;
  if (tab === "check-ins")
    return (
      <CheckInsTab eventId={event.id} eventName={event.name} phase={phase} />
    );
  return (
    <RegistrationsTab eventId={event.id} eventName={event.name} phase={phase} />
  );
}

function Detail({ event: e }: { event: EventItem }) {
  const navigate = useNavigate();
  const [tab, setTab] = useState("details");
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [recording, setRecording] = useState<Mode | null>(null);
  // The page needs two numbers about who came, not the lists: those load when their tab opens.
  const roster = useEventRoster(e.id);
  const phase = eventPhase(e, todayText());
  const stats = roster.data
    ? rosterStats({ event: e, roster: roster.data, phase })
    : waitingStats(e, roster.isError);
  const tabs = [
    { id: "details", label: "Details" },
    { id: "registrations", label: "Registrations" },
    { id: "check-ins", label: "Check-ins" },
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
        <StatStrip
          label="Event numbers"
          stats={stats}
          busy={roster.isPending}
        />
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
        <TabBody tab={tab} event={e} phase={phase} />
      </div>
      {recording && (
        <RecordSheet
          event={e}
          mode={recording}
          onClose={() => setRecording(null)}
        />
      )}
      {editing && (
        <EditEventSheet event={e} onClose={() => setEditing(false)} />
      )}
      {deleting && (
        <DeleteEventDialog
          event={e}
          checkIns={roster.data?.checkedIn}
          onClose={() => setDeleting(false)}
          onDeleted={() => navigate(paths.events, { replace: true })}
        />
      )}
    </div>
  );
}
