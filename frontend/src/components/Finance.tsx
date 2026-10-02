import React, {
  useEffect,
  useState,
  useRef,
  useMemo,
  useCallback,
} from "react";
import { useNavigate } from "react-router-dom";
import { useLayout } from "../hooks/useLayout";
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
} from "./Icons";
import DeleteConfirmModal from "./common/DeleteConfirmModal";
import "../styles/Finance.css";
import type {
  Transaction,
  TransactionType,
  TransactionCreatePayload,
  CategoryStream,
  RecordTransactionForm,
} from "../types/transaction";
import type {
  TransactionCategory,
  TransactionCategoryCreatePayload,
  TransactionCategoryUpdatePayload,
  CategoryForm,
} from "../types/transactionCategory";
import type { MonthOfYear } from "../types/tithe";
import {
  useTransactions,
  useCreateTransaction,
} from "../hooks/useTransactions";
import {
  useTransactionCategories,
  useCreateTransactionCategory,
  useUpdateTransactionCategory,
  useDeleteTransactionCategory,
} from "../hooks/useTransactionCategories";
import { useTithes } from "../hooks/useTithes";
import ExportPanel from "./ExportPanel";

// ─── Constants ───────────────────────────────────────────────────────────────

const MONTHS: MonthOfYear[] = [
  "January",
  "February",
  "March",
  "April",
  "May",
  "June",
  "July",
  "August",
  "September",
  "October",
  "November",
  "December",
];

