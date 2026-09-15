import React, { useEffect, useState } from 'react';
import { useLayout } from '../hooks/useLayout';
import { useMembers, useCreateMember, useUpdateMember, useDeleteMember } from '../hooks/useMembers';
import type { Member } from '../types/member';
import { CloseIcon, MembersIcon, CalendarIcon } from './Icons';
import DeleteConfirmModal from './common/DeleteConfirmModal';
import ExportPanel from './ExportPanel';
import '../styles/Members.css';

// ─── Constants ────────────────────────────────────────────────────────────────

const MINISTRY_GROUPS = [
  "Women's Fellowship",
  "Men's Fellowship",
  'Youth',
  'Choir',
  'Ushers',
  'Children Ministry',
  'Prayer Band',
  'Media Team',
];

const GHANA_REGIONS = [
  { value: 'GreaterAccra', label: 'Greater Accra Region' },
  { value: 'Ashanti', label: 'Ashanti Region' },
  { value: 'Eastern', label: 'Eastern Region' },
  { value: 'Western', label: 'Western Region' },
  { value: 'Central', label: 'Central Region' },
  { value: 'Northern', label: 'Northern Region' },
  { value: 'UpperEast', label: 'Upper East Region' },
  { value: 'UpperWest', label: 'Upper West Region' },
  { value: 'Volta', label: 'Volta Region' },
  { value: 'Bono', label: 'Bono Region' },
  { value: 'Oti', label: 'Oti Region' },
  { value: 'Savannah', label: 'Savannah Region' },
  { value: 'NorthEast', label: 'North East Region' },
  { value: 'BonoEast', label: 'Bono East Region' },
  { value: 'Ahafo', label: 'Ahafo Region' },
  { value: 'WesternNorth', label: 'Western North Region' },
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

const getInitials = (firstName: string, lastName: string) =>
  `${firstName[0] ?? ''}${lastName[0] ?? ''}`.toUpperCase();

const statusClass = (s: MemberStatus) => {
  if (s === 'Active') return 'status-active';
  if (s === 'Inactive') return 'status-inactive';
  return 'status-visitor';
};

const emptyForm = (): Partial<Member> => ({
  firstName: '',
  lastName: '',
  phoneNumber: '',
  emailAddress: '',
  dateOfBirth: '',
  gender: 'Male',
  status: 'Active',
  residentialAddress: '',
  hometown: '',
  region: '',
  gpsAddress: '',
  nextOfKin: '',
  emergencyContactName: '',
  emergencyContactPhoneNumber: '',
  maritalStatus: 'Single',
  city: 'Accra'
});

// ─── Members Component ────────────────────────────────────────────────────────

const Members: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  const [activeTab, setActiveTab] = useState<string>('All Members');
  const [searchQuery, setSearchQuery] = useState('');

  // Map ActiveTab to backend status
  let backendStatus = '';
  if (activeTab === 'Active') backendStatus = 'Active';
  
  if (activeTab === 'Inactive') backendStatus = 'Inactive';
  if (activeTab === 'Archived') backendStatus = 'Archived';

  const { data: pagedResponse, isLoading } = useMembers({ 
    name: searchQuery || undefined,
    status: backendStatus || undefined
  });
  
  const members = pagedResponse?.data || [];

  const createMember = useCreateMember();
  const updateMember = useUpdateMember();
  const deleteMember = useDeleteMember();

  const [showAddPanel, setShowAddPanel] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [editingMember, setEditingMember] = useState<Member | null>(null);

  const [form, setForm] = useState(emptyForm());

  const [panelError, setPanelError] = useState<string | null>(null);

  // ── Layout header ─────────────────────────────────────────────────────────

  const openAddPanel = () => {
    setPanelError(null);
    setForm(emptyForm());
    setShowAddPanel(true);
  };

  useEffect(() => {
    setTitle('Members');
    setCtas([
      {
        type: 'button',
        label: 'Export',
        icon: 'export',
        variant: 'secondary',
        onClick: () => setShowExportModal(true),
      },
      {
        type: 'button',
        label: 'Add Member',
        icon: 'plus',
        variant: 'primary',
        onClick: openAddPanel,
      },
    ]);
  }, [setTitle, setCtas]);

  // ── Derived stats ─────────────────────────────────────────────────────────

  const totalMembership = members.length;
  const newMembers = members.filter(m => m.status === 'Archived').length;
  // "Active Families" — mock: count distinct first-letter families
  const activeFamilies = new Set(members.filter(m => m.status === 'Active').map(m => m.lastName)).size;
  const retentionRate = totalMembership === 0 ? 0 : Math.round((members.filter(m => m.status === 'Active').length / totalMembership) * 100);

  // ── Handlers ──────────────────────────────────────────────────────────────

  const openEditPanel = (member: Member) => {
    setPanelError(null);
    setEditingMember(member);
    setForm({
      firstName: member.firstName,
      lastName: member.lastName,
      phoneNumber: member.phoneNumber,
      emailAddress: member.emailAddress || '',
      dateOfBirth: member.dateOfBirth || '',
      gender: member.gender,
      status: member.status,
      residentialAddress: member.residentialAddress,
      hometown: member.hometown,
      region: member.region,
      gpsAddress: member.gpsAddress || '',
      nextOfKin: member.nextOfKin,
      emergencyContactName: member.emergencyContactName,
      emergencyContactPhoneNumber: member.emergencyContactPhoneNumber,
      maritalStatus: member.maritalStatus,
      city: member.city,
    });
  };

  const formatDateDisplay = (iso?: string) => {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  };

  const handleSaveAdd = async () => {
    setPanelError(null);
    if (!form.firstName?.trim() || !form.lastName?.trim()) {
      setPanelError('First name and last name are required.');
      return;
    }
    try {
      await createMember.mutateAsync(form);
      setShowAddPanel(false);
    } catch (err: any) {
      setPanelError(err?.message || 'Failed to create member.');
    }
  };

  const handleSaveEdit = async () => {
    if (!editingMember) return;
    setPanelError(null);
    try {
      await updateMember.mutateAsync({ id: editingMember.id, data: form });
      setEditingMember(null);
    } catch (err: any) {
      setPanelError(err?.message || 'Failed to update member.');
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
    } catch (err: any) {
      alert(err?.message || 'Failed to delete member.');
    }
  };

  const closePanel = () => {
    setPanelError(null);
    setShowAddPanel(false);
    setEditingMember(null);
  };

  const tabs = ['All Members', 'Active', 'Inactive', 'Archived'];
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
          <span className="members-stat-label">New Members</span>
          <div className="members-stat-value">{newMembers}</div>
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

        {/* Tabs toolbar */}
        <div className="members-card-header">
          <div className="members-tabs">
            {tabs.map(tab => (
              <button
                key={tab}
                className={`members-tab ${activeTab === tab ? 'members-tab-active' : ''}`}
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
            onChange={e => setSearchQuery(e.target.value)}
          />
        </div>

        {/* Card grid body */}
        <div className="members-card-body">
          {isLoading ? <div className="members-empty-state">Loading...</div> : members.length === 0 ? (
            <div className="members-empty-state">No members found.</div>
          ) : (
            <div className="members-card-grid">
              {members.map(member => (
                <div key={member.id} className="member-card">

                  {/* Name row + status badge */}
                  <div className="member-card-top">
                    <div className="member-card-avatar">
                      {getInitials(member.firstName, member.lastName)}
                    </div>
                    <div className="member-card-identity">
                      <span className="member-card-name">
                        {member.firstName}{'\n'}{member.lastName}
                      </span>
                    </div>
                    <span className={`member-status-badge ${statusClass(member.status)}`}>
                      {member.status}
                    </span>
                  </div>

                  {/* Meta info */}
                  <div className="member-card-meta">
                    <div className="member-meta-row">
                      {/* Phone icon */}
                      <svg className="member-meta-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07A19.5 19.5 0 0 1 4.9 12a19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 3.81 1h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 8.91a16 16 0 0 0 6 6l.96-.96a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/>
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

                  {/* Member ID */}
                  <div className="member-card-id-row">
                    <span className="member-card-id-label">MEMBER ID</span>
                    <span className="member-card-id-value">{member.id.split('-')[0]}</span>
                  </div>

                  {/* Actions */}
                  <div className="member-card-actions">
                    <button
                      className="member-action-btn"
                      onClick={() => openEditPanel(member)}
                    >
                      Edit
                    </button>
                    <button
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
          <div className="members-side-panel" onClick={e => e.stopPropagation()}>

            {/* Header */}
            <div className="members-panel-header">
              <div>
                <h2 className="members-panel-title">
                  {showAddPanel ? 'Add Member' : 'Edit Member'}
                </h2>
                <p className="members-panel-subtitle">
                  {showAddPanel ? 'Fill in details to add a new member' : 'Update member profile details'}
                </p>
                {panelError && (
                  <div style={{ color: '#ef4444', fontSize: '0.875rem', marginTop: '0.5rem', fontWeight: 500 }}>
                    {panelError}
                  </div>
                )}
              </div>
              <button className="members-panel-close" onClick={closePanel}>
                <CloseIcon />
              </button>
            </div>

            {/* Scrollable body */}
            <div className="members-panel-body">
              <MemberFormFields form={form} setForm={setForm} />
            </div>

            {/* Footer */}
            <div className="members-panel-footer">
              <button className="members-btn members-btn-secondary" onClick={closePanel}>
                Cancel
              </button>
              <button
                className="members-btn members-btn-primary"
                onClick={showAddPanel ? handleSaveAdd : handleSaveEdit}
              >
                Save Changes
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
            title: 'Members Roster',
            filename: 'echo-members-roster',
            columns: [
              { key: 'memberId', label: 'Member ID' },
              { key: 'firstName', label: 'First Name' },
              { key: 'lastName', label: 'Last Name' },
              { key: 'email', label: 'Email' },
              { key: 'phone', label: 'Phone' },
              { key: 'status', label: 'Status' },
              { key: 'ministryGroup', label: 'Ministry Group' },
              { key: 'joinedDate', label: 'Joined Date' },
              { key: 'gender', label: 'Gender' },
              { key: 'dateOfBirth', label: 'Date of Birth' },
              { key: 'address', label: 'Residential Address' },
              { key: 'hometown', label: 'Hometown' },
              { key: 'region', label: 'Region' },
              { key: 'ghanaPost', label: 'Ghana Post' },
              { key: 'emergencyContact', label: 'Emergency Contact' },
              { key: 'emergencyPhone', label: 'Emergency Phone' },
            ],
            // Use the filtered members list or all members based on preference.
            // Using all members for the full roster is usually safer.
            rows: members,
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
        itemName={deletingMember ? `${deletingMember.firstName} ${deletingMember.lastName}`.trim() : ''}
        title="Delete Member"
        confirmText="Delete Member"
      />
    </div>
  );
};

// ─── Shared Form ──────────────────────────────────────────────────────────────

const MemberFormFields: React.FC<{
  form: Partial<Member>;
  setForm: React.Dispatch<React.SetStateAction<Partial<Member>>>;
}> = ({ form, setForm }) => {
  const set = <K extends keyof Member>(key: K, value: Member[K]) =>
    setForm(prev => ({ ...prev, [key]: value }));

  return (
    <>
      <div className="mf-row">
        <div className="mf-group">
          <label className="mf-label">First Name</label>
          <input className="mf-input" placeholder="e.g., John" value={form.firstName || ''}
            onChange={e => set('firstName', e.target.value)} />
        </div>
        <div className="mf-group">
          <label className="mf-label">Last Name</label>
          <input className="mf-input" placeholder="e.g., Doe" value={form.lastName || ''}
            onChange={e => set('lastName', e.target.value)} />
        </div>
      </div>

      <div className="mf-group">
        <label className="mf-label">Email Address</label>
        <input className="mf-input" type="email" placeholder="member@example.com"
          value={form.emailAddress || ''} onChange={e => set('emailAddress', e.target.value)} />
      </div>

      <div className="mf-group">
        <label className="mf-label">Phone Number</label>
        <input className="mf-input" placeholder="+ 233 00 000 0000"
          value={form.phoneNumber || ''} onChange={e => set('phoneNumber', e.target.value)} />
      </div>

      <div className="mf-row">
        <div className="mf-group">
          <label className="mf-label">Marital Status</label>
          <select className="mf-select" value={form.maritalStatus || 'Single'}
            onChange={e => set('maritalStatus', e.target.value as any)}>
            <option value="Single">Single</option>
            <option value="Married">Married</option>
            
            <option value="Widowed">Widowed</option>
          </select>
        </div>
        <div className="mf-group">
          <label className="mf-label">Status</label>
          <select className="mf-select" value={form.status || 'Active'}
            onChange={e => set('status', e.target.value as any)}>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="Archived">Archived</option>
          </select>
        </div>
      </div>

      <div className="mf-row">
        <div className="mf-group">
          <label className="mf-label">Gender</label>
          <select className="mf-select" value={form.gender || 'Male'}
            onChange={e => set('gender', e.target.value as any)}>
            <option value="Male">Male</option>
            <option value="Female">Female</option>
            <option value="Other">Other</option>
          </select>
        </div>
        <div className="mf-group">
          <label className="mf-label">Date of Birth</label>
          <input className="mf-input" type="date"
            value={form.dateOfBirth || ''} onChange={e => set('dateOfBirth', e.target.value)} />
        </div>
      </div>

      <div className="mf-divider" />

      <div className="mf-group">
        <label className="mf-label">Residential Address</label>
        <input className="mf-input" placeholder="Hebron, Soldier Lane"
          value={form.residentialAddress || ''} onChange={e => set('residentialAddress', e.target.value)} />
      </div>

      <div className="mf-group">
        <label className="mf-label">City</label>
        <input className="mf-input" placeholder="Accra"
          value={form.city || ''} onChange={e => set('city', e.target.value)} />
      </div>

      <div className="mf-group">
        <label className="mf-label">Hometown</label>
        <input className="mf-input" placeholder="Aburi"
          value={form.hometown || ''} onChange={e => set('hometown', e.target.value)} />
      </div>

      <div className="mf-group">
        <label className="mf-label">Region</label>
        <select className="mf-select" value={form.region || ''}
          onChange={e => set('region', e.target.value)}>
          <option value="">Select region</option>
          {GHANA_REGIONS.map(r => <option key={r.value} value={r.value}>{r.label}</option>)}
        </select>
      </div>

      <div className="mf-group">
        <label className="mf-label">Ghana Post Address</label>
        <input className="mf-input" placeholder="GPS-282-282"
          value={form.gpsAddress || ''} onChange={e => set('gpsAddress', e.target.value)} />
      </div>

      <div className="mf-divider" />

      <div className="mf-group">
        <label className="mf-label">Next of Kin</label>
        <input className="mf-input" placeholder="Enter name here..."
          value={form.nextOfKin || ''} onChange={e => set('nextOfKin', e.target.value)} />
      </div>

      <div className="mf-group">
        <label className="mf-label">Emergency Contact Name</label>
        <input className="mf-input" placeholder="Enter name here..."
          value={form.emergencyContactName || ''} onChange={e => set('emergencyContactName', e.target.value)} />
      </div>

      <div className="mf-group">
        <label className="mf-label">Emergency Contact Phone</label>
        <input className="mf-input" placeholder="+ 233 00 000 0000"
          value={form.emergencyContactPhoneNumber || ''} onChange={e => set('emergencyContactPhoneNumber', e.target.value)} />
      </div>
    </>
  );
};

export default Members;

