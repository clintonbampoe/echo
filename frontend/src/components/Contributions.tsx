import React, { useEffect, useState } from 'react';
import { useLayout } from '../hooks/useLayout';
import { CloseIcon, SearchIcon, CalendarIcon, MembersIcon, ClockIcon, ChevronLeftIcon } from './Icons';
import ExportPanel from './ExportPanel';
import '../styles/Contributions.css';

// ─── Types ────────────────────────────────────────────────────────────────────

type FundStatus = 'Active' | 'Paused' | 'Completed';
type ContributionStatus = 'Paid' | 'Pledged';

interface Fund {
  id: string;
  name: string;
  target: number;
  raised: number;
  contributorsCount: number;
  status: FundStatus;
  lastContributionText: string;
}

interface ContributionRecord {
  id: string;
  fundId: string;
  memberId: string;
  memberName: string;
  memberInitials: string;
  date: string;
  amount: number;
  paymentMethod: string;
  status: ContributionStatus;
  notes?: string;
}

// ─── Mock Data ────────────────────────────────────────────────────────────────

const mockFunds: Fund[] = [
  {
    id: 'F-001',
    name: 'Sanctuary Renovation',
    target: 44000,
    raised: 12000,
    contributorsCount: 45,
    status: 'Active',
    lastContributionText: 'Today',
  },
  {
    id: 'F-002',
    name: 'Youth Camp 2024',
    target: 22000,
    raised: 8000,
    contributorsCount: 56,
    status: 'Paused',
    lastContributionText: 'Yesterday',
  },
  {
    id: 'F-003',
    name: 'General Welfare Fund',
    target: 300000,
    raised: 45000,
    contributorsCount: 231,
    status: 'Active',
    lastContributionText: 'Today',
  },
];

