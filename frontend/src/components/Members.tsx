import React, { useEffect, useRef, useState } from 'react';
import { useLayout } from '../context/LayoutContext';
import { CloseIcon, ExportIcon, MembersIcon, CalendarIcon, PlusIcon } from './Icons';
import ExportPanel from './ExportPanel';
import '../styles/Members.css';

// ─── Types ────────────────────────────────────────────────────────────────────

type MemberStatus = 'Active' | 'Inactive' | 'New Visitor';

interface ChurchMember {
  id: number;
  memberId: string;         // e.g. WF-101
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
  joinedDate: string;       // display string e.g. "Oct 31, 2023"
  joinedDateISO: string;    // for input field
  dateOfBirth: string;      // display
  dateOfBirthISO: string;
  gender: 'Male' | 'Female' | 'Other';
  ministryGroup: string;
  status: MemberStatus;
  address: string;
  hometown: string;
  region: string;
  ghanaPost: string;
  nextOfKin: string;
  emergencyContact: string;
  emergencyPhone: string;
}

type TabFilter = 'All Members' | 'Active' | 'New Visitors' | 'Archived';

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
  'Greater Accra Region',
  'Ashanti Region',
  'Eastern Region',
  'Western Region',
  'Central Region',
  'Northern Region',
  'Upper East Region',
  'Upper West Region',
  'Volta Region',
  'Brong-Ahafo Region',
  'Oti Region',
  'Savannah Region',
  'North East Region',
  'Bono Region',
  'Bono East Region',
  'Ahafo Region',
  'Western North Region',
];

// ─── Mock Data ────────────────────────────────────────────────────────────────

