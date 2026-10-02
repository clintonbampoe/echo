import React, { useCallback, useEffect, useState } from "react";
import { useLayout } from "../hooks/useLayout";
import {
  useMembers,
  useCreateMember,
  useUpdateMember,
  useDeleteMember,
} from "../hooks/useMembers";
import type {
  Member,
  MemberStatus,
  MemberGender,
  MemberMaritalStatus,
  MemberRegion,
  MemberCreatePayload,
} from "../types/member";
import { CloseIcon, MembersIcon, CalendarIcon } from "./Icons";
import DeleteConfirmModal from "./common/DeleteConfirmModal";
import ExportPanel from "./ExportPanel";
import { getErrorMessage } from "../utils/errors";
import "../styles/Members.css";

// ─── Constants ────────────────────────────────────────────────────────────────

const GHANA_REGIONS: { value: MemberRegion; label: string }[] = [
  { value: "GreaterAccra", label: "Greater Accra Region" },
  { value: "Ashanti", label: "Ashanti Region" },
  { value: "Eastern", label: "Eastern Region" },
  { value: "Western", label: "Western Region" },
  { value: "Central", label: "Central Region" },
  { value: "Northern", label: "Northern Region" },
  { value: "UpperEast", label: "Upper East Region" },
  { value: "UpperWest", label: "Upper West Region" },
  { value: "Volta", label: "Volta Region" },
  { value: "Bono", label: "Bono Region" },
  { value: "Oti", label: "Oti Region" },
  { value: "Savannah", label: "Savannah Region" },
  { value: "NorthEast", label: "North East Region" },
  { value: "BonoEast", label: "Bono East Region" },
  { value: "Ahafo", label: "Ahafo Region" },
  { value: "WesternNorth", label: "Western North Region" },
];

const STATUS_TABS: Partial<Record<string, MemberStatus>> = {
  Active: "Active",
  Inactive: "Inactive",
  Archived: "Archived",
};

// ─── Helpers ──────────────────────────────────────────────────────────────────

const getInitials = (firstName: string, lastName: string) =>
  `${firstName[0] ?? ""}${lastName[0] ?? ""}`.toUpperCase();

const statusClass = (s: MemberStatus) => {
  if (s === "Active") return "status-active";
  if (s === "Inactive") return "status-inactive";
  if (s === "Archived") return "status-inactive";
  if (s === "Transferred") return "status-inactive";
  return "status-inactive";
};

// ─── Form State ───────────────────────────────────────────────────────────────

interface MemberFormState {
  firstName: string;
  lastName: string;
  otherNames: string;
  emailAddress: string;
  phoneNumber: string;
  dateOfBirth: string;
  joinedDate: string;
  gender: MemberGender;
  residentialAddress: string;
  city: string;
  hometown: string;
  region: MemberRegion | "";
  gpsAddress: string;
  maritalStatus: MemberMaritalStatus;
  nextOfKin: string;
  emergencyContactName: string;
  emergencyContactPhoneNumber: string;
  status: MemberStatus;
}

const emptyForm = (): MemberFormState => ({
  firstName: "",
  lastName: "",
  otherNames: "",
  emailAddress: "",
  phoneNumber: "",
  dateOfBirth: "",
  joinedDate: "",
  gender: "Male",
  residentialAddress: "",
  city: "Accra",
  hometown: "",
  region: "",
  gpsAddress: "",
  maritalStatus: "Single",
  nextOfKin: "",
  emergencyContactName: "",
  emergencyContactPhoneNumber: "",
  status: "Active",
});

function toCreatePayload(form: MemberFormState): MemberCreatePayload {
  return {
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    otherNames: form.otherNames.trim() || null,
    emailAddress: form.emailAddress.trim() || null,
    phoneNumber: form.phoneNumber.trim(),
    dateOfBirth: form.dateOfBirth,
    joinedDate: form.joinedDate || null,
    gender: form.gender,
    residentialAddress: form.residentialAddress.trim(),
    city: form.city.trim(),
    hometown: form.hometown.trim(),
    region: form.region as MemberRegion,
    gpsAddress: form.gpsAddress.trim() || null,
    maritalStatus: form.maritalStatus,
    nextOfKin: form.nextOfKin.trim(),
    emergencyContactName: form.emergencyContactName.trim(),
    emergencyContactPhoneNumber: form.emergencyContactPhoneNumber.trim(),
    status: form.status,
  };
}

// ─── Members Component ────────────────────────────────────────────────────────

