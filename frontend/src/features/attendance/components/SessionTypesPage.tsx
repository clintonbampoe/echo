import { useState } from "react";
import { plainList } from "@/lib/paged";
import { paths } from "@/lib/paths";
import {
  ActionMenu,
  BackLink,
  IconTile,
  DeleteDialog,
  PageHeader,
  RecordTable,
} from "@/ui";
import type { SessionLabel } from "../api";
import { useDeleteSessionLabel, useSessionLabels } from "../hooks";
import { SessionLabelSheet } from "./SessionLabelSheet";

type Open =
  { kind: "new" } | { kind: "edit" | "delete"; label: SessionLabel } | null;

// The kinds of gathering the church holds. Sessions are created from these, so the list comes first.
export function SessionTypesPage() {
  const labels = useSessionLabels();
  const remove = useDeleteSessionLabel();
  const [open, setOpen] = useState<Open>(null);
  const close = () => setOpen(null);

  const list = plainList(labels.data ?? [], labels);

  return (
    <div className="anim-rise">
      <BackLink to={paths.attendance}>Attendance</BackLink>
      <PageHeader
        title="Session types"
        add={{
          label: "New session type",
          onClick: () => setOpen({ kind: "new" }),
        }}
      />
      <RecordTable
        label="Session types"
        columns={[{ header: "Name", cell: (l) => l.name }]}
        list={list}
        onOpen={(l) => setOpen({ kind: "edit", label: l })}
        rowName={(l) => l.name}
        line={(l) => ({
          lead: <IconTile icon="calendar-check" />,
          title: l.name,
          sub: l.category,
          end: (
            <ActionMenu
              label={`Actions for ${l.name}`}
              title={l.name}
              items={[
                {
                  label: "Edit",
                  onSelect: () => setOpen({ kind: "edit", label: l }),
                },
                {
                  label: "Delete",
                  danger: true,
                  separated: true,
                  onSelect: () => setOpen({ kind: "delete", label: l }),
                },
              ]}
            />
          ),
        })}
        rowKey={(l) => String(l.id)}
        empty={{
          icon: "calendar-check",
          title: "No session types yet",
          text: "Add the kinds of gathering you hold, like Sunday Service or Bible Study. Then you can create sessions from them.",
        }}
        noun={["session type", "session types"]}
        large
      />
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
