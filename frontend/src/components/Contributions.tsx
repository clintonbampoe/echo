import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useLayout } from '../hooks/useLayout';
import { CloseIcon, SearchIcon, MembersIcon, ClockIcon, ChevronLeftIcon } from './Icons';
import ExportPanel from './ExportPanel';
import DeleteConfirmModal from './common/DeleteConfirmModal';
import { getErrorMessage } from '../utils/errors';
import '../styles/Contributions.css';
import { useProjects } from '../hooks/useProjects';
import {
  useProjectContributions,
  useCreateProjectContribution,
  useDeleteProjectContribution,
} from '../hooks/useProjectContributions';
import type { ProjectContribution, PaymentMethod } from '../types/project';

// ─── Helpers ──────────────────────────────────────────────────────────────────

const formatCurrency = (amount: number) =>
  `$ ${amount.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 2 })}`;

const formatDate = (dateStr?: string | null) => {
  if (!dateStr) return 'N/A';
  const d = new Date(dateStr);
  if (isNaN(d.getTime())) return dateStr;
  return d.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
};

const getProgressPercent = (raised: number, target: number) =>
  target > 0 ? Math.min(Math.round((raised / target) * 100), 100) : 0;

const formatPaymentMethod = (pm: PaymentMethod | string) => {
  switch (pm) {
    case 'MobileMoney':   return 'Mobile Money';
    case 'BankTransfer':  return 'Bank Transfer';
    case 'CreditCard':    return 'Credit Card';
    case 'Cash':          return 'Cash';
    case 'Cheque':        return 'Cheque';
    default:              return pm;
  }
};

const getStatusBadgeClass = (status: string) => {
  switch (status) {
    case 'OnTrack':  return 'active';
    case 'Complete': return 'completed';
    case 'Planning': return 'active';
    case 'AtRisk':
    case 'Missed':   return 'paused';
    default:         return 'active';
  }
};

const getStatusDisplay = (status: string) => {
  switch (status) {
    case 'OnTrack':  return 'On Track';
    case 'Complete': return 'Completed';
    case 'Planning': return 'Planning';
    case 'AtRisk':   return 'At Risk';
    case 'Missed':   return 'Missed';
    default:         return status;
  }
};

// ─── Empty Forms ──────────────────────────────────────────────────────────────

const emptyContribution = (defaultProjectId: string = '') => ({
  projectId: defaultProjectId,
  amount: '',
  dateContributed: new Date().toISOString().split('T')[0],
  paymentMethod: 'Cash' as PaymentMethod,
  description: '',
});

// ─── Component ────────────────────────────────────────────────────────────────

