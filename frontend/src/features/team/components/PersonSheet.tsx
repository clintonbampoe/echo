import { roleLabel } from "@/features/account";
import { Button, Chip, FactList, Sheet } from "@/ui";
import type { TeamUser } from "../api/team";
import { dayOf, isDisabled, statusText } from "../model/display";
import type { RowAction } from "./RoleCard";
import styles from "./PersonSheet.module.css";

type Props = {
  user: TeamUser;
  isMe: boolean;
  onAct: (action: RowAction) => void;
  onClose: () => void;
};

// One person, read only, with the things an admin can do to the account (design rule S11).
export function PersonSheet({ user, isMe, onAct, onClose }: Props) {
  return (
    <Sheet
      open
      onClose={onClose}
      title="Account"
      subtitle={user.emailAddress}
      footer={isMe ? undefined : <Actions user={user} onAct={onAct} />}
    >
      <div className={styles.body}>
        <p className={styles.name}>{user.name}</p>
        <div className={styles.chips}>
          <Chip tone="strong">{roleLabel(user.role)}</Chip>
          {isMe && <Chip tone="outline">You</Chip>}
          {isDisabled(user) && <Chip>Disabled</Chip>}
          {!user.verifiedAt && <Chip tone="outline">Unverified</Chip>}
        </div>
        <FactList
          rows={[
            ["Role", roleLabel(user.role)],
            ["Status", statusText(user)],
            [
              "Email verified",
              user.verifiedAt ? dayOf(user.verifiedAt) : "Not yet",
            ],
            ["Joined", dayOf(user.createdAt)],
          ]}
        />
        {isMe && (
          <p className={styles.note}>
            This is your account. Another admin can change it.
          </p>
        )}
      </div>
    </Sheet>
  );
}

function Actions({
  user,
  onAct,
}: {
  user: TeamUser;
  onAct: (action: RowAction) => void;
}) {
  return (
    <div className={styles.footer}>
      <Button block onClick={() => onAct("role")}>
        Change role
      </Button>
      <div className={styles.pair}>
        <Button
          block
          variant="secondary"
          onClick={() => onAct(isDisabled(user) ? "enable" : "disable")}
        >
          {isDisabled(user) ? "Enable account" : "Disable account"}
        </Button>
        <Button block variant="danger" onClick={() => onAct("remove")}>
          Remove account
        </Button>
      </div>
    </div>
  );
}
