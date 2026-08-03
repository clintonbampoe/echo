import React, { useEffect, useState } from 'react';
import { useLayout } from '../context/LayoutContext';
import {
  CloseIcon, CalendarIcon, MapPinIcon, ClockIcon, ChevronLeftIcon
} from './Icons';
import DeleteConfirmModal from './common/DeleteConfirmModal';
import ExportPanel from './ExportPanel';
import '../styles/Events.css';

// ─── Types ────────────────────────────────────────────────────────────────────


interface ChurchEvent {
  id: string;
  name: string;
  location: string;
  organizerId: string;
  startDate: string;
  startTime: string;
  endDate: string;
  endTime: string;
  capacity: number | null; // null means unlimited
  registeredCount: number;
  attendedCount: number;
  description: string;
}

interface RegistrationRecord {
  id: string;
  eventId: string;
  memberId: string;
  memberName: string;
  memberInitials: string;
  registrationDate: string;
  checkInTime?: string;
}

interface AttendanceRecord {
  id: string;
  eventId: string;
  memberId: string;
  memberName: string;
  memberInitials: string;
  checkInTime: string;
}

// ─── Mock Data ────────────────────────────────────────────────────────────────

const mockMembersDirectory = [
  { id: 'M-101', name: 'Pastor David' },
  { id: 'M-102', name: 'Sarah Kent' },
  { id: 'M-103', name: 'John Maxwell' },
  { id: 'M-104', name: 'Dr. Mensah' },
  { id: 'M-105', name: 'Jane Doe' },
];

const mockEvents: ChurchEvent[] = [
  {
    id: 'E-001',
    name: 'New Members Class',
    location: 'Adult Confere...',
    organizerId: 'M-101',
    startDate: 'Oct 27, 2026',
    startTime: '9:00 AM',
    endDate: 'Oct 27, 2026',
    endTime: '11:30 AM',
    capacity: 400,
    registeredCount: 225,
    attendedCount: 0,
    description: 'Orientation class for new members.',
  },
  {
    id: 'E-002',
    name: 'Youth Camp',
    location: 'Church Premises',
    organizerId: 'M-102',
    startDate: 'Oct 27, 2026',
    startTime: '9:00 AM',
    endDate: 'Oct 30, 2026',
    endTime: '11:30 AM',
    capacity: 482,
    registeredCount: 203,
    attendedCount: 34,
    description: 'Join us for our regular Sunday morning worship service. This week\'s gathering will focus on community, faith, and fellowship. Please arrive early if you require childcare services or special seating arrangements.',
  },
  {
    id: 'E-003',
    name: 'Health Walk',
    location: 'Aburi',
    organizerId: 'M-103',
    startDate: 'May 27, 2026',
    startTime: '6:00 AM',
    endDate: 'May 27, 2026',
    endTime: '11:00 AM',
    capacity: 400,
    registeredCount: 342,
    attendedCount: 0,
    description: 'A 10km health walk for all members.',
  },
];

const mockRegistrations: RegistrationRecord[] = [
  { id: 'R-001', eventId: 'E-002', memberId: 'M-1', memberName: 'Sarah Kent', memberInitials: 'SK', registrationDate: 'Oct 01, 2026', checkInTime: '3:00 PM' },
  { id: 'R-002', eventId: 'E-002', memberId: 'M-2', memberName: 'John Maxwell', memberInitials: 'JM', registrationDate: 'Oct 05, 2026', checkInTime: '3:00 PM' },
  { id: 'R-003', eventId: 'E-002', memberId: 'M-3', memberName: 'John Doe', memberInitials: 'JD', registrationDate: 'Oct 10, 2026' },
  { id: 'R-004', eventId: 'E-002', memberId: 'M-4', memberName: 'David Okonjo', memberInitials: 'DO', registrationDate: 'Oct 12, 2026', checkInTime: '3:00 PM' },
  { id: 'R-005', eventId: 'E-002', memberId: 'M-5', memberName: 'Habib', memberInitials: 'H', registrationDate: 'Oct 15, 2026' },
  { id: 'R-006', eventId: 'E-002', memberId: 'M-6', memberName: 'Baba Tunday', memberInitials: 'BT', registrationDate: 'Oct 16, 2026' },
];