const Contributions: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  // Queries
  const { data: projectsData, isLoading: isLoadingProjects } = useProjects();
  const { data: contributionsData, isLoading: isLoadingContributions } = useProjectContributions({}, 1000);

  // Mutations
  const createContributionMutation = useCreateProjectContribution();
  const deleteContributionMutation = useDeleteProjectContribution();

  // View state
  const [viewingProjectId, setViewingProjectId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'Active Funds' | 'Completed' | 'All Funds' | 'All Entries'>('Active Funds');
  const [searchQuery, setSearchQuery] = useState('');

  // Modals state
  const [showAddPanel, setShowAddPanel] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [deletingContribution, setDeletingContribution] = useState<ProjectContribution | null>(null);
  const [formError, setFormError] = useState<string | null>(null);

  // Forms
  const [form, setForm] = useState(emptyContribution());

  const projectsList = useMemo(() => projectsData?.data || [], [projectsData]);
  const contributionsList = useMemo(() => contributionsData?.data || [], [contributionsData]);

  const viewingProject = useMemo(
    () => projectsList.find((p) => p.id === viewingProjectId) || null,
    [projectsList, viewingProjectId]
  );

  // Aggregate contributions per project
  const projectStats = useMemo(() => {
    const stats = new Map<string, { raised: number; count: number; lastDate: string | null }>();
    for (const c of contributionsList) {
      const existing = stats.get(c.projectId) || { raised: 0, count: 0, lastDate: null };
      existing.raised += c.amount;
      existing.count += 1;
      if (!existing.lastDate || new Date(c.dateContributed) > new Date(existing.lastDate)) {
        existing.lastDate = c.dateContributed;
      }
      stats.set(c.projectId, existing);
    }
    return stats;
  }, [contributionsList]);

  // Contributions for current view
  const currentRecords = useMemo(() => {
    if (!viewingProjectId) return contributionsList;
    return contributionsList.filter((c) => c.projectId === viewingProjectId);
  }, [contributionsList, viewingProjectId]);

  const filteredRecords = useMemo(() => {
    if (!searchQuery.trim()) return currentRecords;
    const q = searchQuery.toLowerCase();
    return currentRecords.filter(
      (r) =>
        (r.projectName && r.projectName.toLowerCase().includes(q)) ||
        (r.description && r.description.toLowerCase().includes(q)) ||
        r.paymentMethod.toLowerCase().includes(q)
    );
  }, [currentRecords, searchQuery]);

  // ── Layout Setup ──────────────────────────────────────────────────────────

  // ── Layout Setup ──────────────────────────────────────────────────────────

  const handleOpenAdd = useCallback((projectId?: string) => {
    setForm(emptyContribution(projectId || viewingProjectId || (projectsList[0]?.id ?? '')));
    setFormError(null);
    setShowAddPanel(true);
  }, [viewingProjectId, projectsList]);

  useEffect(() => {
    if (viewingProjectId && viewingProject) {
      setTitle(
        <button className="back-btn" onClick={() => { setViewingProjectId(null); setSearchQuery(''); }}>
          <ChevronLeftIcon size={20} />
          <span>Contributions / {viewingProject.name}</span>
        </button>
      );
      setCtas([
        { type: 'button', label: 'Export', icon: 'export', variant: 'secondary', onClick: () => setShowExportModal(true) },
        { type: 'button', label: 'Add Entry', icon: 'plus', variant: 'primary', onClick: () => handleOpenAdd(viewingProjectId) },
      ]);
    } else {
      setTitle('Contributions');
      setCtas([
        { type: 'button', label: 'Export', icon: 'export', variant: 'secondary', onClick: () => setShowExportModal(true) },
        { type: 'button', label: 'Add Entry', icon: 'plus', variant: 'primary', onClick: () => handleOpenAdd() },
      ]);
    }
  }, [viewingProjectId, viewingProject, handleOpenAdd, setTitle, setCtas]);

  // ── Stats (List View) ──────────────────────────────────────────────────────

  const totalReceived = useMemo(() => contributionsList.reduce((sum, c) => sum + c.amount, 0), [contributionsList]);
  const activeCount = useMemo(() => projectsList.filter((f) => f.status !== 'Complete').length, [projectsList]);
  const totalContributors = contributionsList.length;
  const averageContrib = totalContributors > 0 ? totalReceived / totalContributors : 0;

  // ── Stats (Detail View) ───────────────────────────────────────────────────

  const viewingStats = viewingProjectId ? projectStats.get(viewingProjectId) : null;
  const detailTotalRaised = viewingStats?.raised || 0;
  const detailTarget = viewingProject ? viewingProject.targetAmount : 0;
  const detailContributors = viewingStats?.count || 0;
  const detailRecent = viewingStats?.lastDate ? formatDate(viewingStats.lastDate) : 'None';

  // ── Handlers ───────────────────────────────────────────────────────────────

  const handleClosePanel = () => {
    setShowAddPanel(false);
    setFormError(null);
  };

  const handleSave = async () => {
    if (!form.projectId) {
      setFormError('Please select a project.');
      return;
    }
    const amt = parseFloat(form.amount);
    if (isNaN(amt) || amt <= 0) {
      setFormError('Please enter a valid amount.');
      return;
    }

    try {
      setFormError(null);
      await createContributionMutation.mutateAsync({
        projectId: form.projectId,
        amount: amt,
        dateContributed: form.dateContributed,
        paymentMethod: form.paymentMethod,
        description: form.description.trim() || null,
      });
      handleClosePanel();
    } catch (err: unknown) {
      setFormError(getErrorMessage(err, 'Failed to save contribution'));
    }
  };

  const confirmDelete = async () => {
    if (!deletingContribution) return;
    try {
      await deleteContributionMutation.mutateAsync(deletingContribution.id);
      setDeletingContribution(null);
    } catch (err: unknown) {
      console.error('Failed to delete contribution', err);
    }
  };

  // Filtered project list for main tab view
  const tabFilteredFunds = useMemo(() => {
    return projectsList.filter((fund) => {
      if (activeTab === 'Active Funds') return fund.status !== 'Complete';
      if (activeTab === 'Completed') return fund.status === 'Complete';
      return true;
    });
  }, [projectsList, activeTab]);

  const searchFilteredFunds = useMemo(() => {
    if (!searchQuery.trim()) return tabFilteredFunds;
    const q = searchQuery.toLowerCase();
    return tabFilteredFunds.filter(
      (fund) =>
        fund.name.toLowerCase().includes(q) ||
        (fund.categoryName && fund.categoryName.toLowerCase().includes(q))
    );
  }, [tabFilteredFunds, searchQuery]);

  // ── Main Render ────────────────────────────────────────────────────────────

  return (
    <div className="contributions-container">

      {/* ─── LIST VIEW ──────────────────────────────────────────────────────── */}
      {!viewingProject && (
        <>
          <div className="contributions-summary-cards">
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Total Received</span>
              <div className="contributions-card-value">{formatCurrency(totalReceived)}</div>
            </div>
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Active Funds</span>
              <div className="contributions-card-value">{activeCount}</div>
            </div>
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Total Entries</span>
              <div className="contributions-card-value">{totalContributors}</div>
            </div>
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Average Contribution</span>
              <div className="contributions-card-value">{formatCurrency(Math.round(averageContrib))}</div>
            </div>
          </div>

          <div className="contributions-toolbar">
            <div className="contributions-tabs">
              {(['Active Funds', 'Completed', 'All Funds', 'All Entries'] as const).map((tab) => (
                <button
                  key={tab}
                  className={`contributions-tab ${activeTab === tab ? 'active' : ''}`}
                  onClick={() => setActiveTab(tab)}
                >
                  {tab}
                </button>
              ))}
            </div>
            <div style={{ display: 'flex', alignItems: 'center', background: 'white', borderRadius: '8px', padding: '6px 12px', border: '1px solid #e2e8f0', minWidth: '240px' }}>
              <SearchIcon size={16} style={{ color: '#94a3b8', marginRight: '8px' }} />
              <input
                type="text"
                placeholder={activeTab === 'All Entries' ? 'Search entries...' : 'Search funds...'}
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                style={{ border: 'none', outline: 'none', width: '100%', fontSize: '13px' }}
              />
            </div>
          </div>

          {activeTab === 'All Entries' ? (
            /* Table view of all individual contribution entries across projects */
            <div className="detail-table-card">
              <div className="detail-table-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h3 className="detail-table-title">All Contribution Entries</h3>
              </div>
              <table className="contributions-table">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Project / Fund</th>
                    <th>Description / Notes</th>
                    <th>Payment Method</th>
                    <th>Amount</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredRecords.length === 0 ? (
                    <tr>
                      <td colSpan={6} style={{ textAlign: 'center', padding: '24px', color: '#94a3b8' }}>
                        No contribution records found.
                      </td>
                    </tr>
                  ) : (
                    filteredRecords.map((record) => {
                      const proj = projectsList.find((p) => p.id === record.projectId);
                      return (
                        <tr key={record.id}>
                          <td>{formatDate(record.dateContributed)}</td>
                          <td style={{ fontWeight: 600 }}>{record.projectName || proj?.name || 'Project'}</td>
                          <td>
                            <div className="member-cell">
                              <span>{record.description || 'General Contribution'}</span>
                            </div>
                          </td>
                          <td>
                            <span className="status-cell-badge">
                              {formatPaymentMethod(record.paymentMethod)}
                            </span>
                          </td>
                          <td className="amount-cell">{formatCurrency(record.amount)}</td>
                          <td>
                            <div className="actions-cell">
                              <button
                                className="action-sm-btn"
                                style={{ color: '#ef4444' }}
                                onClick={() => setDeletingContribution(record)}
                              >
                                Delete
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          ) : (
            /* Fund Card Grid */
            <div className="contributions-card-grid">
              {isLoadingProjects || isLoadingContributions ? (
                <div style={{ padding: '32px', textAlign: 'center', color: '#64748b' }}>Loading contributions & funds...</div>
              ) : searchFilteredFunds.length === 0 ? (
                <div style={{ padding: '32px', textAlign: 'center', color: '#64748b' }}>No funds or projects found.</div>
              ) : (
                searchFilteredFunds.map((fund) => {
                  const stat = projectStats.get(fund.id) || { raised: 0, count: 0, lastDate: null };
                  const pct = getProgressPercent(stat.raised, fund.targetAmount);
                  return (
                    <div key={fund.id} className="fund-card">
                      <div className="fund-card-header">
                        <h3 className="fund-card-title">{fund.name}</h3>
                        <span className={`fund-status-badge fund-status-${getStatusBadgeClass(fund.status)}`}>
                          {getStatusDisplay(fund.status)}
                        </span>
                      </div>

                      <div className="fund-card-progress-section">
                        <div className="fund-card-amounts">
                          <span className="fund-card-raised">{formatCurrency(stat.raised)}</span>
                          <span className="fund-card-target">of {formatCurrency(fund.targetAmount)} target</span>
                        </div>
                        <div className="fund-card-bar-track">
                          <div
                            className="fund-card-bar-fill"
                            style={{ width: `${pct}%` }}
                          />
                        </div>
                      </div>

                      <div className="fund-card-meta">
                        <div className="fund-meta-item">
                          <MembersIcon size={14} />
                          <span>{stat.count} contributions</span>
                        </div>
                        <div className="fund-meta-item">
                          <ClockIcon size={14} />
                          <span>Last: {stat.lastDate ? formatDate(stat.lastDate) : 'None'}</span>
                        </div>
                      </div>

                      <div className="fund-card-actions">
                        <button className="fund-action-btn" onClick={() => { setViewingProjectId(fund.id); setSearchQuery(''); }}>
                          View Records
                        </button>
                        <button className="fund-action-btn primary" onClick={() => handleOpenAdd(fund.id)}>
                          Add Entry
                        </button>
                      </div>
                    </div>
                  );
                })
              )}
            </div>
          )}
        </>
      )}

      {/* ─── DETAIL VIEW ────────────────────────────────────────────────────── */}
      {viewingProject && (
        <>
          <div className="contributions-summary-cards">
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Total Raised</span>
              <div className="contributions-card-value">{formatCurrency(detailTotalRaised)}</div>
            </div>
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Target Goal</span>
              <div className="contributions-card-value">{formatCurrency(detailTarget)}</div>
            </div>
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Total Entries</span>
              <div className="contributions-card-value">{detailContributors}</div>
            </div>
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Recent Entry</span>
              <div className="contributions-card-value">{detailRecent}</div>
            </div>
          </div>

          <div className="detail-table-card">
            <div className="detail-table-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <h3 className="detail-table-title">Contributions History</h3>
              <div style={{ display: 'flex', alignItems: 'center', background: '#f8fafc', borderRadius: '6px', padding: '4px 10px', border: '1px solid #e2e8f0' }}>
                <SearchIcon size={14} style={{ color: '#94a3b8', marginRight: '6px' }} />
                <input
                  type="text"
                  placeholder="Filter records..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  style={{ border: 'none', background: 'transparent', outline: 'none', fontSize: '12px' }}
                />
              </div>
            </div>
            <table className="contributions-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Description / Notes</th>
                  <th>Payment Method</th>
                  <th>Amount</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredRecords.length === 0 ? (
                  <tr>
                    <td colSpan={5} style={{ textAlign: 'center', padding: '24px', color: '#94a3b8' }}>
                      No contribution records found.
                    </td>
                  </tr>
                ) : (
                  filteredRecords.map((record) => (
                    <tr key={record.id}>
                      <td>{formatDate(record.dateContributed)}</td>
                      <td>
                        <div className="member-cell">
                          <span>{record.description || 'General Contribution'}</span>
                        </div>
                      </td>
                      <td>
                        <span className="status-cell-badge">
                          {formatPaymentMethod(record.paymentMethod)}
                        </span>
                      </td>
                      <td className="amount-cell">{formatCurrency(record.amount)}</td>
                      <td>
                        <div className="actions-cell">
                          <button
                            className="action-sm-btn"
                            style={{ color: '#ef4444' }}
                            onClick={() => setDeletingContribution(record)}
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
        </>
      )}

      {/* ─── ADD CONTRIBUTION SIDE PANEL ────────────────────────────────────── */}
      {showAddPanel && (
        <div className="panel-overlay" onClick={handleClosePanel}>
          <div className="panel" onClick={(e) => e.stopPropagation()}>
            <div className="panel-header">
              <h3 className="panel-title">Add Contribution Entry</h3>
              <button className="panel-close-btn" onClick={handleClosePanel}>
                <CloseIcon size={18} />
              </button>
            </div>

            <div className="panel-body">
              {formError && (
                <div style={{ padding: '10px 14px', background: '#fef2f2', border: '1px solid #fecaca', borderRadius: '8px', color: '#dc2626', fontSize: '13px', marginBottom: '16px' }}>
                  {formError}
                </div>
              )}

              {/* Fund Selector */}
              <div className="form-group">
                <label className="form-label">Select Project / Fund</label>
                <select
                  className="form-select"
                  value={form.projectId}
                  onChange={(e) => setForm((prev) => ({ ...prev, projectId: e.target.value }))}
                  disabled={!!viewingProjectId}
                >
                  <option value="">Select a project...</option>
                  {projectsList.map((p) => (
                    <option key={p.id} value={p.id}>{p.name}</option>
                  ))}
                </select>
              </div>

              {/* Amount */}
              <div className="form-group">
                <label className="form-label">Amount ($)</label>
                <input
                  type="number"
                  step="0.01"
                  min="0.01"
                  className="form-input"
                  placeholder="0.00"
                  value={form.amount}
                  onChange={(e) => setForm((prev) => ({ ...prev, amount: e.target.value }))}
                />
              </div>

              {/* Date */}
              <div className="form-group">
                <label className="form-label">Date Contributed</label>
                <input
                  type="date"
                  className="form-input"
                  value={form.dateContributed}
                  onChange={(e) => setForm((prev) => ({ ...prev, dateContributed: e.target.value }))}
                />
              </div>

              {/* Payment Method */}
              <div className="form-group">
                <label className="form-label">Payment Method</label>
                <select
                  className="form-select"
                  value={form.paymentMethod}
                  onChange={(e) => setForm((prev) => ({ ...prev, paymentMethod: e.target.value as PaymentMethod }))}
                >
                  <option value="Cash">Cash</option>
                  <option value="MobileMoney">Mobile Money</option>
                  <option value="BankTransfer">Bank Transfer</option>
                  <option value="CreditCard">Credit Card</option>
                  <option value="Cheque">Cheque</option>
                </select>
              </div>

              {/* Notes */}
              <div className="form-group">
                <label className="form-label">Description / Notes <span>(Optional)</span></label>
                <textarea
                  className="form-textarea"
                  placeholder="Add any details about this contribution..."
                  value={form.description}
                  onChange={(e) => setForm((prev) => ({ ...prev, description: e.target.value }))}
                />
              </div>
            </div>

            <div className="panel-footer">
              <button className="panel-cancel-btn" onClick={handleClosePanel}>
                Cancel
              </button>
              <button
                className="panel-submit-btn"
                onClick={handleSave}
                disabled={createContributionMutation.isPending}
              >
                {createContributionMutation.isPending ? 'Saving...' : 'Save Contribution'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ─── EXPORT PANEL ───────────────────────────────────────────────────── */}
      {showExportModal && (
        <ExportPanel
          title={viewingProject ? `Export ${viewingProject.name} Contributions` : 'Export All Contributions'}
          exportConfig={{
            title: viewingProject ? `${viewingProject.name} Contributions` : 'All Contributions',
            filename: viewingProject ? `contributions-${viewingProject.name.toLowerCase().replace(/\s+/g, '-')}` : 'all-contributions',
            columns: [
              { key: 'dateContributed', label: 'Date' },
              { key: 'projectName', label: 'Project' },
              { key: 'amount', label: 'Amount' },
              { key: 'paymentMethod', label: 'Payment Method' },
              { key: 'description', label: 'Notes' },
            ],
            rows: currentRecords.map((c) => ({
              ...c,
              projectName: c.projectName || projectsList.find((p) => p.id === c.projectId)?.name || 'N/A',
            })),
          }}
          onClose={() => setShowExportModal(false)}
        />
      )}

      {/* ─── DELETE CONFIRM MODAL ───────────────────────────────────────────── */}
      <DeleteConfirmModal
        isOpen={!!deletingContribution}
        onClose={() => setDeletingContribution(null)}
        onConfirm={confirmDelete}
        title="Delete Contribution"
        itemName={deletingContribution ? `${formatCurrency(deletingContribution.amount)} entry` : 'this entry'}
        confirmText="Delete"
        warningMessage="Are you sure you want to delete this contribution? This cannot be undone."
      />
    </div>
  );
};

export default Contributions;
