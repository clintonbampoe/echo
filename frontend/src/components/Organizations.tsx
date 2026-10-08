import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useLayout } from "../hooks/useLayout";
import { useDebouncedValue } from "../hooks/useDebouncedValue";
import {
  useOrganization,
  useOrganizationsInfinite,
  useOrganizationSearch,
  useCreateOrganization,
  useUpdateOrganization,
  useDeleteOrganization,
} from "../hooks/useOrganizations";
import {
  useOrganizationMembersInfinite,
  useCreateOrganizationMember,
  useUpdateOrganizationMember,
  useDeleteOrganizationMember,
} from "../hooks/useOrganizationMembers";
import { useMembers } from "../hooks/useMembers";
import type { Organization } from "../types/organization";
import type {
  OrganizationMember,
  OrganizationMemberRole,
} from "../types/organizationMember";
import { getErrorMessage } from "../utils/errors";
import {
  CloseIcon,
  EditIcon,
  OrganizationsIcon,
  PlusIcon,
  SearchIcon,
  TrashIcon,
} from "./Icons";
import DeleteConfirmModal from "./common/DeleteConfirmModal";
import "../styles/Organizations.css";

// ─── Constants & Helpers ─────────────────────────────────────────────────────

const ROLES: OrganizationMemberRole[] = ["Leader", "Secretary", "Member"];

const NAME_MAX = 100;
const DESCRIPTION_MAX = 2000;

const todayIso = (): string => new Date().toLocaleDateString("en-CA");

const formatDate = (value?: string | null): string => {
  if (!value) return "—";
  // Date-only values (YYYY-MM-DD) are parsed as local dates to avoid timezone shifts.
  const date = /^\d{4}-\d{2}-\d{2}$/.test(value)
    ? new Date(`${value}T00:00:00`)
    : new Date(value);
  if (Number.isNaN(date.getTime())) return "—";
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
};

const getInitials = (name: string): string => {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "?";
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
  return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
};

const AVATAR_TONES = 6;
const getTone = (seed: string): number => {
  let hash = 0;
  for (let i = 0; i < seed.length; i++) {
    hash = (hash * 31 + seed.charCodeAt(i)) | 0;
  }
  return Math.abs(hash) % AVATAR_TONES;
};

const OrgAvatar: React.FC<{ id: string; name: string; large?: boolean }> = ({
  id,
  name,
  large,
}) => (
  <div
    className={`org-avatar org-avatar-tone-${getTone(id)} ${large ? "org-avatar-lg" : ""}`}
  >
    {getInitials(name)}
  </div>
);

interface OrgListItem {
  id: string;
  name: string;
  description?: string | null;
}

// ─── Organization Create / Edit Panel ────────────────────────────────────────

interface OrganizationFormPanelProps {
  organization: Organization | null;
  onClose: () => void;
  onSaved: (organization: Organization) => void;
}

