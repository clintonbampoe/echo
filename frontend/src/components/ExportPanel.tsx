import React, { useState } from 'react';
import { CloseIcon, DocumentIcon, TableIcon, CsvIcon, InfoIcon } from './Icons';
import { exportData } from '../utils/exportUtils';
import type { ExportFormat, DateRange, ExportOptions } from '../utils/exportUtils';
import '../styles/ExportPanel.css';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ExportPanelProps {
  /** Panel title shown in the header */
  title?: string;
  /** The exportData options, minus `format` (the panel controls that) */
  exportConfig: Omit<ExportOptions, 'format'>;
  /** Called when the overlay or Cancel is clicked */
  onClose: () => void;
}

// ─── ExportPanel component ────────────────────────────────────────────────────

const ExportPanel: React.FC<ExportPanelProps> = ({
  title = 'Export Report',
  exportConfig,
  onClose,
}) => {
  const [format, setFormat] = useState<ExportFormat>('csv');
  const [dateRange, setDateRange] = useState<DateRange>('last_30');

  const handleExport = () => {
    exportData({ ...exportConfig, format });
    onClose();
  };

  return (
    <div className="ep-overlay" onClick={onClose}>
      <div className="ep-panel" onClick={e => e.stopPropagation()}>

        {/* Header */}
        <div className="ep-header">
          <div>
            <h2 className="ep-title">{title}</h2>
            <p className="ep-subtitle">Choose options and download your report</p>
          </div>
          <button className="ep-close" onClick={onClose} aria-label="Close">
            <CloseIcon size={20} />
          </button>
        </div>

        {/* Body */}
        <div className="ep-body">

          {/* Info hint */}
          <div className="ep-hint">
            <InfoIcon size={16} />
            Select a date range and export format to download your report.
          </div>

          {/* Date Range */}
          <div className="ep-form-group">
            <label className="ep-label">Date Range</label>
            <select
              className="ep-select"
              value={dateRange}
              onChange={e => setDateRange(e.target.value as DateRange)}
            >
              <option value="last_30">Last 30 Days</option>
              <option value="last_90">Last 90 Days</option>
              <option value="this_year">This Year</option>
              <option value="custom">Custom Range</option>
            </select>
          </div>

          {/* Export Format */}
          <div className="ep-form-group">
            <label className="ep-label">Export Format</label>
            <div className="ep-format-group">

              <button
                className={`ep-format-card ${format === 'pdf' ? 'selected' : ''}`}
                onClick={() => setFormat('pdf')}
              >
                <div className="ep-format-icon"><DocumentIcon size={24} /></div>
                <span className="ep-format-label">PDF</span>
              </button>

              <button
                className={`ep-format-card ${format === 'excel' ? 'selected' : ''}`}
                onClick={() => setFormat('excel')}
              >
                <div className="ep-format-icon"><TableIcon size={24} /></div>
                <span className="ep-format-label">Excel</span>
              </button>

              <button
                className={`ep-format-card ${format === 'csv' ? 'selected' : ''}`}
                onClick={() => setFormat('csv')}
              >
                <div className="ep-format-icon"><CsvIcon size={24} /></div>
                <span className="ep-format-label">CSV</span>
              </button>

            </div>
          </div>

        </div>

        {/* Footer */}
        <div className="ep-footer">
          <button className="ep-btn ep-btn-secondary" onClick={onClose}>
            Cancel
          </button>
          <button className="ep-btn ep-btn-primary" onClick={handleExport}>
            Export Report
          </button>
        </div>

      </div>
    </div>
  );
};

export default ExportPanel;
