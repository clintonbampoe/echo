import React, { useEffect, useMemo, useState } from 'react';
import { useLayout } from '../hooks/useLayout';
import {
  useTithes,
  useCreateTithe,
  useUpdateTithe,
  useDeleteTithe,
} from '../hooks/useTithes';
import { useMembers } from '../hooks/useMembers';
import type { Tithe as TitheItem, MonthOfYear, PaymentMethod } from '../types/finance';
import { RecordIcon, CloseIcon } from './Icons';
import DeleteConfirmModal from './common/DeleteConfirmModal';
import '../styles/Tithe.css';

const ITEMS_PER_PAGE = 10;

const MONTHS: MonthOfYear[] = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December'
];

interface FormState {
  memberId: string;
  amount: string;
  paymentMethod: PaymentMethod;
  forMonth: MonthOfYear;
  forYear: number;
  collectionDate: string;
  description: string;
}

const Tithe: React.FC = () => {
  const { setTitle, setCtas, searchQuery, setSearchQuery } = useLayout();

  const [selectedMonth, setSelectedMonth] = useState<string>('');
  const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());
  const [currentPage, setCurrentPage] = useState<number>(1);

  // ── Queries & Mutations ───────────────────────────────────────────────────
  const filters = useMemo(() => ({
    year: selectedYear,
    month: (selectedMonth as MonthOfYear) || undefined,
  }), [selectedYear, selectedMonth]);

  const { data: tithesData, isLoading: isLoadingTithes } = useTithes(filters);
  const records = useMemo(() => tithesData?.data || [], [tithesData?.data]);

  // Full year data for chart overview
  const { data: allYearTithesData } = useTithes({ year: selectedYear }, 500);
  const yearRecords = useMemo(() => allYearTithesData?.data || [], [allYearTithesData?.data]);

  const { data: membersResponse } = useMembers({}, 100);
  const members = useMemo(() => membersResponse?.data || [], [membersResponse?.data]);

  const createTithe = useCreateTithe();
  const updateTithe = useUpdateTithe();
  const deleteTithe = useDeleteTithe();

  // ── UI States ─────────────────────────────────────────────────────────────
  const [showModal, setShowModal] = useState(false);
  const [editingRecord, setEditingRecord] = useState<TitheItem | null>(null);

  const [formData, setFormData] = useState<FormState>({
    memberId: '',
    amount: '',
    paymentMethod: 'Cash',
    forMonth: MONTHS[new Date().getMonth()],
    forYear: new Date().getFullYear(),
    collectionDate: new Date().toISOString().split('T')[0],
    description: '',
  });

  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingRecord, setDeletingRecord] = useState<TitheItem | null>(null);

  const handleDelete = (record: TitheItem) => {
    setDeletingRecord(record);
    setShowDeleteConfirm(true);
  };

  const confirmDelete = async () => {
    if (!deletingRecord) return;
    try {
      await deleteTithe.mutateAsync(deletingRecord.id);
      setShowDeleteConfirm(false);
      setDeletingRecord(null);
    } catch (err) {
      console.error('Failed to delete tithe record:', err);
    }
  };

  const handleEdit = (record: TitheItem) => {
    setEditingRecord(record);
    setFormData({
      memberId: record.memberId,
      amount: String(record.amount),
      paymentMethod: record.paymentMethod,
      forMonth: record.forMonth,
      forYear: record.forYear,
      collectionDate: record.collectionDate.split('T')[0],
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

    try {
      if (editingRecord) {
        await updateTithe.mutateAsync({
          id: editingRecord.id,
          data: {
            memberId: formData.memberId,
            amount: parseFloat(formData.amount),
            paymentMethod: formData.paymentMethod,
            forMonth: formData.forMonth,
            forYear: parseInt(String(formData.forYear), 10),
            collectionDate: formData.collectionDate,
            description: formData.description.trim() || undefined,
          },
        });
      } else {
        await createTithe.mutateAsync({
          memberId: formData.memberId,
          amount: parseFloat(formData.amount),
          paymentMethod: formData.paymentMethod,
          forMonth: formData.forMonth,
          forYear: parseInt(String(formData.forYear), 10),
          collectionDate: formData.collectionDate,
          description: formData.description.trim() || undefined,
        });
      }

      setShowModal(false);
    } catch (err) {
      console.error('Failed to save tithe record:', err);
      alert('Failed to save tithe record.');
    }
  };

  // Filtered by layout search query
  const filteredRecords = useMemo(() => {
    if (!searchQuery.trim()) return records;
    const query = searchQuery.toLowerCase();
    return records.filter((r) => r.memberName?.toLowerCase().includes(query));
  }, [records, searchQuery]);

  // Pagination logic
  const totalPages = Math.max(1, Math.ceil(filteredRecords.length / ITEMS_PER_PAGE));
  const currentRecords = useMemo(() => {
    return filteredRecords.slice(
      (currentPage - 1) * ITEMS_PER_PAGE,
      currentPage * ITEMS_PER_PAGE
    );
  }, [filteredRecords, currentPage]);

  // Chart Logic (Monthly aggregation)
  const chartData = useMemo(() => {
    const monthlyTotals = new Map<string, number>();
    MONTHS.forEach((m) => monthlyTotals.set(m, 0));

    yearRecords.forEach((r) => {
      const current = monthlyTotals.get(r.forMonth) || 0;
      monthlyTotals.set(r.forMonth, current + r.amount);
    });

    const maxVal = Math.max(...Array.from(monthlyTotals.values()), 1);

    return MONTHS.map((m) => {
      const val = monthlyTotals.get(m) || 0;
      return {
        label: m.substring(0, 3),
        value: val,
        heightPercent: (val / maxVal) * 100,
      };
    });
  }, [yearRecords]);

  const formatCurrency = (amount: number): string => {
    return `₵ ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  };

  useEffect(() => {
    setTitle('Tithe');
    setCtas([
      { type: 'search', placeholder: 'Search tithes...' },
      {
        type: 'button',
        label: 'Record Tithe',
        icon: 'plus',
        variant: 'primary',
        onClick: handleOpenAddModal,
      },
    ]);
    setSearchQuery('');
  }, [setTitle, setCtas, setSearchQuery]);

  return (
    <div className="tithe-container">
      {/* ─── Filters ──────────────────────────────────────────────────────── */}
      <div className="tithe-filters-row">
        <div className="tithe-filter-group">
          <span className="tithe-filter-label">Year</span>
          <select
            className="tithe-select"
            value={selectedYear}
            onChange={(e) => {
              setSelectedYear(parseInt(e.target.value, 10));
              setCurrentPage(1);
            }}
          >
            {[2024, 2025, 2026, 2027, 2028].map((y) => (
              <option key={y} value={y}>{y}</option>
            ))}
          </select>
        </div>

        <div className="tithe-filter-group">
          <span className="tithe-filter-label">Month</span>
          <select
            className="tithe-select"
            value={selectedMonth}
            onChange={(e) => {
              setSelectedMonth(e.target.value);
              setCurrentPage(1);
            }}
          >
            <option value="">All Months</option>
            {MONTHS.map((m) => (
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
          {isLoadingTithes ? (
            <div className="tithe-table-empty">Loading tithe records...</div>
          ) : currentRecords.length === 0 ? (
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
                {currentRecords.map((record) => (
                  <tr key={record.id}>
                    <td className="tithe-member-name">{record.memberName || 'Member'}</td>
                    <td className="tithe-amount-cell">{formatCurrency(record.amount)}</td>
                    <td>{record.forMonth} {record.forYear}</td>
                    <td>{new Date(record.collectionDate).toLocaleDateString()}</td>
                    <td>{record.paymentMethod}</td>
                    <td>
                      <div className="tithe-table-actions">
                        <button
                          type="button"
                          className="action-btn"
                          onClick={() => handleEdit(record)}
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          className="action-btn delete"
                          onClick={() => handleDelete(record)}
                        >
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
              type="button"
              className="pagination-btn"
              disabled={currentPage === 1}
              onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
            >
              Previous
            </button>
            <span className="pagination-info">
              Page {currentPage} of {totalPages}
            </span>
            <button
              type="button"
              className="pagination-btn"
              disabled={currentPage === totalPages}
              onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            >
              Next
            </button>
          </div>
        )}
      </div>

      {/* ─── Add / Edit Modal ─────────────────────────────────────────────── */}
      {showModal && (
        <div className="tithe-panel-overlay" onClick={() => setShowModal(false)}>
          <form
            className="tithe-side-panel"
            onClick={(e) => e.stopPropagation()}
            onSubmit={handleSaveTithe}
          >
            <div className="tithe-panel-header">
              <h2 className="tithe-panel-title">
                {editingRecord ? 'Edit Tithe Record' : 'Record Tithe'}
              </h2>
              <button
                type="button"
                className="tithe-panel-close"
                onClick={() => setShowModal(false)}
                aria-label="Close panel"
              >
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
                  onChange={(e) => setFormData((prev) => ({ ...prev, memberId: e.target.value }))}
                  required
                >
                  <option value="">Select Member...</option>
                  {members.map((m) => {
                    const displayName = m.name || `${m.firstName} ${m.lastName}`;
                    return (
                      <option key={m.id} value={m.id}>
                        {displayName}
                      </option>
                    );
                  })}
                </select>
              </div>

              {/* Amount & Payment Method */}
              <div className="tithe-form-row">
                <div className="tithe-form-group">
                  <label className="tithe-form-label">Amount</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0.01"
                    className="tithe-form-input"
                    placeholder="₵ 0.00"
                    value={formData.amount}
                    onChange={(e) => setFormData((prev) => ({ ...prev, amount: e.target.value }))}
                    required
                  />
                </div>
                <div className="tithe-form-group">
                  <label className="tithe-form-label">Payment Method</label>
                  <select
                    className="tithe-form-select"
                    value={formData.paymentMethod}
                    onChange={(e) => setFormData((prev) => ({ ...prev, paymentMethod: e.target.value as PaymentMethod }))}
                    required
                  >
                    <option value="Cash">Cash</option>
                    <option value="Cheque">Cheque</option>
                    <option value="CreditCard">Credit Card</option>
                    <option value="MobileMoney">Mobile Money</option>
                    <option value="BankTransfer">Bank Transfer</option>
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
                    onChange={(e) => setFormData((prev) => ({ ...prev, forMonth: e.target.value as MonthOfYear }))}
                    required
                  >
                    {MONTHS.map((m) => (
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
                    onChange={(e) => setFormData((prev) => ({ ...prev, forYear: parseInt(e.target.value, 10) || new Date().getFullYear() }))}
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
                  onChange={(e) => setFormData((prev) => ({ ...prev, collectionDate: e.target.value }))}
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
                  onChange={(e) => setFormData((prev) => ({ ...prev, description: e.target.value }))}
                />
              </div>
            </div>

            <div className="tithe-panel-footer">
              <button
                type="button"
                className="tithe-btn tithe-btn-secondary"
                onClick={() => setShowModal(false)}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="tithe-btn tithe-btn-primary"
                disabled={createTithe.isPending || updateTithe.isPending}
              >
                <RecordIcon />{' '}
                {createTithe.isPending || updateTithe.isPending
                  ? 'Saving...'
                  : editingRecord
                  ? 'Save Changes'
                  : 'Record Tithe'}
              </button>
            </div>
          </form>
        </div>
      )}

      <DeleteConfirmModal
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={confirmDelete}
        itemName={deletingRecord?.memberName ? `Tithe for ${deletingRecord.memberName}` : 'Tithe Record'}
        title="Delete Tithe Record"
        confirmText="Delete Record"
      />
    </div>
  );
};

export default Tithe;
