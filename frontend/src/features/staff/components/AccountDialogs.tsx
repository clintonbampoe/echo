import { useState } from "react";
import { Button, ConfirmDialog, DeleteDialog, useToast } from "@/ui";
import type { StaffUser } from "../api/staff";
import { useDisableUser, useEnableUser, useRemoveUser } from "../hooks";
import { messageFor } from "../model/errors";

type Props = { user: StaffUser; onClose: () => void };

const first = (user: StaffUser) => user.name.split(" ")[0] ?? user.name;

// Reversible, so the confirm button is ink, not red.
export function DisableDialog({ user, onClose }: Props) {
  const disable = useDisableUser();
  const toast = useToast();
  const [error, setError] = useState<string>();

  async function confirm() {
    setError(undefined);
    try {
      await disable.mutateAsync(user.id);
      onClose();
      toast({ message: `${user.name}'s account is disabled` });
    } catch (e) {
      setError(messageFor(e));
    }
  }

  return (
    <ConfirmDialog
      title={`Disable ${user.name}'s account?`}
      confirmLabel="Disable account"
      cancelLabel="Cancel"
      busy={disable.isPending}
      error={error}
      onConfirm={() => void confirm()}
      onCancel={onClose}
    >
      <p>
        {first(user)} will not be able to sign in, and will be signed out of
        every device straight away. Their records stay exactly as they are. You
        can enable the account again at any time.
      </p>
    </ConfirmDialog>
  );
}

export function EnableDialog({ user, onClose }: Props) {
  const enable = useEnableUser();
  const toast = useToast();
  const [failed, setFailed] = useState(false);

  async function confirm() {
    setFailed(false);
    try {
      await enable.mutateAsync(user.id);
      onClose();
      toast({ message: `${user.name}'s account is enabled` });
    } catch {
      setFailed(true);
    }
  }

  return (
    <ConfirmDialog
      title={`Enable ${user.name}'s account?`}
      confirmLabel="Enable account"
      cancelLabel="Cancel"
      busy={enable.isPending}
      error={
        failed ? "We could not enable this account. Try again." : undefined
      }
      onConfirm={() => void confirm()}
      onCancel={onClose}
    >
      <p>
        {first(user)} can sign in again with their usual password. The devices
        they were signed out of stay signed out.
      </p>
    </ConfirmDialog>
  );
}

export function RemoveDialog({
  user,
  onClose,
  onDisableInstead,
}: Props & { onDisableInstead: () => void }) {
  const remove = useRemoveUser();
  const toast = useToast();
  return (
    <DeleteDialog
      title={`Remove ${user.name}'s account?`}
      noun="account"
      confirmLabel="Remove account"
      cancelLabel="Cancel"
      onDelete={() => remove.mutateAsync(user.id)}
      onClose={onClose}
      onDeleted={() => toast({ message: `${user.name}'s account was removed` })}
    >
      <p>
        {first(user)} will lose access to Echo for good. Records they created,
        such as attendance and tithes, are kept. Removing cannot be undone from
        Echo.
      </p>
      <p>
        Want to keep the account?{" "}
        <Button variant="ghost" size="sm" onClick={onDisableInstead}>
          Disable it instead
        </Button>
      </p>
    </DeleteDialog>
  );
}
