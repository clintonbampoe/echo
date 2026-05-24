import React, { useEffect, useState, useRef } from 'react';
import { useLayout } from '../context/LayoutContext';
import '../styles/Finance.css';
import type {
  TransactionCategory,
  FinancialTransaction,
  CategoryStream,
  TransactionType,
  PaymentMethod,
  RecordTransactionForm,
  ExportReportForm,
  CategoryForm,
} from '../types/finance';

// ─── Mock Data ───────────────────────────────────────────────────────────────

const mockCategories: TransactionCategory[] = [
  { categoryId: 1, uniqueId: 'cat-001', name: 'Offerings', categoryType: 'Income' },
  { categoryId: 2, uniqueId: 'cat-002', name: 'Venue Rentals', categoryType: 'Income' },
  { categoryId: 3, uniqueId: 'cat-003', name: 'Special Donations', categoryType: 'Income' },
  { categoryId: 4, uniqueId: 'cat-004', name: 'Bookstore Sales', categoryType: 'Income' },
  { categoryId: 5, uniqueId: 'cat-005', name: 'Staff Salaries', categoryType: 'Expenditure' },
  { categoryId: 6, uniqueId: 'cat-006', name: 'Utilities & Maintenance', categoryType: 'Expenditure' },
  { categoryId: 7, uniqueId: 'cat-007', name: 'Outreach & Missions', categoryType: 'Expenditure' },
  { categoryId: 8, uniqueId: 'cat-008', name: 'Tech & Media', categoryType: 'Expenditure' },
  { categoryId: 9, uniqueId: 'cat-009', name: 'Hospitality', categoryType: 'Expenditure' },
  { categoryId: 10, uniqueId: 'cat-010', name: 'Transportation', categoryType: 'Expenditure' },
];

const mockTransactions: FinancialTransaction[] = [
  { transactionId: 1, uniqueId: 'txn-001', categoryId: 1, transactionType: 'Income', transactionDate: '2026-05-01', amount: 12400, description: 'Sunday offering', paymentMethod: 'Cash', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 2, uniqueId: 'txn-002', categoryId: 1, transactionType: 'Income', transactionDate: '2026-05-08', amount: 10000, description: 'Sunday offering', paymentMethod: 'Cash', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 3, uniqueId: 'txn-003', categoryId: 1, transactionType: 'Income', transactionDate: '2026-05-15', amount: 10000, description: null, paymentMethod: 'MobileMoney', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 4, uniqueId: 'txn-004', categoryId: 2, transactionType: 'Income', transactionDate: '2026-05-03', amount: 25000, description: 'Hall rental – wedding', paymentMethod: 'CreditCard', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 5, uniqueId: 'txn-005', categoryId: 2, transactionType: 'Income', transactionDate: '2026-05-10', amount: 26000, description: 'Conference room', paymentMethod: 'CreditCard', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 6, uniqueId: 'txn-006', categoryId: 3, transactionType: 'Income', transactionDate: '2026-05-05', amount: 16598, description: 'Anonymous donation', paymentMethod: 'CreditCard', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 7, uniqueId: 'txn-007', categoryId: 4, transactionType: 'Income', transactionDate: '2026-05-12', amount: 2009, description: null, paymentMethod: 'Cash', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 8, uniqueId: 'txn-008', categoryId: 5, transactionType: 'Expenditure', transactionDate: '2026-05-01', amount: 18400, description: 'Monthly salaries', paymentMethod: 'CreditCard', isRecurring: true, recurrenceType: 'monthly' },
  { transactionId: 9, uniqueId: 'txn-009', categoryId: 6, transactionType: 'Expenditure', transactionDate: '2026-05-02', amount: 4500, description: 'Electricity and water', paymentMethod: 'CreditCard', isRecurring: true, recurrenceType: 'monthly' },
  { transactionId: 10, uniqueId: 'txn-010', categoryId: 7, transactionType: 'Expenditure', transactionDate: '2026-05-06', amount: 6000, description: 'Community outreach', paymentMethod: 'Cash', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 11, uniqueId: 'txn-011', categoryId: 8, transactionType: 'Expenditure', transactionDate: '2026-05-09', amount: 2200, description: 'Streaming equipment', paymentMethod: 'CreditCard', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 12, uniqueId: 'txn-012', categoryId: 9, transactionType: 'Expenditure', transactionDate: '2026-05-04', amount: 5500, description: 'Fellowship refreshments', paymentMethod: 'Cash', isRecurring: true, recurrenceType: 'weekly' },
  { transactionId: 13, uniqueId: 'txn-013', categoryId: 10, transactionType: 'Expenditure', transactionDate: '2026-05-07', amount: 2400, description: 'Bus rental for outreach', paymentMethod: 'CreditCard', isRecurring: true, recurrenceType: 'weekly' },
];