const mockRecords: ContributionRecord[] = [
  { id: 'C-001', fundId: 'F-001', memberId: 'WF-231', memberName: 'Sarah Kent', memberInitials: 'SK', date: 'Nov 15, 2026', amount: 202, paymentMethod: 'Card', status: 'Paid' },
  { id: 'C-002', fundId: 'F-001', memberId: 'WF-104', memberName: 'John Maxwell', memberInitials: 'JM', date: 'Feb 14, 2026', amount: 2380, paymentMethod: 'Bank Transfer', status: 'Paid' },
  { id: 'C-003', fundId: 'F-001', memberId: 'WF-089', memberName: 'John Doe', memberInitials: 'JD', date: 'Jan 29, 2026', amount: 223, paymentMethod: 'Cash', status: 'Pledged' },
  { id: 'C-004', fundId: 'F-001', memberId: 'WF-412', memberName: 'David Okonjo', memberInitials: 'DO', date: 'Mar 25, 2026', amount: 43, paymentMethod: 'Card', status: 'Paid' },
  { id: 'C-005', fundId: 'F-001', memberId: 'WF-305', memberName: 'Habib', memberInitials: 'H', date: 'Nov 15, 2026', amount: 223, paymentMethod: 'Cash', status: 'Pledged' },
  { id: 'C-006', fundId: 'F-001', memberId: 'WF-552', memberName: 'Bubu Tunday', memberInitials: 'BT', date: 'Nov 15, 2026', amount: 1000, paymentMethod: 'Bank Transfer', status: 'Pledged' },
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

const formatCurrency = (amount: number) => `$ ${amount.toLocaleString('en-US')}`;

const getProgressPercent = (raised: number, target: number) =>
  target > 0 ? Math.min(Math.round((raised / target) * 100), 100) : 0;

// ─── Empty Forms ──────────────────────────────────────────────────────────────

const emptyContribution = () => ({
  member: '',
  amount: '',
  date: new Date().toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' }),
  paymentMethod: '',
  status: 'Paid' as ContributionStatus,
  notes: '',
});

// ─── Component ────────────────────────────────────────────────────────────────

const Contributions: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  // View state
  const [viewingFund, setViewingFund] = useState<Fund | null>(null);
  const [activeTab, setActiveTab] = useState<'Active Funds' | 'Completed' | 'All Funds'>('Active Funds');


  // Modals state
  const [showAddPanel, setShowAddPanel] = useState(false);
  const [editingRecord, setEditingRecord] = useState<ContributionRecord | null>(null);
  const [showExportModal, setShowExportModal] = useState(false);

  // Forms
  const [form, setForm] = useState(emptyContribution());

  // ── Layout Setup ──────────────────────────────────────────────────────────

  useEffect(() => {
    if (viewingFund) {
      setTitle(
        <button className="back-btn" onClick={() => setViewingFund(null)}>
          <ChevronLeftIcon size={20} />
          <span>Contributions / {viewingFund.name}</span>
        </button>
      );
      setCtas([
        { type: 'search', placeholder: 'Search Contributors...' },
        { type: 'button', label: 'Export', icon: 'export', variant: 'secondary', onClick: () => setShowExportModal(true) },
        { type: 'button', label: 'Add Entry', icon: 'plus', variant: 'primary', onClick: () => setShowAddPanel(true) },
      ]);
    } else {
      setTitle('Contributions');
      setCtas([
        { type: 'search', placeholder: 'Search Projects...' },
        { type: 'button', label: 'Export', icon: 'export', variant: 'secondary', onClick: () => setShowExportModal(true) },
      ]);
    }
  }, [viewingFund, setTitle, setCtas]);

  // ── Stats (List View) ──────────────────────────────────────────────────────

  const totalReceived = mockFunds.reduce((sum, f) => sum + f.raised, 0);
  const activeCount = mockFunds.filter(f => f.status === 'Active').length;
  const totalContributors = mockFunds.reduce((sum, f) => sum + f.contributorsCount, 0);
  const averageContrib = totalContributors > 0 ? totalReceived / totalContributors : 0;

  // ── Derived Data (Detail View) ─────────────────────────────────────────────

  // Note: in a real app, you'd filter by fundId. For now, just use mockRecords.
  const currentRecords = mockRecords;
  const detailTotalRaised = viewingFund ? viewingFund.raised : 0;
  const detailTarget = viewingFund ? viewingFund.target : 0;
  const detailContributors = viewingFund ? viewingFund.contributorsCount : 0;

  // ── Handlers ───────────────────────────────────────────────────────────────

  const handleOpenAdd = () => {
    setForm(emptyContribution());
    setShowAddPanel(true);
  };

  const handleOpenEdit = (record: ContributionRecord) => {
    setForm({
      member: record.memberName,
      amount: String(record.amount),
      date: record.date,
      paymentMethod: record.paymentMethod,
      status: record.status,
      notes: record.notes || '',
    });
    setEditingRecord(record);
  };

  const handleClosePanel = () => {
    setShowAddPanel(false);
    setEditingRecord(null);
  };

  const handleSave = () => {
    // Save logic goes here
    handleClosePanel();
  };

  // ── Render Form Fields ─────────────────────────────────────────────────────

  const renderFormFields = () => (
    <>
      {editingRecord ? (
        <div className="contrib-edit-member-card">
          <div className="contrib-edit-member-avatar">{editingRecord.memberInitials}</div>
          <div className="contrib-edit-member-info">
            <span className="contrib-edit-member-name">{editingRecord.memberName}</span>
            <span className="contrib-edit-member-id">ID: {editingRecord.memberId}</span>
          </div>
        </div>
      ) : (
        <div className="contrib-form-group">
          <label className="contrib-form-label">Member</label>
          <div className="contrib-form-input-icon-wrap">
            <input
              type="text"
              className="contrib-form-input"
              placeholder="Search member name or id..."
              value={form.member}
              onChange={e => setForm({ ...form, member: e.target.value })}
            />
            <SearchIcon size={16} className="contrib-form-input-icon" />
          </div>
        </div>
      )}

      <div className="contrib-form-group">
        <label className="contrib-form-label">Amount</label>
        <input
          type="text"
          className="contrib-form-input"
          placeholder="$ 0.00"
          value={form.amount}
          onChange={e => setForm({ ...form, amount: e.target.value.replace(/[^0-9.]/g, '') })}
        />
      </div>

      <div className="contrib-form-group">
        <label className="contrib-form-label">Date</label>
        <div className="contrib-form-input-icon-wrap">
          <input
            type="text"
            className="contrib-form-input"
            value={form.date}
            onChange={e => setForm({ ...form, date: e.target.value })}
          />
          <CalendarIcon size={16} className="contrib-form-input-icon" />
        </div>
      </div>

      <div className="contrib-form-group">
        <label className="contrib-form-label">Payment Method</label>
        <select
          className="contrib-form-select"
          value={form.paymentMethod}
          onChange={e => setForm({ ...form, paymentMethod: e.target.value })}
        >
          <option value="">Select Method</option>
          <option value="Cash">Cash</option>
          <option value="Card">Card</option>
          <option value="Bank Transfer">Bank Transfer</option>
          <option value="Mobile Money">Mobile Money</option>
          <option value="Cheque">Cheque</option>
        </select>
      </div>

      <div className="contrib-form-group">
        <label className="contrib-form-label">Status</label>
        <select
          className="contrib-form-select"
          value={form.status}
          onChange={e => setForm({ ...form, status: e.target.value as ContributionStatus })}
        >
          <option value="Paid">Paid</option>
          <option value="Pledged">Pledged</option>
        </select>
      </div>

      <div className="contrib-form-group">
        <label className="contrib-form-label">Notes (Optional)</label>
        <textarea
          className="contrib-form-textarea"
          placeholder="Any additional details here..."
          value={form.notes}
          onChange={e => setForm({ ...form, notes: e.target.value })}
        />
      </div>
    </>
  );

  // ── Main Render ────────────────────────────────────────────────────────────

  return (
    <div className="contributions-container">

      {/* ─── LIST VIEW ──────────────────────────────────────────────────────── */}
      {!viewingFund && (
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
              <span className="contributions-card-label">Total Contributors</span>
              <div className="contributions-card-value">{totalContributors}</div>
            </div>
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Average Contribution</span>
              <div className="contributions-card-value">{formatCurrency(Math.round(averageContrib))}</div>
            </div>
          </div>

          <div className="contributions-toolbar">
            <div className="contributions-tabs">
              {(['Active Funds', 'Completed', 'All Funds'] as const).map(tab => (
                <button
                  key={tab}
                  className={`contributions-tab ${activeTab === tab ? 'active' : ''}`}
                  onClick={() => setActiveTab(tab)}
                >
                  {tab}
                </button>
              ))}
            </div>
          </div>

          <div className="contributions-card-grid">
            {mockFunds.map(fund => (
              <div key={fund.id} className="fund-card">
                <div className="fund-card-header">
                  <h3 className="fund-card-title">{fund.name}</h3>
                  <span className={`fund-status-badge fund-status-${fund.status.toLowerCase()}`}>
                    {fund.status}
                  </span>
                </div>

                <div className="fund-card-progress-section">
                  <div className="fund-card-amounts">
                    <span className="fund-card-raised">{formatCurrency(fund.raised)}</span>
                    <span className="fund-card-target">of {formatCurrency(fund.target)} target</span>
                  </div>
                  <div className="fund-card-bar-track">
                    <div
                      className="fund-card-bar-fill"
                      style={{ width: `${getProgressPercent(fund.raised, fund.target)}%` }}
                    />
                  </div>
                </div>

                <div className="fund-card-meta">
                  <div className="fund-meta-item">
                    <MembersIcon size={14} />
                    <span>{fund.contributorsCount} contributors</span>
                  </div>
                  <div className="fund-meta-item">
                    <ClockIcon size={14} />
                    <span>Last contribution: {fund.lastContributionText}</span>
                  </div>
                </div>

                <div className="fund-card-actions">
                  <button className="fund-action-btn" onClick={() => setViewingFund(fund)}>
                    View Records
                  </button>
                  <button className="fund-action-btn primary" onClick={handleOpenAdd}>
                    Add Entry
                  </button>
                </div>
              </div>
            ))}
          </div>
        </>
      )}

      {/* ─── DETAIL VIEW ────────────────────────────────────────────────────── */}
      {viewingFund && (
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
              <span className="contributions-card-label">Contributors</span>
              <div className="contributions-card-value">{detailContributors}</div>
            </div>
            <div className="contributions-summary-card">
              <span className="contributions-card-label">Recent Entry</span>
              <div className="contributions-card-value">Today</div>
            </div>
          </div>

          <div className="detail-table-card">
            <div className="detail-table-header">
              <h3 className="detail-table-title">Contributors</h3>
            </div>
            <table className="contributions-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Member</th>
                  <th>Status</th>
                  <th>Amount</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {currentRecords.map(record => (
                  <tr key={record.id}>
                    <td>{record.date}</td>
                    <td>
                      <div className="member-cell">
                        <div className="member-avatar">{record.memberInitials}</div>
                        <span>{record.memberName}</span>
                      </div>
                    </td>
                    <td>
                      <span className={`status-cell-badge ${record.status === 'Pledged' ? 'pledged' : ''}`}>
                        {record.status}
                      </span>
                    </td>
                    <td className="amount-cell">{formatCurrency(record.amount)}</td>
                    <td>
                      <div className="actions-cell">
                        <button className="action-sm-btn" onClick={() => handleOpenEdit(record)}>Edit</button>
                        <button className="action-sm-btn">Delete</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {/* ─── ADD/EDIT SIDE PANELS ─────────────────────────────────────────── */}
      {(showAddPanel || editingRecord) && (
        <div className="contrib-panel-overlay" onClick={handleClosePanel}>
          <div className="contrib-side-panel" onClick={e => e.stopPropagation()}>
            <div className="contrib-panel-header">
              <h2 className="contrib-panel-title">
                {editingRecord ? 'Edit Contribution' : 'Add Contribution'}
              </h2>
              <button className="contrib-panel-close" onClick={handleClosePanel}>
                <CloseIcon />
              </button>
            </div>

            <div className="contrib-panel-body">
              {renderFormFields()}
            </div>

            <div className="contrib-panel-footer">
              <button className="fund-action-btn" onClick={handleClosePanel}>
                Cancel
              </button>
              <button className="fund-action-btn primary" onClick={handleSave}>
                Save Changes
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ─── EXPORT PANEL ───────────────────────────────────────────────────── */}
      {showExportModal && (
        <ExportPanel
          title="Export Contributions"
          exportConfig={{
            title: 'Contributions Report',
            filename: 'echo-contributions-report',
            columns: [
              { key: 'date', label: 'Date' },
              { key: 'memberName', label: 'Member' },
              { key: 'status', label: 'Status' },
              { key: 'amount', label: 'Amount' },
              { key: 'paymentMethod', label: 'Payment Method' },
            ],
            rows: currentRecords,
          }}
          onClose={() => setShowExportModal(false)}
        />
      )}

    </div>
  );
};

export default Contributions;
