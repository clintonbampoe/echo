import type { FormEventHandler, ReactNode } from "react";
import { Alert } from "./Alert";
import { Button } from "./Button";
import { Sheet } from "./Sheet";
import styles from "./FormSheet.module.css";

type Props = {
  title: string;
  subtitle: string;
  onClose: () => void;
  // Ties the footer's save button to the form.
  formId: string;
  onSubmit: FormEventHandler<HTMLFormElement>;
  // The label of the main button, e.g. "Create member".
  submitLabel: string;
  pending: boolean;
  // How many fields are failing validation.
  failures: number;
  // A line of good news or a reminder next to the buttons, e.g. "Ama was added." Hidden while fields fail.
  note?: string;
  // A server error that has no field to sit on.
  problem?: { title: string; message: string } | null;
  // A second way to save, e.g. "Save and add another".
  another?: { label: string; onClick: () => void };
  // The fields.
  children: ReactNode;
};

// A sheet holding one form, with the footer every form shares: what is wrong, Cancel, and the save buttons.
// The form only exists while the sheet is mounted, so it starts blank or filled every time it opens.
export function FormSheet({
  title,
  subtitle,
  onClose,
  formId,
  onSubmit,
  submitLabel,
  pending,
  failures,
  note,
  problem,
  another,
  children,
}: Props) {
  const footer = (
    <div className={styles.actions}>
      {failures > 0 && (
        <p role="alert" className={styles.problem}>
          Fix {failures} {failures === 1 ? "field" : "fields"} to continue
        </p>
      )}
      {failures === 0 && note && (
        <p role="status" className={styles.note}>
          {note}
        </p>
      )}
      <Button variant="secondary" onClick={onClose} className={styles.cancel}>
        Cancel
      </Button>
      {another && (
        <Button
          variant="secondary"
          disabled={pending}
          onClick={another.onClick}
          className={styles.another}
        >
          {another.label}
        </Button>
      )}
      <Button
        type="submit"
        form={formId}
        loading={pending}
        className={styles.primary}
      >
        {submitLabel}
      </Button>
    </div>
  );
  return (
    <Sheet
      open
      onClose={onClose}
      title={title}
      subtitle={subtitle}
      footer={footer}
    >
      {problem && (
        <div className={styles.alert}>
          <Alert title={problem.title}>{problem.message}</Alert>
        </div>
      )}
      <form id={formId} noValidate onSubmit={onSubmit}>
        {children}
      </form>
    </Sheet>
  );
}
