import { DetailCard, DetailGroup } from "@/ui";
import type { Asset } from "../../api/assets";
import { statusLabel } from "../../model/labels";
import { purchased } from "../../model/display";
import styles from "./AssetDetails.module.css";

// What the asset is, where it is filed and when it was bought. Missing values show a dash so the rows never jump.
export function AssetDetails({ asset: a }: { asset: Asset }) {
  return (
    <DetailCard label="Asset details">
      <DetailGroup
        title="Details"
        rows={[
          ["Serial number", a.serialNumber],
          ["Category", a.categoryName],
          ["Status", statusLabel[a.status]],
          ["Purchased", purchased(a)],
          ["Description", a.description],
        ]}
      />
      <p className={styles.note}>
        Current value is entered by hand when you edit the asset. Echo does not
        depreciate it automatically.
      </p>
    </DetailCard>
  );
}
