import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useLayout } from '../context/LayoutContext';
import {
    deleteAttendanceRecord,
    getAttendanceRecords,
    getMembers,
    isChild,
    saveAttendanceRecord,
} from '../services/attendanceService';
import '../styles/Attendance.css';
import type { AttendanceRecord, ChurchServiceType, MarkAttendanceForm, Member } from '../types/attendance';
import {
    CalendarIcon,
    ChevronLeftIcon,
    ChevronRightIcon,
    ClockIcon,
    CloseIcon,
    FilterIcon,
    SearchIcon,
} from './Icons';

// ─── Helpers ─────────────────────────────────────────────────────────────────

const formatDateDisplay = (dateStr: string): string => {
  const parts = dateStr.split('-');
  if (parts.length !== 3) return dateStr;
  return `${parts[2]} / ${parts[1]} / ${parts[0].slice(2)}`;
};

const formatDateVerbose = (dateStr: string): string => {
  const dateObj = new Date(dateStr);
  if (isNaN(dateObj.getTime())) return dateStr;
  return dateObj.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
};

const addDays = (dateStr: string, days: number): string => {
  const d = new Date(dateStr);
  d.setDate(d.getDate() + days);
  return d.toISOString().split('T')[0];
};

const getContextsForType = (type: string) => {
  if (type === 'Practice') {
    return [{ value: 'ChoirPractice' as ChurchServiceType, label: 'Choir Practice' }];
  }
  if (type === 'Meeting') {
    return [{ value: 'General' as ChurchServiceType, label: 'Committee Meeting' }];
  }
  return [
    { value: 'Evening' as ChurchServiceType, label: 'Evening Service' },
    { value: 'General' as ChurchServiceType, label: 'General Service' },
  ];
};

