import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useLayout } from '../hooks/useLayout';
import '../styles/Events.css';
import DeleteConfirmModal from './common/DeleteConfirmModal';
import ExportPanel from './ExportPanel';
import { getErrorMessage } from '../utils/errors';
import {
  CalendarIcon,
  ChevronLeftIcon,
  ClockIcon,
  CloseIcon,
  MapPinIcon,
} from './Icons';
import {
  useEvents,
  useCreateEvent,
  useUpdateEvent,
  useDeleteEvent,
  useOrganizations,
  useCreateOrganization,
  useEventRegistrations,
  useCreateEventRegistration,
  useDeleteEventRegistration,
  useEventAttendance,
  useCreateEventAttendance,
  useDeleteEventAttendance,
} from '../hooks/useEvents';
import { useMembers } from '../hooks/useMembers';
import type { Event, EventRegistration, EventAttendance } from '../types/event';

// ─── Helpers ──────────────────────────────────────────────────────────────────

const formatDate = (dateStr?: string | null) => {
  if (!dateStr) return 'TBD';
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return d.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
};

const formatTime = (timeStr?: string | null) => {
  if (!timeStr) return '';
  const parts = timeStr.split(':');
  if (parts.length >= 2) {
    const hours = parseInt(parts[0], 10);
    const minutes = parts[1];
    const ampm = hours >= 12 ? 'PM' : 'AM';
    const h12 = hours % 12 || 12;
    return `${h12}:${minutes} ${ampm}`;
  }
  return timeStr;
};

const getInitials = (name?: string) => {
  if (!name) return '??';
  const parts = name.trim().split(/\s+/);
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
};

const getProgressPercent = (value: number, total: number | null | undefined) => {
  if (!total || total === 0) return 0;
  return Math.min(Math.round((value / total) * 100), 100);
};

interface FormState {
  name: string;
  organizationId: string;
  organizerId: string;
  startDate: string;
  startTime: string;
  endDate: string;
  endTime: string;
  location: string;
  capacity: string;
  description: string;
}

const emptyEventForm = (): FormState => ({
  name: '',
  organizationId: '',
  organizerId: '',
  startDate: new Date().toISOString().split('T')[0],
  startTime: '09:00',
  endDate: new Date().toISOString().split('T')[0],
  endTime: '11:00',
  location: '',
  capacity: '',
  description: '',
});

// ─── Component ────────────────────────────────────────────────────────────────

