import React from 'react';
import { CloseIcon, WarningIcon } from '../Icons';
import '../../styles/DeleteConfirmModal.css';

interface DeleteConfirmModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  itemName: string;
  title?: string;
  warningMessage?: string;
  confirmText?: string;
}

const DeleteConfirmModal: React.FC<DeleteConfirmModalProps> = ({
  isOpen,
  onClose,
  onConfirm,
  itemName,
  title = "Delete Item",
  warningMessage = "This action cannot be undone and all associated data will be permanently removed.",
  confirmText = "Delete"
}) => {
  if (!isOpen) return null;

  return (
    <div className="delete-modal-overlay" onClick={onClose}>
      <div className="delete-modal" onClick={e => e.stopPropagation()}>
        <div className="delete-modal-header">
          <h2 className="delete-modal-title">{title}</h2>
          <button className="delete-modal-close" onClick={onClose}>
            <CloseIcon />
          </button>
        </div>
        <div className="delete-modal-body">
          <div className="delete-modal-icon-container">
            <WarningIcon size={28} />
          </div>
          <p className="delete-modal-text">
            Are you sure you want to delete{' '}
            <span className="delete-modal-name">"{itemName}"</span>?
          </p>
          <p className="delete-modal-warning">
            {warningMessage}
          </p>
        </div>
        <div className="delete-modal-footer">
          <button className="delete-modal-btn delete-modal-btn-secondary" onClick={onClose}>
            Cancel
          </button>
          <button className="delete-modal-btn delete-modal-btn-danger" onClick={onConfirm}>
            {confirmText}
          </button>
        </div>
      </div>
    </div>
  );
};

export default DeleteConfirmModal;
