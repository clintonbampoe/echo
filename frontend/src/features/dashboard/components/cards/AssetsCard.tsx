import { Aside, DashCard, Figure } from "@/ui";
import { formatNumber } from "@/lib/format";
import { useAssets } from "../../hooks/assets";
import { CardFailed } from "./CardFailed";
import { Split } from "./Split";

// What the church owns: what it is worth now, cut by category. Liquidated assets are not counted. The value
// is typed in by hand, Echo does not depreciate it.
export function AssetsCard() {
  const { figures, rows, failed, retry } = useAssets();
  const parts = [...(rows ?? [])]
    .sort((a, b) => b.value - a.value)
    .map((r) => ({
      key: r.key,
      name: r.name,
      value: r.value,
      text: formatNumber(r.value),
    }));
  return (
    <DashCard title="Assets" control={<Aside>Current value</Aside>}>
      {failed ? (
        <CardFailed onRetry={retry} />
      ) : (
        <>
          <Figure
            label="Worth now"
            value={figures ? formatNumber(figures.value) : "–"}
            unit="GHS"
          />
          <Split
            label="Assets by category"
            empty="No assets yet"
            parts={parts}
          />
        </>
      )}
    </DashCard>
  );
}
