import React, { useEffect, useState, useRef, useMemo } from 'react';
import { useLayout } from '../hooks/useLayout';
import {
  CloseIcon,
  MoreVerticalIcon,
  EditIcon,
  TrashIcon,
  TrendUpIcon,
  TrendDownIcon,
  DocumentIcon,
  RecordIcon,
  FinanceIcon,
  TitheIcon,
  ProjectsIcon,
  CalendarIcon,
  BoxIcon,
  MembersIcon,
  DashboardIcon,
} from './Icons';
import DeleteConfirmModal from './common/DeleteConfirmModal';
import '../styles/Finance.css';
import type {
  TransactionCategory,
  Transaction,
  CategoryStream,
  TransactionType,
  RecordTransactionForm,
  CategoryForm,
} from '../types/finance';
import {
  useTransactions,
  useTransactionCategories,
  useCreateTransaction,
  useCreateTransactionCategory,
  useUpdateTransactionCategory,
  useDeleteTransactionCategory,
} from '../hooks/useTransactions';
import { useTithes } from '../hooks/useTithes';
import ExportPanel from './ExportPanel';

// ─── Helper: format currency ─────────────────────────────────────────────────

const formatCurrency = (amount: number): string => {
  return `₵ ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
};

// ─── SVG Icons (renderCategoryIcon helper) ───────────────────────────────────

const renderCategoryIcon = (categoryName: string, categoryType: string) => {
  const size = 20;
  const className = `stream-icon ${categoryType.toLowerCase() === 'income' ? 'income' : 'expenditure'}`;

  const name = categoryName.toLowerCase();
  if (name.includes('offering') || name.includes('tithe')) {
    return <TitheIcon className={className} size={size} />;
  }
  if (name.includes('rent') || name.includes('venue')) {
    return <ProjectsIcon className={className} size={size} />;
  }
  if (name.includes('donation') || name.includes('gift')) {
    return <TitheIcon className={className} size={size} />;
  }
  if (name.includes('book') || name.includes('sales')) {
    return <DocumentIcon className={className} size={size} />;
  }
  if (name.includes('salaries') || name.includes('staff') || name.includes('payroll')) {
    return <MembersIcon className={className} size={size} />;
  }
  if (name.includes('utilities') || name.includes('electric') || name.includes('water') || name.includes('maintenance')) {
    return <BoxIcon className={className} size={size} />;
  }
  if (name.includes('outreach') || name.includes('mission')) {
    return <CalendarIcon className={className} size={size} />;
  }
  if (name.includes('tech') || name.includes('media') || name.includes('stream')) {
    return <DashboardIcon className={className} size={size} />;
  }
  if (name.includes('hospitality') || name.includes('food')) {
    return <RecordIcon className={className} size={size} />;
  }
  if (name.includes('transport') || name.includes('bus') || name.includes('car')) {
    return <BoxIcon className={className} size={size} />;
  }

  return <FinanceIcon className={className} size={size} />;
};

const Finance: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  // ── Queries & Mutations ───────────────────────────────────────────────────
  const { data: categories = [], isLoading: isLoadingCategories } = useTransactionCategories();
  const { data: transactionsData, isLoading: isLoadingTransactions } = useTransactions({}, 200);
  const transactions: Transaction[] = useMemo(() => transactionsData?.data || [], [transactionsData?.data]);

  const { data: tithesData } = useTithes({}, 200);
  const tithes = useMemo(() => tithesData?.data || [], [tithesData?.data]);

  const createTransaction = useCreateTransaction();
  const createCategory = useCreateTransactionCategory();
  const updateCategory = useUpdateTransactionCategory();
  const deleteCategory = useDeleteTransactionCategory();

  // ── Modals State ──────────────────────────────────────────────────────────
  const [showRecordModal, setShowRecordModal] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [showCategoryModal, setShowCategoryModal] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  // Edit/Delete state
  const [editingCategory, setEditingCategory] = useState<TransactionCategory | null>(null);
  const [deletingCategory, setDeletingCategory] = useState<TransactionCategory | null>(null);
  const [categoryModalType, setCategoryModalType] = useState<TransactionType>('Income');

  // Dropdown menu
  const [activeMenuId, setActiveMenuId] = useState<number | null>(null);
  const menuRef = useRef<HTMLDivElement>(null);

  // Record Transaction form
  const [recordForm, setRecordForm] = useState<RecordTransactionForm>({
    transactionType: 'Income',
    amount: '',
    categoryId: '',
    transactionDate: new Date().toISOString().split('T')[0],
    description: '',
  });

  // Category form
  const [categoryForm, setCategoryForm] = useState<CategoryForm>({
    name: '',
    categoryType: 'Income',
  });

  // ── Layout Header ─────────────────────────────────────────────────────────
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

  // ── Computed Streams & Totals ─────────────────────────────────────────────
  const titheTotal = useMemo(() => tithes.reduce((sum, t) => sum + t.amount, 0), [tithes]);
  const titheCount = tithes.length;

  const totalIncomeFromTx = useMemo(
    () => transactions.filter((t) => t.transactionType === 'Income').reduce((sum, t) => sum + t.amount, 0),
    [transactions]
  );
  const totalExpenditure = useMemo(
    () => transactions.filter((t) => t.transactionType === 'Expense').reduce((sum, t) => sum + t.amount, 0),
    [transactions]
  );

  const displayTotalIncome = totalIncomeFromTx + titheTotal;
  const displayNetBalance = displayTotalIncome - totalExpenditure;

  const incomeCount = transactions.filter((t) => t.transactionType === 'Income').length + titheCount;
  const expenditureCount = transactions.filter((t) => t.transactionType === 'Expense').length;

  const getCategoryStreams = (type: TransactionType): CategoryStream[] => {
    const typeCats = categories.filter((c) => c.categoryType === type);
    const typeTransactions = transactions.filter((t) => t.transactionType === type);
    const totalForType = type === 'Income' ? displayTotalIncome : totalExpenditure;

    const streams: CategoryStream[] = typeCats.map((cat) => {
      const catTransactions = typeTransactions.filter((t) => t.categoryId === cat.id);
      const total = catTransactions.reduce((sum, t) => sum + t.amount, 0);

      return {
        category: cat,
        totalAmount: total,
        percentOfTotal: totalForType > 0 ? Math.round((total / totalForType) * 100) : 0,
        transactionCount: catTransactions.length,
      };
    });

    // If type is Income, prepend the virtual Tithes stream
    if (type === 'Income' && titheCount > 0) {
      const titheStream: CategoryStream = {
        category: {
          id: -1,
          name: 'Tithes',
          categoryType: 'Income',
        },
        totalAmount: titheTotal,
        percentOfTotal: displayTotalIncome > 0 ? Math.round((titheTotal / displayTotalIncome) * 100) : 0,
        transactionCount: titheCount,
      };
      streams.unshift(titheStream);
    }

    return streams;
  };

  const incomeStreams = useMemo(() => getCategoryStreams('Income'), [categories, transactions, displayTotalIncome, titheTotal, titheCount]);
  const expenditureStreams = useMemo(() => getCategoryStreams('Expense'), [categories, transactions, totalExpenditure]);

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

  const confirmDeleteCategory = async () => {
    if (!deletingCategory) return;
    try {
      await deleteCategory.mutateAsync(deletingCategory.id);
      setShowDeleteConfirm(false);
      setDeletingCategory(null);
    } catch (err) {
      console.error('Failed to delete category:', err);
    }
  };

  const handleSaveCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!categoryForm.name.trim()) return;

    try {
      if (editingCategory) {
        await updateCategory.mutateAsync({
          id: editingCategory.id,
          data: {
            name: categoryForm.name.trim(),
            categoryType: categoryModalType,
          },
        });
      } else {
        await createCategory.mutateAsync({
          name: categoryForm.name.trim(),
          categoryType: categoryModalType,
        });
      }

      setShowCategoryModal(false);
      setEditingCategory(null);
    } catch (err) {
      console.error('Failed to save category:', err);
      alert('Failed to save category.');
    }
  };

  const handleRecordTransaction = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!recordForm.amount || !recordForm.categoryId) {
      alert('Please enter an amount and select a category.');
      return;
    }

    try {
      await createTransaction.mutateAsync({
        categoryId: Number(recordForm.categoryId),
        transactionType: recordForm.transactionType,
        transactionDate: recordForm.transactionDate,
        amount: parseFloat(recordForm.amount.replace(/,/g, '')),
        description: recordForm.description.trim() || undefined,
      });

      setShowRecordModal(false);
    } catch (err) {
      console.error('Failed to record transaction:', err);
      alert('Failed to record transaction.');
    }
  };

  // Filter categories for the record modal based on selected type
  const recordCategories = useMemo(() => {
    return categories.filter((c) => c.categoryType === recordForm.transactionType);
  }, [categories, recordForm.transactionType]);

  return (
    <div className="finance-container">
      {/* ─── Summary Cards ──────────────────────────────────────────────────── */}
      <div className="finance-summary-cards">
        <div className="finance-summary-card">
          <span className="finance-card-label">Total Monthly Income</span>
          <div className="finance-card-value">{formatCurrency(displayTotalIncome)}</div>
          <div className="finance-card-trend">
            <TrendUpIcon />
            +{incomeCount} transactions recorded
          </div>
        </div>

        <div className="finance-summary-card">
          <span className="finance-card-label">Total Expenditure</span>
          <div className="finance-card-value">{formatCurrency(totalExpenditure)}</div>
          <div className="finance-card-trend">
            <TrendDownIcon />
            +{expenditureCount} transactions recorded
          </div>
        </div>

        <div className="finance-summary-card">
          <span className="finance-card-label">Net Balance</span>
          <div className="finance-card-value">{formatCurrency(displayNetBalance)}</div>
          <div className="finance-card-trend">
            <TrendUpIcon />
            Net liquidity position
          </div>
        </div>
      </div>

      {/* ─── Streams Grid (side-by-side) ────────────────────────────────────── */}
      <div className="finance-streams-grid">
        {/* Income Column */}
        <div className="finance-streams-column">
          <div className="stream-header">
            <div className="stream-header-left">
              <TrendUpIcon className="stream-header-icon income" size={20} />
              <h3 className="stream-header-title">Income Streams</h3>
            </div>
            <button
              type="button"
              className="stream-add-btn"
              onClick={() => handleAddCategory('Income')}
            >
              Add Stream
            </button>
          </div>

          {isLoadingCategories || isLoadingTransactions ? (
            <div className="stream-empty">Loading income streams...</div>
          ) : incomeStreams.length === 0 ? (
            <div className="stream-empty">No income streams yet. Add one to get started.</div>
          ) : (
            incomeStreams.map((stream) => (
              <div className="stream-card" key={stream.category.id}>
                <div className="stream-card-icon income">
                  {renderCategoryIcon(stream.category.name, 'Income')}
                </div>
                <div className="stream-card-info">
                  <div className="stream-card-name">{stream.category.name}</div>
                  <div className="stream-card-meta">{stream.transactionCount} transactions</div>
                </div>
                <div className="stream-card-right">
                  <div className="stream-card-amount">{formatCurrency(stream.totalAmount)}</div>
                  <div className="stream-card-percent">{stream.percentOfTotal}% of total</div>
                </div>

                {/* Three-dot menu - hidden for virtual streams */}
                {stream.category.id !== -1 && (
                  <button
                    type="button"
                    className="stream-card-menu-trigger"
                    onClick={(e) => {
                      e.stopPropagation();
                      setActiveMenuId(activeMenuId === stream.category.id ? null : stream.category.id);
                    }}
                    aria-label="Stream actions"
                  >
                    <MoreVerticalIcon />
                  </button>
                )}

                {activeMenuId === stream.category.id && stream.category.id !== -1 && (
                  <div className="stream-card-dropdown" ref={menuRef}>
                    <button
                      type="button"
                      className="stream-card-dropdown-item"
                      onClick={() => handleEditCategory(stream.category)}
                    >
                      <EditIcon /> Edit
                    </button>
                    <button
                      type="button"
                      className="stream-card-dropdown-item danger"
                      onClick={() => handleDeleteCategory(stream.category)}
                    >
                      <TrashIcon /> Delete
                    </button>
                  </div>
                )}
              </div>
            ))
          )}
        </div>

        {/* Expenditure Column */}
        <div className="finance-streams-column">
          <div className="stream-header">
            <div className="stream-header-left">
              <TrendDownIcon className="stream-header-icon expenditure" size={20} />
              <h3 className="stream-header-title">Expenditure Streams</h3>
            </div>
            <button
              type="button"
              className="stream-add-btn"
              onClick={() => handleAddCategory('Expense')}
            >
              Add Stream
            </button>
          </div>

          {isLoadingCategories || isLoadingTransactions ? (
            <div className="stream-empty">Loading expenditure streams...</div>
          ) : expenditureStreams.length === 0 ? (
            <div className="stream-empty">No expenditure streams yet. Add one to get started.</div>
          ) : (
            expenditureStreams.map((stream) => (
              <div className="stream-card" key={stream.category.id}>
                <div className="stream-card-icon expenditure">
                  {renderCategoryIcon(stream.category.name, 'Expense')}
                </div>
                <div className="stream-card-info">
                  <div className="stream-card-name">{stream.category.name}</div>
                  <div className="stream-card-meta">{stream.transactionCount} transactions</div>
                </div>
                <div className="stream-card-right">
                  <div className="stream-card-amount">{formatCurrency(stream.totalAmount)}</div>
                  <div className="stream-card-percent">{stream.percentOfTotal}% of total</div>
                </div>

                <button
                  type="button"
                  className="stream-card-menu-trigger"
                  onClick={(e) => {
                    e.stopPropagation();
                    setActiveMenuId(activeMenuId === stream.category.id ? null : stream.category.id);
                  }}
                  aria-label="Stream actions"
                >
                  <MoreVerticalIcon />
                </button>

                {activeMenuId === stream.category.id && (
                  <div className="stream-card-dropdown" ref={menuRef}>
                    <button
                      type="button"
                      className="stream-card-dropdown-item"
                      onClick={() => handleEditCategory(stream.category)}
                    >
                      <EditIcon /> Edit
                    </button>
                    <button
                      type="button"
                      className="stream-card-dropdown-item danger"
                      onClick={() => handleDeleteCategory(stream.category)}
                    >
                      <TrashIcon /> Delete
                    </button>
                  </div>
                )}
              </div>
            ))
          )}
        </div>
      </div>

      {/* ─── Record Transaction Side Panel ──────────────────────────────────── */}
      {showRecordModal && (
        <div className="finance-panel-overlay" onClick={() => setShowRecordModal(false)}>
          <form
            className="finance-side-panel"
            onClick={(e) => e.stopPropagation()}
            onSubmit={handleRecordTransaction}
          >
            <div className="finance-panel-header">
              <h2 className="finance-panel-title">Record Transaction</h2>
              <button
                type="button"
                className="finance-panel-close"
                onClick={() => setShowRecordModal(false)}
                aria-label="Close panel"
              >
                <CloseIcon />
              </button>
            </div>

            <div className="finance-panel-body">
              {/* Type toggle */}
              <div className="transaction-type-toggle">
                <button
                  type="button"
                  className={`transaction-type-btn ${recordForm.transactionType === 'Income' ? 'active' : ''}`}
                  onClick={() => setRecordForm((prev) => ({ ...prev, transactionType: 'Income', categoryId: '' }))}
                >
                  Income
                </button>
                <button
                  type="button"
                  className={`transaction-type-btn ${recordForm.transactionType === 'Expense' ? 'active' : ''}`}
                  onClick={() => setRecordForm((prev) => ({ ...prev, transactionType: 'Expense', categoryId: '' }))}
                >
                  Expenditure
                </button>
              </div>

              {/* Amount */}
              <div className="finance-form-group">
                <label className="finance-form-label">Amount</label>
                <input
                  type="number"
                  step="0.01"
                  min="0.01"
                  className="finance-form-input"
                  placeholder="₵ 0.00"
                  value={recordForm.amount}
                  onChange={(e) => setRecordForm((prev) => ({ ...prev, amount: e.target.value }))}
                  required
                />
              </div>

              {/* Category + Date row */}
              <div className="finance-form-row">
                <div className="finance-form-group">
                  <label className="finance-form-label">Stream/Category</label>
                  <select
                    className="finance-form-select"
                    value={recordForm.categoryId}
                    onChange={(e) => setRecordForm((prev) => ({ ...prev, categoryId: Number(e.target.value) || '' }))}
                    required
                  >
                    <option value="">Select...</option>
                    {recordCategories.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="finance-form-group">
                  <label className="finance-form-label">Date</label>
                  <input
                    type="date"
                    className="finance-form-input"
                    value={recordForm.transactionDate}
                    onChange={(e) => setRecordForm((prev) => ({ ...prev, transactionDate: e.target.value }))}
                    required
                  />
                </div>
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
                  onChange={(e) => setRecordForm((prev) => ({ ...prev, description: e.target.value }))}
                />
              </div>
            </div>

            <div className="finance-panel-footer">
              <button
                type="button"
                className="finance-btn finance-btn-secondary"
                onClick={() => setShowRecordModal(false)}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="finance-btn finance-btn-primary"
                disabled={createTransaction.isPending}
              >
                <RecordIcon /> {createTransaction.isPending ? 'Saving...' : 'Record Transaction'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* ─── Export Report Side Panel ──────────────────────────────────────── */}
      {showExportModal && (
        <ExportPanel
          title="Export Financial Report"
          exportConfig={{
            title: 'Financial Report',
            filename: 'echo-financial-report',
            columns: [
              { key: 'transactionDate', label: 'Date' },
              { key: 'transactionType', label: 'Type' },
              { key: 'categoryName', label: 'Category' },
              { key: 'amount', label: 'Amount' },
              { key: 'description', label: 'Description' },
            ],
            rows: transactions.map((t) => ({
              ...t,
              categoryName: t.categoryName || categories.find((c) => c.id === t.categoryId)?.name || 'General',
            })),
          }}
          onClose={() => setShowExportModal(false)}
        />
      )}

      {/* ─── Add / Edit Category Modal ──────────────────────────────────────── */}
      {showCategoryModal && (
        <div className="finance-modal-overlay" onClick={() => setShowCategoryModal(false)}>
          <div className="finance-modal" onClick={(e) => e.stopPropagation()}>
            <form onSubmit={handleSaveCategory}>
              <div className="finance-modal-header">
                <h2 className="finance-modal-title">
                  {editingCategory ? 'Edit Stream' : 'Add Stream'}
                </h2>
                <button
                  type="button"
                  className="finance-modal-close"
                  onClick={() => setShowCategoryModal(false)}
                  aria-label="Close modal"
                >
                  <CloseIcon />
                </button>
              </div>

              <div className="finance-modal-body">
                <div className="finance-form-group">
                  <label className="finance-form-label">Stream Name</label>
                  <input
                    type="text"
                    className="finance-form-input"
                    placeholder="e.g., Offerings, Rent, Utilities..."
                    value={categoryForm.name}
                    onChange={(e) => setCategoryForm((prev) => ({ ...prev, name: e.target.value }))}
                    autoFocus
                    required
                  />
                </div>

                <div className="finance-form-group">
                  <label className="finance-form-label">Type</label>
                  {editingCategory ? (
                    <input
                      type="text"
                      className="finance-form-input"
                      value={categoryModalType === 'Income' ? 'Income' : 'Expenditure'}
                      disabled
                      style={{ backgroundColor: '#f2f2f7', color: 'var(--text-muted)' }}
                    />
                  ) : (
                    <div className="transaction-type-toggle">
                      <button
                        type="button"
                        className={`transaction-type-btn ${categoryModalType === 'Income' ? 'active' : ''}`}
                        onClick={() => {
                          setCategoryModalType('Income');
                          setCategoryForm((prev) => ({ ...prev, categoryType: 'Income' }));
                        }}
                      >
                        Income
                      </button>
                      <button
                        type="button"
                        className={`transaction-type-btn ${categoryModalType === 'Expense' ? 'active' : ''}`}
                        onClick={() => {
                          setCategoryModalType('Expense');
                          setCategoryForm((prev) => ({ ...prev, categoryType: 'Expense' }));
                        }}
                      >
                        Expenditure
                      </button>
                    </div>
                  )}
                </div>
              </div>

              <div className="finance-modal-footer">
                <button
                  type="button"
                  className="finance-btn finance-btn-secondary"
                  onClick={() => setShowCategoryModal(false)}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="finance-btn finance-btn-primary"
                  disabled={createCategory.isPending || updateCategory.isPending}
                >
                  {createCategory.isPending || updateCategory.isPending
                    ? 'Saving...'
                    : editingCategory
                    ? 'Save Changes'
                    : 'Add Stream'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ─── Delete Category Confirmation ───────────────────────────────────── */}
      <DeleteConfirmModal
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={confirmDeleteCategory}
        itemName={deletingCategory?.name || ''}
        title="Delete Stream"
        confirmText="Delete Stream"
        warningMessage="Transactions under this stream will become uncategorized. This action cannot be undone."
      />
    </div>
  );
};

export default Finance;
