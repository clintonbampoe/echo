import React, { useState } from "react";
import { CloseIcon, EditIcon, TagIcon, TrashIcon } from "../Icons";
import DeleteConfirmModal from "./DeleteConfirmModal";
import { getErrorMessage } from "../../utils/errors";
import "../../styles/ManageCategoriesModal.css";

export interface ManagedCategory {
  id: number;
  name: string;
  /** Optional sub-type, e.g. "Income" / "Expense" for transaction categories. */
  type?: string;
}

interface ManageCategoriesModalProps {
  isOpen: boolean;
  onClose: () => void;
  /** e.g. "Asset Categories" */
  title: string;
  /** Plural noun for what uses the categories, e.g. "assets". */
  usedBy: string;
  categories: ManagedCategory[];
  isLoading?: boolean;
  /** When set, each category also has a type picked from these options. */
  typeOptions?: string[];
  onCreate: (name: string, type?: string) => Promise<unknown>;
  onUpdate: (id: number, name: string, type?: string) => Promise<unknown>;
  onDelete: (id: number) => Promise<unknown>;
}

const ManageCategoriesModal: React.FC<ManageCategoriesModalProps> = ({
  isOpen,
  onClose,
  title,
  usedBy,
  categories,
  isLoading = false,
  typeOptions,
  onCreate,
  onUpdate,
  onDelete,
}) => {
  const [newName, setNewName] = useState("");
  const [newType, setNewType] = useState(typeOptions?.[0] ?? "");
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editName, setEditName] = useState("");
  const [editType, setEditType] = useState("");
  const [deleting, setDeleting] = useState<ManagedCategory | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const run = async (action: () => Promise<unknown>, fallback: string) => {
    setBusy(true);
    setError(null);
    try {
      await action();
      return true;
    } catch (err) {
      setError(getErrorMessage(err, fallback));
      return false;
    } finally {
      setBusy(false);
    }
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    const name = newName.trim();
    if (!name) return;
    const ok = await run(
      () => onCreate(name, typeOptions ? newType : undefined),
      "Failed to add category.",
    );
    if (ok) setNewName("");
  };

  const startEdit = (category: ManagedCategory) => {
    setError(null);
    setEditingId(category.id);
    setEditName(category.name);
    setEditType(category.type ?? typeOptions?.[0] ?? "");
  };

  const handleSaveEdit = async () => {
    if (editingId === null) return;
    const name = editName.trim();
    if (!name) return;
    const ok = await run(
      () => onUpdate(editingId, name, typeOptions ? editType : undefined),
      "Failed to update category.",
    );
    if (ok) setEditingId(null);
  };

  const handleConfirmDelete = async () => {
    if (!deleting) return;
    const target = deleting;
    setDeleting(null);
    await run(() => onDelete(target.id), "Failed to delete category.");
  };

  const sorted = [...categories].sort(
    (a, b) =>
      (a.type ?? "").localeCompare(b.type ?? "") || a.name.localeCompare(b.name),
  );

  return (
    <div className="manage-cat-overlay" onClick={onClose}>
      <div
        className="manage-cat-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="manage-cat-title"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="manage-cat-header">
          <div className="manage-cat-heading">
            <span className="manage-cat-header-icon">
              <TagIcon size={18} />
            </span>
            <div>
              <h2 id="manage-cat-title" className="manage-cat-title">
                {title}
              </h2>
              <p className="manage-cat-subtitle">
                Add, rename or remove the categories used to group {usedBy}.
              </p>
            </div>
          </div>
          <button
            type="button"
            className="manage-cat-icon-btn"
            onClick={onClose}
            aria-label="Close"
          >
            <CloseIcon size={18} />
          </button>
        </div>

        <form className="manage-cat-add" onSubmit={handleCreate}>
          <input
            className="manage-cat-input"
            placeholder="New category name"
            value={newName}
            maxLength={100}
            onChange={(e) => setNewName(e.target.value)}
            disabled={busy}
          />
          {typeOptions && (
            <select
              className="manage-cat-select"
              value={newType}
              onChange={(e) => setNewType(e.target.value)}
              disabled={busy}
            >
              {typeOptions.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>
          )}
          <button
            type="submit"
            className="manage-cat-btn primary"
            disabled={busy || !newName.trim()}
          >
            Add
          </button>
        </form>

        {error && <div className="manage-cat-error">{error}</div>}

        <div className="manage-cat-list">
          {isLoading ? (
            <div className="manage-cat-empty">Loading categories…</div>
          ) : sorted.length === 0 ? (
            <div className="manage-cat-empty">
              No categories yet. Add your first one above.
            </div>
          ) : (
            sorted.map((category) =>
              editingId === category.id ? (
                <div key={category.id} className="manage-cat-row editing">
                  <input
                    className="manage-cat-input"
                    value={editName}
                    maxLength={100}
                    autoFocus
                    onChange={(e) => setEditName(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter") void handleSaveEdit();
                      if (e.key === "Escape") setEditingId(null);
                    }}
                    disabled={busy}
                  />
                  {typeOptions && (
                    <select
                      className="manage-cat-select"
                      value={editType}
                      onChange={(e) => setEditType(e.target.value)}
                      disabled={busy}
                    >
                      {typeOptions.map((t) => (
                        <option key={t} value={t}>
                          {t}
                        </option>
                      ))}
                    </select>
                  )}
                  <button
                    type="button"
                    className="manage-cat-btn primary"
                    onClick={handleSaveEdit}
                    disabled={busy || !editName.trim()}
                  >
                    Save
                  </button>
                  <button
                    type="button"
                    className="manage-cat-btn"
                    onClick={() => setEditingId(null)}
                    disabled={busy}
                  >
                    Cancel
                  </button>
                </div>
              ) : (
                <div key={category.id} className="manage-cat-row">
                  <span className="manage-cat-name">{category.name}</span>
                  {category.type && (
                    <span
                      className={`manage-cat-type type-${category.type.toLowerCase()}`}
                    >
                      {category.type}
                    </span>
                  )}
                  <div className="manage-cat-row-actions">
                    <button
                      type="button"
                      className="manage-cat-icon-btn"
                      onClick={() => startEdit(category)}
                      disabled={busy}
                      aria-label={`Edit ${category.name}`}
                      title="Edit"
                    >
                      <EditIcon size={16} />
                    </button>
                    <button
                      type="button"
                      className="manage-cat-icon-btn danger"
                      onClick={() => {
                        setError(null);
                        setDeleting(category);
                      }}
                      disabled={busy}
                      aria-label={`Delete ${category.name}`}
                      title="Delete"
                    >
                      <TrashIcon size={16} />
                    </button>
                  </div>
                </div>
              ),
            )
          )}
        </div>
      </div>

      {/* Keep clicks on the confirm dialog's backdrop from also closing this modal. */}
      <div onClick={(e) => e.stopPropagation()}>
        <DeleteConfirmModal
          isOpen={!!deleting}
          onClose={() => setDeleting(null)}
          onConfirm={handleConfirmDelete}
          itemName={deleting?.name ?? ""}
          title="Delete Category"
          warningMessage={`A category that is still used by ${usedBy} can't be deleted — move those ${usedBy} to another category first.`}
        />
      </div>
    </div>
  );
};

export default ManageCategoriesModal;
