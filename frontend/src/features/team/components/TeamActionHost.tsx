import { useMe } from "@/features/account";
import { Presence, useHeld } from "@/ui";
import type { TeamUser } from "../api/team";
import { DisableDialog, EnableDialog, RemoveDialog } from "./AccountDialogs";
import { ChangeRoleDialog } from "./ChangeRoleDialog";
import { PersonSheet } from "./PersonSheet";
import type { RowAction } from "./RoleCard";

export type TeamAction = { kind: RowAction; user: TeamUser };

type Props = {
  action: TeamAction | null;
  onAct: (action: RowAction, user: TeamUser) => void;
  onClose: () => void;
};

type DialogProps = { user: TeamUser; onClose: () => void };

// The dialogs that only need the person; "remove" also needs a way to switch to disabling.
const dialogs: Partial<Record<RowAction, (p: DialogProps) => React.ReactNode>> =
  {
    role: (p) => <ChangeRoleDialog {...p} />,
    disable: (p) => <DisableDialog {...p} />,
    enable: (p) => <EnableDialog {...p} />,
  };

// Shows the panel or dialog for what an admin asked to do to an account, and lets it play its exit.
export function TeamActionHost({ action, onAct, onClose }: Props) {
  const held = useHeld(action);
  const user = held?.user;
  return (
    <>
      <Presence open={action?.kind === "view"}>
        {user && <ViewSheet user={user} onAct={onAct} onClose={onClose} />}
      </Presence>
      {(Object.keys(dialogs) as RowAction[]).map((kind) => (
        <Presence key={kind} open={action?.kind === kind} ms={160}>
          {user && dialogs[kind]?.({ user, onClose })}
        </Presence>
      ))}
      <Presence open={action?.kind === "remove"} ms={160}>
        {user && (
          <RemoveDialog
            user={user}
            onClose={onClose}
            onDisableInstead={() => onAct("disable", user)}
          />
        )}
      </Presence>
    </>
  );
}

function ViewSheet({
  user,
  onAct,
  onClose,
}: DialogProps & { onAct: Props["onAct"] }) {
  const me = useMe();
  return (
    <PersonSheet
      user={user}
      isMe={user.id === me.data?.id}
      onAct={(kind) => onAct(kind, user)}
      onClose={onClose}
    />
  );
}
