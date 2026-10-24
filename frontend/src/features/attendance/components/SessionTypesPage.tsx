import { useState } from "react";
import { plainList } from "@/lib/paged";
import { paths } from "@/lib/paths";
import {
  ActionMenu,
  BackLink,
  Button,
  Chip,
  DeleteDialog,
  Icon,
  PageHeader,
  RecordTable,
  type Column,
} from "@/ui";
import type { SessionLabel } from "../api";
import { useDeleteSessionLabel, useSessionLabels } from "../hooks";
import { SessionLabelSheet } from "./SessionLabelSheet";
import styles from "./SessionTypesPage.module.css";

type Open =
  { kind: "new" } | { kind: "edit" | "delete"; label: SessionLabel } | null;

function typeColumns(
  onEdit: (label: SessionLabel) => void,
  onDelete: (label: SessionLabel) => void,
): Column<SessionLabel>[] {
  return [
    { header: "Name", cell: (l) => l.name },
    {
      header: "Category",
      cell: (l) => <Chip tone="outline">{l.category}</Chip>,
    },
    {
      header: "",
      end: true,
      cell: (l) => (
        <ActionMenu
          label={`Actions for ${l.name}`}
          title={l.name}
          items={[
            { label: "Edit", onSelect: () => onEdit(l) },
            {
              label: "Delete",
              danger: true,
              separated: true,
              onSelect: () => onDelete(l),
            },
          ]}
        />
      ),
    },
  ];
}

// The kinds of gathering the church holds. Sessions are created from these, so the list comes first.
export function SessionTypesPage() {
  const labels = useSessionLabels();
  const remove = useDeleteSessionLabel();
  const [open, setOpen] = useState<Open>(null);
  const close = () => setOpen(null);

  const list = plainList(labels.data ?? [], labels);
  const columns = typeColumns(
    (label) => setOpen({ kind: "edit", label }),
    (label) => setOpen({ kind: "delete", label }),
  );

  return (
    <div className="anim-rise">
      <BackLink to={paths.attendance}>Attendance</BackLink>
      <PageHeader
        title="Session types"
        action={
          <Button onClick={() => setOpen({ kind: "new" })}>
            <Icon name="plus" size={16} />
            New session type
          </Button>
        }
      />
      <div className={styles.note}>
        <RecordTable
          label="Session types"
          columns={columns}
          list={list}
          rowKey={(l) => String(l.id)}
          empty={{
            icon: "calendar-check",
            title: "No session types yet",
            text: "Add the kinds of gathering you hold, like Sunday Service or Bible Study. Then you can create sessions from them.",
          }}
          noun={["session type", "session types"]}
          large
        />
      </div>
      {open?.kind === "new" && <SessionLabelSheet onClose={close} />}
      {open?.kind === "edit" && (
        <SessionLabelSheet label={open.label} onClose={close} />
      )}
      {open?.kind === "delete" && (
        <DeleteDialog
          title={`Delete the ${open.label.name} type?`}
          noun="type"
          onDelete={() => remove.mutateAsync(open.label.id)}
          onClose={close}
        >
          <p>You will not be able to pick it for new sessions.</p>
          <p>
            Sessions that already use it keep their history and still show the
            type name.
          </p>
        </DeleteDialog>
      )}
    </div>
  );
}
