import { useState, type FormEventHandler, type ReactNode } from "react";
import { Alert } from "../feedback/Alert";
import { Button } from "../layout/Button";
import { ConfirmDialog } from "./ConfirmDialog";
import { Sheet } from "./Sheet";
import styles from "./FormSheet.module.css";

type Props = {
  title: string;
  subtitle?: string;
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
  // True when saving would be refused (say, a duplicate that is already known), so the save buttons are off.
  blocked?: boolean;
  // A second way to save, e.g. "Save and add another".
  another?: { label: string; onClick: () => void };
  // The fields.
  children: ReactNode;
};

// A sheet holding one form, with the footer every form shares: what is wrong and the save buttons. The X in the corner is the one way out.
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
  blocked,
  problem,
  another,
  children,
}: Props) {
  // Typing is remembered with the note showing at the time: once a save shows a new note, the form counts as clean.
  const [typedAt, setTypedAt] = useState<{ note?: string } | null>(null);
  const dirty = typedAt !== null && typedAt.note === note;
  const [shaking, setShaking] = useState(false);
  const [asking, setAsking] = useState(false);

  // Closing a form that has been typed into shakes the sheet and asks first. An untouched form just closes.
  function requestClose() {
    if (!dirty || pending) return onClose();
    setShaking(true);
    window.setTimeout(() => setShaking(false), 280);
    setAsking(true);
  }

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
      {another && (
        <Button
          variant="secondary"
          disabled={pending || blocked}
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
        disabled={blocked}
        className={styles.primary}
      >
        {submitLabel}
      </Button>
    </div>
  );
  return (
    <Sheet
      open
      onClose={requestClose}
      title={title}
      subtitle={subtitle}
      footer={footer}
      shaking={shaking}
    >
      {problem && (
        <div className={styles.alert}>
          <Alert title={problem.title}>{problem.message}</Alert>
        </div>
      )}
      <form
        id={formId}
        noValidate
        onSubmit={onSubmit}
        onChange={() => setTypedAt({ note })}
      >
        {children}
      </form>
      {asking && (
        <ConfirmDialog
          title="Discard your changes?"
          confirmLabel="Discard"
          cancelLabel="Keep editing"
          tone="danger"
          onConfirm={onClose}
          onCancel={() => setAsking(false)}
        >
          What you typed here has not been saved.
        </ConfirmDialog>
      )}
    </Sheet>
  );
}
