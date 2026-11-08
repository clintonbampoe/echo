import { useState } from "react";
import { roleLabel, type UserRole } from "@/features/account";
import { ConfirmDialog, SelectField, useToast } from "@/ui";
import type { StaffUser } from "../api/staff";
import { useChangeRole } from "../hooks";
import { messageFor } from "../model/errors";
import { roleOptions } from "../model/roles";

type Props = { user: StaffUser; onClose: () => void };

// Asks for the new role, then changes it. The change takes effect on their next page load.
export function ChangeRoleDialog({ user, onClose }: Props) {
  const change = useChangeRole();
  const toast = useToast();
  const [role, setRole] = useState<UserRole>(user.role);
  const [error, setError] = useState<string>();

  async function confirm() {
    setError(undefined);
    try {
      await change.mutateAsync({ id: user.id, role });
      onClose();
      toast({ message: `${user.name} is now ${roleLabel(role)}` });
    } catch (e) {
      setError(messageFor(e));
    }
  }

  return (
    <ConfirmDialog
      title={`Change ${user.name}'s role`}
      confirmLabel="Save role"
      cancelLabel="Cancel"
      confirmDisabled={role === user.role}
      busy={change.isPending}
      error={error}
      onConfirm={() => void confirm()}
      onCancel={onClose}
    >
      <SelectField
        label="Role"
        options={roleOptions}
        value={role}
        onChange={(v) => setRole(v as UserRole)}
      />
      <p>Their access changes straight away.</p>
    </ConfirmDialog>
  );
}