const OrganizationFormPanel: React.FC<OrganizationFormPanelProps> = ({
  organization,
  onClose,
  onSaved,
}) => {
  const createOrganization = useCreateOrganization();
  const updateOrganization = useUpdateOrganization();

  const [name, setName] = useState(organization?.name ?? "");
  const [description, setDescription] = useState(
    organization?.description ?? "",
  );
  const [error, setError] = useState<string | null>(null);

  const isSaving = createOrganization.isPending || updateOrganization.isPending;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const trimmedName = name.trim();
    if (!trimmedName) {
      setError("Organization name is required.");
      return;
    }

    try {
      const saved = organization
        ? await updateOrganization.mutateAsync({
            id: organization.id,
            // The API ignores null fields on update, so send "" to clear the description.
            data: { name: trimmedName, description: description.trim() },
          })
        : await createOrganization.mutateAsync({
            name: trimmedName,
            description: description.trim() || null,
          });
      onSaved(saved);
    } catch (err) {
      setError(getErrorMessage(err, "Failed to save organization."));
    }
  };

  return (
    <div className="org-panel-overlay" onClick={onClose}>
      <form
        className="org-side-panel"
        onClick={(e) => e.stopPropagation()}
        onSubmit={handleSubmit}
      >
        <div className="org-panel-header">
          <h2 className="org-panel-title">
            {organization ? "Edit Organization" : "New Organization"}
          </h2>
          <button
            type="button"
            className="org-panel-close"
            onClick={onClose}
            aria-label="Close panel"
          >
            <CloseIcon />
          </button>
        </div>

        <div className="org-panel-body">
          {error && <div className="org-form-error">{error}</div>}

          <div className="org-form-group">
            <label className="org-form-label" htmlFor="org-name">
              Name
            </label>
            <input
              id="org-name"
              type="text"
              className="org-form-input"
              placeholder="e.g. Youth Ministry, Choir, Ushering Team"
              value={name}
              maxLength={NAME_MAX}
              onChange={(e) => setName(e.target.value)}
              required
              autoFocus
            />
          </div>

          <div className="org-form-group">
            <label className="org-form-label" htmlFor="org-description">
              Description <span>(Optional)</span>
            </label>
            <textarea
              id="org-description"
              className="org-form-textarea"
              placeholder="What does this group do? When does it meet?"
              value={description}
              maxLength={DESCRIPTION_MAX}
              onChange={(e) => setDescription(e.target.value)}
              rows={6}
            />
            <span className="org-form-hint">
              {description.length} / {DESCRIPTION_MAX}
            </span>
          </div>
        </div>

        <div className="org-panel-footer">
          <button
            type="button"
            className="org-btn org-btn-secondary"
            onClick={onClose}
          >
            Cancel
          </button>
          <button
            type="submit"
            className="org-btn org-btn-primary"
            disabled={isSaving}
          >
            {isSaving
              ? "Saving..."
              : organization
                ? "Save Changes"
                : "Create Organization"}
          </button>
        </div>
      </form>
    </div>
  );
};

// ─── Add Member Panel ────────────────────────────────────────────────────────

interface AddMemberPanelProps {
  organization: Organization;
  existingMemberIds: Set<string>;
  onClose: () => void;
}