const Events: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  // Queries
  const { data: eventsData, isLoading: isLoadingEvents } = useEvents();
  const { data: organizationsData } = useOrganizations();
  const { data: membersData } = useMembers({}, 100);

  // Mutations
  const createEventMutation = useCreateEvent();
  const updateEventMutation = useUpdateEvent();
  const deleteEventMutation = useDeleteEvent();
  const createOrgMutation = useCreateOrganization();

  // Navigation State
  const [activeTab, setActiveTab] = useState<'Upcoming' | 'Past'>('Upcoming');
  const [viewMode, setViewMode] = useState<'list' | 'registrations' | 'attendance'>('list');
  const [viewingEventId, setViewingEventId] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  // Panels State
  const [showEventDetailPanel, setShowEventDetailPanel] = useState(false);
  const [showCreatePanel, setShowCreatePanel] = useState(false);
  const [showEditPanel, setShowEditPanel] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);

  // Form State
  const [form, setForm] = useState<FormState>(emptyEventForm());
  const [formError, setFormError] = useState<string | null>(null);

  // Inline Organization Creation
  const [isAddingOrg, setIsAddingOrg] = useState(false);
  const [newOrgName, setNewOrgName] = useState('');
  const [orgError, setOrgError] = useState<string | null>(null);

  // Sub-record form state
  const [showRegForm, setShowRegForm] = useState(false);
  const [regForm, setRegForm] = useState({ memberId: '', registrationDate: new Date().toISOString().split('T')[0] });

  const [showAttForm, setShowAttForm] = useState(false);
  const [attForm, setAttForm] = useState({ memberId: '', checkInTime: '09:00' });

  // Delete State
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingType, setDeletingType] = useState<'event' | 'registration' | 'attendance'>('event');
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deletingItemName, setDeletingItemName] = useState('');

  const eventsList = useMemo(() => eventsData?.data || [], [eventsData]);
  const organizationsList = useMemo(() => organizationsData?.data || [], [organizationsData]);
  const membersList = useMemo(() => membersData?.data || [], [membersData]);

  const viewingEvent = useMemo(
    () => eventsList.find(e => e.id === viewingEventId) || null,
    [eventsList, viewingEventId]
  );

  // Query registrations and attendance for current event
  const { data: registrationsData } = useEventRegistrations(viewingEventId || undefined);
  const { data: attendanceData } = useEventAttendance(viewingEventId || undefined);

  const createRegistrationMutation = useCreateEventRegistration();
  const deleteRegistrationMutation = useDeleteEventRegistration();
  const createAttendanceMutation = useCreateEventAttendance();
  const deleteAttendanceMutation = useDeleteEventAttendance();

  const registrationsList = useMemo(() => registrationsData?.data || [], [registrationsData]);
  const attendanceList = useMemo(() => attendanceData?.data || [], [attendanceData]);

  // ─── Handlers ───────────────────────────────────────────────────────────────

  const handleOpenEventPanel = (event: Event) => {
    setViewingEventId(event.id);
    setShowEventDetailPanel(true);
  };

  const handleOpenCreate = useCallback(() => {
    setForm({
      ...emptyEventForm(),
      organizationId: organizationsList[0]?.id || '',
      organizerId: membersList[0]?.id || '',
    });
    setFormError(null);
    setShowCreatePanel(true);
  }, [organizationsList, membersList]);

  const handleOpenEdit = () => {
    if (!viewingEvent) return;
    setForm({
      name: viewingEvent.name,
      organizationId: viewingEvent.organizationId,
      organizerId: viewingEvent.organizerId,
      startDate: viewingEvent.startDate ? viewingEvent.startDate.split('T')[0] : '',
      startTime: viewingEvent.startTime ? viewingEvent.startTime.slice(0, 5) : '09:00',
      endDate: viewingEvent.endDate ? viewingEvent.endDate.split('T')[0] : '',
      endTime: viewingEvent.endTime ? viewingEvent.endTime.slice(0, 5) : '11:00',
      location: viewingEvent.location || '',
      capacity: viewingEvent.capacity ? String(viewingEvent.capacity) : '',
      description: viewingEvent.description || '',
    });
    setFormError(null);
    setShowEventDetailPanel(false);
    setShowEditPanel(true);
  };

  const handleAddOrg = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newOrgName.trim()) return;
    try {
      setOrgError(null);
      const res = await createOrgMutation.mutateAsync({ name: newOrgName.trim() });
      if (res && res.id) {
        setForm(prev => ({ ...prev, organizationId: res.id }));
      }
      setNewOrgName('');
      setIsAddingOrg(false);
    } catch (err: unknown) {
      setOrgError(getErrorMessage(err, 'Failed to create organization'));
    }
  };

  const handleSaveCreate = async () => {
    if (!form.name.trim()) {
      setFormError('Event name is required.');
      return;
    }
    if (!form.organizationId) {
      setFormError('Please select or create an organizing group / ministry.');
      return;
    }
    if (!form.organizerId) {
      setFormError('Please select an event organizer.');
      return;
    }
    if (!form.startDate || !form.endDate) {
      setFormError('Please enter start and end dates.');
      return;
    }

    try {
      setFormError(null);
      await createEventMutation.mutateAsync({
        name: form.name.trim(),
        organizationId: form.organizationId,
        organizerId: form.organizerId,
        startDate: form.startDate,
        endDate: form.endDate,
        startTime: form.startTime ? `${form.startTime}:00` : null,
        endTime: form.endTime ? `${form.endTime}:00` : null,
        location: form.location.trim() || null,
        capacity: form.capacity ? parseInt(form.capacity, 10) : null,
        description: form.description.trim() || null,
      });
      setShowCreatePanel(false);
    } catch (err: unknown) {
      setFormError(getErrorMessage(err, 'Failed to create event'));
    }
  };

  const handleSaveEdit = async () => {
    if (!viewingEvent) return;
    if (!form.name.trim()) {
      setFormError('Event name is required.');
      return;
    }

    try {
      setFormError(null);
      await updateEventMutation.mutateAsync({
        id: viewingEvent.id,
        data: {
          name: form.name.trim(),
          organizationId: form.organizationId || undefined,
          organizerId: form.organizerId || undefined,
          startDate: form.startDate || undefined,
          endDate: form.endDate || undefined,
          startTime: form.startTime ? `${form.startTime}:00` : null,
          endTime: form.endTime ? `${form.endTime}:00` : null,
          location: form.location.trim() || null,
          capacity: form.capacity ? parseInt(form.capacity, 10) : null,
          description: form.description.trim() || null,
        },
      });
      setShowEditPanel(false);
    } catch (err: unknown) {
      setFormError(getErrorMessage(err, 'Failed to update event'));
    }
  };

  const handleSaveRegistration = async () => {
    if (!viewingEventId || !regForm.memberId) return;
    try {
      await createRegistrationMutation.mutateAsync({
        eventId: viewingEventId,
        memberId: regForm.memberId,
        registrationDate: regForm.registrationDate,
      });
      setShowRegForm(false);
    } catch (err: unknown) {
      console.error('Failed to register member', err);
    }
  };

  const handleSaveAttendance = async () => {
    if (!viewingEventId || !attForm.memberId) return;
    try {
      await createAttendanceMutation.mutateAsync({
        eventId: viewingEventId,
        memberId: attForm.memberId,
        checkInTime: `${attForm.checkInTime}:00`,
      });
      setShowAttForm(false);
    } catch (err: unknown) {
      console.error('Failed to check-in member', err);
    }
  };

  const handleDeleteEvent = (event: Event) => {
    setDeletingType('event');
    setDeletingId(event.id);
    setDeletingItemName(event.name);
    setShowDeleteConfirm(true);
  };

  const handleDeleteRegistration = (reg: EventRegistration) => {
    setDeletingType('registration');
    setDeletingId(reg.id);
    setDeletingItemName(`Registration for ${reg.memberName}`);
    setShowDeleteConfirm(true);
  };

  const handleDeleteAttendance = (att: EventAttendance) => {
    setDeletingType('attendance');
    setDeletingId(att.id);
    setDeletingItemName(`Check-in for ${att.memberName}`);
    setShowDeleteConfirm(true);
  };

  const confirmDelete = async () => {
    if (!deletingId) return;
    try {
      if (deletingType === 'event') {
        await deleteEventMutation.mutateAsync(deletingId);
        setShowEventDetailPanel(false);
        setViewingEventId(null);
      } else if (deletingType === 'registration') {
        await deleteRegistrationMutation.mutateAsync(deletingId);
      } else if (deletingType === 'attendance') {
        await deleteAttendanceMutation.mutateAsync(deletingId);
      }
      setShowDeleteConfirm(false);
      setDeletingId(null);
    } catch (err: unknown) {
      console.error('Failed to delete', err);
    }
  };

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
        { type: 'button', label: 'Export', icon: 'export', variant: 'secondary', onClick: () => setShowExportModal(true) },
        {
          type: 'button',
          label: 'Add Registration',
          icon: 'plus',
          variant: 'primary',
          onClick: () => {
            setRegForm({
              memberId: membersList[0]?.id || '',
              registrationDate: new Date().toISOString().split('T')[0],
            });
            setShowRegForm(true);
          },
        },
      ]);
    } else if (viewMode === 'attendance' && viewingEvent) {
      setTitle(
        <button className="back-btn" onClick={() => { setViewMode('list'); setShowEventDetailPanel(true); }}>
          <ChevronLeftIcon size={20} />
          <span>Events / {viewingEvent.name}</span>
        </button>
      );
      setCtas([
        { type: 'button', label: 'Export', icon: 'export', variant: 'secondary', onClick: () => setShowExportModal(true) },
        {
          type: 'button',
          label: 'Check-in Member',
          icon: 'plus',
          variant: 'primary',
          onClick: () => {
            const now = new Date();
            const timeStr = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;
            setAttForm({
              memberId: membersList[0]?.id || '',
              checkInTime: timeStr,
            });
            setShowAttForm(true);
          },
        },
      ]);
    } else {
      setTitle('Events');
      setCtas([
        { type: 'button', label: 'Add Event', icon: 'plus', variant: 'primary', onClick: handleOpenCreate },
      ]);
    }
  }, [viewMode, viewingEvent, handleOpenCreate, setTitle, setCtas, membersList]);

  // ─── Filtered Data & Stats ──────────────────────────────────────────────────

  const now = new Date();
  const upcomingEvents = eventsList.filter(e => new Date(e.endDate || e.startDate) >= new Date(now.setHours(0,0,0,0)));
  const pastEvents = eventsList.filter(e => new Date(e.endDate || e.startDate) < new Date(now.setHours(0,0,0,0)));

  const currentTabEvents = activeTab === 'Upcoming' ? upcomingEvents : pastEvents;

  const filteredEvents = currentTabEvents.filter(e =>
    e.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    (e.location && e.location.toLowerCase().includes(searchQuery.toLowerCase())) ||
    (e.organizerName && e.organizerName.toLowerCase().includes(searchQuery.toLowerCase()))
  );

  const upcomingCount = upcomingEvents.length;
  const totalEvents = eventsList.length;

  // ─── Render Forms ───────────────────────────────────────────────────────────

  const renderEventForm = () => (
    <>
      {formError && (
        <div style={{ color: '#ef4444', marginBottom: '12px', fontSize: '13px' }}>
          {formError}
        </div>
      )}

      {/* Event Name */}
      <div className="event-form-group">
        <label className="event-form-label">Event Name *</label>
        <input
          type="text"
          className="event-form-input"
          placeholder="e.g. Youth Camp 2026"
          value={form.name}
          onChange={e => setForm({ ...form, name: e.target.value })}
        />
      </div>

      {/* Organizing Ministry */}
      <div className="event-form-group">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '6px' }}>
          <label className="event-form-label" style={{ marginBottom: 0 }}>Organizing Ministry / Group *</label>
          {!isAddingOrg && (
            <button
              type="button"
              onClick={() => { setIsAddingOrg(true); setOrgError(null); }}
              style={{
                background: 'none',
                border: 'none',
                color: 'var(--primary, #4361ee)',
                fontSize: '12px',
                fontWeight: 600,
                cursor: 'pointer',
                padding: '0 4px',
              }}
            >
              + New Group
            </button>
          )}
        </div>

        {isAddingOrg ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
            <div style={{ display: 'flex', gap: '8px' }}>
              <input
                type="text"
                className="event-form-input"
                placeholder="Ministry name (e.g. Youth Ministry)"
                value={newOrgName}
                onChange={e => setNewOrgName(e.target.value)}
                autoFocus
              />
              <button
                type="button"
                className="event-action-btn primary"
                style={{ padding: '0 14px', whiteSpace: 'nowrap', fontSize: '13px' }}
                onClick={handleAddOrg}
                disabled={createOrgMutation.isPending || !newOrgName.trim()}
              >
                {createOrgMutation.isPending ? 'Adding...' : 'Add'}
              </button>
              <button
                type="button"
                className="event-action-btn"
                style={{ padding: '0 10px', fontSize: '13px' }}
                onClick={() => { setIsAddingOrg(false); setNewOrgName(''); setOrgError(null); }}
              >
                Cancel
              </button>
            </div>
            {orgError && (
              <span style={{ color: '#ef4444', fontSize: '12px' }}>{orgError}</span>
            )}
          </div>
        ) : (
          <select
            className="event-form-select"
            value={form.organizationId}
            onChange={e => setForm({ ...form, organizationId: e.target.value })}
          >
            <option value="">Select Organizing Ministry</option>
            {organizationsList.map(org => (
              <option key={org.id} value={org.id}>{org.name}</option>
            ))}
          </select>
        )}
      </div>

      {/* Dates Row */}
      <div className="event-form-row">
        <div className="event-form-group">
          <label className="event-form-label">Start Date *</label>
          <input
            type="date"
            className="event-form-input"
            value={form.startDate}
            onChange={e => setForm({ ...form, startDate: e.target.value })}
          />
        </div>
        <div className="event-form-group">
          <label className="event-form-label">Start Time</label>
          <input
            type="time"
            className="event-form-input"
            value={form.startTime}
            onChange={e => setForm({ ...form, startTime: e.target.value })}
          />
        </div>
      </div>

      <div className="event-form-row">
        <div className="event-form-group">
          <label className="event-form-label">End Date *</label>
          <input
            type="date"
            className="event-form-input"
            value={form.endDate}
            onChange={e => setForm({ ...form, endDate: e.target.value })}
          />
        </div>
        <div className="event-form-group">
          <label className="event-form-label">End Time</label>
          <input
            type="time"
            className="event-form-input"
            value={form.endTime}
            onChange={e => setForm({ ...form, endTime: e.target.value })}
          />
        </div>
      </div>

      {/* Location */}
      <div className="event-form-group">
        <label className="event-form-label">Location</label>
        <div className="event-form-input-icon-wrap">
          <input
            type="text"
            className="event-form-input"
            placeholder="e.g. Main Sanctuary / Aburi"
            value={form.location}
            onChange={e => setForm({ ...form, location: e.target.value })}
          />
          <MapPinIcon size={16} className="event-form-input-icon" />
        </div>
      </div>

      {/* Organizer + Capacity */}
      <div className="event-form-row">
        <div className="event-form-group">
          <label className="event-form-label">Organizer *</label>
          <select
            className="event-form-select"
            value={form.organizerId}
            onChange={e => setForm({ ...form, organizerId: e.target.value })}
          >
            <option value="">Select Organizer</option>
            {membersList.map(m => (
              <option key={m.id} value={m.id}>{m.firstName} {m.lastName}</option>
            ))}
          </select>
        </div>
        <div className="event-form-group">
          <label className="event-form-label">Capacity</label>
          <input
            type="text"
            className="event-form-input"
            placeholder="Unlimited"
            value={form.capacity}
            onChange={e => setForm({ ...form, capacity: e.target.value.replace(/\D/g, '') })}
          />
        </div>
      </div>

      {/* Description */}
      <div className="event-form-group">
        <label className="event-form-label">Description</label>
        <textarea
          className="event-form-textarea"
          placeholder="Add event details, goals, and itinerary..."
          value={form.description}
          onChange={e => setForm({ ...form, description: e.target.value })}
        />
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
              <span className="events-card-label">Total Events</span>
              <div className="events-card-value">{totalEvents}</div>
            </div>
            <div className="events-summary-card">
              <span className="events-card-label">Organized Groups</span>
              <div className="events-card-value">{organizationsList.length}</div>
            </div>
            <div className="events-summary-card">
              <span className="events-card-label">Registered Members</span>
              <div className="events-card-value">{membersList.length}</div>
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
            <div style={{ display: 'flex', alignItems: 'center', background: 'white', borderRadius: '8px', padding: '6px 12px', border: '1px solid #e2e8f0', minWidth: '240px' }}>
              <input
                type="text"
                placeholder="Search events..."
                value={searchQuery}
                onChange={e => setSearchQuery(e.target.value)}
                style={{ border: 'none', outline: 'none', width: '100%', fontSize: '13px' }}
              />
            </div>
          </div>

          <div className="events-card-grid">
            {isLoadingEvents ? (
              <div style={{ padding: '32px', textAlign: 'center', color: '#64748b' }}>Loading church events...</div>
            ) : filteredEvents.length === 0 ? (
              <div style={{ padding: '32px', textAlign: 'center', color: '#64748b' }}>No {activeTab.toLowerCase()} events found.</div>
            ) : (
              filteredEvents.map(event => {
                return (
                  <div key={event.id} className="event-card">
                    <div className="event-card-header">
                      <h3 className="event-card-title">{event.name}</h3>
                      <span style={{ fontSize: '11px', color: '#64748b', fontWeight: 600, background: '#f1f5f9', padding: '2px 8px', borderRadius: '12px' }}>
                        {event.organizationName || 'General'}
                      </span>
                    </div>

                    <div className="event-card-details">
                      <div className="event-detail-row">
                        <MapPinIcon size={14} />
                        <span>{event.location || 'Church Premises'}</span>
                      </div>
                      <div className="event-detail-row">
                        <CalendarIcon size={14} />
                        <span>{formatDate(event.startDate)} {event.endDate && event.endDate !== event.startDate ? ` - ${formatDate(event.endDate)}` : ''}</span>
                      </div>
                      <div className="event-detail-row">
                        <ClockIcon size={14} />
                        <span>{formatTime(event.startTime)} {event.endTime ? `- ${formatTime(event.endTime)}` : ''}</span>
                      </div>
                    </div>

                    <div className="event-card-actions">
                      <button className="event-action-btn" onClick={() => handleOpenEventPanel(event)}>
                        Manage Event
                      </button>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </>
      )}

      {/* ─── DETAIL VIEW: REGISTRATIONS ─────────────────────────────────────── */}
      {viewMode === 'registrations' && viewingEvent && (
        <div className="detail-table-card">
          <div className="detail-table-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h3 className="detail-table-title">Registrations: {viewingEvent.name}</h3>
            <button
              className="event-action-btn primary"
              onClick={() => {
                setRegForm({
                  memberId: membersList[0]?.id || '',
                  registrationDate: new Date().toISOString().split('T')[0],
                });
                setShowRegForm(true);
              }}
            >
              + Add Registration
            </button>
          </div>
          <table className="events-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Reg Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {registrationsList.length === 0 ? (
                <tr>
                  <td colSpan={3} style={{ textAlign: 'center', padding: '24px', color: '#94a3b8' }}>
                    No registrations found for this event yet.
                  </td>
                </tr>
              ) : (
                registrationsList.map(reg => (
                  <tr key={reg.id}>
                    <td>
                      <div className="member-cell">
                        <div className="member-avatar">{getInitials(reg.memberName)}</div>
                        <span>{reg.memberName}</span>
                      </div>
                    </td>
                    <td>{formatDate(reg.registrationDate)}</td>
                    <td>
                      <div className="actions-cell">
                        <button
                          className="action-sm-btn"
                          style={{ color: '#ef4444' }}
                          onClick={() => handleDeleteRegistration(reg)}
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* ─── DETAIL VIEW: ATTENDANCE ────────────────────────────────────────── */}
      {viewMode === 'attendance' && viewingEvent && (
        <div className="detail-table-card">
          <div className="detail-table-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h3 className="detail-table-title">Attendance Check-ins: {viewingEvent.name}</h3>
            <button
              className="event-action-btn primary"
              onClick={() => {
                const now = new Date();
                const timeStr = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;
                setAttForm({
                  memberId: membersList[0]?.id || '',
                  checkInTime: timeStr,
                });
                setShowAttForm(true);
              }}
            >
              + Check-in Member
            </button>
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
              {attendanceList.length === 0 ? (
                <tr>
                  <td colSpan={3} style={{ textAlign: 'center', padding: '24px', color: '#94a3b8' }}>
                    No check-ins recorded for this event yet.
                  </td>
                </tr>
              ) : (
                attendanceList.map(att => (
                  <tr key={att.id}>
                    <td>
                      <div className="member-cell">
                        <div className="member-avatar">{getInitials(att.memberName)}</div>
                        <span>{att.memberName}</span>
                      </div>
                    </td>
                    <td>{formatTime(att.checkInTime)}</td>
                    <td>
                      <div className="actions-cell">
                        <button
                          className="action-sm-btn"
                          style={{ color: '#ef4444' }}
                          onClick={() => handleDeleteAttendance(att)}
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* ─── SIDE PANEL: EVENT OVERVIEW ─────────────────────────────────────── */}
      {showEventDetailPanel && viewingEvent && (
        <div className="event-panel-overlay" onClick={() => setShowEventDetailPanel(false)}>
          <div className="event-side-panel" onClick={e => e.stopPropagation()}>

            <div className="event-panel-header">
              <div>
                <h2 className="event-panel-title">{viewingEvent.name}</h2>
                <span style={{ fontSize: '12px', color: '#64748b' }}>{viewingEvent.organizationName}</span>
              </div>
              <button className="event-panel-close" onClick={() => setShowEventDetailPanel(false)}>
                <CloseIcon />
              </button>
            </div>

            <div className="event-panel-body">
              <div className="event-detail-section">
                <div className="event-detail-date-time">
                  <div className="event-detail-date-time-label">Date / Time</div>
                  <div className="event-detail-date">{formatDate(viewingEvent.startDate)} {viewingEvent.endDate !== viewingEvent.startDate ? `- ${formatDate(viewingEvent.endDate)}` : ''}</div>
                  <div className="event-detail-time">{formatTime(viewingEvent.startTime)} {viewingEvent.endTime ? `- ${formatTime(viewingEvent.endTime)}` : ''}</div>
                </div>

                <div className="event-info-box">
                  <div className="event-info-row">
                    <span className="event-info-label">Location</span>
                    <span className="event-info-value">{viewingEvent.location || 'Church Premises'}</span>
                  </div>
                  <div className="event-info-row">
                    <span className="event-info-label">Organized By</span>
                    <span className="event-info-value">{viewingEvent.organizerName || 'Church Admin'}</span>
                  </div>
                </div>

                <div className="event-detail-date-time-label">Description</div>
                <p className="event-detail-desc">{viewingEvent.description || 'No description provided.'}</p>
              </div>

              {/* Registration Tracking */}
              <div className="event-detail-section" style={{ borderTop: '1px solid var(--border)', paddingTop: '20px' }}>
                <div className="event-detail-date-time-label">Registration Status</div>
                <div className="event-card-progress-section" style={{ borderTop: 'none', paddingTop: '8px' }}>
                  <div className="event-card-amounts">
                    <span>{registrationsList.length} / {viewingEvent.capacity || '∞'} Registered</span>
                    <span className="event-card-amounts-sub">{getProgressPercent(registrationsList.length, viewingEvent.capacity)}% Full</span>
                  </div>
                  <div className="event-card-bar-track">
                    <div className="event-card-bar-fill" style={{ width: `${getProgressPercent(registrationsList.length, viewingEvent.capacity)}%` }} />
                  </div>
                </div>
                <div className="event-card-actions">
                  <button className="event-action-btn" onClick={() => { setShowEventDetailPanel(false); setViewMode('registrations'); }}>
                    View Registrations ({registrationsList.length})
                  </button>
                </div>
              </div>

              {/* Attendance Tracking */}
              <div className="event-detail-section" style={{ borderTop: '1px solid var(--border)', paddingTop: '20px' }}>
                <div className="event-detail-date-time-label">Attendance Status</div>
                <div className="event-card-progress-section" style={{ borderTop: 'none', paddingTop: '8px' }}>
                  <div className="event-card-amounts">
                    <span>{attendanceList.length} Checked in</span>
                  </div>
                  <div className="event-card-bar-track">
                    <div className="event-card-bar-fill" style={{ width: `${getProgressPercent(attendanceList.length, registrationsList.length || viewingEvent.capacity || 100)}%` }} />
                  </div>
                </div>
                <div className="event-card-actions">
                  <button className="event-action-btn" onClick={() => { setShowEventDetailPanel(false); setViewMode('attendance'); }}>
                    View Attendance ({attendanceList.length})
                  </button>
                </div>
              </div>

            </div>

            <div className="event-panel-footer">
              <button className="event-action-btn" onClick={handleOpenEdit}>
                Edit Event Details
              </button>
              <button
                className="event-action-btn"
                style={{ color: '#ef4444', borderColor: '#fca5a5' }}
                onClick={() => handleDeleteEvent(viewingEvent)}
              >
                Delete Event
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
              <button
                className="event-action-btn"
                onClick={() => { setShowCreatePanel(false); setShowEditPanel(false); }}
                disabled={createEventMutation.isPending || updateEventMutation.isPending}
              >
                Cancel
              </button>
              <button
                className="event-action-btn primary"
                onClick={showCreatePanel ? handleSaveCreate : handleSaveEdit}
                disabled={createEventMutation.isPending || updateEventMutation.isPending}
              >
                {createEventMutation.isPending || updateEventMutation.isPending ? 'Saving...' : 'Save Changes'}
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
              <h2 className="event-panel-title">Add Event Registration</h2>
              <button className="event-panel-close" onClick={() => setShowRegForm(false)}>
                <CloseIcon />
              </button>
            </div>
            <div className="event-panel-body">
              <div className="event-form-group">
                <label className="event-form-label">Member *</label>
                <select
                  className="event-form-select"
                  value={regForm.memberId}
                  onChange={e => setRegForm({ ...regForm, memberId: e.target.value })}
                >
                  <option value="">Select Member</option>
                  {membersList.map(member => (
                    <option key={member.id} value={member.id}>{member.firstName} {member.lastName}</option>
                  ))}
                </select>
              </div>
              <div className="event-form-group">
                <label className="event-form-label">Registration Date</label>
                <input
                  type="date"
                  className="event-form-input"
                  value={regForm.registrationDate}
                  onChange={e => setRegForm({ ...regForm, registrationDate: e.target.value })}
                />
              </div>
            </div>
            <div className="event-panel-footer">
              <button
                className="event-action-btn"
                onClick={() => setShowRegForm(false)}
                disabled={createRegistrationMutation.isPending}
              >
                Cancel
              </button>
              <button
                className="event-action-btn primary"
                onClick={handleSaveRegistration}
                disabled={createRegistrationMutation.isPending}
              >
                {createRegistrationMutation.isPending ? 'Saving...' : 'Save Registration'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ─── SIDE PANEL: ATTENDANCE FORM ────────────────────────────────────── */}
      {showAttForm && (
        <div className="event-panel-overlay" onClick={() => setShowAttForm(false)}>
          <div className="event-side-panel" onClick={e => e.stopPropagation()}>
            <div className="event-panel-header">
              <h2 className="event-panel-title">Check-in Attendee</h2>
              <button className="event-panel-close" onClick={() => setShowAttForm(false)}>
                <CloseIcon />
              </button>
            </div>
            <div className="event-panel-body">
              <div className="event-form-group">
                <label className="event-form-label">Member *</label>
                <select
                  className="event-form-select"
                  value={attForm.memberId}
                  onChange={e => setAttForm({ ...attForm, memberId: e.target.value })}
                >
                  <option value="">Select Member</option>
                  {membersList.map(member => (
                    <option key={member.id} value={member.id}>{member.firstName} {member.lastName}</option>
                  ))}
                </select>
              </div>
              <div className="event-form-group">
                <label className="event-form-label">Check-in Time</label>
                <input
                  type="time"
                  className="event-form-input"
                  value={attForm.checkInTime}
                  onChange={e => setAttForm({ ...attForm, checkInTime: e.target.value })}
                />
              </div>
            </div>
            <div className="event-panel-footer">
              <button
                className="event-action-btn"
                onClick={() => setShowAttForm(false)}
                disabled={createAttendanceMutation.isPending}
              >
                Cancel
              </button>
              <button
                className="event-action-btn primary"
                onClick={handleSaveAttendance}
                disabled={createAttendanceMutation.isPending}
              >
                {createAttendanceMutation.isPending ? 'Saving...' : 'Record Check-in'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ─── EXPORT PANEL ───────────────────────────────────────────────────── */}
      {showExportModal && viewingEvent && (
        <ExportPanel
          title={`Export ${viewMode === 'registrations' ? 'Registrations' : 'Attendance'}`}
          exportConfig={{
            title: `${viewingEvent.name} - ${viewMode === 'registrations' ? 'Registrations' : 'Attendance'} Report`,
            filename: `echo-event-${viewMode}`,
            columns: [
              { key: 'memberName', label: 'Member' },
              ...(viewMode === 'registrations'
                ? [{ key: 'registrationDate', label: 'Registration Date' }]
                : [{ key: 'checkInTime', label: 'Check-in Time' }]
              ),
            ],
            rows: (viewMode === 'registrations' ? registrationsList : attendanceList) as unknown as Record<string, unknown>[],
          }}
          onClose={() => setShowExportModal(false)}
        />
      )}

      {/* ─── DELETE CONFIRMATION MODAL ──────────────────────────────────────── */}
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