const mockMembers: ChurchMember[] = [
  {
    id: 1,
    memberId: 'WF-101',
    firstName: 'Sarah',
    lastName: 'Martinez',
    phone: '+233 54 202 8520',
    email: 'sarah.martinez@example.com',
    joinedDate: 'Oct 31, 2023',
    joinedDateISO: '2023-10-31',
    dateOfBirth: 'Mar 14, 1990',
    dateOfBirthISO: '1990-03-14',
    gender: 'Female',
    ministryGroup: "Women's Fellowship",
    status: 'Active',
    address: 'Hebron, Soldier Lane',
    hometown: 'Aburi',
    region: 'Eastern Region',
    ghanaPost: 'GPS-282-282',
    nextOfKin: 'James Martinez',
    emergencyContact: 'James Martinez',
    emergencyPhone: '+233 00 000 0000',
  },
  {
    id: 2,
    memberId: 'MF-028',
    firstName: 'John',
    lastName: 'Doe',
    phone: '+233 53 232 8520',
    email: 'john.doe@example.com',
    joinedDate: 'Sept 01, 2023',
    joinedDateISO: '2023-09-01',
    dateOfBirth: 'Jun 22, 1985',
    dateOfBirthISO: '1985-06-22',
    gender: 'Male',
    ministryGroup: "Men's Fellowship",
    status: 'Inactive',
    address: '14 Cantonments Road',
    hometown: 'Kumasi',
    region: 'Ashanti Region',
    ghanaPost: 'AK-039-5028',
    nextOfKin: 'Mary Doe',
    emergencyContact: 'Mary Doe',
    emergencyPhone: '+233 24 111 2233',
  },
  {
    id: 3,
    memberId: 'CH-057',
    firstName: 'Ruth',
    lastName: 'Acheampong',
    phone: '+233 54 202 3238',
    email: 'ruth.acheampong@example.com',
    joinedDate: 'Jan 31, 2023',
    joinedDateISO: '2023-01-31',
    dateOfBirth: 'Nov 03, 1995',
    dateOfBirthISO: '1995-11-03',
    gender: 'Female',
    ministryGroup: 'Choir',
    status: 'Active',
    address: '7 Osu Badu Street',
    hometown: 'Cape Coast',
    region: 'Central Region',
    ghanaPost: 'CC-005-1234',
    nextOfKin: 'Kwame Acheampong',
    emergencyContact: 'Kwame Acheampong',
    emergencyPhone: '+233 27 455 6677',
  },
  {
    id: 4,
    memberId: 'YT-003',
    firstName: 'Kofi',
    lastName: 'Mensah',
    phone: '+233 50 111 4422',
    email: 'kofi.mensah@example.com',
    joinedDate: 'Feb 14, 2024',
    joinedDateISO: '2024-02-14',
    dateOfBirth: 'Apr 18, 2002',
    dateOfBirthISO: '2002-04-18',
    gender: 'Male',
    ministryGroup: 'Youth',
    status: 'New Visitor',
    address: 'North Legon, Accra',
    hometown: 'Tamale',
    region: 'Northern Region',
    ghanaPost: 'NR-011-9090',
    nextOfKin: 'Abena Mensah',
    emergencyContact: 'Abena Mensah',
    emergencyPhone: '+233 26 777 8899',
  },
  {
    id: 5,
    memberId: 'US-019',
    firstName: 'Akosua',
    lastName: 'Frimpong',
    phone: '+233 55 333 7788',
    email: 'akosua.frimpong@example.com',
    joinedDate: 'Mar 05, 2022',
    joinedDateISO: '2022-03-05',
    dateOfBirth: 'Jul 29, 1988',
    dateOfBirthISO: '1988-07-29',
    gender: 'Female',
    ministryGroup: 'Ushers',
    status: 'Active',
    address: 'Spintex Road, Accra',
    hometown: 'Takoradi',
    region: 'Western Region',
    ghanaPost: 'WR-200-4567',
    nextOfKin: 'Emmanuel Frimpong',
    emergencyContact: 'Emmanuel Frimpong',
    emergencyPhone: '+233 20 999 1122',
  },
  {
    id: 6,
    memberId: 'MT-011',
    firstName: 'Nana',
    lastName: 'Boateng',
    phone: '+233 24 888 5566',
    email: 'nana.boateng@example.com',
    joinedDate: 'Nov 18, 2021',
    joinedDateISO: '2021-11-18',
    dateOfBirth: 'Dec 11, 1993',
    dateOfBirthISO: '1993-12-11',
    gender: 'Male',
    ministryGroup: 'Media Team',
    status: 'Active',
    address: 'Madina, Accra',
    hometown: 'Ho',
    region: 'Volta Region',
    ghanaPost: 'VR-088-3344',
    nextOfKin: 'Ama Boateng',
    emergencyContact: 'Ama Boateng',
    emergencyPhone: '+233 54 222 3344',
  },
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

const getInitials = (firstName: string, lastName: string) =>
  `${firstName[0] ?? ''}${lastName[0] ?? ''}`.toUpperCase();

const statusClass = (s: MemberStatus) => {
  if (s === 'Active') return 'status-active';
  if (s === 'Inactive') return 'status-inactive';
  return 'status-visitor';
};

const emptyForm = (): Omit<ChurchMember, 'id' | 'memberId' | 'joinedDate' | 'dateOfBirth'> => ({
  firstName: '',
  lastName: '',
  phone: '',
  email: '',
  joinedDateISO: '',
  dateOfBirthISO: '',
  gender: 'Male',
  ministryGroup: '',
  status: 'Active',
  address: '',
  hometown: '',
  region: '',
  ghanaPost: '',
  nextOfKin: '',
  emergencyContact: '',
  emergencyPhone: '',
});

// ─── Members Component ────────────────────────────────────────────────────────

const Members: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  const [members, setMembers] = useState<ChurchMember[]>(mockMembers);
  const [activeTab, setActiveTab] = useState<TabFilter>('All Members');
  const [searchQuery, setSearchQuery] = useState('');

  const [showAddPanel, setShowAddPanel] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [editingMember, setEditingMember] = useState<ChurchMember | null>(null);

  const [form, setForm] = useState(emptyForm());
  const nextId = useRef(mockMembers.length + 1);

  // ── Layout header ─────────────────────────────────────────────────────────

  const openAddPanel = () => {
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
  const newMembers = members.filter(m => m.status === 'New Visitor').length;
  // "Active Families" — mock: count distinct first-letter families
  const activeFamilies = new Set(members.filter(m => m.status === 'Active').map(m => m.lastName)).size;
  const retentionRate = Math.round((members.filter(m => m.status === 'Active').length / totalMembership) * 100);

  // ── Filtering ─────────────────────────────────────────────────────────────

  const tabFiltered = members.filter(m => {
    if (activeTab === 'All Members') return true;
    if (activeTab === 'Active') return m.status === 'Active';
    if (activeTab === 'New Visitors') return m.status === 'New Visitor';
    if (activeTab === 'Archived') return m.status === 'Inactive';
    return true;
  });

  const filtered = tabFiltered.filter(m => {
    const q = searchQuery.toLowerCase();
    return (
      m.firstName.toLowerCase().includes(q) ||
      m.lastName.toLowerCase().includes(q) ||
      m.memberId.toLowerCase().includes(q) ||
      m.ministryGroup.toLowerCase().includes(q) ||
      m.phone.includes(q)
    );
  });

  // ── Handlers ──────────────────────────────────────────────────────────────

  const openEditPanel = (member: ChurchMember) => {
    setEditingMember(member);
    setForm({
      firstName: member.firstName,
      lastName: member.lastName,
      phone: member.phone,
      email: member.email,
      joinedDateISO: member.joinedDateISO,
      dateOfBirthISO: member.dateOfBirthISO,
      gender: member.gender,
      ministryGroup: member.ministryGroup,
      status: member.status,
      address: member.address,
      hometown: member.hometown,
      region: member.region,
      ghanaPost: member.ghanaPost,
      nextOfKin: member.nextOfKin,
      emergencyContact: member.emergencyContact,
      emergencyPhone: member.emergencyPhone,
    });
  };

  const formatDateDisplay = (iso: string) => {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  };

  const handleSaveAdd = () => {
    if (!form.firstName.trim() || !form.lastName.trim()) return;
    const id = nextId.current++;
    const newMember: ChurchMember = {
      id,
      memberId: `MB-${String(id).padStart(3, '0')}`,
      firstName: form.firstName,
      lastName: form.lastName,
      phone: form.phone,
      email: form.email,
      joinedDate: formatDateDisplay(form.joinedDateISO),
      joinedDateISO: form.joinedDateISO,
      dateOfBirth: formatDateDisplay(form.dateOfBirthISO),
      dateOfBirthISO: form.dateOfBirthISO,
      gender: form.gender,
      ministryGroup: form.ministryGroup,
      status: form.status,
      address: form.address,
      hometown: form.hometown,
      region: form.region,
      ghanaPost: form.ghanaPost,
      nextOfKin: form.nextOfKin,
      emergencyContact: form.emergencyContact,
      emergencyPhone: form.emergencyPhone,
    };
    setMembers(prev => [newMember, ...prev]);
    setShowAddPanel(false);
  };

  const handleSaveEdit = () => {
    if (!editingMember) return;
    setMembers(prev =>
      prev.map(m =>
        m.id === editingMember.id
          ? {
              ...m,
              ...form,
              joinedDate: formatDateDisplay(form.joinedDateISO) || m.joinedDate,
              dateOfBirth: formatDateDisplay(form.dateOfBirthISO) || m.dateOfBirth,
            }
          : m
      )
    );
    setEditingMember(null);
  };

  const handleDelete = (id: number) => {
    setMembers(prev => prev.filter(m => m.id !== id));
  };

  const closePanel = () => {
    setShowAddPanel(false);
    setEditingMember(null);
  };

  const tabs: TabFilter[] = ['All Members', 'Active', 'New Visitors', 'Archived'];
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
          {filtered.length === 0 ? (
            <div className="members-empty-state">No members found.</div>
          ) : (
            <div className="members-card-grid">
              {filtered.map(member => (
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
                      <span>{member.phone}</span>
                    </div>
                    <div className="member-meta-row">
                      <CalendarIcon size={14} className="member-meta-icon" />
                      <span>Joined {member.joinedDate}</span>
                    </div>
                    <div className="member-meta-row">
                      <MembersIcon size={14} className="member-meta-icon" />
                      <span>{member.ministryGroup}</span>
                    </div>
                  </div>

                  {/* Member ID */}
                  <div className="member-card-id-row">
                    <span className="member-card-id-label">MEMBER ID</span>
                    <span className="member-card-id-value">{member.memberId}</span>
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
                      onClick={() => handleDelete(member.id)}
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

    </div>
  );
};

// ─── Shared Form ──────────────────────────────────────────────────────────────

type FormState = ReturnType<typeof emptyForm>;

const MemberFormFields: React.FC<{
  form: FormState;
  setForm: React.Dispatch<React.SetStateAction<FormState>>;
}> = ({ form, setForm }) => {
  const set = <K extends keyof FormState>(key: K, value: FormState[K]) =>
    setForm(prev => ({ ...prev, [key]: value }));

  return (
    <>
      {/* First + Last name */}
      <div className="mf-row">
        <div className="mf-group">
          <label className="mf-label">First Name</label>
          <input className="mf-input" placeholder="e.g., John" value={form.firstName}
            onChange={e => set('firstName', e.target.value)} />
        </div>
        <div className="mf-group">
          <label className="mf-label">Last Name</label>
          <input className="mf-input" placeholder="e.g., Doe" value={form.lastName}
            onChange={e => set('lastName', e.target.value)} />
        </div>
      </div>

      {/* Email */}
      <div className="mf-group">
        <label className="mf-label">Email Address</label>
        <input className="mf-input" type="email" placeholder="member@example.com"
          value={form.email} onChange={e => set('email', e.target.value)} />
      </div>

      {/* Phone */}
      <div className="mf-group">
        <label className="mf-label">Phone Number</label>
        <input className="mf-input" placeholder="+ 233 00 000 0000"
          value={form.phone} onChange={e => set('phone', e.target.value)} />
      </div>

      {/* Ministry + Status */}
      <div className="mf-row">
        <div className="mf-group">
          <label className="mf-label">Ministry Group</label>
          <select className="mf-select" value={form.ministryGroup}
            onChange={e => set('ministryGroup', e.target.value)}>
            <option value="">Select group</option>
            {MINISTRY_GROUPS.map(g => <option key={g} value={g}>{g}</option>)}
          </select>
        </div>
        <div className="mf-group">
          <label className="mf-label">Status</label>
          <select className="mf-select" value={form.status}
            onChange={e => set('status', e.target.value as MemberStatus)}>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="New Visitor">New Visitor</option>
          </select>
        </div>
      </div>

      {/* Joined date */}
      <div className="mf-group">
        <label className="mf-label">Joined Date</label>
        <input className="mf-input" type="date" placeholder="DD / MM / YY"
          value={form.joinedDateISO} onChange={e => set('joinedDateISO', e.target.value)} />
      </div>

      {/* Gender + DOB */}
      <div className="mf-row">
        <div className="mf-group">
          <label className="mf-label">Gender</label>
          <select className="mf-select" value={form.gender}
            onChange={e => set('gender', e.target.value as FormState['gender'])}>
            <option value="Male">Male</option>
            <option value="Female">Female</option>
            <option value="Other">Other</option>
          </select>
        </div>
        <div className="mf-group">
          <label className="mf-label">Date of Birth</label>
          <input className="mf-input" type="date" placeholder="DD / MM / YY"
            value={form.dateOfBirthISO} onChange={e => set('dateOfBirthISO', e.target.value)} />
        </div>
      </div>

      <div className="mf-divider" />

      {/* Residential address */}
      <div className="mf-group">
        <label className="mf-label">Residential Address</label>
        <input className="mf-input" placeholder="Hebron, Soldier Lane"
          value={form.address} onChange={e => set('address', e.target.value)} />
      </div>

      {/* Hometown */}
      <div className="mf-group">
        <label className="mf-label">Hometown</label>
        <input className="mf-input" placeholder="Aburi"
          value={form.hometown} onChange={e => set('hometown', e.target.value)} />
      </div>

      {/* Region */}
      <div className="mf-group">
        <label className="mf-label">Region</label>
        <select className="mf-select" value={form.region}
          onChange={e => set('region', e.target.value)}>
          <option value="">Select region</option>
          {GHANA_REGIONS.map(r => <option key={r} value={r}>{r}</option>)}
        </select>
      </div>

      {/* Ghana Post */}
      <div className="mf-group">
        <label className="mf-label">Ghana Post Address</label>
        <input className="mf-input" placeholder="GPS-282-282"
          value={form.ghanaPost} onChange={e => set('ghanaPost', e.target.value)} />
      </div>

      <div className="mf-divider" />

      {/* Next of Kin */}
      <div className="mf-group">
        <label className="mf-label">Next of Kin</label>
        <input className="mf-input" placeholder="Enter name here..."
          value={form.nextOfKin} onChange={e => set('nextOfKin', e.target.value)} />
      </div>

      {/* Emergency Contact Name */}
      <div className="mf-group">
        <label className="mf-label">Emergency Contact Name</label>
        <input className="mf-input" placeholder="Enter name here..."
          value={form.emergencyContact} onChange={e => set('emergencyContact', e.target.value)} />
      </div>

      {/* Emergency Contact Phone */}
      <div className="mf-group">
        <label className="mf-label">Emergency Contact Phone Number</label>
        <input className="mf-input" placeholder="+ 233 00 000 0000"
          value={form.emergencyPhone} onChange={e => set('emergencyPhone', e.target.value)} />
      </div>
    </>
  );
};

export default Members;
