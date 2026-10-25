import { paymentLabel } from "@/features/transactions";
import { formatDate, formatMoney } from "@/lib/format";
import { Button, DetailGroup, Sheet } from "@/ui";
import type { Contribution } from "../../api/contributions";
import styles from "./ViewContributionSheet.module.css";

type Props = {
  contribution: Contribution;
  onEdit: () => void;
  onDelete: () => void;
  onClose: () => void;
};

// A contribution as it was recorded, read only. Changing or removing it is a deliberate next step.
export function ViewContributionSheet({
  contribution: c,
  onEdit,
  onDelete,
  onClose,
}: Props) {
  const footer = (
    <div className={styles.actions}>
      <Button variant="secondary" onClick={onDelete} className={styles.delete}>
        Delete contribution
      </Button>
      <Button onClick={onEdit}>Edit contribution</Button>
    </div>
  );
  return (
    <Sheet
      open
      onClose={onClose}
      title="Contribution"
      subtitle={`Towards ${c.projectName}`}
      footer={footer}
    >
      <div className={styles.body}>
        <p className={styles.amount}>{formatMoney(c.amount)}</p>
        <DetailGroup
          title="Details"
          rows={[
            ["Project", c.projectName],
            ["Date", formatDate(c.dateContributed.slice(0, 10))],
            ["Payment method", paymentLabel[c.paymentMethod]],
            ["Note", c.description],
            ["Recorded on", formatDate(c.createdAt.slice(0, 10))],
          ]}
        />
      </div>
    </Sheet>
  );
}
