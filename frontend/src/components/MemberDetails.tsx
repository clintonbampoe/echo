import React, { useEffect } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import type {
  InfiniteData,
  UseInfiniteQueryResult,
} from "@tanstack/react-query";
import { useLayout } from "../hooks/useLayout";
import { useMember } from "../hooks/useMembers";
import { useMemberActivity } from "../hooks/useMemberProfile";
import { memberProfileService } from "../services/memberProfileService";
import type { Member, MemberStatus } from "../types/member";
import type { MemberActivitySection } from "../types/memberProfile";
import type { PagedResponse } from "../types/pagination";
import {
  AttendanceIcon,
  CalendarIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ClockIcon,
  OrganizationsIcon,
  ProjectsIcon,
  RecordIcon,
  TitheIcon,
} from "./Icons";
import { getErrorMessage } from "../utils/errors";
import "../styles/MemberDetails.css";

// ─── Sections ─────────────────────────────────────────────────────────────────

interface SectionInfo {
  key: MemberActivitySection;
  label: string;
  description: string;
  icon: React.FC<{ size?: number | string }>;
}

const SECTIONS: SectionInfo[] = [
  {
    key: "attendance",
    label: "View attendance",
    description: "Church services this member has attended",
    icon: AttendanceIcon,
  },
  {
    key: "tithes",
    label: "View tithes",
    description: "Tithes paid by this member",
    icon: TitheIcon,
  },
  {
    key: "organizations",
    label: "View organizations",
    description: "Groups this member belongs to and their role",
    icon: OrganizationsIcon,
  },
  {
    key: "events-attended",
    label: "View events attended",
    description: "Events this member checked in to",
    icon: CalendarIcon,
  },
  {
    key: "events-registered",
    label: "View events registered",
    description: "Events this member signed up for",
    icon: RecordIcon,
  },
  {
    key: "projects-managed",
    label: "View projects managed",
    description: "Projects led by this member",
    icon: ProjectsIcon,
  },
  {
    key: "events-organized",
    label: "View events organized",
    description: "Events this member is the organizer of",
    icon: ClockIcon,
  },
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Parses YYYY-MM-DD as a local date so the day never shifts across timezones. */
const formatDate = (value?: string | null) => {
  if (!value) return "—";
  const [y, m, d] = value.slice(0, 10).split("-").map(Number);
  const date = value.length === 10 ? new Date(y, m - 1, d) : new Date(value);
  if (isNaN(date.getTime())) return value;
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
};

const formatTime = (value?: string | null) => {
  if (!value) return "—";
  const [h, m] = value.split(":");
  const hours = parseInt(h, 10);
  if (isNaN(hours)) return value;
  return `${hours % 12 || 12}:${m} ${hours >= 12 ? "PM" : "AM"}`;
};

const formatCurrency = (amount: number) =>
  `₵ ${amount.toLocaleString("en-US", { minimumFractionDigits: 0, maximumFractionDigits: 2 })}`;

/** "GreaterAccra" → "Greater Accra", "MobileMoney" → "Mobile Money". */
const splitWords = (value?: string | null) =>
  value ? value.replace(/([a-z])([A-Z])/g, "$1 $2") : "—";

const getAge = (dateOfBirth?: string | null) => {
  if (!dateOfBirth) return null;
  const [y, m, d] = dateOfBirth.split("-").map(Number);
  const today = new Date();
  let age = today.getFullYear() - y;
  if (today.getMonth() + 1 < m || (today.getMonth() + 1 === m && today.getDate() < d))
    age--;
  return age;
};

const statusClass = (s: MemberStatus) =>
  s === "Active" ? "status-active" : "status-inactive";

const fullName = (member: Member) =>
  [member.firstName, member.otherNames, member.lastName]
    .filter(Boolean)
    .join(" ");

// ─── Activity table ───────────────────────────────────────────────────────────

interface Column<T> {
  header: string;
  render: (row: T) => React.ReactNode;
}

interface ActivityTableProps<T> {
  query: UseInfiniteQueryResult<InfiniteData<PagedResponse<T>>>;
  columns: Column<T>[];
  getKey: (row: T) => string;
  emptyText: string;
}

function ActivityTable<T>({
  query,
  columns,
  getKey,
  emptyText,
}: ActivityTableProps<T>) {
  const rows = query.data?.pages.flatMap((p) => p.data) ?? [];

  if (query.isLoading) {
    return <div className="member-details-state">Loading…</div>;
  }

  if (query.isError) {
    return (
      <div className="member-details-state error">
        {getErrorMessage(query.error, "Failed to load records.")}
      </div>
    );
  }

  if (rows.length === 0) {
    return <div className="member-details-state">{emptyText}</div>;
  }

  return (
    <>
      <div className="member-activity-table-wrap">
        <table className="member-activity-table">
          <thead>
            <tr>
              {columns.map((c) => (
                <th key={c.header}>{c.header}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={getKey(row)}>
                {columns.map((c) => (
                  <td key={c.header}>{c.render(row)}</td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {query.hasNextPage && (
        <div className="member-activity-more">
          <button
            type="button"
            className="member-action-btn"
            onClick={() => query.fetchNextPage()}
            disabled={query.isFetchingNextPage}
          >
            {query.isFetchingNextPage ? "Loading…" : "Load more"}
          </button>
        </div>
      )}
    </>
  );
}

// ─── One component per section (each owns its typed query) ───────────────────

const AttendanceActivity = ({ memberId }: { memberId: string }) => (
  <ActivityTable
    query={useMemberActivity("attendance", memberId, memberProfileService.attendance)}
    getKey={(r) => r.id}
    emptyText="No church attendance recorded for this member."
    columns={[
      { header: "Date", render: (r) => formatDate(r.date) },
      { header: "Service", render: (r) => r.attendanceTypeName },
      { header: "Check-in", render: (r) => formatTime(r.checkInTime) },
      { header: "Notes", render: (r) => r.notes || "—" },
    ]}
  />
);

const TithesActivity = ({ memberId }: { memberId: string }) => (
  <ActivityTable
    query={useMemberActivity("tithes", memberId, memberProfileService.tithes)}
    getKey={(r) => r.id}
    emptyText="No tithes recorded for this member."
    columns={[
      { header: "Collected", render: (r) => formatDate(r.collectionDate) },
      { header: "For", render: (r) => `${r.forMonth} ${r.forYear}` },
      { header: "Amount", render: (r) => formatCurrency(r.amount) },
      { header: "Payment method", render: (r) => splitWords(r.paymentMethod) },
    ]}
  />
);

const OrganizationsActivity = ({ memberId }: { memberId: string }) => (
  <ActivityTable
    query={useMemberActivity(
      "organizationMembers",
      memberId,
      memberProfileService.organizations,
    )}
    getKey={(r) => r.id}
    emptyText="This member doesn't belong to any organization."
    columns={[
      { header: "Organization", render: (r) => r.organizationName },
      { header: "Role", render: (r) => r.role },
      { header: "Joined", render: (r) => formatDate(r.joinedAt) },
    ]}
  />
);

const EventsAttendedActivity = ({ memberId }: { memberId: string }) => (
  <ActivityTable
    query={useMemberActivity(
      "eventAttendance",
      memberId,
      memberProfileService.eventsAttended,
    )}
    getKey={(r) => r.id}
    emptyText="This member hasn't checked in to any event."
    columns={[
      { header: "Event", render: (r) => r.eventName },
      { header: "Check-in", render: (r) => formatTime(r.checkInTime) },
      { header: "Recorded", render: (r) => formatDate(r.createdAt) },
    ]}
  />
);

const EventsRegisteredActivity = ({ memberId }: { memberId: string }) => (
  <ActivityTable
    query={useMemberActivity(
      "eventRegistrations",
      memberId,
      memberProfileService.eventsRegistered,
    )}
    getKey={(r) => r.id}
    emptyText="This member hasn't registered for any event."
    columns={[
      { header: "Event", render: (r) => r.eventName },
      { header: "Registered on", render: (r) => formatDate(r.registrationDate) },
    ]}
  />
);

const ProjectsManagedActivity = ({ memberId }: { memberId: string }) => (
  <ActivityTable
    query={useMemberActivity("projects", memberId, memberProfileService.projectsManaged)}
    getKey={(r) => r.id}
    emptyText="This member doesn't manage any project."
    columns={[
      { header: "Project", render: (r) => r.name },
      { header: "Category", render: (r) => r.categoryName },
      { header: "Status", render: (r) => splitWords(r.status) },
      { header: "Start", render: (r) => formatDate(r.startDate) },
      { header: "Target", render: (r) => formatCurrency(r.targetAmount) },
    ]}
  />
);

const EventsOrganizedActivity = ({ memberId }: { memberId: string }) => (
  <ActivityTable
    query={useMemberActivity("events", memberId, memberProfileService.eventsOrganized)}
    getKey={(r) => r.id}
    emptyText="This member hasn't organized any event."
    columns={[
      { header: "Event", render: (r) => r.name },
      { header: "Organization", render: (r) => r.organizationName },
      {
        header: "Dates",
        render: (r) =>
          r.startDate === r.endDate
            ? formatDate(r.startDate)
            : `${formatDate(r.startDate)} – ${formatDate(r.endDate)}`,
      },
      { header: "Location", render: (r) => r.location || "—" },
    ]}
  />
);

const ACTIVITY_VIEWS: Record<
  MemberActivitySection,
  React.FC<{ memberId: string }>
> = {
  attendance: AttendanceActivity,
  tithes: TithesActivity,
  organizations: OrganizationsActivity,
  "events-attended": EventsAttendedActivity,
  "events-registered": EventsRegisteredActivity,
  "projects-managed": ProjectsManagedActivity,
  "events-organized": EventsOrganizedActivity,
};

// ─── Details view ─────────────────────────────────────────────────────────────

const DetailField = ({
  label,
  value,
}: {
  label: string;
  value?: React.ReactNode;
}) => (
  <div className="member-detail-field">
    <span className="member-detail-label">{label}</span>
    <span className="member-detail-value">{value || "—"}</span>
  </div>
);

const MemberProfile = ({ member }: { member: Member }) => {
  const age = getAge(member.dateOfBirth);

  return (
    <>
      <div className="member-details-card member-profile-header">
        <div className="member-profile-avatar">
          {`${member.firstName[0] ?? ""}${member.lastName[0] ?? ""}`.toUpperCase()}
        </div>
        <div className="member-profile-identity">
          <h2 className="member-profile-name">{fullName(member)}</h2>
          <span className="member-profile-sub">
            Member ID {member.id.split("-")[0]} · Joined{" "}
            {formatDate(member.joinedDate)}
          </span>
        </div>
        <span className={`member-status-badge ${statusClass(member.status)}`}>
          {member.status}
        </span>
      </div>

      <div className="member-details-grid">
        <section className="member-details-card">
          <h3 className="member-details-section-title">Contact</h3>
          <DetailField label="Phone" value={member.phoneNumber} />
          <DetailField label="Email" value={member.emailAddress} />
        </section>

        <section className="member-details-card">
          <h3 className="member-details-section-title">Personal</h3>
          <DetailField
            label="Date of birth"
            value={
              member.dateOfBirth
                ? `${formatDate(member.dateOfBirth)}${age !== null ? ` (${age} yrs)` : ""}`
                : null
            }
          />
          <DetailField label="Gender" value={member.gender} />
          <DetailField label="Marital status" value={member.maritalStatus} />
        </section>

        <section className="member-details-card">
          <h3 className="member-details-section-title">Address</h3>
          <DetailField label="Residential address" value={member.residentialAddress} />
          <DetailField label="City" value={member.city} />
          <DetailField label="Hometown" value={member.hometown} />
          <DetailField label="Region" value={splitWords(member.region)} />
          <DetailField label="GPS address" value={member.gpsAddress} />
        </section>

        <section className="member-details-card">
          <h3 className="member-details-section-title">Emergency</h3>
          <DetailField label="Next of kin" value={member.nextOfKin} />
          <DetailField label="Emergency contact" value={member.emergencyContactName} />
          <DetailField
            label="Emergency phone"
            value={member.emergencyContactPhoneNumber}
          />
        </section>
      </div>

      <section className="member-details-card">
        <h3 className="member-details-section-title">Activity</h3>
        <div className="member-activity-links">
          {SECTIONS.map(({ key, label, description, icon: Icon }) => (
            <Link
              key={key}
              to={`/members/${member.id}/${key}`}
              className="member-activity-link"
            >
              <span className="member-activity-link-icon">
                <Icon size={18} />
              </span>
              <span className="member-activity-link-text">
                <span className="member-activity-link-label">{label}</span>
                <span className="member-activity-link-desc">{description}</span>
              </span>
              <ChevronRightIcon size={16} />
            </Link>
          ))}
        </div>
      </section>
    </>
  );
};

// ─── Page ─────────────────────────────────────────────────────────────────────

const MemberDetails: React.FC = () => {
  const { memberId = "", section } = useParams();
  const navigate = useNavigate();
  const { setTitle, setCtas } = useLayout();
  const { data: member, isLoading, isError, error } = useMember(memberId);

  const activeSection = SECTIONS.find((s) => s.key === section);
  const name = member ? `${member.firstName} ${member.lastName}` : "Member";

  useEffect(() => {
    const backTo = activeSection ? `/members/${memberId}` : "/members";
    setTitle(
      <button type="button" className="back-btn" onClick={() => navigate(backTo)}>
        <ChevronLeftIcon size={20} />
        <span>
          Members / {name}
          {activeSection ? ` / ${activeSection.label.replace(/^View /, "")}` : ""}
        </span>
      </button>,
    );
    setCtas([]);
  }, [setTitle, setCtas, navigate, memberId, name, activeSection]);

  if (isLoading) {
    return <div className="member-details-container member-details-state">Loading…</div>;
  }

  if (isError || !member) {
    return (
      <div className="member-details-container">
        <div className="member-details-state error">
          {isError
            ? getErrorMessage(error, "Failed to load member.")
            : "Member not found."}
        </div>
      </div>
    );
  }

  if (section && !activeSection) {
    return (
      <div className="member-details-container">
        <div className="member-details-state">
          Unknown section. <Link to={`/members/${memberId}`}>Back to profile</Link>
        </div>
      </div>
    );
  }

  if (activeSection) {
    const ActivityView = ACTIVITY_VIEWS[activeSection.key];
    return (
      <div className="member-details-container">
        <section className="member-details-card member-activity-card">
          <div className="member-activity-header">
            <h3 className="member-details-section-title">
              {activeSection.label.replace(/^View /, "").replace(/^./, (c) => c.toUpperCase())}
            </h3>
            <span className="member-activity-subtitle">{activeSection.description}</span>
          </div>
          <ActivityView memberId={memberId} />
        </section>
      </div>
    );
  }

  return (
    <div className="member-details-container">
      <MemberProfile member={member} />
    </div>
  );
};

export default MemberDetails;