const Attendance: React.FC = () => {
  const { setTitle, setCtas, searchQuery, setSearchQuery } = useLayout();

  // ── States ─────────────────────────────────────────────────────────────────

  const [date, setDate] = useState('2026-10-23'); // Initialize to seeded date matching wireframe
  const [eventType, setEventType] = useState<'Service' | 'Practice' | 'Meeting'>('Service');
  const [serviceContext, setServiceContext] = useState<ChurchServiceType>('Evening');

  const [members, setMembers] = useState<Member[]>([]);
  const [attendees, setAttendees] = useState<AttendanceRecord[]>([]);

  const [selectedTab, setSelectedTab] = useState<'All' | 'Members' | 'Visitors'>('All');

  // Modal / Side Panel State
  const [showMarkPanel, setShowMarkPanel] = useState(false);
  const [editingRecord, setEditingRecord] = useState<AttendanceRecord | null>(null);

  const [formData, setFormData] = useState<MarkAttendanceForm>({
    memberId: '',
    status: 'Present',
    roleOverride: 'Member',
    timeRecorded: '15:00',
    notes: '',
  });

  const [memberSearchQuery, setMemberSearchQuery] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);
  const suggestionsRef = useRef<HTMLDivElement>(null);

  // ── Data Fetching ──────────────────────────────────────────────────────────

  const fetchData = useCallback(async () => {
    try {
      const allMembers = await getMembers();
      setMembers(allMembers);
      const records = await getAttendanceRecords(date, serviceContext);
      setAttendees(records);
    } catch (err) {
      console.error('Failed to load data:', err);
    }
  }, [date, serviceContext]);

  // Adjust serviceContext when eventType changes
  const handleEventTypeChange = (newType: 'Service' | 'Practice' | 'Meeting') => {
    setEventType(newType);
    const available = getContextsForType(newType);
    if (available.length > 0) {
      setServiceContext(available[0].value);
    }
  };

  // Date Navigation handlers
  const handlePrevDay = () => setDate((d) => addDays(d, -1));
  const handleNextDay = () => setDate((d) => addDays(d, 1));
  const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.value) {
      setDate(e.target.value);
    }
  };

  // ── Handlers ───────────────────────────────────────────────────────────────

  const handleOpenMarkPanel = useCallback((record?: AttendanceRecord) => {
    if (record) {
      // Edit mode
      setEditingRecord(record);
      const member = members.find((m) => m.memberId === record.memberId);
      setFormData({
        memberId: String(record.memberId),
        status: 'Present', // backend AttendanceRecord tracks only present attendees
        roleOverride: record.attendeeType,
        timeRecorded: record.checkInTime,
        notes: record.description || '',
      });
      setMemberSearchQuery(member ? `${member.firstName} ${member.lastName}` : '');
    } else {
      // Create mode
      setEditingRecord(null);

      // Get current local time in HH:mm
      const now = new Date();
      const currentHours = String(now.getHours()).padStart(2, '0');
      const currentMinutes = String(now.getMinutes()).padStart(2, '0');

      setFormData({
        memberId: '',
        status: 'Present',
        roleOverride: 'Member',
        timeRecorded: `${currentHours}:${currentMinutes}`,
        notes: '',
      });
      setMemberSearchQuery('');
    }
    setShowSuggestions(false);
    setShowMarkPanel(true);
  }, [members]);

  const handleCloseMarkPanel = useCallback(() => {
    setShowMarkPanel(false);
    setEditingRecord(null);
  }, []);

  const handleSaveAttendance = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.memberId) {
      alert('Please select an attendee first.');
      return;
    }

    try {
      if (formData.status === 'Absent' && editingRecord) {
        // If status marked as absent, remove the record
        await deleteAttendanceRecord(editingRecord.attendanceId);
      } else if (formData.status === 'Present') {
        await saveAttendanceRecord({
          attendanceId: editingRecord?.attendanceId,
          memberId: parseInt(formData.memberId),
          forDate: date,
          churchServiceType: serviceContext,
          attendeeType: formData.roleOverride,
          checkInTime: formData.timeRecorded,
          description: formData.notes,
        });
      }

      handleCloseMarkPanel();
      fetchData(); // Refresh list and stats
    } catch (err) {
      console.error('Failed to save attendance record:', err);
    }
  };

  // ── Layout Header & Effects ────────────────────────────────────────────────

  useEffect(() => {
    setTitle('Attendance');
    setCtas([
      {
        type: 'search',
        placeholder: 'Search Attendees...',
      },
      {
        type: 'button',
        label: 'Record Attendance',
        icon: 'plus',
        variant: 'primary',
        onClick: () => handleOpenMarkPanel(),
      },
    ]);

    // Clear search on mount
    setSearchQuery('');
  }, [setTitle, setCtas, setSearchQuery, handleOpenMarkPanel]);

  // Close auto-suggest on outside click
  useEffect(() => {
    const handleOutsideClick = (e: MouseEvent) => {
      if (suggestionsRef.current && !suggestionsRef.current.contains(e.target as Node)) {
        setShowSuggestions(false);
      }
    };
    document.addEventListener('mousedown', handleOutsideClick);
    return () => document.removeEventListener('mousedown', handleOutsideClick);
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchData();
    }, 0);
    return () => clearTimeout(timer);
  }, [fetchData]);

  const handleDeleteAttendance = async (attendanceId: number) => {
    if (window.confirm('Are you sure you want to delete this attendance record?')) {
      try {
        await deleteAttendanceRecord(attendanceId);
        fetchData();
      } catch (err) {
        console.error('Failed to delete attendance record:', err);
      }
    }
  };

  // Autocomplete suggest selection
  const handleSelectMemberSuggestion = (member: Member) => {
    setFormData((prev) => ({
      ...prev,
      memberId: String(member.memberId),
      // Set the default role based on their activity status/profile
      roleOverride: member.firstName === 'John' && member.lastName === 'Doe' ? 'Guest' :
                    member.firstName === 'Baba' && member.lastName === 'Tundey' ? 'Visitor' : 'Member',
    }));
    setMemberSearchQuery(`${member.firstName} ${member.lastName}`);
    setShowSuggestions(false);
  };

  // ── Calculations & Filtering ───────────────────────────────────────────────

  // Stats computation
  const totalPresent = attendees.length;
  const membersPresent = attendees.filter((a) => a.attendeeType === 'Member').length;
  const firstTimeVisitors = attendees.filter((a) => a.attendeeType === 'Visitor').length;
  const childrenPresent = attendees.filter((a) => {
    const mem = members.find((m) => m.memberId === a.memberId);
    return mem ? isChild(mem, date) : false;
  }).length;

  // Filter attendees list for table display
  const displayAttendees = attendees
    .map((record) => {
      const member = members.find((m) => m.memberId === record.memberId);
      return {
        ...record,
        memberName: member ? `${member.firstName} ${member.lastName}` : `Unknown Member #${record.memberId}`,
      };
    })
    .filter((a) => {
      // Tab selection filter
      if (selectedTab === 'Members' && a.attendeeType !== 'Member') return false;
      if (selectedTab === 'Visitors' && a.attendeeType === 'Member') return false;

      // Header search bar filter
      if (searchQuery.trim() !== '') {
        return a.memberName.toLowerCase().includes(searchQuery.toLowerCase());
      }
      return true;
    });

  // Filter suggestions list for auto-suggest
  const activeSuggestions = members.filter((m) => {
    const fullName = `${m.firstName} ${m.lastName}`.toLowerCase();
    const query = memberSearchQuery.toLowerCase();

    // Don't show members already present, unless we are editing their record
    const isAlreadyPresent = attendees.some(
      (a) => a.memberId === m.memberId && (!editingRecord || editingRecord.memberId !== m.memberId)
    );

    return fullName.includes(query) && !isAlreadyPresent;
  });

  return (
    <div className="attendance-container">
      {/* ─── Filters & Selectors ────────────────────────────────────────────── */}
      <div className="attendance-filters-row">
        {/* SELECT DATE */}
        <div className="filter-group">
          <span className="filter-label">Select Date</span>
          <div className="date-picker-control">
            <button className="date-nav-btn" onClick={handlePrevDay}>
              <ChevronLeftIcon size={16} />
            </button>
            <span className="date-display">{formatDateDisplay(date)}</span>
            <button className="date-nav-btn" onClick={handleNextDay}>
              <ChevronRightIcon size={16} />
            </button>
            <button className="date-calendar-btn">
              <CalendarIcon size={16} />
              <input
                type="date"
                className="hidden-date-input"
                value={date}
                onChange={handleDateChange}
              />
            </button>
          </div>
        </div>

        {/* TYPE */}
        <div className="filter-group">
          <span className="filter-label">Type</span>
          <select
            className="attendance-select"
            value={eventType}
            onChange={(e) => handleEventTypeChange(e.target.value as 'Service' | 'Practice' | 'Meeting')}
          >
            <option value="Service">Service</option>
            <option value="Practice">Practice</option>
            <option value="Meeting">Meeting</option>
          </select>
        </div>

        {/* ATTENDANCE CONTEXT */}
        <div className="filter-group">
          <span className="filter-label">Attendance Context</span>
          <select
            className="attendance-select"
            value={serviceContext}
            onChange={(e) => setServiceContext(e.target.value as ChurchServiceType)}
          >
            {getContextsForType(eventType).map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* ─── Stats Cards ────────────────────────────────────────────────────── */}
      <div className="attendance-stats-row">
        <div className="attendance-stat-card">
          <span className="attendance-stat-label">Total Present</span>
          <div className="attendance-stat-value">{totalPresent}</div>
        </div>

        <div className="attendance-stat-card">
          <span className="attendance-stat-label">First-time Visitors</span>
          <div className="attendance-stat-value">{firstTimeVisitors}</div>
        </div>

        <div className="attendance-stat-card">
          <span className="attendance-stat-label">Members Present</span>
          <div className="attendance-stat-value">{membersPresent}</div>
        </div>

        <div className="attendance-stat-card">
          <span className="attendance-stat-label">Children</span>
          <div className="attendance-stat-value">{childrenPresent}</div>
        </div>
      </div>

      {/* ─── Attendees Table Card ───────────────────────────────────────────── */}
      <div className="attendees-card">
        <div className="card-tabs-row">
          <div className="tabs-group">
            {(['All Attendees', 'Members', 'Visitors'] as const).map((tab) => {
              const tabId = tab === 'All Attendees' ? 'All' : tab;
              return (
                <button
                  key={tab}
                  className={`tab-btn ${selectedTab === tabId ? 'active' : ''}`}
                  onClick={() => setSelectedTab(tabId)}
                >
                  {tab}
                </button>
              );
            })}
          </div>

          <button className="table-filter-btn">
            <FilterIcon size={14} /> Filter
          </button>
        </div>

        <div className="table-container">
          {displayAttendees.length === 0 ? (
            <div className="table-empty">No attendees recorded for this selection.</div>
          ) : (
            <table className="attendees-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Type</th>
                  <th>Check-In Time</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {displayAttendees.map((att) => {
                  // Format time (e.g. 15:00 -> 3:00 PM)
                  let checkInDisplay = att.checkInTime;
                  try {
                    const timeParts = att.checkInTime.split(':');
                    const hr = parseInt(timeParts[0]);
                    const min = timeParts[1];
                    const suffix = hr >= 12 ? 'PM' : 'AM';
                    const displayHr = hr % 12 === 0 ? 12 : hr % 12;
                    checkInDisplay = `${displayHr}:${min} ${suffix}`;
                  } catch {
                    // fall back
                  }

                  return (
                    <tr key={att.attendanceId}>
                      <td className="attendee-name">{att.memberName}</td>
                      <td>
                        <span className={`type-badge ${att.attendeeType.toLowerCase()}`}>
                          {att.attendeeType}
                        </span>
                      </td>
                      <td>{checkInDisplay}</td>
                      <td>
                        <div className="table-actions">
                          <button className="action-btn" onClick={() => handleOpenMarkPanel(att)}>
                            Edit
                          </button>
                          <button
                            className="action-btn delete"
                            onClick={() => handleDeleteAttendance(att.attendanceId)}
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {/* ─── Mark Attendance Slide Modal ────────────────────────────────────── */}
      {showMarkPanel && (
        <div className="mark-panel-overlay" onClick={handleCloseMarkPanel}>
          <form
            className="mark-panel"
            onClick={(e) => e.stopPropagation()}
            onSubmit={handleSaveAttendance}
          >
            {/* Header */}
            <div className="panel-header">
              <h3 className="panel-title">
                {editingRecord ? 'Edit Attendance' : 'Mark Attendance'}
              </h3>
              <button
                type="button"
                className="panel-close-btn"
                onClick={handleCloseMarkPanel}
              >
                <CloseIcon size={20} />
              </button>
            </div>

            {/* Body */}
            <div className="panel-body">
              {/* Context display banner */}
              <div className="panel-context-card">
                <div className="context-icon-container">
                  <CalendarIcon size={20} />
                </div>
                <div className="context-info">
                  <span className="context-title">
                    {getContextsForType(eventType).find((c) => c.value === serviceContext)?.label || serviceContext}
                  </span>
                  <span className="context-date">{formatDateVerbose(date)}</span>
                </div>
              </div>

              {/* ATTENDEE NAME (auto-suggest input) */}
              <div className="panel-form-group" ref={suggestionsRef}>
                <label className="panel-label">Attendee Name</label>
                <div className="panel-search-wrapper">
                  <input
                    type="text"
                    className="panel-input"
                    placeholder="Search member or visitor..."
                    value={memberSearchQuery}
                    onChange={(e) => {
                      setMemberSearchQuery(e.target.value);
                      setShowSuggestions(true);
                      // Clear id if user edits the text
                      if (formData.memberId) {
                        setFormData((prev) => ({ ...prev, memberId: '' }));
                      }
                    }}
                    onFocus={() => setShowSuggestions(true)}
                    disabled={!!editingRecord} // Don't allow changing member on edit
                  />
                  <div className="panel-search-icon">
                    <SearchIcon size={16} />
                  </div>
                </div>

                {/* Suggestions List */}
                {showSuggestions && memberSearchQuery.trim() !== '' && (
                  <div className="autocomplete-dropdown">
                    {activeSuggestions.length === 0 ? (
                      <div className="autocomplete-item" style={{ color: 'var(--text-muted)', cursor: 'default' }}>
                        No matches found
                      </div>
                    ) : (
                      activeSuggestions.map((m) => {
                        // Guess role based on mock rules for tag visual representation
                        let roleLabel = 'Member';
                        if (m.firstName === 'John' && m.lastName === 'Doe') roleLabel = 'Guest';
                        if (m.firstName === 'Baba' && m.lastName === 'Tundey') roleLabel = 'Visitor';

                        return (
                          <div
                            key={m.memberId}
                            className="autocomplete-item"
                            onClick={() => handleSelectMemberSuggestion(m)}
                          >
                            <span>{m.firstName} {m.lastName}</span>
                            <span className="autocomplete-item-role">{roleLabel}</span>
                          </div>
                        );
                      })
                    )}
                  </div>
                )}
              </div>

              {/* STATUS */}
              <div className="panel-form-group">
                <label className="panel-label">Status</label>
                <div className="radio-group">
                  {(['Present', 'Absent'] as const).map((opt) => (
                    <label key={opt} className="radio-option">
                      <div
                        className={`radio-input-styled ${formData.status === opt ? 'checked' : ''}`}
                        onClick={() => setFormData((prev) => ({ ...prev, status: opt }))}
                      >
                        <div className="radio-inner-dot" />
                      </div>
                      {opt}
                    </label>
                  ))}
                </div>
              </div>

              {/* ROLE (OPTIONAL OVERRIDE) */}
              <div className="panel-form-group">
                <label className="panel-label">Role (Optional Override)</label>
                <div className="radio-group">
                  {(['Member', 'Visitor'] as const).map((opt) => (
                    <label key={opt} className="radio-option">
                      <div
                        className={`radio-input-styled ${formData.roleOverride === opt ? 'checked' : ''}`}
                        onClick={() => setFormData((prev) => ({ ...prev, roleOverride: opt }))}
                      >
                        <div className="radio-inner-dot" />
                      </div>
                      {opt}
                    </label>
                  ))}
                </div>
              </div>

              {/* TIME RECORDED */}
              <div className="panel-form-group">
                <label className="panel-label">Time Recorded</label>
                <div className="time-input-wrapper">
                  <input
                    type="time"
                    className="panel-input"
                    value={formData.timeRecorded}
                    onChange={(e) => setFormData((prev) => ({ ...prev, timeRecorded: e.target.value }))}
                  />
                  <div className="time-icon">
                    <ClockIcon size={16} />
                  </div>
                </div>
              </div>

              {/* NOTES */}
              <div className="panel-form-group">
                <label className="panel-label">Notes</label>
                <textarea
                  className="panel-textarea"
                  placeholder="Any relevant notes here..."
                  value={formData.notes}
                  onChange={(e) => setFormData((prev) => ({ ...prev, notes: e.target.value }))}
                />
              </div>
            </div>

            {/* Footer */}
            <div className="panel-footer">
              <button
                type="button"
                className="panel-btn panel-btn-cancel"
                onClick={handleCloseMarkPanel}
              >
                Cancel
              </button>
              <button type="submit" className="panel-btn panel-btn-save">
                Save Changes
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};

export default Attendance;
