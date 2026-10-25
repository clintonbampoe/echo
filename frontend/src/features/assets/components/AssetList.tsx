import { formatMoney, formatMonth, formatNumber } from "@/lib/format";
import type { Paged } from "@/lib/paged";
import { paths } from "@/lib/paths";
import { Chip, IconTile, RecordTable } from "@/ui";
import type { Asset } from "../api";
import { statusLabel, statusTone } from "../labels";
import { purchased } from "../view";
import type { AssetStart } from "./AssetDialogs";
import styles from "./AssetList.module.css";

const purchasedMonth = (a: Asset) =>
  a.purchaseDate ? formatMonth(a.purchaseDate.slice(0, 10)) : "";

type Props = {
  list: Paged<Asset>;
  filtered: boolean;
  onAction: (asset: Asset, start: AssetStart) => void;
};

// An asset's name with its serial number, so the first column says which one it is at a glance.
function Name({ asset }: { asset: Asset }) {
  return (
    <span className={styles.name}>
      <strong>{asset.name}</strong>
      <span className={asset.serialNumber ? styles.serial : styles.none}>
        {asset.serialNumber || "No serial number"}
      </span>
    </span>
  );
}

const empty = (filtered: boolean) =>
  filtered
    ? {
        icon: "search" as const,
        title: "No assets found",
        text: "Nothing matches these filters. Try a wider date range, or reset the filters.",
      }
    : {
        icon: "assets" as const,
        title: "Add your first asset",
        text: "Keep a record of equipment, vehicles, furniture and property: what it cost, what it is worth now, and where it stands.",
      };

// Every asset that matches, by name. A table on a wide screen, a card for each on a phone. A row opens the asset's
// page; the ⋯ menu goes straight to editing or deleting.
export function AssetList({ list, filtered, onAction }: Props) {
  return (
    <RecordTable
      label="Assets"
      title="All assets"
      list={list}
      rowKey={(a) => a.id}
      large
      href={(a) => paths.asset(a.id)}
      line={(a) => ({
        lead: <IconTile icon="assets" />,
        title: a.name,
        sub: a.categoryName,
        end: (
          <>
            <strong className={styles.value}>
              {formatNumber(a.currentValue)}
            </strong>
            <span className={styles.when}>{purchasedMonth(a)}</span>
          </>
        ),
      })}
      rowName={(a) => a.name}
      actions={(a) => [
        { label: "Edit asset", onSelect: () => onAction(a, "edit") },
        {
          label: "Delete asset",
          danger: true,
          onSelect: () => onAction(a, "delete"),
        },
      ]}
      noun={["asset", "assets"]}
      summary=". Sorted by name."
      empty={empty(filtered)}
      columns={[
        { header: "Asset", cell: (a) => <Name asset={a} /> },
        { header: "Category", cell: (a) => a.categoryName },
        {
          header: "Status",
          cell: (a) => (
            <Chip tone={statusTone[a.status]}>{statusLabel[a.status]}</Chip>
          ),
        },
        { header: "Purchased", cell: (a) => purchased(a) },
        { header: "Cost", cell: (a) => formatMoney(a.purchaseCost), end: true },
        {
          header: "Current value",
          cell: (a) => (
            <strong className={styles.value}>
              {formatMoney(a.currentValue)}
            </strong>
          ),
          end: true,
        },
      ]}
    />
  );
}
