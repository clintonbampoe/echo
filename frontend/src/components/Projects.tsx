import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useLayout } from '../hooks/useLayout';
import { useMembers } from '../hooks/useMembers';
import { useProjectContributions } from '../hooks/useProjectContributions';
import {
  useCreateProject,
  useCreateProjectCategory,
  useDeleteProject,
  useProjectCategories,
  useProjects,
  useUpdateProject,
} from '../hooks/useProjects';
import '../styles/Projects.css';
import type { Project, ProjectStatus } from '../types/project';
import {
  CalendarIcon,
  CloseIcon,
  EditIcon,
  MembersIcon,
  TrashIcon,
} from './Icons';
import DeleteConfirmModal from './common/DeleteConfirmModal';
import { getErrorMessage } from '../utils/errors';

// ─── Types ────────────────────────────────────────────────────────────────────

type TabFilter = 'All Projects' | 'Active' | 'Planning' | 'Completed';

interface FormState {
  name: string;
  categoryId: number | '';
  managerId: string;
  targetAmount: string;
  startDate: string;
  endDate: string;
  status: ProjectStatus;
  description: string;
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

const formatCurrency = (amount: number) =>
  `$ ${amount.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 2 })}`;

const formatDate = (dateStr?: string | null) => {
  if (!dateStr) return 'TBD';
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return d.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
};

const getProgressPercent = (raised: number, target: number) =>
  target > 0 ? Math.min(Math.round((raised / target) * 100), 100) : 0;

const getInitials = (name?: string) => {
  if (!name) return '??';
  const parts = name.trim().split(/\s+/);
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
};

const getStatusBadgeLabel = (status: ProjectStatus): string => {
  switch (status) {
    case 'OnTrack':  return 'On Track';
    case 'Complete': return 'Completed';
    case 'Planning': return 'Planning';
    case 'AtRisk':   return 'At Risk';
    case 'Missed':   return 'Missed';
    default:         return status;
  }
};

const statusColor = (status: ProjectStatus): string => {
  switch (status) {
    case 'OnTrack':  return 'status-on-track';
    case 'Complete': return 'status-completed';
    case 'Planning': return 'status-planning';
    case 'AtRisk':
    case 'Missed':   return 'status-off-track';
    default:         return 'status-planning';
  }
};

const emptyForm = (): FormState => ({
  name: '',
  categoryId: '',
  managerId: '',
  targetAmount: '',
  startDate: new Date().toISOString().split('T')[0],
  endDate: '',
  status: 'Planning',
  description: '',
});

// ─── Projects Component ───────────────────────────────────────────────────────

const Projects: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  // Queries
  const { data: projectsData, isLoading: isLoadingProjects } = useProjects();
  const { data: categories = [] } = useProjectCategories();
  const { data: membersData } = useMembers({}, 100);
  const { data: contributionsData } = useProjectContributions({}, 1000);

  // Mutations
  const createProjectMutation = useCreateProject();
  const updateProjectMutation = useUpdateProject();
  const deleteProjectMutation = useDeleteProject();

  // UI State
  const [activeTab, setActiveTab] = useState<TabFilter>('All Projects');
  const [searchQuery, setSearchQuery] = useState('');

