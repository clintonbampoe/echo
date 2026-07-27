import React, { useEffect, useRef, useState } from 'react';
import { useLayout } from '../context/LayoutContext';
import '../styles/Projects.css';
import {
  CalendarIcon,
  CloseIcon,
  EditIcon,
  MembersIcon,
  TrashIcon,
  WarningIcon
} from './Icons';

// ─── Types ────────────────────────────────────────────────────────────────────

type ProjectStatus = 'On Track' | 'Planning' | 'Completed' | 'Off Track';

interface TeamMember {
  id: number;
  name: string;
  role: string;
  initials: string;
}

interface Project {
  id: number;
  name: string;
  category: string;
  lead: string;
  leadInitials: string;
  status: ProjectStatus;
  target: number;
  raised: number;
  startDate: string;
  deadline: string;
  description: string;
  team: TeamMember[];
}

type TabFilter = 'All Members' | 'Active' | 'Planning' | 'Completed';

// ─── Mock Data ────────────────────────────────────────────────────────────────

const mockProjects: Project[] = [
  {
    id: 1,
    name: 'Sanctuary Renovation',
    category: 'Building & Maintenance',
    lead: 'David Daniels',
    leadInitials: 'DD',
    status: 'On Track',
    target: 100000,
    raised: 35000,
    startDate: 'Feb 01, 2026',
    deadline: 'Nov 15, 2026',
    description:
      'Comprehensive renovation of the main sanctuary including new seating, upgraded acoustic panels, stage expansion, and fresh painting. This will increase our seating capacity by 35% and significantly improve the worship experience for all attendees.',
    team: [
      { id: 1, name: 'Sarah Martinez', role: 'Project Lead', initials: 'SM' },
      { id: 2, name: 'David Okonjo', role: 'Design Consultant', initials: 'DO' },
    ],
  },
  {
    id: 2,
    name: 'Community Outreach',
    category: 'Outreach & Missions',
    lead: 'John Doe',
    leadInitials: 'JD',
    status: 'Completed',
    target: 44000,
    raised: 44000,
    startDate: 'Jan 10, 2026',
    deadline: 'Apr 30, 2026',
    description:
      'A community outreach initiative aimed at distributing food packages, clothing, and essential items to over 500 families in the surrounding neighbourhoods.',
    team: [
      { id: 3, name: 'Abena Acheampong', role: 'Coordinator', initials: 'AA' },
      { id: 4, name: 'James Kwarteng', role: 'Volunteer Lead', initials: 'JK' },
    ],
  },
  {
    id: 3,
    name: 'Musical Instruments',
    category: 'Worship & Arts',
    lead: 'Sarah Jenkins',
    leadInitials: 'SJ',
    status: 'Off Track',
    target: 30000,
    raised: 10000,
    startDate: 'Mar 01, 2026',
    deadline: 'Aug 31, 2026',
    description:
      'Purchasing new musical instruments for the worship team: two electric guitars, a digital piano, a full drum kit, and upgraded microphones and in-ear monitors.',
    team: [
      { id: 5, name: 'Michael Asante', role: 'Music Director', initials: 'MA' },
    ],
  },
  {
    id: 4,
    name: 'Youth Camp',
    category: 'Youth Ministry',
    lead: 'John Doe',
    leadInitials: 'JD',
    status: 'Planning',
    target: 24400,
    raised: 0,
    startDate: 'Jun 01, 2026',
    deadline: 'Jul 15, 2026',
    description:
      'Annual youth summer camp for teenagers aged 13–19. The camp will feature Bible studies, outdoor activities, leadership workshops, and team-building events.',
    team: [
      { id: 6, name: 'Esi Mensah', role: 'Youth Pastor', initials: 'EM' },
      { id: 7, name: 'Kofi Antwi', role: 'Camp Counsellor', initials: 'KA' },
    ],
  },
  {
    id: 5,
    name: 'Livestream Upgrade',
    category: 'Technology & Media',
    lead: 'Michelle Obama',
    leadInitials: 'MO',
    status: 'Off Track',
    target: 100000,
    raised: 19000,
    startDate: 'Apr 01, 2026',
    deadline: 'Sep 30, 2026',
    description:
      'Upgrading the church livestream infrastructure with 4K cameras, professional lighting rigs, broadcast-grade audio mixers, and a high-speed dedicated internet connection.',
    team: [
      { id: 8, name: 'Nana Boateng', role: 'AV Technician', initials: 'NB' },
    ],
  },
  {
    id: 6,
    name: 'Choir Party',
    category: 'Worship & Arts',
    lead: 'Samuel Mensah',
    leadInitials: 'SM',
    status: 'Planning',
    target: 5000,
    raised: 800,
    startDate: 'Aug 01, 2026',
    deadline: 'Aug 31, 2026',
    description:
      'End-of-year celebration party for all choir members. Includes catering, awards, live music performances, and a slideshow of highlights from the year.',
    team: [
      { id: 9, name: 'Akosua Frimpong', role: 'Events Lead', initials: 'AF' },
    ],
  },
];

