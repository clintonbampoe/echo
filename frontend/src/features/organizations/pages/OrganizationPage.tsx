import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { OrganizationEvents, useUpcomingEvents } from "@/features/events";
import { isApiError } from "@/lib/api";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { Alert, BackLink, Button, StatStrip, Tabs } from "@/ui";
import type { Organization } from "../api/organizations";
import { AddMemberSheet } from "../components/members/AddMemberSheet";
import { DeleteOrganizationDialog } from "../components/dialogs/DeleteOrganizationDialog";
import { EditOrganizationSheet } from "../components/form/EditOrganizationSheet";
import { useOrganization, useOrganizationMembers } from "../hooks/queries";
import { OrganizationDetails } from "../components/profile/OrganizationDetails";
import { OrganizationHeader } from "../components/profile/OrganizationHeader";
import { OrganizationMembers } from "../components/members/OrganizationMembers";
import styles from "./OrganizationPage.module.css";
import {
  createdStat,
  ledByStat,
  membersStat,
  upcomingStat,
} from "../model/figures";

// One organization: who it is, who leads it, its members and the events it is hosting.
export function OrganizationPage() {
  const { id = "" } = useParams();
  const organization = useOrganization(id);

  if (organization.isPending)
    return (
      <p role="status" className={styles.note}>
        Loading organization…
      </p>
    );
  if (organization.isError) {
    const missing =
      isApiError(organization.error) && organization.error.status === 404;
    return (
      <div className={styles.note}>
        <BackLink to={paths.organizations}>Organizations</BackLink>
        <Alert
          title={
            missing
              ? "We could not find this organization"
              : "We could not load this organization"
          }
        >
          {missing
            ? "It may have been deleted."
            : "Check your connection and try again."}
        </Alert>
        {!missing && (
          <Button
            variant="secondary"
            onClick={() => void organization.refetch()}
          >
            Try again
          </Button>
        )}
      </div>
    );
  }
  return <Detail organization={organization.data} />;
}

function Detail({ organization: o }: { organization: Organization }) {
  const navigate = useNavigate();
  const [tab, setTab] = useState("details");
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [adding, setAdding] = useState(false);
  const today = todayText();
  const leaders = useOrganizationMembers(o.id, { role: "Leader" });
  const events = useUpcomingEvents(o.id, today);
  const everyone = useOrganizationMembers(o.id, {});
  const taken = new Set(everyone.rows.map((m) => m.memberId));
  const stats = [
    membersStat(o),
    ledByStat(
      leaders.rows.map((m) => m.memberName),
      leaders.loading,
    ),
    upcomingStat(
      events.rows.map((e) => e.startDate),
      events.hasMore,
      today,
      events.loading,
    ),
    createdStat(o, today),
  ];
  const tabs = [
    { id: "details", label: "Details" },
    { id: "members", label: "Members" },
    { id: "events", label: "Events" },
  ];

  return (
    <div className="anim-rise">
      <BackLink to={paths.organizations}>Organizations</BackLink>
      <OrganizationHeader
        organization={o}
        onEdit={() => setEditing(true)}
        onDelete={() => setDeleting(true)}
        onAdd={() => setAdding(true)}
      />
      <StatStrip label="Organization numbers" stats={stats} />
      <div>
        <Tabs
          label="Organization"
          tabs={tabs}
          value={tab}
          onChange={setTab}
          idPrefix="organization"
        />
      </div>
      <div
        role="tabpanel"
        id={`organization-panel-${tab}`}
        aria-labelledby={`organization-tab-${tab}`}
      >
        {tab === "details" && (
          <OrganizationDetails
            organization={o}
            leaders={everyone.rows.filter((m) => m.role !== "Member")}
          />
        )}
        {tab === "members" && <OrganizationMembers organization={o} />}
        {tab === "events" && <OrganizationEvents list={events} />}
      </div>
      {editing && (
        <EditOrganizationSheet
          organization={o}
          onClose={() => setEditing(false)}
        />
      )}
      {adding && (
        <AddMemberSheet
          organization={o}
          taken={taken}
          onClose={() => setAdding(false)}
        />
      )}
      {deleting && (
        <DeleteOrganizationDialog
          organization={o}
          onClose={() => setDeleting(false)}
          onDeleted={() => navigate(paths.organizations, { replace: true })}
        />
      )}
    </div>
  );
}