const AddMemberPanel: React.FC<AddMemberPanelProps> = ({
  organization,
  existingMemberIds,
  onClose,
}) => {
  const createMembership = useCreateOrganizationMember();

  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search);
  const memberFilters = useMemo(
    () => ({ name: debouncedSearch.trim() || undefined }),
    [debouncedSearch],
  );
  const { data: membersData, isLoading: isLoadingMembers } =
    useMembers(memberFilters);
  const candidates = membersData?.data ?? [];

  const [selected, setSelected] = useState<{ id: string; name: string } | null>(
    null,
  );
  const [role, setRole] = useState<OrganizationMemberRole>("Member");
  const [joinedAt, setJoinedAt] = useState(todayIso());
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!selected) {
      setError("Select a member to add.");
      return;
    }

    try {
      await createMembership.mutateAsync({
        memberId: selected.id,
        organizationId: organization.id,
        role,
        joinedAt,
      });
      onClose();
    } catch (err) {
      setError(getErrorMessage(err, "Failed to add member."));
    }
  };

  return (
    <div className="org-panel-overlay" onClick={onClose}>
      <form
        className="org-side-panel"
        onClick={(e) => e.stopPropagation()}
        onSubmit={handleSubmit}
      >
        <div className="org-panel-header">
          <div>
            <h2 className="org-panel-title">Add Member</h2>
            <p className="org-panel-subtitle">to {organization.name}</p>
          </div>
          <button
            type="button"
            className="org-panel-close"
            onClick={onClose}
            aria-label="Close panel"
          >
            <CloseIcon />
          </button>
        </div>

        <div className="org-panel-body">
          {error && <div className="org-form-error">{error}</div>}

          <div className="org-form-group">
            <label className="org-form-label" htmlFor="org-member-search">
              Member
            </label>
            {selected ? (
              <div className="org-selected-member">
                <div className="org-member-avatar">
                  {getInitials(selected.name)}
                </div>
                <span className="org-selected-member-name">{selected.name}</span>
                <button
                  type="button"
                  className="org-link-btn"
                  onClick={() => setSelected(null)}
                >
                  Change
                </button>
              </div>
            ) : (
              <>
                <div className="org-input-with-icon">
                  <SearchIcon size={16} />
                  <input
                    id="org-member-search"
                    type="text"
                    className="org-form-input"
                    placeholder="Search members by name..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    autoFocus
                  />
                </div>
                <div className="org-candidate-list" role="listbox">
                  {isLoadingMembers ? (
                    <div className="org-candidate-empty">Searching...</div>
                  ) : candidates.length === 0 ? (
                    <div className="org-candidate-empty">
                      No members match "{debouncedSearch}".
                    </div>
                  ) : (
                    candidates.map((m) => {
                      const alreadyAdded = existingMemberIds.has(m.id);
                      return (
                        <button
                          key={m.id}
                          type="button"
                          role="option"
                          aria-selected={false}
                          className="org-candidate"
                          disabled={alreadyAdded}
                          onClick={() =>
                            setSelected({ id: m.id, name: m.name })
                          }
                        >
                          <div className="org-member-avatar">
                            {getInitials(m.name)}
                          </div>
                          <div className="org-candidate-info">
                            <span className="org-candidate-name">{m.name}</span>
                            <span className="org-candidate-meta">
                              {m.phoneNumber || m.emailAddress || m.status}
                            </span>
                          </div>
                          {alreadyAdded && (
                            <span className="org-candidate-tag">
                              Already added
                            </span>
                          )}
                        </button>
                      );
                    })
                  )}
                </div>
              </>
            )}
          </div>

          <div className="org-form-row">
            <div className="org-form-group">
              <label className="org-form-label" htmlFor="org-member-role">
                Role
              </label>
              <select
                id="org-member-role"
                className="org-form-select"
                value={role}
                onChange={(e) =>
                  setRole(e.target.value as OrganizationMemberRole)
                }
              >
                {ROLES.map((r) => (
                  <option key={r} value={r}>
                    {r}
                  </option>
                ))}
              </select>
            </div>

            <div className="org-form-group">
              <label className="org-form-label" htmlFor="org-member-joined">
                Joined On
              </label>
              <input
                id="org-member-joined"
                type="date"
                className="org-form-input"
                value={joinedAt}
                max={todayIso()}
                onChange={(e) => setJoinedAt(e.target.value)}
                required
              />
            </div>
          </div>
        </div>

        <div className="org-panel-footer">
          <button
            type="button"
            className="org-btn org-btn-secondary"
            onClick={onClose}
          >
            Cancel
          </button>
          <button
            type="submit"
            className="org-btn org-btn-primary"
            disabled={createMembership.isPending || !selected}
          >
            {createMembership.isPending ? "Adding..." : "Add Member"}
          </button>
        </div>
      </form>
    </div>
  );
};

// ─── Edit Membership Panel ───────────────────────────────────────────────────

interface EditMembershipPanelProps {
  membership: OrganizationMember;
  onClose: () => void;
}

const EditMembershipPanel: React.FC<EditMembershipPanelProps> = ({
  membership,
  onClose,
}) => {
  const updateMembership = useUpdateOrganizationMember();

  const [role, setRole] = useState<OrganizationMemberRole>(membership.role);
  const [joinedAt, setJoinedAt] = useState(membership.joinedAt);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await updateMembership.mutateAsync({
        id: membership.id,
        data: { role, joinedAt },
      });
      onClose();
    } catch (err) {
      setError(getErrorMessage(err, "Failed to update membership."));
    }
  };

  return (
    <div className="org-panel-overlay" onClick={onClose}>
      <form
        className="org-side-panel"
        onClick={(e) => e.stopPropagation()}
        onSubmit={handleSubmit}
      >
        <div className="org-panel-header">
          <div>
            <h2 className="org-panel-title">Edit Membership</h2>
            <p className="org-panel-subtitle">
              {membership.memberName} · {membership.organizationName}
            </p>
          </div>
          <button
            type="button"
            className="org-panel-close"
            onClick={onClose}
            aria-label="Close panel"
          >
            <CloseIcon />
          </button>
        </div>

        <div className="org-panel-body">
          {error && <div className="org-form-error">{error}</div>}

          <div className="org-form-group">
            <span className="org-form-label">Role</span>
            <div className="org-role-picker">
              {ROLES.map((r) => (
                <button
                  key={r}
                  type="button"
                  className={`org-role-option ${role === r ? "active" : ""}`}
                  onClick={() => setRole(r)}
                  aria-pressed={role === r}
                >
                  <span className={`org-role-badge ${r.toLowerCase()}`}>
                    {r}
                  </span>
                </button>
              ))}
            </div>
          </div>

          <div className="org-form-group">
            <label className="org-form-label" htmlFor="org-edit-joined">
              Joined On
            </label>
            <input
              id="org-edit-joined"
              type="date"
              className="org-form-input"
              value={joinedAt}
              max={todayIso()}
              onChange={(e) => setJoinedAt(e.target.value)}
              required
            />
          </div>
        </div>

        <div className="org-panel-footer">
          <button
            type="button"
            className="org-btn org-btn-secondary"
            onClick={onClose}
          >
            Cancel
          </button>
          <button
            type="submit"
            className="org-btn org-btn-primary"
            disabled={updateMembership.isPending}
          >
            {updateMembership.isPending ? "Saving..." : "Save Changes"}
          </button>
        </div>
      </form>
    </div>
  );
};

