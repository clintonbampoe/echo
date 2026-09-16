import React, { useEffect, useMemo, useState } from 'react';
import { useLayout } from '../hooks/useLayout';
import {
  useAssets,
  useAssetCategories,
  useCreateAsset,
  useUpdateAsset,
  useDeleteAsset,
  useCreateAssetCategory,
} from '../hooks/useAssets';
import type { Asset, AssetStatus } from '../types/asset';
import { CloseIcon, RecordIcon } from './Icons';
import DeleteConfirmModal from './common/DeleteConfirmModal';
import '../styles/Assets.css';

const formatCurrency = (amount: number): string => {
  return `₵ ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
};

const STATUS_LABELS: Record<AssetStatus, string> = {
  Active: 'Active',
  InUse: 'In Use',
  InStorage: 'In Storage',
  UnderMaintenance: 'Under Maintenance',
  Liquidated: 'Liquidated',
};

interface FormState {
  name: string;
  categoryId: number | '';
  serialNumber: string;
  purchaseDate: string;
  purchaseCost: string;
  currentValue: string;
  status: AssetStatus;
  description: string;
}

const Assets: React.FC = () => {
  const { setTitle, setCtas, searchQuery, setSearchQuery } = useLayout();

  // ── Filters & Query State ──────────────────────────────────────────────────
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | ''>('');

  const filters = useMemo(() => ({
    status: (selectedStatus as AssetStatus) || undefined,
    categoryId: selectedCategoryId !== '' ? Number(selectedCategoryId) : undefined,
  }), [selectedStatus, selectedCategoryId]);

  // ── Queries & Mutations ───────────────────────────────────────────────────
  const { data: assetsData, isLoading: isLoadingAssets } = useAssets(filters, 100);
  const assets = useMemo(() => assetsData?.data || [], [assetsData?.data]);

  const { data: categories = [] } = useAssetCategories();

  const createAsset = useCreateAsset();
  const updateAsset = useUpdateAsset();
  const deleteAsset = useDeleteAsset();
  const createCategory = useCreateAssetCategory();

  // ── Modals & Form State ───────────────────────────────────────────────────
  const [showModal, setShowModal] = useState(false);
  const [editingAsset, setEditingAsset] = useState<Asset | null>(null);

  const [formData, setFormData] = useState<FormState>({
    name: '',
    categoryId: '',
    serialNumber: '',
    purchaseDate: new Date().toISOString().split('T')[0],
    purchaseCost: '',
    currentValue: '',
    status: 'Active',
    description: '',
  });

  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingAsset, setDeletingAsset] = useState<Asset | null>(null);

  // New Category Modal
  const [showNewCategoryModal, setShowNewCategoryModal] = useState(false);
  const [newCategoryName, setNewCategoryName] = useState('');

  // ── Layout Header ─────────────────────────────────────────────────────────
  const handleOpenAddModal = () => {
    setEditingAsset(null);
    setFormData({
      name: '',
      categoryId: categories[0]?.id || '',
      serialNumber: '',
      purchaseDate: new Date().toISOString().split('T')[0],
      purchaseCost: '',
      currentValue: '',
      status: 'Active',
      description: '',
    });
    setShowModal(true);
  };

  useEffect(() => {
    setTitle('Assets');
    setCtas([
      { type: 'search', placeholder: 'Search assets...' },
      {
        type: 'button',
        label: 'Add Asset',
        icon: 'plus',
        variant: 'primary',
        onClick: handleOpenAddModal,
      },
    ]);
    setSearchQuery('');
  }, [setTitle, setCtas, setSearchQuery, categories]);

  // ── Calculations & Filtering ───────────────────────────────────────────────
  const totalAssetsCount = assets.length;
  const totalCurrentValue = useMemo(() => assets.reduce((sum, a) => sum + (a.currentValue || 0), 0), [assets]);
  const activeInUseCount = useMemo(
    () => assets.filter((a) => a.status === 'Active' || a.status === 'InUse').length,
    [assets]
  );
  const underMaintenanceCount = useMemo(
    () => assets.filter((a) => a.status === 'UnderMaintenance').length,
    [assets]
  );

  const filteredAssets = useMemo(() => {
    if (!searchQuery.trim()) return assets;
    const query = searchQuery.toLowerCase();
    return assets.filter(
      (a) =>
        a.name.toLowerCase().includes(query) ||
        (a.serialNumber && a.serialNumber.toLowerCase().includes(query)) ||
        (a.categoryName && a.categoryName.toLowerCase().includes(query))
    );
  }, [assets, searchQuery]);

  // ── Handlers ───────────────────────────────────────────────────────────────
  const handleEdit = (asset: Asset) => {
    setEditingAsset(asset);
    setFormData({
      name: asset.name,
      categoryId: asset.categoryId,
      serialNumber: asset.serialNumber || '',
      purchaseDate: asset.purchaseDate ? asset.purchaseDate.split('T')[0] : '',
      purchaseCost: String(asset.purchaseCost || 0),
      currentValue: String(asset.currentValue || 0),
      status: asset.status,
      description: asset.description || '',
    });
    setShowModal(true);
  };

  const handleDelete = (asset: Asset) => {
    setDeletingAsset(asset);
    setShowDeleteConfirm(true);
  };

  const confirmDelete = async () => {
    if (!deletingAsset) return;
    try {
      await deleteAsset.mutateAsync(deletingAsset.id);
      setShowDeleteConfirm(false);
      setDeletingAsset(null);
    } catch (err) {
      console.error('Failed to delete asset:', err);
    }
  };

  const handleSaveAsset = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name.trim() || !formData.categoryId) {
      alert('Please fill in the asset name and select a category.');
      return;
    }

    try {
      const payload: Partial<Asset> = {
        name: formData.name.trim(),
        categoryId: Number(formData.categoryId),
        serialNumber: formData.serialNumber.trim() || undefined,
        purchaseDate: formData.purchaseDate || undefined,
        purchaseCost: parseFloat(formData.purchaseCost) || 0,
        currentValue: parseFloat(formData.currentValue) || 0,
        status: formData.status,
        description: formData.description.trim() || undefined,
      };

      if (editingAsset) {
        await updateAsset.mutateAsync({ id: editingAsset.id, data: payload });
      } else {
        await createAsset.mutateAsync(payload);
      }

      setShowModal(false);
    } catch (err) {
      console.error('Failed to save asset:', err);
      alert('Failed to save asset.');
    }
  };

  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCategoryName.trim()) return;

    try {
      const created = await createCategory.mutateAsync({ name: newCategoryName.trim() });
      setFormData((prev) => ({ ...prev, categoryId: created.id }));
      setNewCategoryName('');
      setShowNewCategoryModal(false);
    } catch (err) {
      console.error('Failed to create category:', err);
      alert('Failed to create category.');
    }
  };

  return (
    <div className="assets-container">
      {/* ─── Summary Cards ──────────────────────────────────────────────────── */}
      <div className="assets-summary-cards">
        <div className="assets-summary-card">
          <span className="assets-card-label">Total Assets</span>
          <div className="assets-card-value">{totalAssetsCount}</div>
        </div>

        <div className="assets-summary-card">
          <span className="assets-card-label">Total Current Value</span>
          <div className="assets-card-value">{formatCurrency(totalCurrentValue)}</div>
        </div>

        <div className="assets-summary-card">
          <span className="assets-card-label">Active / In Use</span>
          <div className="assets-card-value">{activeInUseCount}</div>
        </div>

        <div className="assets-summary-card">
          <span className="assets-card-label">Under Maintenance</span>
          <div className="assets-card-value">{underMaintenanceCount}</div>
        </div>
      </div>

      {/* ─── Filters Row ────────────────────────────────────────────────────── */}
      <div className="assets-filters-row">
        <div className="assets-filter-group">
          <select
            className="assets-select"
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
          >
            <option value="">All Statuses</option>
            {Object.keys(STATUS_LABELS).map((status) => (
              <option key={status} value={status}>
                {STATUS_LABELS[status as AssetStatus]}
              </option>
            ))}
          </select>

          <select
            className="assets-select"
            value={selectedCategoryId}
            onChange={(e) => setSelectedCategoryId(e.target.value ? Number(e.target.value) : '')}
          >
            <option value="">All Categories</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>

        <button
          type="button"
          className="assets-btn assets-btn-secondary"
          onClick={() => setShowNewCategoryModal(true)}
        >
          + Add Category
        </button>
      </div>

      {/* ─── Table Section ──────────────────────────────────────────────────── */}
      <div className="assets-table-card">
        <div className="assets-table-header">
          <h3 className="assets-table-title">Inventory Records</h3>
        </div>

        <div className="assets-table-container">
          {isLoadingAssets ? (
            <div className="assets-empty">Loading assets...</div>
          ) : filteredAssets.length === 0 ? (
            <div className="assets-empty">No assets found matching the criteria.</div>
          ) : (
            <table className="assets-table">
              <thead>
                <tr>
                  <th>Asset Name</th>
                  <th>Category</th>
                  <th>Serial Number</th>
                  <th>Purchase Cost</th>
                  <th>Current Value</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredAssets.map((asset) => (
                  <tr key={asset.id}>
                    <td className="asset-name-cell">{asset.name}</td>
                    <td>{asset.categoryName || 'General'}</td>
                    <td className="asset-serial-cell">{asset.serialNumber || '—'}</td>
                    <td>{formatCurrency(asset.purchaseCost)}</td>
                    <td className="asset-cost-cell">{formatCurrency(asset.currentValue)}</td>
                    <td>
                      <span className={`asset-status-badge ${asset.status.toLowerCase()}`}>
                        {STATUS_LABELS[asset.status] || asset.status}
                      </span>
                    </td>
                    <td>
                      <div className="assets-table-actions">
                        <button
                          type="button"
                          className="action-btn"
                          onClick={() => handleEdit(asset)}
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          className="action-btn delete"
                          onClick={() => handleDelete(asset)}
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
      </div>

      {/* ─── Add / Edit Modal ───────────────────────────────────────────────── */}
      {showModal && (
        <div className="assets-panel-overlay" onClick={() => setShowModal(false)}>
          <form
            className="assets-side-panel"
            onClick={(e) => e.stopPropagation()}
            onSubmit={handleSaveAsset}
          >
            <div className="assets-panel-header">
              <h2 className="assets-panel-title">
                {editingAsset ? 'Edit Asset' : 'Record Asset'}
              </h2>
              <button
                type="button"
                className="assets-panel-close"
                onClick={() => setShowModal(false)}
                aria-label="Close modal"
              >
                <CloseIcon />
              </button>
            </div>

            <div className="assets-panel-body">
              <div className="assets-form-group">
                <label className="assets-form-label">Asset Name</label>
                <input
                  type="text"
                  className="assets-form-input"
                  placeholder="e.g. Yamaha Audio Mixer, Projector"
                  value={formData.name}
                  onChange={(e) => setFormData((prev) => ({ ...prev, name: e.target.value }))}
                  required
                  autoFocus
                />
              </div>

              <div className="assets-form-row">
                <div className="assets-form-group">
                  <label className="assets-form-label">Category</label>
                  <select
                    className="assets-form-select"
                    value={formData.categoryId}
                    onChange={(e) => setFormData((prev) => ({ ...prev, categoryId: Number(e.target.value) || '' }))}
                    required
                  >
                    <option value="">Select Category...</option>
                    {categories.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="assets-form-group">
                  <label className="assets-form-label">Status</label>
                  <select
                    className="assets-form-select"
                    value={formData.status}
                    onChange={(e) => setFormData((prev) => ({ ...prev, status: e.target.value as AssetStatus }))}
                    required
                  >
                    {Object.keys(STATUS_LABELS).map((s) => (
                      <option key={s} value={s}>
                        {STATUS_LABELS[s as AssetStatus]}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="assets-form-row">
                <div className="assets-form-group">
                  <label className="assets-form-label">Purchase Cost</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    className="assets-form-input"
                    placeholder="₵ 0.00"
                    value={formData.purchaseCost}
                    onChange={(e) => setFormData((prev) => ({ ...prev, purchaseCost: e.target.value }))}
                    required
                  />
                </div>

                <div className="assets-form-group">
                  <label className="assets-form-label">Current Value</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    className="assets-form-input"
                    placeholder="₵ 0.00"
                    value={formData.currentValue}
                    onChange={(e) => setFormData((prev) => ({ ...prev, currentValue: e.target.value }))}
                    required
                  />
                </div>
              </div>

              <div className="assets-form-row">
                <div className="assets-form-group">
                  <label className="assets-form-label">Serial / Tag Number <span>(Optional)</span></label>
                  <input
                    type="text"
                    className="assets-form-input"
                    placeholder="e.g. SN-89234"
                    value={formData.serialNumber}
                    onChange={(e) => setFormData((prev) => ({ ...prev, serialNumber: e.target.value }))}
                  />
                </div>

                <div className="assets-form-group">
                  <label className="assets-form-label">Purchase Date <span>(Optional)</span></label>
                  <input
                    type="date"
                    className="assets-form-input"
                    value={formData.purchaseDate}
                    onChange={(e) => setFormData((prev) => ({ ...prev, purchaseDate: e.target.value }))}
                  />
                </div>
              </div>

              <div className="assets-form-group">
                <label className="assets-form-label">Description <span>(Optional)</span></label>
                <textarea
                  className="assets-form-textarea"
                  placeholder="Location details, condition notes..."
                  value={formData.description}
                  onChange={(e) => setFormData((prev) => ({ ...prev, description: e.target.value }))}
                />
              </div>
            </div>

            <div className="assets-panel-footer">
              <button
                type="button"
                className="assets-btn assets-btn-secondary"
                onClick={() => setShowModal(false)}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="assets-btn assets-btn-primary"
                disabled={createAsset.isPending || updateAsset.isPending}
              >
                <RecordIcon />{' '}
                {createAsset.isPending || updateAsset.isPending
                  ? 'Saving...'
                  : editingAsset
                  ? 'Save Changes'
                  : 'Record Asset'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* ─── New Category Modal ──────────────────────────────────────────────── */}
      {showNewCategoryModal && (
        <div
          className="assets-panel-overlay"
          onClick={() => setShowNewCategoryModal(false)}
          style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
        >
          <div
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{
              background: 'white',
              borderRadius: '16px',
              padding: '24px',
              width: '100%',
              maxWidth: '440px',
              boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1)',
            }}
          >
            <h3 style={{ fontSize: '18px', fontWeight: 600, marginBottom: '16px' }}>
              Create Asset Category
            </h3>
            <form onSubmit={handleCreateCategory}>
              <div style={{ marginBottom: '20px' }}>
                <label style={{ display: 'block', fontSize: '13px', fontWeight: 500, marginBottom: '6px' }}>
                  Category Name
                </label>
                <input
                  type="text"
                  className="assets-form-input"
                  placeholder="e.g. Musical Instruments, Audio Equipment, Furniture"
                  value={newCategoryName}
                  onChange={(e) => setNewCategoryName(e.target.value)}
                  required
                  autoFocus
                />
              </div>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
                <button
                  type="button"
                  className="assets-btn assets-btn-secondary"
                  onClick={() => setShowNewCategoryModal(false)}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="assets-btn assets-btn-primary"
                  disabled={createCategory.isPending}
                >
                  {createCategory.isPending ? 'Creating...' : 'Create Category'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      <DeleteConfirmModal
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={confirmDelete}
        itemName={deletingAsset?.name || 'Asset'}
        title="Delete Asset"
        confirmText="Delete Asset"
      />
    </div>
  );
};

export default Assets;
