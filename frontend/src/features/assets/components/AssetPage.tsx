import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { paths } from "@/lib/paths";
import { Alert, BackLink, Button, StatStrip, Tabs } from "@/ui";
import type { Asset } from "../api";
import { useAsset } from "../hooks";
import { assetFigures } from "../view";
import { AssetDetails } from "./AssetDetails";
import { AssetHeader } from "./AssetHeader";
import { AssetValue } from "./AssetValue";
import styles from "./AssetPage.module.css";
import { DeleteAssetDialog } from "./DeleteAssetDialog";
import { EditAssetSheet } from "./EditAssetSheet";

// One asset: what it is, what it cost and what it is worth now.
export function AssetPage() {
  const { id = "" } = useParams();
  const asset = useAsset(id);

  if (asset.isPending)
    return (
      <p role="status" className={styles.note}>
        Loading asset…
      </p>
    );
  if (asset.isError) {
    const missing = isApiError(asset.error) && asset.error.status === 404;
    return (
      <div className={styles.note}>
        <BackLink to={paths.assets}>Assets</BackLink>
        <Alert
          title={
            missing
              ? "We could not find this asset"
              : "We could not load this asset"
          }
        >
          {missing
            ? "It may have been deleted."
            : "Check your connection and try again."}
        </Alert>
        {!missing && (
          <Button variant="secondary" onClick={() => void asset.refetch()}>
            Try again
          </Button>
        )}
      </div>
    );
  }
  return <Detail asset={asset.data} />;
}

const tabs = [
  { id: "details", label: "Details" },
  { id: "value", label: "Value" },
];

function Detail({ asset: a }: { asset: Asset }) {
  const navigate = useNavigate();
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [tab, setTab] = useState("details");

  return (
    <div className="anim-rise">
      <BackLink to={paths.assets}>Assets</BackLink>
      <AssetHeader
        asset={a}
        onEdit={() => setEditing(true)}
        onDelete={() => setDeleting(true)}
      />
      <StatStrip label="Asset figures" stats={assetFigures(a)} />
      <Tabs
        label="Asset"
        tabs={tabs}
        value={tab}
        onChange={setTab}
        idPrefix="asset"
      />
      <div
        role="tabpanel"
        id={`asset-panel-${tab}`}
        aria-labelledby={`asset-tab-${tab}`}
        className={styles.panel}
      >
        {tab === "details" && <AssetDetails asset={a} />}
        {tab === "value" && (
          <AssetValue asset={a} thisYear={new Date().getFullYear()} />
        )}
      </div>
      {editing && (
        <EditAssetSheet asset={a} onClose={() => setEditing(false)} />
      )}
      {deleting && (
        <DeleteAssetDialog
          asset={a}
          onClose={() => setDeleting(false)}
          onDeleted={() => navigate(paths.assets, { replace: true })}
        />
      )}
    </div>
  );
}
