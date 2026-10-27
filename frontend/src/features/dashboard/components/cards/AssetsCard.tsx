import { paths } from "@/lib/paths";
import { Aside, DashCard, DistributionBar, Figure } from "@/ui";
import { formatNumber } from "@/lib/format";
import { useAssets } from "../../hooks/assets";
import { CardFailed } from "./CardFailed";

const SHOWN = 4;

// What the church owns: what it is worth now, cut by category. Liquidated assets are not counted. The value
// is typed in by hand, Echo does not depreciate it.
export function AssetsCard() {
  const { figures, rows, failed, retry } = useAssets();
  const sorted = [...(rows ?? [])].sort((a, b) => b.value - a.value);
  const rest = sorted.slice(SHOWN).reduce((sum, r) => sum + r.value, 0);
  // The four worth the most, then one Other. Other never lists what is inside it; Categories holds the full breakdown.
  const parts = [
    ...sorted.slice(0, SHOWN).map((r) => ({
      key: r.key,
      label: r.name,
      value: r.value,
      text: formatNumber(r.value),
      to: `${paths.assets}?category=${r.key}`,
    })),
    ...(sorted.length > SHOWN
      ? [
          {
            key: "other",
            label: "Other",
            value: rest,
            text: formatNumber(rest),
            closed: true,
          },
        ]
      : []),
  ];
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
          <DistributionBar
            label="Assets by category"
            empty="No assets yet"
            parts={parts}
          />
        </>
      )}
    </DashCard>
  );
}