  // Panels
  const [viewingProject, setViewingProject] = useState<Project | null>(null);
  const [editingProject, setEditingProject] = useState<Project | null>(null);
  const [showCreatePanel, setShowCreatePanel] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingProject, setDeletingProject] = useState<Project | null>(null);
  const [formError, setFormError] = useState<string | null>(null);

  // Form
  const [form, setForm] = useState<FormState>(emptyForm());

  const projectsList = projectsData?.data || [];
  const membersList = membersData?.data || [];

  // Map contributions raised per project
  const raisedByProjectId = useMemo(() => {
    const contributionsList = contributionsData?.data || [];
    const map = new Map<string, number>();
    for (const c of contributionsList) {
      const current = map.get(c.projectId) || 0;
      map.set(c.projectId, current + c.amount);
    }
    return map;
  }, [contributionsData?.data]);

  // ── Layout Header ──────────────────────────────────────────────────────────

  const openCreatePanel = useCallback(() => {
    setForm(emptyForm());
    setFormError(null);
    setShowCreatePanel(true);
  }, []);

  useEffect(() => {
    setTitle('Projects');
    setCtas([
      {
        type: 'button',
        label: 'Add Project',
        icon: 'plus',
        variant: 'primary',
        onClick: openCreatePanel,
      },
    ]);
  }, [setTitle, setCtas, openCreatePanel]);

  // ── Handlers ───────────────────────────────────────────────────────────────

  const openEditPanel = (project: Project) => {
    setViewingProject(null);
    setEditingProject(project);
    setForm({
      name: project.name,
      categoryId: project.categoryId || '',
      managerId: project.managerId || '',
      targetAmount: project.targetAmount ? String(project.targetAmount) : '',
      startDate: project.startDate ? project.startDate.split('T')[0] : '',
      endDate: project.endDate ? project.endDate.split('T')[0] : '',
      status: project.status,
      description: project.description || '',
    });
    setFormError(null);
  };

  const handleSaveCreate = async () => {
    if (!form.name.trim()) {
      setFormError('Project name is required.');
      return;
    }
    if (!form.categoryId) {
      setFormError('Project category is required.');
      return;
    }
    if (!form.managerId) {
      setFormError('Project manager is required.');
      return;
    }
    if (!form.targetAmount || isNaN(parseFloat(form.targetAmount))) {
      setFormError('A valid target amount is required.');
      return;
    }
    if (!form.startDate) {
      setFormError('Start date is required.');
      return;
    }

    try {
      setFormError(null);
      await createProjectMutation.mutateAsync({
        name: form.name.trim(),
        categoryId: Number(form.categoryId),
        managerId: form.managerId,
        targetAmount: parseFloat(form.targetAmount),
        status: form.status,
        startDate: form.startDate,
        endDate: form.endDate || null,
        description: form.description.trim() || null,
      });
      setShowCreatePanel(false);
    } catch (err: unknown) {
      setFormError(getErrorMessage(err, 'Failed to create project'));
    }
  };

  const handleSaveEdit = async () => {
    if (!editingProject) return;
    if (!form.name.trim()) {
      setFormError('Project name is required.');
      return;
    }

    try {
      setFormError(null);
      await updateProjectMutation.mutateAsync({
        id: editingProject.id,
        data: {
          name: form.name.trim(),
          categoryId: form.categoryId ? Number(form.categoryId) : undefined,
          managerId: form.managerId || undefined,
          targetAmount: form.targetAmount ? parseFloat(form.targetAmount) : undefined,
          status: form.status,
          startDate: form.startDate || undefined,
          endDate: form.endDate || null,
          description: form.description.trim() || null,
        },
      });
      setEditingProject(null);
    } catch (err: unknown) {
      setFormError(getErrorMessage(err, 'Failed to update project'));
    }
  };

  const openDeleteConfirm = (project: Project) => {
    setViewingProject(null);
    setDeletingProject(project);
    setShowDeleteConfirm(true);
  };

  const confirmDelete = async () => {
    if (!deletingProject) return;
    try {
      await deleteProjectMutation.mutateAsync(deletingProject.id);
      setShowDeleteConfirm(false);
      setDeletingProject(null);
    } catch (err: unknown) {
      console.error('Failed to delete project', err);
    }
  };

  // ── Derived Data & Stats ───────────────────────────────────────────────────

  const activeCount = projectsList.filter(p => p.status === 'OnTrack').length;
  const totalRaised = Array.from(raisedByProjectId.values()).reduce((s, r) => s + r, 0);
  const totalTarget = projectsList.reduce((s, p) => s + (p.targetAmount || 0), 0);
  const completedCount = projectsList.filter(p => p.status === 'Complete').length;

  const tabFiltered = projectsList.filter(p => {
    if (activeTab === 'All Projects') return true;
    if (activeTab === 'Active')      return p.status === 'OnTrack';
    if (activeTab === 'Planning')    return p.status === 'Planning';
    if (activeTab === 'Completed')   return p.status === 'Complete';
    return true;
  });

  const filtered = tabFiltered.filter(p =>
    p.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    (p.managerName && p.managerName.toLowerCase().includes(searchQuery.toLowerCase())) ||
    (p.categoryName && p.categoryName.toLowerCase().includes(searchQuery.toLowerCase()))
  );

  const tabs: TabFilter[] = ['All Projects', 'Active', 'Planning', 'Completed'];

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
          <span className="projects-card-label">Completed</span>
          <div className="projects-card-value">{completedCount}</div>
        </div>
      </div>

      {/* ─── Projects Card Section ────────────────────────────────────────── */}
      <div className="projects-list-card">

        {/* Tabs + Search */}
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
          {isLoadingProjects ? (
            <div className="projects-empty-state">Loading projects...</div>
          ) : filtered.length === 0 ? (
            <div className="projects-empty-state">No projects found.</div>
          ) : (
            <div className="projects-card-grid">
              {filtered.map(project => {
                const raised = raisedByProjectId.get(project.id) || project.raisedAmount || 0;
                const pct = getProgressPercent(raised, project.targetAmount);
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
                        <span className="project-card-category">{project.categoryName || 'General'}</span>
                      </div>
                      <span className={`project-status-badge ${statusColor(project.status)}`}>
                        {getStatusBadgeLabel(project.status)}
                      </span>
                    </div>

                    {/* Funding progress */}
                    <div className="project-card-progress-section">
                      <div className="project-card-amounts">
                        <span className="project-card-raised">{formatCurrency(raised)}</span>
                        <span className="project-card-target">of {formatCurrency(project.targetAmount)}</span>
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
                        <div className="project-lead-avatar">{getInitials(project.managerName)}</div>
                        <span className="project-card-lead-name">{project.managerName || 'Unassigned'}</span>
                      </div>
                      <div className="project-card-deadline">
                        <CalendarIcon size={13} className="project-card-deadline-icon" />
                        <span>{formatDate(project.endDate || project.startDate)}</span>
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
                <p className="projects-panel-subtitle">{viewingProject.categoryName || 'General'}</p>
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
              {(() => {
                const raised = raisedByProjectId.get(viewingProject.id) || viewingProject.raisedAmount || 0;
                const pct = getProgressPercent(raised, viewingProject.targetAmount);
                return (
                  <section className="detail-section">
                    <h3 className="detail-section-title">FUNDING PROGRESS</h3>
                    <div className="detail-funding-amount">
                      {formatCurrency(raised)}
                      <span className="detail-funding-total">
                        &nbsp;&nbsp;Target: {formatCurrency(viewingProject.targetAmount)}
                      </span>
                    </div>
                    <div className="detail-progress-track">
                      <div
                        className={`detail-progress-fill ${statusColor(viewingProject.status)}`}
                        style={{ width: `${pct}%` }}
                      />
                    </div>
                    <div className="detail-progress-label">
                      {pct}% Funded
                    </div>
                  </section>
                );
              })()}

              {/* Status & Dates */}
              <section className="detail-section">
                <div className="detail-meta-grid">
                  <div className="detail-meta-item">
                    <span className="detail-meta-label">Status</span>
                    <span className={`project-status-badge ${statusColor(viewingProject.status)}`}>
                      {getStatusBadgeLabel(viewingProject.status)}
                    </span>
                  </div>
                  <div className="detail-meta-item">
                    <span className="detail-meta-label">Start Date</span>
                    <span className="detail-meta-value">{formatDate(viewingProject.startDate)}</span>
                  </div>
                  <div className="detail-meta-item">
                    <span className="detail-meta-label">End Date</span>
                    <span className="detail-meta-value">{formatDate(viewingProject.endDate)}</span>
                  </div>
                  <div className="detail-meta-item">
                    <span className="detail-meta-label">Remaining Funds Needed</span>
                    <span className="detail-meta-value detail-meta-highlight">
                      {formatCurrency(Math.max(viewingProject.targetAmount - (raisedByProjectId.get(viewingProject.id) || 0), 0))}
                    </span>
                  </div>
                </div>
              </section>

              {/* Description */}
              <section className="detail-section">
                <h3 className="detail-section-title">ABOUT THIS PROJECT</h3>
                <p className="detail-description">{viewingProject.description || 'No description provided.'}</p>
              </section>

              {/* Project Lead */}
              <section className="detail-section">
                <h3 className="detail-section-title">PROJECT LEAD</h3>
                <div className="detail-team-list">
                  <div className="detail-team-member">
                    <div className="detail-member-avatar">{getInitials(viewingProject.managerName)}</div>
                    <div>
                      <div className="detail-member-name">{viewingProject.managerName || 'Unassigned'}</div>
                      <div className="detail-member-role">Project Manager</div>
                    </div>
                  </div>
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
                <p className="projects-panel-subtitle">Update details for {editingProject.name}</p>
              </div>
              <button
                className="projects-panel-close"
                onClick={() => setEditingProject(null)}
              >
                <CloseIcon />
              </button>
            </div>

            <div className="projects-panel-body">
              {formError && <div style={{ color: '#ef4444', marginBottom: '12px', fontSize: '13px' }}>{formError}</div>}
              <ProjectFormFields
                form={form}
                setForm={setForm}
                categories={categories}
                members={membersList}
              />
            </div>

            <div className="projects-panel-footer">
              <button
                className="projects-btn projects-btn-secondary"
                onClick={() => setEditingProject(null)}
                disabled={updateProjectMutation.isPending}
              >
                Cancel
              </button>
              <button
                className="projects-btn projects-btn-primary"
                onClick={handleSaveEdit}
                disabled={updateProjectMutation.isPending}
              >
                {updateProjectMutation.isPending ? 'Saving...' : 'Save Changes'}
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
                <p className="projects-panel-subtitle">Add a new church project or campaign</p>
              </div>
              <button
                className="projects-panel-close"
                onClick={() => setShowCreatePanel(false)}
              >
                <CloseIcon />
              </button>
            </div>

            <div className="projects-panel-body">
              {formError && <div style={{ color: '#ef4444', marginBottom: '12px', fontSize: '13px' }}>{formError}</div>}
              <ProjectFormFields
                form={form}
                setForm={setForm}
                categories={categories}
                members={membersList}
              />
            </div>

            <div className="projects-panel-footer">
              <button
                className="projects-btn projects-btn-secondary"
                onClick={() => setShowCreatePanel(false)}
                disabled={createProjectMutation.isPending}
              >
                Cancel
              </button>
              <button
                className="projects-btn projects-btn-primary"
                onClick={handleSaveCreate}
                disabled={createProjectMutation.isPending}
              >
                {createProjectMutation.isPending ? 'Creating...' : 'Create Project'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ════════════════════════════════════════════════════════════════════
          DELETE CONFIRMATION MODAL
          ════════════════════════════════════════════════════════════════ */}
      <DeleteConfirmModal
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={confirmDelete}
        itemName={deletingProject?.name || ''}
        title="Delete Project"
        confirmText="Delete Project"
      />
    </div>
  );
};

// ─── Shared Form Fields ───────────────────────────────────────────────────────

interface ProjectFormFieldsProps {
  form: FormState;
  setForm: React.Dispatch<React.SetStateAction<FormState>>;
  categories: Array<{ id: number; name: string }>;
  members: Array<{ id: string; firstName: string; lastName: string }>;
}

const ProjectFormFields: React.FC<ProjectFormFieldsProps> = ({
  form,
  setForm,
  categories,
  members,
}) => {
  const [isAddingCategory, setIsAddingCategory] = useState(false);
  const [newCategoryName, setNewCategoryName] = useState('');
  const [categoryError, setCategoryError] = useState<string | null>(null);
  const createCategoryMutation = useCreateProjectCategory();

  const handleAddCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCategoryName.trim()) return;
    try {
      setCategoryError(null);
      const res = await createCategoryMutation.mutateAsync({ name: newCategoryName.trim() });
      if (res && res.id) {
        set('categoryId', res.id);
      }
      setNewCategoryName('');
      setIsAddingCategory(false);
    } catch (err: unknown) {
      setCategoryError(getErrorMessage(err, 'Failed to create category'));
    }
  };

  const set = <K extends keyof FormState>(key: K, value: FormState[K]) =>
    setForm(prev => ({ ...prev, [key]: value }));

  return (
    <>
      {/* Project Name */}
      <div className="projects-form-group">
        <label className="projects-form-label">Project Name *</label>
        <input
          type="text"
          className="projects-form-input"
          placeholder="e.g. Sanctuary Renovation"
          value={form.name}
          onChange={e => set('name', e.target.value)}
        />
      </div>

      {/* Category */}
      <div className="projects-form-group">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '6px' }}>
          <label className="projects-form-label" style={{ marginBottom: 0 }}>Category *</label>
          {!isAddingCategory && (
            <button
              type="button"
              onClick={() => { setIsAddingCategory(true); setCategoryError(null); }}
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
              + New Category
            </button>
          )}
        </div>

        {isAddingCategory ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
            <div style={{ display: 'flex', gap: '8px' }}>
              <input
                type="text"
                className="projects-form-input"
                placeholder="Category name (e.g. Building)"
                value={newCategoryName}
                onChange={e => setNewCategoryName(e.target.value)}
                autoFocus
                onKeyDown={e => {
                  if (e.key === 'Enter') {
                    e.preventDefault();
                    handleAddCategory(e);
                  }
                }}
              />
              <button
                type="button"
                className="projects-btn projects-btn-primary"
                style={{ padding: '0 14px', whiteSpace: 'nowrap', fontSize: '13px' }}
                onClick={handleAddCategory}
                disabled={createCategoryMutation.isPending || !newCategoryName.trim()}
              >
                {createCategoryMutation.isPending ? 'Adding...' : 'Add'}
              </button>
              <button
                type="button"
                className="projects-btn projects-btn-secondary"
                style={{ padding: '0 10px', fontSize: '13px' }}
                onClick={() => { setIsAddingCategory(false); setNewCategoryName(''); setCategoryError(null); }}
              >
                Cancel
              </button>
            </div>
            {categoryError && (
              <span style={{ color: '#ef4444', fontSize: '12px' }}>{categoryError}</span>
            )}
          </div>
        ) : (
          <select
            className="projects-form-select"
            value={form.categoryId}
            onChange={e => set('categoryId', e.target.value ? Number(e.target.value) : '')}
          >
            <option value="">Select Category</option>
            {categories.map(cat => (
              <option key={cat.id} value={cat.id}>{cat.name}</option>
            ))}
          </select>
        )}
      </div>

      {/* Project Lead / Manager */}
      <div className="projects-form-group">
        <label className="projects-form-label">Project Lead *</label>
        <div className="projects-form-input-icon-wrap">
          <select
            className="projects-form-select with-icon"
            value={form.managerId}
            onChange={e => set('managerId', e.target.value)}
          >
            <option value="">Select Project Lead</option>
            {members.map(m => (
              <option key={m.id} value={m.id}>{m.firstName} {m.lastName}</option>
            ))}
          </select>
          <MembersIcon size={16} className="projects-form-input-icon" />
        </div>
      </div>

      {/* Target + Start Date row */}
      <div className="projects-form-row">
        <div className="projects-form-group">
          <label className="projects-form-label">Target Amount ($) *</label>
          <input
            type="text"
            className="projects-form-input"
            placeholder="0.00"
            value={form.targetAmount}
            onChange={e => set('targetAmount', e.target.value.replace(/[^0-9.]/g, ''))}
          />
        </div>
        <div className="projects-form-group">
          <label className="projects-form-label">Start Date *</label>
          <input
            type="date"
            className="projects-form-input"
            value={form.startDate}
            onChange={e => set('startDate', e.target.value)}
          />
        </div>
      </div>

      {/* End Date + Status row */}
      <div className="projects-form-row">
        <div className="projects-form-group">
          <label className="projects-form-label">End Date (Optional)</label>
          <input
            type="date"
            className="projects-form-input"
            value={form.endDate}
            onChange={e => set('endDate', e.target.value)}
          />
        </div>
        <div className="projects-form-group">
          <label className="projects-form-label">Status</label>
          <select
            className="projects-form-select"
            value={form.status}
            onChange={e => set('status', e.target.value as ProjectStatus)}
          >
            <option value="Planning">Planning</option>
            <option value="OnTrack">On Track</option>
            <option value="AtRisk">At Risk</option>
            <option value="Complete">Completed</option>
            <option value="Missed">Missed</option>
          </select>
        </div>
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