// ─── Helper: format currency ─────────────────────────────────────────────────

const formatCurrency = (amount: number): string => {
  return `$ ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
};

// ─── Helper: format recurrence ───────────────────────────────────────────────

const formatRecurrence = (type?: string): string => {
  const labels: Record<string, string> = {
    one_time: 'One-time',
    weekly: 'Weekly',
    bi_weekly: 'Bi-Weekly',
    monthly: 'Monthly',
    quarterly: 'Quarterly',
    annually: 'Annually',
  };
  return labels[type || 'one_time'] || 'One-time';
};

// ─── Helper: format payment method for display ───────────────────────────────

const paymentMethodLabels: Record<PaymentMethod, string> = {
  Cash: 'Cash',
  Cheque: 'Cheque',
  CreditCard: 'Credit Card',
  MobileMoney: 'Mobile Money',
};

// ─── SVG Icons (inline, matching existing patterns) ──────────────────────────

const IconClose = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
  </svg>
);

const IconMoreVertical = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="5" r="1" /><circle cx="12" cy="12" r="1" /><circle cx="12" cy="19" r="1" />
  </svg>
);

const IconEdit = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
  </svg>
);

const IconTrash = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="3 6 5 6 21 6" /><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
  </svg>
);

const IconWarning = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
    <line x1="12" y1="9" x2="12" y2="13" /><line x1="12" y1="17" x2="12.01" y2="17" />
  </svg>
);

const IconTrendUp = () => (
  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="23 6 13.5 15.5 8.5 10.5 1 18" />
    <polyline points="17 6 23 6 23 12" />
  </svg>
);

const IconInfo = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10" /><line x1="12" y1="16" x2="12" y2="12" /><line x1="12" y1="8" x2="12.01" y2="8" />
  </svg>
);


const IconDocument = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
    <polyline points="14 2 14 8 20 8" /><line x1="16" y1="13" x2="8" y2="13" /><line x1="16" y1="17" x2="8" y2="17" />
  </svg>
);

const IconTable = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="3" width="18" height="18" rx="2" /><line x1="3" y1="9" x2="21" y2="9" /><line x1="3" y1="15" x2="21" y2="15" /><line x1="9" y1="3" x2="9" y2="21" /><line x1="15" y1="3" x2="15" y2="21" />
  </svg>
);

const IconCsv = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="3" width="18" height="18" rx="2" />
    <text x="12" y="15" textAnchor="middle" fontSize="7" fontWeight="700" fill="currentColor" stroke="none">CSV</text>
  </svg>
);

const IconRecord = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10" /><circle cx="12" cy="12" r="3" />
  </svg>
);

// ─── Finance Page Component ──────────────────────────────────────────────────

const Finance: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  // ── State ──────────────────────────────────────────────────────────────────

  const [categories, setCategories] = useState<TransactionCategory[]>(mockCategories);
  const [transactions, setTransactions] = useState<FinancialTransaction[]>(mockTransactions);

  // Modals
  const [showRecordModal, setShowRecordModal] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [showCategoryModal, setShowCategoryModal] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  // Edit/Delete state
  const [editingCategory, setEditingCategory] = useState<TransactionCategory | null>(null);
  const [deletingCategory, setDeletingCategory] = useState<TransactionCategory | null>(null);
  const [categoryModalType, setCategoryModalType] = useState<TransactionType>('Income');

  // Dropdown menu
  const [activeMenuId, setActiveMenuId] = useState<string | null>(null);
  const menuRef = useRef<HTMLDivElement>(null);

  // Record Transaction form
  const [recordForm, setRecordForm] = useState<RecordTransactionForm>({
    transactionType: 'Income',
    amount: '',
    categoryId: '',
    transactionDate: new Date().toISOString().split('T')[0],
    paymentMethod: 'Cash',
    description: '',
  });

  // Export Report form
  const [exportForm, setExportForm] = useState<ExportReportForm>({
    dateRange: 'last_30',
    reportType: 'full',
    exportFormat: 'pdf',
  });

  // Category form
  const [categoryForm, setCategoryForm] = useState<CategoryForm>({
    name: '',
    categoryType: 'Income',
  });

  // ── Layout Setup ───────────────────────────────────────────────────────────

  useEffect(() => {
    setTitle('Finances');
    setCtas([
      {
        type: 'button',
        label: 'Export Report',
        icon: 'export',
        variant: 'secondary',
        onClick: () => setShowExportModal(true),
      },
      {
        type: 'button',
        label: 'New Transaction',
        icon: 'plus',
        variant: 'primary',
        onClick: () => {
          setRecordForm({
            transactionType: 'Income',
            amount: '',
            categoryId: '',
            transactionDate: new Date().toISOString().split('T')[0],
            paymentMethod: 'Cash',
            description: '',
          });
          setShowRecordModal(true);
        },
      },
    ]);
  }, [setTitle, setCtas]);

  // Close dropdown on outside click
  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setActiveMenuId(null);
      }
    };
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  // ── Computed Data ──────────────────────────────────────────────────────────


  const getCategoryStreams = (type: TransactionType): CategoryStream[] => {
    const typeCats = categories.filter(c => c.categoryType === type);
    const typeTransactions = transactions.filter(t => t.transactionType === type);
    const totalForType = typeTransactions.reduce((sum, t) => sum + t.amount, 0);

    return typeCats.map(cat => {
      const catTransactions = typeTransactions.filter(t => t.categoryId === cat.categoryId);
      const total = catTransactions.reduce((sum, t) => sum + t.amount, 0);
      const latestTxn = catTransactions[0];

      return {
        category: cat,
        totalAmount: total,
        percentOfTotal: totalForType > 0 ? Math.round((total / totalForType) * 100) : 0,
        isRecurring: latestTxn?.isRecurring ?? false,
        recurrenceLabel: latestTxn?.isRecurring
          ? `Recurring · ${formatRecurrence(latestTxn.recurrenceType)}`
          : 'One-time',
        transactionCount: catTransactions.length,
      };
    });
  };

  const incomeStreams = getCategoryStreams('Income');
  const expenditureStreams = getCategoryStreams('Expenditure');

  const totalIncome = transactions
    .filter(t => t.transactionType === 'Income')
    .reduce((sum, t) => sum + t.amount, 0);

  const totalExpenditure = transactions
    .filter(t => t.transactionType === 'Expenditure')
    .reduce((sum, t) => sum + t.amount, 0);

  const netBalance = totalIncome - totalExpenditure;

  const incomeCount = transactions.filter(t => t.transactionType === 'Income').length;
  const expenditureCount = transactions.filter(t => t.transactionType === 'Expenditure').length;

  // ── Handlers ───────────────────────────────────────────────────────────────

  const handleAddCategory = (type: TransactionType) => {
    setCategoryModalType(type);
    setEditingCategory(null);
    setCategoryForm({ name: '', categoryType: type });
    setShowCategoryModal(true);
  };

  const handleEditCategory = (cat: TransactionCategory) => {
    setActiveMenuId(null);
    setEditingCategory(cat);
    setCategoryModalType(cat.categoryType);
    setCategoryForm({ name: cat.name, categoryType: cat.categoryType });
    setShowCategoryModal(true);
  };

  const handleDeleteCategory = (cat: TransactionCategory) => {
    setActiveMenuId(null);
    setDeletingCategory(cat);
    setShowDeleteConfirm(true);
  };

  const confirmDeleteCategory = () => {
    if (!deletingCategory) return;
    // Remove category
    setCategories(prev => prev.filter(c => c.categoryId !== deletingCategory.categoryId));
    // Set categoryId to null for orphaned transactions
    setTransactions(prev =>
      prev.map(t =>
        t.categoryId === deletingCategory.categoryId ? { ...t, categoryId: null } : t
      )
    );
    setShowDeleteConfirm(false);
    setDeletingCategory(null);
  };

  const handleSaveCategory = () => {
    if (!categoryForm.name.trim()) return;

    if (editingCategory) {
      // Update existing
      setCategories(prev =>
        prev.map(c =>
          c.categoryId === editingCategory.categoryId
            ? { ...c, name: categoryForm.name.trim() }
            : c
        )
      );
    } else {
      // Create new
      const newId = Math.max(...categories.map(c => c.categoryId), 0) + 1;
      const newCat: TransactionCategory = {
        categoryId: newId,
        uniqueId: `cat-${String(newId).padStart(3, '0')}`,
        name: categoryForm.name.trim(),
        categoryType: categoryModalType,
      };
      setCategories(prev => [...prev, newCat]);
    }

    setShowCategoryModal(false);
    setEditingCategory(null);
  };

  const handleRecordTransaction = () => {
    if (!recordForm.amount || !recordForm.categoryId) return;

    const newId = Math.max(...transactions.map(t => t.transactionId), 0) + 1;
    const newTxn: FinancialTransaction = {
      transactionId: newId,
      uniqueId: `txn-${String(newId).padStart(3, '0')}`,
      categoryId: parseInt(recordForm.categoryId),
      transactionType: recordForm.transactionType,
      transactionDate: recordForm.transactionDate,
      amount: parseFloat(recordForm.amount.replace(/,/g, '')),
      description: recordForm.description || null,
      paymentMethod: recordForm.paymentMethod,
      isRecurring: false,
      recurrenceType: 'one_time',
    };
    setTransactions(prev => [...prev, newTxn]);
    setShowRecordModal(false);
  };

  const handleExportReport = () => {
    // UI-only — no backend yet
    console.log('Export report:', exportForm);
    setShowExportModal(false);
  };

  // ── Filter categories for the record modal based on selected type ──────────

  const recordCategories = categories.filter(
    c => c.categoryType === recordForm.transactionType
  );

  // ── Render ─────────────────────────────────────────────────────────────────

  return (
    <div className="finance-container">
      {/* ─── Summary Cards ──────────────────────────────────────────────────── */}
      <div className="finance-summary-cards">
        <div className="finance-summary-card">
          <span className="finance-card-label">Total Monthly Income</span>
          <div className="finance-card-value">{formatCurrency(totalIncome)}</div>
          <div className="finance-card-trend">
            <IconTrendUp />
            +{incomeCount} this month
          </div>
        </div>

        <div className="finance-summary-card">
          <span className="finance-card-label">Total Expenditure</span>
          <div className="finance-card-value">{formatCurrency(totalExpenditure)}</div>
          <div className="finance-card-trend">
            <IconTrendUp />
            +{expenditureCount} this month
          </div>
        </div>

        <div className="finance-summary-card">
          <span className="finance-card-label">Net Balance</span>
          <div className="finance-card-value">{formatCurrency(netBalance)}</div>
          <div className="finance-card-trend">
            <IconTrendUp />
            +{incomeCount + expenditureCount} this month
          </div>
        </div>
      </div>

      {/* ─── Streams Grid (side-by-side) ────────────────────────────────────── */}
      <div className="finance-streams-grid">
        {/* Income Column */}
        <div className="finance-streams-column">
          <div className="stream-header">
            <div className="stream-header-left">
              <div className="stream-header-icon" />
              <h3 className="stream-header-title">Income Streams</h3>
            </div>
            <button className="stream-add-btn" onClick={() => handleAddCategory('Income')}>
              Add Stream
            </button>
          </div>

          {incomeStreams.length === 0 && (
            <div className="stream-empty">No income streams yet. Add one to get started.</div>
          )}

          {incomeStreams.map(stream => (
            <div className="stream-card" key={stream.category.uniqueId}>
              <div className="stream-card-icon" />
              <div className="stream-card-info">
                <div className="stream-card-name">{stream.category.name}</div>
                <div className="stream-card-meta">{stream.recurrenceLabel}</div>
              </div>
              <div className="stream-card-right">
                <div className="stream-card-amount">{formatCurrency(stream.totalAmount)}</div>
                <div className="stream-card-percent">{stream.percentOfTotal}% of total</div>
              </div>

              {/* Three-dot menu */}
              <button
                className="stream-card-menu-trigger"
                onClick={(e) => {
                  e.stopPropagation();
                  setActiveMenuId(
                    activeMenuId === stream.category.uniqueId ? null : stream.category.uniqueId
                  );
                }}
              >
                <IconMoreVertical />
              </button>

              {activeMenuId === stream.category.uniqueId && (
                <div className="stream-card-dropdown" ref={menuRef}>
                  <button
                    className="stream-card-dropdown-item"
                    onClick={() => handleEditCategory(stream.category)}
                  >
                    <IconEdit /> Edit
                  </button>
                  <button
                    className="stream-card-dropdown-item danger"
                    onClick={() => handleDeleteCategory(stream.category)}
                  >
                    <IconTrash /> Delete
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>

        {/* Expenditure Column */}
        <div className="finance-streams-column">
          <div className="stream-header">
            <div className="stream-header-left">
              <div className="stream-header-icon" />
              <h3 className="stream-header-title">Expenditure Streams</h3>
            </div>
            <button className="stream-add-btn" onClick={() => handleAddCategory('Expenditure')}>
              Add Stream
            </button>
          </div>

          {expenditureStreams.length === 0 && (
            <div className="stream-empty">No expenditure streams yet. Add one to get started.</div>
          )}

          {expenditureStreams.map(stream => (
            <div className="stream-card" key={stream.category.uniqueId}>
              <div className="stream-card-icon" />
              <div className="stream-card-info">
                <div className="stream-card-name">{stream.category.name}</div>
                <div className="stream-card-meta">{stream.recurrenceLabel}</div>
              </div>
              <div className="stream-card-right">
                <div className="stream-card-amount">{formatCurrency(stream.totalAmount)}</div>
                <div className="stream-card-percent">{stream.percentOfTotal}% of total</div>
              </div>

              <button
                className="stream-card-menu-trigger"
                onClick={(e) => {
                  e.stopPropagation();
                  setActiveMenuId(
                    activeMenuId === stream.category.uniqueId ? null : stream.category.uniqueId
                  );
                }}
              >
                <IconMoreVertical />
              </button>

              {activeMenuId === stream.category.uniqueId && (
                <div className="stream-card-dropdown" ref={menuRef}>
                  <button
                    className="stream-card-dropdown-item"
                    onClick={() => handleEditCategory(stream.category)}
                  >
                    <IconEdit /> Edit
                  </button>
                  <button
                    className="stream-card-dropdown-item danger"
                    onClick={() => handleDeleteCategory(stream.category)}
                  >
                    <IconTrash /> Delete
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>

      {/* ═══════════════════════════════════════════════════════════════════════
          MODALS
          ═══════════════════════════════════════════════════════════════════════ */}

      {/* ─── Record Transaction Modal ───────────────────────────────────────── */}
      {showRecordModal && (
        <div className="finance-modal-overlay" onClick={() => setShowRecordModal(false)}>
          <div className="finance-modal" onClick={e => e.stopPropagation()}>
            <div className="finance-modal-header">
              <h2 className="finance-modal-title">Record Transaction</h2>
              <button className="finance-modal-close" onClick={() => setShowRecordModal(false)}>
                <IconClose />
              </button>
            </div>

            <div className="finance-modal-body">
              {/* Type toggle */}
              <div className="transaction-type-toggle">
                <button
                  className={`transaction-type-btn ${recordForm.transactionType === 'Income' ? 'active' : ''}`}
                  onClick={() => setRecordForm(prev => ({ ...prev, transactionType: 'Income', categoryId: '' }))}
                >
                  Income
                </button>
                <button
                  className={`transaction-type-btn ${recordForm.transactionType === 'Expenditure' ? 'active' : ''}`}
                  onClick={() => setRecordForm(prev => ({ ...prev, transactionType: 'Expenditure', categoryId: '' }))}
                >
                  Expenditure
                </button>
              </div>

              {/* Amount */}
              <div className="finance-form-group">
                <label className="finance-form-label">Amount</label>
                <input
                  type="text"
                  className="finance-form-input"
                  placeholder="$ 0.00"
                  value={recordForm.amount ? `$ ${recordForm.amount}` : ''}
                  onChange={e => {
                    const raw = e.target.value.replace(/[^0-9.]/g, '');
                    setRecordForm(prev => ({ ...prev, amount: raw }));
                  }}
                />
              </div>

              {/* Category + Date row */}
              <div className="finance-form-row">
                <div className="finance-form-group">
                  <label className="finance-form-label">Stream/Category</label>
                  <select
                    className="finance-form-select"
                    value={recordForm.categoryId}
                    onChange={e => setRecordForm(prev => ({ ...prev, categoryId: e.target.value }))}
                  >
                    <option value="">Select...</option>
                    {recordCategories.map(c => (
                      <option key={c.categoryId} value={c.categoryId}>{c.name}</option>
                    ))}
                  </select>
                </div>

                <div className="finance-form-group">
                  <label className="finance-form-label">Date</label>
                  <input
                    type="date"
                    className="finance-form-input"
                    value={recordForm.transactionDate}
                    onChange={e => setRecordForm(prev => ({ ...prev, transactionDate: e.target.value }))}
                  />
                </div>
              </div>

              {/* Payment Method */}
              <div className="finance-form-group">
                <label className="finance-form-label">Payment Method</label>
                <select
                  className="finance-form-select"
                  value={recordForm.paymentMethod}
                  onChange={e => setRecordForm(prev => ({ ...prev, paymentMethod: e.target.value as PaymentMethod }))}
                >
                  {(Object.keys(paymentMethodLabels) as PaymentMethod[]).map(pm => (
                    <option key={pm} value={pm}>{paymentMethodLabels[pm]}</option>
                  ))}
                </select>
              </div>

              {/* Notes */}
              <div className="finance-form-group">
                <label className="finance-form-label">
                  Notes <span>(Optional)</span>
                </label>
                <textarea
                  className="finance-form-textarea"
                  placeholder="Add any additional details or context..."
                  value={recordForm.description}
                  onChange={e => setRecordForm(prev => ({ ...prev, description: e.target.value }))}
                />
              </div>
            </div>

            <div className="finance-modal-footer">
              <button className="finance-btn finance-btn-secondary" onClick={() => setShowRecordModal(false)}>
                Cancel
              </button>
              <button className="finance-btn finance-btn-primary" onClick={handleRecordTransaction}>
                <IconRecord /> Record Transaction
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ─── Export Report Modal ─────────────────────────────────────────────── */}
      {showExportModal && (
        <div className="finance-modal-overlay" onClick={() => setShowExportModal(false)}>
          <div className="finance-modal" onClick={e => e.stopPropagation()}>
            <div className="finance-modal-header">
              <h2 className="finance-modal-title">Export Financial Report</h2>
              <button className="finance-modal-close" onClick={() => setShowExportModal(false)}>
                <IconClose />
              </button>
            </div>

            <div className="finance-modal-body">
              {/* Hint */}
              <div className="export-hint">
                <IconInfo />
                Select date and range format to export report
              </div>

              {/* Date Range */}
              <div className="finance-form-group">
                <label className="finance-form-label">Date Range</label>
                <select
                  className="finance-form-select"
                  value={exportForm.dateRange}
                  onChange={e => setExportForm(prev => ({ ...prev, dateRange: e.target.value as ExportReportForm['dateRange'] }))}
                >
                  <option value="last_30">Last 30 Days</option>
                  <option value="last_90">Last 90 Days</option>
                  <option value="this_year">This Year</option>
                  <option value="custom">Custom Range</option>
                </select>
              </div>

              {/* Report Type */}
              <div className="finance-form-group">
                <label className="finance-form-label">Report Type</label>
                <div className="report-type-group">
                  {([
                    { value: 'full', label: 'Full Financial Summary' },
                    { value: 'income_only', label: 'Income Only' },
                    { value: 'expenditure_only', label: 'Expenditure Only' },
                  ] as const).map(opt => (
                    <button
                      key={opt.value}
                      className={`report-type-option ${exportForm.reportType === opt.value ? 'selected' : ''}`}
                      onClick={() => setExportForm(prev => ({ ...prev, reportType: opt.value }))}
                    >
                      <div className="report-type-radio">
                        <div className="report-type-radio-dot" />
                      </div>
                      {opt.label}
                    </button>
                  ))}
                </div>
              </div>

              {/* Export Format */}
              <div className="finance-form-group">
                <label className="finance-form-label">Export Format</label>
                <div className="export-format-group">
                  <button
                    className={`export-format-card ${exportForm.exportFormat === 'pdf' ? 'selected' : ''}`}
                    onClick={() => setExportForm(prev => ({ ...prev, exportFormat: 'pdf' }))}
                  >
                    <div className="export-format-icon"><IconDocument /></div>
                    <span className="export-format-label">PDF</span>
                  </button>
                  <button
                    className={`export-format-card ${exportForm.exportFormat === 'excel' ? 'selected' : ''}`}
                    onClick={() => setExportForm(prev => ({ ...prev, exportFormat: 'excel' }))}
                  >
                    <div className="export-format-icon"><IconTable /></div>
                    <span className="export-format-label">Excel</span>
                  </button>
                  <button
                    className={`export-format-card ${exportForm.exportFormat === 'csv' ? 'selected' : ''}`}
                    onClick={() => setExportForm(prev => ({ ...prev, exportFormat: 'csv' }))}
                  >
                    <div className="export-format-icon"><IconCsv /></div>
                    <span className="export-format-label">CSV</span>
                  </button>
                </div>
              </div>
            </div>

            <div className="finance-modal-footer">
              <button className="finance-btn finance-btn-secondary" onClick={() => setShowExportModal(false)}>
                Cancel
              </button>
              <button className="finance-btn finance-btn-primary" onClick={handleExportReport}>
                Export Report
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ─── Add / Edit Category Modal ──────────────────────────────────────── */}
      {showCategoryModal && (
        <div className="finance-modal-overlay" onClick={() => setShowCategoryModal(false)}>
          <div className="finance-modal" onClick={e => e.stopPropagation()}>
            <div className="finance-modal-header">
              <h2 className="finance-modal-title">
                {editingCategory ? 'Edit Stream' : 'Add Stream'}
              </h2>
              <button className="finance-modal-close" onClick={() => setShowCategoryModal(false)}>
                <IconClose />
              </button>
            </div>

            <div className="finance-modal-body">
              <div className="finance-form-group">
                <label className="finance-form-label">Stream Name</label>
                <input
                  type="text"
                  className="finance-form-input"
                  placeholder="e.g., Tithes, Rent, Utilities..."
                  value={categoryForm.name}
                  onChange={e => setCategoryForm(prev => ({ ...prev, name: e.target.value }))}
                  autoFocus
                />
              </div>

              <div className="finance-form-group">
                <label className="finance-form-label">Type</label>
                {editingCategory ? (
                  <input
                    type="text"
                    className="finance-form-input"
                    value={categoryModalType}
                    disabled
                    style={{ backgroundColor: '#f2f2f7', color: 'var(--text-muted)' }}
                  />
                ) : (
                  <div className="transaction-type-toggle">
                    <button
                      className={`transaction-type-btn ${categoryModalType === 'Income' ? 'active' : ''}`}
                      onClick={() => {
                        setCategoryModalType('Income');
                        setCategoryForm(prev => ({ ...prev, categoryType: 'Income' }));
                      }}
                    >
                      Income
                    </button>
                    <button
                      className={`transaction-type-btn ${categoryModalType === 'Expenditure' ? 'active' : ''}`}
                      onClick={() => {
                        setCategoryModalType('Expenditure');
                        setCategoryForm(prev => ({ ...prev, categoryType: 'Expenditure' }));
                      }}
                    >
                      Expenditure
                    </button>
                  </div>
                )}
              </div>
            </div>

            <div className="finance-modal-footer">
              <button className="finance-btn finance-btn-secondary" onClick={() => setShowCategoryModal(false)}>
                Cancel
              </button>
              <button className="finance-btn finance-btn-primary" onClick={handleSaveCategory}>
                {editingCategory ? 'Save Changes' : 'Add Stream'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ─── Delete Category Confirmation ───────────────────────────────────── */}
      {showDeleteConfirm && deletingCategory && (
        <div className="finance-modal-overlay" onClick={() => setShowDeleteConfirm(false)}>
          <div className="finance-modal" onClick={e => e.stopPropagation()}>
            <div className="finance-modal-header">
              <h2 className="finance-modal-title">Delete Stream</h2>
              <button className="finance-modal-close" onClick={() => setShowDeleteConfirm(false)}>
                <IconClose />
              </button>
            </div>

            <div className="finance-modal-body">
              <div className="delete-confirm-body">
                <div className="delete-confirm-icon">
                  <IconWarning />
                </div>
                <p className="delete-confirm-text">
                  Are you sure you want to delete{' '}
                  <span className="delete-confirm-name">"{deletingCategory.name}"</span>?
                </p>
                <p className="delete-confirm-warning">
                  Transactions under this stream will become uncategorized. This action cannot be undone.
                </p>
              </div>
            </div>

            <div className="finance-modal-footer">
              <button className="finance-btn finance-btn-secondary" onClick={() => setShowDeleteConfirm(false)}>
                Cancel
              </button>
              <button className="finance-btn finance-btn-danger" onClick={confirmDeleteCategory}>
                <IconTrash /> Delete Stream
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Finance;
