import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useLayout } from '../hooks/useLayout';
import {
  useAttendance,
  useAttendanceTypes,
  useAttendanceContexts,
  useCreateAttendance,
  useUpdateAttendance,
  useDeleteAttendance,
  useCreateAttendanceContext,
  useCreateAttendanceType,
} from '../hooks/useAttendance';
import { useMembers } from '../hooks/useMembers';
import '../styles/Attendance.css';
import type { AttendanceRecord, AttendeeType } from '../types/attendance';
import type { Member } from '../types/member';
import {
  CalendarIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ClockIcon,
  CloseIcon,
  FilterIcon,
  SearchIcon,
} from './Icons';
import DeleteConfirmModal from './common/DeleteConfirmModal';

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

const getTodayDate = (): string => {
  return new Date().toISOString().split('T')[0];
};

interface FormState {
  memberId: string;
  attendanceContextId: number | '';
  status: 'Present' | 'Absent';
  roleOverride: AttendeeType;
  timeRecorded: string;
  notes: string;
}

const Attendance: React.FC = () => {
  const { setTitle, setCtas, searchQuery, setSearchQuery } = useLayout();

  // ── Queries & Mutations ─────────────────────────────────────────────────────
  const { data: attendanceTypes = [] } = useAttendanceTypes();
  const { data: attendanceContexts = [] } = useAttendanceContexts();
  const { data: membersResponse } = useMembers({}, 100);
  const members = useMemo(() => membersResponse?.data || [], [membersResponse?.data]);

  const [date, setDate] = useState<string>(getTodayDate);
  const [selectedTypeId, setSelectedTypeId] = useState<number | ''>('');
  const [selectedContextId, setSelectedContextId] = useState<number | ''>('');

  const effectiveTypeId = selectedTypeId === '' ? attendanceTypes[0]?.id ?? '' : selectedTypeId;

  // Contexts matching selected type
  const availableContexts = useMemo(() => {
    if (!effectiveTypeId) return attendanceContexts;
    const selectedType = attendanceTypes.find((t) => t.id === effectiveTypeId);
    return attendanceContexts.filter(
      (c) => c.attendanceTypeId === effectiveTypeId || (selectedType && c.attendanceTypeName === selectedType.name)
    );
  }, [attendanceContexts, attendanceTypes, effectiveTypeId]);

  const effectiveContextId =
    selectedContextId !== '' && availableContexts.some((c) => c.id === selectedContextId)
      ? selectedContextId
      : (availableContexts[0]?.id ?? '');

  // Attendance Records Query
  const filters = useMemo(() => ({
    forDate: date,
    attendanceContextId: typeof effectiveContextId === 'number' ? effectiveContextId : undefined,
  }), [date, effectiveContextId]);

  const { data: attendanceData, isLoading: isLoadingAttendance } = useAttendance(filters);
  const attendees = useMemo(() => attendanceData?.data || [], [attendanceData?.data]);

  const createAttendance = useCreateAttendance();
  const updateAttendance = useUpdateAttendance();
  const deleteAttendance = useDeleteAttendance();
  const createContextMutation = useCreateAttendanceContext();
  const createTypeMutation = useCreateAttendanceType();

  // ── UI States ───────────────────────────────────────────────────────────────
  const [selectedTab, setSelectedTab] = useState<'All' | 'Members' | 'Visitors'>('All');
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingAttendance, setDeletingAttendance] = useState<AttendanceRecord | null>(null);

  // Modal / Side Panel State
  const [showMarkPanel, setShowMarkPanel] = useState(false);
  const [editingRecord, setEditingRecord] = useState<AttendanceRecord | null>(null);
  const [formData, setFormData] = useState<FormState>({
    memberId: '',
    attendanceContextId: '',
    status: 'Present',
    roleOverride: 'Member',
    timeRecorded: '12:00',
    notes: '',
  });

  const [memberSearchQuery, setMemberSearchQuery] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);
  const suggestionsRef = useRef<HTMLDivElement>(null);

  // New Context Modal State
  const [showNewContextModal, setShowNewContextModal] = useState(false);
  const [newContextName, setNewContextName] = useState('');
  const [newContextTypeId, setNewContextTypeId] = useState<number | ''>('');

  // ── Date Navigation Handlers ────────────────────────────────────────────────
  const handlePrevDay = () => setDate((d) => addDays(d, -1));
  const handleNextDay = () => setDate((d) => addDays(d, 1));
  const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.value) {
      setDate(e.target.value);
    }
  };

  // ── Mark Attendance Handlers ────────────────────────────────────────────────
  const handleOpenMarkPanel = useCallback((record?: AttendanceRecord) => {
    if (record) {
      setEditingRecord(record);
      setFormData({
        memberId: record.memberId,
        attendanceContextId: record.attendanceContextId,
        status: 'Present',
        roleOverride: record.attendeeType,
        timeRecorded: record.checkInTime.slice(0, 5),
        notes: record.description || '',
      });
      setMemberSearchQuery(record.memberName || '');
    } else {
      setEditingRecord(null);
      const now = new Date();
      const currentHours = String(now.getHours()).padStart(2, '0');
      const currentMinutes = String(now.getMinutes()).padStart(2, '0');

      setFormData({
        memberId: '',
        attendanceContextId: effectiveContextId,
        status: 'Present',
        roleOverride: 'Member',
        timeRecorded: `${currentHours}:${currentMinutes}`,
        notes: '',
      });
      setMemberSearchQuery('');
    }
    setShowSuggestions(false);
    setShowMarkPanel(true);
  }, [effectiveContextId]);

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

    const contextId = formData.attendanceContextId || effectiveContextId;
    if (!contextId) {
      alert('Please select or create an attendance context first.');
      return;
    }

    try {
      if (formData.status === 'Absent' && editingRecord) {
        await deleteAttendance.mutateAsync(editingRecord.id);
      } else if (editingRecord) {
        await updateAttendance.mutateAsync({
          id: editingRecord.id,
          data: {
            memberId: formData.memberId,
            attendanceContextId: Number(contextId),
            attendeeType: formData.roleOverride,
            forDate: date,
            checkInTime: formData.timeRecorded ? `${formData.timeRecorded}:00` : '12:00:00',
            description: formData.notes.trim() || undefined,
          },
        });
      } else {
        await createAttendance.mutateAsync({
          memberId: formData.memberId,
          attendanceContextId: Number(contextId),
          attendeeType: formData.roleOverride,
          forDate: date,
          checkInTime: formData.timeRecorded ? `${formData.timeRecorded}:00` : '12:00:00',
          description: formData.notes.trim() || undefined,
        });
      }

      handleCloseMarkPanel();
    } catch (err) {
      console.error('Failed to save attendance record:', err);
      alert('Failed to save attendance record.');
    }
  };

  const handleDeleteAttendance = (att: AttendanceRecord) => {
    setDeletingAttendance(att);
    setShowDeleteConfirm(true);
  };

  const confirmDeleteAttendance = async () => {
    if (!deletingAttendance) return;
    try {
      await deleteAttendance.mutateAsync(deletingAttendance.id);
      setShowDeleteConfirm(false);
      setDeletingAttendance(null);
    } catch (err) {
      console.error('Failed to delete attendance record:', err);
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

  // Autocomplete suggest selection
  const handleSelectMemberSuggestion = (member: Member) => {
    const displayName = member.name || `${member.firstName} ${member.lastName}`;
    setFormData((prev) => ({
      ...prev,
      memberId: member.id,
      roleOverride: member.status === 'Visitor' ? 'Visitor' : 'Member',
    }));
    setMemberSearchQuery(displayName);
    setShowSuggestions(false);
  };

  // ── Create New Context Modal ────────────────────────────────────────────────
  const handleOpenNewContext = () => {
    setNewContextName('');
    setNewContextTypeId(effectiveTypeId);
    setShowNewContextModal(true);
  };

  const handleCreateContext = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newContextName.trim()) return;

    let typeId = newContextTypeId;
    // If no type exists, create a default 'Service' type first
    if (!typeId) {
      try {
        const createdType = await createTypeMutation.mutateAsync({ name: 'Service' });
        typeId = createdType.id;
        setSelectedTypeId(createdType.id);
      } catch (err) {
        console.error('Failed to create default attendance type:', err);
        return;
      }
    }

    try {
      const created = await createContextMutation.mutateAsync({
        name: newContextName.trim(),
        attendanceTypeId: Number(typeId),
      });
      setSelectedContextId(created.id);
      setShowNewContextModal(false);
    } catch (err) {
      console.error('Failed to create attendance context:', err);
      alert('Failed to create attendance context.');
    }
  };

  // ── Calculations & Filtering ───────────────────────────────────────────────
  const totalPresent = attendees.length;
  const membersPresent = attendees.filter((a) => a.attendeeType === 'Member').length;
  const firstTimeVisitors = attendees.filter(
    (a) => a.attendeeType === 'Visitor' || a.attendeeType === 'Guest'
  ).length;
  const childrenPresent = attendees.filter((a) => a.attendeeType === 'Child').length;

  // Filter attendees list for table display
  const displayAttendees = useMemo(() => {
    return attendees
      .filter((a) => {
        if (selectedTab === 'Members' && a.attendeeType !== 'Member') return false;
        if (selectedTab === 'Visitors' && a.attendeeType === 'Member') return false;

        if (searchQuery.trim() !== '') {
          return a.memberName?.toLowerCase().includes(searchQuery.toLowerCase());
        }
        return true;
      });
  }, [attendees, selectedTab, searchQuery]);

  // Filter suggestions list for auto-suggest
  const activeSuggestions = useMemo(() => {
    const query = memberSearchQuery.toLowerCase().trim();
    if (!query) return members.slice(0, 10);

    return members
      .filter((m) => {
        const fullName = (m.name || `${m.firstName} ${m.lastName}`).toLowerCase();
        const isAlreadyPresent = attendees.some(
          (a) => a.memberId === m.id && (!editingRecord || editingRecord.memberId !== m.id)
        );
        return fullName.includes(query) && !isAlreadyPresent;
      })
      .slice(0, 10);
  }, [members, memberSearchQuery, attendees, editingRecord]);

  const currentContextName = useMemo(() => {
    const ctx = attendanceContexts.find((c) => c.id === effectiveContextId);
    return ctx?.name || 'General Service';
  }, [attendanceContexts, effectiveContextId]);

  return (
    <div className="attendance-container">
      {/* ─── Filters & Selectors ────────────────────────────────────────────── */}
      <div className="attendance-filters-row">
        {/* SELECT DATE */}
        <div className="filter-group">
          <span className="filter-label">Select Date</span>
          <div className="date-picker-control">
            <button type="button" className="date-nav-btn" onClick={handlePrevDay} aria-label="Previous day">
              <ChevronLeftIcon size={16} />
            </button>
            <span className="date-display">{formatDateDisplay(date)}</span>
            <button type="button" className="date-nav-btn" onClick={handleNextDay} aria-label="Next day">
              <ChevronRightIcon size={16} />
            </button>
            <label className="date-calendar-btn" aria-label="Choose date">
              <CalendarIcon size={16} />
              <input
                type="date"
                className="hidden-date-input"
                value={date}
                onChange={handleDateChange}
              />
            </label>
          </div>
        </div>

        {/* TYPE */}
        <div className="filter-group">
          <span className="filter-label">Type</span>
          <select
            className="attendance-select"
            value={effectiveTypeId}
            onChange={(e) => {
              const val = e.target.value ? Number(e.target.value) : '';
              setSelectedTypeId(val);
            }}
          >
            {attendanceTypes.length === 0 ? (
              <option value="">Service</option>
            ) : (
              attendanceTypes.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))
            )}
          </select>
        </div>

        {/* ATTENDANCE CONTEXT */}
        <div className="filter-group">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span className="filter-label">Attendance Context</span>
            <button
              type="button"
              onClick={handleOpenNewContext}
              style={{
                background: 'none',
                border: 'none',
                color: 'var(--primary, #007aff)',
                fontSize: '11px',
                fontWeight: 600,
                cursor: 'pointer',
                padding: '0 4px',
              }}
            >
              + New Context
            </button>
          </div>
          <select
            className="attendance-select"
            value={effectiveContextId}
            onChange={(e) => {
              const val = e.target.value ? Number(e.target.value) : '';
              setSelectedContextId(val);
            }}
          >
            {availableContexts.length === 0 ? (
              <option value="">No contexts available</option>
            ) : (
              availableContexts.map((opt) => (
                <option key={opt.id} value={opt.id}>
                  {opt.name}
                </option>
              ))
            )}
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
                  type="button"
                  className={`tab-btn ${selectedTab === tabId ? 'active' : ''}`}
                  onClick={() => setSelectedTab(tabId)}
                >
                  {tab}
                </button>
              );
            })}
          </div>

          <button type="button" className="table-filter-btn">
            <FilterIcon size={14} /> Filter
          </button>
        </div>

        <div className="table-container">
          {isLoadingAttendance ? (
            <div className="table-empty">Loading attendance records...</div>
          ) : displayAttendees.length === 0 ? (
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
                  let checkInDisplay = att.checkInTime;
                  try {
                    const timeParts = att.checkInTime.split(':');
                    const hr = parseInt(timeParts[0], 10);
                    const min = timeParts[1];
                    const suffix = hr >= 12 ? 'PM' : 'AM';
                    const displayHr = hr % 12 === 0 ? 12 : hr % 12;
                    checkInDisplay = `${displayHr}:${min} ${suffix}`;
                  } catch {
                    // fall back to raw string
                  }

                  return (
                    <tr key={att.id}>
                      <td className="attendee-name">{att.memberName}</td>
                      <td>
                        <span className={`type-badge ${(att.attendeeType || 'member').toLowerCase()}`}>
                          {att.attendeeType}
                        </span>
                      </td>
                      <td>{checkInDisplay}</td>
                      <td>
                        <div className="table-actions">
                          <button
                            type="button"
                            className="action-btn"
                            onClick={() => handleOpenMarkPanel(att)}
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            className="action-btn delete"
                            onClick={() => handleDeleteAttendance(att)}
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
                aria-label="Close panel"
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
                  <span className="context-title">{currentContextName}</span>
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
                      if (formData.memberId) {
                        setFormData((prev) => ({ ...prev, memberId: '' }));
                      }
                    }}
                    onFocus={() => setShowSuggestions(true)}
                    disabled={!!editingRecord}
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
                        const displayName = m.name || `${m.firstName} ${m.lastName}`;
                        return (
                          <div
                            key={m.id}
                            className="autocomplete-item"
                            onClick={() => handleSelectMemberSuggestion(m)}
                          >
                            <span>{displayName}</span>
                            <span className="autocomplete-item-role">{m.status || 'Member'}</span>
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
                <label className="panel-label">Role</label>
                <div className="radio-group">
                  {(['Member', 'Visitor', 'Guest', 'Child'] as const).map((opt) => (
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
              <button
                type="submit"
                className="panel-btn panel-btn-save"
                disabled={createAttendance.isPending || updateAttendance.isPending || deleteAttendance.isPending}
              >
                {createAttendance.isPending || updateAttendance.isPending || deleteAttendance.isPending
                  ? 'Saving...'
                  : 'Save Changes'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* ─── New Attendance Context Modal ────────────────────────────────────── */}
      {showNewContextModal && (
        <div
          className="mark-panel-overlay"
          onClick={() => setShowNewContextModal(false)}
          style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
        >
          <div
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{
              background: 'white',
              borderRadius: '16px',
              padding: '24px',
              width: '100%',
              maxWidth: '440px',
              boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
            }}
          >
            <h3 style={{ fontSize: '18px', fontWeight: 600, marginBottom: '16px', color: 'var(--text-main)' }}>
              Create Attendance Context
            </h3>
            <form onSubmit={handleCreateContext}>
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', fontSize: '13px', fontWeight: 500, marginBottom: '6px' }}>
                  Context Name
                </label>
                <input
                  type="text"
                  className="panel-input"
                  placeholder="e.g. Sunday Morning Service, Midweek Service"
                  value={newContextName}
                  onChange={(e) => setNewContextName(e.target.value)}
                  required
                  autoFocus
                />
              </div>

              <div style={{ marginBottom: '24px' }}>
                <label style={{ display: 'block', fontSize: '13px', fontWeight: 500, marginBottom: '6px' }}>
                  Attendance Type
                </label>
                <select
                  className="attendance-select"
                  style={{ width: '100%' }}
                  value={newContextTypeId}
                  onChange={(e) => setNewContextTypeId(Number(e.target.value))}
                >
                  {attendanceTypes.length === 0 ? (
                    <option value="">Default Service</option>
                  ) : (
                    attendanceTypes.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.name}
                      </option>
                    ))
                  )}
                </select>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
                <button
                  type="button"
                  className="panel-btn panel-btn-cancel"
                  onClick={() => setShowNewContextModal(false)}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="panel-btn panel-btn-save"
                  disabled={createContextMutation.isPending}
                >
                  {createContextMutation.isPending ? 'Creating...' : 'Create Context'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      <DeleteConfirmModal
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={confirmDeleteAttendance}
        itemName={deletingAttendance?.memberName || 'Record'}
        title="Delete Attendance Record"
        confirmText="Delete Record"
      />
    </div>
  );
};

export default Attendance;
