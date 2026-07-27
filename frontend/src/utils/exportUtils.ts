/**
 * exportUtils.ts
 *
 * Pure, framework-agnostic export utilities.
 * All functions are stateless so they can be reused across any page.
 */

// ─── Types ────────────────────────────────────────────────────────────────────

export type ExportFormat = 'csv' | 'pdf' | 'excel';

export type DateRange = 'last_30' | 'last_90' | 'this_year' | 'custom';

export interface ExportOptions {
  /** Human-readable title for the report (used in PDF/Excel) */
  title: string;
  /** The format to produce */
  format: ExportFormat;
  /** Suggested filename WITHOUT extension */
  filename: string;
  /** Column definitions: key = object property, label = header text */
  columns: Array<{ key: string; label: string }>;
  /** Row data — each object must have all keys listed in `columns` */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  rows: Record<string, any>[];
}

// ─── CSV ──────────────────────────────────────────────────────────────────────

/**
 * Converts column + row data into a RFC-4180-compliant CSV string.
 */
export function buildCsv(
  columns: ExportOptions['columns'],
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  rows: Record<string, any>[]
): string {
  const escape = (val: unknown): string => {
    const str = val == null ? '' : String(val);
    // Wrap in quotes if the value contains commas, quotes, or newlines
    if (str.includes(',') || str.includes('"') || str.includes('\n')) {
      return `"${str.replace(/"/g, '""')}"`;
    }
    return str;
  };

  const header = columns.map(c => escape(c.label)).join(',');
  const body = rows
    .map(row => columns.map(c => escape(row[c.key])).join(','))
    .join('\n');

  return `${header}\n${body}`;
}

// ─── Download trigger ─────────────────────────────────────────────────────────

/**
 * Triggers a browser download of the given content blob.
 */
export function downloadFile(content: string, filename: string, mimeType: string): void {
  const blob = new Blob([content], { type: mimeType });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

// ─── Main dispatcher ──────────────────────────────────────────────────────────

/**
 * Exports data in the requested format.
 *
 * - `csv`   → real download in the browser
 * - `pdf`   → stub (logs to console; wire up a PDF library when ready)
 * - `excel` → stub (logs to console; wire up SheetJS/xlsx when ready)
 */
export function exportData(options: ExportOptions): void {
  const { title, format, filename, columns, rows } = options;

  switch (format) {
    case 'csv': {
      const csv = buildCsv(columns, rows);
      downloadFile(csv, `${filename}.csv`, 'text/csv;charset=utf-8;');
      break;
    }

    case 'pdf': {
      // TODO: integrate a PDF library (e.g., jsPDF, react-pdf)
      console.info(`[Export] PDF requested for "${title}". Wire up a PDF library to fulfil this.`);
      alert(`PDF export for "${title}" is not yet implemented.`);
      break;
    }

    case 'excel': {
      // TODO: integrate SheetJS / xlsx
      console.info(`[Export] Excel requested for "${title}". Wire up SheetJS to fulfil this.`);
      alert(`Excel export for "${title}" is not yet implemented.`);
      break;
    }

    default:
      console.warn(`[Export] Unknown format: ${format}`);
  }
}