// ─── Categories for form ──────────────────────────────────────────────────────

const PROJECT_CATEGORIES = [
  'Building & Maintenance',
  'Outreach & Missions',
  'Worship & Arts',
  'Youth Ministry',
  'Technology & Media',
  'Administration',
  'Education',
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

const formatCurrency = (amount: number) =>
  `$ ${amount.toLocaleString('en-US')}`;

const getProgressPercent = (raised: number, target: number) =>
  target > 0 ? Math.min(Math.round((raised / target) * 100), 100) : 0;

const statusColor = (status: ProjectStatus): string => {
  switch (status) {
    case 'On Track':  return 'status-on-track';
    case 'Completed': return 'status-completed';
    case 'Planning':  return 'status-planning';
    case 'Off Track': return 'status-off-track';
  }
};

// ─── Empty form defaults ──────────────────────────────────────────────────────

const emptyForm = () => ({
  name: '',
  category: '',
  lead: '',
  target: '',
  deadline: '',
  status: 'Planning' as ProjectStatus,
  description: '',
});

// ─── Projects Component ───────────────────────────────────────────────────────

const Projects: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  // ── State ──────────────────────────────────────────────────────────────────
  const [projects, setProjects] = useState<Project[]>(mockProjects);
  const [activeTab, setActiveTab] = useState<TabFilter>('All Members');
  const [searchQuery, setSearchQuery] = useState('');

  // Panels
  const [viewingProject, setViewingProject] = useState<Project | null>(null);
  const [editingProject, setEditingProject] = useState<Project | null>(null);
  const [showCreatePanel, setShowCreatePanel] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingProject, setDeletingProject] = useState<Project | null>(null);

  // Create / Edit form
  const [form, setForm] = useState(emptyForm());

  const nextId = useRef(mockProjects.length + 1);

  // ── Layout ─────────────────────────────────────────────────────────────────

  useEffect(() => {
    setTitle('Projects');
    setCtas([
      {
        type: 'button',
        label: 'Filter',
        icon: 'filter',
        variant: 'secondary',
        onClick: () => {},
      },
      {
        type: 'button',
        label: 'Add Project',
        icon: 'plus',
        variant: 'primary',
        onClick: openCreatePanel,
      },
    ]);
  }, [setTitle, setCtas]);

  // ── Derived data ───────────────────────────────────────────────────────────

  const activeCount  = projects.filter(p => p.status === 'On Track').length;
  const totalRaised  = projects.reduce((s, p) => s + p.raised, 0);
  const totalTarget  = projects.reduce((s, p) => s + p.target, 0);
  const completedCount = projects.filter(p => p.status === 'Completed').length;

  const tabFiltered = projects.filter(p => {
    if (activeTab === 'All Members') return true;
    if (activeTab === 'Active')      return p.status === 'On Track';
    if (activeTab === 'Planning')    return p.status === 'Planning';
    if (activeTab === 'Completed')   return p.status === 'Completed';
    return true;
  });

  const filtered = tabFiltered.filter(p =>
    p.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    p.lead.toLowerCase().includes(searchQuery.toLowerCase()) ||
    p.category.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // ── Handlers ───────────────────────────────────────────────────────────────

  const openCreatePanel = () => {
    setForm(emptyForm());
    setShowCreatePanel(true);
  };

  const openEditPanel = (project: Project) => {
    setViewingProject(null);
    setEditingProject(project);
    setForm({
      name: project.name,
      category: project.category,
      lead: project.lead,
      target: String(project.target),
      deadline: project.deadline,
      status: project.status,
      description: project.description,
    });
  };

  const handleSaveCreate = () => {
    if (!form.name.trim()) return;
    const id = nextId.current++;
    const newProject: Project = {
      id,
      name: form.name,
      category: form.category || 'Administration',
      lead: form.lead || 'TBD',
      leadInitials: form.lead ? form.lead.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase() : 'TB',
      status: form.status,
      target: parseFloat(form.target.replace(/,/g, '')) || 0,
      raised: 0,
      startDate: new Date().toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' }),
      deadline: form.deadline || 'TBD',
      description: form.description,
      team: [],
    };
    setProjects(prev => [...prev, newProject]);
    setShowCreatePanel(false);
  };

  const handleSaveEdit = () => {
    if (!editingProject) return;
    setProjects(prev =>
      prev.map(p =>
        p.id === editingProject.id
          ? {
              ...p,
              name: form.name,
              category: form.category,
              lead: form.lead,
              leadInitials: form.lead.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase(),
              status: form.status,
              target: parseFloat(form.target.replace(/,/g, '')) || p.target,
              deadline: form.deadline,
              description: form.description,
            }
          : p
      )
    );
    setEditingProject(null);
  };

  const openDeleteConfirm = (project: Project) => {
    setViewingProject(null);
    setDeletingProject(project);
    setShowDeleteConfirm(true);
  };

  const confirmDelete = () => {
    if (!deletingProject) return;
    setProjects(prev => prev.filter(p => p.id !== deletingProject.id));
    setShowDeleteConfirm(false);
    setDeletingProject(null);
  };

  // ── Render ─────────────────────────────────────────────────────────────────

  const tabs: TabFilter[] = ['All Members', 'Active', 'Planning', 'Completed'];

  return (
    <div className="projects-container">

      {/* ─── Summary Cards ────────────────────────────────────────────────── */}
      <div className="projects-summary-cards">
        <div className="projects-summary-card">
          <span className="projects-card-label">Active Projects</span>
          <div className="projects-card-value">{activeCount}</div>
        </div>
        <div className="projects-summary-card">
          <span className="projects-card-label">Total Raised</span>
          <div className="projects-card-value">{formatCurrency(totalRaised)}</div>
        </div>
        <div className="projects-summary-card">
          <span className="projects-card-label">Total Budget</span>
          <div className="projects-card-value">{formatCurrency(totalTarget)}</div>
        </div>
        <div className="projects-summary-card">
          <span className="projects-card-label">Completed This Quarter</span>
          <div className="projects-card-value">{completedCount}</div>
        </div>
      </div>

      {/* ─── Projects Card Section ────────────────────────────────────────── */}
      <div className="projects-list-card">

        {/* Tabs + Search — mirrors attendees-card header */}
        <div className="projects-card-header">
          <div className="projects-tabs">
            {tabs.map(tab => (
              <button
                key={tab}
                className={`projects-tab ${activeTab === tab ? 'projects-tab-active' : ''}`}
                onClick={() => setActiveTab(tab)}
              >
                {tab}
              </button>
            ))}
          </div>
          <input
            type="text"
            className="projects-search"
            placeholder="Search projects..."
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
          />
        </div>

        {/* Card Grid Body */}
        <div className="projects-card-body">
          {filtered.length === 0 ? (
            <div className="projects-empty-state">No projects found.</div>
          ) : (
            <div className="projects-card-grid">
              {filtered.map(project => {
                const pct = getProgressPercent(project.raised, project.target);
                return (
                  <div
                    key={project.id}
                    className="project-card"
                    onClick={() => setViewingProject(project)}
                  >
                    {/* Card Top Row: name + status badge */}
                    <div className="project-card-top">
                      <div className="project-card-title-group">
                        <span className="project-card-name">{project.name}</span>
                        <span className="project-card-category">{project.category}</span>
                      </div>
                      <span className={`project-status-badge ${statusColor(project.status)}`}>
                        {project.status}
                      </span>
                    </div>

                    {/* Funding progress */}
                    <div className="project-card-progress-section">
                      <div className="project-card-amounts">
                        <span className="project-card-raised">{formatCurrency(project.raised)}</span>
                        <span className="project-card-target">of {formatCurrency(project.target)}</span>
                      </div>
                      <div className="project-card-bar-track">
                        <div
                          className={`project-card-bar-fill ${statusColor(project.status)}`}
                          style={{ width: `${pct}%` }}
                        />
                      </div>
                      <span className="project-card-pct">{pct}% funded</span>
                    </div>

                    {/* Meta row: lead + deadline */}
                    <div className="project-card-meta">
                      <div className="project-card-lead">
                        <div className="project-lead-avatar">{project.leadInitials}</div>
                        <span className="project-card-lead-name">{project.lead}</span>
                      </div>
                      <div className="project-card-deadline">
                        <CalendarIcon size={13} className="project-card-deadline-icon" />
                        <span>{project.deadline}</span>
                      </div>
                    </div>

                    {/* Action buttons */}
                    <div className="project-card-actions" onClick={e => e.stopPropagation()}>
                      <button
                        className="project-action-btn"
                        onClick={() => openEditPanel(project)}
                      >
                        Edit
                      </button>
                      <button
                        className="project-action-btn danger"
                        onClick={() => openDeleteConfirm(project)}
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>

      {/* ════════════════════════════════════════════════════════════════════
          VIEW PROJECT DETAIL PANEL
          ════════════════════════════════════════════════════════════════ */}
      {viewingProject && (
        <div className="projects-panel-overlay" onClick={() => setViewingProject(null)}>
          <div className="projects-side-panel" onClick={e => e.stopPropagation()}>
            <div className="projects-panel-header">
              <div>
                <h2 className="projects-panel-title">{viewingProject.name}</h2>
                <p className="projects-panel-subtitle">{viewingProject.category}</p>
              </div>
              <button
                className="projects-panel-close"
                onClick={() => setViewingProject(null)}
              >
                <CloseIcon />
              </button>
            </div>

            <div className="projects-panel-body">
              {/* Funding Progress */}
              <section className="detail-section">
                <h3 className="detail-section-title">FUNDING PROGRESS</h3>
                <div className="detail-funding-amount">
                  {formatCurrency(viewingProject.raised)}
                  <span className="detail-funding-total">
                    &nbsp;&nbsp;Target: {formatCurrency(viewingProject.target)}
                  </span>
                </div>
                <div className="detail-progress-track">
                  <div
                    className={`detail-progress-fill ${statusColor(viewingProject.status)}`}
                    style={{ width: `${getProgressPercent(viewingProject.raised, viewingProject.target)}%` }}
                  />
                </div>
                <div className="detail-progress-label">
                  {getProgressPercent(viewingProject.raised, viewingProject.target)}% Funded
                </div>
              </section>

              {/* Status & Dates */}
              <section className="detail-section">
                <div className="detail-meta-grid">
                  <div className="detail-meta-item">
                    <span className="detail-meta-label">Status</span>
                    <span className={`project-status-badge ${statusColor(viewingProject.status)}`}>
                      {viewingProject.status}
                    </span>
                  </div>
                  <div className="detail-meta-item">
                    <span className="detail-meta-label">Start Date</span>
                    <span className="detail-meta-value">{viewingProject.startDate}</span>
                  </div>
                  <div className="detail-meta-item">
                    <span className="detail-meta-label">Deadline</span>
                    <span className="detail-meta-value">{viewingProject.deadline}</span>
                  </div>
                  <div className="detail-meta-item">
                    <span className="detail-meta-label">Remaining Funds Needed</span>
                    <span className="detail-meta-value detail-meta-highlight">
                      {formatCurrency(Math.max(viewingProject.target - viewingProject.raised, 0))}
                    </span>
                  </div>
                </div>
              </section>

              {/* Description */}
              <section className="detail-section">
                <h3 className="detail-section-title">ABOUT THIS PROJECT</h3>
                <p className="detail-description">{viewingProject.description}</p>
              </section>

              {/* Team */}
              <section className="detail-section">
                <h3 className="detail-section-title">ASSIGNED TEAM</h3>
                <div className="detail-team-list">
                  {viewingProject.team.map(member => (
                    <div key={member.id} className="detail-team-member">
                      <div className="detail-member-avatar">{member.initials}</div>
                      <div>
                        <div className="detail-member-name">{member.name}</div>
                        <div className="detail-member-role">{member.role}</div>
                      </div>
                    </div>
                  ))}
                  {viewingProject.team.length === 0 && (
                    <p className="detail-empty-team">No team members assigned yet.</p>
                  )}
                </div>
              </section>
            </div>

            <div className="projects-panel-footer">
              <button
                className="projects-btn projects-btn-secondary"
                onClick={() => openEditPanel(viewingProject)}
              >
                <EditIcon size={16} /> Edit Details
              </button>
              <button
                className="projects-btn projects-btn-danger"
                onClick={() => openDeleteConfirm(viewingProject)}
              >
                <TrashIcon size={16} /> Delete Project
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ════════════════════════════════════════════════════════════════════
          EDIT PROJECT PANEL
          ════════════════════════════════════════════════════════════════ */}
      {editingProject && (
        <div className="projects-panel-overlay" onClick={() => setEditingProject(null)}>
          <div className="projects-side-panel" onClick={e => e.stopPropagation()}>
            <div className="projects-panel-header">
              <div>
                <h2 className="projects-panel-title">Edit Project</h2>
                <p className="projects-panel-subtitle">Update details for Project</p>
              </div>
              <button
                className="projects-panel-close"
                onClick={() => setEditingProject(null)}
              >
                <CloseIcon />
              </button>
            </div>

            <div className="projects-panel-body">
              <ProjectFormFields form={form} setForm={setForm} />
            </div>

            <div className="projects-panel-footer">
              <button
                className="projects-btn projects-btn-secondary"
                onClick={() => setEditingProject(null)}
              >
                Cancel
              </button>
              <button
                className="projects-btn projects-btn-primary"
                onClick={handleSaveEdit}
              >
                Save Changes
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ════════════════════════════════════════════════════════════════════
          CREATE PROJECT PANEL
          ════════════════════════════════════════════════════════════════ */}
      {showCreatePanel && (
        <div className="projects-panel-overlay" onClick={() => setShowCreatePanel(false)}>
          <div className="projects-side-panel" onClick={e => e.stopPropagation()}>
            <div className="projects-panel-header">
              <div>
                <h2 className="projects-panel-title">Create Project</h2>
                <p className="projects-panel-subtitle">Update details for Project</p>
              </div>
              <button
                className="projects-panel-close"
                onClick={() => setShowCreatePanel(false)}
              >
                <CloseIcon />
              </button>
            </div>

            <div className="projects-panel-body">
              <ProjectFormFields form={form} setForm={setForm} />
            </div>

            <div className="projects-panel-footer">
              <button
                className="projects-btn projects-btn-secondary"
                onClick={() => setShowCreatePanel(false)}
              >
                Cancel
              </button>
              <button
                className="projects-btn projects-btn-primary"
                onClick={handleSaveCreate}
              >
                Save Changes
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ════════════════════════════════════════════════════════════════════
          DELETE CONFIRMATION MODAL
          ════════════════════════════════════════════════════════════════ */}
      {showDeleteConfirm && deletingProject && (
        <div className="projects-modal-overlay" onClick={() => setShowDeleteConfirm(false)}>
          <div className="projects-modal" onClick={e => e.stopPropagation()}>
            <div className="projects-modal-header">
              <h2 className="projects-modal-title">Delete Project</h2>
              <button
                className="projects-panel-close"
                onClick={() => setShowDeleteConfirm(false)}
              >
                <CloseIcon />
              </button>
            </div>
            <div className="projects-modal-body">
              <div className="delete-confirm-icon">
                <WarningIcon size={28} />
              </div>
              <p className="delete-confirm-text">
                Are you sure you want to delete{' '}
                <span className="delete-confirm-name">"{deletingProject.name}"</span>?
              </p>
              <p className="delete-confirm-warning">
                This action cannot be undone and all associated data will be permanently removed.
              </p>
            </div>
            <div className="projects-modal-footer">
              <button
                className="projects-btn projects-btn-secondary"
                onClick={() => setShowDeleteConfirm(false)}
              >
                Cancel
              </button>
              <button
                className="projects-btn projects-btn-danger"
                onClick={confirmDelete}
              >
                Delete Project
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

// ─── Shared Form Fields ───────────────────────────────────────────────────────

interface FormState {
  name: string;
  category: string;
  lead: string;
  target: string;
  deadline: string;
  status: ProjectStatus;
  description: string;
}

const ProjectFormFields: React.FC<{
  form: FormState;
  setForm: React.Dispatch<React.SetStateAction<FormState>>;
}> = ({ form, setForm }) => {
  const set = (key: keyof FormState, value: string) =>
    setForm(prev => ({ ...prev, [key]: value }));

  return (
    <>
      {/* Project Name */}
      <div className="projects-form-group">
        <label className="projects-form-label">Project Name</label>
        <input
          type="text"
          className="projects-form-input"
          placeholder="e.g. Youth Camp 2025"
          value={form.name}
          onChange={e => set('name', e.target.value)}
        />
      </div>

      {/* Category */}
      <div className="projects-form-group">
        <label className="projects-form-label">Category</label>
        <select
          className="projects-form-select"
          value={form.category}
          onChange={e => set('category', e.target.value)}
        >
          <option value="">Select Category</option>
          {PROJECT_CATEGORIES.map(cat => (
            <option key={cat} value={cat}>{cat}</option>
          ))}
        </select>
      </div>

      {/* Project Lead */}
      <div className="projects-form-group">
        <label className="projects-form-label">Project Lead</label>
        <div className="projects-form-input-icon-wrap">
          <input
            type="text"
            className="projects-form-input with-icon"
            placeholder="Select Project Lead..."
            value={form.lead}
            onChange={e => set('lead', e.target.value)}
          />
          <MembersIcon size={16} className="projects-form-input-icon" />
        </div>
      </div>

      {/* Target + Deadline row */}
      <div className="projects-form-row">
        <div className="projects-form-group">
          <label className="projects-form-label">Target Amount</label>
          <input
            type="text"
            className="projects-form-input"
            placeholder="$ 0.00"
            value={form.target}
            onChange={e => set('target', e.target.value.replace(/[^0-9.]/g, ''))}
          />
        </div>
        <div className="projects-form-group">
          <label className="projects-form-label">Deadline</label>
          <div className="projects-form-input-icon-wrap">
            <input
              type="text"
              className="projects-form-input with-icon"
              placeholder="Select date"
              value={form.deadline}
              onChange={e => set('deadline', e.target.value)}
            />
            <CalendarIcon size={16} className="projects-form-input-icon" />
          </div>
        </div>
      </div>

      {/* Status */}
      <div className="projects-form-group">
        <label className="projects-form-label">Initial Status</label>
        <select
          className="projects-form-select"
          value={form.status}
          onChange={e => set('status', e.target.value as ProjectStatus)}
        >
          <option value="Planning">Planning</option>
          <option value="On Track">On Track</option>
          <option value="Off Track">Off Track</option>
          <option value="Completed">Completed</option>
        </select>
      </div>

      {/* Description */}
      <div className="projects-form-group">
        <label className="projects-form-label">Description</label>
        <textarea
          className="projects-form-textarea"
          placeholder="Add project description, goals, or notes here..."
          value={form.description}
          onChange={e => set('description', e.target.value)}
        />
      </div>
    </>
  );
};

export default Projects;