const formatCurrency = (amount: number): string => {
  return `₵ ${amount.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
};

const renderCategoryIcon = (
  categoryName: string,
  categoryType: TransactionType,
) => {
  const size = 20;
  const className = `stream-icon ${categoryType === "Income" ? "income" : "expenditure"}`;

  const name = categoryName.toLowerCase();
  if (name.includes("offering") || name.includes("tithe")) {
    return <TitheIcon className={className} size={size} />;
  }
  if (name.includes("rent") || name.includes("venue")) {
    return <ProjectsIcon className={className} size={size} />;
  }
  if (name.includes("donation") || name.includes("gift")) {
    return <TitheIcon className={className} size={size} />;
  }
  if (name.includes("book") || name.includes("sales")) {
    return <DocumentIcon className={className} size={size} />;
  }
  if (
    name.includes("salaries") ||
    name.includes("staff") ||
    name.includes("payroll")
  ) {
    return <MembersIcon className={className} size={size} />;
  }
  if (
    name.includes("utilities") ||
    name.includes("electric") ||
    name.includes("water") ||
    name.includes("maintenance")
  ) {
    return <BoxIcon className={className} size={size} />;
  }
  if (name.includes("outreach") || name.includes("mission")) {
    return <CalendarIcon className={className} size={size} />;
  }
  if (
    name.includes("tech") ||
    name.includes("media") ||
    name.includes("stream")
  ) {
    return <DashboardIcon className={className} size={size} />;
  }
  if (name.includes("hospitality") || name.includes("food")) {
    return <RecordIcon className={className} size={size} />;
  }
  if (
    name.includes("transport") ||
    name.includes("bus") ||
    name.includes("car")
  ) {
    return <BoxIcon className={className} size={size} />;
  }

  return <FinanceIcon className={className} size={size} />;
};

// ─── Component ───────────────────────────────────────────────────────────────

const Finance: React.FC = () => {
  const { setTitle, setCtas } = useLayout();
  const navigate = useNavigate();

  // ── Period State ──────────────────────────────────────────────────────────
  const [periodYear, setPeriodYear] = useState(() => new Date().getFullYear());
  const [periodMonth, setPeriodMonth] = useState<MonthOfYear>(
    () => MONTHS[new Date().getMonth()],
  );

  const periodValue = `${periodYear}-${String(MONTHS.indexOf(periodMonth) + 1).padStart(2, "0")}`;
  const periodLabel = `${periodMonth} ${periodYear}`;

  const handlePeriodChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    if (!value) return;
    const [y, m] = value.split("-");
    setPeriodYear(parseInt(y, 10));
    setPeriodMonth(MONTHS[parseInt(m, 10) - 1]);
  };

  const handleViewTithes = () => {
    navigate(`/tithes?year=${periodYear}&month=${periodMonth}`);
  };

  // ── Queries & Mutations ───────────────────────────────────────────────────
  const { data: categories = [], isLoading: isLoadingCategories } =
    useTransactionCategories();

  // Fetch ALL transactions (period filter applied client-side — backend supports single-day only)
  const { data: transactionsData, isLoading: isLoadingTransactions } =
    useTransactions({}, 500);
  const allTransactions: Transaction[] = useMemo(
    () => transactionsData?.data || [],
    [transactionsData?.data],
  );

  // Filter transactions to the selected period
  const transactions = useMemo(
    () =>
      allTransactions.filter((t) => t.transactionDate.startsWith(periodValue)),
    [allTransactions, periodValue],
  );

  // Tithes — filtered server-side by period
  const { data: tithesData, isLoading: isLoadingTithes } = useTithes(
    { year: periodYear, month: periodMonth },
    500,
  );
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

  const [editingCategory, setEditingCategory] =
    useState<TransactionCategory | null>(null);
  const [deletingCategory, setDeletingCategory] =
    useState<TransactionCategory | null>(null);
  const [categoryModalType, setCategoryModalType] =
    useState<TransactionType>("Income");

  const [activeMenuId, setActiveMenuId] = useState<number | null>(null);
  const menuRef = useRef<HTMLDivElement>(null);

  const [recordForm, setRecordForm] = useState<RecordTransactionForm>({
    transactionType: "Income",
    amount: "",
    categoryId: "",
    transactionDate: new Date().toISOString().split("T")[0],
    description: "",
  });

  const [categoryForm, setCategoryForm] = useState<CategoryForm>({
    name: "",
    categoryType: "Income",
  });

  // ── Layout Header ─────────────────────────────────────────────────────────
  useEffect(() => {
    setTitle("Finances");
    setCtas([
      {
        type: "button",
        label: "Export Report",
        icon: "export",
        variant: "secondary",
        onClick: () => setShowExportModal(true),
      },
      {
        type: "button",
        label: "New Transaction",
        icon: "plus",
        variant: "primary",
        onClick: () => {
          setRecordForm({
            transactionType: "Income",
            amount: "",
            categoryId: "",
            transactionDate: new Date().toISOString().split("T")[0],
            description: "",
          });
          setShowRecordModal(true);
        },
      },
    ]);
  }, [setTitle, setCtas]);

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setActiveMenuId(null);
      }
    };
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  // ── Computed Streams & Totals (period-scoped) ─────────────────────────────
  const titheTotal = useMemo(
    () => tithes.reduce((sum, t) => sum + t.amount, 0),
    [tithes],
  );
  const titheCount = tithes.length;

  const totalIncomeFromTx = useMemo(
    () =>
      transactions
        .filter((t) => t.transactionType === "Income")
        .reduce((sum, t) => sum + t.amount, 0),
    [transactions],
  );
  const totalExpenditure = useMemo(
    () =>
      transactions
        .filter((t) => t.transactionType === "Expense")
        .reduce((sum, t) => sum + t.amount, 0),
    [transactions],
  );

  const displayTotalIncome = totalIncomeFromTx + titheTotal;
  const displayNetBalance = displayTotalIncome - totalExpenditure;

  const incomeCount =
    transactions.filter((t) => t.transactionType === "Income").length +
    titheCount;
  const expenditureCount = transactions.filter(
    (t) => t.transactionType === "Expense",
  ).length;

  const getCategoryStreams = useCallback(
    (type: TransactionType): CategoryStream[] => {
      const typeCats = categories.filter((c) => c.categoryType === type);
      const typeTransactions = transactions.filter(
        (t) => t.transactionType === type,
      );
      const totalForType =
        type === "Income" ? displayTotalIncome : totalExpenditure;

      const streams: CategoryStream[] = typeCats.map((cat) => {
        const catTransactions = typeTransactions.filter(
          (t) => t.categoryId === cat.id,
        );
        const total = catTransactions.reduce((sum, t) => sum + t.amount, 0);

        return {
          category: cat,
          totalAmount: total,
          percentOfTotal:
            totalForType > 0 ? Math.round((total / totalForType) * 100) : 0,
          transactionCount: catTransactions.length,
        };
      });

      if (type === "Income" && titheCount > 0) {
        const titheStream: CategoryStream = {
          category: {
            id: -1,
            name: "Tithes",
            categoryType: "Income",
          },
          totalAmount: titheTotal,
          percentOfTotal:
            displayTotalIncome > 0
              ? Math.round((titheTotal / displayTotalIncome) * 100)
              : 0,
          transactionCount: titheCount,
        };
        streams.unshift(titheStream);
      }

      return streams;
    },
    [
      categories,
      transactions,
      displayTotalIncome,
      totalExpenditure,
      titheCount,
      titheTotal,
    ],
  );

  const incomeStreams = useMemo(
    () => getCategoryStreams("Income"),
    [getCategoryStreams],
  );
  const expenditureStreams = useMemo(
    () => getCategoryStreams("Expense"),
    [getCategoryStreams],
  );

  // ── Handlers ──────────────────────────────────────────────────────────────
  const handleAddCategory = (type: TransactionType) => {
    setCategoryModalType(type);
    setEditingCategory(null);
    setCategoryForm({ name: "", categoryType: type });
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
      console.error("Failed to delete category:", err);
    }
  };

  const handleSaveCategory = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!categoryForm.name.trim()) return;

    try {
      if (editingCategory) {
        const payload: TransactionCategoryUpdatePayload = {
          name: categoryForm.name.trim(),
          categoryType: categoryModalType,
        };
        await updateCategory.mutateAsync({
          id: editingCategory.id,
          data: payload,
        });
      } else {
        const payload: TransactionCategoryCreatePayload = {
          name: categoryForm.name.trim(),
          categoryType: categoryModalType,
        };
        await createCategory.mutateAsync(payload);
      }

      setShowCategoryModal(false);
      setEditingCategory(null);
    } catch (err) {
      console.error("Failed to save category:", err);
      alert("Failed to save category.");
    }
  };

  const handleRecordTransaction = async (
    e: React.FormEvent<HTMLFormElement>,
  ) => {
    e.preventDefault();
    if (!recordForm.amount || !recordForm.categoryId) {
      alert("Please enter an amount and select a category.");
      return;
    }

    const payload: TransactionCreatePayload = {
      categoryId: Number(recordForm.categoryId),
      transactionType: recordForm.transactionType,
      transactionDate: recordForm.transactionDate,
      amount: parseFloat(recordForm.amount.replace(/,/g, "")),
      description: recordForm.description.trim() || null,
    };

    try {
      await createTransaction.mutateAsync(payload);
      setShowRecordModal(false);
    } catch (err) {
      console.error("Failed to record transaction:", err);
      alert("Failed to record transaction.");
    }
  };

  const recordCategories = useMemo(
    () =>
      categories.filter((c) => c.categoryType === recordForm.transactionType),
    [categories, recordForm.transactionType],
  );

  return (
    <div className="finance-container">
      {/* ─── Period Selector ────────────────────────────────────────────────── */}
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          gap: "12px",
          marginBottom: "20px",
          padding: "12px 16px",
          background: "#fff",
          borderRadius: "12px",
          border: "1px solid var(--border, #e5e7eb)",
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
          <span style={{ fontSize: "13px", color: "#64748b", fontWeight: 500 }}>
            Period
          </span>
          <input
            type="month"
            value={periodValue}
            onChange={handlePeriodChange}
            max={`${new Date().getFullYear() + 1}-12`}
            style={{
              padding: "6px 10px",
              borderRadius: "8px",
              border: "1px solid #e2e8f0",
              fontSize: "13px",
              fontWeight: 500,
              color: "#1e293b",
              background: "#fff",
              cursor: "pointer",
            }}
          />
        </div>
        <span style={{ fontSize: "12px", color: "#94a3b8" }}>
          Showing activity for {periodLabel}
        </span>
      </div>

      {/* ─── Summary Cards ──────────────────────────────────────────────────── */}
      <div className="finance-summary-cards">
        <div className="finance-summary-card">
          <span className="finance-card-label">Income ({periodLabel})</span>
          <div className="finance-card-value">
            {formatCurrency(displayTotalIncome)}
          </div>
          <div className="finance-card-trend">
            <TrendUpIcon />
            {incomeCount} transactions recorded
          </div>
        </div>

        <div className="finance-summary-card">
          <span className="finance-card-label">
            Expenditure ({periodLabel})
          </span>
          <div className="finance-card-value">
            {formatCurrency(totalExpenditure)}
          </div>
          <div className="finance-card-trend">
            <TrendDownIcon />
            {expenditureCount} transactions recorded
          </div>
        </div>

        <div className="finance-summary-card">
          <span className="finance-card-label">
            Net Balance ({periodLabel})
          </span>
          <div className="finance-card-value">
            {formatCurrency(displayNetBalance)}
          </div>
          <div className="finance-card-trend">
            <TrendUpIcon />
            Net liquidity position
          </div>
        </div>
      </div>

      {/* ─── Streams Grid ───────────────────────────────────────────────────── */}
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
              onClick={() => handleAddCategory("Income")}
            >
              Add Stream
            </button>
          </div>

          {isLoadingCategories || isLoadingTransactions || isLoadingTithes ? (
            <div className="stream-empty">Loading income streams...</div>
          ) : incomeStreams.length === 0 ? (
            <div className="stream-empty">
              No income streams for {periodLabel}.
            </div>
          ) : (
            incomeStreams.map((stream) => {
              const isTithesStream = stream.category.id === -1;

              return (
                <div
                  className="stream-card"
                  key={stream.category.id}
                  onClick={isTithesStream ? handleViewTithes : undefined}
                  role={isTithesStream ? "button" : undefined}
                  tabIndex={isTithesStream ? 0 : undefined}
                  onKeyDown={
                    isTithesStream
                      ? (e) => {
                          if (e.key === "Enter" || e.key === " ") {
                            e.preventDefault();
                            handleViewTithes();
                          }
                        }
                      : undefined
                  }
                  style={isTithesStream ? { cursor: "pointer" } : undefined}
                >
                  <div className="stream-card-icon income">
                    {renderCategoryIcon(stream.category.name, "Income")}
                  </div>
                  <div className="stream-card-info">
                    <div className="stream-card-name">
                      {stream.category.name}
                      {isTithesStream && (
                        <span
                          style={{
                            marginLeft: "8px",
                            fontSize: "11px",
                            color: "#64748b",
                            fontWeight: 400,
                          }}
                        >
                          View all →
                        </span>
                      )}
                    </div>
                    <div className="stream-card-meta">
                      {stream.transactionCount}{" "}
                      {isTithesStream ? "tithes" : "transactions"} in{" "}
                      {periodLabel}
                    </div>
                  </div>
                  <div className="stream-card-right">
                    <div className="stream-card-amount">
                      {formatCurrency(stream.totalAmount)}
                    </div>
                    <div className="stream-card-percent">
                      {stream.percentOfTotal}% of total
                    </div>
                  </div>

                  {!isTithesStream && (
                    <>
                      <button
                        type="button"
                        className="stream-card-menu-trigger"
                        onClick={(e) => {
                          e.stopPropagation();
                          setActiveMenuId(
                            activeMenuId === stream.category.id
                              ? null
                              : stream.category.id,
                          );
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
                            onClick={() =>
                              handleDeleteCategory(stream.category)
                            }
                          >
                            <TrashIcon /> Delete
                          </button>
                        </div>
                      )}
                    </>
                  )}
                </div>
              );
            })
          )}
        </div>

        {/* Expenditure Column */}
        <div className="finance-streams-column">
          <div className="stream-header">
            <div className="stream-header-left">
              <TrendDownIcon
                className="stream-header-icon expenditure"
                size={20}
              />
              <h3 className="stream-header-title">Expenditure Streams</h3>
            </div>
            <button
              type="button"
              className="stream-add-btn"
              onClick={() => handleAddCategory("Expense")}
            >
              Add Stream
            </button>
          </div>

          {isLoadingCategories || isLoadingTransactions ? (
            <div className="stream-empty">Loading expenditure streams...</div>
          ) : expenditureStreams.length === 0 ? (
            <div className="stream-empty">
              No expenditure streams for {periodLabel}.
            </div>
          ) : (
            expenditureStreams.map((stream) => (
              <div className="stream-card" key={stream.category.id}>
                <div className="stream-card-icon expenditure">
                  {renderCategoryIcon(stream.category.name, "Expense")}
                </div>
                <div className="stream-card-info">
                  <div className="stream-card-name">{stream.category.name}</div>
                  <div className="stream-card-meta">
                    {stream.transactionCount} transactions in {periodLabel}
                  </div>
                </div>
                <div className="stream-card-right">
                  <div className="stream-card-amount">
                    {formatCurrency(stream.totalAmount)}
                  </div>
                  <div className="stream-card-percent">
                    {stream.percentOfTotal}% of total
                  </div>
                </div>

                <button
                  type="button"
                  className="stream-card-menu-trigger"
                  onClick={(e) => {
                    e.stopPropagation();
                    setActiveMenuId(
                      activeMenuId === stream.category.id
                        ? null
                        : stream.category.id,
                    );
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
        <div
          className="finance-panel-overlay"
          onClick={() => setShowRecordModal(false)}
        >
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
              <div className="transaction-type-toggle">
                <button
                  type="button"
                  className={`transaction-type-btn ${recordForm.transactionType === "Income" ? "active" : ""}`}
                  onClick={() =>
                    setRecordForm((prev) => ({
                      ...prev,
                      transactionType: "Income",
                      categoryId: "",
                    }))
                  }
                >
                  Income
                </button>
                <button
                  type="button"
                  className={`transaction-type-btn ${recordForm.transactionType === "Expense" ? "active" : ""}`}
                  onClick={() =>
                    setRecordForm((prev) => ({
                      ...prev,
                      transactionType: "Expense",
                      categoryId: "",
                    }))
                  }
                >
                  Expenditure
                </button>
              </div>

              <div className="finance-form-group">
                <label className="finance-form-label">Amount</label>
                <input
                  type="number"
                  step="0.01"
                  min="0.01"
                  className="finance-form-input"
                  placeholder="₵ 0.00"
                  value={recordForm.amount}
                  onChange={(e) =>
                    setRecordForm((prev) => ({
                      ...prev,
                      amount: e.target.value,
                    }))
                  }
                  required
                />
              </div>

              <div className="finance-form-row">
                <div className="finance-form-group">
                  <label className="finance-form-label">Stream/Category</label>
                  <select
                    className="finance-form-select"
                    value={recordForm.categoryId}
                    onChange={(e) =>
                      setRecordForm((prev) => ({
                        ...prev,
                        categoryId: Number(e.target.value) || "",
                      }))
                    }
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
                    onChange={(e) =>
                      setRecordForm((prev) => ({
                        ...prev,
                        transactionDate: e.target.value,
                      }))
                    }
                    required
                  />
                </div>
              </div>

              <div className="finance-form-group">
                <label className="finance-form-label">
                  Notes <span>(Optional)</span>
                </label>
                <textarea
                  className="finance-form-textarea"
                  placeholder="Add any additional details or context..."
                  value={recordForm.description}
                  onChange={(e) =>
                    setRecordForm((prev) => ({
                      ...prev,
                      description: e.target.value,
                    }))
                  }
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
                <RecordIcon />{" "}
                {createTransaction.isPending
                  ? "Saving..."
                  : "Record Transaction"}
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
            title: `Financial Report — ${periodLabel}`,
            filename: `echo-financial-report-${periodValue}`,
            columns: [
              { key: "transactionDate", label: "Date" },
              { key: "transactionType", label: "Type" },
              { key: "categoryName", label: "Category" },
              { key: "amount", label: "Amount" },
              { key: "description", label: "Description" },
            ],
            rows: transactions.map((t) => ({
              ...t,
              categoryName:
                t.categoryName ||
                categories.find((c) => c.id === t.categoryId)?.name ||
                "General",
            })),
          }}
          onClose={() => setShowExportModal(false)}
        />
      )}

      {/* ─── Add / Edit Category Modal ──────────────────────────────────────── */}
      {showCategoryModal && (
        <div
          className="finance-modal-overlay"
          onClick={() => setShowCategoryModal(false)}
        >
          <div className="finance-modal" onClick={(e) => e.stopPropagation()}>
            <form onSubmit={handleSaveCategory}>
              <div className="finance-modal-header">
                <h2 className="finance-modal-title">
                  {editingCategory ? "Edit Stream" : "Add Stream"}
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
                    onChange={(e) =>
                      setCategoryForm((prev) => ({
                        ...prev,
                        name: e.target.value,
                      }))
                    }
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
                      value={
                        categoryModalType === "Income"
                          ? "Income"
                          : "Expenditure"
                      }
                      disabled
                      style={{
                        backgroundColor: "#f2f2f7",
                        color: "var(--text-muted)",
                      }}
                    />
                  ) : (
                    <div className="transaction-type-toggle">
                      <button
                        type="button"
                        className={`transaction-type-btn ${categoryModalType === "Income" ? "active" : ""}`}
                        onClick={() => {
                          setCategoryModalType("Income");
                          setCategoryForm((prev) => ({
                            ...prev,
                            categoryType: "Income",
                          }));
                        }}
                      >
                        Income
                      </button>
                      <button
                        type="button"
                        className={`transaction-type-btn ${categoryModalType === "Expense" ? "active" : ""}`}
                        onClick={() => {
                          setCategoryModalType("Expense");
                          setCategoryForm((prev) => ({
                            ...prev,
                            categoryType: "Expense",
                          }));
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
                  disabled={
                    createCategory.isPending || updateCategory.isPending
                  }
                >
                  {createCategory.isPending || updateCategory.isPending
                    ? "Saving..."
                    : editingCategory
                      ? "Save Changes"
                      : "Add Stream"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      <DeleteConfirmModal
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={confirmDeleteCategory}
        itemName={deletingCategory?.name || ""}
        title="Delete Stream"
        confirmText="Delete Stream"
        warningMessage="Transactions under this stream will become uncategorized. This action cannot be undone."
      />
    </div>
  );
};

export default Finance;