const Members: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  const [activeTab, setActiveTab] = useState<string>("All Members");
  const [searchQuery, setSearchQuery] = useState("");

  const backendStatus = STATUS_TABS[activeTab];

  const { data: pagedResponse, isLoading } = useMembers(
    {
      name: searchQuery || undefined,
      status: backendStatus,
    },
    500,
  );

  const members = pagedResponse?.data || [];

  const createMember = useCreateMember();
  const updateMember = useUpdateMember();
  const deleteMember = useDeleteMember();

  const [showAddPanel, setShowAddPanel] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [editingMember, setEditingMember] = useState<Member | null>(null);

  const [form, setForm] = useState<MemberFormState>(emptyForm());
  const [panelError, setPanelError] = useState<string | null>(null);

  // ── Layout header ─────────────────────────────────────────────────────────

  const openAddPanel = useCallback(() => {
    setPanelError(null);
    setForm(emptyForm());
    setShowAddPanel(true);
  }, []);

  useEffect(() => {
    setTitle("Members");
    setCtas([
      {
        type: "button",
        label: "Export",
        icon: "export",
        variant: "secondary",
        onClick: () => setShowExportModal(true),
      },
      {
        type: "button",
        label: "Add Member",
        icon: "plus",
        variant: "primary",
        onClick: openAddPanel,
      },
    ]);
  }, [setTitle, setCtas, openAddPanel]);

  // ── Derived stats ─────────────────────────────────────────────────────────

  const totalMembership = members.length;
  const activeMembers = members.filter((m) => m.status === "Active").length;
  const archivedMembers = members.filter((m) => m.status === "Archived").length;
  const activeFamilies = new Set(
    members.filter((m) => m.status === "Active").map((m) => m.lastName),
  ).size;
  const retentionRate =
    totalMembership === 0
      ? 0
      : Math.round((activeMembers / totalMembership) * 100);

  // ── Handlers ──────────────────────────────────────────────────────────────

  const openEditPanel = (member: Member) => {
    setPanelError(null);
    setEditingMember(member);
    setForm({
      firstName: member.firstName,
      lastName: member.lastName,
      otherNames: member.otherNames || "",
      emailAddress: member.emailAddress || "",
      phoneNumber: member.phoneNumber,
      dateOfBirth: member.dateOfBirth || "",
      joinedDate: member.joinedDate || "",
      gender: member.gender,
      residentialAddress: member.residentialAddress,
      city: member.city,
      hometown: member.hometown,
      region: member.region,
      gpsAddress: member.gpsAddress || "",
      maritalStatus: member.maritalStatus,
      nextOfKin: member.nextOfKin,
      emergencyContactName: member.emergencyContactName,
      emergencyContactPhoneNumber: member.emergencyContactPhoneNumber,
      status: member.status,
    });
  };

  const formatDateDisplay = (iso?: string | null) => {
    if (!iso) return "";
    const d = new Date(iso);
    return d.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const validateForm = (): string | null => {
    if (!form.firstName.trim() || !form.lastName.trim()) {
      return "First name and last name are required.";
    }
    if (!form.phoneNumber.trim()) {
      return "Phone number is required.";
    }
    if (!form.dateOfBirth) {
      return "Date of birth is required.";
    }
    if (!form.region) {
      return "Please select a region.";
    }
    if (!form.residentialAddress.trim()) {
      return "Residential address is required.";
    }
    if (!form.city.trim()) {
      return "City is required.";
    }
    if (!form.hometown.trim()) {
      return "Hometown is required.";
    }
    if (!form.nextOfKin.trim()) {
      return "Next of kin is required.";
    }
    if (!form.emergencyContactName.trim()) {
      return "Emergency contact name is required.";
    }
    if (!form.emergencyContactPhoneNumber.trim()) {
      return "Emergency contact phone is required.";
    }
    return null;
  };

  const handleSaveAdd = async () => {
    setPanelError(null);
    const validationError = validateForm();
    if (validationError) {
      setPanelError(validationError);
      return;
    }

    const payload = toCreatePayload(form);

    try {
      await createMember.mutateAsync(payload);
      setShowAddPanel(false);
    } catch (err: unknown) {
      setPanelError(getErrorMessage(err, "Failed to create member."));
    }
  };

  const handleSaveEdit = async () => {
    if (!editingMember) return;
    setPanelError(null);
    const validationError = validateForm();
    if (validationError) {
      setPanelError(validationError);
      return;
    }

    const payload = toCreatePayload(form);

    try {
      await updateMember.mutateAsync({ id: editingMember.id, data: payload });
      setEditingMember(null);
    } catch (err: unknown) {
      setPanelError(getErrorMessage(err, "Failed to update member."));
    }
  };

  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingMember, setDeletingMember] = useState<Member | null>(null);

  const handleDelete = (member: Member) => {
    setDeletingMember(member);
    setShowDeleteConfirm(true);
  };

  const confirmDelete = async () => {
    if (!deletingMember) return;
    try {
      await deleteMember.mutateAsync(deletingMember.id);
      setShowDeleteConfirm(false);
      setDeletingMember(null);
    } catch (err: unknown) {
      alert(getErrorMessage(err, "Failed to delete member."));
    }
  };

  const closePanel = () => {
    setPanelError(null);
    setShowAddPanel(false);
    setEditingMember(null);
  };

  const tabs = ["All Members", "Active", "Inactive", "Archived"];
  const panelOpen = showAddPanel || !!editingMember;

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <div className="members-container">
      {/* ─── Stat Cards ─────────────────────────────────────────────────── */}
      <div className="members-stats-row">
        <div className="members-stat-card">
          <span className="members-stat-label">Total Membership</span>
          <div className="members-stat-value">{totalMembership}</div>
        </div>
        <div className="members-stat-card">
          <span className="members-stat-label">Archived</span>
          <div className="members-stat-value">{archivedMembers}</div>
        </div>
        <div className="members-stat-card">
          <span className="members-stat-label">Active Families</span>
          <div className="members-stat-value">{activeFamilies}</div>
        </div>
        <div className="members-stat-card">
          <span className="members-stat-label">Retention Rate</span>
          <div className="members-stat-value">{retentionRate}%</div>
        </div>
      </div>

      {/* ─── List Card ──────────────────────────────────────────────────── */}
      <div className="members-list-card">
        <div className="members-card-header">
          <div className="members-tabs">
            {tabs.map((tab) => (
              <button
                key={tab}
                type="button"
                className={`members-tab ${activeTab === tab ? "members-tab-active" : ""}`}
                onClick={() => setActiveTab(tab)}
              >
                {tab}
              </button>
            ))}
          </div>
          <input
            type="text"
            className="members-search"
            placeholder="Search Members..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>

        <div className="members-card-body">
          {isLoading ? (
            <div className="members-empty-state">Loading...</div>
          ) : members.length === 0 ? (
            <div className="members-empty-state">No members found.</div>
          ) : (
            <div className="members-card-grid">
              {members.map((member) => (
                <div key={member.id} className="member-card">
                  <div className="member-card-top">
                    <div className="member-card-avatar">
                      {getInitials(member.firstName, member.lastName)}
                    </div>
                    <div className="member-card-identity">
                      <span className="member-card-name">
                        {member.firstName}
                        {"\n"}
                        {member.lastName}
                      </span>
                    </div>
                    <span
                      className={`member-status-badge ${statusClass(member.status)}`}
                    >
                      {member.status}
                    </span>
                  </div>

                  <div className="member-card-meta">
                    <div className="member-meta-row">
                      <svg
                        className="member-meta-icon"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      >
                        <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07A19.5 19.5 0 0 1 4.9 12a19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 3.81 1h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 8.91a16 16 0 0 0 6 6l.96-.96a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z" />
                      </svg>
                      <span>{member.phoneNumber}</span>
                    </div>
                    <div className="member-meta-row">
                      <CalendarIcon size={14} className="member-meta-icon" />
                      <span>Joined {formatDateDisplay(member.joinedDate)}</span>
                    </div>
                    <div className="member-meta-row">
                      <MembersIcon size={14} className="member-meta-icon" />
                      <span>{member.maritalStatus}</span>
                    </div>
                  </div>

                  <div className="member-card-id-row">
                    <span className="member-card-id-label">MEMBER ID</span>
                    <span className="member-card-id-value">
                      {member.id.split("-")[0]}
                    </span>
                  </div>

                  <div className="member-card-actions">
                    <button
                      type="button"
                      className="member-action-btn"
                      onClick={() => openEditPanel(member)}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      className="member-action-btn danger"
                      onClick={() => handleDelete(member)}
                    >
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* ════════════════════════════════════════════════════════════════════
          SLIDE-OUT PANEL  (Add | Edit)
          ════════════════════════════════════════════════════════════════ */}
      {panelOpen && (
        <div className="members-panel-overlay" onClick={closePanel}>
          <div
            className="members-side-panel"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="members-panel-header">
              <div>
                <h2 className="members-panel-title">
                  {showAddPanel ? "Add Member" : "Edit Member"}
                </h2>
                <p className="members-panel-subtitle">
                  {showAddPanel
                    ? "Fill in details to add a new member"
                    : "Update member profile details"}
                </p>
                {panelError && (
                  <div
                    style={{
                      color: "#ef4444",
                      fontSize: "0.875rem",
                      marginTop: "0.5rem",
                      fontWeight: 500,
                    }}
                  >
                    {panelError}
                  </div>
                )}
              </div>
              <button
                type="button"
                className="members-panel-close"
                onClick={closePanel}
              >
                <CloseIcon />
              </button>
            </div>

            <div className="members-panel-body">
              <MemberFormFields form={form} setForm={setForm} />
            </div>

            <div className="members-panel-footer">
              <button
                type="button"
                className="members-btn members-btn-secondary"
                onClick={closePanel}
              >
                Cancel
              </button>
              <button
                type="button"
                className="members-btn members-btn-primary"
                onClick={showAddPanel ? handleSaveAdd : handleSaveEdit}
                disabled={createMember.isPending || updateMember.isPending}
              >
                {createMember.isPending || updateMember.isPending
                  ? "Saving..."
                  : "Save Changes"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ════════════════════════════════════════════════════════════════════
          EXPORT PANEL
          ════════════════════════════════════════════════════════════════ */}
      {showExportModal && (
        <ExportPanel
          title="Export Members Roster"
          exportConfig={{
            title: "Members Roster",
            filename: "echo-members-roster",
            columns: [
              { key: "id", label: "Member ID" },
              { key: "firstName", label: "First Name" },
              { key: "lastName", label: "Last Name" },
              { key: "otherNames", label: "Other Names" },
              { key: "emailAddress", label: "Email" },
              { key: "phoneNumber", label: "Phone" },
              { key: "status", label: "Status" },
              { key: "gender", label: "Gender" },
              { key: "dateOfBirth", label: "Date of Birth" },
              { key: "joinedDate", label: "Joined Date" },
              { key: "maritalStatus", label: "Marital Status" },
              { key: "residentialAddress", label: "Residential Address" },
              { key: "city", label: "City" },
              { key: "hometown", label: "Hometown" },
              { key: "region", label: "Region" },
              { key: "gpsAddress", label: "Ghana Post" },
              { key: "nextOfKin", label: "Next of Kin" },
              { key: "emergencyContactName", label: "Emergency Contact" },
              { key: "emergencyContactPhoneNumber", label: "Emergency Phone" },
            ],
            rows: members as unknown as Record<string, unknown>[],
          }}
          onClose={() => setShowExportModal(false)}
        />
      )}

      {/* ════════════════════════════════════════════════════════════════════
          DELETE CONFIRMATION MODAL
          ════════════════════════════════════════════════════════════════ */}
      <DeleteConfirmModal
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={confirmDelete}
        itemName={
          deletingMember
            ? `${deletingMember.firstName} ${deletingMember.lastName}`.trim()
            : ""
        }
        title="Delete Member"
        confirmText="Delete Member"
      />
    </div>
  );
};

// ─── Shared Form ──────────────────────────────────────────────────────────────

const MemberFormFields: React.FC<{
  form: MemberFormState;
  setForm: React.Dispatch<React.SetStateAction<MemberFormState>>;
}> = ({ form, setForm }) => {
  const set = <K extends keyof MemberFormState>(
    key: K,
    value: MemberFormState[K],
  ) => setForm((prev) => ({ ...prev, [key]: value }));

  return (
    <>
      <div className="mf-row">
        <div className="mf-group">
          <label className="mf-label">First Name</label>
          <input
            className="mf-input"
            placeholder="e.g., John"
            value={form.firstName}
            onChange={(e) => set("firstName", e.target.value)}
          />
        </div>
        <div className="mf-group">
          <label className="mf-label">Last Name</label>
          <input
            className="mf-input"
            placeholder="e.g., Doe"
            value={form.lastName}
            onChange={(e) => set("lastName", e.target.value)}
          />
        </div>
      </div>

      <div className="mf-group">
        <label className="mf-label">Other Names</label>
        <input
          className="mf-input"
          placeholder="Optional"
          value={form.otherNames}
          onChange={(e) => set("otherNames", e.target.value)}
        />
      </div>

      <div className="mf-group">
        <label className="mf-label">Email Address</label>
        <input
          className="mf-input"
          type="email"
          placeholder="member@example.com"
          value={form.emailAddress}
          onChange={(e) => set("emailAddress", e.target.value)}
        />
      </div>

      <div className="mf-group">
        <label className="mf-label">Phone Number</label>
        <input
          className="mf-input"
          placeholder="+ 233 00 000 0000"
          value={form.phoneNumber}
          onChange={(e) => set("phoneNumber", e.target.value)}
        />
      </div>

      <div className="mf-row">
        <div className="mf-group">
          <label className="mf-label">Marital Status</label>
          <select
            className="mf-select"
            value={form.maritalStatus}
            onChange={(e) =>
              set("maritalStatus", e.target.value as MemberMaritalStatus)
            }
          >
            <option value="Single">Single</option>
            <option value="Married">Married</option>
            <option value="Widowed">Widowed</option>
          </select>
        </div>
        <div className="mf-group">
          <label className="mf-label">Status</label>
          <select
            className="mf-select"
            value={form.status}
            onChange={(e) => set("status", e.target.value as MemberStatus)}
          >
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="Archived">Archived</option>
            <option value="Transferred">Transferred</option>
          </select>
        </div>
      </div>

      <div className="mf-row">
        <div className="mf-group">
          <label className="mf-label">Gender</label>
          <select
            className="mf-select"
            value={form.gender}
            onChange={(e) => set("gender", e.target.value as MemberGender)}
          >
            <option value="Male">Male</option>
            <option value="Female">Female</option>
            <option value="Other">Other</option>
          </select>
        </div>
        <div className="mf-group">
          <label className="mf-label">Date of Birth</label>
          <input
            className="mf-input"
            type="date"
            value={form.dateOfBirth}
            onChange={(e) => set("dateOfBirth", e.target.value)}
          />
        </div>
      </div>

      <div className="mf-group">
        <label className="mf-label">Joined Date</label>
        <input
          className="mf-input"
          type="date"
          value={form.joinedDate}
          onChange={(e) => set("joinedDate", e.target.value)}
        />
      </div>

      <div className="mf-divider" />

      <div className="mf-group">
        <label className="mf-label">Residential Address</label>
        <input
          className="mf-input"
          placeholder="Hebron, Soldier Lane"
          value={form.residentialAddress}
          onChange={(e) => set("residentialAddress", e.target.value)}
        />
      </div>

      <div className="mf-group">
        <label className="mf-label">City</label>
        <input
          className="mf-input"
          placeholder="Accra"
          value={form.city}
          onChange={(e) => set("city", e.target.value)}
        />
      </div>

      <div className="mf-group">
        <label className="mf-label">Hometown</label>
        <input
          className="mf-input"
          placeholder="Aburi"
          value={form.hometown}
          onChange={(e) => set("hometown", e.target.value)}
        />
      </div>

      <div className="mf-group">
        <label className="mf-label">Region</label>
        <select
          className="mf-select"
          value={form.region}
          onChange={(e) => set("region", e.target.value as MemberRegion | "")}
        >
          <option value="">Select region</option>
          {GHANA_REGIONS.map((r) => (
            <option key={r.value} value={r.value}>
              {r.label}
            </option>
          ))}
        </select>
      </div>

      <div className="mf-group">
        <label className="mf-label">Ghana Post Address</label>
        <input
          className="mf-input"
          placeholder="GPS-282-282"
          value={form.gpsAddress}
          onChange={(e) => set("gpsAddress", e.target.value)}
        />
      </div>

      <div className="mf-divider" />

      <div className="mf-group">
        <label className="mf-label">Next of Kin</label>
        <input
          className="mf-input"
          placeholder="Enter name here..."
          value={form.nextOfKin}
          onChange={(e) => set("nextOfKin", e.target.value)}
        />
      </div>

      <div className="mf-group">
        <label className="mf-label">Emergency Contact Name</label>
        <input
          className="mf-input"
          placeholder="Enter name here..."
          value={form.emergencyContactName}
          onChange={(e) => set("emergencyContactName", e.target.value)}
        />
      </div>

      <div className="mf-group">
        <label className="mf-label">Emergency Contact Phone</label>
        <input
          className="mf-input"
          placeholder="+ 233 00 000 0000"
          value={form.emergencyContactPhoneNumber}
          onChange={(e) => set("emergencyContactPhoneNumber", e.target.value)}
        />
      </div>
    </>
  );
};

export default Members;