// ─── Page ────────────────────────────────────────────────────────────────────

const Organizations: React.FC = () => {
  const { setTitle, setCtas, searchQuery, setSearchQuery } = useLayout();

  // ── Organizations List ─────────────────────────────────────────────────────
  const orgsQuery = useOrganizationsInfinite();
  const organizations = useMemo(
    () => orgsQuery.data?.pages.flatMap((p) => p.data) ?? [],
    [orgsQuery.data],
  );

  // Falls back to the first organization until the user picks one.
  const [chosenOrgId, setChosenOrgId] = useState<string | undefined>();
  const selectedOrgId = chosenOrgId ?? organizations[0]?.id;

  // ── Panels & Modals ────────────────────────────────────────────────────────
  const [orgPanel, setOrgPanel] = useState<
    { mode: "create" } | { mode: "edit"; organization: Organization } | null
  >(null);
  const [deletingOrg, setDeletingOrg] = useState<Organization | null>(null);
  const [showAddMember, setShowAddMember] = useState(false);
  const [editingMembership, setEditingMembership] =
    useState<OrganizationMember | null>(null);
  const [removingMembership, setRemovingMembership] =
    useState<OrganizationMember | null>(null);

  // Per-organization UI state is keyed by org id so it resets when the selection changes.
  const [actionErrorState, setActionErrorState] = useState<{
    orgId?: string;
    message: string;
  } | null>(null);
  const actionError =
    actionErrorState && actionErrorState.orgId === selectedOrgId
      ? actionErrorState.message
      : null;
  const setActionError = (message: string | null) =>
    setActionErrorState(message ? { orgId: selectedOrgId, message } : null);

  const [roleFilterState, setRoleFilterState] = useState<{
    orgId?: string;
    role: OrganizationMemberRole | "";
  }>({ role: "" });
  const roleFilter =
    roleFilterState.orgId === selectedOrgId ? roleFilterState.role : "";
  const setRoleFilter = (role: OrganizationMemberRole | "") =>
    setRoleFilterState({ orgId: selectedOrgId, role });

  // ── Layout Header ──────────────────────────────────────────────────────────
  const openCreatePanel = useCallback(
    () => setOrgPanel({ mode: "create" }),
    [setOrgPanel],
  );

  useEffect(() => {
    setTitle("Organizations");
    setCtas([
      { type: "search", placeholder: "Search organizations..." },
      {
        type: "button",
        label: "New Organization",
        icon: "plus",
        variant: "primary",
        onClick: openCreatePanel,
      },
    ]);
    setSearchQuery("");
  }, [setTitle, setCtas, setSearchQuery, openCreatePanel]);

  const debouncedSearch = useDebouncedValue(searchQuery);
  const isSearching = debouncedSearch.trim().length > 0;
  const searchResultsQuery = useOrganizationSearch(debouncedSearch);

  const listItems: OrgListItem[] = useMemo(() => {
    if (!isSearching) return organizations;
    // Search results only carry id + name; enrich with descriptions we already have.
    const known = new Map(organizations.map((o) => [o.id, o]));
    return (searchResultsQuery.data ?? []).map((r) => ({
      id: r.id,
      name: r.name,
      description: known.get(r.id)?.description,
    }));
  }, [isSearching, organizations, searchResultsQuery.data]);

  const isListLoading = isSearching
    ? searchResultsQuery.isLoading
    : orgsQuery.isLoading;

  // ── Selected Organization ──────────────────────────────────────────────────
  const orgDetailQuery = useOrganization(selectedOrgId);
  const selectedOrg: Organization | undefined =
    orgDetailQuery.data ?? organizations.find((o) => o.id === selectedOrgId);

  const memberFilters = useMemo(
    () => ({ role: roleFilter || undefined }),
    [roleFilter],
  );
  const membersQuery = useOrganizationMembersInfinite(
    selectedOrg?.id,
    memberFilters,
  );
  const memberships = useMemo(
    () => membersQuery.data?.pages.flatMap((p) => p.data) ?? [],
    [membersQuery.data],
  );
  const existingMemberIds = useMemo(
    () => new Set(memberships.map((m) => m.memberId)),
    [memberships],
  );

  const roleCounts = useMemo(() => {
    const counts: Record<OrganizationMemberRole, number> = {
      Leader: 0,
      Secretary: 0,
      Member: 0,
    };
    memberships.forEach((m) => {
      counts[m.role] = (counts[m.role] ?? 0) + 1;
    });
    return counts;
  }, [memberships]);

  const leaders = useMemo(
    () => memberships.filter((m) => m.role === "Leader"),
    [memberships],
  );

  const moreSuffix = membersQuery.hasNextPage ? "+" : "";

  // ── Handlers ───────────────────────────────────────────────────────────────
  const deleteOrganization = useDeleteOrganization();
  const deleteMembership = useDeleteOrganizationMember();

  const confirmDeleteOrg = async () => {
    if (!deletingOrg) return;
    setActionError(null);
    try {
      await deleteOrganization.mutateAsync(deletingOrg.id);
      if (deletingOrg.id === selectedOrgId) setChosenOrgId(undefined);
      setDeletingOrg(null);
    } catch (err) {
      setDeletingOrg(null);
      setActionError(getErrorMessage(err, "Failed to delete organization."));
    }
  };

  const confirmRemoveMembership = async () => {
    if (!removingMembership) return;
    setActionError(null);
    try {
      await deleteMembership.mutateAsync(removingMembership.id);
      setRemovingMembership(null);
    } catch (err) {
      setRemovingMembership(null);
      setActionError(getErrorMessage(err, "Failed to remove member."));
    }
  };

  // ── Render ─────────────────────────────────────────────────────────────────
  const hasNoOrganizations =
    !orgsQuery.isLoading && !isSearching && organizations.length === 0;

  return (
    <div className="org-container">
      {hasNoOrganizations ? (
        <div className="org-empty-state">
          <div className="org-empty-icon">
            <OrganizationsIcon size={32} />
          </div>
          <h3 className="org-empty-title">No organizations yet</h3>
          <p className="org-empty-text">
            Organizations group members into ministries, departments, and
            fellowships — like the choir, youth ministry, or ushering team.
          </p>
          <button
            type="button"
            className="org-btn org-btn-primary"
            onClick={openCreatePanel}
          >
            <PlusIcon size={16} /> Create your first organization
          </button>
        </div>
      ) : (
        <div className="org-layout">
          {/* ─── Organization List ─────────────────────────────────────────── */}
          <aside className="org-list-card">
            <div className="org-list-header">
              <h3 className="org-list-title">
                {isSearching ? "Search Results" : "All Organizations"}
              </h3>
              <span className="org-count-pill">
                {listItems.length}
                {!isSearching && orgsQuery.hasNextPage ? "+" : ""}
              </span>
            </div>

            <div className="org-list">
              {isListLoading ? (
                <div className="org-list-empty">Loading organizations...</div>
              ) : listItems.length === 0 ? (
                <div className="org-list-empty">
                  No organizations match "{debouncedSearch}".
                </div>
              ) : (
                listItems.map((org) => (
                  <button
                    key={org.id}
                    type="button"
                    className={`org-list-item ${org.id === selectedOrgId ? "active" : ""}`}
                    onClick={() => setChosenOrgId(org.id)}
                  >
                    <OrgAvatar id={org.id} name={org.name} />
                    <div className="org-list-item-text">
                      <span className="org-list-item-name">{org.name}</span>
                      {org.description && (
                        <span className="org-list-item-desc">
                          {org.description}
                        </span>
                      )}
                    </div>
                  </button>
                ))
              )}

              {!isSearching && orgsQuery.hasNextPage && (
                <button
                  type="button"
                  className="org-load-more"
                  onClick={() => orgsQuery.fetchNextPage()}
                  disabled={orgsQuery.isFetchingNextPage}
                >
                  {orgsQuery.isFetchingNextPage ? "Loading..." : "Load more"}
                </button>
              )}
            </div>
          </aside>

          {/* ─── Organization Detail ───────────────────────────────────────── */}
          <section className="org-detail">
            {!selectedOrg ? (
              <div className="org-detail-placeholder">
                {orgDetailQuery.isLoading || orgsQuery.isLoading
                  ? "Loading organization..."
                  : "Select an organization to see its members."}
              </div>
            ) : (
              <>
                {actionError && (
                  <div className="org-form-error org-action-error">
                    <span>{actionError}</span>
                    <button
                      type="button"
                      className="org-panel-close"
                      onClick={() => setActionError(null)}
                      aria-label="Dismiss error"
                    >
                      <CloseIcon size={16} />
                    </button>
                  </div>
                )}

                {/* Header Card */}
                <div className="org-hero">
                  <div className="org-hero-main">
                    <OrgAvatar id={selectedOrg.id} name={selectedOrg.name} large />
                    <div className="org-hero-text">
                      <h2 className="org-hero-name">{selectedOrg.name}</h2>
                      <span className="org-hero-meta">
                        Created {formatDate(selectedOrg.createdAt)}
                      </span>
                    </div>
                    <div className="org-hero-actions">
                      <button
                        type="button"
                        className="org-btn org-btn-secondary"
                        onClick={() =>
                          setOrgPanel({ mode: "edit", organization: selectedOrg })
                        }
                      >
                        <EditIcon size={15} /> Edit
                      </button>
                      <button
                        type="button"
                        className="org-btn org-btn-danger"
                        onClick={() => setDeletingOrg(selectedOrg)}
                      >
                        <TrashIcon size={15} /> Delete
                      </button>
                    </div>
                  </div>

                  <p
                    className={`org-hero-desc ${selectedOrg.description ? "" : "muted"}`}
                  >
                    {selectedOrg.description || "No description provided."}
                  </p>

                  {!roleFilter && (
                    <div className="org-stats">
                      <div className="org-stat">
                        <span className="org-stat-value">
                          {memberships.length}
                          {moreSuffix}
                        </span>
                        <span className="org-stat-label">Members</span>
                      </div>
                      {ROLES.map((r) => (
                        <div className="org-stat" key={r}>
                          <span className="org-stat-value">
                            {roleCounts[r]}
                            {moreSuffix}
                          </span>
                          <span className="org-stat-label">
                            {r === "Secretary" ? "Secretaries" : `${r}s`}
                          </span>
                        </div>
                      ))}
                    </div>
                  )}

                  {!roleFilter && leaders.length > 0 && (
                    <div className="org-leaders">
                      <span className="org-leaders-label">Led by</span>
                      {leaders.slice(0, 4).map((l) => (
                        <span className="org-leader-chip" key={l.id}>
                          <span className="org-member-avatar sm">
                            {getInitials(l.memberName)}
                          </span>
                          {l.memberName}
                        </span>
                      ))}
                      {leaders.length > 4 && (
                        <span className="org-leaders-more">
                          +{leaders.length - 4} more
                        </span>
                      )}
                    </div>
                  )}
                </div>

                {/* Members Table */}
                <div className="org-table-card">
                  <div className="org-table-header">
                    <h3 className="org-table-title">Members</h3>
                    <div className="org-table-controls">
                      <div className="org-role-tabs" role="tablist">
                        {(["", ...ROLES] as const).map((r) => (
                          <button
                            key={r || "all"}
                            type="button"
                            role="tab"
                            aria-selected={roleFilter === r}
                            className={`org-role-tab ${roleFilter === r ? "active" : ""}`}
                            onClick={() => setRoleFilter(r)}
                          >
                            {r || "All"}
                          </button>
                        ))}
                      </div>
                      <button
                        type="button"
                        className="org-btn org-btn-primary"
                        onClick={() => setShowAddMember(true)}
                      >
                        <PlusIcon size={16} /> Add Member
                      </button>
                    </div>
                  </div>

                  <div className="org-table-container">
                    {membersQuery.isLoading ? (
                      <div className="org-table-empty">Loading members...</div>
                    ) : memberships.length === 0 ? (
                      <div className="org-table-empty">
                        {roleFilter
                          ? `No ${roleFilter.toLowerCase()}s in this organization.`
                          : "No members yet. Add someone to get started."}
                      </div>
                    ) : (
                      <table className="org-table">
                        <thead>
                          <tr>
                            <th>Member</th>
                            <th>Role</th>
                            <th>Joined</th>
                            <th>Added</th>
                            <th aria-label="Actions" />
                          </tr>
                        </thead>
                        <tbody>
                          {memberships.map((m) => (
                            <tr key={m.id}>
                              <td>
                                <div className="org-member-cell">
                                  <div className="org-member-avatar">
                                    {getInitials(m.memberName)}
                                  </div>
                                  <span className="org-member-name">
                                    {m.memberName}
                                  </span>
                                </div>
                              </td>
                              <td>
                                <span
                                  className={`org-role-badge ${m.role.toLowerCase()}`}
                                >
                                  {m.role}
                                </span>
                              </td>
                              <td>{formatDate(m.joinedAt)}</td>
                              <td className="org-muted-cell">
                                {formatDate(m.createdAt)}
                              </td>
                              <td>
                                <div className="org-table-actions">
                                  <button
                                    type="button"
                                    className="org-icon-btn"
                                    onClick={() => setEditingMembership(m)}
                                    title="Edit membership"
                                    aria-label={`Edit ${m.memberName}'s membership`}
                                  >
                                    <EditIcon size={16} />
                                  </button>
                                  <button
                                    type="button"
                                    className="org-icon-btn danger"
                                    onClick={() => setRemovingMembership(m)}
                                    title="Remove from organization"
                                    aria-label={`Remove ${m.memberName}`}
                                  >
                                    <TrashIcon size={16} />
                                  </button>
                                </div>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    )}
                  </div>

                  {membersQuery.hasNextPage && (
                    <div className="org-table-footer">
                      <button
                        type="button"
                        className="org-load-more"
                        onClick={() => membersQuery.fetchNextPage()}
                        disabled={membersQuery.isFetchingNextPage}
                      >
                        {membersQuery.isFetchingNextPage
                          ? "Loading..."
                          : "Load more members"}
                      </button>
                    </div>
                  )}
                </div>
              </>
            )}
          </section>
        </div>
      )}

      {/* ─── Panels & Modals ───────────────────────────────────────────────── */}
      {orgPanel && (
        <OrganizationFormPanel
          organization={orgPanel.mode === "edit" ? orgPanel.organization : null}
          onClose={() => setOrgPanel(null)}
          onSaved={(saved) => {
            setOrgPanel(null);
            setChosenOrgId(saved.id);
          }}
        />
      )}

      {showAddMember && selectedOrg && (
        <AddMemberPanel
          organization={selectedOrg}
          existingMemberIds={existingMemberIds}
          onClose={() => setShowAddMember(false)}
        />
      )}

      {editingMembership && (
        <EditMembershipPanel
          membership={editingMembership}
          onClose={() => setEditingMembership(null)}
        />
      )}

      <DeleteConfirmModal
        isOpen={!!deletingOrg}
        onClose={() => setDeletingOrg(null)}
        onConfirm={confirmDeleteOrg}
        itemName={deletingOrg?.name || "Organization"}
        title="Delete Organization"
        warningMessage="The organization will be removed and its members will no longer be listed under it. Member profiles themselves are not affected."
        confirmText={
          deleteOrganization.isPending ? "Deleting..." : "Delete Organization"
        }
      />

      <DeleteConfirmModal
        isOpen={!!removingMembership}
        onClose={() => setRemovingMembership(null)}
        onConfirm={confirmRemoveMembership}
        itemName={removingMembership?.memberName || "Member"}
        title="Remove Member"
        warningMessage={`They will be removed from ${removingMembership?.organizationName ?? "this organization"}. Their member profile is not affected.`}
        confirmText={deleteMembership.isPending ? "Removing..." : "Remove"}
      />
    </div>
  );
};

export default Organizations;