const mockAttendance: AttendanceRecord[] = [
  { id: 'A-001', eventId: 'E-002', memberId: 'M-1', memberName: 'Sarah Kent', memberInitials: 'SK', checkInTime: '3:00 PM' },
  { id: 'A-002', eventId: 'E-002', memberId: 'M-2', memberName: 'John Maxwell', memberInitials: 'JM', checkInTime: '3:00 PM' },
  { id: 'A-003', eventId: 'E-002', memberId: 'M-3', memberName: 'John Doe', memberInitials: 'JD', checkInTime: '3:00 PM' },
  { id: 'A-004', eventId: 'E-002', memberId: 'M-4', memberName: 'David Okonjo', memberInitials: 'DO', checkInTime: '3:00 PM' },
  { id: 'A-005', eventId: 'E-002', memberId: 'M-5', memberName: 'Habib', memberInitials: 'H', checkInTime: '3:00 PM' },
  { id: 'A-006', eventId: 'E-002', memberId: 'M-6', memberName: 'Baba Tunday', memberInitials: 'BT', checkInTime: '3:00 PM' },
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

const getProgressPercent = (value: number, total: number | null) => {
  if (!total || total === 0) return 0;
  return Math.min(Math.round((value / total) * 100), 100);
};

const emptyEventForm = () => ({
  name: '',
  startDate: '',
  startTime: '',
  endDate: '',
  endTime: '',
  location: '',
  organizerId: '',
  capacity: '',
  description: '',
});

// ─── Component ────────────────────────────────────────────────────────────────

const Events: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  // Navigation State
  const [activeTab, setActiveTab] = useState<'Upcoming' | 'Past'>('Upcoming');
  const [viewMode, setViewMode] = useState<'list' | 'registrations' | 'attendance'>('list');
  const [viewingEvent, setViewingEvent] = useState<ChurchEvent | null>(null);

  // Panels State
  const [showEventDetailPanel, setShowEventDetailPanel] = useState(false);
  const [showCreatePanel, setShowCreatePanel] = useState(false);
  const [showEditPanel, setShowEditPanel] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);

  // Form State
  const [form, setForm] = useState(emptyEventForm());
  
  const [showRegForm, setShowRegForm] = useState(false);
  const [editingReg, setEditingReg] = useState<RegistrationRecord | null>(null);
  const [regForm, setRegForm] = useState({ memberId: '', registrationDate: '', checkInTime: '' });

  const [showAttForm, setShowAttForm] = useState(false);
  const [editingAtt, setEditingAtt] = useState<AttendanceRecord | null>(null);
  const [attForm, setAttForm] = useState({ memberId: '', checkInTime: '' });

  // ─── Layout TopBar Setup ────────────────────────────────────────────────────

  useEffect(() => {
    if (viewMode === 'registrations' && viewingEvent) {
      setTitle(
        <button className="back-btn" onClick={() => { setViewMode('list'); setShowEventDetailPanel(true); }}>
          <ChevronLeftIcon size={20} />
          <span>Events / {viewingEvent.name}</span>
        </button>
      );
      setCtas([
        { type: 'search', placeholder: 'Search Attendees...' },
        { type: 'button', label: 'Export', icon: 'export', variant: 'secondary', onClick: () => setShowExportModal(true) },
        { type: 'button', label: 'Add Entry', icon: 'plus', variant: 'primary', onClick: () => {
          setRegForm({ memberId: '', registrationDate: new Date().toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' }), checkInTime: '' });
          setEditingReg(null);
          setShowRegForm(true);
        } },
      ]);
    } else if (viewMode === 'attendance' && viewingEvent) {
      setTitle(
        <button className="back-btn" onClick={() => { setViewMode('list'); setShowEventDetailPanel(true); }}>
          <ChevronLeftIcon size={20} />
          <span>Events / {viewingEvent.name}</span>
        </button>
      );
      setCtas([
        { type: 'search', placeholder: 'Search Attendees...' },
        { type: 'button', label: 'Export', icon: 'export', variant: 'secondary', onClick: () => setShowExportModal(true) },
        { type: 'button', label: 'Check-in', icon: 'plus', variant: 'primary', onClick: () => {
          setAttForm({ memberId: '', checkInTime: new Date().toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true }) });
          setEditingAtt(null);
          setShowAttForm(true);
        } },
      ]);
    } else {
      setTitle('Events');
      setCtas([
        { type: 'search', placeholder: 'Search Events...' },
        { type: 'button', label: 'Calendar View', icon: 'calendar', variant: 'secondary', onClick: () => {} },
        { type: 'button', label: 'Add Event', icon: 'plus', variant: 'primary', onClick: handleOpenCreate },
      ]);
    }
  }, [viewMode, viewingEvent, setTitle, setCtas]);


  // ─── Handlers ───────────────────────────────────────────────────────────────

  const handleOpenEventPanel = (event: ChurchEvent) => {
    setViewingEvent(event);
    setShowEventDetailPanel(true);
  };

  const handleOpenCreate = () => {
    setForm(emptyEventForm());
    setShowCreatePanel(true);
  };

  const handleOpenEdit = () => {
    if (!viewingEvent) return;
    setForm({
      name: viewingEvent.name,
      startDate: viewingEvent.startDate,
      startTime: viewingEvent.startTime,
      endDate: viewingEvent.endDate,
      endTime: viewingEvent.endTime,
      location: viewingEvent.location,
      organizerId: viewingEvent.organizerId,
      capacity: viewingEvent.capacity ? String(viewingEvent.capacity) : '',
      description: viewingEvent.description,
    });
    setShowEventDetailPanel(false);
    setShowEditPanel(true);
  };

  const handleViewRegistrations = () => {
    setShowEventDetailPanel(false);
    setViewMode('registrations');
  };

  const handleViewAttendance = () => {
    setShowEventDetailPanel(false);
    setViewMode('attendance');
  };

  const handleEditReg = (reg: RegistrationRecord) => {
    setRegForm({ memberId: reg.memberId, registrationDate: reg.registrationDate, checkInTime: reg.checkInTime || '' });
    setEditingReg(reg);
    setShowRegForm(true);
  };

  const handleEditAtt = (att: AttendanceRecord) => {
    setAttForm({ memberId: att.memberId, checkInTime: att.checkInTime });
    setEditingAtt(att);
    setShowAttForm(true);
  };

  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingItemName, setDeletingItemName] = useState('');

  const handleDelete = (name: string) => {
    setDeletingItemName(name);
    setShowDeleteConfirm(true);
  };

  const confirmDelete = () => {
    setShowDeleteConfirm(false);
    setDeletingItemName('');
  };

  // ─── Stats ──────────────────────────────────────────────────────────────────

  const upcomingCount = mockEvents.length; // Simplified for mockup
  const totalRegs = mockEvents.reduce((s, e) => s + e.registeredCount, 0);
  const pendingApprovals = 3; // Mock
  const venuesBooked = 2; // Mock

  // ─── Render Forms ───────────────────────────────────────────────────────────

  const renderEventForm = () => (
    <>
      <div className="event-form-group">
        <label className="event-form-label">Event Name</label>
        <input type="text" className="event-form-input" placeholder="e.g. Youth Camp" value={form.name} onChange={e => setForm({...form, name: e.target.value})} />
      </div>

      <div className="event-form-row">
        <div className="event-form-group">
          <label className="event-form-label">Start Date</label>
          <div className="event-form-input-icon-wrap">
            <input type="text" className="event-form-input" placeholder="DD/MM/YY" value={form.startDate} onChange={e => setForm({...form, startDate: e.target.value})} />
            <CalendarIcon size={16} className="event-form-input-icon" />
          </div>
        </div>
        <div className="event-form-group">
          <label className="event-form-label">Start Time</label>
          <div className="event-form-input-icon-wrap">
            <input type="text" className="event-form-input" placeholder="00:00 AM" value={form.startTime} onChange={e => setForm({...form, startTime: e.target.value})} />
            <ClockIcon size={16} className="event-form-input-icon" />
          </div>
        </div>
      </div>

      <div className="event-form-row">
        <div className="event-form-group">
          <label className="event-form-label">End Date</label>
          <div className="event-form-input-icon-wrap">
            <input type="text" className="event-form-input" placeholder="DD/MM/YY" value={form.endDate} onChange={e => setForm({...form, endDate: e.target.value})} />
            <CalendarIcon size={16} className="event-form-input-icon" />
          </div>
        </div>
        <div className="event-form-group">
          <label className="event-form-label">End Time</label>
          <div className="event-form-input-icon-wrap">
            <input type="text" className="event-form-input" placeholder="00:00 AM" value={form.endTime} onChange={e => setForm({...form, endTime: e.target.value})} />
            <ClockIcon size={16} className="event-form-input-icon" />
          </div>
        </div>
      </div>

      <div className="event-form-group">
        <label className="event-form-label">Location</label>
        <div className="event-form-input-icon-wrap">
          <input type="text" className="event-form-input" placeholder="e.g. Church Premises" value={form.location} onChange={e => setForm({...form, location: e.target.value})} />
          <MapPinIcon size={16} className="event-form-input-icon" />
        </div>
      </div>

      <div className="event-form-row">
        <div className="event-form-group">
          <label className="event-form-label">Organizer</label>
          <select className="event-form-select" value={form.organizerId} onChange={e => setForm({...form, organizerId: e.target.value})}>
            <option value="">Select an Organizer</option>
            {mockMembersDirectory.map(member => (
              <option key={member.id} value={member.id}>{member.name}</option>
            ))}
          </select>
        </div>
        <div className="event-form-group">
          <label className="event-form-label">Capacity</label>
          <input type="text" className="event-form-input" placeholder="Leave blank for unlimited" value={form.capacity} onChange={e => setForm({...form, capacity: e.target.value.replace(/\D/g, '')})} />
        </div>
      </div>

      <div className="event-form-group">
        <label className="event-form-label">Description</label>
        <textarea className="event-form-textarea" placeholder="Add event details..." value={form.description} onChange={e => setForm({...form, description: e.target.value})} />
      </div>
    </>
  );

  // ─── Main Render ────────────────────────────────────────────────────────────

  return (
    <div className="events-container">
      
      {/* ─── LIST VIEW ──────────────────────────────────────────────────────── */}
      {viewMode === 'list' && (
        <>
          <div className="events-summary-cards">
            <div className="events-summary-card">
              <span className="events-card-label">Upcoming Events</span>
              <div className="events-card-value">{upcomingCount}</div>
            </div>
            <div className="events-summary-card">
              <span className="events-card-label">Total Registrations</span>
              <div className="events-card-value">{totalRegs}</div>
            </div>
            <div className="events-summary-card">
              <span className="events-card-label">Pending Approvals</span>
              <div className="events-card-value">{pendingApprovals}</div>
            </div>
            <div className="events-summary-card">
              <span className="events-card-label">Venues Booked</span>
              <div className="events-card-value">{venuesBooked} / 8</div>
            </div>
          </div>

          <div className="events-toolbar">
            <div className="events-tabs">
              {(['Upcoming', 'Past'] as const).map(tab => (
                <button
                  key={tab}
                  className={`events-tab ${activeTab === tab ? 'active' : ''}`}
                  onClick={() => setActiveTab(tab)}
                >
                  {tab}
                </button>
              ))}
            </div>
          </div>

          <div className="events-card-grid">
            {mockEvents.map(event => {
              const pct = getProgressPercent(event.registeredCount, event.capacity);
              return (
                <div key={event.id} className="event-card">
                  <div className="event-card-header">
                    <h3 className="event-card-title">{event.name}</h3>
                  </div>

                  <div className="event-card-details">
                    <div className="event-detail-row">
                      <MapPinIcon size={14} />
                      <span>{event.location}</span>
                    </div>
                    <div className="event-detail-row">
                      <CalendarIcon size={14} />
                      <span>{event.startDate}</span>
                    </div>
                    <div className="event-detail-row">
                      <ClockIcon size={14} />
                      <span>{event.startTime} - {event.endTime}</span>
                    </div>
                  </div>

                  <div className="event-card-progress-section">
                    <div className="event-card-amounts">
                      <span>{event.registeredCount} / {event.capacity || '∞'} Attendees</span>
                      <span className="event-card-amounts-sub">{pct}%</span>
                    </div>
                    <div className="event-card-bar-track">
                      <div className="event-card-bar-fill" style={{ width: `${pct}%` }} />
                    </div>
                  </div>

                  <div className="event-card-actions">
                    <button className="event-action-btn" onClick={() => handleOpenEventPanel(event)}>
                      Manage Event
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        </>
      )}

      {/* ─── DETAIL VIEW: REGISTRATIONS ─────────────────────────────────────── */}
      {viewMode === 'registrations' && viewingEvent && (
        <div className="detail-table-card">
          <div className="detail-table-header">
            <h3 className="detail-table-title">Registrations: {viewingEvent.name}</h3>
          </div>
          <table className="events-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Reg Date</th>
                <th>Check-in Time</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {mockRegistrations.map(reg => (
                <tr key={reg.id}>
                  <td>
                    <div className="member-cell">
                      <div className="member-avatar">{reg.memberInitials}</div>
                      <span>{reg.memberName}</span>
                    </div>
                  </td>
                  <td>{reg.registrationDate}</td>
                  <td>{reg.checkInTime || '-'}</td>
                  <td>
                    <div className="actions-cell">
                      <button className="action-sm-btn" onClick={() => handleEditReg(reg)}>Edit</button>
                      <button className="action-sm-btn" onClick={() => handleDelete(`Registration for ${reg.memberName}`)}>Delete</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* ─── DETAIL VIEW: ATTENDANCE ────────────────────────────────────────── */}
      {viewMode === 'attendance' && viewingEvent && (
        <div className="detail-table-card">
          <div className="detail-table-header">
            <h3 className="detail-table-title">Attendance: {viewingEvent.name}</h3>
          </div>
          <table className="events-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Check-in Time</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {mockAttendance.map(att => (
                <tr key={att.id}>
                  <td>
                    <div className="member-cell">
                      <div className="member-avatar">{att.memberInitials}</div>
                      <span>{att.memberName}</span>
                    </div>
                  </td>
                  <td>{att.checkInTime}</td>
                  <td>
                    <div className="actions-cell">
                      <button className="action-sm-btn" onClick={() => handleEditAtt(att)}>Edit</button>
                      <button className="action-sm-btn" onClick={() => handleDelete(`Attendance for ${att.memberName}`)}>Delete</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* ─── SIDE PANEL: EVENT OVERVIEW ─────────────────────────────────────── */}
      {showEventDetailPanel && viewingEvent && (
        <div className="event-panel-overlay" onClick={() => setShowEventDetailPanel(false)}>
          <div className="event-side-panel" onClick={e => e.stopPropagation()}>
            
            <div className="event-panel-header">
              <h2 className="event-panel-title">{viewingEvent.name}</h2>
              <button className="event-panel-close" onClick={() => setShowEventDetailPanel(false)}>
                <CloseIcon />
              </button>
            </div>
            
            <div className="event-panel-body">
              <div className="event-detail-section">
                <div className="event-detail-date-time">
                  <div className="event-detail-date-time-label">Date / Time</div>
                  <div className="event-detail-date">{viewingEvent.startDate}</div>
                  <div className="event-detail-time">{viewingEvent.startTime} - {viewingEvent.endTime}</div>
                </div>

                <div className="event-info-box">
                  <div className="event-info-row">
                    <span className="event-info-label">Location</span>
                    <span className="event-info-value">{viewingEvent.location}</span>
                  </div>
                  <div className="event-info-row">
                    <span className="event-info-label">Organized By</span>
                    <span className="event-info-value">
                      {mockMembersDirectory.find(m => m.id === viewingEvent.organizerId)?.name || 'Unknown Organizer'}
                    </span>
                  </div>
                </div>

                <div className="event-detail-date-time-label">Description</div>
                <p className="event-detail-desc">{viewingEvent.description}</p>
              </div>

              {/* Registration Tracking */}
              <div className="event-detail-section" style={{ borderTop: '1px solid var(--border)', paddingTop: '20px' }}>
                <div className="event-detail-date-time-label">Registration Status</div>
                <div className="event-card-progress-section" style={{ borderTop: 'none', paddingTop: '8px' }}>
                  <div className="event-card-amounts">
                    <span>{viewingEvent.registeredCount} / {viewingEvent.capacity || '∞'} Registered</span>
                    <span className="event-card-amounts-sub">{getProgressPercent(viewingEvent.registeredCount, viewingEvent.capacity)}% Full</span>
                  </div>
                  <div className="event-card-bar-track">
                    <div className="event-card-bar-fill" style={{ width: `${getProgressPercent(viewingEvent.registeredCount, viewingEvent.capacity)}%` }} />
                  </div>
                </div>
                <div className="event-card-actions">
                  <button className="event-action-btn" onClick={handleViewRegistrations}>
                    View Registrations
                  </button>
                </div>
              </div>

              {/* Attendance Tracking */}
              <div className="event-detail-section" style={{ borderTop: '1px solid var(--border)', paddingTop: '20px' }}>
                <div className="event-detail-date-time-label">Attendance Status</div>
                <div className="event-card-progress-section" style={{ borderTop: 'none', paddingTop: '8px' }}>
                  <div className="event-card-amounts">
                    <span>{viewingEvent.attendedCount} / {viewingEvent.registeredCount} Attended</span>
                    <span className="event-card-amounts-sub">{getProgressPercent(viewingEvent.attendedCount, viewingEvent.registeredCount)}% Present</span>
                  </div>
                  <div className="event-card-bar-track">
                    <div className="event-card-bar-fill" style={{ width: `${getProgressPercent(viewingEvent.attendedCount, viewingEvent.registeredCount)}%` }} />
                  </div>
                </div>
                <div className="event-card-actions">
                  <button className="event-action-btn" onClick={handleViewAttendance}>
                    View Attendance
                  </button>
                </div>
              </div>

            </div>

            <div className="event-panel-footer">
              <button className="event-action-btn primary" onClick={handleOpenEdit}>
                Edit Event Details
              </button>
            </div>

          </div>
        </div>
      )}

      {/* ─── SIDE PANEL: CREATE/EDIT FORM ───────────────────────────────────── */}
      {(showCreatePanel || showEditPanel) && (
        <div className="event-panel-overlay" onClick={() => { setShowCreatePanel(false); setShowEditPanel(false); }}>
          <div className="event-side-panel" onClick={e => e.stopPropagation()}>
            
            <div className="event-panel-header">
              <h2 className="event-panel-title">
                {showCreatePanel ? 'Create Event' : 'Edit Event'}
              </h2>
              <button className="event-panel-close" onClick={() => { setShowCreatePanel(false); setShowEditPanel(false); }}>
                <CloseIcon />
              </button>
            </div>
            
            <div className="event-panel-body">
              {renderEventForm()}
            </div>

            <div className="event-panel-footer">
              <button className="event-action-btn" onClick={() => { setShowCreatePanel(false); setShowEditPanel(false); }}>
                Cancel
              </button>
              <button className="event-action-btn primary" onClick={() => { setShowCreatePanel(false); setShowEditPanel(false); }}>
                Save Changes
              </button>
            </div>

          </div>
        </div>
      )}

      {/* ─── SIDE PANEL: REGISTRATION FORM ──────────────────────────────────── */}
      {showRegForm && (
        <div className="event-panel-overlay" onClick={() => setShowRegForm(false)}>
          <div className="event-side-panel" onClick={e => e.stopPropagation()}>
            <div className="event-panel-header">
              <h2 className="event-panel-title">{editingReg ? 'Edit Registration' : 'Add Registration'}</h2>
              <button className="event-panel-close" onClick={() => setShowRegForm(false)}>
                <CloseIcon />
              </button>
            </div>
            <div className="event-panel-body">
              <div className="event-form-group">
                <label className="event-form-label">Member</label>
                <select className="event-form-select" value={regForm.memberId} onChange={e => setRegForm({...regForm, memberId: e.target.value})}>
                  <option value="">Select Member</option>
                  {mockMembersDirectory.map(member => (
                    <option key={member.id} value={member.id}>{member.name}</option>
                  ))}
                </select>
              </div>
              <div className="event-form-group">
                <label className="event-form-label">Registration Date</label>
                <div className="event-form-input-icon-wrap">
                  <input type="text" className="event-form-input" value={regForm.registrationDate} onChange={e => setRegForm({...regForm, registrationDate: e.target.value})} />
                  <CalendarIcon size={16} className="event-form-input-icon" />
                </div>
              </div>
              <div className="event-form-group">
                <label className="event-form-label">Check-in Time (Optional)</label>
                <div className="event-form-input-icon-wrap">
                  <input type="text" className="event-form-input" placeholder="00:00 AM" value={regForm.checkInTime} onChange={e => setRegForm({...regForm, checkInTime: e.target.value})} />
                  <ClockIcon size={16} className="event-form-input-icon" />
                </div>
              </div>
            </div>
            <div className="event-panel-footer">
              <button className="event-action-btn" onClick={() => setShowRegForm(false)}>Cancel</button>
              <button className="event-action-btn primary" onClick={() => setShowRegForm(false)}>Save Changes</button>
            </div>
          </div>
        </div>
      )}

      {/* ─── SIDE PANEL: ATTENDANCE FORM ────────────────────────────────────── */}
      {showAttForm && (
        <div className="event-panel-overlay" onClick={() => setShowAttForm(false)}>
          <div className="event-side-panel" onClick={e => e.stopPropagation()}>
            <div className="event-panel-header">
              <h2 className="event-panel-title">{editingAtt ? 'Edit Attendance' : 'Add Check-in'}</h2>
              <button className="event-panel-close" onClick={() => setShowAttForm(false)}>
                <CloseIcon />
              </button>
            </div>
            <div className="event-panel-body">
              <div className="event-form-group">
                <label className="event-form-label">Member</label>
                <select className="event-form-select" value={attForm.memberId} onChange={e => setAttForm({...attForm, memberId: e.target.value})}>
                  <option value="">Select Member</option>
                  {mockMembersDirectory.map(member => (
                    <option key={member.id} value={member.id}>{member.name}</option>
                  ))}
                </select>
              </div>
              <div className="event-form-group">
                <label className="event-form-label">Check-in Time</label>
                <div className="event-form-input-icon-wrap">
                  <input type="text" className="event-form-input" placeholder="00:00 AM" value={attForm.checkInTime} onChange={e => setAttForm({...attForm, checkInTime: e.target.value})} />
                  <ClockIcon size={16} className="event-form-input-icon" />
                </div>
              </div>
            </div>
            <div className="event-panel-footer">
              <button className="event-action-btn" onClick={() => setShowAttForm(false)}>Cancel</button>
              <button className="event-action-btn primary" onClick={() => setShowAttForm(false)}>Save Changes</button>
            </div>
          </div>
        </div>
      )}

      {/* ─── EXPORT PANEL ───────────────────────────────────────────────────── */}
      {showExportModal && (
        <ExportPanel
          title={`Export ${viewMode === 'registrations' ? 'Registrations' : 'Attendance'}`}
          exportConfig={{
            title: `Event ${viewMode === 'registrations' ? 'Registrations' : 'Attendance'} Report`,
            filename: `echo-event-${viewMode}`,
            columns: [
              { key: 'memberName', label: 'Member' },
              ...(viewMode === 'registrations' ? [{ key: 'registrationDate', label: 'Reg Date' }] : []),
              { key: 'checkInTime', label: 'Check-in Time' },
            ],
            rows: viewMode === 'registrations' ? mockRegistrations : mockAttendance,
          }}
          onClose={() => setShowExportModal(false)}
        />
      )}

      <DeleteConfirmModal
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={confirmDelete}
        itemName={deletingItemName}
        title="Delete Record"
        confirmText="Delete"
      />
    </div>
  );
};

export default Events;
