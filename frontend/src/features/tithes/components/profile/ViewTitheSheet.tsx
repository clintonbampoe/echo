import { formatDate, formatMoney } from "@/lib/format";
import { paymentLabel } from "@/features/transactions";
import { Button, DetailGroup, Sheet } from "@/ui";
import type { Tithe } from "../../api/tithes";
import { MemberCard } from "./MemberCard";
import styles from "./ViewTitheSheet.module.css";

type Props = {
  tithe: Tithe;
  onEdit: () => void;
  onDelete: () => void;
  onClose: () => void;
};

// A tithe as it was recorded, read only. Changing or removing it is a deliberate next step.
export function ViewTitheSheet({ tithe, onEdit, onDelete, onClose }: Props) {
  const footer = (
    <div className={styles.actions}>
      <Button variant="secondary" onClick={onDelete} className={styles.delete}>
        Delete tithe
      </Button>
      <Button onClick={onEdit}>Edit tithe</Button>
    </div>
  );
  return (
    <Sheet
      open
      onClose={onClose}
      title="Tithe"
      subtitle={`For ${tithe.forMonth} ${tithe.forYear}`}
      footer={footer}
    >
      <div className={styles.body}>
        <MemberCard id={tithe.memberId} name={tithe.memberName} />
        <p className={styles.amount}>{formatMoney(tithe.amount)}</p>
        <DetailGroup
          title="Details"
          rows={[
            ["Tithe for", `${tithe.forMonth} ${tithe.forYear}`],
            ["Collected on", formatDate(tithe.collectionDate)],
            ["Payment method", paymentLabel[tithe.paymentMethod]],
            ["Note", tithe.description],
            ["Recorded on", formatDate(tithe.createdAt.slice(0, 10))],
          ]}
        />
      </div>
    </Sheet>
  );
}
