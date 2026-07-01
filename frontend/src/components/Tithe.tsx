import React, { useEffect, useState } from 'react';
import { useLayout } from '../context/LayoutContext';
import { deleteTitheRecord, getTitheRecords, saveTitheRecord } from '../services/titheService';
import { getMembers } from '../services/attendanceService';
import type { TitheRecord } from '../types/tithe';
import type { Member } from '../types/attendance';
import { CloseIcon, RecordIcon } from './Icons';
import '../styles/Tithe.css';

const ITEMS_PER_PAGE = 5;

const MONTHS = [
  'January', 'February', 'March', 'April', 'May', 'June', 
  'July', 'August', 'September', 'October', 'November', 'December'
];

const Tithe: React.FC = () => {
  const { setTitle, setCtas } = useLayout();
  
  const [records, setRecords] = useState<TitheRecord[]>([]);
  const [selectedMonth, setSelectedMonth] = useState<string>('');
  const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());
  const [currentPage, setCurrentPage] = useState<number>(1);
  
  const [members, setMembers] = useState<Member[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [editingRecord, setEditingRecord] = useState<TitheRecord | null>(null);

  const [formData, setFormData] = useState({
    memberId: '',
    amount: '',
    paymentMethod: 'Cash',
    forMonth: MONTHS[new Date().getMonth()],
    forYear: new Date().getFullYear(),
    collectionDate: new Date().toISOString().split('T')[0],
    description: '',
  });

  useEffect(() => {
    let mounted = true;
    const loadData = async () => {
      try {
        const data = await getTitheRecords(selectedMonth || undefined, selectedYear || undefined);
        const membersData = await getMembers();
        if (mounted) {
          setRecords(data);
          setMembers(membersData);
          setCurrentPage(1); // reset pagination when filters change
        }
      } catch (err) {
        console.error('Failed to load tithe records:', err);
      }
    };
    loadData();
    return () => { mounted = false; };
  }, [selectedMonth, selectedYear]);

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this tithe record?')) {
      await deleteTitheRecord(id);
      const data = await getTitheRecords(selectedMonth || undefined, selectedYear || undefined);
      setRecords(data);
    }
  };

  const handleEdit = (record: TitheRecord) => {
    setEditingRecord(record);
    setFormData({
      memberId: String(record.memberId),
      amount: String(record.amount),
      paymentMethod: record.paymentMethod,
      forMonth: record.forMonth,
      forYear: record.forYear,
      collectionDate: record.collectionDate,
      description: record.description || '',
    });
    setShowModal(true);
  };

  const handleOpenAddModal = () => {
    setEditingRecord(null);
    setFormData({
      memberId: '',
      amount: '',
      paymentMethod: 'Cash',
      forMonth: MONTHS[new Date().getMonth()],
      forYear: new Date().getFullYear(),
      collectionDate: new Date().toISOString().split('T')[0],
      description: '',
    });
    setShowModal(true);
  };

  const handleSaveTithe = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.memberId || !formData.amount) {
      alert('Please select a member and enter an amount.');
      return;
    }

    const member = members.find(m => m.memberId === parseInt(formData.memberId));

    try {
      await saveTitheRecord({
        titheId: editingRecord?.titheId,
        memberId: parseInt(formData.memberId),
        amount: parseFloat(formData.amount),
        paymentMethod: formData.paymentMethod as any,
        forMonth: formData.forMonth as any,
        forYear: parseInt(String(formData.forYear)),
        collectionDate: formData.collectionDate,
        description: formData.description,
        memberName: member ? `${member.firstName} ${member.lastName}` : undefined,
      });

      setShowModal(false);
      const data = await getTitheRecords(selectedMonth || undefined, selectedYear || undefined);
      setRecords(data);
    } catch (err) {
      console.error('Failed to save tithe record:', err);
    }
  };

  // Pagination logic
  const totalPages = Math.ceil(records.length / ITEMS_PER_PAGE);
  const currentRecords = records.slice(
    (currentPage - 1) * ITEMS_PER_PAGE,
    currentPage * ITEMS_PER_PAGE
  );

  // Chart Logic (Mock data aggregation by month for the chart if no specific month is selected)
  // Or just show daily aggregation if month is selected
  // For simplicity of a beautiful bar chart, we'll aggregate by month for the given year
  const [chartData, setChartData] = useState<{label: string, value: number, heightPercent: number}[]>([]);

  useEffect(() => {
    // We'll quickly fetch all data for the year to build the chart, bypassing month filter
    const buildChart = async () => {
      const yearData = await getTitheRecords(undefined, selectedYear);
      
      const monthlyTotals = new Map<string, number>();
      MONTHS.forEach(m => monthlyTotals.set(m, 0));
      
      yearData.forEach(r => {
        const current = monthlyTotals.get(r.forMonth) || 0;
        monthlyTotals.set(r.forMonth, current + r.amount);
      });

      const maxVal = Math.max(...Array.from(monthlyTotals.values()), 1); // avoid div by 0

      const mapped = MONTHS.map(m => {
        const val = monthlyTotals.get(m) || 0;
        return {
          label: m.substring(0, 3), // Jan, Feb
          value: val,
          heightPercent: (val / maxVal) * 100
        };
      });
      
      setChartData(mapped);
    };

    buildChart();
  }, [selectedYear, records]); // also update if records change (add/delete)

  const formatCurrency = (amount: number): string => {
    return `₵ ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  };

  useEffect(() => {
    setTitle('Tithe');
    setCtas([
      { type: 'search', placeholder: 'Search members...' },
      {
        type: 'button',
        label: 'Record Tithe',
        icon: 'plus',
        variant: 'primary',
        onClick: handleOpenAddModal,
      },
    ]);
  }, [setTitle, setCtas]);

  return (
    <div className="tithe-container">
      {/* ─── Filters ──────────────────────────────────────────────────────── */}
      <div className="tithe-filters-row">
        <div className="tithe-filter-group">
          <span className="tithe-filter-label">Year</span>
          <select 
            className="tithe-select" 
            value={selectedYear}
            onChange={e => setSelectedYear(parseInt(e.target.value))}
          >
            {[2024, 2025, 2026, 2027].map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </select>
        </div>

        <div className="tithe-filter-group">
          <span className="tithe-filter-label">Month</span>
          <select 
            className="tithe-select"
            value={selectedMonth}
            onChange={e => setSelectedMonth(e.target.value)}
          >
            <option value="">All Months</option>
            {MONTHS.map(m => (
              <option key={m} value={m}>{m}</option>
            ))}
          </select>
        </div>
      </div>

      {/* ─── Chart Section ────────────────────────────────────────────────── */}
      <div className="tithe-chart-card">
        <div className="tithe-chart-header">
          <h3 className="tithe-chart-title">Tithe Contributions Overview</h3>
          <div className="tithe-chart-subtitle">Monthly aggregate for {selectedYear}</div>
        </div>
        
        <div className="tithe-chart-container">
          {chartData.map((data, idx) => (
            <div key={idx} className="tithe-chart-bar-wrapper">
              <div 
                className="tithe-chart-bar" 
                style={{ height: `${data.heightPercent}%`, minHeight: data.value > 0 ? '4px' : '0' }}
              />
              <div className="tithe-chart-label">{data.label}</div>
              <div className="tithe-chart-tooltip">
                {data.label}: {formatCurrency(data.value)}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* ─── Table Section ────────────────────────────────────────────────── */}
      <div className="tithe-table-card">
        <div className="tithe-table-header">
          <h3 className="tithe-table-title">Recent Tithes</h3>
        </div>
        
        <div className="tithe-table-container">
          {currentRecords.length === 0 ? (
            <div className="tithe-table-empty">No tithe records found for the selected period.</div>
          ) : (
            <table className="tithe-table">
              <thead>
                <tr>
                  <th>Member</th>
                  <th>Amount</th>
                  <th>For Period</th>
                  <th>Date Paid</th>
                  <th>Payment Method</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {currentRecords.map(record => (
                  <tr key={record.titheId}>
                    <td className="tithe-member-name">{record.memberName || `Member #${record.memberId}`}</td>
                    <td className="tithe-amount-cell">{formatCurrency(record.amount)}</td>
                    <td>{record.forMonth} {record.forYear}</td>
                    <td>{new Date(record.collectionDate).toLocaleDateString()}</td>
                    <td>{record.paymentMethod}</td>
                    <td>
                      <div className="tithe-table-actions">
                        <button className="action-btn" onClick={() => handleEdit(record)}>
                          Edit
                        </button>
                        <button className="action-btn delete" onClick={() => handleDelete(record.titheId)}>
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="tithe-pagination">
            <button 
              className="pagination-btn"
              disabled={currentPage === 1}
              onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
            >
              Previous
            </button>
            <span className="pagination-info">
              Page {currentPage} of {totalPages}
            </span>
            <button 
              className="pagination-btn"
              disabled={currentPage === totalPages}
              onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
            >
              Next
            </button>
          </div>
        )}
      </div>

      {/* ─── Add / Edit Modal ─────────────────────────────────────────────── */}
      {showModal && (
        <div className="tithe-panel-overlay" onClick={() => setShowModal(false)}>
          <form className="tithe-side-panel" onClick={e => e.stopPropagation()} onSubmit={handleSaveTithe}>
            <div className="tithe-panel-header">
              <h2 className="tithe-panel-title">
                {editingRecord ? 'Edit Tithe Record' : 'Record Tithe'}
              </h2>
              <button type="button" className="tithe-panel-close" onClick={() => setShowModal(false)}>
                <CloseIcon />
              </button>
            </div>

            <div className="tithe-panel-body">
              {/* Member Selection */}
              <div className="tithe-form-group">
                <label className="tithe-form-label">Member</label>
                <select
                  className="tithe-form-select"
                  value={formData.memberId}
                  onChange={e => setFormData(prev => ({ ...prev, memberId: e.target.value }))}
                  required
                >
                  <option value="">Select Member...</option>
                  {members.map(m => (
                    <option key={m.memberId} value={m.memberId}>
                      {m.firstName} {m.lastName}
                    </option>
                  ))}
                </select>
              </div>

              {/* Amount & Payment Method */}
              <div className="tithe-form-row">
                <div className="tithe-form-group">
                  <label className="tithe-form-label">Amount</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    className="tithe-form-input"
                    placeholder="₵ 0.00"
                    value={formData.amount}
                    onChange={e => setFormData(prev => ({ ...prev, amount: e.target.value }))}
                    required
                  />
                </div>
                <div className="tithe-form-group">
                  <label className="tithe-form-label">Payment Method</label>
                  <select
                    className="tithe-form-select"
                    value={formData.paymentMethod}
                    onChange={e => setFormData(prev => ({ ...prev, paymentMethod: e.target.value }))}
                    required
                  >
                    <option value="Cash">Cash</option>
                    <option value="Cheque">Cheque</option>
                    <option value="CreditCard">Credit Card</option>
                    <option value="MobileMoney">Mobile Money</option>
                  </select>
                </div>
              </div>

              {/* For Month & Year */}
              <div className="tithe-form-row">
                <div className="tithe-form-group">
                  <label className="tithe-form-label">For Month</label>
                  <select
                    className="tithe-form-select"
                    value={formData.forMonth}
                    onChange={e => setFormData(prev => ({ ...prev, forMonth: e.target.value }))}
                    required
                  >
                    {MONTHS.map(m => (
                      <option key={m} value={m}>{m}</option>
                    ))}
                  </select>
                </div>
                <div className="tithe-form-group">
                  <label className="tithe-form-label">For Year</label>
                  <input
                    type="number"
                    className="tithe-form-input"
                    value={formData.forYear}
                    onChange={e => setFormData(prev => ({ ...prev, forYear: parseInt(e.target.value) || new Date().getFullYear() }))}
                    required
                  />
                </div>
              </div>

              {/* Collection Date */}
              <div className="tithe-form-group">
                <label className="tithe-form-label">Date Paid</label>
                <input
                  type="date"
                  className="tithe-form-input"
                  value={formData.collectionDate}
                  onChange={e => setFormData(prev => ({ ...prev, collectionDate: e.target.value }))}
                  required
                />
              </div>

              {/* Notes */}
              <div className="tithe-form-group">
                <label className="tithe-form-label">Notes <span>(Optional)</span></label>
                <textarea
                  className="tithe-form-textarea"
                  placeholder="Additional details..."
                  value={formData.description}
                  onChange={e => setFormData(prev => ({ ...prev, description: e.target.value }))}
                />
              </div>
            </div>

            <div className="tithe-panel-footer">
              <button type="button" className="tithe-btn tithe-btn-secondary" onClick={() => setShowModal(false)}>
                Cancel
              </button>
              <button type="submit" className="tithe-btn tithe-btn-primary">
                <RecordIcon /> {editingRecord ? 'Save Changes' : 'Record Tithe'}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};

export default Tithe;
